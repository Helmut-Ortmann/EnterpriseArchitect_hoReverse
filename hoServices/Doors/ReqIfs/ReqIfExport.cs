using System;
using System.IO;
using System.Linq;
using System.Reflection;
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
        SpecificationType _specificationTypeModule = null;

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
        /// Write the ReqIF module for the pkg
        /// </summary>
        /// <param name="pkg"></param>
        private void WriteModule([NotNull]EA.Package pkg)
        {
            var reqIfContent = Enumerable.SingleOrDefault<ReqIFContent>(_reqIf.CoreContent);



            var moduleSpecification = new Specification
            {
                Description = $"Package name of EA Requirement Module, GUID={pkg.PackageGUID}",
                Identifier = pkg.PackageGUID,
                LastChange = DateTime.Now,
                LongName = pkg.Name,
                Type = _specificationTypeModule ?? 
                       (SpecificationType)reqIfContent.SpecTypes.SingleOrDefault(x => x.GetType() == typeof(SpecificationType) &&
                                                                                      x.Identifier == SpecificationTypeModuleId)
            };


            reqIfContent.Specifications.Add(moduleSpecification);

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
            //AddAttributeDefinitionsModule(specificationTypeModule, reqIfContent);

            //AddAttributeDefinitionsModule(specificationType, reqIfContent);
            reqIfContent.SpecTypes.Add(specificationTypeModule);
            return specificationTypeModule;
        }
        /// <summary>
        /// Add Attributes to SpecType.
        /// </summary>
        /// <param name="specType"></param>
        /// <param name="reqIfContent"></param>
        private void AddAttributeDefinitionsModule(SpecType specType, ReqIFContent reqIfContent)
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
