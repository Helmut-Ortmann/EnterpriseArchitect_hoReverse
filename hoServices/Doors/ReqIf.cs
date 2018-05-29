using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using hoReverse.hoUtils;
using ReqIFSharp;

namespace EaServices.Doors
{
    public class ReqIf : DoorsModule
    {
        readonly FileImportSettingsItem _settings;

        public ReqIf(EA.Repository rep, EA.Package pkg, string importFile, FileImportSettingsItem settings) : base(rep, pkg, importFile)
        {
            _settings = settings;
        }
         /// <summary>
        /// Import and update Requirements.
        /// </summary>
        /// async Task
        /// public override async Task ImportUpdateRequirements(string eaObjectType = "Requirement",
        public override void ImportUpdateRequirements(string eaObjectType = "Requirement",
            string eaStereotype = "",
            string stateNew = "",
            string stateChanged = "")
        {
            Rep.BatchAppend = true;
            Rep.EnableUIUpdates = false;

            // Deserialize
            ReqIFDeserializer deserializer = new ReqIFDeserializer();
            var reqIf = deserializer.Deserialize(ImportModuleFile);

            InitializeReqIfRequirementsTable(reqIf);

            foreach (Specification el in reqIf.CoreContent[0].Specifications)
            {
                AddRequirements(DtRequirements, el.Children,1);
            }


            ReadPackageRequirements();
            CreatePackageDeletedObjects();

            

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

                string alias = CombineAttrValues(_settings.AliasList, row, 40);
                string name = CombineAttrValues(_settings.AttrNameList, row, 40);
                string notes = GetAttrValue(notesColumn != "" ? row[notesColumn].ToString(): row[1].ToString());
                string nameShort = GetAttrValue(name.Length > 40 ? name.Substring(0, 40) : name);

               // Check if requirement with Doors ID already exists
                bool isExistingRequirement = DictPackageRequirements.TryGetValue(objectId, out int elId);


                EA.Element el;
                if (isExistingRequirement)
                {
                    el = Rep.GetElementByID(elId);
                    if (el.Alias != alias ||
                        el.Name != nameShort ||
                        el.Notes != notes )
                    {
                        if (stateChanged != "") el.Status = stateChanged;
                        CountChanged += 1;
                    }
                }
                else
                {
                    el = (EA.Element)Pkg.Elements.AddNew(name, "Requirement");
                    if (stateNew != "") el.Status = stateNew;
                    CountChanged += 1;
                }


                el.Alias = alias;
                el.Name = name;
                el.Multiplicity = CombineAttrValues(_settings.IdList, row, 40);
                el.Notes = notes;
                el.TreePos = Count * 10;
                el.PackageID = Pkg.PackageID;
                el.ParentID = parentElementId;
                el.Type = eaObjectType;
                el.Stereotype = eaStereotype;

                el.Update();
                Pkg.Elements.Refresh();
                lastElementId = el.ElementID;

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

                UpdateLinkedDocument(el, rtfValue);
                
                // Update/Create Tagged value
                foreach (var c in cols)
                {
                   if (notesColumn != c.Name) TaggedValue.SetUpdate(el, c.Name, GetAttrValue(c.Value??""));                     
                    
                }
            }

            MoveDeletedRequirements();
            UpdatePackage(reqIf);

            Rep.BatchAppend = false;
            Rep.EnableUIUpdates = true;
            Rep.ReloadPackage(Pkg.PackageID);
        }


        /// <summary>
        /// Update Package with Standard and ReqIF Properties
        /// </summary>
        /// <param name="reqIf"></param>
        protected void UpdatePackage(ReqIF reqIf)
        {
            EA.Element el = Rep.GetElementByGuid(Pkg.PackageGUID);
            GetModuleProperties(reqIf, el);

            base.UpdatePackage();
        }

        /// <summary>
        /// Update EA linked document with xhtmlValue. 
        /// </summary>
        /// <param name="el">EA Element to update linked document</param>
        /// <param name="xhtmlValue">The XHTML value in XHTML or flat string format</param>
        /// <returns></returns>
        protected bool UpdateLinkedDocument(EA.Element el, string xhtmlValue)
        {
            // Handle *.rtf content
            string docFile = $"{System.IO.Path.GetDirectoryName(ImportModuleFile)}";
            bool IsGenerateDocx = true;
            if (IsGenerateDocx)
                docFile = System.IO.Path.Combine(docFile, "xxxxxxx.docx");
            else
                docFile = System.IO.Path.Combine(docFile, "xxxxxxx.rtf");

            if (docFile.EndsWith(".rtf"))
                HtmlToDocx.ConvertSautin(docFile, xhtmlValue);
            else HtmlToDocx.Convert(docFile, xhtmlValue);
            try
            {
                bool res = el.LoadLinkedDocument(docFile);
                if (!res)
                    MessageBox.Show($@"ImportFile: '{ImportModuleFile}'
Id: '{el.Multiplicity}'
Name: '{el.Name}'
Err: '{el.GetLastError()}'
RtfDocxFile:'{docFile}

XHTML:'{xhtmlValue}",
                        @"Error loading Linked Document, break current requirement, continue!");
            }
            catch (Exception e)
            {
                MessageBox.Show($@"ImportFile: '{ImportModuleFile}'
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
        /// Initialize ReqIF Requirement DataTable
        /// </summary>
        /// <param name="reqIf"></param>
        private void InitializeReqIfRequirementsTable(ReqIF reqIf)
        {
            // Initialize table
            // Standard columns
            DtRequirements = new DataTable();
            DtRequirements.Columns.Add("Id", typeof(string));
            DtRequirements.Columns.Add("Object Level", typeof(string));

            // get list of all used attributes
            var blackList = new String[] { };// {"TableType", "TableBottomBorder", "TableCellWidth", "TableChangeBars","TableLeftBorder","TableLinkIndicators","TableRightBorder","TableShowAttrs","TableTopBorder"};
            var qAttr = (from obj in reqIf.CoreContent[0].SpecObjects
                from attr in obj.Values
                where ! blackList.Any(bl=>bl == attr.AttributeDefinition.LongName)
				
                select new { Name = attr.AttributeDefinition.LongName, Type=attr.AttributeDefinition.DatatypeDefinition.ToString()}).Distinct();
           
            // over all Attributes
            foreach (var attr in qAttr)
            {
                DtRequirements.Columns.Add(attr.Name, typeof(string));
            }
        }


        /// <summary>
        /// Add ObjSpec childrens to DataTable, recursive with 
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="children"></param>
        /// <param name="level"></param>
        private void AddRequirements(DataTable dt, List<SpecHierarchy> children, int level)
        {
            if (children == null || children.Count == 0) return;
            foreach (SpecHierarchy child in children)
            {
                DataRow row = dt.NewRow();
                SpecObject specObject = child.Object;
                row["Id"] = specObject.Identifier;
                row["Object Level"] = level;

                List<AttributeValue> columns = specObject.Values;
                foreach (var column in columns)
                {
                    try
                    {   // Handle enums
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
                        continue;
                    }
                }

                dt.Rows.Add(row);
                AddRequirements(dt, child.Children, level + 1);
            }
            

        }
        /// <summary>
        /// Add Tagged Values with Module Properties to Package/Object
        /// </summary>
        /// <param name="reqIf"></param>
        /// <param name="pkg"></param>
        private void GetModuleProperties(ReqIF reqIf, EA.Element el)
        {
            var moduleProperties = from obj in reqIf.CoreContent[0].Specifications[0].Values
                select new { Value = obj.ObjectValue.ToString(), Name = obj.AttributeDefinition.LongName, Type = obj.AttributeDefinition.GetType() };
            foreach (var property in moduleProperties)
            {
                TaggedValue.SetUpdate(el, property.Name, GetAttrValue(property.Value ?? ""));
            }
        }

        /// <summary>
        /// Get Attribute Value
        /// </summary>
        /// <param name="attrValue"></param>
        /// <returns></returns>
        private string GetAttrValue(string attrValue)
        {
            if (attrValue.Contains("http://www.w3.org/1999/xhtml"))
                return HtmlToText.ConvertReqIfXhtml(attrValue);
            else return attrValue;

        }

        /// <summary>
        /// Get Attribute value for a list of attribute names. Limit the output length if length > 0
        /// </summary>
        /// <param name="lNames"></param>
        /// <param name="row"></param>
        /// <param name="length">The leghth of the output string. Default:40</param>
        /// <returns></returns>
        private string CombineAttrValues(List<string> lNames, DataRow row, int length=0)
        {
            string attrValue = lNames.Count == 0 ? GetAttrValue(row[0].ToString()):"";
            string delimeter = "";
            foreach (var columnName in lNames)
            {

                try
                {
                    attrValue = $"{attrValue}{delimeter}{GetAttrValue(row[columnName].ToString())}";
                }
                catch (Exception e)
                {
                    MessageBox.Show($"Attribute name:\r\n{columnName}\r\n\r\n{e}", "Can't read Attribute!");
                }

                delimeter = " ";
            }
            // linit length
            return length > 0 && attrValue.Length > length 
                ? attrValue.Substring(0, length) 
                : attrValue;

        }
        /// <summary>
        /// Combine rtf Attribute values for a list of attribute names.
        /// </summary>
        /// <param name="lNames"></param>
        /// <param name="row"></param>
        /// <param name="length">The leghth of the output string. Default:40</param>
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
                    MessageBox.Show($"Attribute name:\r\n{columnName}\r\n\r\n{e}", "Can't read Attribute!");
                }

                delimeter = $@"<p><br><br><br></p>";
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
