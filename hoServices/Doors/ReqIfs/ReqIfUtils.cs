using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace EaServices.Doors.ReqIfs
{
    public class ReqIfUtils
    {
        public enum ReqIfIdType 
        {
            ReqIfHeader, 
            SpecificationTypeModule,
            Specification,
            SpecObjectType,
            SpecObject,
            SpecHierarchy,
            Attribute,  // Package Name, Attributname/TaggedValue
            DataType,
            EnumValue
        }
        /// <summary>
        /// Make ReqIF ID  for a ReqIF Type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="string1"></param>
        /// <param name="string2"></param>
        /// <returns></returns>
        public static string MakeReqIfId(ReqIfIdType type, string string1="", string string2="")
        {
            switch (type)
            {
                case ReqIfIdType.Specification:
                    return MakeIdReqIfConform($"_specification_{string1}");
                case ReqIfIdType.SpecHierarchy:
                    return MakeIdReqIfConform($"_specHierarchy_{string1}");
                case ReqIfIdType.SpecObject:
                    return MakeIdReqIfConform($"_specObject_{string1}");
                case ReqIfIdType.SpecObjectType:
                    return MakeIdReqIfConform($"_specObjectType_{string1}");
                case ReqIfIdType.SpecificationTypeModule:
                    return MakeIdReqIfConform($"_specificationTypeModule");
                case ReqIfIdType.ReqIfHeader:
                    return MakeIdReqIfConform($"_header");
                case ReqIfIdType.Attribute:
                    return MakeIdReqIfConform($"_attr_{string1}_{string2}");
                   
                case ReqIfIdType.DataType:
                    return MakeIdReqIfConform($"_dataType_{string1}_{string2}");
                case ReqIfIdType.EnumValue:
                    return MakeIdReqIfConform($"_enumValue_{string1}_{string2}");
                default:
                    MessageBox.Show($@"ReqIF type: {type.ToString()}", @"Invalid ReqIF type");
                    return @"xxxxxxxxxxxxx";

            }
        }

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
