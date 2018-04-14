// ReSharper disable once CheckNamespace
namespace hoReverse.hoUtils
{
    public static class TaggedValue
    {
        /// <summary>
        /// Get Tagged Value with 'Name'. If tagged value doesn't exists a new one is created. Don't forget to write the value and update.
        /// </summary>
        /// <param name="el"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static EA.TaggedValue AddTaggedValue(EA.Element el, string name)
        {
            foreach (EA.TaggedValue taggedValue in el.TaggedValues)
            {
                if (taggedValue.Name == name)
                {
                    return taggedValue;
                }
            }

            // create tagged value
            EA.TaggedValue tg = (EA.TaggedValue)el.TaggedValues.AddNew(name, "Tag");
            el.TaggedValues.Refresh();

            return tg;
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
    }
}