using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using MariGold.OpenXHTML;

namespace EaServices.Doors
{
    public class HtmlToDocx
    {
        /// <summary>
        /// Convert XHTML to *.docx
        /// </summary>
        /// <param name="docXFile">The file to store the result *.docX document</param>
        /// <param name="xhtml"></param>
        /// <returns></returns>
        public static void Convert(string docXFile, string xhtml)
        {
		
            // remove namespace http://www.w3.org/1999/xhtml
            Regex regNameSpaceXhtml = new Regex(@"xmlns:(\w*)=""http://www.w3.org/1999/xhtml"">");
            Match match = regNameSpaceXhtml.Match(xhtml);
            if (match.Success == true)
            {
                xhtml = xhtml.Replace($"{match.Groups[1].Value}:", "");

                
            }
            // <object data=" ==> <img src="
            // <object/> ==> <img/>
            Regex regObjectToImg = new Regex(@"<object.*</object>");
            match = regObjectToImg.Match(xhtml);
            if (match.Success == true)
            {
                string found = match.Groups[0].Value;
                found = found.Replace("<object ", "<img ");
                found = found.Replace("</object>", "</img>");
                found = found.Replace(@" data=""", @" src=""");
                xhtml = xhtml.Replace(match.Groups[0].Value, found);
            }


            // write to *.docx
            if (String.IsNullOrWhiteSpace(xhtml)) xhtml = "Empty!!!";
            WordDocument doc = new WordDocument(docXFile);
            var uri = new System.Uri(System.IO.Path.GetDirectoryName(docXFile));
            doc.ImagePath = uri.AbsoluteUri;

            //doc.Process(new HtmlParser("<div>sample text</div>"));
            try
            {
                doc.Process(new HtmlParser(xhtml));
                doc.Save();

            }
            catch (Exception e)
            {
                MessageBox.Show($"XHTML:'{xhtml}\r\n\r\n{e}", "Error converting XHTML to *.docx");
            }
            

//        else 
//		{
//			 
//		     System.Windows.Forms.MessageBox.Show($@"Regex{Environment.NewLine}:xmlns:(\w*)=""http://www.w3.org/1999/xhtml"">","Error finding xhtml namespace");
//			 return "Error finding xhtml namespace";
//		}

        }
    }
}
