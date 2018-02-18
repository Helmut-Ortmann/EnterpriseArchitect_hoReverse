using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

// ReSharper disable once CheckNamespace
namespace hoReverse.hoUtils.Extension
{
    /// <summary>
    /// Extension Methods for EA Repository
    /// </summary>
    public static class EaExtensionClass
    {
      
        /// <summary>
        /// Returns a list of connectors. The query has to select ea_guid (the only column) of the needed connectors.
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="sql">SQL which select ea_guid</param>
        /// <returns>List of connectors</returns>
        // ReSharper disable once UnusedMember.Global
        public static List<EA.Connector> GetConnectorsBySql(this EA.Repository rep, string sql)
        {

            var lCon = new List<EA.Connector>();
            // run query into XDocument to proceed with LinQ
            string xml =rep.SQLQuery(sql);
            var x = new XDocument(XDocument.Parse(xml));

            // get ea_guid from descendants of 
            var node = from row in x.Descendants("Row").Descendants()
                         select row;

            foreach (var row in node)
            {
                EA.Connector con = rep.GetConnectorByGuid(row.Value);
                lCon.Add(con);
            }
    
            return lCon;
        }
        /// <summary>
        /// Returns a list of strings of the query with one column.
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="sql">SQL which one column</param>
        /// <returns>List of strings</returns>
        public static List<string> GetStringsBySql(this EA.Repository rep, string sql)
        {

            var lCon = new List<string>();
            // run query into XDocument to proceed with LinQ
            string xml = rep.SQLQuery(sql);
            var x = new XDocument(XDocument.Parse(xml));

            // get ea_guid from descendants of 
            var node = from row in x.Descendants("Row").Descendants()
                       select row;

            foreach (var row in node)
            {
                lCon.Add(row.Value);
            }

            return lCon;
        }

       
        /// <summary>
        /// Return true if Element is an Embedded Element Type
        /// - Port
        /// - Activity Parameter
        /// - Parameter
        /// - ExpansionNode
        /// - Pin
        /// </summary>
        /// <param name="el"></param>
        /// <returns>EA Version</returns>
        // ReSharper disable once UnusedMember.Global
        public static bool IsEmbeddedElement(this EA.Element el)
        {
            return el.Type == "Port" ||
                   el.Type == "ActivityParameter" ||
                   el.Type == "Parameter" ||
                   el.Type == "ExpansionNode" ||
                   el.Type == "ActionPin";
        }
        /// <summary>
        /// Make an IEnumerable from EA collection to easily use LINQ
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="container"></param>
        /// <returns></returns>
        public static IEnumerable<T> ToEnumerable<T>(this EA.Collection container)
        {
            for (short j = 0; j < container.Count; j++)
            {
                T item = (T) container.GetAt(j);
                yield return item;
            }
            
        }


    }
}
