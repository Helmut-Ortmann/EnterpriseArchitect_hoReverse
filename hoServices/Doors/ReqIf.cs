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
    public class ReqIf : DoorsModule
    {
        readonly FileImportSettingsItem _settings;

        /// <summary>
        /// Dictionary of Attrubutes
        /// </summary>
        Dictionary<string, DatatypeDefinition> _attributes= new Dictionary<string, DatatypeDefinition>();
        public ReqIf(EA.Repository rep, EA.Package pkg, string importFile, FileImportSettingsItem settings) : base(rep, pkg, importFile)
        {
            _settings = settings;
        }
         /// <summary>
        /// Import and update Requirements.
        /// </summary>
        /// async Task
        public override async Task ImportUpdateRequirements(string eaObjectType = "Requirement",
            string eaStereotype = "",
            string stateNew = "",
            string stateChanged = "")
        {
            Rep.BatchAppend = true;
            Rep.EnableUIUpdates = false;

            // Deserialize
            ReqIFDeserializer deserializer = new ReqIFDeserializer();
            var reqIf = deserializer.Deserialize(ImportModuleFile);

            InitializeReqIfRequirementsTable(reqIf);

            foreach (Specification el in reqIf.CoreContent[0].Specifications)
            {
                AddRequirements(DtRequirements, el.Children,1);
            }


            ReadPackageRequirements();
            CreatePackageDeletedObjects();

            

            Count = 0;
            CountChanged = 0;
            CountNew = 0;
            List<int> parentElementIdsPerLevel = new List<int>();
            parentElementIdsPerLevel.Add(0);
            int parentElementId = 0;
            int lastElementId = 0;

            int oldLevel = 0;

            string rtfColumn  = _settings.RtfNameList.Count > 0 ? _settings.RtfNameList[0] : "";
            string nameColumn = _settings.AttrNameList.Count > 0 ? _settings.AttrNameList[0] : "";
            string notesColumn = _settings.AttrNotes ?? "";
            foreach (DataRow row in DtRequirements.Rows)
            {
                Count += 1;
                string objectId = row["Id"].ToString();
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


                string name = GetAttrValue(nameColumn != "" ? row[nameColumn].ToString(): row[0].ToString());
                string notes = GetAttrValue(notesColumn != "" ? row[notesColumn].ToString(): row[1].ToString());
                string nameShort = GetAttrValue(name.Length > 40 ? name.Substring(0, 40) : name);

               // Check if requirement with Doors ID already exists
                bool isExistingRequirement = DictPackageRequirements.TryGetValue(objectId, out int elId);


                EA.Element el;
                if (isExistingRequirement)
                {
                    el = Rep.GetElementByID(elId);
                    if (el.Alias != objectId ||
                        el.Name != nameShort ||
                        el.Notes != notes )
                    {
                        el.Status = stateChanged;
                        CountChanged += 1;
                    }
                }
                else
                {
                    el = (EA.Element)Pkg.Elements.AddNew(name, "Requirement");
                    CountChanged += 1;
                }


                el.Alias = objectId;
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
                    if (c.Name == rtfColumn)
                    {
                        string docFile = $"{System.IO.Path.GetDirectoryName(ImportModuleFile)}";
                        docFile = System.IO.Path.Combine(docFile, "xxxxxxx.docx");

                        HtmlToDocx.Convert( docFile, c.Value??"");
                        el.LoadLinkedDocument(docFile);
                    }
                    else
                    {
                        if (c.Name.Trim() != notesColumn)  TaggedValue.SetUpdate(el, c.Name, GetAttrValue(c.Value??""));                     
                    }
                    
                }
            }

            MoveDeletedRequirements();
            UpdatePackage();

            Rep.BatchAppend = false;
            Rep.EnableUIUpdates = true;
            Rep.ReloadPackage(Pkg.PackageID);
        }
        /// <summary>
        /// Initialize ReqIF Requirement DataTable
        /// </summary>
        /// <param name="reqIf"></param>
        private void InitializeReqIfRequirementsTable(ReqIF reqIf)
        {
            // Initialize table
            DtRequirements = new DataTable();
            DtRequirements.Columns.Add("Id", typeof(string));
            DtRequirements.Columns.Add("Object Level", typeof(string));
           
            // over all Attributes
            foreach (var attr in reqIf.CoreContent[0].SpecObjects[0].Values)
            {
                string attrName = attr.AttributeDefinition.LongName;
                DatatypeDefinition attrType = attr.AttributeDefinition.DatatypeDefinition;
                _attributes.Add(attrName, attrType);
                DtRequirements.Columns.Add(attrName, typeof(string));
            }
        }


        /// <summary>
        /// Add ObjSpec childrens to DataTable, recursive with 
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="children"></param>
        /// <param name="level"></param>
        private void AddRequirements(DataTable dt, List<SpecHierarchy> children, int level)
        {
            if (children == null || children.Count == 0) return;
            foreach (SpecHierarchy child in children)
            {
                DataRow row = dt.NewRow();
                SpecObject specObject = child.Object;
                row["Id"] = specObject.Identifier;
                row["Object Level"] = level;

                List<AttributeValue> columns = specObject.Values;
                for (int i = 0; i < columns.Count; i++)
                {
                    try
                    {
                        row[columns[i].AttributeDefinition.LongName] = columns[i].ObjectValue.ToString();
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show($"AttrName: '{columns[i].AttributeDefinition.LongName}'\r\n\r\n{e}", 
                            "Exception add ReqIF Attribute");
                        continue;
                    }
                    
                }

                dt.Rows.Add(row);
                AddRequirements(dt, child.Children, level + 1);
            }
            

        }
        /// <summary>
        /// Get Attribute Value
        /// </summary>
        /// <param name="attrName"></param>
        /// <param name="attrValue"></param>
        /// <returns></returns>
        private string GetAttrValue(string attrName, string attrValue)
        {
            switch (_attributes[attrName])
            {
                case DatatypeDefinitionXHTML s:
                    return HtmlToText.Convert(attrValue);
 

                default:
                    return attrValue;

            }
        }
        /// <summary>
        /// Get Attribute Value
        /// </summary>
        /// <param name="attrValue"></param>
        /// <returns></returns>
        private string GetAttrValue(string attrValue)
        {
            if (attrValue.Contains("http://www.w3.org/1999/xhtml"))
                return HtmlToText.Convert(attrValue);
            else return attrValue;

        }
        /// <summary>
        /// Move deleted EA requirements to Package 'DeletedRequirements'
        /// </summary>
        public override void MoveDeletedRequirements()
        {
            var moveEaElements = from m in DictPackageRequirements
                where !DtRequirements.Rows.Cast<DataRow>().Any(x => x["Id"].ToString() == m.Key)
                select m.Value;
            foreach (var eaId in moveEaElements)
            {
                EA.Element el = Rep.GetElementByID(eaId);
                el.PackageID = PkgDeletedObjects.PackageID;
                el.ParentID = 0;
                el.Update();
            }
        }
    }
}
