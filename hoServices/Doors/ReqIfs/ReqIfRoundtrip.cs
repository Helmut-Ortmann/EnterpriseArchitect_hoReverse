using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using EA;
using ReqIFSharp;
using TaggedValue = hoReverse.hoUtils.TaggedValue;

namespace EaServices.Doors.ReqIfs
{
    public class ReqIfRoundtrip:ReqIf
    {
        public ReqIfRoundtrip(EA.Repository rep, EA.Package pkg, string importFile, FileImportSettingsItem settings) : 
            base(rep,  pkg, importFile, settings)
        {
           
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

            // over all packages/guids
            int packageIndex = 0;
            foreach (var item in Settings.PackageGuidList)
            {
                // get the column/taggedValueType prefix for current module
                _prefixTv = Settings.GetPrefixTaggedValueType(packageIndex);
                string pkgGuid = item.Guid;
                string reqIfSpecId = item.ReqIfModuleId;

                // Use reqIF Specification ID or iterate by sequence (package index)
                ReqIfFileItem reqIfFileItem = String.IsNullOrWhiteSpace(reqIfSpecId)
                    ? reqIfFileList.GetItemForIndex(packageIndex)
                    : reqIfFileList.GetItemForReqIfId(reqIfSpecId);

                // estimate package of guid list in settings 
                Pkg = Rep.GetPackageByGuid(pkgGuid);
                Rep.ShowInProjectView(Pkg);

                bool result = RoundtripSpecification(reqIfFileItem.FilePath, reqIfFileItem.SpecContentIndex, reqIfFileItem.SpecIndex);
                if (result == false || _errorMessage1) return false;
                // next package
                packageIndex += 1;
            }


            // write the changes back
            return Compress(ImportModuleFile, Path.GetDirectoryName(importReqIfFiles[0]));

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
            ReqIfDeserialized = DeSerializeReqIf(file, validate: Settings.ValidateReqIF);
            if (ReqIfDeserialized == null) return false;

            // The Attribute definitions
            _moduleAttributeDefinitions = GetTypesModule(ReqIfDeserialized, reqifContentIndex, reqIfSpecIndex);


            if (Pkg.Elements.Count == 0)
            {
                MessageBox.Show($@"File: '{file}'
Contains: {ReqIfDeserialized.CoreContent.Count} modules

Roundtrip needs at least initial import and model elements in EA!

", @"No ReqIF initial import for an Roundtrip available, break!");
                return false;
            }

            // Export ReqIF SpecObjects stored in EA
            foreach (EA.Element el in Pkg.Elements)
            {
                _level = 0;
                if (!RoundtripUpdateReqIfForElementRecursive(el)) return false;
            }

            // serialize ReqIF
            return SerializeReqIf(file, compress: false);
        }
        /// <summary>
        /// Recursive update an element
        /// </summary>
        /// <param name="el"></param>
        /// <returns></returns>
        public bool RoundtripUpdateReqIfForElementRecursive(EA.Element el)
        {


            _count += 1;
            _countAll += 1;
            // Check type and stereotype
            if (el.Type != Settings.ObjectType || el.Stereotype != Settings.Stereotype) return true;
            if (!RoundtripUpdateReqIfForElement(el)) return false;


            if (el.Elements.Count > 0)
            {
                _level += 1;
                foreach (EA.Element childEl in el.Elements)
                {
                    if (!RoundtripUpdateReqIfForElementRecursive(childEl)) return false;
                }

                _level -= 1;
            }

            return true;

        }
        /// <summary>
        /// UpdateReqIf for an element. Handle for TV: Values or Macros like '=EA.GUID' and updating naming to Module prefix
        /// </summary>
        /// <param name="el"></param>
        /// <returns></returns>
        public bool RoundtripUpdateReqIfForElement(EA.Element el)
        {
            string id = el.Multiplicity;  //TaggedValue.GetTaggedValue(el, GetPrefixedTagValueName("Id"));
            SpecObject specObj;
            try
            {
                specObj = ReqIfDeserialized.CoreContent[0].SpecObjects.SingleOrDefault(x => x.Identifier == id);
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
                if (!RoundtripChangeValueReqIf(specObj, GetUnPrefixedTagValueName(tvName), tvValue, caseSensitive: false))
                    return false;
            }

            return true;
        }
        /// <summary>
        /// Change ReqIF value of the specObject and the attribute value. The Attribute must be part of the ReqIF file. 
        /// </summary>
        /// <param name="specObject"></param>
        /// <param name="name"></param>
        /// <param name="eaValue"></param>
        /// <param name="caseSensitive"></param>
        /// <returns></returns>
        private bool RoundtripChangeValueReqIf(SpecObject specObject, string name, string eaValue, bool caseSensitive = false)
        {
            try
            {
                AttributeValue attrValueObject = caseSensitive
                    ? specObject.Values.SingleOrDefault(x => x.AttributeDefinition.LongName == name)
                    : specObject.Values.SingleOrDefault(x =>
                        x.AttributeDefinition.LongName.ToLower() == name.ToLower());
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
                            if (!SetReqIfEnumValue((AttributeValueEnumeration)attrValueObject, eaValue)) return false;
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
    }
}
