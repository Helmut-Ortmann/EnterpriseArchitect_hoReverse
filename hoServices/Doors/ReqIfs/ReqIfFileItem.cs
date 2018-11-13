using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaServices.Doors.ReqIfs
{
    /// <summary>
    /// Item to inventory reqIF files
    /// </summary>
    public class ReqIfFileItem
    {
        public string FilePath { get; set; }
        public string SpecId { get; set; }
        public string SpecLongName { get; set; }
        /// <summary>
        /// The index in the ReqIF.Content
        /// </summary>
        public int SpecIndex { get; set; }

        /// <summary>
        /// Create a ReqIfFileItem
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="specId"></param>
        /// <param name="specLongName"></param>
        /// <param name="specIndex"></param>
        public ReqIfFileItem(string filePath, string specId, string specLongName, int specIndex)
        {
            FilePath = filePath;
            SpecId = specId;
            SpecLongName = specLongName;
            SpecIndex = specIndex;
        }
    }
}
