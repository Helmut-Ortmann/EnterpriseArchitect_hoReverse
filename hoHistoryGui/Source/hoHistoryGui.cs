using System.Windows.Forms;
using System.Runtime.InteropServices;
using hoReverse.HistoryList;

namespace hoReverse.History
{
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    [Guid("D7ED709D-E9C5-48BC-81E2-05C2A8C3A926")]
    [ProgId("hoReverse.HistoryGui")]
    [ComDefaultInterface(typeof(IHistoryActiveX))]
    public partial class HistoryGui : UserControl, IHistoryActiveX
    {
        
        public HistoryGui()
        {
            InitializeComponent();
        }
        public void setRepository(EA.Repository rep)
        {
            userControlWpf1.SetRepository(rep);
        }
        public void setHistory(EaHistoryList history)
        {
            userControlWpf1.SetHistory(history);
        }
        public void show()
        {
            userControlWpf1.Show();
        }

        private void elementHost1_ChildChanged(object sender, System.Windows.Forms.Integration.ChildChangedEventArgs e)
        {

        }

        private void elementHost1_ChildChanged_1(object sender, System.Windows.Forms.Integration.ChildChangedEventArgs e)
        {

        }
    }
}
