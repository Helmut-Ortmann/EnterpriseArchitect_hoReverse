using System.Collections.Generic;
using System.Globalization;
using System.Linq;

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
            //select new { Name = n[0], Macro = n.Length == 1 ? "" : n[1] }).ToList();//.ToList<ExportFieldItem>();

        }
        /// <summary>
        /// returns true if the tagged Value with the name 'name' is writable.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool IsWritableValue(string name)
        {
            return _fields.SingleOrDefault(x => x.Split('=')[0] == name)!=null;
        }
        /// <summary>
        /// Get Macro value of the fiels
        /// </summary>
        /// <param name="el"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public string GetMacroValue(EA.Element el, string fieldName)
        {
            var macro = (from field in _fields
                let n = field.Split('=')
                where n.Length > 1 &&
                      n[0] == fieldName
                select n[1]).FirstOrDefault();
            if (macro == null) return "";
           
            switch (macro)
            {
                case "EA.GUID":
                    return el.ElementGUID;
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
