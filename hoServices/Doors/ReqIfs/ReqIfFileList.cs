using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ReqIFSharp;

namespace EaServices.Doors.ReqIfs
{

    /// <summary>
    /// Inventory all reqIF files and their specifications/Modules
    /// - File
    /// - Specifications with
    /// -- Identifier
    /// -- Longname
    /// </summary>
    public class ReqIfFileList
    {
        private readonly List<ReqIfFileItem> _reqIfFileItemList;

        public List<ReqIfFileItem> ReqIfFileItemList
        {
            get => _reqIfFileItemList;
        }

        /// <summary>
        /// Inventory current reqIF list
        /// - FileName
        /// - ReqIF Content Index
        /// - ReqIF Specification Index
        /// - ReqIF Specification ID
        /// - ReqIF Specification LongName


        /// </summary>
        /// <param name="fileList"></param>
        public ReqIfFileList(string[] fileList)
        {
            _reqIfFileItemList = new List<ReqIfFileItem>();
            foreach (var reqIfFile in fileList)
            {
                ReqIF reqIf = ReqIf.DeSerializeReqIf(reqIfFile, false);
                if (reqIf == null) continue; // skip reqIF
                int specContentIndex = 0;
                // over all contents
                foreach (var cont in reqIf.CoreContent)
                {
                    int specIndex = 0;
                    // overall specifications
                    foreach (var spec in reqIf.CoreContent[specIndex].Specifications)
                    {
                        _reqIfFileItemList.Add(new ReqIfFileItem(reqIfFile, specContentIndex, specIndex, spec.Identifier, spec.LongName ));
                        // Next specification
                        specIndex += 1;
                    }
                    // Next Content
                    specContentIndex += 1;
                }

            }
            if (_reqIfFileItemList.Count == 0)
            {
                MessageBox.Show("", @"Errors in ReqIF, break");
            }
        }
        /// <summary>
        /// Get the ReqIF Item
        /// </summary>
        /// <param name="specId"></param>
        /// <returns></returns>
        public ReqIfFileItem GetItemForReqIfId(string specId)
        {
            var items = _reqIfFileItemList.Where(id => id.SpecId == specId).ToArray();
            if (! items.Any())
            {
                MessageBox.Show($@"Id={specId}", @"Can't find ReqIF Specification ID");
                return null;
            }
            if (items.Count() > 1) 
            {
                MessageBox.Show($@"Id={specId}", @"ReqIF Specification ID more than once found");
                return null;
            }

            return items[0];
        }

    }
}
