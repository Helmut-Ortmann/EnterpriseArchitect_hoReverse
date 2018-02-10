using System.Collections.Generic;
using hoUtils.Gui;

namespace hoUtils.BulkChange
{

    

    /// <summary>
    /// Deserialize json for bulk change of EA items
    /// </summary>
    public class BulkElement : IMenuItem
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public IList<string> TypesCheck { get; set; }
        public IList<string> StereotypesCheck { get; set; }
        public IList<string> StereotypesApply { get; set; }
        public IList<Tv> TaggedValuesApply { get; set; }
    }
   /// <summary>
   /// Tagged Value
   /// </summary>
    public class Tv
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

}
