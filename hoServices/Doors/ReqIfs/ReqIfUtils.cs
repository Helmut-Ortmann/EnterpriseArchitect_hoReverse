using System;
using System.Text.RegularExpressions;

namespace EaServices.Doors.ReqIfs
{
    public class ReqIfUtils
    {
        /// <summary>
        /// Get ReqIF ID from EA guid
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public static string IdFromGuid(string guid)
        {
            return $"_{guid.Replace("{", "").Replace("}", "").Replace("-", "_")}";
        }
        /// <summary>
        /// Get Guid from id transformed by IdFromGuid
        /// </summary>
        /// <param name="reqIfIdentifier"></param>
        /// <returns></returns>
        public static string GuidFromId(string reqIfIdentifier)
        {
            //return $"_{guid.Replace("{", "").Replace("}", "").Replace("-", "_")}";
            return $@"{{{reqIfIdentifier.Substring(1).Replace("_", " - ")}}}";
        }
        /// <summary>
        /// Make id following 'xsd:ID' 
        /// se: http://www.datypic.com/sc/xsd/t-xsd_ID.html
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string MakeIdReqIfConform(string id)
        {
           id = Regex.Replace(id, "[^0-9a-zA-Z_.~]+", "_");
            if (!(Char.IsLetter(id[0]) || id[0] == '_')) id = $"_{id}";
            return id;
        }
        /// <summary>
        /// Get Tagged EA Tagged Value. It handles memo fileds
        /// </summary>
        /// <param name="value"></param>
        /// <param name="note"></param>
        /// <returns></returns>
        public static string GetEaTaggedValue(string value, string note)
        {
            return (value??"").StartsWith("<memo>") ? note??"" : value??"";
        }
    }
}
