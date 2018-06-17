using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using hoReverse.hoUtils;
using hoUtils.ExportImport;

namespace EaServices.Doors
{
    public class DoorsCsv : DoorsModule
    {
        /// <summary>
        /// The Setting of the current item to import
        /// </summary>
        FileImportSettingsItem _settings;
        public DoorsCsv(string jsonFilePath, EA.Repository rep) : base(jsonFilePath, rep)
        {
        }
        public DoorsCsv(EA.Repository rep, EA.Package pkg) : base(rep, pkg)
        {
        }
        public DoorsCsv(EA.Repository rep, EA.Package pkg, string importFile) : base(rep, pkg, importFile)
        {
        }

        public DoorsCsv(EA.Repository rep, EA.Package pkg, string importFile, FileImportSettingsItem settings) : base(rep, pkg, importFile)
        {
            _settings = settings;
        }
        
        /// <summary>
        /// Import and update Requirements. You can set EA ObjectType like "Requirement" or EA Stereotype like "FunctionalRequirement"
        /// </summary>
        /// async Task
        public override bool ImportUpdateRequirements(string eaObjectType = "Requirement",
            string eaStereotype = "",
            string stateNew = "",
            string stateChanged = "")
        {
            Rep.BatchAppend = true;
            Rep.EnableUIUpdates = false;

            bool result = true;

            // Prepare
            DtRequirements = ExpImp.MakeDataTableFromCsvFile(ImportModuleFile, ',');

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
                string reqAbsNumber = GetAbsoluteNumerFromDoorsId(objectId);
                int objectLevel = Int32.Parse(row["Object Level"].ToString()) - 1;
                string objectNumber = row["Object Number"].ToString();
                string objectType = row["ObjectType"].ToString();
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
                        if (stateChanged != "") el.Status = stateChanged;
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
            return result;
        }
    }

}
