using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using hoLinqToSql.LinqUtils;
using ReqIFSharp;
using System.Data;

namespace EaServices.Doors.ReqIfs.Inventory
{
    /// <summary>
    /// Inventory a *.reqifz file
    /// </summary>
    public class ReqIfInventory
    {
        static public DataTable Inventory(string file, bool validate=false)
        {
            List<InventoryItem> lSpec = new List<InventoryItem>();
            bool noError = InventoryReqIfzFile(lSpec, file, validate);
            if (noError) noError = InventoryDirectory(lSpec, file, validate);
            if (noError)
            {
                // handle simple *.reqif file
                if (file.ToLower().EndsWith(".reqif")) noError = InventoryFile(lSpec, file, validate);
            }

            return lSpec.OrderBy(x => x.Name).ToDataTable();

        }

        static private bool InventoryReqIfzFile(List<InventoryItem> lSpec, string reqIfzFile, bool validate)
        {
            if (reqIfzFile.ToLower().EndsWith(".reqifz"))
            {
                ReqIf reqIf = new ReqIf();
                foreach (var file in reqIf.Decompress(reqIfzFile))
                {
                    if (!InventoryFile(lSpec, file, validate)) return false;
                };
            }
            return true;
        }
        private static bool InventoryDirectory(List<InventoryItem> lSpec, string directory, bool validate)
        {
            if (Directory.Exists(directory))
            {
                foreach (var file in Directory.GetFiles(directory, "*.reqif"))
                {
                    if (!InventoryFile(lSpec, file, validate)) return false;
                };
                foreach (var file in Directory.GetFiles(directory, "*.reqifz"))
                {
                    if (!InventoryReqIfzFile(lSpec, file, validate)) return false;
                };

            }
            return true;
        }

        /// <summary>
        /// Returns a list of InventoryItem of the passed *.reqifz file. An row for each contained *.reqif file.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="validate"></param>
        /// <returns></returns>
        private static bool InventoryFile(List<InventoryItem> lSpec, string file, bool validate = false) { 
            ReqIFDeserializer deserializer = new ReqIFDeserializer();
            try
            {
                var reqIf = deserializer.Deserialize(file, validate: validate);
                var info = (from core in reqIf.CoreContent
                        from s in core.Specifications
                        let count = (from spec in core.SpecObjects select spec.Identifier).Count()
                        let countLinks = (from link in core.SpecRelations select link.Target).Count()
                        select new InventoryItem(Path.GetFileName(file), s.Identifier, s.LongName, count, countLinks))
                    .ToArray();
                lSpec.AddRange(info);
                return true;
            }
            catch (Exception ex)
            {

                MessageBox.Show($@"Validation: {validate}

File: {file}

{ex}", "Error deserialize *.reqif file.", MessageBoxButton.OK);
                return false;
            }
        }
    }
}
