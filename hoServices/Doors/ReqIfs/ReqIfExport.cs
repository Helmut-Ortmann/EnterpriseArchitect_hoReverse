﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using DataModels;
using hoUtils.DirFile;
using JetBrains.Annotations;
using ReqIFSharp;

namespace EaServices.Doors.ReqIfs
{
    public partial class ReqIf
    {
        // Identifier for export
        const string SpecificationTypeModuleId = @"specificationTypeModule";
        const string SpecificationTypeModuleName = @"SpecificationTypeModule";
        // Modulspecifisch, has to be estimated from ReqIF for a new module
        SpecificationType _specificationTypeModule;

        const string SpecificationTypePrefixId = @"specificationType";
        const string SpecificationTypePrefixName = @"SpecificationType";
        // Modulspecifisch, has to be estimated from ReqIF for a new module
        SpecificationType _specificationType;



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
            _reqIf = AddHeader(ImportModuleFile, subModuleIndex == 0);

            WriteModule(Pkg);

            // Export ReqIF SpecObjects stored in EA
            foreach (EA.Element el in Pkg.Elements)
            {
                _level = 0;
                //if (!UpdateReqIfForElementRecursive(el)) return false;
            }

            // serialize ReqIF
            return SerializeReqIf(ImportModuleFile);
        }
        /// <summary>
        /// Add Data Types for Enums
        /// </summary>
        /// <param name="pkg"></param>
        private void GetTaggedValueTypes(ReqIF reqIf, EA.Package pkg)
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
            var moduleSpecification = new Specification
            {
                Description = $"Module specification of Package '{pkg.Name}', GUID={pkg.PackageGUID}",
                Identifier = ReqIfUtils.IdFromGuid(pkg.PackageGUID),
                LastChange = DateTime.Now,
                LongName = $"Module {pkg.Name}",
                Type = _specificationTypeModule ?? // Find the general SpecificationType for modules
                       (SpecificationType)reqIfContent.SpecTypes.SingleOrDefault(x => x.GetType() == typeof(SpecificationType) &&
                                                                                      x.Identifier == SpecificationTypeModuleId)
            };
            reqIfContent.Specifications.Add(moduleSpecification);

            AddAttributesModuleSpecification(moduleSpecification, pkg);

            GetTaggedValueTypes(_reqIf, pkg);

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
                string importReqIfFile = Decompress(file);
                if (String.IsNullOrWhiteSpace(importReqIfFile)) return null;

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
