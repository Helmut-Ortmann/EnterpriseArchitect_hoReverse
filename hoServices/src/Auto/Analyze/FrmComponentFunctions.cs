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
            InitComponent(component, folderRoot, dt);

        }

        private void FrmComponentFunctions_Load(object sender, System.EventArgs e)
        {
            ShowComponent();
        }

        /// <summary>
        /// Show Form content
        /// </summary>
        private void ShowComponent()
        {
            txtComponent.Text = _component.Name;
            txtGuid.Text = _component.ElementGUID;
            txtFq.Text = _component.FQName;
            txtFolderRoot.Text = _folderRoot;
            grdFunctions.DataSource = _dt;
            if (grdFunctions.ColumnCount > 6)
            {
                grdFunctions.Columns[0].Width = 250;
                grdFunctions.Columns[1].Width = 200;
                grdFunctions.Columns[2].Width = 200;
                // set columns headings
                grdFunctions.Columns[0].HeaderText = "Interfacer";
                grdFunctions.Columns[1].HeaderText = "Implementation";
                grdFunctions.Columns[2].HeaderText = "File implementation";
                grdFunctions.Columns[3].HeaderText = "File callee";
                grdFunctions.Columns[4].HeaderText = "Path implementation";
                grdFunctions.Columns[5].HeaderText = "Path callee";
                grdFunctions.Columns[6].HeaderText = "IsCalled";

            }
        }

        /// <summary>
        /// Initialize component data for form
        /// </summary>
        /// <param name="component"></param>
        /// <param name="folderRoot"></param>
        /// <param name="dt"></param>
        private void InitComponent(EA.Element component, string folderRoot, DataTable dt)
        {
            _dt = dt;
            _component = component;
            _folderRoot = folderRoot;

        }

        /// <summary>
        /// Change component
        /// </summary>
        /// <param name="component"></param>
        /// <param name="folderRoot"></param>
        /// <param name="dt"></param>
        public void ChangeComponent(EA.Element component, string folderRoot, DataTable dt)
        {
            InitComponent(component, folderRoot, dt);
            ShowComponent();

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
