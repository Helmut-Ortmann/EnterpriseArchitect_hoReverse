using System.Data;
using System.Windows.Forms;

namespace hoReverse.Services.AutoCpp.Analyze
{
    public partial class FrmComponentFunctions : Form
    {
        private DataTable _dt;
        private EA.Element _component;
        public FrmComponentFunctions(EA.Element component, DataTable dt)
        {
            InitializeComponent();
            _dt = dt;
            _component = component;
            
        }

        private void FrmComponentFunctions_Load(object sender, System.EventArgs e)
        {
            txtComponent.Text = _component.Name;
            txtGuid.Text = _component.ElementGUID;
            txtFq.Text = _component.FQName;
            grdFunctions.DataSource = _dt;
            if (grdFunctions.ColumnCount > 2)
            {
                grdFunctions.Columns[0].Width = 250;
                grdFunctions.Columns[1].Width = 200;
                grdFunctions.Columns[2].Width = 200;
            }
        }

        private void btnOk_Click(object sender, System.EventArgs e)
        {
            this.Close();
        }

        private void btnCancel_Click(object sender, System.EventArgs e)
        {
            this.Close();
        }
    }
}
