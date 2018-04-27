using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using hoReverse.hoUtils;
using ReqIFSharp;

namespace EaServices.Doors
{
    public class DoorsReqIf : DoorsModule
    {
        public DoorsReqIf(EA.Repository rep, EA.Package pkg, string importFile) : base(rep, pkg, importFile)
        {
        }
         /// <summary>
        /// Import and update Requirements. You can set EA ObjectType like "Requirement" or EA Stereotype like "FunctionalRequirement"
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
            InitializeDoorsRequirementsTable(reqIf);


            //reqIf.CoreContent[0].Specifications.Dump();
            foreach (Specification el in reqIf.CoreContent[0].Specifications)
            {
                AddRequirements(DtDoorsRequirements, el.Children,0);
            }


            base.ReadPackageRequirements();
            CreatePackageDeletedObjects();

            

            Count = 0;
            CountChanged = 0;
            CountNew = 0;
            List<int> parentElementIdsPerLevel = new List<int>();
            parentElementIdsPerLevel.Add(0);
            int parentElementId = 0;
            int lastElementId = 0;

            int oldLevel = 0;
            foreach (DataRow row in DtDoorsRequirements.Rows)
            {
                Count += 1;
                string objectId = row["Id"].ToString();
                string reqAbsNumber = GetAbsoluteNumerFromDoorsId(objectId);
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
                    objectShorttext = objectShorttext.Length > 40 ? objectShorttext.Substring(0, 40) : objectShorttext;
                    name = $"{reqAbsNumber.PadRight(7)} {objectShorttext}";

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
                        el.Status = stateChanged;
                        CountChanged += 1;
                    }
                }
                else
                {
                    el = (EA.Element)Pkg.Elements.AddNew(name, "Requirement");
                    el.Status = stateNew;
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
                var cols = from c in DtDoorsRequirements.Columns.Cast<DataColumn>()
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
                    TaggedValue.SetTaggedValue(el, c.Name, c.Value);
                }
            }

            MoveDeletedRequirements();
            UpdatePackage();

            Rep.BatchAppend = false;
            Rep.EnableUIUpdates = true;
            Rep.ReloadPackage(Pkg.PackageID);
        }
        /// <summary>
        /// Initialize DOORS Requirement DataTable
        /// </summary>
        /// <param name="reqIf"></param>
        private void InitializeDoorsRequirementsTable(ReqIF reqIf)
        {
            // Initialize table
            DtDoorsRequirements = new DataTable();
            DtDoorsRequirements.Columns.Add("Id", typeof(string));
            DtDoorsRequirements.Columns.Add("Object Level", typeof(int));
            DtDoorsRequirements.Columns.Add("Object Number", typeof(string));
            DtDoorsRequirements.Columns.Add("Object Type", typeof(string));
            DtDoorsRequirements.Columns.Add("Object Heading", typeof(string));
            foreach (var attr in reqIf.CoreContent[0].SpecObjects[0].Values)
            {
                if (
                    attr.AttributeDefinition.LongName == "Object Text" ||
                    attr.AttributeDefinition.LongName == "Object Heading") continue;
                DtDoorsRequirements.Columns.Add(attr.AttributeDefinition.LongName, typeof(string));
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
                DataRow row = dt.NewRow();
                SpecObject specObject = child.Object;
                row["Id"] = specObject.Identifier;
                row["Object Level"] = level;

                List<AttributeValue> columns = specObject.Values;
                for (int i = 0; i < columns.Count; i++)
                {
                    row[columns[i].AttributeDefinition.LongName] = columns[i].ObjectValue.ToString();
                }

                dt.Rows.Add(row);
                AddRequirements(dt, child.Children, level + 1);
            }
            

        }
    }
}
