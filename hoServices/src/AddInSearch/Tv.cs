namespace EaServices.AddInSearch
{
    /// <summary>
    /// Tagged Values. It handles 'memo' fields.
    /// </summary>
    public class Tv
    {
        public string Property;
        public string Value;
        public Tv(string property, string value)
        {
            Property = property;
            Value = value;
        }
    }
}
