using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DocumentFormat.OpenXml.Drawing;
using Path = System.IO.Path;

namespace hoUtils.DirFile
{
    public class DirFiles
    {
        /// <summary>
        /// Delete file and output error message
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public static bool FileDelete(string filePath, string errorMessage="Can't delete file, break!")
        {
            try
            {
                File.Delete(filePath);
            }
            catch (Exception e)
            {
                return FileErrorMessage(filePath, errorMessage, e);
            }

            return true;
        }
        /// <summary>
        /// Write all bytes
        /// </summary>
        /// <param name="file"></param>
        /// <param name="bytes"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public static bool WriteAllBytes(string file, byte[] bytes, string errorMessage="Cant write in file")
        {
            try
            {
                File.WriteAllBytes(file, bytes);
            }
            catch (Exception e)
            {
                return FileErrorMessage(file, errorMessage, e);
            }

            return true;
        }

        /// <summary>
        /// Make error Message for a file related error
        /// </summary>
        /// <param name="file"></param>
        /// <param name="errorMessage"></param>
        /// <param name="e"></param>
        /// <returns>false</returns>
        public static bool FileErrorMessage(string file,  string errorMessage, Exception e )
        {
            MessageBox.Show($@"File: '{Path.GetFileName(file)}'
Dictionary: '{Path.GetDirectoryName(file)}'

{e}", errorMessage);
            return false;

        }
    }
}
