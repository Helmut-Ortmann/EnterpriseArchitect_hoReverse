using System;
using System.Collections.Generic;
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
        public static bool Convert(string docXFile, string xhtml)
        {
            // Make XHTML from ReqIF xhtml
            xhtml = XhtmlFromReqIf(xhtml);
            // write to *.docx
            if (String.IsNullOrWhiteSpace(xhtml)) xhtml = "Empty!!!";
            WordDocument doc;
            try
            {
                doc = new WordDocument(docXFile);
            }
            catch (Exception e)
            {
                MessageBox.Show($@"File: '{docXFile}'

{e}", @"Can't create WordDocument, already opened?");
                return false;
            }

            var uri = new System.Uri(System.IO.Path.GetDirectoryName(docXFile));
            doc.ImagePath = uri.AbsoluteUri;

            //doc.Process(new HtmlParser("<div>sample text</div>"));
            try
            {

                if (xhtml.Contains("<img"))
                {
                    // with images 
                    doc.Process(new HtmlParser(xhtml));
                    doc.Save();
                }
                else
                {
                    //without images
                    doc.Process(new HtmlParser(xhtml));
                    doc.Save();
                }
               

            }
            catch (Exception e)
            {
                MessageBox.Show($@"XHTML:'{xhtml}{Environment.NewLine}{Environment.NewLine}{e}", @"Error converting XHTML to *.docx");
                return false;
            }

            return true;

        }
        /// <summary>
        /// Convert xhtml to rtf with Sautin converter
        /// </summary>
        /// <param name="xhtml"></param>
        /// <param name="rtfFile"></param>
        public static bool ConvertSautin(string rtfFile, string xhtml )
        {
            try
            {
                xhtml = XhtmlFromReqIf(xhtml);
                // write to *.docx
                if (String.IsNullOrWhiteSpace(xhtml)) xhtml = "Empty!!!";

                // Developer License
                SautinSoft.HtmlToRtf h = new SautinSoft.HtmlToRtf {Serial = "10281946238"};
                string xhtmlFile = System.IO.Path.GetDirectoryName(rtfFile);
                xhtmlFile = System.IO.Path.Combine(xhtmlFile, "xxxxxx.xhtml");

                //rtfFile = System.IO.Path.GetDirectoryName(rtfFile);
                //rtfFile = System.IO.Path.Combine(rtfFile, "xxxxxx.rtf");


                System.IO.File.WriteAllText(xhtmlFile, xhtml);
                if (h.OpenHtml(xhtmlFile))
                {
                    bool ok;
                    if (xhtml.Contains("<img"))
                    {
                        xhtml = xhtml.Replace(@"type=""image/png""", "");
                        ok = h.ToRtf(rtfFile);
                    }
                    else
                    {
                        ok = h.ToRtf(rtfFile);
                    }
                    if (! ok)
                    {
                        MessageBox.Show($@"XHTML:'{xhtml}{Environment.NewLine}File:{rtfFile}", @"Error0 converting XHTML to *.rtf");
                        return false;
                    }
                }
                else
                {
                    MessageBox.Show($@"XHTML:'{xhtml}{Environment.NewLine}File:{rtfFile}", @"Error1 converting XHTML to *.rtf");
                    return false;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show($@"XHTML:'{xhtml}{Environment.NewLine}{Environment.NewLine}{e}", @"Error converting XHTML to *.rtf");
                return false;
            }

            return true;
        }


        /// <summary>
        /// Delete XHTML objects from xhtml. Currently only 'img' is supported
        /// </summary>
        /// <param name="xhtml"></param>
        /// <returns></returns>
        public static string DeleteObjects(string xhtml)
        {
            if ( xhtml.Contains("<img") )
            {
                string delOleObject = @"<img.*?(</img>| />)";
                Regex regDelOleObject = new Regex(delOleObject);
                Match match = regDelOleObject.Match(xhtml);
                while (match.Success)
                {
                    xhtml = xhtml.Replace(match.Groups[0].Value, "");
                    match = match.NextMatch();

                }
            }

            return xhtml;

        }
        /// <summary>
        /// Make XHTML from ReqIF xhtml. This is:
        /// - Remove namespace
        /// - nested <object for ole object to simple image
        /// - transform <object to <img
        /// </summary>
        /// <param name="xhtml"></param>
        /// <returns></returns>
        public static string XhtmlFromReqIf(string xhtml)
        {
            // remove namespace http://www.w3.org/1999/xhtml
            Regex regNameSpaceXhtml = new Regex(@"xmlns:([^=]*)=""http://www.w3.org/1999/xhtml"">");
            Match match = regNameSpaceXhtml.Match(xhtml);
            if (match.Success)
            {
                xhtml = xhtml.Replace($"{match.Groups[1].Value}:", "");
            }

            // Replace embedded object 
            /*
                <object data="[^"]*?" type="[^"]*?"(>[^/><]*</object>1| />)
                <object data="OLE_.ole" type="text/rtf">
                    <object data="OLE_0.png" type="image/png">OLE Object
                    </object>
                </object>

                <object data="OLE_AB_.ole" type="text/rtf">
                   <object data="OLE_AB_0.png" type="image/png" />
                </object></div>
            */
            //string delOleObject = @"<object data=[^>]*>\s*?(<object.*?\s*?</object>)\s*?</object>";
            string delOleObject = @"<object data=[^>]*>\s*?(<object.*?\s*(</object>|>))\s*?</object>";

            Regex regDelOleObject = new Regex(delOleObject);
            match = regDelOleObject.Match(xhtml);
            while (match.Success)
            {
               xhtml = xhtml.Replace(match.Groups[0].Value, match.Groups[1].Value);
               match =  match.NextMatch();

            }


            // change '<object' to '<img' 
            //string regex = @"<object.*type=\""([^\""]*)\"">.*</object>";
            //<object.*type="([^"]*)".*?(</object>| />)
            string regex = @"<object.*type=""([^""]*)"".*?(</object>| />)";

            Regex regObjectToImg = new Regex(regex);
            match = regObjectToImg.Match(xhtml);
            while (match.Success)
            {
                switch (match.Groups[1].Value)
                {
                      case  "image/png":
                          string found = match.Groups[0].Value;
                          found = found.Replace("<object ", "<img ");
                          found = found.Replace("</object>", "</img>");
                          found = found.Replace(@" data=""", @" src=""");
                          xhtml = xhtml.Replace(match.Groups[0].Value, found);
                          break;
                      // not allowed types
                      default: 
                          xhtml = xhtml.Replace(match.Groups[0].Value, "");
                          break;

                }

                match = match.NextMatch();

            }

            return xhtml;
        }
        /// <summary>
        /// Get embedded files as List of file paths
        /// </summary>
        /// <param name="xhtml"></param>
        /// <returns>List of file paths</returns>
        public static List<string> GetEmbeddedFiles(string xhtml)
        {
            List<string> embeddedFileList = new List<string>();
            // group[1] = path; group[2] = type
            string s = @"object\s+data=""([^""]*)""\s+type=""([^""]*)""";
     
            Regex rx = new Regex( s);
            Match match = rx.Match(xhtml);
            while (match.Success)
            {
                if (! match.Groups[2].Value.StartsWith("image/"))
                    embeddedFileList.Add(match.Groups[1].Value);
                match = match.NextMatch();
            }
            return embeddedFileList;
        }
    }
}
