using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using hoReverse.hoUtils;
using ReqIFSharp;

namespace EaServices.Doors.ReqIfs
{
    /// <summary>
    /// Generalization of Import, Export, Roundtrip
    /// Currently generalization isn't implemented by class hierarchy.
    /// </summary>
    public class ReqIf : DoorsModule
    {
        protected static string NameSpace; // XHTML NameSpace
        /// <summary>
        /// The Attribute definitions of the specification
        /// </summary>
        protected List<ReqIFSharp.AttributeDefinition> _moduleAttributeDefinitions;

        protected readonly FileImportSettingsItem Settings;

        protected bool _errorMessage1;


        protected ReqIFContent _reqIfContent;

        /// <summary>
        /// Deserialized ReqIF
        /// </summary>
        public ReqIF ReqIfDeserialized { get; protected set; }

        protected int _subModuleIndex;

        // Prefix Tagged Values and Column-names
        protected string _prefixTv = "";
        /// <summary>
        /// The export fields to  export for Export and Roundtrip
        /// </summary>
        protected ExportFields _exportFields;


        public ReqIf()
        {

        }

        /// <summary>
        /// ReqIF Import/Roundtrip/Export
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="pkg"></param>
        /// <param name="importFile"></param>
        /// <param name="settings"></param>
        public ReqIf(EA.Repository rep, EA.Package pkg, string importFile, FileImportSettingsItem settings, List<ReqIfLog> reqIfLogList = null) : base(rep,
            pkg, importFile, reqIfLogList)
        {
            Settings = settings;
            NameSpace = settings.NameSpace;
        }
        /// <summary>
        /// Deserialize ReqIF with or without XML validation. If error and no validation was selected ask if retrying with validation or cancel
        /// </summary>
        /// <param name="file"></param>
        /// <param name="validate"></param>
        /// <returns></returns>
        public static ReqIF DeSerializeReqIf(string file, bool validate = false)
        {
            // Deserialize
            ReqIFDeserializer deserializer = new ReqIFDeserializer();
            try
            {
                return deserializer.Deserialize(file, validate: validate);
            }
            catch (Exception e)
            {
                if (validate)
                {
                    MessageBox.Show($@"File: {file}
Validate: true

{e}", @"Can't deserialize existing ReqIF file");
                    return null;
                }
                else
                {
                    // was without validation, ask if retry with validation
                    var ret = MessageBox.Show($@"File: {file}
Validate: false

{e}", @"Can't deserialize existing ReqIF file, retry with XML validation?", MessageBoxButtons.RetryCancel);
                    if (ret == DialogResult.Cancel) return null;
                    try
                    {
                        return deserializer.Deserialize(file, validate: true);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($@"File: {file}
Validate: true

{ex}", @"Can't deserialize existing ReqIF file, break!");
                        return null;
                    }

                }
            }
        }
        /// <summary>
        /// Serialize ReqIF
        /// </summary>
        /// <param name="file"></param>
        /// <param name="compress"></param>
        /// <param name="prefixReqIf">Prefix to add to the reqIF file (e.g. "_")</param>
        /// <returns></returns>
        protected bool SerializeReqIf(string file, bool compress = true, string prefixReqIf = "")
        {
            var serializer = new ReqIFSerializer(false);
            Debug.Assert(file != null, nameof(file) + " != null");
            string pathSerialize = file;
            try
            {
                string prefix = Path.GetFileNameWithoutExtension(file).StartsWith(prefixReqIf) ? "" : prefixReqIf;
                pathSerialize = Path.Combine(Path.GetDirectoryName(file) ?? throw new InvalidOperationException(),
                    $"{prefix}{Path.GetFileNameWithoutExtension(file)}.reqif");

                serializer.Serialize(ReqIfDeserialized, pathSerialize, null);


            }
            catch (Exception e)
            {
                MessageBox.Show($@"Path:
'{pathSerialize}'

{e}", @"Error serialize ReqIF, break!");
                return false;
            }
            if (compress)
                return Compress(ImportModuleFile, Path.GetDirectoryName(pathSerialize));
            return true;
        }
        /// <summary>
        /// Make xhtml namespace and correct some peculiar things (not supported by xhtml for reqIF)
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        protected static string MakeNameSpace(string text)
        {
            text = text.Replace(@"&nbsp;", "");
            text = text.Replace(@"&laquo;", "");// <<
            text = text.Replace(@"&raquo;", "");// >>

            Regex rx = new Regex(@"</");
            text = rx.Replace(text, $@"</{NameSpace}:");

            rx = new Regex(@"<(\w)");
            text = rx.Replace(text, $@"<{NameSpace}:$1");

            text = text.Replace($@"{NameSpace}:br />", $@"{NameSpace}:br/>");


            return text;
        }
        /// <summary>
        /// Add XHTML Namespace to string
        /// </summary>
        /// <param name="stringText"></param>
        /// <returns></returns>
        protected static string AddXtmlNameSpace(string stringText)
        {
            string xhtmlContent =
                $@"<{NameSpace}:div xmlns:{NameSpace}=""http://www.w3.org/1999/xhtml"">{MakeNameSpace(stringText)}</{NameSpace}:div>";
            return xhtmlContent;
        }
        /// <summary>
        /// Limit to ReqIF XHTML tags
        /// </summary>
        /// <param name="stringText"></param>
        /// <returns></returns>
        protected static string LimitReqIfXhtml(string stringText)
        {
            // delete not allowed ReqIF things
            // '<u>' underline, replace by 
            // </u>>
            // ul type="xxxx" xxxx=disc, ...  remove type="xxx"
            // li value="n"   n=Number        remove value="n"
            stringText = stringText.Replace("<u>", "<ins>").Replace("</u>", "</ins>");

            stringText = Regex.Replace(stringText, @"<ol type=""[^""]*""", "<ol");  // Replace type in ul
            stringText = Regex.Replace(stringText, @"<ul type=""[^""]*""", "<ul");  // Replace type in ul
            stringText = Regex.Replace(stringText, @"<li value=""[^""]*""", "<li");  // Replace value in li
            stringText = Regex.Replace(stringText, @"<font [^>]*>", "");  // Replace font tag
            stringText = Regex.Replace(stringText, @"</font>", "");  // Replace font tag



            return stringText;

        }
        /// <summary>
        /// Make XHTML from a html string. It inserts the xhtml namespace
        /// </summary>
        /// <param name="htmlValue"></param>
        /// <returns></returns>
        protected static string MakeXhtmlFromHtml(string htmlValue)
        {
            //stringValue = stringValue.Replace("\r\n", "<br></br>");
            htmlValue = htmlValue.Replace("&nbsp;", "");
            //htmlValue = Regex.Replace(htmlValue, @">\s*<", "><");  // Replace Blanks between control sequences
            htmlValue = LimitReqIfXhtml(htmlValue);
            return AddXtmlNameSpace(htmlValue);
        }
        /// <summary>
        /// Make XHTML from a simple string. It inserts the xhtml namespace and handles cr/lf
        /// </summary>
        /// <param name="stringValue"></param>
        /// <returns></returns>
        protected static string MakeXhtmlFromString(string stringValue)
        {
            if (String.IsNullOrWhiteSpace(stringValue)) stringValue = "";

            stringValue = stringValue.Replace("\r\n", "<br/>");
            stringValue = stringValue.Replace("&nbsp;", "");
            stringValue = Regex.Replace(stringValue, @">\s*<", "><");  // Replace Blanks between control sequences
            stringValue = LimitReqIfXhtml(stringValue);
            return AddXtmlNameSpace(stringValue);
        }
        /// <summary>
        /// Set the values of a ReqIF enum (single value or multi value) in the ReqIF (AttributeDefinition)
        /// </summary>
        /// <param name="attributeValueEnumeration"></param>
        /// <param name="value"></param>
        public bool SetReqIfEnumValue(AttributeValueEnumeration attributeValueEnumeration, string value)
        {
            // delete old values
            attributeValueEnumeration.Values.Clear();

            if (!attributeValueEnumeration.Definition.IsMultiValued)
            {
                // single value enum
                // Check if valid value
                if (String.IsNullOrWhiteSpace(value))
                {
                    MessageBox.Show($@"Empty value of ReqIF Attribute: '{attributeValueEnumeration.Definition.LongName}'

", @"Can't find enum value, break");
                    return false;
                }

                var enumValue = attributeValueEnumeration.Definition.Type.SpecifiedValues
                    .SingleOrDefault(x => x.LongName == value.Trim());
                if (enumValue == null)
                {
                    MessageBox.Show($@"ReqIF Attribute: '{attributeValueEnumeration.Definition.LongName}'

Value='{value}'
", @"Can't find enum value, break");
                    return false;
                }
                attributeValueEnumeration.Values.Add(enumValue);
            }
            else
            {   // all enums (multi value enum)
                // no value is valid
                if (String.IsNullOrWhiteSpace(value)) return true;

                var enumValues = attributeValueEnumeration.Definition.Type.SpecifiedValues
                    .Select(x => x);
                var values = Regex.Replace(value.Trim(), @"\r\n?|\n|;|,|:|-|=", ",").Split(',');
                int index = 0;
                foreach (EnumValue enumValue in enumValues)
                {
                    if (values.Length > index && values[index] == "1")
                    {
                        attributeValueEnumeration.Values.Add(enumValue);
                    }
                    index += 1;

                }
            }

            return true;

        }
        /// <summary>
        /// Get prefixed Tagged Value name from name without prefix
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        protected string GetPrefixedTagValueName(string name)
        {
            if (name.StartsWith(_prefixTv)) return name;
            return $"{_prefixTv}{name}";
        }
        /// <summary>
        /// Get prefixed Tagged Value name from name without prefix
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        protected string GetUnPrefixedTagValueName(string name)
        {
            if (name.StartsWith(_prefixTv)) return name.Substring(_prefixTv.Length);
            return name;
        }
        /// <summary>
        /// Compress the files of the directory
        /// </summary>
        /// <param name="zipFile">The path of the zip achive</param>
        /// <param name="dirNameReqIfFiles">The directory to zip</param>
        /// <param name="dirNameFiles"></param>
        protected bool Compress(string zipFile, string dirNameReqIfFiles, string dirNameFiles = "")
        {

            if (File.Exists(zipFile)) File.Delete(zipFile);

            // Check if directory for zip-file exists
            if (! Directory.Exists(Path.GetDirectoryName(zipFile)))
            {
                MessageBox.Show($@"Zip-File: {zipFile}", @"Directory zip files doesn't exists, break;");
                return false;
            }
            if (!String.IsNullOrEmpty(dirNameFiles))
            {
                ZipFile.CreateFromDirectory(dirNameFiles, zipFile);
            }

            using (var zip = ZipFile.Open(zipFile, ZipArchiveMode.Update))
            {


                // ReqIF files
                foreach (var file in Directory.GetFiles(dirNameReqIfFiles))
                {
                    if (Path.GetFileName(file).ToLower().EndsWith("reqif"))
                        zip.CreateEntryFromFile(file, Path.GetFileName(file));
                }

            }

            return true;
        }
        /// <summary>
        /// Decompress the import/export reqIf file if compressed format (*.reqifz). It returns an array of *.reqIF files.
        /// </summary>
        /// <param name="importReqIfFile"></param>
        /// <returns>The path to the *.reqif file</returns>
        public string[] Decompress(string importReqIfFile)
        {
            // *.reqifz for compressed ReqIf File
            if (importReqIfFile.ToUpper().EndsWith("Z"))
            {
                string extractDirectory = hoUtils.Compression.Zip.ExtractZip(importReqIfFile);
                if (String.IsNullOrWhiteSpace(extractDirectory)) return new string[] {};

                // extract reqif files from achive
                string pattern = "*.reqif";
                var files = Directory.GetFiles(extractDirectory, pattern);
                if (files.Length == 0)
                {
                    MessageBox.Show($@"Can't find '*.reqif' file(s) in decompressed folder

*.reqifz File :  '{importReqIfFile}'
Pattern       :  '{pattern}'
Extract folder:  '{extractDirectory}'", @"Can't find '*.reqif' file in decompressed folder");
                    return new string[0];
                }

                return files;
            }

            return new string[] { importReqIfFile };
        }
        /// <summary>
        /// Add Tagged Values with Module Properties to Package/Object and
        /// Specification: LongName
        /// Specification: Identifier
        /// </summary>
        /// <param name="reqIf"></param>
        /// <param name="el"></param>
        private void GetModuleProperties(ReqIF reqIf, EA.Element el)
        {
            var moduleProperties = from obj in reqIf.CoreContent[0].Specifications[0].Values
                select new { Value = obj.ObjectValue.ToString(), Name = obj.AttributeDefinition.LongName, Type = obj.AttributeDefinition.GetType() };
            foreach (var property in moduleProperties)
            {
                // if writable don't overwrite value, only create TV
                if (_exportFields.IsWritableValue(property.Value))
                    TaggedValue.CreateTaggedValue(el, property.Value); // only create TV
                else TaggedValue.SetUpdate(el, property.Name, GetStringAttrValue(property.Value ?? "")); // update TV
            }

            TaggedValue.SetUpdate(el, "LongName", reqIf.CoreContent[0].Specifications[0].LongName);
            TaggedValue.SetUpdate(el, "Identifier", reqIf.CoreContent[0].Specifications[0].Identifier);
        }

        /// <summary>
        /// Update Package with Standard and ReqIF Properties
        /// </summary>
        protected override void UpdatePackage()
        {
            EA.Element el = Rep.GetElementByGuid(Pkg.PackageGUID);
            GetModuleProperties(ReqIfDeserialized, el);

            base.UpdatePackage();
        }
        // Get all specObjectTypes of
        // void AddModuleAttributeTypes(List<ReqIFSharp.SpecObjectType> specObjectTypes, List<SpecHierarchy> children) {
        /// <summary>
        /// Get 
        /// </summary>
        /// <param name="specObjectTypes"></param>
        /// <param name="children"></param>
        void AddModuleAttributeTypes(List<ReqIFSharp.AttributeDefinition> specObjectTypes, List<SpecHierarchy> children)
        {
            foreach (SpecHierarchy child in children)
            {
                var specAttributes = child.Object.Type.SpecAttributes.Select(sa => sa);
                specObjectTypes.AddRange(specAttributes);
                AddModuleAttributeTypes(specObjectTypes, child.Children);
            }
        }
        /// <summary>
        /// Get types of a ReqIF module
        /// </summary>
        /// <param name="reqIf"></param>
        /// <param name="reqifContentIndex"></param>
        /// <param name="reqIfSpecIndex"></param>
        /// <returns></returns>
        protected List<ReqIFSharp.AttributeDefinition> GetTypesModule(ReqIF reqIf, int reqifContentIndex, int reqIfSpecIndex)
        {
            var specObjectTypes = new List<ReqIFSharp.AttributeDefinition>();
            var children = reqIf.CoreContent[reqifContentIndex].Specifications[reqIfSpecIndex].Children;
            foreach (SpecHierarchy child in children)
            {
                AddModuleAttributeTypes(specObjectTypes, child.Children);
            }
            return specObjectTypes.Select(x => x).Distinct().ToList();
        }
        /// <summary>
        /// Get string Attribute Value. It converts xhtml and if makeName = true it removes multiple white spaces and control sequences from string
        /// </summary>
        /// <param name="attrValue"></param>
        /// <param name="makeName">True: Remove multiple whitespaces, control characters</param>
        /// <returns></returns>
        protected string GetStringAttrValue(string attrValue, bool makeName = false)
        {
            // convert xhtml or use the origianl text
            var text = attrValue.Contains("http://www.w3.org/1999/xhtml") ? HtmlToText.ConvertReqIfXhtml(attrValue) : attrValue;
            return makeName ? MakeNameFromString(text) : text;
        }
        /// <summary>
        /// Check import/roundtrip file
        /// </summary>
        /// <returns></returns>
        protected bool CheckImportFile()
        {
            if (Settings.PackageGuidList.Count == 0)
            {
                MessageBox.Show(@"See: File, Settings

Parameter: PackageGuidList
is missing!
", @"No Package GUID defined in Settings, break");
                return false;
            }

            return true;
        }
    }
}
