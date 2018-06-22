using System;
using System.IO;
using System.Windows.Forms;

namespace EaServices.Doors
{
    /// <summary>
    /// To convert DOORS OLE files from Microsoft CFB format to a format readable by the target application like *.pdf, Excel, Word.
    /// 
    /// Doors OLE files are in Microsoft CFB format to store embedded files. With *.pdf files e.g. it works to rename the file in *.pdf and to start it.
    /// For Excel it looks as it doesn't work.
    /// </summary>
    public static class OleDoors
    {
        // magic Doc File header
        // check this for more: http://social.msdn.microsoft.com/Forums/en-US/343d09e3-5fdf-4b4a-9fa6-8ccb37a35930/developing-a-tool-to-recognise-ms-office-file-types-doc-xls-mdb-ppt-

        // Define other methods and classes here
        private const string Header = "d0cf11e0";

        public static bool Save(string text, string filePath)
        {
            if (text == null)
            {
                MessageBox.Show("", @"No string to convert found for OLE convertion, break");
                return false;
            }

            if (filePath == null) { 
                MessageBox.Show("", @"No filepath to convert OLE file to convertion, break");
                return false;
            }

            int start = text.IndexOf(Header, StringComparison.Ordinal);
            if (start < 0)
            {
                MessageBox.Show(@"CFB Header should be: '{Header}'", $@"File does not contain a CFB formatted file., break");
                return false;
            }

            int end = text.IndexOf('}', start);
            if (end < 0)
            {
                end = text.Length;
            }
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
                File.WriteAllBytes(filePath, bytes.ToArray());
            }

            return true;
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
