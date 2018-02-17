using System;
using System.Text.RegularExpressions;
using System.Xml;

// ReSharper disable once CheckNamespace
namespace hoReverse.hoUtils
{
    public static class CallOperationAction
    {
        public static bool CreateCallAction(EA.Repository rep, EA.Element action, EA.Method method)
        {
            // add ClassifierGUID to target action
            string updateStr = @"update t_object set classifier_GUID = '" + method.MethodGUID +
                       "' where ea_guid = '" + action.ElementGUID + "' ";
            rep.Execute(updateStr);

            // set CallOperation
            string CallOperationProperty = "@PROP=@NAME=kind@ENDNAME;@TYPE=ActionKind@ENDTYPE;@VALU=CallOperation@ENDVALU;@PRMT=@ENDPRMT;@ENDPROP;";
            Guid g = Guid.NewGuid();
            string xrefid = "{" + g + "}";
            string insertIntoTXref = @"insert into t_xref 
                (XrefID,            Name,               Type,              Visibility, Namespace, Requirement, [Constraint], Behavior, Partition, Description, Client, Supplier, Link)
                VALUES('" + xrefid + "', 'CustomProperties', 'element property','Public', '','','', '',0, '" + CallOperationProperty + "', '" + action.ElementGUID + "', null,'')";
                rep.Execute(insertIntoTXref);

            // Link Call Operation to operation
                Guid.NewGuid();
            //rep.Execute(insertIntoT_xref);
              
            return true;
        }
        
        public static bool SetClassifierId(EA.Repository rep, EA.Element el, string guid)
        {
            // add ClassifierGUID to target action
            string updateStr = @"update t_object set classifier_GUID = '" + guid +
                       "' where ea_guid = '" + el.ElementGUID + "' ";
            rep.Execute(updateStr);
            return true;
        }

       

        /// <summary>
    /// Remove '{'...'}' 
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static string RemoveBody(string s)
        {
            return Regex.Replace(s, @"{[^}]*}", @"");

        }
        /// <summary>
        /// Get the body of '{'..'}'
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string GetBody(string s)
        {
            Regex rgx = new Regex(@"{([^})]*)}");
            Match match = rgx.Match(s);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            else return "";


        }

        public static string RemoveFirstParenthesisPairFromString(string s)
        {
            // delete first (
            // not for macros
            if (! s.Substring(0, 1).Equals("#"))
            {
                int i = s.IndexOf(@"(", 1, StringComparison.Ordinal);
                if (i >= 0)
                {
                    s = s.Remove(i, 1);
                    s = s.Substring(0, s.Length - 1);
                }
            }

            //s = Regex.Replace(s, @"(\()|(\))","");
            return s;
        }

        public static string AddQuestionMark(string s)
        {
            if (! s.Contains("end")) {
            if (s.Substring(s.Length - 1, 1) == "?") return s;
            s = s + "?";
            }

            return s;
        }
        // Remove module prefix from call string
        // modulePrefix_function(..) ==> function(..)
        public static string RemoveModuleNameFromCallString(string s)
        {
            Regex pattern = new Regex(@"([a-zA-Z][a-zA-Z_0-9]*_)[a-zA-Z_0-9]*\(");
            Match regMatch = pattern.Match(s);
            if (regMatch.Success | regMatch.Groups.Count > 1)
            {
                // delete 1 Group
                s = s.Replace(regMatch.Groups[1].Value, "");
            }
            return s;
        }
        public static string RemoveCasts(string s)
        {
            // remove casts
            if (!s.ToLower().Contains("sizeof("))
            {
                s = Regex.Replace(s, @"\([\s]*[a-zA-Z][a-zA-Z0-9_]+_t[\s]*[\*]*[\s]*[const]{0,5}[\s]*\)", ""); // delete casts
            }
            s = Regex.Replace(s, @"\([\s]*void[\s][\*]*\)", ""); // delete (void *) casts


            // delete 1223ul (type suffix)
            //s = Regex.Replace(s, @"([0-9])([uU][lL]*)", "");
            Regex pattern = new Regex(@"[^a-zA-Z_]([0-9])([uU][lL]*)");
            Match regMatch = pattern.Match(s);
            while (regMatch.Success)
            {
                string old = regMatch.Groups[1].Value;
                s = s.Replace(regMatch.Groups[1].Value + regMatch.Groups[2].Value, old);
                regMatch = regMatch.NextMatch();
            }
            //-------------------------------------------------------
            // remove type suffix

            // delete 1223u/123l
            //s = Regex.Replace(s, @"([0-9])([fFuUlL])", "");
            pattern = new Regex(@"[^a-zA-Z_]([0-9])([fFuUlL])");
            regMatch = pattern.Match(s);
            while (regMatch.Success)
            {
                string old = regMatch.Groups[1].Value;
                s = s.Replace(regMatch.Groups[1].Value + regMatch.Groups[2].Value, old);
                regMatch = regMatch.NextMatch();
            }

            // delete 0x012u
            pattern = new Regex(@"[^a-zA-Z_](0x[0-9AECDF]+)([uU])");
            regMatch = pattern.Match(s);
            while (regMatch.Success)
            {
                string old = regMatch.Groups[1].Value;
                s = s.Replace(regMatch.Groups[1].Value + regMatch.Groups[2].Value, old);
                regMatch = regMatch.NextMatch();
            }
            return s;
        }

        /// <summary>
        /// Remove:
        /// - Everything behind ";"
        /// - Everything behind "{" if not variable definition
        /// - Delete multiple blanks
        /// - Remove '\' at end of line (C-Macro)
        /// </summary>
        /// <param name="s"></param>
        /// <param name="deleteMultipleSpaces"></param>
        /// <param name="isLoop"></param>
        /// <returns></returns>
        public static string RemoveUnwantedStringsFromText(string s, bool deleteMultipleSpaces= true, bool isLoop=false)
        {
            s = RemoveCasts(s);
            

            if (! s.Contains("while"))
            { 
                // replace double "((" by "("
                //s = Regex.Replace(s, @"\(\(", "(");
                // replace double "))" by ")"
                //s = Regex.Replace(s, @"\)\)", ")");
            }
            // delete everything behind";"
            // not for a for loop
            if (!isLoop)
            {
                if (!(s.Contains("for ") || s.Contains("for(")) || s.Contains("while ") || s.Contains("do "))
                {
                    s = Regex.Replace(s, @";.*", "");
                    //s = Regex.Replace(s, @"\n", "\r\n");
                }
            }

            // delete everything behind"{" if not an variable assignment ' = {'
            Regex isAssignment = new Regex(@"=\s*{");
            if (! isAssignment.IsMatch(s) )
            {
                s = Regex.Replace(s, @"{.*", "");
                //s = Regex.Replace(s, @"\n", "\r\n");
            }

            // delete '\' at end of line
            s = s.Replace($@"\{Environment.NewLine}",$@"{Environment.NewLine}");

            if (deleteMultipleSpaces)
            {
                // delete multiple spaces
                s = Regex.Replace(s, @"  +", " ");
            }

            
           

            return s.Trim();
        }
        public static EA.Element GetElementFromName(EA.Repository rep, string elementName, string elementType)
        {
            EA.Element el = null;
            string query = @"select o.ea_guid AS EA_GUID
                      from t_object o 
                      where o.name = '" + elementName + "' AND " +
                            "o.Object_Type = '" + elementType + "' ";
            string str = rep.SQLQuery(query);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(str);

            XmlNode operationGuidNode = xmlDoc.SelectSingleNode("//EA_GUID");
            if (operationGuidNode != null)
            {
                string guid = operationGuidNode.InnerText;
                el = rep.GetElementByGuid(guid);
            }

            return el;
        }
        /// <summary>
        /// Get Method Name from call string xxxx() returns 'xxxx'
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string GetMethodNameFromCallString(string s)
        {
            Regex pattern = new Regex(@"[a-zA-Z_][a-zA-Z_0-9]+\s*\(");
            Match regMatch = pattern.Match(s);
            if (regMatch.Success)
            {
                // delete old string
                string s1 = regMatch.Value;
                return s1.Substring(0,s1.Length -1);
            }
            return "";
        }

        /// <summary>
        /// Find method from method name
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="methodName"></param>
        /// <param name="isNoExtern"></param>
        /// <returns></returns>
        public static EA.Method GetMethodFromMethodName(EA.Repository rep, string methodName, bool isNoExtern=false)
        {
            EA.Method method = null;
            string externStereotype = "";
            if (isNoExtern) externStereotype = " AND (stereotype = NULL OR stereotype <> 'extern')";
            string query = $@"select op.ea_guid AS EA_GUID
                                from t_operation op 
                                where op.name = '{methodName}' {externStereotype}";
            string str = rep.SQLQuery(query);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(str);

            XmlNode operationGuidNode = xmlDoc.SelectSingleNode("//EA_GUID");
            if (operationGuidNode != null)
            {
                string guid = operationGuidNode.InnerText;
                method = rep.GetMethodByGuid(guid);
            }

            return method;
        }
    }
}
