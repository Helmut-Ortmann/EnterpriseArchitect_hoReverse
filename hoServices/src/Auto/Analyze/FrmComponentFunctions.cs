using System.Data;
using System.Windows.Forms;

namespace hoReverse.Services.AutoCpp.Analyze
{
    public partial class FrmComponentFunctions : Form
    {
        private DataTable _dt;
        private EA.Element _component;
        private string _folderRoot;
        public FrmComponentFunctions(EA.Element component, string folderRoot, DataTable dt)
        {
            InitializeComponent();
            _dt = dt;
            _component = component;
            _folderRoot = folderRoot;

        }

        private void FrmComponentFunctions_Load(object sender, System.EventArgs e)
        {
            txtComponent.Text = _component.Name;
            txtGuid.Text = _component.ElementGUID;
            txtFq.Text = _component.FQName;
            txtFolderRoot.Text = _folderRoot;
            grdFunctions.DataSource = _dt;
            if (grdFunctions.ColumnCount > 2)
            {
                grdFunctions.Columns[0].Width = 250;
                grdFunctions.Columns[1].Width = 200;
                grdFunctions.Columns[2].Width = 200;
                // set columns headings
                grdFunctions.Columns[0].HeaderText = "C-Function";
                grdFunctions.Columns[1].HeaderText = "Macro-Function";
                grdFunctions.Columns[2].HeaderText = "Implementation";
                grdFunctions.Columns[3].HeaderText = "Called in";
                grdFunctions.Columns[4].HeaderText = "Implementation";
                grdFunctions.Columns[5].HeaderText = "Called in";

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
