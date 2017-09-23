namespace EaServices.Files
{
    public class Interfaces : EaServices.Files.Files
    {
        public Interfaces(EA.Repository rep) : base(rep) { }


        

        public new InterfaceItem Add(string filePath)
        {
            return base.Add(filePath) as InterfaceItem;

        }


    }
}
