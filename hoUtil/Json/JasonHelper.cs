﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace hoUtils.Json
{
    public class JasonHelper
    {
        /// <summary>
        /// Get configuration from json. Usually it's advisable not to throw an error.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="search"></param>
        /// <param name="jsonChapter">"DiagramStyle", "DiagramObjectStyle","DiagramLinkStyle", "AutoIncrement"</param>
        /// <param name="ignoreError"></param>
        public static  IList<T> GetConfigurationItems<T>(JObject search, string jsonChapter, bool ignoreError=true)
        {
            try
            {

                IList<JToken> results = search[jsonChapter].Children().ToList();

                // serialize JSON results into .NET objects
                IList<T> searchResults = new List<T>();
                foreach (JToken result in results)
                {
                    // JToken.ToObject is a helper method that uses JsonSerializer internally
                    T searchResult = result.ToObject<T>();
                    if (searchResult == null) continue;
                    searchResults.Add(searchResult);
                }
                return searchResults.ToList();
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
                string text = System.IO.File.ReadAllText(jsonFilePath);
                return JObject.Parse(text);
            }
            catch (Exception e)
            {
                MessageBox.Show($@"Can't read '{jsonFilePath}!'

Consider resetting to Factory Settings.

{e}

",
                    "Can't import 'setting.json'.");
                return null;
            }
        }
    }
}
