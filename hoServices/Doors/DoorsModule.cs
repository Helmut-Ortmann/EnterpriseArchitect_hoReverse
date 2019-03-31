using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using DataModels;
using EA;
using hoLinqToSql.LinqUtils;
using hoUtils.DirFile;
using hoUtils.ExportImport;
using LinqToDB.DataProvider;
using hoUtils.Json;
using ReqIFSharp;
using TaggedValue = hoReverse.hoUtils.TaggedValue;
using EaServices.Doors.ReqIfs;

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
        /// <summary>
        /// List of changes during one command
        /// </summary>
        protected List<ReqIfLog> _reqIfLogList;

        protected int CountPackage;
        protected readonly string Tab = "\t";
        public const int ShortNameLength = 60; 
        protected int _count ;
        protected int _countAll;
        protected int _countPkg;
        protected int _countItems;
        protected int _countChanged;
        protected int _countNew;
        protected int _level = -1;

        string _importModuleFile;
        EA.Package _pkg;

        private ReqIF _reqIfDeserialized;

        private EA.Package _pkgDeletedObjects;
        EA.Repository _rep;
        DataTable _dtRequirements;
        Dictionary<string, int> _dictPackageRequirements = new Dictionary<string,int>();
        private readonly string _jsonFilePath;

        private List<FileImportSettingsItem> _importSettings;

        protected IDataProvider _provider;
        protected string _connectionString;

        protected readonly string[] ColumnNamesNoTaggedValues = {"Object Level", "Object Number", "ObjectType", "Object Heading", "Object Text", "Column1", "Column2"};

        private readonly string packageNameDeletedObjects = "Trash";

        /// <summary>
        /// Initialize basic
        /// </summary>
        /// <param name="jsonFilePath"></param>
        /// <param name="rep"></param>
        /// <param name="reqIfLogList"></param>
        public DoorsModule(string jsonFilePath, EA.Repository rep, List<ReqIfLog> reqIfLogList = null)
        {
            _jsonFilePath = jsonFilePath;
            _rep = rep;
            _connectionString = LinqUtil.GetConnectionString(_rep, out _provider);
            _reqIfLogList = reqIfLogList;
            ReadImportSettings();
        }
        /// <summary>
        /// 
        /// </summary>
        public DoorsModule()
        {

        }

        /// <summary>
        /// Initialize DoorsModule for handling a file
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="pkg"></param>
        /// <param name="importModuleFile"></param>
        /// <param name="reqIfLogList"></param>
        public DoorsModule(EA.Repository rep, EA.Package pkg, string importModuleFile, List<ReqIfLog> reqIfLogList = null)
        {
            _importModuleFile = importModuleFile;
            Init(rep, pkg, reqIfLogList);
        }


        /// <summary>
        /// Constructor without a file to import. It's used for e.g. checks.
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="pkg"></param>
        /// <param name="reqIfLogList"></param>
        public DoorsModule(EA.Repository rep, EA.Package pkg, List<ReqIfLog> reqIfLogList = null)
        {
            Init(rep, pkg, reqIfLogList);
        }

        /// <summary>
        /// Initialize DoorsModule for usage with package.
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="pkg"></param>
        /// <param name="reqIfLogList"></param>
        private void Init(EA.Repository rep, EA.Package pkg, List<ReqIfLog> reqIfLogList = null)
        {
            _pkg = pkg;
            _rep = rep;
            _reqIfDeserialized = null;
            _reqIfLogList = reqIfLogList;

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
        /// - Key: ID according to ReqIF
        /// - Value: EA ElementID of the requirements
        /// </summary>
        protected void ReadEaPackageRequirements()
        {
            // Read all existing EA Requirements of package
            // Note: In DOORS it's impossible that are there more than an ID(stored in multiplicity)
            using (var db = new EaDataModel(_provider, _connectionString))
            {
                try
                {
                    //var notUniqueRequirements = (from r in db.q_object
                    DictPackageRequirements = (from r in db.q_object
                        where r.Object_Type == "Requirement" && r.Package_ID == _pkg.PackageID
                        group r by r.Multiplicity into grp
                        select new
                        {
                            Name = grp.Key??"", //Multiplicity,
                            Value = grp.Max(x => x.Object_ID)

                        }).ToDictionary(x=>x.Name, x=>x.Value);
                }
                catch (Exception e)
                {
                    MessageBox.Show($@"Package: '{_pkg.Name}{Environment.NewLine}{Environment.NewLine}'{e}", 
                        @"Can't determine EA Requirements of Doors Requirements.");
                }
            }

        }

        public virtual bool ImportForFile(string eaObjectType = "Requirement",
            string eaStereotype = "",
            string stateNew = "")
        {
            return false;
        }
        /// <summary>
        /// Used if addressing only by a specification index in the file
        /// </summary>
        /// <returns></returns>
        public virtual bool RoundtripForFile()
        {
            return true;
        }
        /// <summary>
        /// Used for ReqIF to address by contentIndex and specIndex
        /// </summary>
        /// <param name="reqIfContentIndex"></param>
        /// <param name="reqIfSpecIndex"></param>
        /// <returns></returns>
        public virtual bool RoundtripForFile(int reqIfContentIndex, int reqIfSpecIndex)
        {
            return true;
        }
        /// <summary>
        /// Import and update Requirements. You can set EA ObjectType like "Requirement" or EA Stereotype like "FunctionalRequirement"
        /// </summary>
        /// async Task
        public virtual bool ImportForFile(string eaObjectType = "Requirement",
            string eaStereotype = "",
            string stateNew = "",
            string stateChanged = "")
        {
            _rep.BatchAppend = true;
            _rep.EnableUIUpdates = false;

            // Prepare
            _dtRequirements = ExpImp.MakeDataTableFromCsvFile(_importModuleFile, ',');

            ReadEaPackageRequirements();
            CreateEaPackageDeletedObjects();

            _count = 0;
            _countAll = 0;
            _countChanged = 0;
            _countNew = 0;
            List<int> parentElementIdsPerLevel = new List<int> {0};
            int parentElementId = 0;
            int lastElementId = 0;

            int oldLevel = 0;
            foreach (DataRow row in _dtRequirements.Rows)
            {
                _count += 1;
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
                    el = _rep.GetElementByID(elId);
                    if (el.Alias != objectId ||
                        el.Name != name ||
                        el.Notes != notes ||
                        el.Type != eaObjectType ||
                        el.Stereotype != eaStereotype)
                    {
                        if (stateChanged != "") el.Status = stateChanged;
                        _countChanged += 1;
                    }
                }
                else
                {
                    el = (EA.Element)_pkg.Elements.AddNew(name, "Requirement");
                    if (stateNew != "") el.Status = stateNew;
                    _countChanged += 1;
                }


                el.Alias = objectId;
                el.Name = name;
                el.Multiplicity = reqAbsNumber;
                el.Notes = notes;
                el.TreePos = _count * 10;
                el.PackageID = _pkg.PackageID;
                el.ParentID = parentElementId;
                el.Type = eaObjectType;
                el.Stereotype = eaStereotype;

                el.Update();
                _pkg.Elements.Refresh();
                lastElementId = el.ElementID;

                // handle the remaining columns/ tagged values
                var cols = from c in _dtRequirements.Columns.Cast<DataColumn>()
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

            _rep.BatchAppend = false;
            _rep.EnableUIUpdates = true;
            _rep.ReloadPackage(_pkg.PackageID);
            return true;

        }
        /// <summary>
        /// Update import package with: 
        /// </summary>
        protected virtual void UpdatePackage()
        {
            EA.Element el = _rep.GetElementByGuid(_pkg.PackageGUID);
            TaggedValue.SetUpdate(el, "Imported", $"{DateTime.Now:G}");
            TaggedValue.SetUpdate(el, "ImportedBy", $"{Environment.UserName}");
            TaggedValue.SetUpdate(el, "ImportedFile", $"{_importModuleFile}");
            TaggedValue.SetUpdate(el, "ImportedCount", $"{_dtRequirements.Rows.Count}");
            TaggedValue.SetUpdate(el, "ImportedNew", $"{_countNew}");
            TaggedValue.SetUpdate(el, "ImportedChanged", $"{_countChanged}");

        }

        /// <summary>
        /// Get DOORS unique id from id. It strips the module global prefix which identifies the module.
        /// </summary>
        /// <param name="doorsId"></param>
        /// <returns></returns>
        protected string GetAbsoluteNumerFromDoorsId(string doorsId)
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
        protected string GetTextExtract(string longText)
        {
            // extract doors requirement id (number)
            Regex exFirstLine = new Regex("[^\r]*");
            string objectShorttext = exFirstLine.Match(longText).Value;
            return objectShorttext.Length > ShortNameLength ? objectShorttext.Substring(0, ShortNameLength) : objectShorttext;
        }
        /// <summary>
        /// Move EA requirements to Package with moved requirements
        /// </summary>
        public virtual void MoveDeletedRequirements()
        {
            var moveEaElements = from m in DictPackageRequirements
                where !_dtRequirements.Rows.Cast<DataRow>().Any(x => GetAbsoluteNumerFromDoorsId(x["Id"].ToString()) == m.Key)
                select m.Value;
            foreach (var eaId in moveEaElements)
            {
                EA.Element el = _rep.GetElementByID(eaId);
                el.PackageID = PkgDeletedObjects.PackageID;
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
                    MessageBox.Show($@"{e}", @"Error reading EA Repository, break!!");
                    return false;
                }


            }
        }
        /// <summary>
        /// make a name from a string. Remove control characters, multiple white spaces. It's also possible to limit the string length
        /// </summary>
        /// <param name="text"></param>
        /// <param name="length">0 if not cut to length</param>
        /// <returns></returns>
        protected string MakeNameFromString(string text, int length = 0)
        {
            text = Regex.Replace(text, @"\s+", " ");
            text = new string(text.Where(c => char.IsLetterOrDigit(c) || (c >= ' ' && c <= byte.MaxValue)).ToArray());
            return length > 0 && text.Length > length ? text.Substring(0, length) : text;
        }

        /// <summary>
        /// Create Package for deleted objects
        /// </summary>
        /// <returns>Package created Objects</returns>
        protected EA.Package CreateEaPackageDeletedObjects()
        {
            if (PkgDeletedObjects != null) return PkgDeletedObjects;
            if (_pkg.Packages.Count > 0)
            {
                PkgDeletedObjects = (EA.Package) _pkg.Packages.GetAt(0);
                return PkgDeletedObjects;
            }

            PkgDeletedObjects = (EA.Package)_pkg.Packages.AddNew(packageNameDeletedObjects, "");
            PkgDeletedObjects.Update();
            _pkg.Packages.Refresh();
            return PkgDeletedObjects;

        }

        /// <summary>
        /// Export all jobs of the current list number with the respectively defined settings. Only changed tagged values are exported/updated.
        /// </summary>
        /// <param name="listNumber"></param>
        /// <returns></returns>
        public bool RoundtripBySetting(int listNumber)
        {
            bool result = true;
            _level = -1;
            _count = 0;
            _countAll = 0;
            _countPkg = 0;
            _countItems = 0;
            foreach (FileImportSettingsItem item in _importSettings)
            {
                if (Convert.ToInt32(item.ListNo) == listNumber)
                {
                    // Copy input file to export file
                    if (! DirFiles.FileCopy(item.InputFile, item.ExportFile)) return false;
                    _importModuleFile = item.ExportFile;
                    if (!System.IO.File.Exists(_importModuleFile))
                    {
                        MessageBox.Show($@"File: '{_importModuleFile}'", @"Import files doesn't exists, break");
                        return false;
                    }
                    // check if there are columns to update
                    if (item.WriteAttrNameList.Count == 0)
                    {
                        var attributesToVisualize = String.Join(", ",item.WriteAttrNameList.ToArray());
                        MessageBox.Show($@"Roundtrip needs Attributes to write in Settings ('WriteAttrNameList' is empty):

File: '{_importModuleFile}'

Attributes to write ('{nameof(item.WriteAttrNameList)}'):
'{attributesToVisualize}'
", @"No Attributes to write in 'Setting.json' defined");
                        return false;

                    }


                    // handle more than one package
                    // handle zip files like
                    foreach (var itemGuidList in item.PackageGuidList)
                    {
                        string guid = itemGuidList.Guid;
                        _pkg = _rep.GetPackageByGuid(guid);
                        if (_pkg == null)
                        {
                            MessageBox.Show(
                                $@"Package of export list {listNumber} with GUID='{guid}' not available.
{item.Description}
{item.Name}

    Check Import settings in Settings.Json.",
                                @"Package to import into isn't available, break!");
                            return false;
                        }
                        switch (item.ImportType)
                        {

                            case FileImportSettingsItem.ImportTypes.DoorsReqIf:
                            case FileImportSettingsItem.ImportTypes.ReqIf:
                                var reqIfRoundtrip = new ReqIfs.ReqIfRoundtrip(_rep, _pkg, _importModuleFile, item);
                                result = result && reqIfRoundtrip.RoundtripForFile();
                                //await Task.Run(() =>
                                //    doorsReqIf.ImportForFile(eaObjectType, eaStereotype, eaStatusNew, eaStatusChanged));
                                break;
       

                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Export all jobs of the current list number with the respectively defined settings. Currently only changed tagged values are exported/updated.
        /// </summary>
        /// <param name="listNumber"></param>
        /// <returns></returns>
        public bool ExportBySetting(int listNumber)
        {
            bool result = true;
            _level = -1;
            _count = 0;
            _countAll = 0;
            _countPkg = 0;
            _countItems = 0;
            // over all packages
            foreach (FileImportSettingsItem item in _importSettings)
            {
                if (Convert.ToInt32(item.ListNo) == listNumber)
                {
                   
                    _importModuleFile = item.ExportFile;
                   
                    // handle more than one package
                    int subPackageIndex = -1;
                    // handle zip files like
                    foreach (var itemGuidList in item.PackageGuidList)
                    {
                        string guid = itemGuidList.Guid;
                        subPackageIndex += 1;
                        _pkg = _rep.GetPackageByGuid(guid);
                        if (_pkg == null)
                        {
                            MessageBox.Show(
                                $@"Package of export list {listNumber} with GUID='{guid}' not available.
{item.Description}
{item.Name}

    Check Import settings in Settings.Json.",
                                @"Package to import into isn't available, break!");
                            return false;
                        }

                        switch (item.ImportType)
                        {

                            case FileImportSettingsItem.ImportTypes.DoorsReqIf:
                            case FileImportSettingsItem.ImportTypes.ReqIf:
                                var reqIfExport = new ReqIfs.ReqIfExport(_rep, _pkg, _importModuleFile, item);
                                result = result && reqIfExport.ExportRequirements(subPackageIndex);
                                //await Task.Run(() =>
                                //    doorsReqIf.ImportForFile(eaObjectType, eaStereotype, eaStatusNew, eaStatusChanged));
                                break;


                        }


                    }
                }
            }

            return true;
        }


        /// <summary>
        /// Import all jobs of the current list number with the respectively defined settings.
        /// </summary>
        public bool ImportBySetting(int listNumber)
        {
            bool result = true;
            foreach (FileImportSettingsItem item in _importSettings)
            {
                if (Convert.ToInt32(item.ListNo) == listNumber)
                {
                    _importModuleFile = item.InputFile;
                    if (!System.IO.File.Exists(_importModuleFile))
                    {
                        MessageBox.Show($@"File: '{_importModuleFile}'", @"Import files doesn't exists, break");
                        return false;
                    }
                    if (!ImportByFile(listNumber, item, ref result)) return false;
                    // run for package
                    // handle links
                    if (_reqIfDeserialized != null && 
                        (item.ImportType is FileImportSettingsItem.ImportTypes.ReqIf || item.ImportType is FileImportSettingsItem.ImportTypes.DoorsReqIf))
                    {
                        ReqIfRelation relations = new ReqIfRelation(_reqIfDeserialized, Rep, item);
                        relations.WriteRelations();
                    }
                }

            }

            return result;

        }
        /// <summary>
        /// Import according to a list element / file. An item contains a file reference.
        /// </summary>
        /// <param name="listNumber"></param>
        /// <param name="item"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private bool ImportByFile(int listNumber, FileImportSettingsItem item, ref bool result)
        {
            if (item.PackageGuidList.Count == 0) return true;
                string guid = item.PackageGuidList[0].Guid;
                _pkg = _rep.GetPackageByGuid(guid);
                if (_pkg == null)
                {
                    MessageBox.Show(
                        $@"Package of import list {listNumber} with GUID='{guid}' not available.
{item.Description}
{item.Name}

Check Import settings in Settings.Json.",
                        @"Package to import into isn't available, break!");
                    return false;
                }


                string eaObjectType = item.ObjectType;
                string eaStereotype = item.Stereotype;
                string eaStatusNew = String.IsNullOrEmpty(item.StatusNew) || item.StatusNew == "None"
                    ? ""
                    : item.StatusNew;
                string eaStatusChanged = String.IsNullOrEmpty(item.StatusChanged) || item.StatusChanged == "None"
                    ? ""
                    : item.StatusChanged;


                switch (item.ImportType)
                {
                    case FileImportSettingsItem.ImportTypes.DoorsCsv:
                        var doorsCsv = new DoorsCsv(_rep, _pkg, item.InputFile, item);
                        result = result && doorsCsv.ImportForFile(eaObjectType, eaStereotype, eaStatusNew,
                                     eaStatusChanged);
                        //await Task.Run(() =>
                        //     doorsCsv.ImportForFile(eaObjectType, eaStereotype, eaStatusNew, eaStatusChanged));
                        break;

                    case FileImportSettingsItem.ImportTypes.DoorsReqIf:
                    case FileImportSettingsItem.ImportTypes.ReqIf:
                    var doorsReqIf = new ReqIfs.ReqIfImport(_rep, _pkg, item.InputFile, item, _reqIfLogList);
                        result = result && doorsReqIf.ImportForFile(eaObjectType, eaStereotype, 
                                     eaStatusNew);
                        _reqIfDeserialized = doorsReqIf.ReqIfDeserialized;
                        if (doorsReqIf.CountPackage > 1) return result;
                        //await Task.Run(() =>
                        //    doorsReqIf.ImportForFile(eaObjectType, eaStereotype, eaStatusNew, eaStatusChanged));
                        break;

                    
                       
                    case FileImportSettingsItem.ImportTypes.XmlStruct:
                        var xmlStruct = new XmlStruct(_rep, _pkg, item.InputFile, item);
                        result = result && xmlStruct.ImportForFile(eaObjectType, eaStereotype, eaStatusNew,
                                     eaStatusChanged);
                        //await Task.Run(() =>
                        //    reqIf.ImportForFile(eaObjectType, eaStereotype, eaStatusNew, eaStatusChanged));
                        break;
                }
            return true;
        }


        protected EA.Repository Rep { get => _rep; set => _rep = value; }
        protected EA.Package Pkg { get => _pkg; set => _pkg = value; }
        protected string ImportModuleFile { get => _importModuleFile; set => _importModuleFile = value; }
        /// <summary>
        /// EA Element-Ids of DOORS IDs
        /// </summary>
        public Dictionary<string, int> DictObjectIdFromDoorsId { get => DictPackageRequirements; set => DictPackageRequirements = value; }

        protected DataTable DtRequirements { get => _dtRequirements; set => _dtRequirements = value; }

        protected int Count
        {
            get { return _count; }
            set { _count = value; }
        }

        protected int CountChanged
        {
            get { return _countChanged; }
            set { _countChanged = value; }
        }

        protected int CountNew
        {
            get { return _countNew; }
            set { _countNew = value; }
        }

        /// <summary>
        /// Dictionary for 'Requirement ID' as string and its EA Object_ID as integer
        /// </summary>
        protected Dictionary<string, int> DictPackageRequirements
        {
            get { return _dictPackageRequirements; }
            set { _dictPackageRequirements = value; }
        }

        protected Package PkgDeletedObjects
        {
            get { return _pkgDeletedObjects; }
            set { _pkgDeletedObjects = value; }
        }
    }
}
