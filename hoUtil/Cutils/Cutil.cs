using System.Text.RegularExpressions;

namespace hoReverse.hoUtils.Cutils
{
    public static class Cutil
    {
        /// <summary>
        /// Delete all casts (only simple casts like (uint32))
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static string RemoveCasts(string code)
        {
            // delete casts
            
            var rx = @"\((sint64|sint32|sint16|sint8|uint64|uint32|uint16|uint8|float64|float32|size_t|ssize_t|boolean)\**\)";
            return Regex.Replace(code, rx,"");
        }
    }
}
