namespace EaServices.Doors.ReqIfs.Inventory
{
    /// <summary>
    /// An item in the inventory list
    /// </summary>
    public class InventoryItem
    {
        public string File { set; get; }
        public string Id { set; get; }
        public string Name { set; get; }
        public int CountReq { set; get; }
        public string LastChanged { set; get; }
        public string Description { set; get; }
        public int CountLinks { set; get; }
        public InventoryItem(string file, string id, string name)
        {
            File = file;
            Name = name;
            Id = id;
        }
        /// <summary>
        /// Constructor Inventory item
        /// </summary>
        /// <param name="file"></param>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="countReq"></param>
        public InventoryItem(string file, string id, string name, int countReq)
        {
            File = file;
            Name = name;
            Id = id;
            CountReq = countReq;
        }
        /// <summary>
        /// Constructor Inventory item
        /// </summary>
        /// <param name="file"></param>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="countReq"></param>
        /// <param name="countLinks"></param>
        public InventoryItem(string file, string id, string name, int countReq, int countLinks)
        {
            File = file;
            Name = name;
            Id = id;
            CountReq = countReq;
            CountLinks = countLinks;
        }

        /// <summary>
        /// Constructor Inventory item
        /// </summary>
        /// <param name="file"></param>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="lastChanged"></param>
        /// <param name="countReq"></param>
        /// <param name="countLinks"></param>

        public InventoryItem(string file, string id, string name, string description, string lastChanged, int countReq, int countLinks)
        {
            File = file;
            Name = name;
            Id = id;
            Description = description;
            LastChanged = lastChanged;
            CountReq = countReq;
            CountLinks = countLinks;
        }
    }
}
