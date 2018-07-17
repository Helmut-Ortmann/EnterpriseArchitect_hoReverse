using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using hoReverse.hoUtils;
using ReqIFSharp;
using Path = System.IO.Path;

namespace EaServices.Doors
{
    /// <summary>
    /// Import ReqIf requirements. The subclass ReqIfDoors handles DOORS specific features (currently not used). It handles:
    /// - *.reqif
    /// - *.reqifz (compressed)
    /// - Images like *.png
    /// - Modules (in settings give two package GUIDs
    /// Currently ReqIf doesn't support embedded files other than image (no ole, excel, pdf)
    /// </summary>
    public class ReqIf : DoorsModule
    {
        readonly string Tab = "\t";
        ReqIF _reqIf;
        int _subModuleIndex;
        // Prefix Tagged Values and Columnnames
        private string _prefixTv = "";

        ExportFields _exportFields;
        List<ReqIFSharp.AttributeDefinition> _moduleAttributeDefinitions;

        readonly FileImportSettingsItem _settings;
        bool _errorMessage1;
        // Attributes not to import
        readonly String[] _blackList1 = new String[] { "TableType", "TableBottomBorder", "TableCellWidth", "TableChangeBars", "TableLeftBorder", "TableLinkIndicators", "TableRightBorder", "TableShowAttrs", "TableTopBorder" };// DOORS Table requirements

        /// <summary>
        /// ReqIF Export/Roundtrip
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="pkg"></param>
        /// <param name="importFile"></param>
        /// <param name="settings"></param>
        public ReqIf(EA.Repository rep, EA.Package pkg, string importFile, FileImportSettingsItem settings) : base(rep, pkg, importFile)
        {
            _settings = settings;
        }
        /// <summary>
        /// Export updated Requirements according to TaggedValues/AttributeNames in _settings.WriteAttrNameList
        /// </summary>
        /// <param name="subModuleIndex"></param>
        /// <returns></returns>
        public override bool ExportUpdateRequirements(int subModuleIndex = 0)
        {
            _subModuleIndex = subModuleIndex;
            // Calculate the column/taggedValueType prefix for current module
            _prefixTv = _settings.PrefixTaggedValueTypeList.Count < _subModuleIndex
                ? _settings.PrefixTaggedValueTypeList[subModuleIndex]
                : "";

           
            _errorMessage1 = false;
            _exportFields = new ExportFields(_settings.WriteAttrNameList);
            // decompress reqif file and its embedded files
            string importReqIfFile = Decompress(ImportModuleFile);
            if (String.IsNullOrWhiteSpace(importReqIfFile)) return false;

            // Deserialize
            ReqIFDeserializer deserializer = new ReqIFDeserializer();
            _reqIf = deserializer.Deserialize(importReqIfFile);
            _moduleAttributeDefinitions = GetTypesModule(_reqIf, subModuleIndex);
            // Modules
            if (subModuleIndex >= _reqIf.CoreContent[0].Specifications.Count)
            {
                MessageBox.Show($@"File: '{importReqIfFile}'
Contains: {_reqIf.CoreContent.Count} modules
Requested: {_reqIf.CoreContent.Count}
Packages (per module one package/guid) are defined in Settings.json: 

", @"More packages defined as Modules are in ReqIF file, break!");
                return false;
            }

            // Modules
            if (_reqIf.CoreContent[0].Specifications.Count == 0)
            {
                MessageBox.Show($@"File: '{importReqIfFile}'
Contains: {_reqIf.CoreContent.Count} modules
Requested: {_reqIf.CoreContent.Count}
No module is defined in Settings.json: 

", @"No module defined in ReqIF file, break!");
                return false;
            }

            if (Pkg.Elements.Count == 0)
            {
                MessageBox.Show($@"File: '{importReqIfFile}'
Contains: {_reqIf.CoreContent.Count} modules

Export/Roundtrip needs at least initial import and model elements in EA!

", @"No ReqIF initial import for an Roundtrip export available, break!");
                return false;
            }

            // Export ReqIF SpecObjects stored in EA
            foreach (EA.Element el in Pkg.Elements)
            {
                _level = 0;
                if (! UpdateReqIfForElementRecursive(el)) return false;
            }

            // serialize ReqIF
            return SerializeReqIf(ImportModuleFile);
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
            if (! UpdateReqIfForElement(el)) return false;


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
        /// <param name="zipPath"></param>
        /// <returns></returns>
        bool SerializeReqIf(string zipPath)
        {
            var serializer = new ReqIFSerializer(false);
            string pathSerialize = Path.Combine(Path.GetDirectoryName(zipPath), $"_{Path.GetFileNameWithoutExtension(zipPath)}.reqif");
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
            Compress(zipPath, pathSerialize);
            return true;
        }
        /// <summary>
        /// UpdateReqIf for an element. Handle fot TV: Values or Macros like '=EA.GUID'
        /// </summary>
        /// <param name="el"></param>
        /// <returns></returns>
        public bool UpdateReqIfForElement(EA.Element el)
        {
            string id = TaggedValue.GetTaggedValue(el, "Id");
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
            // update values of ReqIF Attributes by TaggedValues
            foreach (string tvName in _exportFields.GetFields())
            {
                string tvValue = TaggedValue.GetTaggedValue(el, GetPrefixedTagValueName(tvName), caseSensitive:false);
                

                // update value
                string macroValue = _exportFields.GetMacroValue(el, tvName);
                if (macroValue != "") tvValue = macroValue;
                if (tvValue == "") continue;
                if (! ChangeValueReqIf(specObj, GetUnPrefixedTagValueName(tvName), tvValue,caseSensitive:false)) return false;
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
        private bool ChangeValueReqIf(SpecObject specObject, string name, string eaValue, bool caseSensitive=false)
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
                        attrValueObject = new AttributeValueString();
                        attrValueObject.AttributeDefinition = attributeType;
                        break;
                    case AttributeDefinitionXHTML _:
                        attrValueObject = new AttributeValueXHTML();
                        attrValueObject.AttributeDefinition = attributeType;
                        break;
                    case AttributeDefinitionEnumeration moduleAttributDefinitionEnumeration:
                        attrValueObject = new AttributeValueEnumeration();
                        attrValueObject.AttributeDefinition = attributeType;
                        multiValuedEnum = moduleAttributDefinitionEnumeration.IsMultiValued;

                        break;

                }

                if (attrValueObject == null) return true; // not supported datatype
                specObject.Values.Add(attrValueObject);
            }
            var attrType = attrValueObject.AttributeDefinition;//specObj.Values[0].AttributeDefinition.LongName;
            switch (attrType)
            {
                case  AttributeDefinitionXHTML _:
                    // handle new line
                    eaValue = eaValue.Replace("\r\n", "<br></br>");

                    string    xhtmlcontent = $@"<reqif-xhtml:div xmlns:reqif-xhtml=""http://www.w3.org/1999/xhtml"">{eaValue}</reqif-xhtml:div>";
                    attrValueObject.ObjectValue = xhtmlcontent;
                break;

                case AttributeDefinitionString _:
                    attrValueObject.ObjectValue = eaValue;
                    break;
                case AttributeDefinitionEnumeration _:

                    try
                    {
                        // take all the valid enums
                        SetEnumValue((AttributeValueEnumeration)attrValueObject, eaValue, multiValuedEnum);
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
        /// Set the values of a enum (single line or multi line
        /// </summary>
        /// <param name="attributeValueEnumeration"></param>
        /// <param name="values"></param>
        /// <param name="multiValueEnum"></param>
        public void SetEnumValue(AttributeValueEnumeration attributeValueEnumeration, string values, bool multiValueEnum)
        {
            // over all values split by ",:=;-"
            if (String.IsNullOrWhiteSpace(values)) return;

            // delete old values
            attributeValueEnumeration.Values.Clear();

            values = Regex.Replace(values.Trim(), @"\r\n?|\n|;|,|:|-|=", ",");
            foreach (var value in values.Split(','))
            {
                var enumValue = ((AttributeValueEnumeration)attributeValueEnumeration).Definition.Type.SpecifiedValues
                    .SingleOrDefault(x=> x.LongName == value);
                if (enumValue != null) ((AttributeValueEnumeration)attributeValueEnumeration).Values.Add(enumValue);
                if (!multiValueEnum) return;

            }

        }
        

        /// <summary>
        /// Import and update ReqIF Requirements. Derive Tagged Values from ReqSpec Attribut definition
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
            bool result = true;
           _errorMessage1 = false;
            _exportFields = new ExportFields(_settings.WriteAttrNameList);


            // Create Tagged Value Types

            // decompress reqif file and its embedded files
            string importReqIfFile = Decompress(ImportModuleFile);
            if (String.IsNullOrWhiteSpace(importReqIfFile)) return false;

            // Copy and convert embedded files files to target directory, only if the first module in a zipped reqif-file
            if (_settings.EmbeddedFileStorageDictionary != "" && subModuleIndex == 0)
            {
                string sourceDir = Path.GetDirectoryName(importReqIfFile);
                hoUtils.DirectoryExtension.CreateEmptyFolder(_settings.EmbeddedFileStorageDictionary);
                hoUtils.DirectoryExtension.DirectoryCopy(sourceDir, _settings.EmbeddedFileStorageDictionary,
                    copySubDirs: true);
            }

            // Deserialize
            ReqIFDeserializer deserializer = new ReqIFDeserializer();
            _reqIf = deserializer.Deserialize(importReqIfFile);
            _moduleAttributeDefinitions = GetTypesModule(_reqIf, subModuleIndex);

            // prepare EA, existing requirements to detect deleted and changed requirements
            ReadEaPackageRequirements();
            CreateEaPackageDeletedObjects();



            // Add requirements recursiv for module to requirement table
            InitializeReqIfRequirementsTable(_reqIf);
            Specification reqifModule = _reqIf.CoreContent[0].Specifications[subModuleIndex];

            Rep.BatchAppend = true;
            Rep.EnableUIUpdates = false;
            AddRequirementsToDataTable(DtRequirements, reqifModule.Children, 1);

            // Check imported ReqIF requirements
            if (CheckImportedRequirements())
            {
                CreateUpdateDeleteEaRequirements(eaObjectType, eaStereotype, stateNew, stateChanged, importReqIfFile);

                MoveDeletedRequirements();
                UpdatePackage();
            }

            Rep.BatchAppend = false;
            Rep.EnableUIUpdates = true;
            Rep.ReloadPackage(Pkg.PackageID);
            return result && (!_errorMessage1);
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
                if (! DtRequirements.Columns.Contains(column))
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

                CombineAttrValues(_settings.AliasList, row, out string alias, ShortNameLength, makeName:true);
                CombineAttrValues(_settings.AttrNameList,  row, out string name, ShortNameLength, makeName: true);
                string notes = GetAttrValue(notesColumn != "" ? row[notesColumn].ToString() : row[1].ToString());


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
                        where !ColumnNamesNoTaggedValues.Any(n => n == c.ColumnName)
                        select new
                        {
                            Name = c.ColumnName,
                            Value = row[c].ToString()
                        }
                    ;
                // Handle *.rtf/*.docx content
                string rtfValue = CombineRtfAttrValues(_settings.RtfNameList, row);

                // Update EA linked documents by graphics and embedded elements
                UpdateLinkedDocument(el, rtfValue, importFile);

                // Update/Create Tagged value
                foreach (var c in cols)
                {
                    if (notesColumn != c.Name)
                    {
                       
                        TaggedValue.SetUpdate(el, GetPrefixedTagValueName(c.Name), GetAttrValue(c.Value ?? ""));
                    }
                }
            }
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
        /// Compress the exported file
        /// </summary>
        /// <param name="zipFile">The path of the zip achive</param>
        /// <param name="fileName">The file to zip</param>
        void Compress(string zipFile, string fileName)
        {
            if (File.Exists(zipFile)) File.Delete(zipFile);
            using (var zip = ZipFile.Open(zipFile, ZipArchiveMode.Create))
            {
                zip.CreateEntryFromFile(fileName, Path.GetFileName(fileName));
            }
        }

        /// <summary>
        /// Decompress the import/export reqIf file if compressed format (*.reqifz)
        /// </summary>
        /// <param name="importReqIfFile"></param>
        /// <returns>The path to the *.reqif file</returns>
        private string Decompress(string importReqIfFile)
        {
            // *.reqifz for compressed ReqIf File
            if (importReqIfFile.ToUpper().EndsWith("Z"))
            {
                string extractDirectory = hoUtils.Compression.Zip.ExtractZip(importReqIfFile);
                if (String.IsNullOrWhiteSpace(extractDirectory)) return "";

                // extract reqif file from achive
                string pattern = "*.reqif";
                var files = Directory.GetFiles(extractDirectory, pattern);
                if (files.Length != 1)
                {
                    MessageBox.Show($@"Can't find '*.reqif' file in decompressed folder

*.reqifz File :  '{importReqIfFile}'
Pattern       :  '{pattern}'
Extract folder:  '{extractDirectory}'", @"Can't find '*.reqif' file in decompressed folder");
                }
                
                return files.Length > 0 ?files[0] :"";
            }

            return importReqIfFile;
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
        /// Initialize ReqIF Requirement DataTable with Columns (standard columns + one for each attribute)
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
                        if (attrEnumType.IsMultiValued)
                            {

                            }
                            else
                            {
                                TaggedValue.CreateTaggedValueTye(Rep, tvName, 
                                    $@"Type=Enum;{Environment.NewLine}Values={enumValue};", 
                                    "hoReverse:ReqIF automated generated!");
                            }


                        break;
                                
                }

            }

            return true;
        }


        /// <summary>
        /// Add requirements recursive to datatable 
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="children"></param>
        /// <param name="level"></param>
        private void AddRequirementsToDataTable(DataTable dt, List<SpecHierarchy> children, int level)
        {
            if (children == null || children.Count == 0) return;

            foreach (SpecHierarchy child in children)
            {
                DataRow row = dt.NewRow();
                SpecObject specObject = child.Object;

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

                List<AttributeValue> columns = specObject.Values;
                foreach (var column in columns)
                {
                    try
                    {   
                        // Handle blacklist
                        if (_blackList1.Contains(column.AttributeDefinition.LongName)) continue;

                        // Handle enums
                        if (column.AttributeDefinition is AttributeDefinitionEnumeration)
                        {
                            List<EnumValue> enumValues = (List<EnumValue>) column.ObjectValue;
                            string values = "";
                            foreach (var enumValue in enumValues)
                            {
                                values = $"{enumValue.LongName}{Environment.NewLine}";
                            }
                            row[column.AttributeDefinition.LongName] = values;
                        } else row[column.AttributeDefinition.LongName] = column.ObjectValue.ToString();
                        
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show($@"AttrName: '{column.AttributeDefinition.LongName}'{Environment.NewLine}{Environment.NewLine}{e}", 
                            @"Exception add ReqIF Attribute");
                    }
                }

                dt.Rows.Add(row);
                AddRequirementsToDataTable(dt, child.Children, level + 1);
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
                TaggedValue.CreateTv(el, property.Value); // only create TV
                else TaggedValue.SetUpdate(el, property.Name, GetAttrValue(property.Value ?? "")); // update TV
            }
        }

        /// <summary>
        /// Get Attribute Value. It converts xhtml and if makeName = true it removes multiple white spaces and control sequences from string
        /// </summary>
        /// <param name="attrValue"></param>
        /// <param name="makeName">True: Remove multiple whitespaces, control characters</param>
        /// <returns></returns>
        private string GetAttrValue(string attrValue, bool makeName=false)
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
            value = lNames.Count == 0 ? GetAttrValue(row[0].ToString()):"";
            string delimeter = "";
            foreach (var columnName in lNames)
            {

                try
                {
                    value = $"{value}{delimeter}{GetAttrValue(row[columnName].ToString())}";
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
            string attrRtfValue = lNames.Count == 0 ? GetAttrValue(row[0].ToString()):"";
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
