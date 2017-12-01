using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using EaServices.Auto.Analyze;

namespace hoReverse.Services.AutoCpp.Analyze
{
    public partial class FrmFunctions : Form
    {

        string _vcSymbolDataBase;
        private string _folderRoot;
        private DataTable _dtFunctions;

        readonly BindingSource _bsFunctions = new BindingSource();


        public FrmFunctions()
        {
            InitializeComponent();
        }

        private void ShowFolder()
        {

            dataGridView1.DataSource = _bsFunctions;
            txtSourceFolder.Text = _folderRoot;
            txtVcSymbolDb.Text = _vcSymbolDataBase;
            dataGridView1.Columns[0].Width = 300;
            dataGridView1.Columns[1].Width = 100;
            dataGridView1.Columns[2].Width = 200;
            dataGridView1.Columns[3].Visible = false;
            dataGridView1.Columns[4].Width = 300;
            dataGridView1.Columns[5].Visible = false;
            dataGridView1.Columns[6].Visible = false;
            dataGridView1.Columns[7].Width = 70;
            dataGridView1.Columns[8].Visible = false;
            dataGridView1.Columns[9].Visible = false;
            dataGridView1.Columns[10].Visible = false;
            dataGridView1.Columns[11].Visible = false;
            chkOnlyImplementations.Checked = true;

            //dataGridView1.Columns[6].Visible = false;
            //dataGridView1.Columns[7].Visible = false;

        }

        private void InitFunctions(string vcSymbolDataBase, string folderRoot, DataTable dtFunctions)
        {
            _vcSymbolDataBase = vcSymbolDataBase;
            _folderRoot = folderRoot;
            _dtFunctions = dtFunctions;
            _bsFunctions.DataSource = dtFunctions;  // Bind table to binding context


        }

        ///  <summary>
        ///  Change component
        ///  </summary>
        ///  <param name="vcSymbolDataBase"></param>
        /// <param name="folderRoot"></param>
        /// <param name="dtFunctions"></param>
        public void ChangeFolder(string vcSymbolDataBase, string folderRoot, DataTable dtFunctions)
        {
            InitFunctions(vcSymbolDataBase, folderRoot, dtFunctions);
            ShowFolder();

        }

        
        /// <summary>
        /// Filter the form
        /// </summary>
        private void FilterGrid(bool startAtBeginning = false)
        {
            string firstWildCard = "";
            if (startAtBeginning) firstWildCard = "%";
            // Filters to later aggregate to string
            List<string> lFilters = new List<string>();

           
            //----------------------------------
            // Handle FunctionName
            if (txtFilterFunction.Text.Trim() != "" )
            {
                lFilters.Add($"Interface LIKE '{firstWildCard}{txtFilterFunction.Text}%'");
            }
            // Handle ImplementationName
            if (txtFilterImplementation.Text.Trim() != "")
            {
                lFilters.Add($"Implementation LIKE '{firstWildCard}{txtFilterImplementation.Text}%'");
            }
            // Handle File name
            if (txtFilterFile.Text.Trim() != "")
            {
                lFilters.Add($"FileName LIKE '{firstWildCard}{txtFilterFile.Text}%'");
            }

            // Handle Macro checked
            if (chkOnlyMacros.Checked)
            {
                lFilters.Add($"Macro = true");
            }
            // Handle Implemented by C-Function
            if (chkOnlyImplementations.Checked)
            {
                lFilters.Add($"FileName LIKE '%.C'");
            }

            string filter = GuiHelper.AggregateFilter(lFilters);
            _bsFunctions.Filter = filter;
        }

        private void txtFilterFunction_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                FilterGrid();
                e.Handled = true;
            }
        }

        private void chkOnlyMacros_CheckedChanged(object sender, EventArgs e)
        {
            FilterGrid();

        }
    }
}
