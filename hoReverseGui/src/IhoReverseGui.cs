using System.Runtime.InteropServices;

namespace hoReverse.Reverse
{
    [ComVisible(true)]
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    [Guid("26B0849B-0B39-4D0F-9785-8DDFDBEA5531")]
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
