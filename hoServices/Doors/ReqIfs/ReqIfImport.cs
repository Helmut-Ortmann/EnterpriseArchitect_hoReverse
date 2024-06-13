using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using hoReverse.hoUtils;
using ReqIFSharp;
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
    public class ReqIfImport:ReqIf
    {
    
        // Attributes not to import
        private readonly String[] _blackList1 = new String[]
        {
            "TableType", "TableBottomBorder", "TableCellWidth", "TableChangeBars", "TableLeftBorder",
            "TableLinkIndicators", "TableRightBorder", "TableShowAttrs", "TableTopBorder"
        }; // DOORS Table requirements


        public ReqIfImport(EA.Repository rep, EA.Package pkg, string importFile, FileImportSettingsItem settings, List<ReqIfLog> reqIfLogList) :
            base(rep, pkg, importFile, settings, reqIfLogList)
        {

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
        /// Get string with ReqIF Header Information 
        /// </summary>
        /// <returns></returns>
        protected string GetHeaderInfo(ReqIF reqIf=null)
        {
            reqIf = reqIf ?? ReqIfDeserialized;
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
        /// Import and update ReqIF Requirements in EA from ReqIF (compressed file). Derive Tagged Values from ReqSpec Attribute definition.
        /// It catches COM errors
        /// </summary>
        /// <param name="eaObjectType">EA Object type to create</param>
        /// <param name="eaStereotype">EA stereotype to create</param>
        /// <param name="stateNew">The EA state if the EA element is created</param>
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
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
            ReqIfFileList reqIfFileList = new ReqIfFileList(importReqIfFiles, Settings);
            if (reqIfFileList.ReqIfFileItemList.Count == 0) return false;

            // Check import settings
            if (!CheckImportFile()) return false;

            // over all packages/guids
            int packageIndex = 0;
            foreach (var item in Settings.PackageGuidList)
            {
                

                if (!String.IsNullOrWhiteSpace(item.ReqIfReqIfFleStorage))
                    Settings.EmbeddedFileStorageDictionary = item.ReqIfReqIfFleStorage;
                _prefixTv = Settings.GetPrefixTaggedValueType(packageIndex);

                string pkgGuid = item.Guid;
                string reqIfSpecId = item.ReqIfModuleId;
                try
                {
                    // Use reqIF Specification ID or iterate by sequence (package index)
                    ReqIfFileItem reqIfFileItem = String.IsNullOrWhiteSpace(reqIfSpecId)
                        ? reqIfFileList.GetItemForIndex(packageIndex)
                        : reqIfFileList.GetItemForReqIfId(reqIfSpecId);
                    // couldn't find a GUID, Error message output
                    if (reqIfFileItem == null) continue;

                    // estimate package of guid list in settings 
                    Pkg = Rep.GetPackageByGuid(pkgGuid);
                    if (Pkg == null)
                    {
                        MessageBox.Show($@"GUID={pkgGuid}
SpecificationID={reqIfSpecId}
Consider:
- Integrity EA Check + repair
- Compact EA Repository
- Check configuration/package guid

", @"Exception invalid package in configuration, skip package");
                        continue;
                    }
                    try
                    {
                        Rep.RefreshModelView(Pkg.PackageID);
                        Rep.ShowInProjectView(Pkg);
                    }
                    catch (Exception e)
                    {
                                MessageBox.Show($@"GUID={pkgGuid}
SpecificationID={reqIfSpecId}
Consider:
- Integrity EA Check + repair
- Compact EA Repository

{e}", @"Exception import into package");
                        return false;
                    }
                    

                    ImportSpecification(reqIfFileItem.FilePath, eaObjectType, eaStereotype,
                        reqIfFileItem.SpecContentIndex, reqIfFileItem.SpecIndex,
                        stateNew);
                    
                    if (result == false || _errorMessage1) return false;

                    // List all what is done
                    _reqIfLogList.Add(new ReqIfLog(reqIfFileItem.FilePath,Pkg.Name, Pkg.PackageGUID, reqIfSpecId, "")); 


                    // next package
                    packageIndex += 1;
                }
                catch (Exception e)
                {
                    MessageBox.Show($@"GUID={pkgGuid}
SpecificationID={reqIfSpecId}

{e}", @"Exception import into package");
                    return false;
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
            ReqIfDeserialized = DeSerializeReqIf(file, validate: Settings.ValidateReqIF);
            if (ReqIfDeserialized == null ) return MessageBox.Show("",@"Continue with next reqif file or cancel",MessageBoxButtons.OKCancel) == DialogResult.OK;
            
            _moduleAttributeDefinitions = GetTypesModule(ReqIfDeserialized, reqIfContentIndex, reqIfSpecIndex);

            // prepare EA, existing requirements to detect deleted and changed requirements
            ReadEaPackageRequirements();
            CreateEaPackageDeletedObjects();


            // Add requirements recursive for module to requirement table
            InitializeReqIfRequirementsTable(ReqIfDeserialized);
            Specification reqIfModule = ReqIfDeserialized.CoreContent[reqIfContentIndex].Specifications[reqIfSpecIndex];

            Rep.BatchAppend = true;
            Rep.EnableUIUpdates = false;
            UpdatePackage();

            // Read the requirements and put them to data Table
            AddReqIfRequirementsToDataTable(DtRequirements, reqIfModule.Children, 1);

            // Check imported ReqIF requirements
            if (CheckImportedRequirements(file))
            {
                //Rep.RefreshModelView(Pkg.PackageID);
                CreateUpdateDeleteEaRequirements(eaObjectType, eaStereotype, stateNew, "", file);

                //Rep.RefreshModelView(Pkg.PackageID);
                MoveDeletedRequirements();
                

                // handle links
                ReqIfRelation relations = new ReqIfRelation(ReqIfDeserialized, Rep, Settings);
            }

            Rep.BatchAppend = false;
            Rep.EnableUIUpdates = true;

            // Update package content
            Rep.ReloadPackage(Pkg.PackageID);
            Rep.RefreshModelView(Pkg.PackageID);
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

Probable causes:
- JSON import definition expects a non-existing ReqIF Attribute
  - e.g. RtfNameList, AttrNameList, AttrNotes, ..
  - See: File, Settings, ReqIF..
  - Have you reload the settings after change, File, Reload ...?
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
                    // suppress column if already shown in notes
                    if (notesColumn != c.Name)
                    {
                        // Enum with multi value
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
        private bool DeleteWritableTaggedValuesForElement(EA.Element el)
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
        /// Update EA linked document with xhtmlValue. It handles graphics and embedded files
        /// </summary>
        /// <param name="el">EA Element to update linked document</param>
        /// <param name="xhtmlValue">The XHTML value in XHTML or flat string format</param>
        /// <param name="importFile">If "" or null use the Class settings ImportModuleFile from constructor</param>
        /// <returns></returns>
        private bool UpdateLinkedDocument(EA.Element el, string xhtmlValue, string importFile="")
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
            bool isSuccess;
            if (Settings.UseMariGold)
                isSuccess = HtmlToDocx.Convert(docFile, xhtmlValue);
            else isSuccess = HtmlToDocx.ConvertSautin(docFile, xhtmlValue);
            if (!isSuccess)
            {
                ReportXhtmlError(el, importFile);
                return false;
            }

            try
            {
                bool res = el.LoadLinkedDocument(docFile);
                if (!res)
                {
                    ReportLoadLinkedDocument(el, importFile, docFile, xhtmlValue, null);
                    return false;
                }
            }
            catch (Exception e)
            {
                ReportLoadLinkedDocument(el, importFile, docFile, xhtmlValue, e);
                return false;
            }
            return true;

        }

        /// <summary>
        /// ReportLoadLinkedDocument
        /// </summary>
        /// <param name="el"></param>
        /// <param name="importFile"></param>
        /// <param name="docFile"></param>
        /// <param name="xhtmlValue"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private void ReportLoadLinkedDocument(EA.Element el, string importFile, string docFile, string xhtmlValue, Exception e=null)
        {
            string ex = "";
            if (e != null) ex = e.ToString();
            MessageBox.Show($@"ImportFile: '{importFile}'

EA GUID           {Tab}: '{el.ElementGUID}'
EA Multiplicity   {Tab}: '{el.Multiplicity}'
Name:             {Tab}: '{el.Name}'
EaLastError       {Tab}: '{el.GetLastError()}'
RtfDocxFile:      {Tab}: '{docFile}'

XHTML:'{xhtmlValue}

{ex}",
                @"Error loading Linked Document, break current requirement, continue!");

        }
        /// <summary>
        /// Report XHTML converting error
        /// </summary>
        /// <param name="el"></param>
        /// <param name="importFile"></param>
        private void ReportXhtmlError(EA.Element el, string importFile)
        {
            MessageBox.Show($@"ImportFile: '{importFile}'

EA GUID            {Tab}{Tab}{Tab}: '{el.ElementGUID}'
EA Multiplicity/key{Tab}: '{el.Multiplicity}'
Name:              {Tab}{Tab}{Tab}: '{el.Name}'
Alias:             {Tab}{Tab}{Tab}: '{el.Alias}'
",
                @"Error *xhtml to *.rtf/*.docx, break current requirement, continue with next!");
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
