namespace EaServices.Files
{
    public class Modules : EaServices.Files.Files
    {
        public Modules(EA.Repository rep) : base(rep) { }
        public new ModuleItem Add(string filePath)
        {
            return base.Add(filePath) as ModuleItem;

        }
    }
}
