using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using EA;
using hoReverse.hoUtils;
using ReqIFSharp;
using File = System.IO.File;
using Path = System.IO.Path;
using TaggedValue = hoReverse.hoUtils.TaggedValue;

namespace EaServices.Doors.ReqIfs
{

    /// <summary>
    /// Import ReqIf requirements. The subclass ReqIfDoors handles DOORS specific features (currently not used). It handles:
    /// - *.reqif
    /// - *.reqifz (compressed)
    /// - Images like *.png
    /// - Modules (in settings give two package GUIDs
    /// Currently ReqIf doesn't support embedded files other than image (no ole, excel, pdf)
    /// </summary>
    public partial class ReqIf : DoorsModule
    {
        readonly string Tab = "\t";
        protected static string NameSpace; // XHTML NameSpace
        
        public int CountPackage = 0;
        ReqIF _reqIf;
        ReqIFContent _reqIfContent;

        /// <summary>
        /// Deserialized ReqIF
        /// </summary>
        public ReqIF ReqIFDeserialized
        {
            get { return _reqIf; }
        }
        int _subModuleIndex;

        // Prefix Tagged Values and Column-names
        private string _prefixTv = "";

        ExportFields _exportFields;
        List<ReqIFSharp.AttributeDefinition> _moduleAttributeDefinitions;

        protected readonly FileImportSettingsItem Settings;

        bool _errorMessage1;

        // Attributes not to import
        readonly String[] _blackList1 = new String[]
        {
            "TableType", "TableBottomBorder", "TableCellWidth", "TableChangeBars", "TableLeftBorder",
            "TableLinkIndicators", "TableRightBorder", "TableShowAttrs", "TableTopBorder"
        }; // DOORS Table requirements

        /// <summary>
        /// ReqIF Export/Roundtrip
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="pkg"></param>
        /// <param name="importFile"></param>
        /// <param name="settings"></param>
        public ReqIf(EA.Repository rep, EA.Package pkg, string importFile, FileImportSettingsItem settings) : base(rep,
            pkg, importFile)
        {
            Settings = settings;
            NameSpace = settings.NameSpace;
        }

        /// <summary>
        /// Roundtrip for a compressed reqif file (*.reqif)
        /// - Import
        /// - Updated Requirements according to TaggedValues/AttributeNames in _settings.WriteAttrNameList
        /// </summary>
        /// <returns></returns>
        public override bool RoundtripForFile()
        {
            CountPackage = 0;
            _subModuleIndex = 0;
            
            _errorMessage1 = false;
            _exportFields = new ExportFields(Settings.WriteAttrNameList);

            // decompress reqif file and its embedded files
            string[] importReqIfFiles = Decompress(ImportModuleFile);
            if (importReqIfFiles.Length == 0) return false;

            // Inventory all reqIf files with their specifications
            ReqIfFileList reqIfFileList = new ReqIfFileList(importReqIfFiles);
            if (reqIfFileList.ReqIfFileItemList.Count == 0) return false;
            
            // Check import settings
            if (!CheckImportFile()) return false;

            // over all to roundtrip Packages/Specification/Modules of the current *.reqifz file
            // Import all Specifications of file
            // Import by Specification ID in item definition/file definition (more reliable)
            if (!String.IsNullOrWhiteSpace(Settings.PackageGuidList[0].ReqIfModuleId))
            {
                // over all packages/guids
                int packageIndex = 0;
                foreach (var item in Settings.PackageGuidList)
                {
                    // get the column/taggedValueType prefix for current module
                    _prefixTv = Settings.GetPrefixTaggedValueType(packageIndex);
                    string pkgGuid = item.Guid;
                    string reqIfSpecId = item.ReqIfModuleId;
                    // ReqIF Specification ID found
                    if (!String.IsNullOrWhiteSpace(reqIfSpecId))
                    {
                        ReqIfFileItem reqIfFileItem = reqIfFileList.GetItemForReqIfId(reqIfSpecId);
                        // estimate package of guid list in settings 
                        Pkg = Rep.GetPackageByGuid(pkgGuid);
                        Rep.ShowInProjectView(Pkg);

                        bool result = RoundtripSpecification(reqIfFileItem.FilePath, reqIfFileItem.SpecContentIndex, reqIfFileItem.SpecIndex);
                        if (result == false || _errorMessage1) return false;
                    }
                    // next package
                    packageIndex += 1;
                }
            }
            // run all packages in sequential order
            else
            {
                // over all EA packages
                int packageIndex = 0;
                foreach (var file in importReqIfFiles)
                {
                    ReqIfFileItem reqIfFileItem = reqIfFileList.ReqIfFileItemList[packageIndex];
                    // get the column/taggedValueType prefix for current module
                    _prefixTv = Settings.GetPrefixTaggedValueType(packageIndex);
                    CountPackage += 1;

                    if (! CheckGuidAvailable(packageIndex, file, out var isContinueWorking)) return isContinueWorking;

                    // estimate package of guid list in settings 
                    string pkgGuid = Settings.PackageGuidList[packageIndex].Guid;
                    Pkg = Rep.GetPackageByGuid(pkgGuid);
                    Rep.ShowInProjectView(Pkg);

                    bool result = RoundtripSpecification(reqIfFileItem.FilePath, reqIfFileItem.SpecContentIndex, reqIfFileItem.SpecIndex);

                    if (importReqIfFiles.Length > 1) _subModuleIndex += 1;
                    if (result == false || _errorMessage1) return false;

                    packageIndex += 1;


                }
            }
            // write the changes back
            Compress(ImportModuleFile, Path.GetDirectoryName(importReqIfFiles[0]));
            return true;

        }

        /// <summary>
        /// Check ig GUID is available for current package index
        /// </summary>
        /// <param name="packageIndex"></param>
        /// <param name="file"></param>
        /// <param name="isContinueWorking">In case of error: True continue working, False break</param>
        /// <returns></returns>
        private bool CheckGuidAvailable(int packageIndex, string file, out bool isContinueWorking)
        {
// A Guid is available for the current index
            if (Settings.PackageGuidList.Count <= packageIndex)
            {
                var res = MessageBox.Show($@"File:{Tab}{file}

Index:{Tab}{packageIndex}
Count Guids:{Tab}{Settings.PackageGuidList.Count}
List of available GUIDs:
{String.Join("\r\n", Settings.PackageGuidList.Select(x => x.Guid))}

Yes:{Tab}Skip current file
Cancel{Tab}: Cancel whole import

", @"The GUID list in settings doesn't contain a GUID for current file, skip or cancel", MessageBoxButtons.OKCancel
                );
                {
                    // Error with or without continuation
                    isContinueWorking = (res == DialogResult.OK);
                    return false;
                }
            }

            isContinueWorking = true;
            return true;
        }

        /// <summary>
        /// Check import/roundtrip file
        /// </summary>
        /// <returns></returns>
        private bool CheckImportFile()
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


        /// <summary>
        /// RoundTrip a specification. This means it exports the roundtrip attributes.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="reqifContentIndex"></param>
        /// <param name="reqIfSpecIndex"></param>
        /// <returns></returns>
        private bool RoundtripSpecification(string file, int reqifContentIndex, int reqIfSpecIndex)
        {
            // Deserialize
            _reqIf = DeSerializeReqIf(file, validate: Settings.ValidateReqIF);
            if (_reqIf == null) return false;

            _moduleAttributeDefinitions = GetTypesModule(_reqIf, reqifContentIndex, reqIfSpecIndex);
           

            if (Pkg.Elements.Count == 0)
            {
                MessageBox.Show($@"File: '{file}'
Contains: {_reqIf.CoreContent.Count} modules

Roundtrip needs at least initial import and model elements in EA!

", @"No ReqIF initial import for an Roundtrip available, break!");
                return false;
            }

            // Export ReqIF SpecObjects stored in EA
            foreach (EA.Element el in Pkg.Elements)
            {
                _level = 0;
                if (!UpdateReqIfForElementRecursive(el)) return false;
            }

            // serialize ReqIF
            return SerializeReqIf(file, compress:false);
        }


        /// <summary>
        /// Recursive update an element
        /// </summary>
        /// <param name="el"></param>
        /// <returns></returns>
        public bool UpdateReqIfForElementRecursive(EA.Element el)
        {


            _count += 1;
            _countAll += 1;
            // Check type and stereotype
            if (el.Type != Settings.ObjectType || el.Stereotype != Settings.Stereotype) return true;
            if (!UpdateReqIfForElement(el)) return false;


            if (el.Elements.Count > 0)
            {
                _level += 1;
                foreach (EA.Element childEl in el.Elements)
                {
                    if (!UpdateReqIfForElementRecursive(childEl)) return false;
                }

                _level -= 1;
            }

            return true;

        }

        /// <summary>
        /// Deserialize ReqIF with or without XML validation. If error and no validation was selected ask if retrying with validation or cancel
        /// </summary>
        /// <param name="file"></param>
        /// <param name="validate"></param>
        /// <returns></returns>
        public static ReqIF DeSerializeReqIf(string file, bool validate=false)
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

{e}", @"Can't deserialize existing ReqIF file, retry with XML validation?",MessageBoxButtons.RetryCancel);
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
/// <returns></returns>
bool SerializeReqIf(string file, bool compress=true)
        {
            var serializer = new ReqIFSerializer(false);
            string prefix = Path.GetFileNameWithoutExtension(file).StartsWith("_") ? "" : "_";
            string pathSerialize = Path.Combine(Path.GetDirectoryName(file),
                $"{prefix}{Path.GetFileNameWithoutExtension(file)}.reqif");
            try
            {
                serializer.Serialize(_reqIf, pathSerialize, null);

               
            }
            catch (Exception e)
            {
                MessageBox.Show($@"Path:
'{pathSerialize}'

{e}", @"Error serialize ReqIF, break!");
                return false;
            }
            if (compress)
                Compress(ImportModuleFile, Path.GetDirectoryName(pathSerialize));
            return true;
        }

        /// <summary>
        /// UpdateReqIf for an element. Handle for TV: Values or Macros like '=EA.GUID' and updupting naming to Module prefix
        /// </summary>
        /// <param name="el"></param>
        /// <returns></returns>
        public bool UpdateReqIfForElement(EA.Element el)
        {
            string id = el.Multiplicity;  //TaggedValue.GetTaggedValue(el, GetPrefixedTagValueName("Id"));
            SpecObject specObj;
            try
            {
                specObj = _reqIf.CoreContent[0].SpecObjects.SingleOrDefault(x => x.Identifier == id);
            }
            catch (Exception e)
            {
                MessageBox.Show($@"File: '{ImportModuleFile}'
Module in ReqIF: '{_subModuleIndex}'

{e}", @"Error getting identifier from ReqIF");
                return false;
            }

            if (specObj == null)
            {
                MessageBox.Show($@"File: '{ImportModuleFile}'
Module in ReqIF: '{_subModuleIndex}'", @"Error getting identifier from ReqIF");
                return false;

            }

            return ExportUpdateRoundTripAttributes(el, specObj);
        }


        /// <summary>
        /// Update ReqIF RoundTrip Attributes from EA Tagged value for an Element. 
        /// </summary>
        /// <param name="el"></param>
        /// <param name="specObj"></param>
        /// <returns></returns>
        private bool ExportUpdateRoundTripAttributes(Element el, SpecObject specObj)
        {
            // update values of ReqIF Attributes by TaggedValues
            foreach (string tvName in _exportFields.GetFields())
            {
                string tvValue = TaggedValue.GetTaggedValue(el, GetPrefixedTagValueName(tvName), caseSensitive: false);


                // update value
                string macroValue = _exportFields.GetMacroValue(el, tvName);
                if (macroValue != "") tvValue = macroValue;
                if (tvValue == "") continue;
                if (!ChangeValueReqIf(specObj, GetUnPrefixedTagValueName(tvName), tvValue, caseSensitive: false))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Change ReqIF value of the specObject and the attribute value
        /// </summary>
        /// <param name="specObject"></param>
        /// <param name="name"></param>
        /// <param name="eaValue"></param>
        /// <param name="caseSensitive"></param>
        /// <returns></returns>
        private bool ChangeValueReqIf(SpecObject specObject, string name, string eaValue, bool caseSensitive = false)
        {
            try
            {
                AttributeValue attrValueObject = caseSensitive
                    ? specObject.Values.SingleOrDefault(x => x.AttributeDefinition.LongName == name)
                    : specObject.Values.SingleOrDefault(x =>
                        x.AttributeDefinition.LongName.ToLower() == name.ToLower());
                bool multiValuedEnum = false;
                // Attribute not part of ReqIF, skip
                if (attrValueObject == null)
                {
                    // Create AttributValue and assign them to values.
                    AttributeDefinition attributeType =
                        _moduleAttributeDefinitions.SingleOrDefault(x => x.LongName.ToLower() == name.ToLower());
                    switch (attributeType)
                    {
                        case AttributeDefinitionString _:
                            attrValueObject = new AttributeValueString
                            {
                                AttributeDefinition = attributeType
                            };
                            break;
                        case AttributeDefinitionXHTML _:
                            attrValueObject = new AttributeValueXHTML
                            {
                                AttributeDefinition = attributeType
                            };
                            break;
                        case AttributeDefinitionEnumeration moduleAttributDefinitionEnumeration:
                            attrValueObject = new AttributeValueEnumeration
                            {
                                AttributeDefinition = attributeType
                            };
                            multiValuedEnum = moduleAttributDefinitionEnumeration.IsMultiValued;

                            break;

                    }

                    if (attrValueObject == null) return true; // not supported datatype
                    specObject.Values.Add(attrValueObject);
                }

                var attrType = attrValueObject.AttributeDefinition; //specObj.Values[0].AttributeDefinition.LongName;
                switch (attrType)
                {
                    case AttributeDefinitionXHTML _:
                        // make xhtml and handle new line
                        var xhtmlcontent = MakeXhtmlFromString(eaValue);
                        attrValueObject.ObjectValue = xhtmlcontent;
                        break;

                    case AttributeDefinitionString _:
                        attrValueObject.ObjectValue = eaValue;
                        break;
                    case AttributeDefinitionEnumeration _:

                        try
                        {
                            // take all the valid enums
                            if (!SetReqIfEnumValue((AttributeValueEnumeration) attrValueObject, eaValue,
                                multiValuedEnum)) return false;
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show($@"Name: '{name}'

Value: '{eaValue}'

{e}", $@"Error enumeration value TV '{name}'.");
                        }

                        break;
                }

                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show($@"Name: '{name}'

Value: '{eaValue}'

{e}", $@"Error value TV '{name}'.");
                return false;
            }
        }

        /// <summary>
        /// Add XHTML Namespace to string
        /// </summary>
        /// <param name="stringText"></param>
        /// <returns></returns>
        private static string AddXtmlNameSpace(string stringText)
        {
            string xhtmlContent =
                $@"<{NameSpace}:div xmlns:{NameSpace}=""http://www.w3.org/1999/xhtml"">{MakeNameSpace(stringText)}</{NameSpace}:div>";
            return xhtmlContent;
        }
        /// <summary>
        /// Make XHTML from a simple string. It inserts the xhtml namespace and handles cr/lf
        /// </summary>
        /// <param name="stringValue"></param>
        /// <returns></returns>
        private static string MakeXhtmlFromString(string stringValue)
        {
            if (String.IsNullOrWhiteSpace(stringValue)) stringValue = "";

            stringValue = stringValue.Replace("\r\n", "<br/>");
            stringValue = stringValue.Replace("&nbsp;", "");
            stringValue = Regex.Replace(stringValue, @">\s*<", "><");  // Replace Blanks between control sequences
            stringValue = LimitReqIfXhtml(stringValue);
            return AddXtmlNameSpace(stringValue);
        }
        /// <summary>
        /// Make XHTML from a html string. It inserts the xhtml namespace
        /// </summary>
        /// <param name="htmlValue"></param>
        /// <returns></returns>
        private static string MakeXhtmlFromHtml(string htmlValue)
        {
            //stringValue = stringValue.Replace("\r\n", "<br></br>");
            htmlValue = htmlValue.Replace("&nbsp;", "");
            //htmlValue = Regex.Replace(htmlValue, @">\s*<", "><");  // Replace Blanks between control sequences
            htmlValue = LimitReqIfXhtml(htmlValue);
            return AddXtmlNameSpace(htmlValue);
        }
        /// <summary>
        /// Limit to ReqIF XHTML tags
        /// </summary>
        /// <param name="stringText"></param>
        /// <returns></returns>
        private static string LimitReqIfXhtml(string stringText)
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
        /// Get string with ReqIF Header Information 
        /// </summary>
        /// <returns></returns>
        protected string GetHeaderInfo(ReqIF reqIf=null)
        {
            reqIf = reqIf ?? _reqIf;
            if (reqIf == null) return "no ReqIF found, possible SW error";
            if (reqIf.TheHeader == null) return "no ReqIF header found";
            if (reqIf.TheHeader.Count == 0) return "ReqIF header count = 0";
            return $@"Title:{Tab}{Tab}'{reqIf.TheHeader[0].Title}'
ReqIFVersion:{Tab}'{reqIf.TheHeader[0].ReqIFVersion}'
ReqIFToolId:{Tab}'{reqIf.TheHeader[0].ReqIFToolId}'
RepositoryId:{Tab}'{reqIf.TheHeader[0].RepositoryId}'
SourceToolId:{Tab}'{reqIf.TheHeader[0].SourceToolId}'
CreationTime:{Tab}'{reqIf.TheHeader[0].CreationTime}'
Identifier:{Tab}'{reqIf.TheHeader[0].Identifier}'
Comment:{Tab}'{reqIf.TheHeader[0].Comment}'
";
        }

        /// <summary>
        /// Make XHTML from a EA notes. It inserts the xhtml namespace and handles special characters
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="stringText"></param>
        /// <returns></returns>
        private static string MakeXhtmlFromEaNotes(EA.Repository rep, string stringText)
        {
            string stringHtml = rep.GetFormatFromField("HTML", stringText);
            return MakeXhtmlFromHtml(stringHtml);
        }
        /// <summary>
        /// Make ReqIF XHTML from a XHTML. In essence add XHTML namespace
        /// </summary>
        /// <param name="stringText"></param>
        /// <returns></returns>
        private static string MakeReqIfXhtmlFromXhtml(string stringText)
        {
            stringText = LimitReqIfXhtml(stringText);
            return AddXtmlNameSpace(stringText);

        }

        /// <summary>
        /// Make xhtml namespace and correct some peculiar things (not supported by xhtml for reqIF)
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static string MakeNameSpace(string text)
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
        /// Set the values of a ReqIF enum (single value or multi value) in the ReqIF (AttributeDefinition)
        /// </summary>
        /// <param name="attributeValueEnumeration"></param>
        /// <param name="value"></param>
        /// <param name="isMultiValueEnum"></param>
        public bool SetReqIfEnumValue(AttributeValueEnumeration attributeValueEnumeration, string value,
            bool isMultiValueEnum)
        {
           

            // delete old values
            attributeValueEnumeration.Values.Clear();
            
            if (!isMultiValueEnum)
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
                    if (values.Length >  index  && values[index] == "1")
                    {
                        attributeValueEnumeration.Values.Add(enumValue);
                    }
                    index += 1;

                }
            }

            return true;

        }


        /// <summary>
        /// Import and update ReqIF Requirements in EA from ReqIF (compressed file). Derive Tagged Values from ReqSpec Attribute definition
        /// </summary>
        /// <param name="eaObjectType">EA Object type to create</param>
        /// <param name="eaStereotype">EA stereotype to create</param>
        /// <param name="stateNew">The EA state if the EA element is created</param>
        public override bool ImportForFile(string eaObjectType = "Requirement",
            string eaStereotype = "",
            string stateNew = "")
        {
            CountPackage = 0;
            bool result = true;
            _errorMessage1 = false;
            // handle Export fields
            _exportFields = new ExportFields(Settings.WriteAttrNameList);

            _subModuleIndex = 0;
           

            // decompress reqifz file and its embedded files
            string[] importReqIfFiles = Decompress(ImportModuleFile);
            if (importReqIfFiles.Length == 0) return false;

            // Inventory all reqIf files with their specifications
            ReqIfFileList reqIfFileList = new ReqIfFileList(importReqIfFiles);
            if (reqIfFileList.ReqIfFileItemList.Count == 0) return false;

            // Check import settings
            if (!CheckImportFile()) return false;


            // over all to import Packages/Specification/Modules of the current *.reqifz file
            // Import all Specifications of file
            // Import by Specification ID in item definition/file definition (more reliable)
            if (! String.IsNullOrWhiteSpace(Settings.PackageGuidList[0].ReqIfModuleId))
            {
                // over all packages/guids
                int packageIndex = 0;
                foreach (var item in Settings.PackageGuidList)
                {
                    if (!String.IsNullOrWhiteSpace(item.ReqIfReqIfFleStorage)) Settings.EmbeddedFileStorageDictionary = item.ReqIfReqIfFleStorage;
                    _prefixTv  = Settings.GetPrefixTaggedValueType(packageIndex);

                    string pkgGuid = item.Guid;
                    string reqIfSpecId = item.ReqIfModuleId;
                    // ReqIF Specification ID found
                    if (!String.IsNullOrWhiteSpace(reqIfSpecId))
                    {
                        ReqIfFileItem reqIfFileItem = reqIfFileList.GetItemForReqIfId(reqIfSpecId);
                        // estimate package of guid list in settings 
                        Pkg = Rep.GetPackageByGuid(pkgGuid);

                        ImportSpecification(reqIfFileItem.FilePath, eaObjectType, eaStereotype,
                            reqIfFileItem.SpecContentIndex,  reqIfFileItem.SpecIndex, 
                            stateNew);
                        if (result == false || _errorMessage1) return false;
                    }
                    // next package
                    packageIndex += 1;
                }
            }

            // Import all of current reqif file by sequence
            else
            {
                // over all EA packages
                int packageIndex = 0;
                foreach (var file in importReqIfFiles)
                {
                    ReqIfFileItem reqIfFileItem = reqIfFileList.ReqIfFileItemList[packageIndex];

                    // get the column/taggedValueType prefix for current module
                    _prefixTv = Settings.GetPrefixTaggedValueType(packageIndex);

                    CountPackage += 1;

                    // A Guid is available for the current index
                    if (! CheckGuidAvailable(packageIndex, file, out var isContinueWorking)) return isContinueWorking;
                   
                    // estimate package of guid list in settings 
                    string pkgGuid = Settings.PackageGuidList[packageIndex].Guid;
                    Pkg = Rep.GetPackageByGuid(pkgGuid);

                    ImportSpecification(file, eaObjectType, eaStereotype, reqIfFileItem.SpecContentIndex, reqIfFileItem.SpecIndex, stateNew);
                    if (importReqIfFiles.Length > 1) _subModuleIndex += 1;
                    if (result == false || _errorMessage1) return false;

                    packageIndex += 1;


                }
            }

            return result && (!_errorMessage1);
        }

       
        /// <summary>
        /// Import ReqIF Specification
        /// </summary>
        /// <param name="file"></param>
        /// <param name="eaObjectType"></param>
        /// <param name="eaStereotype"></param>
        /// <param name="reqIfContentIndex"></param>
        /// <param name="reqIfSpecIndex"></param>
        /// <param name="stateNew"></param>
        /// <returns></returns>
        private bool ImportSpecification(string file, string eaObjectType, string eaStereotype, int reqIfContentIndex, int reqIfSpecIndex, string stateNew)
        {
            CopyEmbeddedFilesToTarget(file);

            // Deserialize
            _reqIf = DeSerializeReqIf(file, validate: Settings.ValidateReqIF);
            if (_reqIf == null ) return MessageBox.Show("",@"Continue with next reqif file or cancel",MessageBoxButtons.OKCancel) == DialogResult.OK;
            
            _moduleAttributeDefinitions = GetTypesModule(_reqIf, reqIfContentIndex, reqIfSpecIndex);

            // prepare EA, existing requirements to detect deleted and changed requirements
            ReadEaPackageRequirements();
            CreateEaPackageDeletedObjects();


            // Add requirements recursive for module to requirement table
            InitializeReqIfRequirementsTable(_reqIf);
            Specification reqIfModule = _reqIf.CoreContent[reqIfContentIndex].Specifications[reqIfSpecIndex];

            Rep.BatchAppend = true;
            Rep.EnableUIUpdates = false;
            UpdatePackage();

            AddReqIfRequirementsToDataTable(DtRequirements, reqIfModule.Children, 1);

            // Check imported ReqIF requirements
            if (CheckImportedRequirements(file))
            {
                CreateUpdateDeleteEaRequirements(eaObjectType, eaStereotype, stateNew, "", file);

                MoveDeletedRequirements();
                

                // handle links
                ReqIfRelation relations = new ReqIfRelation(_reqIf, Rep, Settings);
            }

            Rep.BatchAppend = false;
            Rep.EnableUIUpdates = true;
            Rep.ReloadPackage(Pkg.PackageID);
            return true;
        }
        /// <summary>
        /// Copy and convert embedded files files to target directory, only if the first module in a zipped reqif-file
        /// </summary>
        /// <param name="file"></param>
        private void CopyEmbeddedFilesToTarget(string file)
        {
            // Copy and convert embedded files files to target directory, only if the first module in a zipped reqif-file
            if (Settings.EmbeddedFileStorageDictionary != "" && _subModuleIndex == 0)
            {
                string sourceDir = Path.GetDirectoryName(file);
                hoUtils.DirectoryExtension.CreateEmptyDir(Settings.EmbeddedFileStorageDictionary);
                hoUtils.DirectoryExtension.DirectoryCopy(sourceDir, Settings.EmbeddedFileStorageDictionary,
                    copySubDirs: true);
            }
        }

        /// <summary>
        /// Check imported requirements:
        /// - more than one requirements found
        /// - all needed columns are available
        /// </summary>
        /// <returns></returns>
        private bool CheckImportedRequirements(string file)
        {
            bool result = true;
            if (DtRequirements == null || DtRequirements.Rows.Count == 0)
            {
                MessageBox.Show($@"Can't find requirements in file: 

'{file}'

{GetHeaderInfo()}
", @"No requirements imported, break!");
                return false;
            }

            List<string> expectedColumns = new List<string>();
            expectedColumns.AddRange(Settings.AliasList);
            expectedColumns.AddRange(Settings.AttrNameList);
            expectedColumns.AddRange(Settings.RtfNameList);
            expectedColumns.Add(Settings.AttrNotes);

            foreach (var column in expectedColumns)
            {
                if (!DtRequirements.Columns.Contains(column))
                {
                    var columns = (from c in DtRequirements.Columns.Cast<DataColumn>()
                        orderby c.ColumnName
                        select c.ColumnName).ToArray();
                    MessageBox.Show($@"File: '{file}

Expected Attribute: '{column}'

Probable cause:
- Incorrect ReqIF file. Missing Attribute definitions

Available Attributes:
{string.Join(Environment.NewLine, columns)}",
                        @"ReqIF import doesn't contain Attribute!");
                    result = false;
                }
            }

            return result;


        }

        /// <summary>
        /// Create, update, delete requirements in EA Package from Requirement DataTable. 
        /// </summary>
        /// <param name="eaObjectType"></param>
        /// <param name="eaStereotype"></param>
        /// <param name="stateNew"></param>
        /// <param name="stateChanged"></param>
        /// <param name="importFile"></param>
        private void CreateUpdateDeleteEaRequirements(string eaObjectType, string eaStereotype, string stateNew,
            string stateChanged, string importFile)
        {
            Count = 0;
            CountChanged = 0;
            CountNew = 0;
            List<int> parentElementIdsPerLevel = new List<int> {0};
            int parentElementId = 0;
            int lastElementId = 0;

            int oldLevel = 0;
            string notesColumn = Settings.AttrNotes ?? "";
            foreach (DataRow row in DtRequirements.Rows)
            {
                Count += 1;
                string objectId = row["Id"].ToString();
                SpecObject specObject = (SpecObject) row["specObject"];


                int objectLevel = Int32.Parse(row["Object Level"].ToString()) - 1;

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

                CombineAttrValues(Settings.AliasList, row, out string alias, ShortNameLength, makeName: true);
                CombineAttrValues(Settings.AttrNameList, row, out string name, ShortNameLength, makeName: true);
                string notes = GetStringAttrValue(notesColumn != "" ? row[notesColumn].ToString() : row[1].ToString());


                // Check if requirement with Doors ID already exists
                bool isExistingRequirement = DictPackageRequirements.TryGetValue(objectId, out int elId);


                EA.Element el;
                if (isExistingRequirement)
                {
                    el = Rep.GetElementByID(elId);
                    if (el.Alias != alias ||
                        el.Name != name ||
                        el.Notes != notes)
                    {
                        if (stateChanged != "") el.Status = stateChanged;
                        CountChanged += 1;
                    }
                }
                else
                {
                    el = (EA.Element) Pkg.Elements.AddNew(name, "Requirement");
                    if (stateNew != "") el.Status = stateNew;
                    CountChanged += 1;
                }


                try
                {
                    el.Alias = alias;
                    el.Name = name;
                    el.Multiplicity = objectId;
                    el.Notes = notes;
                    el.TreePos = Count * 10;
                    el.PackageID = Pkg.PackageID;
                    el.ParentID = parentElementId;
                    el.Type = eaObjectType;
                    el.Stereotype = eaStereotype;

                    el.Update();
                    Pkg.Elements.Refresh();
                    lastElementId = el.ElementID;
                }
                catch (Exception e)
                {
                    if (MessageBox.Show($@"Name: '{name}'
Alias: '{alias}
ObjectId/Multiplicity: '{objectId}

{e}", @"Error update EA Element, skip!",
                            MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                        break;
                    else continue;
                }

                // handle the remaining columns/ tagged values
                var cols = from c in DtRequirements.Columns.Cast<DataColumn>()
                        join v in specObject.SpecType.SpecAttributes on c.ColumnName equals
                            v.LongName // specObject.Values on c.ColumnName equals v.AttributeDefinition.LongName
                        where !ColumnNamesNoTaggedValues.Any(n => n == c.ColumnName)
                        select new
                        {
                            Name = c.ColumnName,
                            Value = row[c].ToString(),
                            AttrDef = v
                        }
                    ;
                // Handle *.rtf/*.docx content
                string rtfValue = CombineRtfAttrValues(Settings.RtfNameList, row);

                // Update EA linked documents by graphics and embedded elements
                UpdateLinkedDocument(el, rtfValue, importFile);

                // Update/Create Tagged value
                DeleteWritableTaggedValuesForElement(el);
                // over all columns
                foreach (var c in cols)
                {
                    // suppress column if already schown in notes
                    if (notesColumn != c.Name)
                    {
                        // Enum with multivalue
                        if (c.AttrDef is AttributeDefinitionEnumeration attrDefinitionEnumeration &&
                            attrDefinitionEnumeration.IsMultiValued)
                        {
                            // Enum values available
                            var arrayEnumValues = ((DatatypeDefinitionEnumeration) c.AttrDef.DatatypeDefinition)
                                .SpecifiedValues
                                .Select(x => x.LongName).ToArray();
                            Regex rx = new Regex(@"\r\n| ");
                            var values = rx.Replace(c.Value, ",").Split(',');
                            var found = from all in arrayEnumValues
                                from s1 in values.Where(xxx => all == xxx).DefaultIfEmpty()
                                select new {All = all, Value = s1};
                            var value = "";
                            var del = "";
                            foreach (var f in found)
                            {
                                if (f.Value == null)
                                    value = $"{value}{del}0";
                                else
                                    value = $"{value}{del}1";
                                del = ",";
                            }
                            CreateUpdateTaggedValueDuringInput(el, c.Name, c.Value, c.AttrDef);
                        }
                        else
                        {
                            // handle roundtrip attributes
                            CreateUpdateTaggedValueDuringInput(el, c.Name, c.Value, c.AttrDef);
                        } 

                        
                    }
                }
            }
        }
        /// <summary>
        /// Delete all writable tagged values for Element
        /// </summary>
        /// <param name="el"></param>
        /// <returns></returns>
        public bool DeleteWritableTaggedValuesForElement(EA.Element el)
        {
            for (int i = el.TaggedValues.Count - 1; i >= 0; i--)
            {

                EA.TaggedValue tv = (EA.TaggedValue)el.TaggedValues.GetAt((short)i);
                if (! _exportFields.IsWritableValue(GetUnPrefixedTagValueName(tv.Name))) 
                    el.TaggedValues.DeleteAt((short)i, false);
            }
            el.TaggedValues.Refresh();
            el.Update();
            return true;

        }

        /// <summary>
        /// Create TaggedValue. If it is a TaggedValue to export: Only set value during creating, don't overwrite a value;
        /// If the field is a roundtrip (writable) attribute of type string/xhtml make a Memo field out of if.
        /// </summary>
        /// <param name="el"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="attrDefinition"></param>
        /// <returns></returns>
        private EA.TaggedValue CreateUpdateTaggedValueDuringInput(EA.Element el, string name, string value, AttributeDefinition attrDefinition)
        {
            EA.TaggedValue tv;
            string tvValue = value ?? "";
            string tvName = GetPrefixedTagValueName(name);

            

            // Values writable/roundtrip or macros to update
            if (_exportFields.IsWritableValue(name) )
            {
               if ( TaggedValue.Exists(el, tvName)) 
                   // only get value of existing one
                    tv = TaggedValue.CreateTaggedValue(el,tvName);
               else
               {
                   if (_exportFields.GetMacroName(name) != "") tvValue = _exportFields.GetMacroValue(el, name);
                   tv = TaggedValue.SetUpdate(el, tvName, GetStringAttrValue(tvValue));
               }
            }
            else tv = TaggedValue.SetUpdate(el, tvName, GetStringAttrValue(tvValue));

            // Make memo field for roundtrip attributes of type string/ xhtml
            if (_exportFields.IsWritableValue(name) && _exportFields.GetMacroName(name) == "" &&
                (attrDefinition is AttributeDefinitionXHTML || attrDefinition is AttributeDefinitionString))
            {
                TaggedValue.MakeMemo(el, tvName);

            }
            return tv;

        }


    /// <summary>
    /// Get prefixed Tagged Value name from name without prefix
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private string GetPrefixedTagValueName(string name)
        {
            if (name.StartsWith(_prefixTv)) return name;
            return $"{_prefixTv}{name}";
        }
        /// <summary>
        /// Get prefixed Tagged Value name from name without prefix
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private string GetUnPrefixedTagValueName(string name)
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
        void Compress(string zipFile, string dirNameReqIfFiles, string dirNameFiles="")
        {
            
            if (File.Exists(zipFile)) File.Delete(zipFile);
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
        }

        /// <summary>
        /// Decompress the import/export reqIf file if compressed format (*.reqifz). It returns an array of *.reqIF files.
        /// </summary>
        /// <param name="importReqIfFile"></param>
        /// <returns>The path to the *.reqif file</returns>
        private string[] Decompress(string importReqIfFile)
        {
            // *.reqifz for compressed ReqIf File
            if (importReqIfFile.ToUpper().EndsWith("Z"))
            {
                string extractDirectory = hoUtils.Compression.Zip.ExtractZip(importReqIfFile);
                if (String.IsNullOrWhiteSpace(extractDirectory)) return new string[0];

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
        /// Update Package with Standard and ReqIF Properties
        /// </summary>
        protected override void UpdatePackage()
        {
            EA.Element el = Rep.GetElementByGuid(Pkg.PackageGUID);
            GetModuleProperties(_reqIf, el);

            base.UpdatePackage();
        }

        /// <summary>
        /// Update EA linked document with xhtmlValue. It handles graphics and embedded files
        /// </summary>
        /// <param name="el">EA Element to update linked document</param>
        /// <param name="xhtmlValue">The XHTML value in XHTML or flat string format</param>
        /// <param name="importFile">If "" or null use the Class settings ImportModuleFile from constructor</param>
        /// <returns></returns>
        protected bool UpdateLinkedDocument(EA.Element el, string xhtmlValue, string importFile="")
        {
            if (String.IsNullOrWhiteSpace(importFile)) importFile = ImportModuleFile;
            // Handle *.rtf content
            string docFile = $"{System.IO.Path.GetDirectoryName(importFile)}";

            // store embedded files
            if (Settings.EmbeddedFileStorageDictionary != "" && xhtmlValue.Contains("object data="))
            {
                List<string> embeddedFiles = HtmlToDocx.GetEmbeddedFiles(xhtmlValue);

                // delete all existing files
                for (int i = el.Files.Count-1; i > -1; i--)
                {
                    el.Files.Delete((short)i);
                }
                foreach (var file in embeddedFiles)
                {
                    string f = Path.Combine(Settings.EmbeddedFileStorageDictionary, file);

                    // make an ole object of *.ole files
                    string filePathNew = "";
                    if (Settings.ImportType == FileImportSettingsItem.ImportTypes.DoorsReqIf)
                    {
                        filePathNew = OleDoors.Save(HoUtil.ReadAllText(f), f, ignoreNotSupportedFiles:Settings.Verbosity == FileImportSettingsItem.VerbosityType.Silent);
                    }

                    if (filePathNew != "")
                    {
                        EA.File eaFile = (EA.File) el.Files.AddNew(filePathNew, "");
                        el.Files.Refresh();
                        eaFile.Type = "Local File";
                        eaFile.Notes = $@"*{Path.GetExtension(filePathNew)}
Name:{Tab}'{Path.GetFileName(filePathNew)}'
Path:{Tab}'{Path.GetDirectoryName(filePathNew)}'
Origin:{Tab}'{Path.GetFileName(f)}'";

                        eaFile.Update();
                        el.Update();
                    }

                }
            }


            // generate docx or rtf
            bool IsGenerateDocx = false;
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            docFile = System.IO.Path.Combine(docFile, IsGenerateDocx ? "xxxxxxx.docx" : "xxxxxxx.rtf");


            // decide what converter to use
            // Mari.Gold.OpenXHTML (open source)
            // SautinSoft.HtmlToRtf (commercial)
            if (Settings.UseMariGold)
                HtmlToDocx.Convert(docFile, xhtmlValue);
            else HtmlToDocx.ConvertSautin(docFile, xhtmlValue);
            try
            {
                bool res = el.LoadLinkedDocument(docFile);
                if (!res)
                    MessageBox.Show($@"ImportFile: '{importFile}'

EA GUID           {Tab}: '{el.ElementGUID}'
EA Multiplicity   {Tab}: '{el.Multiplicity}'
Name:             {Tab}: '{el.Name}'
EaLastError       {Tab}: '{el.GetLastError()}'
RtfDocxFile:      {Tab}: '{docFile}'

XHTML:'{xhtmlValue}",
                        @"Error loading Linked Document, break current requirement, continue!");
            }
            catch (Exception e)
            {
                MessageBox.Show($@"ImportFile: '{importFile}'
Id: '{el.Multiplicity}'
Name: '{el.Name}'
RtfDocxFile:'{docFile}'

XHTML:'{xhtmlValue}

{e}",
                    @"Error loading Linked Document, break current requirement, continue");
                return false;
            }

            return true;

        }

        /// <summary>
        /// Initialize ReqIF Requirement DataTable with Columns (standard columns + one for each attribute). It also adds Tagged Value Types
        /// </summary>
        /// <param name="reqIf"></param>
        private bool InitializeReqIfRequirementsTable(ReqIF reqIf)
        {
            // Initialize table
            // Standard columns
            var standardAttributes = new string[] { "Id", "Object Level" };
            DtRequirements = new DataTable();
            foreach (var attr in standardAttributes)
            {
                DtRequirements.Columns.Add(attr, typeof(string));
            }

            // get list of all used attributes
            var attributeDefinitions = (from attributeDefinition in _moduleAttributeDefinitions
                where (!_blackList1.Any(bl => bl == attributeDefinition.LongName)) && // ignore blacklist, DOORS table attributes
                      (!standardAttributes.Any(bl => bl == attributeDefinition.LongName)) // ignore standard attributes
                          select attributeDefinition);

            // Add columns for all Attributes, except DOORS table attributes and standard attributes
            DtRequirements.Columns.Add("specObject", typeof(SpecObject));
            foreach (var attr in attributeDefinitions)
            {
                // add Column if not exists
                if (DtRequirements.Columns.Contains(attr.LongName)) continue;
                DtRequirements.Columns.Add(attr.LongName, typeof(string));

                // add TaggedValueType to EA if not already exists
                switch (attr)
                {
                    // handle enum with or without multiple value
                    case AttributeDefinitionEnumeration attrEnumType:
                        string tvName = GetPrefixedTagValueName(attr.LongName);
                        if (TaggedValue.TaggedValueTyeExists(Rep, tvName))
                        {
                            TaggedValue.DeleteTaggedValueTye(Rep, tvName);
                        }

                        // get enumeration value
                            var arrayEnumValues = ((DatatypeDefinitionEnumeration) attrEnumType.DatatypeDefinition)
                                .SpecifiedValues
                                .Select(x => x.LongName).ToArray();
                        // Enums as comma separated list
                        var enumValue = String.Join(",", arrayEnumValues);
                        // Multivalue enumeration
                        string typeTv = attrEnumType.IsMultiValued ? "CheckList" : "Enum";
                        TaggedValue.CreateTaggedValueType(Rep, tvName,
                            $@"Type={typeTv};{Environment.NewLine}Values={enumValue};",
                            "hoReverse:ReqIF automated generated!");
                       break;
                                
                }

            }

            return true;
        }


        /// <summary>
        /// Add requirements from ReqIF recursive to datatable. One Row = one Requirement with the row-attributes as columns.
        /// It creates columns also for attributes without values.
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="children"></param>
        /// <param name="level"></param>
        private bool AddReqIfRequirementsToDataTable(DataTable dt, List<SpecHierarchy> children, int level)
        {
            // no children available
            if (children == null || children.Count == 0) return false;

            // go over hierarchy
            foreach (SpecHierarchy child in children)
            {
                DataRow row = dt.NewRow();
                SpecObject specObject = child.Object;
                row["specObject"] = specObject;

                // Limit Identifier length to 50 and output one error message if length exceeds length of 50
                if (!_errorMessage1 && specObject.Identifier.Length > 50)
                {
                    MessageBox.Show($@"Identifier:
'{specObject.Identifier}'

Length: {specObject.Identifier.Length}

Can't correctly identify objects. Identifier cut to 50 characters!", @"ReqIF Identifier has length > 50");
                    _errorMessage1 = true;
                }
                row["Id"] = specObject.Identifier.Length > 50 ? specObject.Identifier.Substring(0, 50) : specObject.Identifier;

                row["Object Level"] = level;


                // Add columns of current row to datatable
                AddColumnsToDataTable(specObject, row);

                dt.Rows.Add(row);
                // handle the sub requirements
                AddReqIfRequirementsToDataTable(dt, child.Children, level + 1);
            }

            return true;


        }
        /// <summary>
        /// Add columns of current row to datatable (import). 
        /// </summary>
        /// <param name="specObject"></param>
        /// <param name="row"></param>
        private void AddColumnsToDataTable(SpecObject specObject, DataRow row)
        {
            // over all columns 
            // - Definitions of all attributes
            // - Values of attributes with values
            var allColumnDefinitions = specObject.SpecType.SpecAttributes;
            var columnsWithValue = specObject.Values;
            var columns = from all in allColumnDefinitions
                from v in columnsWithValue.Where(x => all.LongName == x.AttributeDefinition.LongName).DefaultIfEmpty()
                select new {Definition = all, Value = v};
            foreach (var column in columns)
            {
                // Handle blacklist
                if (_blackList1.Contains(column.Definition.LongName)) continue;

                // column value doesn't exists
                if (column.Value == null)
                {
                    row[column.Definition.LongName] = "";
                }
                else
                {
                    // column value exists
                    try
                    {
                        

                        // Handle enums
                        if (column.Definition is AttributeDefinitionEnumeration)
                        {
                            List<EnumValue> enumValues = (List<EnumValue>) column.Value.ObjectValue;
                            string values = "";
                            string del = "";
                            foreach (var enumValue in enumValues)
                            {
                                values = $"{values}{del}{enumValue.LongName}";
                                del = Environment.NewLine;
                            }

                            row[column.Definition.LongName] = values;
                        }
                        else row[column.Definition.LongName] = column.Value.ObjectValue.ToString();
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(
                            $@"AttrName: '{column.Definition.LongName}'{Environment.NewLine}{Environment.NewLine}{e}",
                            @"Exception add ReqIF Attribute");
                    }
                }
            }
        }

        /// <summary>
        /// Get types of a ReqIF module
        /// </summary>
        /// <param name="reqIf"></param>
        /// <param name="reqifContentIndex"></param>
        /// <param name="reqIfSpecIndex"></param>
        /// <returns></returns>
        List<ReqIFSharp.AttributeDefinition> GetTypesModule(ReqIF reqIf, int reqifContentIndex, int reqIfSpecIndex)
        {
            var specObjectTypes = new List<ReqIFSharp.AttributeDefinition>();
            var children = reqIf.CoreContent[reqifContentIndex].Specifications[reqIfSpecIndex].Children;
            foreach (SpecHierarchy child in children)
            {
                AddModuleAttributeTypes(specObjectTypes, child.Children);
            }
            return specObjectTypes.Select(x=>x).Distinct().ToList();
        }
        // Get all specObjectTypes of
        //void AddModuleAttributeTypes(List<ReqIFSharp.SpecObjectType> specObjectTypes, List<SpecHierarchy> children) {
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
        /// Get string Attribute Value. It converts xhtml and if makeName = true it removes multiple white spaces and control sequences from string
        /// </summary>
        /// <param name="attrValue"></param>
        /// <param name="makeName">True: Remove multiple whitespaces, control characters</param>
        /// <returns></returns>
        private string GetStringAttrValue(string attrValue, bool makeName=false)
        {
            // convert xhtml or use the origianl text
            var text = attrValue.Contains("http://www.w3.org/1999/xhtml") ? HtmlToText.ConvertReqIfXhtml(attrValue) : attrValue;
            return makeName ? MakeNameFromString(text) : text;
        }

        /// <summary>
        /// Get Attribute value for a list of attribute names. Limit the output length if length > 0
        /// </summary>
        /// <param name="lNames"></param>
        /// <param name="row"></param>
        /// <param name="value">The combined value</param>
        /// <param name="length">The leghth of the output string. Default:40</param>
        /// <param name="makeName"></param>
        /// <returns>false for error</returns>
        private bool CombineAttrValues(List<string> lNames, DataRow row, out string value, int length=0, bool makeName=false)
        {
            value = lNames.Count == 0 ? GetStringAttrValue(row[0].ToString()):"";
            string delimeter = "";
            foreach (var columnName in lNames)
            {

                try
                {
                    value = $"{value}{delimeter}{GetStringAttrValue(row[columnName].ToString())}";
                }
                catch (Exception e)
                {
                    MessageBox.Show($@"Attribute name:
{columnName}

{e}", @"Can't read Attribute!");
                    return false;

                }

                delimeter = " ";
            }
            // make name
            value = makeName ? MakeNameFromString(value, length) : value;
            // linit length
            value = length > 0 && value.Length > length 
                ? value.Substring(0, length) 
                : value;
            return true;

        }

        /// <summary>
        /// Combine rtf Attribute values for a list of attribute names.
        /// </summary>
        /// <param name="lNames"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        private string CombineRtfAttrValues(List<string> lNames, DataRow row)
        {
            string attrRtfValue = lNames.Count == 0 ? GetStringAttrValue(row[0].ToString()):"";
            string delimeter = "";
            foreach (var columnName in lNames)
            {

                try
                {
                    attrRtfValue = $"{attrRtfValue}{delimeter}{row[columnName]}";
                }
                catch (Exception e)
                {
                    MessageBox.Show($@"Attribute name:
{columnName}

{e}", @"Can't read Attribute!");
                }

                delimeter = @"<p><br><br><br></p>";
            }
            // linit length
            return attrRtfValue;

        }



        /// <summary>
        /// Move deleted EA requirements to Package 'DeletedRequirements'
        /// </summary>
        public override void MoveDeletedRequirements()
        {
            var moveEaElements = from m in DictPackageRequirements
                where !DtRequirements.Rows.Cast<DataRow>().Any(x => x["Id"].ToString() == m.Key)
                select m.Value;
            foreach (var eaId in moveEaElements)
            {
                EA.Element el = Rep.GetElementByID(eaId);
                el.PackageID = PkgDeletedObjects.PackageID;
                el.ParentID = 0;
                el.Update();
            }
        }
    }
}
