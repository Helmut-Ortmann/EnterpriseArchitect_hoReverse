using System;
using System.IO;
using System.Windows.Forms;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text.RegularExpressions;
using OpenMcdf;

namespace EaServices.Doors
{
    /// <summary>
    /// To convert DOORS OLE files from Microsoft CFB format to a format readable by the target application like *.pdf, Excel, Word.
    /// 
    /// Doors OLE files are in Microsoft CFB format to store embedded files. With *.pdf files e.g. it works to rename the file in *.pdf and to start it.
    /// For Excel it looks as it doesn't work or I haven't understood it correctly.
    /// </summary>
    public static class OleDoors
    {
        // magic Doc File header
        // check this for more:
        // http://social.msdn.microsoft.com/Forums/en-US/343d09e3-5fdf-4b4a-9fa6-8ccb37a35930/developing-a-tool-to-recognise-ms-office-file-types-doc-xls-mdb-ppt-
        // https://www.developerfusion.com/article/84406/com-structured-storage-from-net/

        // Define other methods and classes here
        private const string Header = "d0cf11e0";

        /// <summary>
        /// Save DOORS ole file. Only supported files.
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

            int start = text.IndexOf(Header, StringComparison.Ordinal);
            if (start < 0)
            {
                MessageBox.Show($@"File: '{filePath}'
CFB Header should be: '{Header}'
No CFB Header detected", $@"File does not contain a CFB formatted file., break");
                return "";
            }

            int end = text.IndexOf('}', start);
            if (end < 0)
            {
                end = text.Length;
            }

            // find object type of OLE
            Regex rxObjectType = new Regex(@"objclass\s+([^}]*)}");
            Match match = rxObjectType.Match(text.Substring(0, 70));
            string typeText = "";
            if (match.Success) typeText = match.Groups[1].Value;

            // DOORS file types supported and tested
            string[] lTypes = {"AcroExch." };//,"Excel.", "Word."};
            string[] lExtensions = { ".pdf"};//, "xlsx", "docx" };
            string[] lCompoundFileStreamName = {"CONTENTS"};
            int j = 0;
            string ext = "";
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
            // Extension not supported
            if (ext == "")
            {
                if (ignoreNotSupportedFiles)
                    return filePath;
                else
                {
                    MessageBox.Show($@"File: '{filePath}'
File type not supported: '{typeText}'

Supported ole types: '{String.Join(", ", lTypes)}'


", @"Can't convert *.ole to file, not supported type!");

                    return filePath;
                }

            }
            string filePathNew = Path.Combine(Path.GetDirectoryName(filePath), $"{Path.GetFileNameWithoutExtension(filePath)}{ext}");


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
                        b = (byte)(16 * GetHexValue(c));
                    }
                    else
                    {
                        b |= GetHexValue(c);
                        bytes.WriteByte(b);
                    }
                    highByte = !highByte;
                }
                File.WriteAllBytes(filePathNew, bytes.ToArray());
                // By DOORS supported file type
                if (componentFileStreamName != "")
                {
                    using (CompoundFile cf = new CompoundFile(filePathNew))
                    {
                        CFStream foundStream = cf.RootStorage.GetStream("CONTENTS");
                        hoUtils.DirFile.DirFiles.WriteAllBytes(filePathNew, foundStream.GetData().ToArray());
                    }
                   
                }

                if (hoUtils.DirFile.DirFiles.FileDelete(filePath)) return filePathNew;
                else return "";


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
            MessageBox.Show(@"CFB file contains unexpected character", $@"GetHexValue: unexpected character.");
            return (byte)' ';

        }
    }
}
