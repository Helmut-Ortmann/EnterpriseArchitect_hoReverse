using System;
using System.Collections.Generic;
using System.Windows.Forms;
using hoReverse.hoUtils;
using Newtonsoft.Json.Linq;
using hoUtils.Json;

namespace EaServices.Doors
{


    /// <summary>
    /// Import settings from json and allow features like integrate in Menue, Run menu item feature, this all configurable.
    /// </summary>
    public class ImportSetting
    {
        // Import Settings
        public List<FileImportSettingsItem> ImportSettings { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="jasonFilePath"></param>

        public ImportSetting(string jasonFilePath)
        {

            // use 'Deserializing Partial JSON Fragments'
            JObject jObject;
            try
            {
                // Read JSON
                string text = HoUtil.ReadAllText(jasonFilePath);
                jObject = JObject.Parse(text);
            }
            catch (Exception e)
            {
                MessageBox.Show(
                    $@"Can't read '{jasonFilePath}

Consider Resetting to factory settings
- File, Reset Factory Settings!!!'

{e}", @"Can't import Chapter: 'Importer' from Settings.json");
                return;
            }

            //----------------------------------------------------------------------
            // Deserialize "DiagramStyle", "DiagramObjectStyle",""DiagramLinkStyle"
            // get JSON result objects into a list
            ImportSettings =
                (List<FileImportSettingsItem>) JasonHelper.GetConfigurationItems<FileImportSettingsItem>(
                    jObject, "Importer");

        }

        /// <summary>
        /// Create a ToolStripItem with DropDownitems for each importable feature .
        /// The Tag property contains the style. If no configuration is available insert a text as a hint to a missing configuration.
        /// </summary>
        /// <param name="items"></param>
        /// <param name="nameRoot"></param>
        /// <param name="toolTipRoot"></param>
        /// <param name="eventHandler"></param>
        /// <param name="hoverHandler"></param>
        /// <param name="contectItem">A Context item (right click), optional</param>
        /// <param name="mouseDownEventHandlerContext"></param>
        /// <returns></returns>
        public ToolStripMenuItem ConstructImporterMenuItems<T>(List<T> items, string nameRoot, string toolTipRoot,
            EventHandler eventHandler, 
            EventHandler hoverHandler = null, 
            string contectItem = "",  MouseEventHandler mouseDownEventHandlerContext = null
            )
        {

            return JasonHelper.ConstructStyleToolStripMenuDiagram(items, nameRoot, toolTipRoot,
                eventHandler,
                hoverHandler, 
                contectItem,  mouseDownEventHandlerContext);



        }
    }
}

