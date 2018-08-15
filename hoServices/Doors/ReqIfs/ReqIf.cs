using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using DataModels;
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

        // Prefix Tagged Values and Columnnames
        private string _prefixTv = "";

        ExportFields _exportFields;
        List<ReqIFSharp.AttributeDefinition> _moduleAttributeDefinitions;

        readonly FileImportSettingsItem _settings;

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
            _settings = settings;
        }

        /// <summary>
        /// Roundtrip / updated Requirements according to TaggedValues/AttributeNames in _settings.WriteAttrNameList
        /// </summary>
        /// <param name="subModuleIndex"></param>
        /// <returns></returns>
        public override bool RoundtripUpdateRequirements(int subModuleIndex = 0)
        {
            CountPackage = 0;
            _subModuleIndex = subModuleIndex;
            // Calculate the column/taggedValueType prefix for current module
            _prefixTv = _settings.PrefixTaggedValueTypeList.Count > _subModuleIndex
                ? _settings.PrefixTaggedValueTypeList[subModuleIndex]
                : "";

            _errorMessage1 = false;
            _exportFields = new ExportFields(_settings.WriteAttrNameList);
            // decompress reqif file and its embedded files
            string[] importReqIfFiles = Decompress(ImportModuleFile);
            if (importReqIfFiles.Length == 0) return false;

            // over all import files
            foreach (var file in importReqIfFiles)
            {
                CountPackage += 1;
                // estimate package
                string pkgGuid = _settings.PackageGuidList[_subModuleIndex];
                Pkg = Rep.GetPackageByGuid(pkgGuid);

                bool result = RoundtripFile(file, subModuleIndex);
                if (importReqIfFiles.Length > 1) _subModuleIndex += 1;
                if (result == false || _errorMessage1) return false;
            }
            Compress(ImportModuleFile, Path.GetDirectoryName(importReqIfFiles[0]));
            return true;

        }

        private bool RoundtripFile(string file, int subModuleIndex)
        {
// Deserialize
            ReqIFDeserializer deserializer = new ReqIFDeserializer();
            _reqIf = deserializer.Deserialize(file);
            _moduleAttributeDefinitions = GetTypesModule(_reqIf, subModuleIndex);
            // Modules
            if (subModuleIndex >= _reqIf.CoreContent[0].Specifications.Count)
            {
                MessageBox.Show($@"File: '{file}'
Contains: {_reqIf.CoreContent.Count} modules
Requested: {_reqIf.CoreContent.Count}
Packages (per module one package/guid) are defined in Settings.json: 

", @"More packages defined as Modules are in ReqIF file, break!");
                return false;
            }

            // Modules
            if (_reqIf.CoreContent[0].Specifications.Count == 0)
            {
                MessageBox.Show($@"File: '{file}'
Contains: {_reqIf.CoreContent.Count} modules
Requested: {_reqIf.CoreContent.Count}
No module is defined in Settings.json: 

", @"No module defined in ReqIF file, break!");
                return false;
            }

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
            if (el.Type != _settings.ObjectType || el.Stereotype != _settings.Stereotype) return true;
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
        /// Serialize ReqIF
        /// </summary>
        /// <param name="file"></param>
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

            //var specType = (SpecObjectType)reqIfContent.SpecTypes.SingleOrDefault(x => x.GetType() == typeof(SpecObjectType));
            return true;
        }

        /// <summary>
        /// Change ReqIF value of the specObject and the attribut value
        /// </summary>
        /// <param name="specObject"></param>
        /// <param name="name"></param>
        /// <param name="eaValue"></param>
        /// <param name="caseSensitive"></param>
        /// <returns></returns>
        private bool ChangeValueReqIf(SpecObject specObject, string name, string eaValue, bool caseSensitive = false)
        {
            AttributeValue attrValueObject = caseSensitive
                ? specObject.Values.SingleOrDefault(x => x.AttributeDefinition.LongName == name)
                : specObject.Values.SingleOrDefault(x => x.AttributeDefinition.LongName.ToLower() == name.ToLower());
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
                        SetReqIfEnumValue((AttributeValueEnumeration) attrValueObject, eaValue, multiValuedEnum);
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
        /// <summary>
        /// Make XHTML from a simple string. It inserts the xhtml namespace and handles cr/lf
        /// </summary>
        /// <param name="stringValue"></param>
        /// <returns></returns>
        private static string MakeXhtmlFromString(string stringValue)
        {
            stringValue = stringValue.Replace("\r\n", "<br></br>");
            stringValue = stringValue.Replace("&nbsp;", "");
            stringValue = Regex.Replace(stringValue, @">\s*<","><");  // Replace Blanks between control sequences


            string xhtmlcontent =
                $@"<reqif-xhtml:div xmlns:reqif-xhtml=""http://www.w3.org/1999/xhtml"">{stringValue}</reqif-xhtml:div>";
            return xhtmlcontent;
        }

        /// <summary>
        /// Make XHTML from a string. It inserts the xhtml namespace and handles cr/lf
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="stringText"></param>
        /// <returns></returns>
        private static string MakeXhtmlFromString(EA.Repository rep, string stringText)
        {
            string stringHtml = rep.GetFormatFromField("HTML", stringText);
            stringHtml = stringHtml.Replace("\r\n", "");
            stringHtml = stringHtml.Replace("&nbsp;", "");
            stringHtml = Regex.Replace(stringHtml, @">\s*<", "><");  // Replace Blanks between control sequences


            string xhtmlcontent =
                $@"<reqif-xhtml:div xmlns:reqif-xhtml=""http://www.w3.org/1999/xhtml"">{stringHtml}</reqif-xhtml:div>";
            return xhtmlcontent;
        }

        /// <summary>
        /// Set the values of a ReqIF enum (single value or multi value) in the ReqIF (Attributedefinition)
        /// </summary>
        /// <param name="attributeValueEnumeration"></param>
        /// <param name="value"></param>
        /// <param name="multiValueEnum"></param>
        public void SetReqIfEnumValue(AttributeValueEnumeration attributeValueEnumeration, string value,
            bool multiValueEnum)
        {
            // over all values split by ",:=;-"
            if (String.IsNullOrWhiteSpace(value)) return;

            // delete old values
            attributeValueEnumeration.Values.Clear();
            
            if (!multiValueEnum)
            {
                var enumValue = attributeValueEnumeration.Definition.Type.SpecifiedValues
                    .SingleOrDefault(x => x.LongName == value);
                attributeValueEnumeration.Values.Add(enumValue);
            }
            else
            {   // all enums (multi value enum)
                var enumValues = attributeValueEnumeration.Definition.Type.SpecifiedValues
                    .Select(x => x);
                var values = Regex.Replace(value.Trim(), @"\r\n?|\n|;|,|:|-|=", ",").Split(',');
                int index = 0;
                foreach (var enumValue in enumValues)
                {
                    if (values.Length >  index  && values[index] == "1")
                    {
                        attributeValueEnumeration.Values.Add(enumValue);
                    }
                    index += 1;

                }
            }

        }
       


        /// <summary>
        /// Import and update ReqIF Requirements in EA from ReqIF. Derive Tagged Values from ReqSpec Attribut definition
        /// </summary>
        /// <param name="eaObjectType">EA Object type to create</param>
        /// <param name="eaStereotype">EA stereotype to create</param>
        /// <param name="subModuleIndex">ReqIF can handle multiple submodules. This you can define in settings by the list of package GUIDs to import</param>
        /// <param name="stateNew">The EA state if the EA element is created</param>
        /// <param name="stateChanged">The EA state if the EA element is changed, currently not used</param>
        public override bool ImportUpdateRequirements(string eaObjectType = "Requirement",
            string eaStereotype = "",
            int subModuleIndex = 0,
            string stateNew = "",
            string stateChanged = "")
        {
            CountPackage = 0;
            bool result = true;
            _errorMessage1 = false;
            // handle Export fields
            _exportFields = new ExportFields(_settings.WriteAttrNameList);

            _subModuleIndex = subModuleIndex;
            // Calculate the column/taggedValueType prefix for current module
            _prefixTv = _settings.PrefixTaggedValueTypeList.Count > _subModuleIndex
                ? _settings.PrefixTaggedValueTypeList[subModuleIndex]
                : "";
            // Create Tagged Value Types

            // decompress reqif file and its embedded files
            string[] importReqIfFiles = Decompress(ImportModuleFile);
            if (importReqIfFiles.Length == 0) return false;

            // over all import files
            foreach (var file in importReqIfFiles)
            {
                CountPackage += 1;
                // estimate package
                string pkgGuid = _settings.PackageGuidList[_subModuleIndex];
                Pkg = Rep.GetPackageByGuid(pkgGuid);

                ImportReqifFile(file, eaObjectType, eaStereotype, subModuleIndex, stateNew, stateChanged);
                if (importReqIfFiles.Length > 1) _subModuleIndex += 1;
                if (result == false || _errorMessage1) return false;

            }
            return result && (!_errorMessage1);
        }

        private void ImportReqifFile(string file, string eaObjectType, string eaStereotype, int subModuleIndex, string stateNew,
            string stateChanged)
        {
// Copy and convert embedded files files to target directory, only if the first module in a zipped reqif-file
            if (_settings.EmbeddedFileStorageDictionary != "" && _subModuleIndex == 0)
            {
                string sourceDir = Path.GetDirectoryName(file);
                hoUtils.DirectoryExtension.CreateEmptyFolder(_settings.EmbeddedFileStorageDictionary);
                hoUtils.DirectoryExtension.DirectoryCopy(sourceDir, _settings.EmbeddedFileStorageDictionary,
                    copySubDirs: true);
            }

            // Deserialize
            ReqIFDeserializer deserializer = new ReqIFDeserializer();
            _reqIf = deserializer.Deserialize(file);
            _moduleAttributeDefinitions = GetTypesModule(_reqIf, subModuleIndex);

            // prepare EA, existing requirements to detect deleted and changed requirements
            ReadEaPackageRequirements();
            CreateEaPackageDeletedObjects();


            // Add requirements recursiv for module to requirement table
            InitializeReqIfRequirementsTable(_reqIf);
            Specification reqIfModule = _reqIf.CoreContent[0].Specifications[subModuleIndex];

            Rep.BatchAppend = true;
            Rep.EnableUIUpdates = false;
            AddReqIfRequirementsToDataTable(DtRequirements, reqIfModule.Children, 1);

            // Check imported ReqIF requirements
            if (CheckImportedRequirements())
            {
                CreateUpdateDeleteEaRequirements(eaObjectType, eaStereotype, stateNew, stateChanged, file);

                MoveDeletedRequirements();
                UpdatePackage();

                // handle links
                ReqIfRelation relations = new ReqIfRelation(_reqIf, Rep, _settings);
            }

            Rep.BatchAppend = false;
            Rep.EnableUIUpdates = true;
            Rep.ReloadPackage(Pkg.PackageID);
        }

        /// <summary>
        /// Check imported requirements:
        /// - more than one requirements found
        /// - all needed columns are available
        /// </summary>
        /// <returns></returns>
        private bool CheckImportedRequirements()
        {
            bool result = true;
            if (DtRequirements == null || DtRequirements.Rows.Count == 0)
            {
                MessageBox.Show("", @"No requirements imported, break!");
                return false;
            }

            List<string> expectedColumns = new List<string>();
            expectedColumns.AddRange(_settings.AliasList);
            expectedColumns.AddRange(_settings.AttrNameList);
            expectedColumns.AddRange(_settings.RtfNameList);
            expectedColumns.Add(_settings.AttrNotes);

            foreach (var column in expectedColumns)
            {
                if (!DtRequirements.Columns.Contains(column))
                {
                    var columns = (from c in DtRequirements.Columns.Cast<DataColumn>()
                        orderby c.ColumnName
                        select c.ColumnName).ToArray();
                    MessageBox.Show($@"Expected Attribute: '{column}'

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
            string notesColumn = _settings.AttrNotes ?? "";
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

                CombineAttrValues(_settings.AliasList, row, out string alias, ShortNameLength, makeName: true);
                CombineAttrValues(_settings.AttrNameList, row, out string name, ShortNameLength, makeName: true);
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
                string rtfValue = CombineRtfAttrValues(_settings.RtfNameList, row);

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
        /// <param name="dirName">The directory to zip</param>
        void Compress(string zipFile, string dirName)
        {
            
            if (File.Exists(zipFile)) File.Delete(zipFile);
            using (var zip = ZipFile.Open(zipFile, ZipArchiveMode.Create))
            {
                foreach (var file in Directory.GetFiles(dirName))
                {
                   if (Path.GetFileName(file).ToLower().EndsWith("reqif"))
                            zip.CreateEntryFromFile(file, Path.GetFileName(file));
                }
            }
        }

        /// <summary>
        /// Decompress the import/export reqIf file if compressed format (*.reqifz)
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

                // extract reqif file from achive
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
            if (_settings.EmbeddedFileStorageDictionary != "" && xhtmlValue.Contains("object data="))
            {
                List<string> embeddedFiles = HtmlToDocx.GetEmbeddedFiles(xhtmlValue);

                // delete all existing files
                for (int i = el.Files.Count-1; i > -1; i--)
                {
                    el.Files.Delete((short)i);
                }
                foreach (var file in embeddedFiles)
                {
                    string f = Path.Combine(_settings.EmbeddedFileStorageDictionary, file);

                    // make an ole object of *.ole files
                    string filePathNew = "";
                    if (_settings.ImportType == FileImportSettingsItem.ImportTypes.DoorsReqIf)
                    {
                        filePathNew = OleDoors.Save(HoUtil.ReadAllText(f), f, ignoreNotSupportedFiles:_settings.Verbosity == FileImportSettingsItem.VerbosityType.Silent);
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



            bool IsGenerateDocx = true;
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            docFile = System.IO.Path.Combine(docFile, IsGenerateDocx ? "xxxxxxx.docx" : "xxxxxxx.rtf");

            if (docFile.EndsWith(".rtf"))
                HtmlToDocx.ConvertSautin(docFile, xhtmlValue);
            else HtmlToDocx.Convert(docFile, xhtmlValue);
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
                // add Column
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
        /// Add requirements from ReqIF recursive to datatable. One Row = one Requirement with the row-attributes as columns. It creates columns also for attributes without values.
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="children"></param>
        /// <param name="level"></param>
        private void AddReqIfRequirementsToDataTable(DataTable dt, List<SpecHierarchy> children, int level)
        {
            if (children == null || children.Count == 0) return;

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

Can't correctly identify objects. Identifier cut to 50 characters!", @"ReqIF Indentifier has length > 50");
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
        /// <param name="moduleIndex"></param>
        /// <returns></returns>
        List<ReqIFSharp.AttributeDefinition> GetTypesModule(ReqIF reqIf, int moduleIndex)
        {
            var specObjectTypes = new List<ReqIFSharp.AttributeDefinition>();
            var children = reqIf.CoreContent[0].Specifications[moduleIndex].Children;
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
        /// Add Tagged Values with Module Properties to Package/Object
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
