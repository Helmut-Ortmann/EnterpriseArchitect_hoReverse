using System;
using System.IO;
using System.IO.Compression;

namespace hoUtils.Compression
{
    public static class Zip
    {
        const string TempDirectory = "hoReqIf";

        /// <summary>
        /// Extract the zip file to an extract directory. If no extractDirectory exists a temp directory is created
        /// </summary>
        /// <param name="zipPath"></param>
        /// <param name="extractPath"></param>
        /// <returns></returns>
        public static string ExtractZip(string zipPath, string extractPath = "")
        {


            // estimate extract path for a temporary dictionars
            if (String.IsNullOrWhiteSpace(extractPath))
            {
                extractPath = Path.GetTempPath();
                extractPath = Path.Combine(extractPath, TempDirectory);
                if (Directory.Exists(extractPath))
                    Directory.Delete(extractPath,recursive:true);
                Directory.CreateDirectory(extractPath);
            }

            // Ensures that the last character on the extraction path
            // is the directory separator char. 
            // Without this, a malicious zip file could try to traverse outside of the expected
            // extraction path.
            if (!extractPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                extractPath += Path.DirectorySeparatorChar;
            using (ZipArchive archive = ZipFile.OpenRead(zipPath))
            {

                foreach (ZipArchiveEntry entry in archive.Entries)
                {

                    // Gets the full path to ensure that relative segments are removed.
                    string destinationPath = Path.GetFullPath(Path.Combine(extractPath, entry.FullName));

                    // Ordinal match is safest, case-sensitive volumes can be mounted within volumes that
                    // are case-insensitive.
                    if (destinationPath.StartsWith(extractPath, StringComparison.Ordinal))
                        entry.ExtractToFile(destinationPath);
                }

                return extractPath;
            }
        }

    }

}
