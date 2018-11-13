using System.Collections.Generic;
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
        private List<ReqIfFileItem> _reqIfFileItemList;

        public ReqIfFileList(string[] fileList)
        {
            _reqIfFileItemList = new List<ReqIfFileItem>();
            foreach (var reqIfFile in fileList)
            {
                ReqIF reqIf = ReqIf.DeSerializeReqIf(reqIfFile, false);
                int specIndex = 0;
                foreach (var cont in reqIf.CoreContent)
                {
                    foreach (var spec in reqIf.CoreContent[specIndex].Specifications)
                    {
                        _reqIfFileItemList.Add(new ReqIfFileItem(reqIfFile, spec.Identifier, spec.LongName, specIndex));
                    }

                    specIndex += 1;
                }

            }
        }

    }
}
