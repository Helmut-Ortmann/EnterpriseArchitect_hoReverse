using System.IO;
using hoUtils.DirFile;
using hoReverse.hoUtils;
using File = EA.File;

namespace EaServices.Doors.ReqIfs
{
    public class ExportEmbeddedEaFiles
    {
        readonly string _dirFilesRoot;
        readonly string _mimeTypeImages ;
        readonly string _embeddedFiles;

        /// <summary>
        /// Constructor export embedded files and create XHTM string for them. No namespace supported
        /// </summary>
        /// <param name="dirFilesRoot"></param>
        /// <param name="mimeTypeImages"></param>
        /// <param name="embeddedFiles"></param>
        public ExportEmbeddedEaFiles(string dirFilesRoot, string mimeTypeImages= @"MimeTypeImages", string embeddedFiles = @"EmbeddedFiles")
        {
            _mimeTypeImages = mimeTypeImages;
            _embeddedFiles = embeddedFiles;
            _dirFilesRoot = dirFilesRoot;

        }
        /// <summary>
        /// Returns xhtml string of the embedded files and copies the embedded files. 
        /// </summary>
        /// <param name="el"></param>
        /// <returns></returns>
        public string MakeXhtmlForEmbeddedFiles(EA.Element el)
        {
            
            string xhtml = "<br /><br />Embedded Files:<br />";
            // handle all file of Ea Element
            foreach (EA.File file in el.Files)
            {
                xhtml = RunFile(file, xhtml);
            }
            // make all paths with slash
            return $"{xhtml.Replace(@"\", @"/")}<br /><br />";
        }
        /// <summary>
        /// Run for file. Make XHTML and copy the file
        /// </summary>
        /// <param name="file"></param>
        /// <param name="xhtml"></param>
        /// <returns></returns>
        private string RunFile(File file, string xhtml)
        {
            string defaultText = Path.GetFileName(file.Name);

            string fileName = (Path.GetFileName(file.Name));
            string imagePath;
            string mimeType;
            switch (Path.GetExtension(file.Name))
            {
                case ".pdf":
                    mimeType = "application/pdf";
                    imagePath = "application-pdf.png";
                    break;
                case ".doc":
                    imagePath = "application-msword.png";
                    mimeType = "application/msword";
                    break;
                case ".docx":
                    imagePath = "application-msword.png";
                    mimeType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                    break;
                case ".xls":
                    imagePath = "application-vnd.ms-excel.png";
                    mimeType = "application/vnd.ms-excel";
                    break;
                case ".xlsx":
                    imagePath = "application-vnd.ms-excel.png";
                    mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    break;
                case ".ppt":
                    imagePath = "application-vnd.ms-powerpoint.png";
                    mimeType = "application/vnd.ms-powerpoint";
                    break;
                case "pptx":
                    imagePath = "application-vnd.ms-powerpoint.png";
                    mimeType = "application/vnd.openxmlformats-officedocument.presentationml.presentation";
                    break;
                default:
                    mimeType = "application/pdf";
                    imagePath = @"text-x-java.png";
                    defaultText = @"{defaultText}, not supported mime type/file type";
                    break;
            }

            // copy embedded file
            CopyEmbeddedFile(file.Name, Path.Combine(_dirFilesRoot, _embeddedFiles));


            xhtml = xhtml + $@"
<div>{fileName}</div>
<object data=""{Path.Combine(_embeddedFiles, fileName)}"" type=""{mimeType}"">
    <object data=""{Path.Combine(_mimeTypeImages, imagePath)}"" type=""image/png"">{defaultText}
    </object>
</object>
";
            return xhtml;
        }

        /// <summary>
        /// Copy embedded Files to target. The file may contain a local path by %id%
        /// </summary>
        /// <param name="el"></param>
        public void CopyEmbeddedFiles(EA.Element el)
        {
            foreach (EA.File file in el.Files)
            {
                // copy embedded files
                CopyEmbeddedFile(file.Name, Path.Combine(_dirFilesRoot, _embeddedFiles));
            }
        }
        /// <summary>
        /// Copy file and take local path into consideration
        /// </summary>
        /// <param name="file"></param>
        /// <param name="target"></param>
        private void CopyEmbeddedFile(string file, string target)
        {
            string fileName = HoUtil.GetFilePath("Linked File", file);
            DirFiles.FileCopy(fileName, target);

        }
    }
}
