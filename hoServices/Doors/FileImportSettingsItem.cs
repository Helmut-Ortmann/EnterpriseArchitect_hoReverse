using System.Collections.Generic;
using Newtonsoft.Json;

namespace EaServices.Doors
{
    /// <summary>
    /// Settings item for file import like '*.csv' from DOORS 9.6 compatible export
    /// </summary>
    public class FileImportSettingsItem : hoUtils.Json.IMenuItem
    {
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
        [JsonProperty("ImportType")]
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
        /// Stereotype to use when creating the EA Element for a requirement/test
        /// </summary>
        [JsonIgnore]
        public string Stereotype
        {
            get => _stereotype;
            set => _stereotype = value;
        }
        [JsonProperty("Stereotype")]
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
