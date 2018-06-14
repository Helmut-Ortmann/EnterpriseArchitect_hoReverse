using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using hoReverse.hoUtils;
using LinqToDB.SqlQuery;

namespace EaServices.Doors
{
    /// <summary>
    /// Import a structered Xml. This structured xml is similiar to ReqIF. See also 'hoServices\Doors\XmlStructReference.xml'
    /// -
    /// </summary>
    public class XmlStruct : DoorsModule
    {
        /// <summary>
        /// The Setting of the current item to import
        /// </summary>
        ///
        readonly FileImportSettingsItem _settings;

        public XmlStruct(string jsonFilePath, EA.Repository rep) : base(jsonFilePath, rep)
        {
        }

        public XmlStruct(EA.Repository rep, EA.Package pkg) : base(rep, pkg)
        {
        }

        public XmlStruct(EA.Repository rep, EA.Package pkg, string importFile) : base(rep, pkg, importFile)
        {
        }

        public XmlStruct(EA.Repository rep, EA.Package pkg, string importFile, FileImportSettingsItem settings) : base(
            rep, pkg, importFile)
        {
            _settings = settings;
        }

        // XML names
        private readonly string xmlObjectName = "OBJECT";
        private readonly string xmlHierarchyName = "HIERARCHY";
        private readonly string xmlChildrenName = "CHILDREN";
        private readonly string xmlModuleName = "MODULE";
        private readonly string xmlHeaderName = "HEADER";

        /// <summary>
        /// Import and update Requirements. You can set EA ObjectType like "Requirement" or EA Stereotype like "FunctionalRequirement"
        /// </summary>
        /// async Task
        public override void ImportUpdateRequirements(string eaObjectType = "Requirement",
            string eaStereotype = "",
            string stateNew = "",
            string stateChanged = "")
        {
            Rep.BatchAppend = true;
            Rep.EnableUIUpdates = false;

            // Read xml file
            XElement xElFile;
            try
            {
                xElFile = XElement.Parse(HoUtil.ReadAllText(ImportModuleFile));
            }
            catch (Exception e)
            {
                Rep.BatchAppend = false;
                Rep.EnableUIUpdates = true;
                MessageBox.Show($@"File: {ImportModuleFile}{Environment.NewLine}{Environment.NewLine}{e}", @"Can't import structured *.xml");
                return;
            }

            InitializeXmlStructTable(xElFile);

            // Go through hierarchy and store in DataTable
            var level = 1;
            var children = xElFile.Descendants(xmlChildrenName).FirstOrDefault(); //.Dump();
            OutputChildren(children, level);




            base.ReadEaPackageRequirements();
            CreateEaPackageDeletedObjects();

            Count = 0;
            CountChanged = 0;
            CountNew = 0;
            List<int> parentElementIdsPerLevel = new List<int> {0};
            int parentElementId = 0;
            int lastElementId = 0;

            int oldLevel = 0;

            string notesColumn = _settings.AttrNotes ?? "";
            foreach (DataRow row in DtRequirements.Rows)
            {
                Count += 1;



                int objectLevel = Int32.Parse(row["Object Level"].ToString()) - 1;

                // Maintain parent ids of level
                // get parent id
                if (objectLevel > oldLevel)
                {
                    if (parentElementIdsPerLevel.Count <= objectLevel) parentElementIdsPerLevel.Add(lastElementId);
                    else parentElementIdsPerLevel[objectLevel] = lastElementId;
                    parentElementId = lastElementId;
                }

                if (objectLevel < oldLevel)
                {
                    parentElementId = parentElementIdsPerLevel[objectLevel];
                }

                oldLevel = objectLevel;

                string objectId = CombineAttrValues(_settings.IdList, row, 40);
                string alias = CombineAttrValues(_settings.AliasList, row, 40);
                string name = CombineAttrValues(_settings.AttrNameList, row, 40);
                string notes = notesColumn != "" ? row[notesColumn].ToString() : row[1].ToString();
                string nameShort = name.Length > 40 ? name.Substring(0, 40) : name;

                // Check if requirement with Doors ID already exists
                bool isExistingRequirement = DictPackageRequirements.TryGetValue(objectId, out int elId);


                EA.Element el;
                if (isExistingRequirement)
                {
                    el = Rep.GetElementByID(elId);
                    if (el.Alias != alias ||
                        el.Name != nameShort ||
                        el.Notes != notes)
                    {
                        if (stateChanged != "") el.Status = stateChanged;
                        CountChanged += 1;
                    }
                }
                else
                {
                    el = (EA.Element) Pkg.Elements.AddNew(name, "Requirement");
                    if (stateNew != "") el.Status = stateNew;
                    CountChanged += 1;
                }


                el.Alias = alias;
                el.Name = name;
                el.Multiplicity = objectId;
                el.Notes = notes;
                el.TreePos = Count * 10;
                el.PackageID = Pkg.PackageID;
                el.ParentID = parentElementId;
                el.Type = eaObjectType;
                el.Stereotype = eaStereotype;

                el.Update();
                Pkg.Elements.Refresh();
                lastElementId = el.ElementID;

                // handle the remaining columns/ tagged values
                var cols = from c in DtRequirements.Columns.Cast<DataColumn>()
                        where !ColumnNamesNoTaggedValues.Any(n => n == c.ColumnName)
                        select new
                        {
                            Name = c.ColumnName,
                            Value = row[c].ToString()
                        }

                    ;


                // Update/Create Tagged value
                foreach (var c in cols)
                {
                    if (notesColumn != c.Name) TaggedValue.SetUpdate(el, c.Name, c.Value ?? "");

                }
            }

            MoveDeletedRequirements();
            UpdatePackage(xElFile);

            Rep.BatchAppend = false;
            Rep.EnableUIUpdates = true;
            Rep.ReloadPackage(Pkg.PackageID);
        }

        /// <summary>
        /// Initialize ReqIF Requirement DataTable
        /// </summary>
        /// <param name="xmlStruct"></param>
        private void InitializeXmlStructTable(XElement xmlStruct)
        {
            // Initialize table
            // Standard columns
            DtRequirements = new DataTable();
            DtRequirements.Columns.Add("Object Level", typeof(string));
            DtRequirements.Columns.Add("Id", typeof(string));


            // Estimate attributes
            var attributes = (from o in xmlStruct.Descendants(xmlObjectName).Elements()
                select o.Name).Distinct();


            // over all Attributes
            foreach (var attr in attributes)
            {
                DtRequirements.Columns.Add(attr.ToString(), typeof(string));
            }
        }

        /// <summary>
        /// Output Childrens recursive in DataTable
        /// </summary>
        /// <param name="xeChildren"></param>
        /// <param name="level"></param>
        void OutputChildren(XElement xeChildren, int level)
        {
            // 
            foreach (var elHierarchy in xeChildren.Elements(xmlHierarchyName))
            {
                XElement xObject = elHierarchy.Descendants(xmlObjectName).FirstOrDefault();
                DataRow row = DtRequirements.NewRow();
                row["Object Level"] = level;
                foreach (var attribute in xObject.Elements())
                {
                    row[attribute.Name.ToString()] = attribute.Value;
                }
                row["Id"] = CombineAttrValues(_settings.IdList, row, 40);
                DtRequirements.Rows.Add(row);

                if (elHierarchy.Element(xmlChildrenName) != null)
                {
                    OutputChildren(elHierarchy.Element(xmlChildrenName), level + 1);
                }
            }
        }

        /// <summary>
        /// Get Attribute value for a list of attribute names. Limit the output length if length > 0
        /// </summary>
        /// <param name="lNames"></param>
        /// <param name="row"></param>
        /// <param name="length">The leghth of the output string. Default:40</param>
        /// <returns></returns>
        protected string CombineAttrValues(List<string> lNames, DataRow row, int length = 0)
        {
            string attrValue = lNames.Count == 0 ? row[0].ToString() : "";
            string delimeter = "";
            foreach (var columnName in lNames)
            {

                try
                {
                    attrValue = $"{attrValue}{delimeter}{row[columnName]}";
                }
                catch (Exception e)
                {
                    MessageBox.Show(
                        $@"Attribute name:{Environment.NewLine}{columnName}{Environment.NewLine}{
                                Environment.NewLine
                            }{e}",
                        @"Can't read Attribute!");
                }

                delimeter = " ";
            }

            // linit length
            return length > 0 && attrValue.Length > length
                ? attrValue.Substring(0, length)
                : attrValue;

        }

        /// <summary>
        /// Update import package with file properties/attributes of HEADER and MODULE
        /// </summary>
        private  void UpdatePackage(XElement xElFile)
        {
            var fileAttres = (from attr in xElFile.Descendants(xmlModuleName).Elements()
                select new {Name = attr.Name, Value = attr.Value})
                    .Union
                    (from attr in xElFile.Descendants(xmlHeaderName).Elements()
                        select new { Name = attr.Name, Value = attr.Value })
                ;
            foreach (var attr in fileAttres)
            {
                EA.Element el = Rep.GetElementByGuid(Pkg.PackageGUID);
                TaggedValue.SetUpdate(el, attr.Name.ToString(), attr.Value);
            }
        }
    }
}
