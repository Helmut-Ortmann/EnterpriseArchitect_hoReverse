using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using EA;

// ReSharper disable once CheckNamespace
namespace hoReverse.hoUtils.SQL
{

    public class UtilSql
    {
        readonly Repository _rep;
        public UtilSql(Repository rep)
        {
            _rep = rep;
        }
        public Boolean UserHasPermission(string userGuid, int permissionId)
        {
            string query = @"SELECT 'Group' As PermissionType " +
                        @"from (t_secgrouppermission p inner join t_secusergroup grp on (p.GroupID = grp.GroupID)) " +
                        @"where grp.UserID = '" + userGuid + "' " +
                        @"UNION " +
                        @"select 'User'  from t_secuserpermission p " +
                        @"where p.UserID = '" + userGuid + "' ;";

            string str = _rep.SQLQuery(query);
            XElement xelement = XElement.Parse(str);
            // something found????
            var result = xelement.Descendants("Row").Count() > 0;
            
            return result;
        }
        public Boolean IsConnectionAvailable(EA.Element srcEl, EA.Element trgtEl)
        {
            string sql = "SELECT Start_Object_ID  " +
                         " From t_connector " +
                         " where Start_Object_ID in ( {0},{1} ) AND " +
                         "       End_Object_ID in  ( {0},{1} ) ";
             string query = String.Format(sql, srcEl.ElementID, trgtEl.ElementID);

            
            string str = _rep.SQLQuery(query);
            XElement xelement = XElement.Parse(str);
            // something found????
            var result = xelement.Descendants("Row").Count() > 0;

            return result;
        }


        /// <summary>
        /// Get users of EA element
        /// - t_secuser
        /// </summary>
        /// <returns></returns>
        public List<string> GetUsers()
        {
            List<string> l = new List<string>();
            string query;
            if (_rep.IsSecurityEnabled)
            {
                // authors under security
                query = @"select UserLogin As [User] from t_secuser order by 1";
            }
            else {
                // all used authors
                query = @"select distinct Author As [User] from t_object order by 1";
            }
            string str = _rep.SQLQuery(query);
            XElement xelement = XElement.Parse(str);
            foreach (XElement xEle in xelement.Descendants("Row"))
            {
                l.Add(xEle.Element("User").Value);
            }

            return l;
        }

        /// <summary>
        /// Get list of strings from a SQL
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public List<string> GetListOfStringFromSql(string sql, string columnName)
        {
            List<string> l = new List<string>();
            string str = _rep.SQLQuery(sql);
            XElement xelement = XElement.Parse(str);
            foreach (XElement xEle in xelement.Descendants("Row"))
            {
                l.Add(xEle.Element(columnName).Value);
            }

            return l;
        }

        /// <summary>
        /// Get list of strings from a SQL
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static List<string> GetListOfStringFromSql(EA.Repository rep, string sql, string columnName)
        {
            List<string> l = new List<string>();
            string str = rep.SQLQuery(sql);
            XElement xelement = XElement.Parse(str);
            foreach (XElement xEle in xelement.Descendants("Row"))
            {
                l.Add(xEle.Element(columnName).Value);
            }

            return l;
        }

        public static bool SqlUpdate(EA.Repository rep, string updateString)
        {
            try
            {
                rep.Execute(updateString);
            }
            catch (Exception e)
            {
                MessageBox.Show($@"Update:\r\n{updateString}\r\n\r\n{e}", "Error update SQL");
                return false;
            }
            return true;
        }
        
    }
}
