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
        /// Copy file and output error message
        /// </summary>
        /// <param name="targetFilePath"></param>
        /// <param name="errorMessage"></param>
        /// <param name="sourceFilePath"></param>
        /// <returns></returns>
        public static bool FileCopy(string sourceFilePath, string targetFilePath, string errorMessage = "Can't copy file!")
        {
            try
            {
                File.Copy(sourceFilePath, targetFilePath,overwrite:true);
            }
            catch (Exception e)
            {
                MessageBox.Show($@"Source:
{GetMessageFromFile(sourceFilePath)}

Taget:
{GetMessageFromFile(targetFilePath)}

{e}", errorMessage);
                return false;
            }

            return true;
        }
        /// <summary>
        /// Copy file and output error message
        /// </summary>
        /// <param name="targetFilePath"></param>
        /// <param name="errorMessage"></param>
        /// <param name="sourceFilePath"></param>
        /// <returns></returns>
        public static bool FileMove(string sourceFilePath, string targetFilePath, string errorMessage = "Can't move file!")
        {
            try
            {
                File.Move(sourceFilePath, targetFilePath);
            }
            catch (Exception e)
            {
                MessageBox.Show($@"Source:
{GetMessageFromFile(sourceFilePath)}

Taget:
{GetMessageFromFile(targetFilePath)}

{e}", errorMessage);
                return false;
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
        public static bool WriteAllBytes(string file, byte[] bytes, string errorMessage="Cant write into file")
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
        public static bool FileErrorMessage(string file, string errorMessage, Exception e)
        {
            MessageBox.Show($@"{GetMessageFromFile(file)}

{e}", errorMessage);
            return false;
        }

        public static string GetMessageFromFile(string filePath)
        {
            if (String.IsNullOrWhiteSpace(filePath))
            {
                MessageBox.Show("", @"Empty file path, break");
                return "";
            }
            return $@"File: '{Path.GetFileName(filePath)}'
Dictionary: '{Path.GetDirectoryName(filePath)}'";

        }
    }
}
