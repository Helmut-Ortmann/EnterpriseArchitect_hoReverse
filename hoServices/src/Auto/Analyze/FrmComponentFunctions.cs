using System.Data;
using System.Windows.Forms;

namespace hoReverse.Services.AutoCpp.Analyze
{
    public partial class FrmComponentFunctions : UserControl
    {
        private DataTable _dt;
        private EA.Element _component;
        public FrmComponentFunctions(EA.Element component, DataTable dt)
        {
            InitializeComponent();
            _dt = dt;
            _component = component;
            txtComponent.Text = _component.Name;
            txtGuid.Text = _component.ElementGUID;
            grdFunctions.DataSource = dt;
        }
    }
}
