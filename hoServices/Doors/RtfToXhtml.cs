using System.Text.RegularExpressions;
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
        /// Convert rtf to XHTML
        /// </summary>
        /// <param name="rtfText"></param>
        /// <param name="xhtmlDir"></param>
        /// <returns></returns>
        public static string Convert(string rtfText, string xhtmlDir)
        {
            // test purposes
            bool gen = false;

            string xhtml;

            // generate XHTML
            if (gen)
            {
                var rtfGen = new RtfToHtml();
                rtfGen.OutputFormat = SautinSoft.RtfToHtml.eOutputFormat.XHTML_10;
                rtfGen.Encoding = SautinSoft.RtfToHtml.eEncoding.UTF_8;
                //specify image options
                rtfGen.ImageStyle.ImageFolder = xhtmlDir; //this folder must exist
                rtfGen.ImageStyle.ImageSubFolder = "files"; //this folder will be created by the component
                rtfGen.ImageStyle.ImageFileName = "png"; //template name for images
                rtfGen.ImageStyle.IncludeImageInHtml =
                    false; //false - save images on HDD, true - save images inside HTML

                xhtml = rtfGen.ConvertString(rtfText);
            }
            else
            {
                xhtml = System.IO.File.ReadAllText(@"c:\Temp\Convert\Test\test.xhtml");
            }

            // Adapt images to ReqIF standard
            // From:
            // <img src="test.files\png1.png" width="73" height="53" alt="" />
            // To:
            // <object data="test.files\png1.png" type="image/png" />
            Regex rx = new Regex(@"img src=""([^""]*)""[^>]*>");

            Match match = rx.Match(xhtml);
            while (match.Success)
            {
                string replaceString = $@"<object data=""{match.Groups[1].Value}"" type=""image/png"" />";
                xhtml = xhtml.Replace(match.Groups[0].Value, replaceString);
                match = match.NextMatch();
            }
            return xhtml;
        }
    }
}
