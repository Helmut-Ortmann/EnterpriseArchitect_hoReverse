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
           DoorsCsv,
           DoorsReqIf,
           ReqIf,        // pure ReqIF
           XmlStruct,    // Structured XML
           XmlFlat       // Flat XML
        }
        [JsonProperty("ImportType")]
        public ImportTypes ImportType { get; set; }

        /// <summary>
        /// The file to import
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
        /// PackageGuid
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
        /// Stereotype
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
        /// Status Changed. Sets the EA Element Property: 'Status'. If 'None' or blank no EA Status is updated
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
        /// Description to viziualize as Tooltip
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
        /// List of Attributes for ReqIF
        /// </summary>
        [JsonIgnore]
        public List<string> AttrList
        {
            get => _attrList ?? new List<string>();
            set => _attrList = value;
        }

        [JsonProperty("AttrList")]
        private List<string>  _attrList;

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
        /// ReqIf Attributes to store as name
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
        /// ReqIf Attributes to store as *.rtf
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
        /// ReqIf Attributes to store as Alias
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
        /// Id which build the ID stored in Multiplicity to identify the requirement
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
