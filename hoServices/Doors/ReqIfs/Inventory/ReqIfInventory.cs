using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using hoLinqToSql.LinqUtils;
using ReqIFSharp;
using System.Data;
using System.Globalization;

namespace EaServices.Doors.ReqIfs.Inventory
{
    /// <summary>
    /// Inventory a *.reqifz file
    /// </summary>
    public class ReqIfInventory
    {
        /// <summary>
        /// Inventory file or directory with *.reqif and *.reqifz files
        /// </summary>
        /// <param name="file"></param>
        /// <param name="validate"></param>
        /// <returns></returns>
        public static DataTable Inventory(string file, bool validate=false)
        {
            // List of specifications to output
            List<InventoryItem> lSpec = new List<InventoryItem>();

            // File
            bool noError;
            if (File.Exists(file))
            {
                string fileExtension = Path.GetExtension(file).ToLower();
                if (fileExtension == ".reqifz") noError = InventoryReqIfzFile(lSpec, file, validate);
                if (fileExtension == ".reqif" || fileExtension == ".xml") noError = InventoryReqIfFile(lSpec, file, validate);
            }
            else
            {
                if (Directory.Exists(file)) InventoryDirectory(lSpec, file, validate);
            }
           
            return lSpec.OrderBy(x => x.Name).ToDataTable();

        }
        /// <summary>
        /// Inventory a single *.reqifz file
        /// </summary>
        /// <param name="lSpec"></param>
        /// <param name="reqIfzFile"></param>
        /// <param name="validate"></param>
        /// <returns></returns>
        private static bool InventoryReqIfzFile(List<InventoryItem> lSpec, string reqIfzFile, bool validate)
        {
            if (reqIfzFile.ToLower().EndsWith(".reqifz"))
            {
                ReqIf reqIf = new ReqIf();
                foreach (var file in reqIf.Decompress(reqIfzFile))
                {
                    if (Path.GetExtension(file).ToLower() == @".reqif" && Path.GetFileNameWithoutExtension(file).Trim() != "")
                        if (!InventoryReqIfFile(lSpec, file, validate)) return false;
                }
            }
            return true;
        }
        /// <summary>
        /// Returns a list of InventoryItem of the passed *.reqif file. A row for the *.reqif file.
        /// </summary>
        /// <param name="lSpec"></param>
        /// <param name="file"></param>
        /// <param name="validate"></param>
        /// <returns></returns>
        private static bool InventoryReqIfFile(List<InventoryItem> lSpec, string file, bool validate = false)
        {
            ReqIFDeserializer deserializer = new ReqIFDeserializer();
            try
            {
                var reqIf = deserializer.Deserialize(file, validate: validate);
                var info = (from core in reqIf.CoreContent
                        from s in core.Specifications
                        let count = (from spec in core.SpecObjects select spec.Identifier).Count()
                        let countLinks = (from link in core.SpecRelations select link.Target).Count()
                        select new InventoryItem(Path.GetFileName(file), s.Identifier, s.LongName, s.Description, s.LastChange.ToString(CultureInfo.InvariantCulture),
                            count, countLinks))
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
        /// <summary>
        /// Inventory a directory with *.reqif and *.reqifz files
        /// </summary>
        /// <param name="lSpec"></param>
        /// <param name="directory"></param>
        /// <param name="validate"></param>
        /// <returns></returns>
        private static bool InventoryDirectory(List<InventoryItem> lSpec, string directory, bool validate)
        {
            if (Directory.Exists(directory))
            {
                foreach (var file in Directory.GetFiles(directory, "*.reqif"))
                {
                    if (!InventoryReqIfFile(lSpec, file, validate)) return false;
                }
                foreach (var file in Directory.GetFiles(directory, "*.reqifz"))
                {
                    if (!InventoryReqIfzFile(lSpec, file, validate)) return false;
                }

            }
            return true;
        }

       
    }
}
