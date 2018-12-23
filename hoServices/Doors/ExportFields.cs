using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace EaServices.Doors
{
    /// <summary>
    /// Handle Export fields of the form:
    /// Name=Macro
    /// </summary>
    public class ExportFields
    {
        private readonly List<string> _fields;
        /// <summary>
        /// Create Export fields of the form:
        /// Name=Macro
        /// </summary>
        /// <param name="fields"></param>
        public ExportFields(List<string> fields)
        {
            _fields = fields;
           
        }
        /// <summary>
        /// Get the fields to iterate 
        /// </summary>
        /// <returns></returns>
        public string[] GetFields()
        {
            return (from field in _fields
                let n = field.Split('=')
                select n[0]).ToArray();
        }
        /// <summary>
        /// returns true if the tagged Value with the name 'fieldName' is writable (write back during export/roundtrip).
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public bool IsWritableValue(string fieldName)
        {
            try
            {
                return _fields.FirstOrDefault(x => x.Split('=')[0] == fieldName) != null;
            }
            catch (Exception e)
            {
                MessageBox.Show($@"File Name: {fieldName}

{e}", @"Error determine IsWritable for an Attribute");
                return false;
            }

        }
        /// <summary>
        /// Returns the macro name of a field
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public string GetMacroName(string fieldName)
        {
            var macro = (from field in _fields
                let n = field.Split('=')
                where n.Length > 1 &&
                      n[0] == fieldName
                select n[1]).FirstOrDefault();
            return macro ?? "";
        }

        /// <summary>
        /// Get Macro value of the fiels
        /// </summary>
        /// <param name="el"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public string GetMacroValue(EA.Element el, string fieldName)
        {
            var macro = GetMacroName(fieldName);
            switch (macro)
            {
                case "":        // no macro found
                    return "";
                case "EA.GUID":
                    return el.ElementGUID;
                case "EA.Name":
                    return el.Name;
                case "EA.Type":
                    return el.Type;
                case "EA.Stereotype":
                    return el.Stereotype;
                case "EA.Version":
                    return el.Version;
                case "EA.Phase":
                    return el.Phase;
                case "EA.Status":
                    return el.Status;
                case "EA.Author":
                    return el.Author;
                case "EA.Modified":
                    return el.Modified.ToString(CultureInfo.CurrentCulture);
                case "EA.Created":
                    return el.Created.ToString(CultureInfo.CurrentCulture);
                default:
                    return $"unknown macro: '{macro}'";
            }   
             
        }
    }
}
