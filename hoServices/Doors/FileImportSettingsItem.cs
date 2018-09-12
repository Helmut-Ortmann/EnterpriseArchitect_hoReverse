using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using hoUtils;
using Newtonsoft.Json;

namespace EaServices.Doors
{
    /// <summary>
    /// Settings item for file import like '*.csv' from DOORS 9.6 compatible export
    /// </summary>
    public class FileImportSettingsItem : hoUtils.Json.IMenuItem
    {
        public enum VerbosityType
        {
            Silent,       // ignore false OLE, not supported
            Verbose
        }
        [JsonProperty("Verbosity"), DefaultValue("Silent")]
        public VerbosityType Verbosity  { get; set; }

        /// <summary>
        /// The type to import
        /// </summary>
        public enum ImportTypes
        {
           DoorsCsv,     // DOORS standard *.csv format
           DoorsReqIf,   // DOORS ReqIF format
           ReqIf,        // pure ReqIF
           XmlStruct,    // Structured XML, use it e.g. for test
           XmlFlat       // Flat XML, currently not used
        }
        [JsonProperty("ImportType"),DefaultValue("Requirement")]
        public ImportTypes ImportType { get; set; }

        /// <summary>
        /// The file to import. 
        ///
        /// Example: myImport.reqifz, myImport.csv, myImport.xml
        /// </summary>
        [JsonIgnore]
        public string InputFile
        {
            get => _inputFile?.Replace(@"\", "/") ?? "";
            set => _inputFile = value;
        }
        [JsonProperty("InputFile")]
        private string _inputFile;

        /// <summary>
        /// The file to export. The default is the input file name with "_Export" at the end
        ///
        /// Example: myImport.reqifz, myImport.csv, myImport.xml
        /// </summary>
        [JsonIgnore]
        public string ExportFile
        {
            get => String.IsNullOrWhiteSpace(_exportFile) 
                ? Path.Combine(Path.GetDirectoryName(_inputFile), $"{Path.GetFileNameWithoutExtension(_inputFile)}_Export.{Path.GetExtension(_inputFile)}") 
                : _exportFile?.Replace(@"\", "/") ;
            set => _exportFile = value;
        }
        [JsonProperty("ExportFile")]
        private string _exportFile;

        /// <summary>
        /// The file to export. The default is the input file name with "_Export" at the end
        ///
        /// Example: myImport.reqifz, myImport.csv, myImport.xml
        /// </summary>
        [JsonIgnore]
        public string RoundtripFile
        {
            get => String.IsNullOrWhiteSpace(_roundtripFile)
                ? Path.Combine(Path.GetDirectoryName(_inputFile), $"{Path.GetFileNameWithoutExtension(_inputFile)}_Roundtrip.{Path.GetExtension(_inputFile)}")
                : _roundtripFile?.Replace(@"\", "/");
            set => _roundtripFile = value;
        }
        [JsonProperty("RoundtripFile")]
        private string _roundtripFile;

        /// <summary>
        /// The dictionary to store the embedded files. If this parameter isn't set a temp directory is used.
        ///
        /// Example: myImport.reqifz, myImport.csv, myImport.xml
        /// </summary>
        [JsonIgnore]
        public string EmbeddedFileStorageDictionary
        {
            get => String.IsNullOrWhiteSpace(_embeddedFileStorageDictionary) 
                                ? DirectoryExtension.GetTempDir("ReqIfEmbeddedFiles") 
                                :_embeddedFileStorageDictionary?.Replace(@"\", "/").Trim() ?? "";
            set => _embeddedFileStorageDictionary = value;
        }
        [JsonProperty("EmbeddedFileStorageDictionary"), DefaultValue(" ")]
        private string _embeddedFileStorageDictionary;

        /// <summary>
        /// The dictionary inside EmbeddedFileStorageDictionary for png images in text
        ///
        /// Example: 'Files'
        /// </summary>
        [JsonIgnore]
        public string EmbeddedFilesPng
        {
            get => _embeddedFilesPng?.Replace(@"\", "/").Trim() ?? "Files";
            set => _embeddedFilesPng = value;
        }
        [JsonProperty("EmbeddedFilesPng"), DefaultValue("Files")]
        private string _embeddedFilesPng;

        /// <summary>
        /// The relative path of the embedded files
        ///
        /// Example: 'Files'
        /// </summary>
        [JsonIgnore]
        public string EmbeddedFiles
        {
            get => _embeddedFiles?.Replace(@"\", "/").Trim() ?? "EmbeddedFiles";
            set => _embeddedFiles = value;
        }
        [JsonProperty("EmbeddedFiles"), DefaultValue("EmbeddedFiles")]
        private string _embeddedFiles;

        /// <summary>
        /// The relative path of the embedded file images to visualize for each mime type
        ///
        /// Example: 'Files'
        /// </summary>
        [JsonIgnore]
        public string EmbeddedFileImages
        {
            get => _embeddedFileImages?.Replace(@"\", "/").Trim() ?? "EmbeddedFileImages";
            set => _embeddedFileImages = value;
        }
        [JsonProperty("EmbeddedFileImages"), DefaultValue("EmbeddedFileImages")]
        private string _embeddedFileImages;

        /// <summary>
        /// The xhtml namespace name
        ///
        /// Example: 'Files'
        /// </summary>
        [JsonIgnore]
        public string NameSpace
        {
            get => _nameSpace?.Trim() ?? @"reqif-xhtml";
            set => _nameSpace = value;
        }
        [JsonProperty("NameSpace"), DefaultValue("reqif-xhtml")]
        private string _nameSpace;





        /// <summary>
        /// List of PackageGuids. hoReverse puts the Requirements beneath this package.
        /// </summary>
        [JsonIgnore]
        public List<string> PackageGuidList
        {
            get => _packageGuidList ?? new List<string>();
            set => _packageGuidList = value;
        }
        [JsonProperty("PackageGuidList")]
        private List<string>  _packageGuidList;

        /// <summary>
        /// Export Mapping List to map EA Attributes to ReqIF Attributes.
        ///
        /// Example:
        /// ["ReqIF.Text=EA.LinkedDocument;", "ReqIF.ForeignID=EA.GUID"]
        /// Supported:
        /// - ReqIF.Name  (default: Name)
        /// - ReqIF.Text  (default: Linked Document, Notes)
        /// - ReqIF.ForeignID (default: EA.GUID)
        /// - EA.LinkedDocument
        /// - EA.Alias
        /// - EA.GUID (GUID of requirement or whatever EA type you use)
        /// - EA.Note (EA Notes)
        /// </summary>
        [JsonIgnore]
        public List<string> ExportMappingList
        {
            get => _exportMappingList ?? new List<string>();
            set => _exportMappingList = value;
        }
        [JsonProperty("ExportMappingList")]
        private List<string> _exportMappingList;


        /// <summary>
        /// Get comma separated list of GUIDs ('guid1','guid2')
        /// </summary>
        [JsonIgnore]
        public string PackageGuidCommaList
        {
            get => $"'{String.Join("','", PackageGuidList.ToArray())}'".Replace(" ","");
        }

        /// <summary>
        /// List of EA TaggedValue prefixe per ReqIF module. hoReverse uses this prefixes to allow same column names in ReqIF modules.
        /// </summary>
        [JsonIgnore]
        public List<string> PrefixTaggedValueTypeList
        {
            get => _prefixTaggedValueTypeList ?? new List<string>();
            set => _prefixTaggedValueTypeList = value;
        }
        [JsonProperty("PrefixTaggedValueTypeList")]
        private List<string> _prefixTaggedValueTypeList;

        /// <summary>
        /// ObjectType
        /// </summary>
        [JsonIgnore]
        public string ObjectType
        {
            get => _objectType;
            set => _objectType = value;
        }
        [JsonProperty("ObjectType")]
        private string  _objectType;

        /// <summary>
        /// Stereotype dependency Requirement linking
        /// </summary>
        [JsonIgnore]
        public string StereotypeDependency
        {
            get => String.IsNullOrWhiteSpace(_stereotypeDependency) ? "" : _stereotypeDependency.Trim();
            set => _stereotypeDependency = value;
        }
        [JsonProperty("StereotypeDependency"), DefaultValue(" ")]
        private string _stereotypeDependency;

        /// <summary>
        /// Stereotype to use when creating the EA Element for a requirement/test
        /// </summary>
        [JsonIgnore]
        public string Stereotype
        {
            get => String.IsNullOrWhiteSpace(_stereotype) ? "" : _stereotype.Trim();
            set => _stereotype = value;
        }
        [JsonProperty("Stereotype"), DefaultValue(" ")]
        private string  _stereotype;

        /// <summary>
        /// StatusNew. Sets the EA Element Property: 'Status'. If 'None' or blank no EA Status is updated
        /// </summary>
        [JsonIgnore]
        public string StatusNew
        {
            get => _statusNew;
            set => _statusNew = value;
        }
        [JsonProperty("StatusNew")]
        private string  _statusNew;

        /// <summary>
        /// Only used for *.csv import format.
        /// 
        /// Status Changed. Sets the EA Element Property: 'Status'. If 'None' or blank no EA Status is updated
        ///
        /// </summary>
        [JsonIgnore]
        public string StatusChanged
        {
            get => _statusChanged;
            set => _statusChanged = value;
        }
        [JsonProperty("StatusChanged")]
        private string  _statusChanged;

        /// <summary>
        /// Description to viziualize as Tooltip in hoReverse Menu
        /// </summary>
        [JsonIgnore]
        public string Description
        {
            get => _description;
            set => _description = value;
        }
        [JsonProperty("Description")]
        private string  _description;

        /// <summary>
        /// Name of the Importer to visualize in Menu
        /// </summary>
        [JsonIgnore]
        public string Name
        {
            get => _name;
            set => _name = value;
        }
        [JsonProperty("Name")]
        private string  _name;

        /// <summary>
        /// ListNo. The number of the list to group lists.
        /// Use such lists to import more than one file at a time. If so, give the grouped list the same number
        /// </summary>
        [JsonIgnore]
        public string ListNo
        {
            get => _listNo;
            set => _listNo = value;
        }
        [JsonProperty("ListNo")]
        private string  _listNo;


        /// <summary>
        /// ReqIf Attribute to store in Notes
        /// </summary>
        [JsonIgnore]
        public string AttrNotes
        {
            get => _attrNotes;
            set => _attrNotes = value;
        }
        [JsonProperty("AttrNotes")]
        private string  _attrNotes;

        /// <summary>
        /// List of ReqIf Attributes to store as name
        /// </summary>
        [JsonIgnore]
        public List<string> AttrNameList
        {
            get => _attrNameList ?? new List<string>();
            set => _attrNameList = value;
        }
        [JsonProperty("AttrNameList")]
        private List<string>  _attrNameList;


        /// <summary>
        /// List of ReqIf Attributes to possibly overwrite and send back
        /// This is the implementation of ReqIF Workflow
        /// </summary>
        [JsonIgnore]
        public List<string> WriteAttrNameList
        {
            get => _writeAttrNameList ?? new List<string>();
            set => _writeAttrNameList = value;
        }
        [JsonProperty("WriteAttrNameList")]
        private List<string> _writeAttrNameList;

        /// <summary>
        /// List of ReqIf Attributes to store as *.rtf
        /// </summary>
        [JsonIgnore]
        public List<string> RtfNameList
        {
            get => _rtfNameList ?? new List<string>();
            set => _rtfNameList = value;
           
        }
        [JsonProperty("RtfNameList")]
        private List<string>  _rtfNameList;

        /// <summary>
        /// List of ReqIf Attributes to store as Alias
        /// </summary>
        [JsonIgnore]
        public List<string> AliasList
        {
            get => _aliasList ?? new List<string>();
            set => _aliasList = value;

        }
        [JsonProperty("AliasList")]
        private List<string> _aliasList;

        /// <summary>
        /// List of Attributes which build the ID stored in Multiplicity to identify the requirement (length less than 50). Only ImportType: "XmlStruct"
        ///
        /// Only used for type: xml
        /// </summary>
        [JsonIgnore]
        public List<string> IdList
        {
            get => _idList ?? new List<string>();
            set => _idList = value;

        }
        [JsonProperty("IdList")]
        private List<string> _idList;

       
    }
}
