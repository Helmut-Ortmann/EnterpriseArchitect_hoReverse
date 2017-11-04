using System.Runtime.InteropServices;

namespace hoReverse.Reverse
{
    [ComVisible(true)]
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    [Guid("060EE9F6-8DE6-4553-BBAC-2191CD8132DF")]
    public interface IhoReverseGui
    {
        string getName();
        void Close();
        void Save();
        //void locateType();
        //void findUsage();
        //void addElementNote();
        //void addDiagramNote();
        //void displaySpecification();
        //void displayBehavior();
        //void locateOperation();
    }
}
