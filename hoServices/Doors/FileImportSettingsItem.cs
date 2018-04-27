using Newtonsoft.Json;
// ReSharper disable InconsistentNaming

namespace EaServices.Doors
{
    /// <summary>
    /// Settings item for file import like '*.csv' from DOORS 9.6 compatible export
    /// </summary>
    public class FileImportSettingsItem
    {
        /// <summary>
        /// The type to import
        /// </summary>
        public enum ImportTypes
        {
           DoorsCsv,
           DoorsReqIf
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
        public string PackageGuid
        {
            get => _packageGuid;
            set => _packageGuid = value;
        }
        [JsonProperty("PackageGuid")]
        private string  _packageGuid;

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
        /// StatusNew
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
        /// StatusNew
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
        /// Description
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
        /// Tooltip
        /// </summary>
        [JsonIgnore]
        public string Tooltip
        {
            get => _tooltip;
            set => _tooltip = value;
        }
        [JsonProperty("Tooltip")]
        private string  _tooltip;

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

        


    }
}
