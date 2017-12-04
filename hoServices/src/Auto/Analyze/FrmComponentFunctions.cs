using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using EaServices.Auto.Analyze;
using hoReverse.hoUtils;

// ReSharper disable once CheckNamespace
namespace hoReverse.Services.AutoCpp.Analyze
{
    public partial class FrmComponentFunctions : Form
    {

        readonly BindingSource _bsProvidedInterfaces = new BindingSource();
        readonly BindingSource _bsRequiredInterfaces = new BindingSource();
        private EA.Element _component;
        private string _folderCodeRoot;
        private string _folderCodeComponent;
        private string _vcSymbolDataBase;
        private EA.Repository _rep;
        public FrmComponentFunctions(string vcSymbolDataBase, EA.Repository rep, EA.Element component, string folderRoot, string folderComponent, DataTable dtProvidedInterfaces, DataTable dtRequiredInterfaces)
        {
            InitializeComponent();
            InitComponent(vcSymbolDataBase, rep, component, folderRoot, folderComponent, dtProvidedInterfaces, dtRequiredInterfaces);

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
            txtFolderRoot.Text = _folderCodeRoot;
            txtVcSymbolDb.Text = _vcSymbolDataBase;
            chkOnlyMacros.Checked = false;
            chkOnlyCalledInterfaces.Checked = true;

            grdProvidedInterfaces.DataSource = _bsProvidedInterfaces;
            grdRequiredInterfaces.DataSource = _bsRequiredInterfaces;
            if (grdProvidedInterfaces.ColumnCount > 6)
            {

                grdProvidedInterfaces.Columns[0].Width = 250;
                grdProvidedInterfaces.Columns[1].Width = 50;
                grdProvidedInterfaces.Columns[2].Width = 110;
                grdProvidedInterfaces.Columns[3].Width = 110;
                // set columns headings
                grdProvidedInterfaces.Columns[0].HeaderText = "Prov. Interface";
                grdProvidedInterfaces.Columns[1].HeaderText = "Implementation";
                grdProvidedInterfaces.Columns[2].HeaderText = "File Implementation";
                grdProvidedInterfaces.Columns[3].HeaderText = "File Callee";
                grdProvidedInterfaces.Columns[4].HeaderText = "Path Implementation";
                grdProvidedInterfaces.Columns[5].HeaderText = "Path Callee";
                grdProvidedInterfaces.Columns[6].HeaderText = "Is Called";

            }
            if (grdRequiredInterfaces.ColumnCount > 6)
            {
                grdRequiredInterfaces.Columns[0].Width = 250;
                grdRequiredInterfaces.Columns[1].Width = 200;
                grdRequiredInterfaces.Columns[2].Width = 200;
                // set columns headings
                grdRequiredInterfaces.Columns[0].HeaderText = "Req. Interface";
                grdRequiredInterfaces.Columns[1].HeaderText = "Implementation";
                grdRequiredInterfaces.Columns[2].HeaderText = "File Implementation";
                grdRequiredInterfaces.Columns[3].HeaderText = "File Callee";
                grdRequiredInterfaces.Columns[4].HeaderText = "Path Implementation";
                grdRequiredInterfaces.Columns[5].HeaderText = "Path Callee";
                grdRequiredInterfaces.Columns[6].Visible = false;
                grdRequiredInterfaces.Columns[7].Visible = false;
                grdRequiredInterfaces.Columns[8].Visible = false;
                grdRequiredInterfaces.Columns[9].Visible = false;
                grdRequiredInterfaces.Columns[10].Visible = false;
                grdRequiredInterfaces.Columns[11].Visible = false;



            }
            FilterGrid();
        }

        /// <summary>
        /// Initialize component data for form
        /// </summary>
        /// <param name="vcSymbolDataBase"></param>
        /// <param name="component"></param>
        /// <param name="folderCodeRoot"></param>
        /// <param name="dtProvidedInterfaces"></param>
        /// <param name="dtRequiredInterfaces"></param>
        private void InitComponent(string vcSymbolDataBase, EA.Repository rep, EA.Element component, string folderCodeRoot,string folderCodeComponent,  DataTable dtProvidedInterfaces, DataTable dtRequiredInterfaces)
        {
             // Bind table to binding context
             // for sorting
            _bsProvidedInterfaces.DataSource = dtProvidedInterfaces;
            _bsRequiredInterfaces.DataSource = dtRequiredInterfaces;
            _component = component;
            _folderCodeRoot = folderCodeRoot;
            _folderCodeComponent = folderCodeComponent;
            _vcSymbolDataBase = vcSymbolDataBase;
            _rep = rep;


        }

        /// <summary>
        /// Change component
        /// </summary>
        /// <param name="vcSymbolDataBase"></param>
        /// <param name="rep"></param>
        /// <param name="component"></param>
        /// <param name="folderRoot"></param>
        /// <param name="dtProvidedInterfaces"></param>
        /// <param name="dtRequiredInterfaces"></param>
        public void ChangeComponent(string vcSymbolDataBase, EA.Repository rep, EA.Element component, string folderRoot, string folderComponent,  DataTable dtProvidedInterfaces, DataTable dtRequiredInterfaces)
        {
            InitComponent(vcSymbolDataBase, rep, component, folderRoot, folderComponent, dtProvidedInterfaces, dtRequiredInterfaces);
            ShowComponent();

        }

        /// <summary>
        /// Filter the form
        /// - All sub filters are ' AND ' interwinded
        /// - Wildcard '*' or '%' is only at filter start allowed
        /// - Filters may contain a 'NOT ' at the start to invert the logic
        /// 
        /// Examples:
        /// 'myFilter'
        /// '%myFilter'
        /// 'NOT myFilter'
        /// 'NOT %myFilter'

        /// https://documentation.devexpress.com/WindowsForms/2567/Controls-and-Libraries/Data-Grid/Filter-and-Search/Filtering-in-Code
        /// </summary>
        private void FilterGrid(bool startAtBeginning = false)
        {
            string firstWildCard = "";
            if (startAtBeginning) firstWildCard = "%";
            // Filters to later aggregate to string
            List<string> lFilters = new List<string>();


            // Handle Macro checked
            if (chkOnlyMacros.Checked)
            {
                lFilters.Add($"Implementation <> ''");
            }
            // Handle Implemented by C-Function
            if (chkOnlyCalledInterfaces.Checked)
            {
                lFilters.Add($"isCalled = true");
            }

            GuiHelper.AddSubFilter(lFilters, firstWildCard, "Interface", txtFilterFunctionName.Text);
            GuiHelper.AddSubFilter(lFilters, firstWildCard, "FilePathCallee", txtFilterPathCallee.Text);
            GuiHelper.AddSubFilter(lFilters, firstWildCard, "FilePath", txtFilterPathImpl.Text);


            string filter = GuiHelper.AggregateFilter(lFilters);
            try
            {
                _bsProvidedInterfaces.Filter = filter;
                _bsRequiredInterfaces.Filter = filter;

            }
            catch (Exception e)
            {
                MessageBox.Show($@"Filter: '{filter}'
The Filter may contain:
- '%' or '*' wildcard at string start
- 'NOT ' preceding the filter string

Examples:
'myFilter'
'%myFilter'
'NOT myFilter'
'NOT %myFilter'

hoRerse always adds a wildcard at string end! hoReverse combines filters by ' AND '

Not allowed are wildcard '*' or '%' amidst the filter string.


{e}", 
                    
                    "The filter you have defined is invalid!");
                _bsProvidedInterfaces.Filter = "";
                _bsRequiredInterfaces.Filter = "";
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
        /// <summary>
        /// Filter changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkOnlyMacros_CheckedChanged(object sender, System.EventArgs e)
        {
            FilterGrid();
        }
        /// <summary>
        /// Filter changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkOnlyCalledInterfaces_CheckedChanged(object sender, System.EventArgs e)
        {
            FilterGrid();
        }

        private void txtFilterPathCallee_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                FilterGrid();
                e.Handled = true;
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            string description = @"
Prerequisitions:
- VC Code with C/C++ installed (https://marketplace.visualstudio.com/items?itemName=ms-vscode.cpptools)
- VC Code database is up to date (Open a C/C++ File)
- Open Code with 'Open Folder' and open one file to ensure the VS Code updates the Symbol Database

Troubleshooting:
- Delete all C/C++ Symbol Databases and let VC Code create a new one.
-- Delete conten of folder:
-- c:\users\<user>\AppData\Roaming\Code\User\workspaceStorage\

Background:
VC Code C/C++ has an SQLite Database for each C/C++ folder which stores all Symbols on the C/C++ code.
C/C++ updates this Symbol Database when you edit/open a C/C++ file 

";
            MessageBox.Show(description.Trim(), "Analyze Code your EA model is based on!");

        }

        private void filterToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            string httpFilter = "https://documentation.devexpress.com/WindowsForms/2567/Controls-and-Libraries/Data-Grid/Filter-and-Search/Filtering-in-Code";
            Process.Start(httpFilter);
        }

        private void showCalleeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Try to cast the sender to a ToolStripItem
            ToolStripItem menuItem = sender as ToolStripItem;
            if (menuItem != null)
            {
                // Retrieve the ContextMenuStrip that owns this ToolStripItem
                ContextMenuStrip owner = menuItem.Owner as ContextMenuStrip;
                if (owner != null)
                {
                    // Get the control that is displaying this context menu
                    DataGridView grid = (DataGridView)owner.SourceControl;
                    string filePath = grid.SelectedRows[0].Cells["FilePathCallee"].Value.ToString();
                    filePath = Path.Combine(_folderCodeRoot, filePath);
                    HoUtil.StartFile(filePath);
                }
            }

        }
    }
}
