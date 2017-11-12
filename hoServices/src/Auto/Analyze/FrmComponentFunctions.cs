using System.Data;
using System.Windows.Forms;

namespace hoReverse.Services.AutoCpp.Analyze
{
    public partial class FrmComponentFunctions : Form
    {
        private DataTable _dtProvidedInterfaces;
        private DataTable _dtRequiredInterfaces;
        private EA.Element _component;
        private string _folderCodeRoot;
        private string _vcSymbolDataBase;
        public FrmComponentFunctions(string vcSymbolDataBase, EA.Element component, string folderRoot, DataTable dtProvidedInterfaces, DataTable dtRequiredInterfaces)
        {
            InitializeComponent();
            InitComponent(vcSymbolDataBase, component, folderRoot, dtProvidedInterfaces, dtRequiredInterfaces);

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
            txtFolderRoot.Text = _folderCodeRoot;
            txtVcSymbolDb.Text = _vcSymbolDataBase;
            grdProvidedInterfaces.DataSource = _dtProvidedInterfaces;
            grdRequiredInterfaces.DataSource = _dtRequiredInterfaces;
            if (grdProvidedInterfaces.ColumnCount > 6)
            {
                grdProvidedInterfaces.Columns[0].Width = 250;
                grdProvidedInterfaces.Columns[1].Width = 200;
                grdProvidedInterfaces.Columns[2].Width = 200;
                // set columns headings
                grdProvidedInterfaces.Columns[0].HeaderText = "Prov. Interface";
                grdProvidedInterfaces.Columns[1].HeaderText = "Implementation";
                grdProvidedInterfaces.Columns[2].HeaderText = "File implementation";
                grdProvidedInterfaces.Columns[3].HeaderText = "File callee";
                grdProvidedInterfaces.Columns[4].HeaderText = "Path implementation";
                grdProvidedInterfaces.Columns[5].HeaderText = "Path callee";
                grdProvidedInterfaces.Columns[6].HeaderText = "IsCalled";

            }
            if (grdRequiredInterfaces.ColumnCount > 6)
            {
                grdRequiredInterfaces.Columns[0].Width = 250;
                grdRequiredInterfaces.Columns[1].Width = 200;
                grdRequiredInterfaces.Columns[2].Width = 200;
                // set columns headings
                grdRequiredInterfaces.Columns[0].HeaderText = "Req. Interface";
                grdRequiredInterfaces.Columns[1].HeaderText = "Implementation";
                grdRequiredInterfaces.Columns[2].HeaderText = "File implementation";
                grdRequiredInterfaces.Columns[3].HeaderText = "File callee";
                grdRequiredInterfaces.Columns[4].HeaderText = "Path implementation";
                grdRequiredInterfaces.Columns[5].HeaderText = "Path callee";
                grdRequiredInterfaces.Columns[6].Visible = false;
                grdRequiredInterfaces.Columns[7].Visible = false;
                grdRequiredInterfaces.Columns[8].Visible = false;
                grdRequiredInterfaces.Columns[9].Visible = false;
                grdRequiredInterfaces.Columns[10].Visible = false;



            }
        }

        /// <summary>
        /// Initialize component data for form
        /// </summary>
        /// <param name="vcSymbolDataBase"></param>
        /// <param name="component"></param>
        /// <param name="folderCodeRoot"></param>
        /// <param name="dtProvidedInterfaces"></param>
        /// <param name="dtRequiredInterfaces"></param>
        private void InitComponent(string vcSymbolDataBase, EA.Element component, string folderCodeRoot, DataTable dtProvidedInterfaces, DataTable dtRequiredInterfaces)
        {
            _dtProvidedInterfaces = dtProvidedInterfaces;
            _dtRequiredInterfaces = dtRequiredInterfaces;
            _component = component;
            _folderCodeRoot = folderCodeRoot;
            _vcSymbolDataBase = vcSymbolDataBase;

        }

        /// <summary>
        /// Change component
        /// </summary>
        /// <param name="vcSymbolDataBase"></param>
        /// <param name="component"></param>
        /// <param name="folderRoot"></param>
        /// <param name="dtProvidedInterfaces"></param>
        /// <param name="dtRequiredInterfaces"></param>
        public void ChangeComponent(string vcSymbolDataBase, EA.Element component, string folderRoot, DataTable dtProvidedInterfaces, DataTable dtRequiredInterfaces)
        {
            InitComponent(vcSymbolDataBase, component, folderRoot, dtProvidedInterfaces, dtRequiredInterfaces);
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
