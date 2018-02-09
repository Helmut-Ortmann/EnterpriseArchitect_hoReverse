using System.Collections.Generic;

namespace EaServices.Json
{
    public class BulkElement
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public IList<string> Stereotypes { get; set; }
        public IList<TV> TaggedValues { get; set; }
    }

    public class TV
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

}
