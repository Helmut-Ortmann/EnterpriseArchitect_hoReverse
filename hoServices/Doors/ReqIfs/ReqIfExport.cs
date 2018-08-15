using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using DataModels;
using hoUtils.Compression;
using hoUtils.DirFile;
using JetBrains.Annotations;
using ReqIFSharp;

namespace EaServices.Doors.ReqIfs
{
    public partial class ReqIf
    {
        // Identifier for export
        const string SpecificationTypeModuleId = @"specificationTypeModule";
        const string SpecificationTypeModuleName = @"Modules/Packages";
        // Modulspecifisch, has to be estimated from ReqIF for a new module
        SpecificationType _specificationTypeModule;

        const string SpecificationTypePrefixId = @"specificationType";
        const string SpecificationTypePrefixName = @"SpecificationType";
        // Modulspecifisch, has to be estimated from ReqIF for a new module
        SpecificationType _specificationType;
        private SpecObjectType _specObjectType;

        private Specification _moduleSpecification;



        /// <summary>
        /// Export Requirements according to TaggedValues/AttributeNames in _settings.WriteAttrNameList
        /// </summary>
        /// <param name="subModuleIndex"></param>
        /// <returns></returns>
        public bool ExportRequirements(int subModuleIndex = 0)
        {
            _subModuleIndex = subModuleIndex;
            // Calculate the column/taggedValueType prefix for current module
            _prefixTv = "";


            _errorMessage1 = false;
            _exportFields = new ExportFields(_settings.WriteAttrNameList);

            // Write header, delete file if first package
            _reqIf = AddHeader(ImportModuleFile, newFile:subModuleIndex==0);
            _reqIfContent = _reqIf.CoreContent.SingleOrDefault();

            WriteModule(Pkg);

            CreateSpecObjects();
            CreateSpecHierarchy();

            // serialize ReqIF
            string fileReqIf = Path.Combine(Zip.CreateTempDir(), Path.GetFileName(ImportModuleFile));
            SerializeReqIf(fileReqIf, compress:false);
            Compress(ImportModuleFile, Path.GetDirectoryName(fileReqIf));
            return true;


        }
        /// <summary>
        /// Create Specification with hierachy
        /// </summary>
        private void CreateSpecHierarchy()
        {
            var reqIfContent = _reqIf.CoreContent.SingleOrDefault();
            var parent = _moduleSpecification;
            foreach (EA.Element el in Pkg.Elements)
            {
                var idSpecObject = $"specObj{ReqIfUtils.IdFromGuid(el.ElementGUID)}";
                var specObject = reqIfContent.SpecObjects.SingleOrDefault(x => x.Identifier == idSpecObject);
                var specHierarchy = new SpecHierarchy()
                {
                    Identifier = $"speHierarchy{ReqIfUtils.IdFromGuid(el.ElementGUID)}",
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
        }

        private void CreateSpecHierarchyElement(SpecHierarchy parent, EA.Element el)
        {
            var idSpecObject = $"specObj{ReqIfUtils.IdFromGuid(el.ElementGUID)}";
            var specObject = _reqIfContent.SpecObjects.SingleOrDefault(x => x.Identifier == idSpecObject);
            var specHierarchy = new SpecHierarchy()
            {
                Identifier = $"speHierarchy{ReqIfUtils.IdFromGuid(el.ElementGUID)}",
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
        private void CreateSpecObjects()
        {
            var reqIfContent = _reqIf.CoreContent.SingleOrDefault();
            using (var db = new EaDataModel(_provider, _connectionString))
            {
                var reqs = from r in db.t_object
                    join pkg in db.t_package on r.Package_ID equals pkg.Package_ID
                    join tv in db.t_objectproperties on r.Object_ID equals tv.Object_ID
                    where pkg.ea_guid == Pkg.PackageGUID
                    orderby r.Name, tv.Property
                    select new {Name=r.Name,ModifiedOn=r.ModifiedDate, CreatedOn = r.CreatedDate, Author=r.Author, Guid =r.ea_guid, Desc=r.Note,  TvName= tv.Property, TvValue=tv.Value, TvNote=tv.Notes};
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
                            Identifier = $"specObj{ReqIfUtils.IdFromGuid(r.Guid)}",  
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
                            TheValue = (DateTime)r.CreatedOn
                        };
                        specObject.Values.Add(attributeValueDate);
                        // Created on
                        attributeValueDate = new AttributeValueDate
                        {
                            Definition =
                                (AttributeDefinitionDate)_specObjectType.SpecAttributes.SingleOrDefault(x =>
                                    x.GetType() == typeof(AttributeDefinitionDate) && x.LongName == "ReqIF.ForeignModifiedOn"),
                            TheValue = (DateTime)r.ModifiedOn
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
                        attributeValueXhtml = new AttributeValueXHTML
                        {
                            Definition =
                                (AttributeDefinitionXHTML) _specObjectType.SpecAttributes.SingleOrDefault(x =>
                                    x.GetType() == typeof(AttributeDefinitionXHTML) && x.LongName == "ReqIF.Name"),
                            TheValue = MakeXhtmlFromString(r.Name)
                        };
                        specObject.Values.Add(attributeValueXhtml);
                        // Attribute Text
                        attributeValueXhtml = new AttributeValueXHTML
                        {
                            Definition =
                                (AttributeDefinitionXHTML)_specObjectType.SpecAttributes.SingleOrDefault(x =>
                                    x.GetType() == typeof(AttributeDefinitionXHTML) && x.LongName == "ReqIF.Text"),
                            TheValue = MakeXhtmlFromString(Rep,r.Desc)
                        };
                        specObject.Values.Add(attributeValueXhtml);


                        reqIfContent.SpecObjects.Add(specObject);


                    }
                    // Add Tagged Value
                    // Check if enumeration
                    var dataTypeEnumeration = (DatatypeDefinitionEnumeration)reqIfContent.DataTypes.SingleOrDefault(x => x.GetType() == typeof(DatatypeDefinitionEnumeration)
                                                                                                                         && x.LongName == r.TvName);
                    if (dataTypeEnumeration == null)
                    {
                        var attributeValueXhtml = new AttributeValueXHTML
                        {
                            Definition =
                                (AttributeDefinitionXHTML)_specObjectType.SpecAttributes.SingleOrDefault(x =>
                                    x.GetType() == typeof(AttributeDefinitionXHTML) && x.LongName == r.TvName),
                            TheValue = MakeXhtmlFromString(Rep, ReqIfUtils.GetEaTaggedValue(r.TvValue,r.TvNote))
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
                        SetReqIfEnumValue(attributeValueEnumeration, r.TvValue, attributeDefinitionEnumeration.IsMultiValued);
                        specObject.Values.Add(attributeValueEnumeration);
                        
                    }
                    
                }
            }

        }

        /// <summary>
        /// Add Data Types for Enums
        /// </summary>
        /// <param name="pkg"></param>
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
                var reqIfContent = reqIf.CoreContent.SingleOrDefault();
                foreach (var tv in tvProperties)
                {
                    // Check if Enumeration-Datatype already exists
                    var dataTypeEnumeration = (DatatypeDefinitionEnumeration)reqIfContent.DataTypes.FirstOrDefault(x => x.GetType() == typeof(DatatypeDefinitionEnumeration)
                                                                                                                        && x.LongName == tv.Property);
                    if (dataTypeEnumeration != null) continue;
                    string idEnum = ReqIfUtils.MakeIdReqIfConform($"_{tv.Property}");
                    var datatypeDefinitionEnumeration = new DatatypeDefinitionEnumeration
                    {
                        Description = $"{tv.Notes}",
                        Identifier = idEnum,
                        LastChange = DateTime.Now,
                        LongName = $"{tv.Property}"
                        
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
                                OtherContent = value,

                            };
                            var enumValue = new EnumValue
                            {
                                Identifier = ReqIfUtils.MakeIdReqIfConform($"{idEnum}_{value}"),
                                LastChange = DateTime.Now,
                                LongName = value,
                                Properties = embeddedValue
                            };
                            // Add enum value
                            datatypeDefinitionEnumeration.SpecifiedValues.Add(enumValue);
                        }
                        
                    }
                    reqIfContent.DataTypes.Add(datatypeDefinitionEnumeration);

                }

            }

        }



        /// <summary>
        /// Write the ReqIF module for the pkg
        /// </summary>
        /// <param name="pkg"></param>
        private void WriteModule([NotNull]EA.Package pkg)
        {
            var reqIfContent = Enumerable.SingleOrDefault<ReqIFContent>(_reqIf.CoreContent);

            // Module specification
            _moduleSpecification = new Specification
            {
                Description = $"Module specification of Package '{pkg.Name}', GUID={pkg.PackageGUID}",
                Identifier = ReqIfUtils.IdFromGuid(pkg.PackageGUID),
                LastChange = DateTime.Now,
                LongName = $"Module {pkg.Name}",
                Type = _specificationTypeModule ?? // Find the general SpecificationType for modules
                       (SpecificationType)reqIfContent.SpecTypes.SingleOrDefault(x => x.GetType() == typeof(SpecificationType) &&
                                                                                      x.Identifier == SpecificationTypeModuleId)
            };
            reqIfContent.Specifications.Add(_moduleSpecification);

            AddAttributesModuleSpecification(_moduleSpecification, pkg);
            // Add datatypes for packages (enums)
            AddDatatypesForPackage(_reqIf, pkg);
            // AddSpecObj type for package/module
            AddSpeObjectTypeForModule(_reqIf, pkg);

        }

        /// <summary>
        /// Add Spec object type for module/package
        /// </summary>
        /// <param name="reqIf"></param>
        /// <param name="pkg"></param>
        private void AddSpeObjectTypeForModule(ReqIF reqIf, EA.Package pkg)
        {
            var reqIfContent = reqIf.CoreContent.SingleOrDefault();
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
                Identifier = $"attr_ReqIF.Name{ReqIfUtils.IdFromGuid(pkg.PackageGUID)}",
                LastChange = DateTime.Now,
                Type = (DatatypeDefinitionXHTML) reqIfContent.DataTypes.SingleOrDefault(x =>
                    x.GetType() == typeof(DatatypeDefinitionXHTML))
            };
            _specObjectType.SpecAttributes.Add(attributeDefinitionXhtml);
            attributeDefinitionXhtml = new AttributeDefinitionXHTML
            {
                LongName = "ReqIF.ChapterName",
                Identifier = $"attr_ReqIF.ChapterName{ReqIfUtils.IdFromGuid(pkg.PackageGUID)}",
                LastChange = DateTime.Now,
                Type = (DatatypeDefinitionXHTML)reqIfContent.DataTypes.SingleOrDefault(x =>
                    x.GetType() == typeof(DatatypeDefinitionXHTML))
            };
            _specObjectType.SpecAttributes.Add(attributeDefinitionXhtml);
            attributeDefinitionXhtml = new AttributeDefinitionXHTML
            {
                LongName = "ReqIF.Text",
                Identifier = $"attr_ReqIF.Text{ReqIfUtils.IdFromGuid(pkg.PackageGUID)}",
                LastChange = DateTime.Now,
                Type = (DatatypeDefinitionXHTML)reqIfContent.DataTypes.SingleOrDefault(x =>
                    x.GetType() == typeof(DatatypeDefinitionXHTML))
            };
           _specObjectType.SpecAttributes.Add(attributeDefinitionXhtml);
            attributeDefinitionXhtml = new AttributeDefinitionXHTML
            {
                LongName = "ReqIF.ForeignModifiedBy",
                Identifier = $"attr_ReqIF.ModifiedBy{ReqIfUtils.IdFromGuid(pkg.PackageGUID)}",
                LastChange = DateTime.Now,
                Type = (DatatypeDefinitionXHTML)reqIfContent.DataTypes.SingleOrDefault(x =>
                    x.GetType() == typeof(DatatypeDefinitionXHTML))
            };
            _specObjectType.SpecAttributes.Add(attributeDefinitionXhtml);
            attributeDefinitionXhtml = new AttributeDefinitionXHTML
            {
                LongName = "ReqIF.ForeignCreatedBy",
                Identifier = $"attr_ReqIF.CreatedBy{ReqIfUtils.IdFromGuid(pkg.PackageGUID)}",
                LastChange = DateTime.Now,
                Type = (DatatypeDefinitionXHTML)reqIfContent.DataTypes.SingleOrDefault(x =>
                    x.GetType() == typeof(DatatypeDefinitionXHTML))
            };
            _specObjectType.SpecAttributes.Add(attributeDefinitionXhtml);

            var attributeDefinitionDate = new AttributeDefinitionDate()
            {
                LongName = "ReqIF.ForeignCreatedOn",
                Identifier = $"attr_ReqIF.CreatedOn{ReqIfUtils.IdFromGuid(pkg.PackageGUID)}",
                LastChange = DateTime.Now,
                Type = (DatatypeDefinitionDate)reqIfContent.DataTypes.SingleOrDefault(x =>
                    x.GetType() == typeof(DatatypeDefinitionDate))
            };
            _specObjectType.SpecAttributes.Add(attributeDefinitionDate);
            attributeDefinitionDate = new AttributeDefinitionDate()
            {
                LongName = "ReqIF.ForeignModifiedOn",
                Identifier = $"attr_ReqIF.ModifiedOn{ReqIfUtils.IdFromGuid(pkg.PackageGUID)}",
                LastChange = DateTime.Now,
                Type = (DatatypeDefinitionDate)reqIfContent.DataTypes.SingleOrDefault(x =>
                    x.GetType() == typeof(DatatypeDefinitionDate))
            };
            _specObjectType.SpecAttributes.Add(attributeDefinitionDate);
            var attributeDefinitionBool = new AttributeDefinitionBoolean()
            {
                LongName = "ReqIF.ForeignDeleted",
                Identifier = $"attr_ReqIF.Deleted{ReqIfUtils.IdFromGuid(pkg.PackageGUID)}",
                LastChange = DateTime.Now,
                Type = (DatatypeDefinitionBoolean)reqIfContent.DataTypes.SingleOrDefault(x =>
                    x.GetType() == typeof(DatatypeDefinitionBoolean))
            };
            _specObjectType.SpecAttributes.Add(attributeDefinitionBool);
            var attributeDefinitionString = new AttributeDefinitionString()
            {
                LongName = "ReqIF.ForeignId",
                Identifier = $"attr_ReqIF.ForeignId{ReqIfUtils.IdFromGuid(pkg.PackageGUID)}",
                LastChange = DateTime.Now,
                Type = (DatatypeDefinitionString)reqIfContent.DataTypes.SingleOrDefault(x =>
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
                    // Check if enum (Type=Enum; or Type=Checklist;)
                    //var dataTypeEnumeration = (DatatypeDefinitionEnumeration)reqIfContent.DataTypes.FirstOrDefault(x => x.GetType() == typeof(DatatypeDefinitionEnumeration)
                    //                                                                                                     && x.LongName == tvName);
                    var dataTypeEnumeration = (DatatypeDefinitionEnumeration)reqIfContent.DataTypes.SingleOrDefault(x => x.GetType() == typeof(DatatypeDefinitionEnumeration)
                                                                                                                        && x.LongName == tvName);
                    if (dataTypeEnumeration == null)
                    {
                        attributeDefinitionXhtml = new AttributeDefinitionXHTML
                        {
                            LongName = $"{tvName}",
                            Identifier = ReqIfUtils.MakeIdReqIfConform($"attr{pkg.Name}_{tvName}"),
                            LastChange = DateTime.Now,
                            Type = (DatatypeDefinitionXHTML) reqIfContent.DataTypes.SingleOrDefault(x =>
                                x.GetType() == typeof(DatatypeDefinitionXHTML))
                        };
                        _specObjectType.SpecAttributes.Add(attributeDefinitionXhtml);

                    }
                    else
                    { // Enumeration
                        
                       
                       var attributeDefinitionEnumeration = new AttributeDefinitionEnumeration
                            {
                                LongName = $"{tvName}",
                                Identifier = ReqIfUtils.MakeIdReqIfConform($"attr{pkg.Name}_{tvName}"),
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
            reqIfContent.SpecTypes.Add(_specObjectType);


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

                // Deserialize
                ReqIFDeserializer deserializer = new ReqIFDeserializer();
                return deserializer.Deserialize(file);
            }
            else
            {
                var reqIf = new ReqIF();
                reqIf.Lang = "en";
                var header = new ReqIFHeader()
                {
                    Title = $"{_settings.Name}",
                    CreationTime = DateTime.Now,
                    Identifier = "reqifheader",
                    RepositoryId = Rep.ProjectGUID,
                    ReqIFToolId = $"hoReverse V{Assembly.GetExecutingAssembly().GetName().Version}",
                    ReqIFVersion = "1.2",
                    SourceToolId = $"SPARX EA Lib={Rep.LibraryVersion}",
                    Comment = $"Export EA from {Rep.ConnectionString}"
                };
                reqIf.TheHeader.Add(header);

                // Content
                var reqIfContent = new ReqIFContent();
                reqIf.CoreContent.Add(reqIfContent);

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
        private void AddCoreDataTypes(ReqIF reqIf)
        {
            var reqIfContent = reqIf.CoreContent.SingleOrDefault();

            var datatypeDefinitionBoolean =
                new DatatypeDefinitionBoolean
                {
                    Description = "boolean data type definition",
                    Identifier = "boolean",
                    LastChange = DateTime.Now,
                    LongName = "a boolean"
                };
            reqIfContent.DataTypes.Add(datatypeDefinitionBoolean);

            var datatypeDefinitionDate = new DatatypeDefinitionDate
            {
                Description = "date data type definition",
                Identifier = "DateTime",
                LastChange = DateTime.Now,
                LongName = "a date"
            };
            reqIfContent.DataTypes.Add(datatypeDefinitionDate);

            var datatypeDefinitionInteger =
                new DatatypeDefinitionInteger
                {
                    Description = "integer data type definition",
                    Identifier = "integer",
                    LastChange = DateTime.Parse("2015-12-01"),
                    LongName = "an integer",
                    Min = -2147483648,
                    Max = 2147483647
                };
            reqIfContent.DataTypes.Add(datatypeDefinitionInteger);

            var datatypeDefinitionString = new DatatypeDefinitionString
            {
                Description = "string data type definition",
                Identifier = "string",
                LastChange = DateTime.Now,
                MaxLength = 32000,
                LongName = "a string"
            };
            reqIfContent.DataTypes.Add(datatypeDefinitionString);

            var datatypeDefinitionXhtml = new DatatypeDefinitionXHTML
            {
                Description = "string data type definition",
                Identifier = "xhtml",
                LastChange = DateTime.Now,
                LongName = "a string"
            };
            reqIfContent.DataTypes.Add(datatypeDefinitionXhtml);

        }
        /// <summary>
        /// Create and add the module <see cref="SpecificationType"/> with attribute definitions
        /// </summary>
        private SpecificationType AddSpecificationTypeModule(ReqIF reqIf)
        {
            var reqIfContent = reqIf.CoreContent.SingleOrDefault();

            var specificationTypeModule = new SpecificationType
            {
                LongName = SpecificationTypeModuleName,
                Identifier = SpecificationTypeModuleId,
                LastChange = DateTime.Now

            };
            //AddSpecTypeAttributesCore(specificationTypeModule, reqIfContent);

            //AddSpecTypeAttributesCore(specificationType, reqIfContent);

            AddAttributeDefinitionsToModuleSpecificationType(specificationTypeModule, reqIfContent);
            reqIfContent.SpecTypes.Add(specificationTypeModule);
            return specificationTypeModule;
        }


        /// <summary>
        /// Add Attribut definitions to SpecificationType of Module
        /// </summary>
        /// <param name="specType"></param>
        /// <param name="reqIfContent"></param>
        private void AddAttributeDefinitionsToModuleSpecificationType(SpecType specType, ReqIFContent reqIfContent)
        {
            var attributeDefinitionReqIfName = new AttributeDefinitionXHTML
            {
                LongName = "ReqIF.Name",
                Identifier = "specification-reqif-name",
                LastChange = DateTime.Now,
                Type = (DatatypeDefinitionXHTML) reqIfContent.DataTypes.SingleOrDefault(x =>
                    x.GetType() == typeof(DatatypeDefinitionXHTML))
            };
            specType.SpecAttributes.Add(attributeDefinitionReqIfName);

            var attributeDefinitionReqIfDescription = new AttributeDefinitionXHTML
            {
                LongName = "ReqIF.Description",
                Identifier = "specification-reqif-description",
                LastChange = DateTime.Now,
                Type = (DatatypeDefinitionXHTML) reqIfContent.DataTypes.SingleOrDefault(x =>
                    x.GetType() == typeof(DatatypeDefinitionXHTML))
            };
            specType.SpecAttributes.Add(attributeDefinitionReqIfDescription);

            var attributeDefinitionReqICreatedBy = new AttributeDefinitionXHTML
            {
                LongName = "ReqIF.ForeignCreatedBy",
                Identifier = "specification-reqif-createdby",
                LastChange = DateTime.Now,
                Type = (DatatypeDefinitionXHTML)reqIfContent.DataTypes.SingleOrDefault(x =>
                    x.GetType() == typeof(DatatypeDefinitionXHTML))
            };
            specType.SpecAttributes.Add(attributeDefinitionReqICreatedBy);
            var attributeDefinitionReqIModifiedBy = new AttributeDefinitionXHTML
            {
                LongName = "ReqIF.ForeignModifiedBy",
                Identifier = "specification-reqif-modifiedby",
                LastChange = DateTime.Now,
                Type = (DatatypeDefinitionXHTML)reqIfContent.DataTypes.SingleOrDefault(x =>
                    x.GetType() == typeof(DatatypeDefinitionXHTML))
            };
            specType.SpecAttributes.Add(attributeDefinitionReqIModifiedBy);

            var attributeDefinitionReqIModifiedOn = new AttributeDefinitionDate()
            {
                LongName = "ReqIF.ForeignModifiedOn",
                Identifier = "specification-reqif-modified-on",
                LastChange = DateTime.Now,
                Type = (DatatypeDefinitionDate)reqIfContent.DataTypes.SingleOrDefault(x =>
                    x.GetType() == typeof(DatatypeDefinitionDate))
            };
            specType.SpecAttributes.Add(attributeDefinitionReqIModifiedOn);

            var attributeDefinitionReqCreatedOn = new AttributeDefinitionDate()
            {
                LongName = "ReqIF.ForeignCreatedOn",
                Identifier = "specification-reqif-created-on",
                LastChange = DateTime.Now,
                Type = (DatatypeDefinitionDate)reqIfContent.DataTypes.SingleOrDefault(x =>
                    x.GetType() == typeof(DatatypeDefinitionDate))
            };
            specType.SpecAttributes.Add(attributeDefinitionReqCreatedOn);


            var attributeDefinitionReqPrefix = new AttributeDefinitionXHTML()
            {
                LongName = "ReqIF.Prefix",
                Identifier = "specification-reqif-prefix",
                LastChange = DateTime.Now,
                Type = (DatatypeDefinitionXHTML)reqIfContent.DataTypes.SingleOrDefault(x =>
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
                TheValue = MakeXhtmlFromString(Rep, pkg.Name)
            };
            specElementWithAttributes.Values.Add(attributeValueXhtml);

            // Description
            attributeValueXhtml = new AttributeValueXHTML
            {
                Definition = (AttributeDefinitionXHTML) specType.SpecAttributes.SingleOrDefault(x =>
                    x.GetType() == typeof(AttributeDefinitionXHTML) &&
                    x.Identifier == "specification-reqif-description"),
                    TheValue = MakeXhtmlFromString(Rep, pkg.Notes)
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
        /// Add core attributes to SpecType.
        /// </summary>
        /// <param name="specType"></param>
        /// <param name="reqIfContent"></param>
        private void AddSpecTypeAttributesCore(SpecType specType, ReqIFContent reqIfContent)
        {
            var attributeDefinitionBoolean = new AttributeDefinitionBoolean();
            attributeDefinitionBoolean.LongName = "boolean attribute";
            attributeDefinitionBoolean.Identifier = "specification-boolean-attribute";
            attributeDefinitionBoolean.LastChange = DateTime.Now;
            attributeDefinitionBoolean.Type = (DatatypeDefinitionBoolean)reqIfContent.DataTypes.SingleOrDefault(x => x.GetType() == typeof(DatatypeDefinitionBoolean));
            specType.SpecAttributes.Add(attributeDefinitionBoolean);

            var attributeDefinitionDate = new AttributeDefinitionDate();
            attributeDefinitionDate.LongName = "date attribute";
            attributeDefinitionDate.Identifier = "specification-date-attribute";
            attributeDefinitionDate.LastChange = DateTime.Now;
            attributeDefinitionDate.Type = (DatatypeDefinitionDate)reqIfContent.DataTypes.SingleOrDefault(x => x.GetType() == typeof(DatatypeDefinitionDate));
            specType.SpecAttributes.Add(attributeDefinitionDate);

            var attributeDefinitionInteger = new AttributeDefinitionInteger();
            attributeDefinitionInteger.LongName = "integer attribute";
            attributeDefinitionInteger.Identifier = "specification-integer-attribute";
            attributeDefinitionInteger.LastChange = DateTime.Now;
            attributeDefinitionInteger.Type = (DatatypeDefinitionInteger)reqIfContent.DataTypes.SingleOrDefault(x => x.GetType() == typeof(DatatypeDefinitionInteger));
            specType.SpecAttributes.Add(attributeDefinitionInteger);

            var attributeDefinitionString = new AttributeDefinitionString();
            attributeDefinitionString.LongName = "string attribute";
            attributeDefinitionString.Identifier = "specification-string-attribute";
            attributeDefinitionString.LastChange = DateTime.Now;
            attributeDefinitionString.Type = (DatatypeDefinitionString)reqIfContent.DataTypes.SingleOrDefault(x => x.GetType() == typeof(DatatypeDefinitionString));
            specType.SpecAttributes.Add(attributeDefinitionString);

            var attributeDefinitionXhtml = new AttributeDefinitionXHTML();
            attributeDefinitionXhtml.LongName = "xhtml attribute";
            attributeDefinitionXhtml.Identifier = "specification-xhtml-attribute";
            attributeDefinitionXhtml.LastChange = DateTime.Now;
            attributeDefinitionXhtml.Type = (DatatypeDefinitionXHTML)reqIfContent.DataTypes.SingleOrDefault(x => x.GetType() == typeof(DatatypeDefinitionXHTML));
            specType.SpecAttributes.Add(attributeDefinitionXhtml);
        }
    }
}
