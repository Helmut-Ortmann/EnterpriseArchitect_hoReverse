﻿// ReSharper disable once CheckNamespace
namespace hoReverse.hoUtils
{
    public static class TaggedValue
    {
        public static EA.TaggedValue AddTaggedValue(EA.Element shm, string name)
        {
            EA.TaggedValue tagStart = null;
            foreach (EA.TaggedValue taggedValue in shm.TaggedValues)
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
                tagStart = (EA.TaggedValue)shm.TaggedValues.AddNew(name, "Tag");
                shm.TaggedValues.Refresh();
            }
            return tagStart;
        }
    }
}