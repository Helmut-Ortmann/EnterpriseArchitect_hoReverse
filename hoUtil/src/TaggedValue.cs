﻿// ReSharper disable once CheckNamespace

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;

namespace hoReverse.hoUtils
{
    public static class TaggedValue
    {
        private const string MemoString = "<memo>";
        /// <summary>
        /// Get the value of an element tagged value. It handles Memo field with a lang of > 255
        /// </summary>
        /// <param name="tg"></param>
        /// <returns></returns>
        public static string GetTaggedValue(EA.TaggedValue tg)
        {
            return tg.Value == MemoString ? tg.Notes : tg.Value;
        }

        /// <summary>
        /// Check if the tagged value exists
        /// </summary>
        /// <param name="el"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool Exists(EA.Element el, string name)
        {
            foreach (EA.TaggedValue taggedValue in el.TaggedValues)
            {
                if (taggedValue.Name == name)
                {
                    return true;
                }
            }

            return false;

        }

        /// <summary>
        /// Get Tagged Value with 'Name'. If tagged value doesn't exists than create a new one. Don't forget to write the value and update.
        /// </summary>
        /// <param name="el"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static EA.TaggedValue Add(EA.Element el, string name)
        {
            foreach (EA.TaggedValue taggedValue in el.TaggedValues)
            {
                if (taggedValue.Name == name)
                {
                    return taggedValue;
                }
            }
            
            // create tagged value
            EA.TaggedValue tg = (EA.TaggedValue) el.TaggedValues.AddNew(name, "Tag");
            tg.Update();
            el.TaggedValues.Refresh();

            return tg;
            
        }

        /// <summary>
        /// Get Tagged Value with 'Name'. If TV not exists return "". 
        /// </summary>
        /// <param name="el"></param>
        /// <param name="name"></param>
        /// <param name="caseSensitive"></param>
        /// <returns></returns>
        public static string GetTaggedValue(EA.Element el, string name, bool caseSensitive=true)
        {
            foreach (EA.TaggedValue taggedValue in el.TaggedValues)
            {
                if (caseSensitive)
                {
                    if (taggedValue.Name == name)
                    {
                        return GetTaggedValue(taggedValue);
                    }
                }
                else
                {
                   if (taggedValue.Name.ToLower() == name.ToLower())
                   {
                       return GetTaggedValue(taggedValue);
                   }
                }
            }

            return "";


        }
        /// <summary>
        /// Make a memo field out of the current TaggedValue  
        /// </summary>
        /// <param name="el"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static EA.TaggedValue MakeMemo(EA.Element el, string name)
        {

            EA.TaggedValue tg = Add(el, name);
            string value = tg.Value;
            if (value.ToLower() == MemoString) return tg;
            tg.Value = MemoString;
            tg.Notes = value;
            tg.Update();
            return tg;
        }
        /// <summary>
        /// Set Tagged Value with 'Name' to a value. If tagged value doesn't exists a new one is created. If the  
        /// </summary>
        /// <param name="el"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static EA.TaggedValue SetUpdate(EA.Element el, string name, string value)
        {

            EA.TaggedValue tg = Add(el, name);
            if (value.Length > 255)
            {
                tg.Value = MemoString;
                tg.Notes = value;
            }
            else
            {
                tg.Value = value;
                          
            }
            tg.Update();    
            
            return tg;
        }
        /// <summary>
        /// Create Tagged Value with 'Name'. It return the TaggedValue  
        /// </summary>
        /// <param name="el"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static EA.TaggedValue CreateTaggedValue(EA.Element el, string name)
        {

            EA.TaggedValue tg = Add(el, name);
            tg.Update();
            el.TaggedValues.Refresh();

            return tg;

        }


        /// <summary>
        /// Create a Tagged value type
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="property"></param>
        /// <param name="notes"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public static bool CreateTaggedValueType(EA.Repository rep, string property, string notes, string description)
        {
            try
            {
                string del = notes.Contains("'") ? @"""" : "'";
                string sql = $@"Insert into t_propertytypes ( Property, Notes, Description )
                                   values ({del}{property}{del}, {del}{notes}{del}, {del}{description}{del});";
                rep.Execute(sql);
            }
            catch (Exception e)
            {
                MessageBox.Show($@"Property: '{property}'

{e}", @"Can't create Tagged Value Type");
                return false;
            }

            return true;

        }
        /// <summary>
        /// Get Tagged Value with 'Name'. If tagged value doesn't exists a new one is created
        /// </summary>
        /// <param name="el"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static EA.TaggedValue AddTaggedValue(EA.Element el, string name)
        {
            EA.TaggedValue tagStart = null;
            foreach (EA.TaggedValue taggedValue in el.TaggedValues)
            {
                if (taggedValue.Name == name)
                {
                    tagStart = taggedValue;
                    break;
                }
            }
            if (tagStart == null)
            {
                // create tagged value
                tagStart = (EA.TaggedValue)el.TaggedValues.AddNew(name, "Tag");
                el.TaggedValues.Refresh();
            }
            return tagStart;
        }

        /// <summary>
        /// Set Tagged Value with 'Name' to a value. If tagged value doesn't exists a new one is created. 
        /// </summary>
        /// <param name="el"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static EA.TaggedValue SetTaggedValue(EA.Element el, string name, string value)
        {
            EA.TaggedValue tg = AddTaggedValue(el, name);
            tg.Value = value;
            tg.Update();
            return tg;
        }

        /// <summary>
        /// Set Tagged Value with 'Name' to a value. It handles long memo fields.
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static void SetTaggedValue(EA.TaggedValue tag, string value)
        {
            tag.Value = value.Length > 255 ? MemoString : value;
            tag.Notes = value;
            tag.Update();
        }
        /// <summary>
        /// Set Tagged Value with 'Name' to a value. It handles long memo fields.
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static void SetTaggedValue(EA.AttributeTag tag, string value)
        {
            tag.Value = value.Length > 255 ? MemoString : value;
            tag.Notes = value;
            tag.Update();
        }
        /// <summary>
        /// Set Tagged Value with 'Name' to a value. It handles long memo fields.
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static void SetTaggedValue(EA.MethodTag tag, string value)
        {
            tag.Value = value.Length > 255 ? MemoString : value;
            tag.Notes = value;
            tag.Update();
        }

        /// <summary>
        /// Delete all tagged values for Element
        /// </summary>
        /// <param name="el"></param>
        /// <returns></returns>
        public static bool DeleteTaggedValuesForElement(EA.Element el)
        {
            for (int i = el.TaggedValues.Count - 1; i >= 0; i--)
            {
                el.TaggedValues.DeleteAt((short)i, false);
            }
            el.TaggedValues.Refresh();
            el.Update();
            return true;

        }

        /// <summary>
        /// Create a Tagged value type
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static bool DeleteTaggedValueTye(EA.Repository rep, string property)
        {
            try
            {
                string sql = $@"Delete from t_propertytypes where Property = '{property}';";
                                   
                rep.Execute(sql);
            }
            catch (Exception e)
            {
                MessageBox.Show($@"Property: '{property}'

{e}", @"Can't delete Tagged Value Type");
                return false;
            }

            return true;

        }

        /// <summary>
        /// Create a Tagged value type
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static bool TaggedValueTyeExists(EA.Repository rep, string property)
        {
            try
            {
                string sql = $@"select count(*) as COUNT from t_propertytypes 
                                  where Property = '{property}';";
               string xml = rep.SQLQuery(sql);
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xml);

                XmlNode countXml = xmlDoc.SelectSingleNode("//COUNT");
                if (countXml != null)
                {
                    int count = Int32.Parse(countXml.InnerText);
                    if (count == 0) return false;
                    return true;
                }

                return false;
            }
            catch (Exception e)
            {
                MessageBox.Show($@"Property: '{property}'

{e}", @"Can't read Tagged Value Type");
                return false;
            }

        }
        /// <summary>
        /// Element Tagged Values
        /// - Copy Tagged Values from a template (constructor)
        /// </summary>
        public class ElTagValue
        {
            private readonly List<EA.TaggedValue> _lTv = new List<EA.TaggedValue>();
            private readonly List<EA.TaggedValue> _lTvEx = new List<EA.TaggedValue>();
            private readonly List<string> _lStereotype = new List<string>();
            private EA.Element _el;
            public ElTagValue(EA.Element el, string stereotypeEx = null)
            {
                _el = el;
                if (stereotypeEx != null)
                {
                    foreach (var s in stereotypeEx.Split(','))
                    {
                        _lStereotype.Add(s);
                    }
                }
                foreach (EA.TaggedValue tvEx in el.TaggedValuesEx)
                {
                    _lTvEx.Add(tvEx);
                }
                foreach (EA.TaggedValue tv in el.TaggedValues)
                {
                    _lTv.Add(tv);
                }
            }
            /// <summary>
            /// Copy TV to current element
            /// </summary>
            /// <param name="el"></param>
            public void Copy(EA.Element el)
            {
                //var done = SyncTaggedValues(el);
                foreach (EA.TaggedValue tv in _lTv)
                {
                    var tvNew = TaggedValue.Add(el, tv.Name);

                    TaggedValue.SetTaggedValue(tvNew, tv.Value);
                    tvNew.Update();

                }

                el.Update();
                el.TaggedValues.Refresh();

            }
            /// <summary>
            /// Synchronize all Tagged Values of an Element with its profile/stereotypes
            /// </summary>
            /// <param name="rep"></param>
            /// <param name="el"></param>
            /// <returns></returns>
            public bool SyncTaggedValues(EA.Repository rep, EA.Element el)
            {
                foreach (EA.TaggedValue tv in _el.TaggedValues)
                {
                    // synchronize stereotypes/tagged vales
                    var lFqName = Regex.Split(tv.FQName, "::");
                    if (lFqName.Length == 3)
                    {
                        var sProfile = lFqName[0];
                        var sStereotype = lFqName[1];
                        //seems not to work
                        //ret = ret || el.SynchTaggedValues(sProfile, sStereotype);
                        string par = $"Profile={sProfile};Stereotype={sStereotype};";
                        var ret1 = rep.CustomCommand("Repository", "SynchProfile", par);
                        if (ret1 != "True")
                        {
                            MessageBox.Show($@"{par}", @"Error synchronize EA.Element");
                            return false;
                        }

                    }

                }
                return true;

            }
        }
    }
}