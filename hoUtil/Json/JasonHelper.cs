using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using hoReverse.hoUtils;
using Newtonsoft.Json.Linq;

namespace hoUtils.Json
{
    public class JasonHelper
    {
        /// <summary>
        /// Get configuration from json. Usually it's advisable not to throw an error.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="settings"></param>
        /// <param name="jsonChapter">"DiagramStyle", "DiagramObjectStyle","DiagramLinkStyle", "AutoIncrement"</param>
        /// <param name="ignoreError"></param>
        public static  IList<T> GetConfigurationItems<T>(JObject settings, string jsonChapter, bool ignoreError=true)
        {
            try
            {

                IList<JToken> results = settings[jsonChapter].Children().ToList();

                // serialize JSON results into .NET objects
                IList<T> items = new List<T>();
                foreach (JToken result in results)
                {
                    // JToken.ToObject is a helper method that uses JsonSerializer internally
                    T searchResult = result.ToObject<T>();
                    if (searchResult == null) continue;
                    items.Add(searchResult);
                }
                return items.ToList();
            }
            catch (Exception e)
            {
                if (!ignoreError)
                {
                    MessageBox.Show($@"Cant import '{jsonChapter}' from 'Settings.json'

{e}
The chapter '{jsonChapter}' in Settings.json is missing!
Consider:
- resetting to Factory Settings. File, .....
- compare your Settings.JSON with delivered/factory settings
-- Settings.json '... && more' (current)
-- Settings.json '... && more' (delivery)

The other features should work!
",
                        $@"t JSON Chapter '{jsonChapter}' in Settings.json.");
                   
                }
                return null;
            }
        }

        /// <summary>
        /// Create a ToolStripItem with DropDownitems for each configured item.
        /// The Tag property contains the item for which the menu item runs. If no configuration is available insert a text as a hint to a missing configuration.
        /// </summary>
        /// <param name="items"></param>
        /// <param name="nameRoot"></param>
        /// <param name="toolTipRoot"></param>
        /// <param name="eventHandler"></param>
        /// <param name="hoverHandler"></param>
        /// <param name="contextItem">A Context item (right click), optional</param>
        /// <param name="mouseDownEventHandler"></param>
        /// <returns></returns>
        public static ToolStripMenuItem ConstructStyleToolStripMenuDiagram<T>(List<T> items, string nameRoot, string toolTipRoot, EventHandler eventHandler, 
            EventHandler hoverHandler = null, 
            string contextItem ="", MouseEventHandler mouseDownEventHandler= null  // Additional context menu
            )
        {
            ToolStripMenuItem insertTemplateMenuItem = new ToolStripMenuItem
            {
                Text = nameRoot,
                ToolTipText = toolTipRoot
            };
            // Add item of possible style as items in drop down
            if (items == null)
            {
                ToolStripMenuItem item = new ToolStripMenuItem
                {
                    Text = $@"Settings for '{typeof(T)}' not found!",
                    ToolTipText = $@"Setting Settings.Json not available{Environment.NewLine}Chapter: '{typeof(T)}'{Environment.NewLine}Consider resetting to factory settings or create your own styles{Environment.NewLine}File: '%appdata%\ho\...\Settings.Json'",
                };
                

                insertTemplateMenuItem.DropDownItems.Add(item);

            }
            else
            {
                string listNoOld = "";
                foreach (T style in items)
                {

                    if (!(style is IMenuItem menuItem) || menuItem.ListNo == listNoOld) continue;
                    listNoOld = menuItem.ListNo;
                    ToolStripMenuItem item = new ToolStripMenuItem
                    {
                        Text = menuItem.Name,
                        ToolTipText = menuItem.Description,
                        Tag = menuItem
                    };
                    item.Click += eventHandler;
                    // Create a context item with an event handler
                    if (mouseDownEventHandler != null)
                    {
                        item.MouseDown += mouseDownEventHandler;
                    }
                    if (hoverHandler != null)
                    {
                        item.MouseHover += hoverHandler;
                    }


                    insertTemplateMenuItem.DropDownItems.Add(item);
                }
            }

            return insertTemplateMenuItem;

        }


        /// <summary>
        /// Deserialize Json settings into JObject.
        /// </summary>
        /// <param name="jsonFilePath"></param>
        /// <returns></returns>
        public static JObject DeserializeSettings(string jsonFilePath)
        {
            // use 'Deserializing Partial JSON Fragments'
            try
            {
                // Read JSON
                string text = HoUtil.ReadAllText(jsonFilePath);
                return JObject.Parse(text);
            }
            catch (Exception e)
            {
                MessageBox.Show($@"Can't read '{jsonFilePath}!'

Consider resetting to Factory Settings.

{e}

",
                    @"Can't import 'setting.json'.");
                return null;
            }
        }
    }
}
