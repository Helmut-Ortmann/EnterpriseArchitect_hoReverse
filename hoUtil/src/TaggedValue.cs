// ReSharper disable once CheckNamespace
namespace hoReverse.hoUtils
{
    public static class TaggedValue
    {
        /// <summary>
        /// Get the value of an element tagged value. It handles Memo field with a lang of > 255
        /// </summary>
        /// <param name="tg"></param>
        /// <returns></returns>
        public static string GetTaggedValue(EA.TaggedValue tg)
        {
            return tg.Value == "<memo>"? tg.Notes : tg.Value;
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
            el.TaggedValues.Refresh();

            return tg;
            
        }
        /// <summary>
        /// Get Tagged Value with 'Name'.
        /// </summary>
        /// <param name="el"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetTaggedValue(EA.Element el, string name)
        {
            foreach (EA.TaggedValue taggedValue in el.TaggedValues)
            {
                if (taggedValue.Name == name)
                {
                    return GetTaggedValue(taggedValue);
                }
            }

            return "";


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
                tg.Value = "<memo>";
                tg.Notes = value;
            }
            else
            {
                tg.Value = value;
                          
            }
            tg.Update();    
            
            return tg;
        }
    }
}