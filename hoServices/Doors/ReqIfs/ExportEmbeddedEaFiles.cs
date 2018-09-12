using System.IO;
using hoUtils.DirFile;

namespace EaServices.Doors.ReqIfs
{
    public class ExportEmbeddedEaFiles
    {
        readonly string _filesRoot;
        readonly string _embeddedFileImages ;
        readonly string _embeddedFiles;
        readonly string _nameSpace;

        public ExportEmbeddedEaFiles(string nameSpace, string filesRoot, string embeddedFileImages= @"EmbeddedFileImages", string embeddedFiles = @"EmbeddedFiles")
        {
            _embeddedFileImages = embeddedFileImages;
            _embeddedFiles = embeddedFiles;
            _nameSpace = nameSpace;
            _filesRoot = filesRoot;

        }
        /// <summary>
        /// Returns xhtml string of embedded files and copies the embedded files. 
        /// </summary>
        /// <param name="el"></param>
        /// <returns></returns>
        public string MakeXhtmlForEmbeddedFiles(EA.Element el)
        {
            
            string xhtml = "<br><br>Embedded Files:<br>";
            foreach (EA.File file in el.Files)
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
                // copy embedded files
                DirFiles.FileCopy(file.Name, Path.Combine(_filesRoot, _embeddedFiles));


                xhtml = xhtml + $@"
<object data=""{Path.Combine(_embeddedFiles, fileName)}"" type=""{mimeType}"">
    <object data=""{Path.Combine(_embeddedFileImages, imagePath)}"" type=""image/png"">{defaultText}
    </object>
</object>
";
//<{_nameSpace}:object data=""{Path.Combine(_embeddedFiles, fileName)}"" type=""{mimeType}"">
//    <{_nameSpace}:object data=""{Path.Combine(_embeddedFileImages, imagePath)}"" type=""image/png"">{defaultText}
//    </{_nameSpace}:object>
//</{_nameSpace}:object>
//";
            }
            // make all paths with slash
            return $"{xhtml.Replace(@"\", @"/")}<br><br>";
        }
        /// <summary>
        /// Copy embedded Files
        /// </summary>
        /// <param name="el"></param>
        public void CopyEmbeddedFiles(EA.Element el)
        {
            foreach (EA.File file in el.Files)
            {
                // copy embedded files
                DirFiles.FileCopy(file.Name, Path.Combine(_filesRoot, _embeddedFiles));
            }
        }
    }
}
