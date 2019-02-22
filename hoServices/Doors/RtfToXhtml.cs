using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using hoUtils;
using SautinSoft;

namespace EaServices.Doors
{
    /// <summary>
    /// Rtf to XHTML conversion
    /// see: http://www.sautin.com/products/components/rtftohtml/index.php
    /// </summary>
    public class RtfToXhtml
    {
        /// <summary>
        /// Convert rtf to XHTML. It uses SautinSoft. You may need to have a license. 
        /// </summary>
        /// <param name="rtfText"></param>
        /// <param name="xhtmlDir">The directory to store images.</param>
        /// <param name="imageFileFolder"></param>
        /// <returns></returns>
        public static string Convert(string rtfText, string xhtmlDir, string imageFileFolder="Files")
        {
            // test purposes
            bool gen = true;

            string xhtml;

            // generate XHTML
            if (gen)
            {
                try
                {
                    RtfToHtml rtfGen = new RtfToHtml
                    {
                        OutputFormat = RtfToHtml.eOutputFormat.XHTML_10,
                        Serial = "10460301363", // Serial number developer license
                        Encoding = RtfToHtml.eEncoding.UTF_8
                    };
                    //specify image options
                    rtfGen.ImageStyle.ImageFolder = xhtmlDir; //this folder must exist
                    rtfGen.ImageStyle.ImageSubFolder = imageFileFolder; //this folder will be created by the component
                    rtfGen.ImageStyle.ImageFileName = "png"; //template name for images
                    rtfGen.ImageStyle.IncludeImageInHtml = false; //false - save images on HDD, true - save images inside HTML

                    xhtml = rtfGen.ConvertString(rtfText);
                }
                catch (Exception e)
                {
                    MessageBox.Show($@"{e}

Text to convert:
{rtfText}", @"Error rtf to xhtml converting");
                    xhtml = @"Can't convert EA rtf to ReqIF XHTML";
                }
            }
            else
            {
                // simulation 
                string filesDirectory = Path.Combine(xhtmlDir, imageFileFolder);
                if (!System.IO.Directory.Exists(filesDirectory)) Directory.CreateDirectory(filesDirectory);
                // test data
                DirectoryExtension.DirectoryCopy($@"c:\Temp\Convert\Test\Files\", filesDirectory, copySubDirs:true,overwrite:true);
                xhtml = System.IO.File.ReadAllText(@"c:\Temp\Convert\Test\test.xhtml");
            }

            // Adapt images to ReqIF standard
            // From:
            // <img src="test.files\png1.png" width="73" height="53" alt="" />
            // To:
            // <object data="test.files\png1.png" type="image/png" />
            Regex rx = new Regex(@"<img src=""([^""]*)""[^>]*>");

            Match match = rx.Match(xhtml);
            while (match.Success)
            {
                string replaceString = $@"<object data=""{match.Groups[1].Value}"" type=""image/png"" />";
                xhtml = xhtml.Replace(match.Groups[0].Value, replaceString);
                match = match.NextMatch();
            }
            // delete everything before/including '<body>'
            int pos = xhtml.IndexOf(@"<body>",StringComparison.Ordinal);
            xhtml = xhtml.Substring(pos + 6);

            // delete </body></html>
            xhtml = xhtml.Replace("</body>", "");
            xhtml = xhtml.Replace("</html>", "");
            return xhtml;
        }
    }
}
