using System;
using System.IO;
using System.Windows.Forms;

namespace hoUtils
{
    public static class DirectoryExtension
    {
        
        /// <summary>
        /// Clear recursive the folder
        /// </summary>
        /// <param name="folderName"></param>
        /// <returns></returns>
        public static bool ClearFolder(string folderName)
        {
            DirectoryInfo dir = new DirectoryInfo(folderName);

            foreach (FileInfo fi in dir.GetFiles())
            {
                try
                {
                    fi.Delete();
                }
                catch (Exception e)
                {
                    MessageBox.Show($@"File: '{fi.Name}'

{e}", @"Error delete file");
                    return false;
                }
            }

            foreach (DirectoryInfo di in dir.GetDirectories())
            {
                ClearFolder(di.FullName);
                try
                {
                    di.Delete();
                }
                catch (Exception e)
                {
                    MessageBox.Show($@"Dictionary: '{di.Name}'

{e}", @"Error delete dictionary");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Copy Directories and their filed to target directory (recursive)
        /// </summary>
        /// <param name="sourceDirName"></param>
        /// <param name="destDirName"></param>
        /// <param name="copySubDirs">Recursive copy if true (default)</param>
        /// <param name="overwrite"></param>
        public static bool DirectoryCopy(
            string sourceDirName, string destDirName, bool copySubDirs=true, bool overwrite=false)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the source directory does not exist, throw an exception.
            if (!dir.Exists)
            {
                MessageBox.Show($@"dictionary: '{dir.Name}'
Dictionary to delete doesn't exists"
                    , @"Error delete dictionary");
                return false;
            }

            // If the destination directory does not exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }


            // Get the file contents of the directory to copy.
            FileInfo[] files = dir.GetFiles();

            foreach (FileInfo file in files)
            {
                // Create the path to the new copy of the file.
                string tempPath = Path.Combine(destDirName, file.Name);

                // Copy the file.
                try
                {
                    file.CopyTo(tempPath, overwrite);
                }
                catch (Exception e)
                {
                    MessageBox.Show($@"{tempPath}

{e}", @"Error copying file");
                }

            }

            // If copySubDirs is true, copy the subdirectories.
            if (copySubDirs)
            {

                foreach (DirectoryInfo subdir in dirs)
                {
                    // Create the subdirectory.
                    string tempPath = Path.Combine(destDirName, subdir.Name);

                    // Copy the subdirectories.
                    DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
                }
            }
            return true;
        }
        /// <summary>
        /// Get a temp directory.
        /// </summary>
        /// <returns></returns>
        public static string GetTempDir(string directory)
        {
            return Path.Combine(Path.GetTempPath(), directory);


        }
        /// <summary>
        /// Create a temp directory. Ensure that it is empty. 
        /// </summary>
        /// <returns></returns>
        public static string CreateTempDir(string directory)
        {
            string tempPath = GetTempDir(directory);
            CreateEmptyDir(GetTempDir(directory));
            return tempPath;

        }
        /// <summary>
        /// Create a empty directory.  
        /// </summary>
        /// <returns></returns>
        public static bool CreateEmptyDir(string directory)
        {
            // Delete directory if exists
            if (Directory.Exists(directory))
                Directory.Delete(directory, recursive: true);
            Directory.CreateDirectory(directory);
            return true;

        }

    }
}
