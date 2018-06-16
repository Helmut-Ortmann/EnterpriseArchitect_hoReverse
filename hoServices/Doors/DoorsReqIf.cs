using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using hoReverse.hoUtils;
using ReqIFSharp;

namespace EaServices.Doors
{
    public class DoorsReqIf : ReqIf
    {
        /// <summary>
        /// Import of ReqIF DOORS Requirements
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="pkg"></param>
        /// <param name="importFile"></param>
        /// <param name="settings"></param>
        public DoorsReqIf(EA.Repository rep, EA.Package pkg, string importFile, FileImportSettingsItem settings) : base(rep, pkg, importFile,  settings)
        {
            
        }
         /// <summary>
        /// Import and update ReqIF Requirements. You can set EA ObjectType like "Requirement" or EA Stereotype like "FunctionalRequirement"
        /// </summary>
        /// async Task
        public override void ImportUpdateRequirements(string eaObjectType = "Requirement",
            string eaStereotype = "",
             int subModuleIndex = 0,
            string stateNew = "",
            string stateChanged = "")
        {
            Rep.BatchAppend = true;
            Rep.EnableUIUpdates = false;

            // Deserialize
            ReqIFDeserializer deserializer = new ReqIFDeserializer();
            var reqIf = deserializer.Deserialize(ImportModuleFile);

            InitializeDoorsRequirementsTable(reqIf, subModuleIndex);


            //reqIf.CoreContent[0].Specifications.Dump();
            // over all submodules
            Specification elModule = reqIf.CoreContent[0].Specifications[subModuleIndex];
            AddRequirements(DtRequirements, elModule.Children,1);



            base.ReadEaPackageRequirements();
            CreateEaPackageDeletedObjects();

            

            Count = 0;
            CountChanged = 0;
            CountNew = 0;
            List<int> parentElementIdsPerLevel = new List<int> {0};
            int parentElementId = 0;
            int lastElementId = 0;

            int oldLevel = 0;
            foreach (DataRow row in DtRequirements.Rows)
            {
                Count += 1;
                string objectId = row["Id"].ToString();
                string reqAbsNumber = objectId;
                //string reqAbsNumber = GetAbsoluteNumerFromDoorsId(objectId);
                int objectLevel = Int32.Parse(row["Object Level"].ToString()) - 1;
                string objectNumber = row["Object Number"].ToString();
                string objectType = row["Object Type"].ToString();
                string objectHeading = row["Object Heading"].ToString();


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
                string name;
                string notes;

                // Estimate if header
                if (objectType == "headline" || !String.IsNullOrWhiteSpace(objectHeading))
                {
                    name = $"{objectNumber} {objectHeading}";
                    notes = row["Object Heading"].ToString();
                }
                else
                {
                    notes = row["Object Text"].ToString();
                    string objectShorttext = GetTextExtract(notes);
                    objectShorttext = objectShorttext.Length > ShortNameLength ? objectShorttext.Substring(0, ShortNameLength) : objectShorttext;
                    name = objectShorttext;
                    //name = $"{reqAbsNumber.PadRight(7)} {objectShorttext}";

                }
                // Check if requirement with Doors ID already exists
                bool isExistingRequirement = DictPackageRequirements.TryGetValue(reqAbsNumber, out int elId);


                EA.Element el;
                if (isExistingRequirement)
                {
                    el = (EA.Element) Rep.GetElementByID(elId);
                    if (el.Alias != objectId ||
                        el.Name != name ||
                        el.Notes != notes ||
                        el.Type != eaObjectType ||
                        el.Stereotype != eaStereotype)
                    {
                        if (stateChanged !="") el.Status = stateChanged;
                        CountChanged += 1;
                    }
                }
                else
                {
                    el = (EA.Element)Pkg.Elements.AddNew(name, "Requirement");
                    if (stateNew != "") el.Status = stateNew;
                    CountChanged += 1;
                }


                el.Alias = objectId;
                el.Name = name;
                el.Multiplicity = reqAbsNumber;
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
                    TaggedValue.SetUpdate(el, c.Name, c.Value);
                }
            }

            MoveDeletedRequirements();
            UpdatePackage();

            Rep.BatchAppend = false;
            Rep.EnableUIUpdates = true;
            Rep.ReloadPackage(Pkg.PackageID);
        }

        /// <summary>
        /// Initialize DOORS Requirement DataTable for the sub module according to subModuleIndex.
        /// It creates the table and the column names from the DOORS attributes and the standard attributes. 
        /// </summary>
        /// <param name="reqIf"></param>
        /// <param name="subModuleIndex"></param>
        private void InitializeDoorsRequirementsTable(ReqIF reqIf, int subModuleIndex)
        {
            // Initialize table
            DtRequirements = new DataTable();
            DtRequirements.Columns.Add("Id", typeof(string));
            DtRequirements.Columns.Add("Object Level", typeof(int));
            DtRequirements.Columns.Add("Object Number", typeof(string));
            DtRequirements.Columns.Add("Object Type", typeof(string));
            DtRequirements.Columns.Add("Object Heading", typeof(string));
            DtRequirements.Columns.Add("Object Text", typeof(string));
            // 
            DtRequirements.Columns.Add("VerificationMethod", typeof(string));
            DtRequirements.Columns.Add("RequirementID", typeof(string));
            DtRequirements.Columns.Add("LegacyID", typeof(string));

            // Get all attributes
            var blackList = new String[] { "TableType", "TableBottomBorder", "TableCellWidth", "TableChangeBars", "TableLeftBorder", "TableLinkIndicators", "TableRightBorder", "TableShowAttrs", "TableTopBorder" };
            var attributes = (from obj in reqIf.CoreContent[0].SpecObjects
                from attr in obj.Values
                where !blackList.Any(bl => bl == attr.AttributeDefinition.LongName)

                select new { name = attr.AttributeDefinition.LongName, Type = attr.AttributeDefinition.DatatypeDefinition.ToString() }).Distinct();
            foreach (var attribute in attributes)
            {
                if (
                    attribute.name == "Object Text" ||
                    attribute.name == "Object Heading") continue;
                DtRequirements.Columns.Add(attribute.name, typeof(string));
            }

            
        }


        /// <summary>
        /// Add ObjSpec childrens to DataTable, recursive with 
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="children"></param>
        /// <param name="level"></param>
        public void AddRequirements(DataTable dt, List<SpecHierarchy> children, int level)
        {
            if (children == null || children.Count == 0) return;
            foreach (SpecHierarchy child in children)
            {
                DataRow dataRow = dt.NewRow();
                SpecObject specObject = child.Object;
                dataRow["Id"] = specObject.Identifier;
                dataRow["Object Level"] = level;

                List<AttributeValue> columns = specObject.Values;
                for (int i = 0; i < columns.Count; i++)
                {
                    try
                    {
                        dataRow[columns[i].AttributeDefinition.LongName] = columns[i].ObjectValue.ToString();
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show($@"AttrName: '{columns[i].AttributeDefinition.LongName}'

{e}", 
                            @"Exception add ReqIF Attribute");
                        continue;
                    }
                    
                }

                dt.Rows.Add(dataRow);
                AddRequirements(dt, child.Children, level + 1);
            }
            

        }
    }
}
