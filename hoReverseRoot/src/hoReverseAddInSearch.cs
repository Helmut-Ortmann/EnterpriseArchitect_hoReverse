using System;
using System.Collections.Generic;
using System.Data;
using hoLinqToSql.LinqUtils;
using System.Windows.Forms;
using EA;

namespace hoReverse
{
    /// <summary>
    /// Partial class for Add-In Searches
    /// </summary>
    public partial class hoReverseRoot
    {
        private Dictionary<string, string> _tv = new Dictionary<string, string>();

        /// <summary>
        /// Add-In Search: Sample
        /// See: http://sparxsystems.com/enterprise_architect_user_guide/13.5/automation/add-in_search.html
        /// hoTools:
        /// https://github.com/Helmut-Ortmann/EnterpriseArchitect_hoTools/wiki/AddInModelSearch        
        ///
        /// Configure New Search of type Add-In Search:
        /// - hoReverse.AddInSearchObjectsNested
        /// - Configuration errors are shown in output
        /// 
        /// How it's works:
        /// 1. Create a Table and fill it with your code
        /// 2. Adapt LINQ to output the table (powerful)
        ///    -- Where to select only certain rows
        ///    -- Order By to order the result set
        ///    -- Grouping
        ///    -- Filter
        ///    -- JOIN
        ///    -- etc.
        /// 3. Deploy and test 
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="searchText"></param>
        /// <param name="xmlResults"></param>
        /// <returns></returns>
        public object AddInSearchObjectsNested(EA.Repository repository, string searchText, out string xmlResults)
        {
            // 1. Collect data into a data table
            DataTable dt = AddinSearchObjectsNested_SetTable(repository, searchText);
            // 2. Order, Filter, Join, Format to XML
            xmlResults = AddinSearchObjectsNested_MakeXml(dt);
            return "ok";
        }
        /// <summary>
        /// Test Query to show making EA xml from a Data table by using MakeXml. It queries the data table, orders the content according to Name columen and outputs it in EA xml format
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        private string AddinSearchObjectsNested_MakeXml(DataTable dt)
        {
            try
            {
                // Make a LINQ query (WHERE, JOIN, ORDER,)
                //OrderedEnumerableRowCollection<DataRow> rows = from row in dt.AsEnumerable()
                EnumerableRowCollection<DataRow> rows = from row in dt.AsEnumerable()
                    select row;

                return Xml.MakeXml(dt, rows);
            }
            catch (Exception e)
            {
                MessageBox.Show($@"{e}", @"Error LINQ query Test query to show Table to EA xml format");
                return "";

            }
        }
        private DataTable AddinSearchObjectsNested_SetTable(EA.Repository rep, string searchText)
        {
            _tv = new Dictionary<string, string>();
            DataTable dt = new DataTable();
            dt.Columns.Add("CLASSGUID", typeof(string));
            dt.Columns.Add("CLASSTYPE", typeof(string));
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("Alias", typeof(string));
            dt.Columns.Add("Note", typeof(string));

            if (searchText.Trim().StartsWith("{"))
            {
                EA.Package pkg = rep.GetPackageByGuid(searchText);
                if (pkg != null)
                {
                    NestedPackage(dt, pkg);
                }
            }
            else
            {
                rep.GetContextItem(out var item);
                if (item is Element element) NestedElements(dt, element);
                if (item is Package package) NestedPackage(dt, package);
            }
            return dt;
        }

        private void NestedPackage(DataTable dt, EA.Package pkg)
        {
            foreach (EA.Element el in pkg.Elements)
            {
                NestedElements(dt, el);
            }

        }

        private void NestedElements(DataTable dt, EA.Element el)
        {

            var row = dt.NewRow();
            row["CLASSGUID"] = el.ElementGUID;
            row["CLASSTYPE"] = el.Type;
            row["Name"] = el.Name;
            row["Alias"] = el.Alias;
            row["Note"] = el.Notes;
            AddTaggedValues(dt, el, row);
            dt.Rows.Add(row);

            foreach (EA.Element elChild in el.Elements)
            {

                NestedElements(dt, elChild);
            }

        }

        private void AddTaggedValues(DataTable dt, EA.Element el, DataRow dataRow)
        {
            foreach (EA.TaggedValue tv in el.TaggedValues)
            {
                if (!_tv.ContainsKey(tv.Name))
                {
                    _tv.Add(tv.Name, null);
                    dt.Columns.Add(tv.Name, typeof(string));
                }

                string value = tv.Value;
                if (value.StartsWith("<memo>")) value = tv.Notes;
                dataRow[$"{tv.Name}"] = value;
            }
        }
    }
}
