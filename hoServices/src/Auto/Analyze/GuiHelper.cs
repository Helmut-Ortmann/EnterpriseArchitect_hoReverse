using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaServices.Auto.Analyze
{
    /// <summary>
    /// GUI Helper
    /// </summary>
    public static class GuiHelper
    {
        /// <summary>
        /// Aggregates the filter list an aggregated filter or to null (no filter)
        /// https://documentation.devexpress.com/WindowsForms/2567/Controls-and-Libraries/Data-Grid/Filter-and-Search/Filtering-in-Code
        /// </summary>
        /// <param name="lFilters"></param>
        /// <returns></returns>
        public static string AggregateFilter(List<string> lFilters)
        {

            string delimiter = "";
            string filters = "";
            foreach (var filter in lFilters)
            {
                filters = $@"{filters} {delimiter} {filter}";
                delimiter = " And ";
            }
            if (delimiter == "") return null;
            return filters;
        }
    }
}
