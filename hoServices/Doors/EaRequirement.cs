namespace EaServices.Doors
{
    public class EaRequirement
    {
        public string EaGuid { get; }
        public string Id { get; }
        public EA.Element El { get; }

        public EaRequirement(string eaGuid, string id)
        {
            Id = id;
            EaGuid = eaGuid;
        }
    }
}
