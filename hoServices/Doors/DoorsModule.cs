using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using DataModels;
using hoLinqToSql.LinqUtils;
using hoUtils.ExportImport;
using hoReverse.hoUtils;
using LinqToDB.DataProvider;
using hoUtils.Json;

namespace EaServices.Doors
{
    /// <summary>
    /// Handle import requirements from doors. It requires the following properties:
    /// - Object Level
    /// - Object Number
    /// - ID
    /// - State
    /// - Objecttype
    /// DoorsModule converts additional columns into Tagged value.
    /// 
    /// Note: DoorsModule uses the EA column 'Multiplicity' to store the absolute/unique DOORS ID (without module prefix)
    /// </summary>
    public class DoorsModule
    {
        string _importModuleFile;
        EA.Package _pkg;
        private EA.Package _pkgDeletedObjects;
        EA.Repository _rep;
        DataTable _dtDoorsRequirements;
        Dictionary<string, int> _dictPackageRequirements = new Dictionary<string,int>();
        private string _jsonFilePath;

        private List<FileImportSettingsItem> _importSettings;

        private IDataProvider _provider;
        private string _connectionString;

        readonly string[] _columnNamesNoTaggedValues = {"Object Level", "Object Number", "ObjectType", "Object Heading", "Object Text", "Column1", "Column2"};

        private readonly string packageNameDeletedObjects = "DeletedDoorsRequirements";

        /// <summary>
        /// Initialize basic
        /// </summary>
        /// <param name="jsonFilePath"></param>
        /// <param name="rep"></param>
        public DoorsModule(string jsonFilePath, EA.Repository rep)
        {
            _jsonFilePath = jsonFilePath;
            _rep = rep;
            _connectionString = LinqUtil.GetConnectionString(_rep, out _provider);
            ReadImportSettings();
        }
        /// <summary>
        /// Initialize DoorsModule for handling a file
        /// </summary>
        /// <param name="jsonFilePath"></param>
        /// <param name="rep"></param>
        /// <param name="pkg"></param>
        /// <param name="importModuleFile"></param>
        public DoorsModule(EA.Repository rep, EA.Package pkg, string importModuleFile)
        {
            _importModuleFile = importModuleFile;
            Init(rep, pkg);
        }

        
        /// <summary>
        /// Constructor without a file to import. It's used for e.g. checks.
        /// </summary>
        /// <param name="jsonFilePath"></param>
        /// <param name="rep"></param>
        /// <param name="pkg"></param>
        public DoorsModule(EA.Repository rep, EA.Package pkg)
        {
            Init(rep, pkg);
        }

        /// <summary>
        /// Initialize DoorsModule for usage with package.
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="pkg"></param>
        private void Init(EA.Repository rep, EA.Package pkg)
        {
            _pkg = pkg;
            _rep = rep;

            // get connection string of repository
            _connectionString = LinqUtil.GetConnectionString(_rep, out _provider);
         
        }
        /// <summary>
        /// Read Import Settings
        /// </summary>
        private void ReadImportSettings()
        {
            // Get settings from 'Settings.json', Chapter 'Importer'
            _importSettings = (List<FileImportSettingsItem>)JasonHelper
                .GetConfigurationItems<FileImportSettingsItem>(JasonHelper
                    .DeserializeSettings(_jsonFilePath), "Importer");

        }

        /// <summary>
        /// Read Requirements of _pkg into dictionary _dictPackageRequirements.
        /// </summary>
        private void ReadPackageRequirements()
        {
            // Read all existing EA Requirements of package
            // Note: In DOORS it's impossible that are there more than an ID(stored in multiplicity)
            using (var db = new EaDataModel(_provider, _connectionString))
            {
                try
                {
                    //var notUniqueRequirements = (from r in db.q_object
                    _dictPackageRequirements = (from r in db.q_object
                        where r.Object_Type == "Requirement" && r.Package_ID == _pkg.PackageID
                        group r by r.Multiplicity into grp
                        select new
                        {
                            Name = grp.Key??"", //Multiplicity,
                            Value = grp.Max(x => x.Object_ID)

                        }).ToDictionary(x=>x.Name, x=>x.Value);
                    //DataTable dt = notUniqueRequirements.ToDataTable();
                    //_dictPackageRequirements = notUniqueRequirements.ToDictionary(x=>x.Name, x=>x.Value);
                }
                catch (Exception e)
                {
                    MessageBox.Show($"{e}", "Can't determine EA Elements of Doors requirements.");
                }
            }

        }


        /// <summary>
        /// Import and update Requirements. You can set EA ObjectType like "Requirement" or EA Stereotype like "FunctionalRequirement"
        /// </summary>
        /// async Task
        public async Task ImportUpdateRequirements(string eaObjectType="Requirement", string eaStereotype="")
        {
            _rep.BatchAppend = true;
            _rep.EnableUIUpdates = false;

            // Prepare
            _dtDoorsRequirements = ExpImp.MakeDataTableFromCsvFile(_importModuleFile, ',');

            ReadPackageRequirements();
            CreatePackageDeletedObjects();

            int count = 0;
            List<int> parentElementIdsPerLevel = new List<int>();
            parentElementIdsPerLevel.Add(0);
            int parentElementId = 0;
            int lastElementId = 0;

            int oldLevel = 0;
            foreach (DataRow row in _dtDoorsRequirements.Rows)
            {
                count += 1;
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
                    objectShorttext = objectShorttext.Length > 40 ? objectShorttext.Substring(0, 40) : objectShorttext;
                    name = $"{reqAbsNumber.PadRight(7)} {objectShorttext}";

                }
                // Check if requirement with Doors ID already exists
                bool isExistingRequirement = _dictPackageRequirements.TryGetValue(reqAbsNumber, out int elId);


                EA.Element el = isExistingRequirement ? (EA.Element)_rep.GetElementByID(elId) : (EA.Element)_pkg.Elements.AddNew(name, "Requirement");

                el.Alias = objectId;
                el.Name = name;
                el.Multiplicity = reqAbsNumber;
                el.Notes = notes;
                el.TreePos = count * 10;
                el.PackageID = _pkg.PackageID;
                el.ParentID = parentElementId;
                el.Type = eaObjectType;
                el.Stereotype = eaStereotype;

                el.Update();
                _pkg.Elements.Refresh();
                lastElementId = el.ElementID;

                // handle the remaining columns/ tagged values
                var cols = from c in _dtDoorsRequirements.Columns.Cast<DataColumn>()
                           where !_columnNamesNoTaggedValues.Any(n => n == c.ColumnName)
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

            _rep.BatchAppend = false;
            _rep.EnableUIUpdates = true;
            _rep.ReloadPackage(_pkg.PackageID);
        }
        /// <summary>
        /// Update import package with: 
        /// </summary>
        private void UpdatePackage()
        {
            EA.Element el = _rep.GetElementByGuid(_pkg.PackageGUID);
            TaggedValue.SetTaggedValue(el, "Imported", $"{DateTime.Now:G}");
            TaggedValue.SetTaggedValue(el, "ImportedBy", $"{Environment.UserName}");
            TaggedValue.SetTaggedValue(el, "ImportedFile", $"{_importModuleFile}");
            TaggedValue.SetTaggedValue(el, "ImportedCount", $"{_dtDoorsRequirements.Rows.Count}");

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
            var moveEaElements = from m in _dictPackageRequirements
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
        /// Check requirements of package
        /// </summary>
        /// <returns></returns>
        public bool CheckRequirements()
        {
            using (var db = new EaDataModel(_provider, _connectionString))
            {
                try
                {
                   var falseRequirements = (from o in db.t_object
                        where o.Package_ID == _pkg.PackageID &&
                              o.Object_Type == "Requirement"
                              group o by o.Multiplicity into grp
                              where grp.Count() > 1
                        join o1 in db.t_object on grp.Key equals o1.Multiplicity 
                        where o1.Package_ID == _pkg.PackageID &&
                              o1.Object_Type == "Requirement"
                        orderby o1.Multiplicity
                        select new
                        {
                            CLASSGUID = o1.ea_guid,
                            CLASSTYPE = o1.Object_Type,
                            Name = o1.Name,
                            Type = o1.Object_Type,
                            DoorsID= o1.Multiplicity,
                            Error = "DoorsID not unique"
                        }).ToDataTable();

                    // Make EA xml
                    string xml = Xml.MakeXmlFromDataTable(falseRequirements);
                    // Output to EA
                    _rep.RunModelSearch("", "", "", xml);
                    return falseRequirements.Rows.Count == 0;
                }
                catch (Exception e)
                {
                    MessageBox.Show($"{e}", "Error reading EA Repository, break!!");
                    return false;
                }


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
        /// <summary>
        /// Import according to import settings
        /// </summary>
        public async void ImportBySetting()
        {
            foreach (FileImportSettingsItem item in _importSettings)
            {
                _pkg = _rep.GetPackageByGuid(item.PackageGuid);
                _importModuleFile = item.InputFile;
                string eaObjectType = item.ObjectType;
                string eaStereotype = item.Stereotype;

                await Task.Run(() => ImportUpdateRequirements(eaObjectType,eaStereotype));
                
            }

        }

        public EA.Repository Rep { get => _rep; set => _rep = value; }
        public EA.Package Pkg { get => _pkg; set => _pkg = value; }
        public string ImportModuleFile { get => _importModuleFile; set => _importModuleFile = value; }
        /// <summary>
        /// EA Element-Ids of DOORS IDs
        /// </summary>
        public Dictionary<string, int> DictObjectIdFromDoorsId { get => _dictPackageRequirements; set => _dictPackageRequirements = value; }
        public DataTable DtDoorsRequirements { get => _dtDoorsRequirements; set => _dtDoorsRequirements = value; }
    }
}
