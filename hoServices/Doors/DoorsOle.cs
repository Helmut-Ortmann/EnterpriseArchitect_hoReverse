using System;
using System.IO;
using System.Windows.Forms;
using System.Linq;
using System.Text.RegularExpressions;
using OpenMcdf;
using hoUtils.DirFile;

namespace EaServices.Doors
{
    /// <summary>
    /// DOORS stores the ole files as a binary rtf file. There is the need to
    /// - recognize the header (currently CFB files or *.wmf files)
    /// - convert the content to ASCII file with the correct extension
    /// 
    /// To convert DOORS OLE files from Microsoft CFB and WMF format to a format readable by the target application like *.pdf, Excel, Word, WMF.
    /// 
    /// Doors OLE files are in Microsoft CFB format to store embedded files. With *.pdf files e.g. it works to rename the file in *.pdf and to start it.
    /// For Excel it looks as it doesn't work or I haven't understood it correctly.
    ///
    /// The other format I know is *.wmf files.
    /// </summary>
    public static class OleDoors
    {
        // magic Doc File header
        // check this for more:
        // http://social.msdn.microsoft.com/Forums/en-US/343d09e3-5fdf-4b4a-9fa6-8ccb37a35930/developing-a-tool-to-recognise-ms-office-file-types-doc-xls-mdb-ppt-
        // https://www.developerfusion.com/article/84406/com-structured-storage-from-net/
        // https://joseluisbz.wordpress.com/2013/07/26/exploring-a-wmf-file-0x000900/
        //
        // Parse *.rtf files:
        // https://www.codeproject.com/Articles/27431/Writing-Your-Own-RTF-Converter
        // https://github.com/decalage2/oletools/wiki

        // Define other methods and classes here
        private const string CfbHeader = "d0cf11e0";   //  Microsoft CFB format
        private const string WmfFile1Header = "01000900"; // Microsoft WMF format
        private const string WmfFile2Header = "02000900";  // Microsoft WMF format


        /// <summary>
        /// Save DOORS ole file in target format. If target format isn't supported the pure OLE file is stored.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="filePath"></param>
        /// <param name="ignoreNotSupportedFiles"></param>
        /// <returns></returns>
        public static string Save(string text, string filePath, bool ignoreNotSupportedFiles= true)
        {
            if (text == null)
            {
                MessageBox.Show("", @"No string to convert found for OLE convertion, break");
                return "";
            }

            if (filePath == null) { 
                MessageBox.Show("", @"No filepath to convert OLE file to convertion, break");
                return "";
            }

            // Check if header is available
            // if not use the file extension .error
            int startCfb = StartCfbFormat(text);
            int startWmf = StartWmfFormat(text);

            // no supported file found
            if (startCfb < 0 && startWmf < 0)
            {
                if (! ignoreNotSupportedFiles)
                    MessageBox.Show($@"File: '{filePath}'
CFB CfbHeader should be: '{CfbHeader}'
No CFB or WMF file found", @"File does not contain a CFB or WMF formatted file., break");
                string newFilePath = $@"{filePath}.error";
                DirFiles.FileMove(filePath, newFilePath);
                return newFilePath;
            }

            int start = startCfb > -1 ? startCfb : startWmf;
            int end = text.IndexOf('}', start);
            if (end < 0)
            {
                end = text.Length;
            }

            if (startCfb > -1)
            {
                // CFB File
                return StoreTargetFileFromCfb(text, filePath, ignoreNotSupportedFiles, start, end);
            }
            else
            {
                // WMF File
                return StoreTargetFileFromWmf(text, filePath, start, end);
            }
           
        }
        /// <summary>
        /// Store target file as *.wmf file
        /// </summary>
        /// <param name="text"></param>
        /// <param name="filePath"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        private static string StoreTargetFileFromWmf(string text, string filePath, int start, int end)
        {
            string ext = ".wmf";
            string filePathNew =
                Path.Combine(Path.GetDirectoryName(filePath), $"{Path.GetFileNameWithoutExtension(filePath)}{ext}");
            StoreAsAsciiFile(text, start, end, filePathNew);
            if (DirFiles.FileDelete(filePath)) return filePathNew;
            else return "";
        }

        /// <summary>
        /// Store the CFB file in target format
        /// </summary>
        /// <param name="text"></param>
        /// <param name="filePath"></param>
        /// <param name="ignoreNotSupportedFiles"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        private static string StoreTargetFileFromCfb(string text, string filePath, bool ignoreNotSupportedFiles, 
            int start, int end)
        {
            // find object type of OLE
            Regex rxObjectType = new Regex(@"objclass\s+([^}]*)}");
            Match match = rxObjectType.Match(text.Substring(0, 70));
            string typeText = "";
            string ext = "";
            if (match.Success) typeText = match.Groups[1].Value;

            // DOORS file types supported and tested
            string[] lTypes = {"AcroExch."}; //,"Excel.", "Word."};
            string[] lExtensions = {".pdf"}; //, "xlsx", "docx" };
            string[] lCompoundFileStreamName = {"CONTENTS"};
            int j = 0;
            string componentFileStreamName = "";
            foreach (var type in lTypes)
            {
                if (typeText.Contains(type))
                {
                    ext = lExtensions[j];
                    componentFileStreamName = lCompoundFileStreamName[j];
                    break;
                }

                j += 1;
            }

            if (ext == "")
            {
                string newFilePath = $@"{filePath}.notSupported";
                if (!ignoreNotSupportedFiles)
                {
                    MessageBox.Show($@"File: '{filePath}'
File type not supported: '{typeText}'

Supported ole types: '{String.Join(", ", lTypes)}'

Copied to:
{DirFiles.GetMessageFromFile(newFilePath)}

", @"Can't convert *.ole to file, not supported type!");
                }

                DirFiles.FileMove(filePath, newFilePath);
                return newFilePath;
            }

            string filePathNew =
                Path.Combine(Path.GetDirectoryName(filePath), $"{Path.GetFileNameWithoutExtension(filePath)}{ext}");


            StoreAsAsciiFile(text, start, end, filePathNew);

            // By DOORS supported file type
            if (componentFileStreamName != "")
            {
                using (CompoundFile cf = new CompoundFile(filePathNew))
                {
                    CFStream foundStream = cf.RootStorage.GetStream("CONTENTS");
                    DirFiles.WriteAllBytes(filePathNew, foundStream.GetData().ToArray());
                }
            }

            if (DirFiles.FileDelete(filePath)) return filePathNew;
            else return "";
        }

        private static void StoreAsAsciiFile(string text, int start, int end, string filePathNew)
        {
            using (MemoryStream bytes = new MemoryStream())
            {
                bool highByte = true;
                byte b = 0;
                for (int i = start; i < end; i++)
                {
                    char c = text[i];
                    if (char.IsWhiteSpace(c))
                        continue;
                    if (highByte)
                    {
                        b = (byte) (16 * GetHexValue(c));
                    }
                    else
                    {
                        b |= GetHexValue(c);
                        bytes.WriteByte(b);
                    }

                    highByte = !highByte;
                }

                DirFiles.WriteAllBytes(filePathNew, bytes.ToArray());
            }
        }

        private static byte GetHexValue(char c)
        {
            if (c >= '0' && c <= '9')
                return (byte)(c - '0');
            if (c >= 'a' && c <= 'f')
                return (byte)(10 + (c - 'a'));
            if (c >= 'A' && c <= 'F')
                return (byte)(10 + (c - 'A'));
            MessageBox.Show(@"CFB file contains unexpected character", @"GetHexValue: unexpected character.");
            return (byte)' ';

        }
        /// <summary>
        /// Returns the start in bytes of a wmf file embedded in *.rtf
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static int StartWmfFormat(string text)
        {
            text = text.Substring(0, 200);
            if (!text.StartsWith(@"{\pict\wmetafile8")) return -1;
            int start = text.IndexOf(WmfFile1Header, StringComparison.Ordinal);
            if (start > -1) return start;

            start = text.IndexOf(WmfFile2Header, StringComparison.Ordinal);
            if (start > -1) return start;
            return -1;
        }
        /// <summary>
        /// Returns the start in bytes of a cfb file embedded in *.rtf
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static int StartCfbFormat(string text)
        {
            text = text.Substring(0, 300);
            if (!text.StartsWith(@"{\object\objemb{")) return -1;
            int start = text.IndexOf(CfbHeader, StringComparison.Ordinal);
            if (start > -1) return start;
            return -1;
        }
    }
}
