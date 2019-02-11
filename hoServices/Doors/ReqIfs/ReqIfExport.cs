using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using DataModels;
using hoUtils;
using hoUtils.Compression;
using hoUtils.DirFile;
using JetBrains.Annotations;
using ReqIFSharp;

namespace EaServices.Doors.ReqIfs
{
    public  class ReqIfExport:ReqIf
    {
        // Modulespecific, has to be estimated from ReqIF for a new module
        SpecificationType _specificationTypeModule;
        SpecObjectType _specObjectType;

        // Module specification (hierarchy)
        Specification _moduleSpecification;

        // Handler embedded files
        private ExportEmbeddedEaFiles _exportEmbeddedEaFile;

        /// <inheritdoc />
        public ReqIfExport(EA.Repository rep, EA.Package pkg, string importFile, FileImportSettingsItem settings) :
            base(rep, pkg, importFile, settings)
        {

        }

        /// <summary>
        /// Export Requirements according to TaggedValues/AttributeNames in _settings.WriteAttrNameList
        /// </summary>
        /// <param name="subModuleIndex"></param>
        /// <returns></returns>
        public bool ExportRequirements(int subModuleIndex = 0)
        {
            _subModuleIndex = subModuleIndex;
            // Initialize
            if (_subModuleIndex == 0)
            {
                // delete files and create directories for files to deliver:
                // - Embedded files from EA 
                // - Embedded png files for images in rtf
                // - MimeTypeImages (files from installation)
                // 
                DirectoryExtension.CreateEmptyDir(Settings.EmbeddedFileStorageDictionary);
                DirectoryExtension.CreateEmptyDir(Path.Combine(Settings.EmbeddedFileStorageDictionary, Settings.EmbeddedFiles));
                DirectoryExtension.CreateEmptyDir(Path.Combine(Settings.EmbeddedFileStorageDictionary, Settings.EmbeddedFilesPng));

                // copy mimetype specific images
                string installationPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string mimeTypeImagesInstallationPath = Path.Combine(installationPath, Settings.MimeTypeImages);
                string mimeTypeTypeImagesInReqIf = Path.Combine(Settings.EmbeddedFileStorageDictionary, Settings.MimeTypeImages);
                DirectoryExtension.CreateEmptyDir(mimeTypeTypeImagesInReqIf);
                DirectoryExtension.DirectoryCopy(mimeTypeImagesInstallationPath, mimeTypeTypeImagesInReqIf);


            }

            _exportEmbeddedEaFile = new ExportEmbeddedEaFiles(
                Settings.EmbeddedFileStorageDictionary,
                Settings.MimeTypeImages, 
                Settings.EmbeddedFiles);
            // Calculate the column/taggedValueType prefix for current module
            _prefixTv = "";
            _errorMessage1 = false;
            _exportFields = new ExportFields(Settings.WriteAttrNameList);

            // Write header, delete file if first package
            ReqIfDeserialized = AddHeader(ImportModuleFile, newFile:subModuleIndex==0);
            if (ReqIfDeserialized == null) return false;
            _reqIfContent = ReqIfDeserialized.CoreContent.SingleOrDefault();
            _specificationTypeModule = (SpecificationType)_reqIfContent.SpecTypes.SingleOrDefault(x => x.GetType() == typeof(SpecificationType));

            // Write the requirements for the current module/specification
            WriteModuleSpecification(Pkg);

            if (! CreateSpecObjects()) return false;
            if (!CreateSpecHierarchy()) return false;

            // serialize ReqIF
            string fileReqIf = Path.Combine(Zip.CreateTempDir(), Path.GetFileName(ImportModuleFile));
            SerializeReqIf(fileReqIf, compress:false);

           return  Compress(ImportModuleFile, Path.GetDirectoryName(fileReqIf),Settings.EmbeddedFileStorageDictionary);


        }
        /// <summary>
        /// Create Specification with hierachy
        /// </summary>
        private bool CreateSpecHierarchy()
        {
            var parent = _moduleSpecification;
            foreach (EA.Element el in Pkg.Elements)
            {
                var idSpecObject = ReqIfUtils.MakeReqIfId(ReqIfUtils.ReqIfIdType.SpecObject, ReqIfUtils.IdFromGuid(el.ElementGUID));
                var specObject =_reqIfContent.SpecObjects.SingleOrDefault(x => x.Identifier == idSpecObject);
                var specHierarchy = new SpecHierarchy()
                {   Identifier = ReqIfUtils.MakeReqIfId(ReqIfUtils.ReqIfIdType.SpecHierarchy, ReqIfUtils.IdFromGuid(el.ElementGUID)),
                    LastChange = el.Modified,
                    LongName = el.Name,
                    Object = specObject
                };
                parent.Children.Add(specHierarchy);
                foreach (EA.Element child in el.Elements)
                {
                    CreateSpecHierarchyElement(specHierarchy, child);
                }
            }

            return true;
        }
        /// <summary>
        /// Create a SpecHierarchy with the link to the SpecObject
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="el"></param>
        private void CreateSpecHierarchyElement(SpecHierarchy parent, EA.Element el)
        {
            var idSpecObject = ReqIfUtils.MakeReqIfId(ReqIfUtils.ReqIfIdType.SpecObject, ReqIfUtils.IdFromGuid(el.ElementGUID));
            //var idSpecObject = ReqI$"specObj{ReqIfUtils.IdFromGuid(el.ElementGUID)}";
            var specObject = _reqIfContent.SpecObjects.SingleOrDefault(x => x.Identifier == idSpecObject);
            var specHierarchy = new SpecHierarchy()
            {
                Identifier = ReqIfUtils.MakeReqIfId(ReqIfUtils.ReqIfIdType.SpecHierarchy, ReqIfUtils.IdFromGuid(el.ElementGUID)),
                LastChange = el.Modified,
                LongName = el.Name,
                Object = specObject
            };
            parent.Children.Add(specHierarchy);
            foreach (EA.Element child in el.Elements)
            {
                CreateSpecHierarchyElement(specHierarchy, child);
            }

        }


        /// <summary>
        /// Create SpecObject for all Requirements of Package
        /// </summary>
        private bool CreateSpecObjects()
        {
            using (var db = new EaDataModel(_provider, _connectionString))
            {
                // Read all Requirements for module/specification
                var reqs = (from r in db.t_object
                            join pkg in db.t_package on r.Package_ID equals pkg.Package_ID
                            where pkg.ea_guid == Pkg.PackageGUID
                            from tv in db.t_objectproperties.Where(tv2 => r.Object_ID == tv2.Object_ID)
                                .DefaultIfEmpty() // <<= makes join left join

                            select new
                            {
                                Name = r.Name,
                                ModifiedOn = r.ModifiedDate,
                                CreatedOn = r.CreatedDate,
                                Author = r.Author,
                                Guid = r.ea_guid,
                                Desc = r.Note,
                                TvName = tv.Property,
                                TvValue = tv.Value,
                                TvNote = tv.Notes
                            }).ToArray();



                string currentGuid = "";
                SpecObject specObject = null;
                foreach (var r in reqs)
                {
                    // new Requirements
                    if (currentGuid != r.Guid)
                    {
                        currentGuid = r.Guid;
                        
                        specObject = new SpecObject
                        {
                            LongName = $"{r.Name}",
                            Identifier = ReqIfUtils.MakeReqIfId(ReqIfUtils.ReqIfIdType.SpecObject, ReqIfUtils.IdFromGuid(r.Guid)),
                            LastChange = DateTime.Now,
                            Type       = _specObjectType
                        };
                        // ID
                        var attributeValueString = new AttributeValueString
                        {
                            Definition =
                                (AttributeDefinitionString)_specObjectType.SpecAttributes.SingleOrDefault(x =>
                                    x.GetType() == typeof(AttributeDefinitionString) && x.LongName == "ReqIF.ForeignId"),
                            TheValue = r.Guid
                        };
                        specObject.Values.Add(attributeValueString);
                        // Created on
                        var attributeValueDate = new AttributeValueDate
                        {
                            Definition =
                                (AttributeDefinitionDate)_specObjectType.SpecAttributes.SingleOrDefault(x =>
                                    x.GetType() == typeof(AttributeDefinitionDate) && x.LongName == "ReqIF.ForeignCreatedOn"),
                            TheValue = r.CreatedOn??DateTime.Now
                        };
                        specObject.Values.Add(attributeValueDate);
                        // Modified on
                        attributeValueDate = new AttributeValueDate
                        {
                            Definition =
                                (AttributeDefinitionDate)_specObjectType.SpecAttributes.SingleOrDefault(x =>
                                    x.GetType() == typeof(AttributeDefinitionDate) && x.LongName == "ReqIF.ForeignModifiedOn"),
                            TheValue = r.ModifiedOn??DateTime.Now
                        };
                        specObject.Values.Add(attributeValueDate);
                        // Created by
                        var attributeValueXhtml = new AttributeValueXHTML()
                        {
                            Definition =
                                (AttributeDefinitionXHTML)_specObjectType.SpecAttributes.SingleOrDefault(x =>
                                    x.GetType() == typeof(AttributeDefinitionXHTML) && x.LongName == "ReqIF.ForeignCreatedBy"),
                            TheValue = MakeXhtmlFromString(r.Author)
                        };
                        specObject.Values.Add(attributeValueXhtml);
                        // Modified by
                        attributeValueXhtml = new AttributeValueXHTML
                        {
                            Definition =
                                (AttributeDefinitionXHTML)_specObjectType.SpecAttributes.SingleOrDefault(x =>
                                    x.GetType() == typeof(AttributeDefinitionXHTML) && x.LongName == "ReqIF.ForeignModifiedBy"),
                            TheValue = MakeXhtmlFromString("not supported")
                        };
                        specObject.Values.Add(attributeValueXhtml);
                        // Attribute Name
                        if (r.Name != null)
                        {
                            attributeValueXhtml = new AttributeValueXHTML
                            {
                                Definition =
                                    (AttributeDefinitionXHTML) _specObjectType.SpecAttributes.SingleOrDefault(x =>
                                        x.GetType() == typeof(AttributeDefinitionXHTML) && x.LongName == "ReqIF.Name"),
                                TheValue = MakeXhtmlFromString(r.Name)
                            };
                            specObject.Values.Add(attributeValueXhtml);
                        }


                        // Attribute Text (note or Linked Document)
                        // _settings.SpecHandling
                        // - FileImportSettingsItem.SpecHandlingType.MixedMode  Preferred LinkedDocument, if no LinkedDocument then Notes
                        // - FileImportSettingsItem.SpecHandlingType.OnlyLinkedDocument
                        // - FileImportSettingsItem.SpecHandlingType.OnlyNotes
                        EA.Element el = Rep.GetElementByGuid(r.Guid);
                        string rtfText = el?.GetLinkedDocument();
                        var definition = (AttributeDefinitionXHTML) _specObjectType.SpecAttributes.SingleOrDefault(x =>
                            x.GetType() == typeof(AttributeDefinitionXHTML) && x.LongName == "ReqIF.Text");

                        // rtf text/description available and rtf export wanted (Mixed Mode or OnlyLinkedDocument)
                        if (  !String.IsNullOrEmpty(rtfText) &&
                             (
                                 Settings.SpecHandling == FileImportSettingsItem.SpecHandlingType.MixedMode ||
                                 Settings.SpecHandling == FileImportSettingsItem.SpecHandlingType.OnlyLinkedDocument
                             )
                           )
                        {
                            string fileDir = Settings.EmbeddedFileStorageDictionary;
                            string xhtml = RtfToXhtml.Convert(rtfText, fileDir,Settings.EmbeddedFilesPng);

                            // Handle embedded files of the EA-Element
                            xhtml = $@"{xhtml}
{_exportEmbeddedEaFile.MakeXhtmlForEmbeddedFiles(el)}";

                            // Text = Linked Document
                            attributeValueXhtml = new AttributeValueXHTML
                            {
                                Definition = definition,
                                TheValue = MakeReqIfXhtmlFromXhtml(xhtml)
                            };
                            specObject.Values.Add(attributeValueXhtml);

                        }
                        else
                        {
                            // Check export EA Notes
                            // No rtf export and Mixed Mode or OnlyNotes
                            if (!String.IsNullOrEmpty(r.Desc) &&
                                  ( Settings.SpecHandling == FileImportSettingsItem.SpecHandlingType.MixedMode ||
                                    Settings.SpecHandling == FileImportSettingsItem.SpecHandlingType.OnlyNotes
                                  )
                               )
                            {
                                // Text = notes.
                                attributeValueXhtml = new AttributeValueXHTML
                                {
                                    Definition = definition,
                                    TheValue = MakeXhtmlFromEaNotes(Rep, r.Desc)
                                };
                                specObject.Values.Add(attributeValueXhtml);
                            }
                        }
                    
                        //------------------  Values added to SpecObjects ----------------------------------------
                        // Add the SpecObject
                        _reqIfContent.SpecObjects.Add(specObject);

                        // Export all embedded element files
                        _exportEmbeddedEaFile.CopyEmbeddedFiles(el);

                        // new requirements
                    }



                    // Add Tagged Value if defined
                    if (! String.IsNullOrWhiteSpace(r.TvName)) {

                        // Check if tagged value is enumeration
                        var dataTypeEnumeration =
                            (DatatypeDefinitionEnumeration) _reqIfContent.DataTypes.SingleOrDefault(x =>
                                x.GetType() == typeof(DatatypeDefinitionEnumeration)
                                && x.LongName == r.TvName);
                        if (dataTypeEnumeration == null)
                        {
                            var attributeValueXhtml = new AttributeValueXHTML
                            {
                                Definition =
                                    (AttributeDefinitionXHTML) _specObjectType.SpecAttributes.SingleOrDefault(x =>
                                        x.GetType() == typeof(AttributeDefinitionXHTML) && x.LongName == r.TvName),
                                TheValue = MakeXhtmlFromEaNotes(Rep, ReqIfUtils.GetEaTaggedValue(r.TvValue, r.TvNote))
                            };
                            specObject.Values.Add(attributeValueXhtml);
                        }
                        else
                        {
                            var attributeDefinitionEnumeration =
                                (AttributeDefinitionEnumeration) _specObjectType.SpecAttributes.SingleOrDefault(x =>
                                    x.GetType() == typeof(AttributeDefinitionEnumeration) && x.LongName == r.TvName);
                            var attributeValueEnumeration = new AttributeValueEnumeration
                            {
                                Definition = attributeDefinitionEnumeration

                            };
                            if (!SetReqIfEnumValue(attributeValueEnumeration, r.TvValue)) return false;
                            specObject.Values.Add(attributeValueEnumeration);

                        }
                    }

                }
            }

            return true;

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
        /// Add Data Types for Notes (Enums, Checklist)
        /// </summary>
        /// <param name="reqIf"></param>
        /// <param name="pkg"></param>
        // ReSharper disable once UnusedParameter.Local
        private void AddDatatypesForPackage(ReqIF reqIf, EA.Package pkg)
        {
            using (var db = new EaDataModel(_provider, _connectionString))
            {
                var tvProperties = (from tv in db.t_objectproperties
                    join o in db.t_object on tv.Object_ID equals o.Object_ID
                    join p in db.t_package on o.Package_ID equals p.Package_ID
                    join tvType in db.t_propertytypes on tv.Property equals tvType.Property
                    where p.ea_guid == pkg.PackageGUID && tvType.Notes.Contains("Values=") && 
                                                           (tvType.Notes.Contains("Type=Enum") || tvType.Notes.Contains("Type=CheckList"))
                    orderby tvType.Property
                    group tvType by new { Property = tvType.Property, Notes = tvType.Notes.Substring(0) } into grp  //, tvType.Notes
                    select new { grp.Key.Property, grp.Key.Notes }).Distinct();

                // make ReqIF DataTypes
                foreach (var tv in tvProperties)
                {
                    var propertyValue = tv.Property.Trim();
                    // Check if Enumeration-Datatype already exists
                    var dataTypeEnumeration = (DatatypeDefinitionEnumeration)_reqIfContent.DataTypes.FirstOrDefault(x => x.GetType() == typeof(DatatypeDefinitionEnumeration)
                                                                                                                        && x.LongName == propertyValue);
                    if (dataTypeEnumeration != null) continue;
                    string idEnum = ReqIfUtils.MakeReqIfId(ReqIfUtils.ReqIfIdType.DataType, pkg.Name, propertyValue);
                    var datatypeDefinitionEnumeration = new DatatypeDefinitionEnumeration
                    {
                        Description = $"{tv.Notes}",
                        Identifier = idEnum,
                        LastChange = DateTime.Now,
                        LongName = $"{propertyValue}"
                        
                    };
                    // get enumeration values
                    Regex rx = new Regex("Values=([^;]*);");
                    Match match = rx.Match(tv.Notes);
                    if (match.Success)
                    {
                        int index = 0;
                        foreach (var value in match.Groups[1].Value.Split(','))
                        {
                            index += 1;
                            var embeddedValue = new EmbeddedValue
                            {
                                Key = index,
                                OtherContent = value.Trim(),

                            };
                            var enumValue = new EnumValue
                            {
                                Identifier = ReqIfUtils.MakeReqIfId(ReqIfUtils.ReqIfIdType.EnumValue, propertyValue, index.ToString()),
                                LastChange = DateTime.Now,
                                LongName = value.Trim(),
                                Properties = embeddedValue
                            };
                            // Add enum value
                            datatypeDefinitionEnumeration.SpecifiedValues.Add(enumValue);
                        }
                        
                    }
                   _reqIfContent.DataTypes.Add(datatypeDefinitionEnumeration);

                }

            }

        }



        /// <summary>
        /// Write the ReqIF moduleSpecification, DataTypes, SpecObjecttype for the pkg / module specification
        /// </summary>
        /// <param name="pkg"></param>
        private void WriteModuleSpecification([NotNull]EA.Package pkg)
        {
            // Module specification
            _moduleSpecification = new Specification
            {
                
                Description = $"Module specification of Package '{pkg.Name}', GUID={pkg.PackageGUID}",
                Identifier = ReqIfUtils.MakeReqIfId(ReqIfUtils.ReqIfIdType.Specification, ReqIfUtils.IdFromGuid(pkg.PackageGUID)),
                LastChange = DateTime.Now,
                LongName = $"Module {pkg.Name}",
                Type = _specificationTypeModule
            };
           _reqIfContent.Specifications.Add(_moduleSpecification);

            AddAttributesModuleSpecification(_moduleSpecification, pkg);
            // Add datatypes for packages (enums)
            AddDatatypesForPackage(ReqIfDeserialized, pkg);
            // AddSpecObj type for package/module
            AddSpeObjectTypeForModule(ReqIfDeserialized, pkg);

        }

        /// <summary>
        /// Add Spec object type for module/package
        /// </summary>
        /// <param name="reqIf"></param>
        /// <param name="pkg"></param>
        // ReSharper disable once UnusedParameter.Local
        private void AddSpeObjectTypeForModule(ReqIF reqIf, EA.Package pkg)
        {
            // SpecObjType of package/module
            _specObjectType = new SpecObjectType
            {
                LongName = $"{pkg.Name}",
                Identifier = $"specObjT{ReqIfUtils.IdFromGuid(pkg.PackageGUID)}",
                LastChange = DateTime.Now
               };

            
            // Add Standard Attributes
            // 
            var attributeDefinitionXhtml = new AttributeDefinitionXHTML
            {
                LongName = "ReqIF.Name",
                Identifier = ReqIfUtils.MakeReqIfId(ReqIfUtils.ReqIfIdType.Attribute, pkg.Name, "ReqIF.Name"),
                LastChange = DateTime.Now,
                Type = (DatatypeDefinitionXHTML)_reqIfContent.DataTypes.SingleOrDefault(x =>
                    x.GetType() == typeof(DatatypeDefinitionXHTML))
            };
            _specObjectType.SpecAttributes.Add(attributeDefinitionXhtml);
            attributeDefinitionXhtml = new AttributeDefinitionXHTML
            {
                LongName = "ReqIF.ChapterName",
                Identifier = ReqIfUtils.MakeReqIfId(ReqIfUtils.ReqIfIdType.Attribute, pkg.Name, "ReqIF.ChapterName"),
                LastChange = DateTime.Now,
                Type = (DatatypeDefinitionXHTML)_reqIfContent.DataTypes.SingleOrDefault(x =>
                    x.GetType() == typeof(DatatypeDefinitionXHTML))
            };
            _specObjectType.SpecAttributes.Add(attributeDefinitionXhtml);
            attributeDefinitionXhtml = new AttributeDefinitionXHTML
            {
                LongName = "ReqIF.Text",
                Identifier = ReqIfUtils.MakeReqIfId(ReqIfUtils.ReqIfIdType.Attribute, pkg.Name, "ReqIF.Text"),
                LastChange = DateTime.Now,
                Type = (DatatypeDefinitionXHTML)_reqIfContent.DataTypes.SingleOrDefault(x =>
                    x.GetType() == typeof(DatatypeDefinitionXHTML))
            };
           _specObjectType.SpecAttributes.Add(attributeDefinitionXhtml);
            attributeDefinitionXhtml = new AttributeDefinitionXHTML
            {
                LongName = "ReqIF.ForeignModifiedBy",
                Identifier = ReqIfUtils.MakeReqIfId(ReqIfUtils.ReqIfIdType.Attribute, pkg.Name, "ReqIF.ModifiedBy"),
                LastChange = DateTime.Now,
                Type = (DatatypeDefinitionXHTML)_reqIfContent.DataTypes.SingleOrDefault(x =>
                    x.GetType() == typeof(DatatypeDefinitionXHTML))
            };
            _specObjectType.SpecAttributes.Add(attributeDefinitionXhtml);
            attributeDefinitionXhtml = new AttributeDefinitionXHTML
            {
                LongName = "ReqIF.ForeignCreatedBy",
                Identifier = ReqIfUtils.MakeReqIfId(ReqIfUtils.ReqIfIdType.Attribute, pkg.Name, "ReqIF.CreatedBy"),
                LastChange = DateTime.Now,
                Type = (DatatypeDefinitionXHTML)_reqIfContent.DataTypes.SingleOrDefault(x =>
                    x.GetType() == typeof(DatatypeDefinitionXHTML))
            };
            _specObjectType.SpecAttributes.Add(attributeDefinitionXhtml);

            var attributeDefinitionDate = new AttributeDefinitionDate
            {
                LongName = "ReqIF.ForeignCreatedOn",
                Identifier = ReqIfUtils.MakeReqIfId(ReqIfUtils.ReqIfIdType.Attribute, pkg.Name, "ReqIF.CreatedOn"),
                LastChange = DateTime.Now,
                Type = (DatatypeDefinitionDate)_reqIfContent.DataTypes.SingleOrDefault(x =>
                    x.GetType() == typeof(DatatypeDefinitionDate))
            };
            _specObjectType.SpecAttributes.Add(attributeDefinitionDate);
            attributeDefinitionDate = new AttributeDefinitionDate
            {
                LongName = "ReqIF.ForeignModifiedOn",
                Identifier = ReqIfUtils.MakeReqIfId(ReqIfUtils.ReqIfIdType.Attribute, pkg.Name, "ReqIF.ModifiedOn"),
                LastChange = DateTime.Now,
                Type = (DatatypeDefinitionDate)_reqIfContent.DataTypes.SingleOrDefault(x =>
                    x.GetType() == typeof(DatatypeDefinitionDate))
            };
            _specObjectType.SpecAttributes.Add(attributeDefinitionDate);
            var attributeDefinitionBool = new AttributeDefinitionBoolean
            {
                LongName = "ReqIF.ForeignDeleted",
                Identifier = ReqIfUtils.MakeReqIfId(ReqIfUtils.ReqIfIdType.Attribute, pkg.Name, "ReqIF.ForeignDeleted"),
                LastChange = DateTime.Now,
                Type = (DatatypeDefinitionBoolean)_reqIfContent.DataTypes.SingleOrDefault(x =>
                    x.GetType() == typeof(DatatypeDefinitionBoolean))
            };
            _specObjectType.SpecAttributes.Add(attributeDefinitionBool);
            var attributeDefinitionString = new AttributeDefinitionString
            {
                LongName = "ReqIF.ForeignId",
                Identifier = ReqIfUtils.MakeReqIfId(ReqIfUtils.ReqIfIdType.Attribute, pkg.Name, "ReqIF.ForeignId"),
                LastChange = DateTime.Now,
                Type = (DatatypeDefinitionString)_reqIfContent.DataTypes.SingleOrDefault(x =>
                    x.GetType() == typeof(DatatypeDefinitionString))
            };
            _specObjectType.SpecAttributes.Add(attributeDefinitionString);


            using (var db = new EaDataModel(_provider, _connectionString))
            {
                // get all TVs
                var tvs = (from tv in db.t_objectproperties
                join o in db.t_object on tv.Object_ID equals o.Object_ID
                join p in db.t_package on o.Package_ID equals p.Package_ID
                where p.ea_guid == pkg.PackageGUID
                orderby tv.Property
                //group tv by new { tv.Property, tv.PropertyID} into grp
                select tv.Property).Distinct();

                // add attributes of the tagged values
                foreach (var tvName in tvs)
                {
                    
                    var dataTypeEnumeration = (DatatypeDefinitionEnumeration)_reqIfContent.DataTypes.SingleOrDefault(x => x.GetType() == typeof(DatatypeDefinitionEnumeration)
                                                                                                                        && x.LongName == tvName);
                    if (dataTypeEnumeration == null)
                    {
                        attributeDefinitionXhtml = new AttributeDefinitionXHTML
                        {
                            LongName = $"{tvName}",
                            Identifier = ReqIfUtils.MakeReqIfId(ReqIfUtils.ReqIfIdType.Attribute, pkg.Name, tvName),
                            LastChange = DateTime.Now,
                            Type = (DatatypeDefinitionXHTML)_reqIfContent.DataTypes.SingleOrDefault(x =>
                                x.GetType() == typeof(DatatypeDefinitionXHTML))
                        };
                        _specObjectType.SpecAttributes.Add(attributeDefinitionXhtml);

                    }
                    else
                    { // Enumeration
                        
                       
                       var attributeDefinitionEnumeration = new AttributeDefinitionEnumeration
                            {
                                LongName = $"{tvName}",
                                Identifier = ReqIfUtils.MakeReqIfId(ReqIfUtils.ReqIfIdType.Attribute, pkg.Name, tvName),
                                LastChange = DateTime.Now,
                                Type = dataTypeEnumeration
                            };
                        if (dataTypeEnumeration.Description.Contains("Type=CheckList"))
                            attributeDefinitionEnumeration.IsMultiValued = true;
                        _specObjectType.SpecAttributes.Add(attributeDefinitionEnumeration);

                    }
                }
            }
            // Add Attrributes to SpecObjType
           _reqIfContent.SpecTypes.Add(_specObjectType);


        }

        /// <summary>
        /// Adds the ReqIF header if the *.reqifz file doesn't exists. It also adds the default definitions:
        /// - Core Data Types
        /// - SpecificationType Module
        /// If the *.reqifz file exists it reads and deserialize it. 
        /// </summary>
        /// <param name="file"></param>
        /// <param name="newFile"></param>
        /// <returns></returns>
        private ReqIF AddHeader(string file, bool newFile = true)
        {
            if (newFile && File.Exists(file)) DirFiles.FileDelete(file);
            if (File.Exists(file))
            {
                // decompress reqif file and its embedded files
                string[] importReqIfFile = Decompress(file);
                if (String.IsNullOrWhiteSpace(importReqIfFile[0])) return null;

               // deserialize file
               return DeSerializeReqIf(file,Settings.ValidateReqIF);
              
            }
            else
            {
                // Create new ReqIF file
                var reqIf = new ReqIF
                {
                    Lang = "en"
                };
                var header = new ReqIFHeader()
                {
                    Title = $"{Settings.Name}",
                    CreationTime = DateTime.Now,
                    Identifier = ReqIfUtils.MakeReqIfId(ReqIfUtils.ReqIfIdType.ReqIfHeader),
                    RepositoryId = Rep.ProjectGUID,
                    ReqIFToolId = $"hoReverse V{Assembly.GetExecutingAssembly().GetName().Version}",
                    ReqIFVersion = "1.0",
                    SourceToolId = $"SPARX EA Lib={Rep.LibraryVersion}",
                    Comment = $"Export EA from {Rep.ConnectionString}"
                };
                reqIf.TheHeader.Add(header);

                // Content
                _reqIfContent = new ReqIFContent();
                reqIf.CoreContent.Add(_reqIfContent);

                AddCoreDataTypes(reqIf);
                _specificationTypeModule = AddSpecificationTypeModule(reqIf);
                return reqIf;


            }
        }
        /// <summary>
        /// Create Core Data Types:
        /// - Boolean
        /// - DateTime
        /// - String
        /// - XHTML
        /// </summary>
        // ReSharper disable once UnusedParameter.Local
        private void AddCoreDataTypes(ReqIF reqIf)
        {
            var datatypeDefinitionBoolean =
                new DatatypeDefinitionBoolean
                {
                    Description = "boolean data type definition",
                    Identifier = ReqIfUtils.MakeReqIfId(ReqIfUtils.ReqIfIdType.DataType, "boolean"),
                    LastChange = DateTime.Now,
                    LongName = "a boolean"
                };
           _reqIfContent.DataTypes.Add(datatypeDefinitionBoolean);

            var datatypeDefinitionDate = new DatatypeDefinitionDate
            {
                Description = "date data type definition",
                Identifier = ReqIfUtils.MakeReqIfId(ReqIfUtils.ReqIfIdType.DataType,"DateTime"),
                LastChange = DateTime.Now,
                LongName = "a date"
            };
           _reqIfContent.DataTypes.Add(datatypeDefinitionDate);

            var datatypeDefinitionInteger =
                new DatatypeDefinitionInteger
                {
                    Description = "integer data type definition",
                    Identifier = ReqIfUtils.MakeReqIfId(ReqIfUtils.ReqIfIdType.DataType, "Integer"),
                    LastChange = DateTime.Now,
                    LongName = "an integer",
                    Min = -2147483648,
                    Max = 2147483647
                };
           _reqIfContent.DataTypes.Add(datatypeDefinitionInteger);

            var datatypeDefinitionString = new DatatypeDefinitionString
            {
                Description = "string data type definition",
                Identifier = ReqIfUtils.MakeReqIfId(ReqIfUtils.ReqIfIdType.DataType, "String"),
                LastChange = DateTime.Now,
                MaxLength = 32000,
                LongName = "a string"
            };
           _reqIfContent.DataTypes.Add(datatypeDefinitionString);

            var datatypeDefinitionXhtml = new DatatypeDefinitionXHTML
            {
                Description = "string data type definition",
                Identifier = "xhtml",
                LastChange = DateTime.Now,
                LongName = "a string"
            };
           _reqIfContent.DataTypes.Add(datatypeDefinitionXhtml);

        }
        /// <summary>
        /// Create and add the module <see cref="SpecificationType"/> with attribute definitions
        /// </summary>
        // ReSharper disable once UnusedParameter.Local
        private SpecificationType AddSpecificationTypeModule(ReqIF reqIf)
        {
            var specificationTypeModule = new SpecificationType
            {
                LongName = "Modules/Packages",
                Identifier = ReqIfUtils.MakeReqIfId(ReqIfUtils.ReqIfIdType.DataType, _subModuleIndex.ToString()),
                LastChange = DateTime.Now

            };

            AddAttributeDefinitionsToModuleSpecificationType(specificationTypeModule);
           _reqIfContent.SpecTypes.Add(specificationTypeModule);
            return specificationTypeModule;
        }


        /// <summary>
        /// Add Attribut definitions to SpecificationType of Modules
        /// </summary>
        /// <param name="specType"></param>
        private void AddAttributeDefinitionsToModuleSpecificationType(SpecType specType)
        {
            var attributeDefinitionReqIfName = new AttributeDefinitionXHTML
            {
                LongName = "ReqIF.Name",
                Identifier = "specification-reqif-name",
                LastChange = DateTime.Now,
                Type = (DatatypeDefinitionXHTML)_reqIfContent.DataTypes.SingleOrDefault(x =>
                    x.GetType() == typeof(DatatypeDefinitionXHTML))
            };
            specType.SpecAttributes.Add(attributeDefinitionReqIfName);

            var attributeDefinitionReqIfDescription = new AttributeDefinitionXHTML
            {
                LongName = "ReqIF.Description",
                Identifier = "specification-reqif-description",
                LastChange = DateTime.Now,
                Type = (DatatypeDefinitionXHTML)_reqIfContent.DataTypes.SingleOrDefault(x =>
                    x.GetType() == typeof(DatatypeDefinitionXHTML))
            };
            specType.SpecAttributes.Add(attributeDefinitionReqIfDescription);

            var attributeDefinitionReqICreatedBy = new AttributeDefinitionXHTML
            {
                LongName = "ReqIF.ForeignCreatedBy",
                Identifier = "specification-reqif-createdby",
                LastChange = DateTime.Now,
                Type = (DatatypeDefinitionXHTML)_reqIfContent.DataTypes.SingleOrDefault(x =>
                    x.GetType() == typeof(DatatypeDefinitionXHTML))
            };
            specType.SpecAttributes.Add(attributeDefinitionReqICreatedBy);
            var attributeDefinitionReqIModifiedBy = new AttributeDefinitionXHTML
            {
                LongName = "ReqIF.ForeignModifiedBy",
                Identifier = "specification-reqif-modifiedby",
                LastChange = DateTime.Now,
                Type = (DatatypeDefinitionXHTML)_reqIfContent.DataTypes.SingleOrDefault(x =>
                    x.GetType() == typeof(DatatypeDefinitionXHTML))
            };
            specType.SpecAttributes.Add(attributeDefinitionReqIModifiedBy);

            var attributeDefinitionReqIModifiedOn = new AttributeDefinitionDate()
            {
                LongName = "ReqIF.ForeignModifiedOn",
                Identifier = "specification-reqif-modified-on",
                LastChange = DateTime.Now,
                Type = (DatatypeDefinitionDate)_reqIfContent.DataTypes.SingleOrDefault(x =>
                    x.GetType() == typeof(DatatypeDefinitionDate))
            };
            specType.SpecAttributes.Add(attributeDefinitionReqIModifiedOn);

            var attributeDefinitionReqCreatedOn = new AttributeDefinitionDate()
            {
                LongName = "ReqIF.ForeignCreatedOn",
                Identifier = "specification-reqif-created-on",
                LastChange = DateTime.Now,
                Type = (DatatypeDefinitionDate)_reqIfContent.DataTypes.SingleOrDefault(x =>
                    x.GetType() == typeof(DatatypeDefinitionDate))
            };
            specType.SpecAttributes.Add(attributeDefinitionReqCreatedOn);


            var attributeDefinitionReqPrefix = new AttributeDefinitionXHTML()
            {
                LongName = "ReqIF.Prefix",
                Identifier = "specification-reqif-prefix",
                LastChange = DateTime.Now,
                Type = (DatatypeDefinitionXHTML)_reqIfContent.DataTypes.SingleOrDefault(x =>
                    x.GetType() == typeof(DatatypeDefinitionXHTML))
            };
            specType.SpecAttributes.Add(attributeDefinitionReqPrefix);

        }


        /// <summary>
        /// Create and add the module specifications <see cref="SpecificationType"/> with attribute definitions
        /// </summary>
        private void AddAttributesModuleSpecification(SpecElementWithAttributes specElementWithAttributes,  EA.Package pkg)
        {
            EA.Element elPkg = Rep.GetElementByGuid(pkg.PackageGUID);
            SpecType specType = specElementWithAttributes.SpecType;

            // Name
            var attributeValueXhtml = new AttributeValueXHTML
            {
                Definition = (AttributeDefinitionXHTML) specType.SpecAttributes.SingleOrDefault(x =>
                    x.GetType() == typeof(AttributeDefinitionXHTML) && x.Identifier == "specification-reqif-name"),
                TheValue = MakeXhtmlFromEaNotes(Rep, pkg.Name)
            };
            specElementWithAttributes.Values.Add(attributeValueXhtml);

            // Description
            attributeValueXhtml = new AttributeValueXHTML
            {
                Definition = (AttributeDefinitionXHTML) specType.SpecAttributes.SingleOrDefault(x =>
                    x.GetType() == typeof(AttributeDefinitionXHTML) &&
                    x.Identifier == "specification-reqif-description"),
                    TheValue = MakeXhtmlFromEaNotes(Rep, pkg.Notes)
            };
            specElementWithAttributes.Values.Add(attributeValueXhtml);


            // Prefix
            attributeValueXhtml = new AttributeValueXHTML
            {
                Definition = (AttributeDefinitionXHTML)specType.SpecAttributes.SingleOrDefault(x =>
                    x.GetType() == typeof(AttributeDefinitionXHTML) &&
                    x.Identifier == "specification-reqif-prefix"),
                TheValue = MakeXhtmlFromString("")
            };
            specElementWithAttributes.Values.Add(attributeValueXhtml);

            // CreatedBy
            attributeValueXhtml = new AttributeValueXHTML
            {
                Definition = (AttributeDefinitionXHTML)specType.SpecAttributes.SingleOrDefault(x =>
                    x.GetType() == typeof(AttributeDefinitionXHTML) &&
                    x.Identifier == "specification-reqif-createdby"),
                TheValue = MakeXhtmlFromString(elPkg.Author)
            };
            specElementWithAttributes.Values.Add(attributeValueXhtml);

            // CreatedOn
            var attributeValueDate = new AttributeValueDate()
            {
                Definition = (AttributeDefinitionDate)specType.SpecAttributes.SingleOrDefault(x =>
                    x.GetType() == typeof(AttributeDefinitionDate) &&
                    x.Identifier == "specification-reqif-created-on"),
                TheValue = elPkg.Created
            };
            specElementWithAttributes.Values.Add(attributeValueDate);

            // ModifiedOn
            attributeValueDate = new AttributeValueDate()
            {
                Definition = (AttributeDefinitionDate)specType.SpecAttributes.SingleOrDefault(x =>
                    x.GetType() == typeof(AttributeDefinitionDate) &&
                    x.Identifier == "specification-reqif-modified-on"),
                TheValue = elPkg.Created
            };
            specElementWithAttributes.Values.Add(attributeValueDate);

            // ModifiedBy
            attributeValueXhtml = new AttributeValueXHTML
            {
                Definition = (AttributeDefinitionXHTML)specType.SpecAttributes.SingleOrDefault(x =>
                    x.GetType() == typeof(AttributeDefinitionXHTML) &&
                    x.Identifier == "specification-reqif-modifiedby"),
                TheValue = MakeXhtmlFromString("Not supported")
            };
            specElementWithAttributes.Values.Add(attributeValueXhtml);

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


    }
}
