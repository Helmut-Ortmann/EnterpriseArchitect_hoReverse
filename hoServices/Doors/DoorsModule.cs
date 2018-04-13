using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using DataModels;
using hoLinqToSql.LinqUtils;
using hoUtils.ExportImport;

namespace EaServices.Doors
{
    public class DoorsModule
    {
        string _importModuleFile;
        EA.Package _pkg;
        private EA.Package _pkgDeletedObjects;
        EA.Repository _rep;
        DataTable _dtDoorsRequirements;
        Dictionary<string, int> _dictObjectIdFromDoorsId = new Dictionary<string,int>();

        readonly string[] _columnNamesNoTaggedValues = {"Object Level", "Object Number", "ObjectType", "Object Heading", "Object Text", "Column1", "Column2"};

        private readonly string packageNameDeletedObjects = "DeletedDoorsRequirements";

        public DoorsModule(EA.Repository rep, EA.Package pkg, string importModuleFile)
        {
            _importModuleFile = importModuleFile;
            _pkg = pkg;
            _rep = rep;

            // Read all requirements
            _dtDoorsRequirements = ExpImp.MakeDataTableFromCsvFile(_importModuleFile, ',');

            // get connection string of repository
            string connectionString = LinqUtil.GetConnectionString(_rep, out var provider);

            if (connectionString == "") return;

            // Read all existing EA Requirements of package
            // Note: In DOORS it's impossible that are there more than an ID(stored in multiplicity)
            using (var db = new EaDataModel(provider, connectionString))
            {
                try
                {
                    _dictObjectIdFromDoorsId = (from r in db.q_object
                        where r.Object_Type == "Requirement" && r.Package_ID == _pkg.PackageID
                        group r by r.Multiplicity into grp
                        select new
                        {
                           Name = grp.Key, //Multiplicity,
                           Value = grp.Max(x => x.Object_ID)

                        }).ToDictionary(x=>x.Name, x=>x.Value);
                }
                catch (Exception e)
                {
                    MessageBox.Show($"{e}", "Can't determine EA Elements of Doors requirements.");
                }
            }

            CreatePackageDeletedObjects();
        }


        /// <summary>
        /// Import and update Requirements
        /// </summary>
        public void ImportUpdateRequirements()
        {
            int count = 0;
            List<int> parentElementIdsPerLevel = new List<int>();
            parentElementIdsPerLevel.Add(0);
            int parentElementId = 0;
            int lastElementId = 0;
            
            int oldLevel = 0;

            foreach (DataRow row in _dtDoorsRequirements.Rows)
            {
                count +=1;
                string objectId =  row["Id"].ToString();
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
                    parentElementId =  parentElementIdsPerLevel[objectLevel];
                }

                oldLevel = objectLevel;
                string name;
                string notes;

                // Estimate if header
                if (objectType == "headline" || ! String.IsNullOrWhiteSpace(objectHeading))
                {
                    name = $"{objectNumber} {objectHeading}";
                    notes =  row["Object Heading"].ToString(); 
                }
                else
                {
                    notes =  row["Object Text"].ToString();
                    string objectShorttext = GetTextExtract(notes);
                    objectShorttext = objectShorttext.Length > 40 ? objectShorttext.Substring(0,40) : objectShorttext;
                    name = $"{reqAbsNumber.PadRight(7)} {objectShorttext}";
                    
                }

                bool isExistingRequirement = _dictObjectIdFromDoorsId.TryGetValue(reqAbsNumber, out int elId);
                

                EA.Element el = isExistingRequirement ? (EA.Element)_rep.GetElementByID(elId) : (EA.Element)_pkg.Elements.AddNew(name, "Requirement");

                el.Alias = objectId;
                el.Name = name;
                el.Multiplicity = reqAbsNumber;
                el.Notes = notes;
                el.TreePos = count*10;
                el.PackageID = _pkg.PackageID;
                el.ParentID = parentElementId;
                el.Update();
                _pkg.Elements.Refresh();
                lastElementId = el.ElementID;

                // handle the remaining columns/ tagged values
                var cols = from c in _dtDoorsRequirements.Columns.Cast<DataColumn>()
                           where !_columnNamesNoTaggedValues.Any(n => n == c.ColumnName ) 
                    select new {
                        Name=c.ColumnName,
                        Value=row[c].ToString()
                    }
                        
                    ;
                // Update/Create Tagged value
                foreach (var c in cols)
                {
                    bool find = false;
                    foreach (EA.TaggedValue t in el.TaggedValues)
                    {
                        if (t.Name == c.Name)
                        {
                            find = true;
                            t.Value = c.Value;
                            t.Update();
                            break;
                        }
                        
                    }

                    if (find == false)
                    {
                        EA.TaggedValue tg = (EA.TaggedValue) el.TaggedValues.AddNew(c.Name, "");
                        tg.Value = c.Value;
                        tg.Update();
                        el.TaggedValues.Refresh();

                    }
               }
            }

            MoveDeletedRequirements();
        }

        /// <summary>
        /// Get DOORS unique id from id. It strips the module global prefix which identifies the module.
        /// </summary>
        /// <param name="doorsId"></param>
        /// <returns></returns>
        private string GetAbsoluteNumerFromDoorsId(string doorsId)
        {
            // Get only the number of the id
            Regex exDoorsAbsNumber = new Regex("[0-9]*$");
            return exDoorsAbsNumber.Match(doorsId).Value;
        }


        /// <summary>
        /// Get text extract from text to show as name
        /// </summary>
        /// <param name="longText"></param>
        /// <returns></returns>
        private string GetTextExtract(string longText)
        {
            // extract doors requirement id (number)
            Regex exFirstLine = new Regex("[^\r]*");
            string objectShorttext = exFirstLine.Match(longText).Value;
            return objectShorttext.Length > 40 ? objectShorttext.Substring(0,40) : objectShorttext;
        }
        /// <summary>
        /// Move EA requirements to Package with moved requirements
        /// </summary>
        public void MoveDeletedRequirements()
        {
            var moveEaElements = from m in _dictObjectIdFromDoorsId
                where !_dtDoorsRequirements.Rows.Cast<DataRow>().Any(x => GetAbsoluteNumerFromDoorsId(x["Id"].ToString()) == m.Key)
                select m.Value;
            foreach (var eaId in moveEaElements)
            {
                EA.Element el = _rep.GetElementByID(eaId);
                el.PackageID = _pkgDeletedObjects.PackageID;
                el.ParentID = 0;
                el.Update();
            }
        }

        /// <summary>
        /// Create Package for deleted objects
        /// </summary>
        /// <returns>Package created Objects</returns>
        private EA.Package CreatePackageDeletedObjects()
        {
            if (_pkgDeletedObjects != null) return _pkgDeletedObjects;
            if (_pkg.Packages.Count > 0)
            {
                _pkgDeletedObjects = (EA.Package) _pkg.Packages.GetAt(0);
                return _pkgDeletedObjects;
            }

            _pkgDeletedObjects = (EA.Package)_pkg.Packages.AddNew(packageNameDeletedObjects, "");
            _pkgDeletedObjects.Update();
            _pkg.Packages.Refresh();
            return _pkgDeletedObjects;

        }

        public EA.Repository Rep { get => _rep; set => _rep = value; }
        public EA.Package Pkg { get => _pkg; set => _pkg = value; }
        public string ImportModuleFile { get => _importModuleFile; set => _importModuleFile = value; }
        /// <summary>
        /// EA Element-Ids of DOORS IDs
        /// </summary>
        public Dictionary<string, int> DictObjectIdFromDoorsId { get => _dictObjectIdFromDoorsId; set => _dictObjectIdFromDoorsId = value; }
        public DataTable DtDoorsRequirements { get => _dtDoorsRequirements; set => _dtDoorsRequirements = value; }
    }
}
