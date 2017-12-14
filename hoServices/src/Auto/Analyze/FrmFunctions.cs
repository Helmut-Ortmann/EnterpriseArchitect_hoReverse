using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using EaServices.Auto.Analyze;
using hoLinqToSql.LinqUtils;
using hoReverse.hoUtils;

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
            txtVcSymbolDb.Text = LinqUtil.GetDataSourceFromConnectionString(_vcSymbolDataBase); 
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


            GuiHelper.AddSubFilter(lFilters, firstWildCard, "Interface", txtFilterFunction.Text);
            GuiHelper.AddSubFilter(lFilters, firstWildCard, "Implementation", txtFilterImplementation.Text);
            GuiHelper.AddSubFilter(lFilters, firstWildCard, "FileName", txtFilterFile.Text);

           
            // Handle Macro checked
            if (chkOnlyMacros.Checked)
            {
                lFilters.Add($"Macro = true");
            }
            // Handle Implemented by C-Function
            if (chkOnlyImplementations.Checked)
            {
                //lFilters.Add($"FileName LIKE '%.C'");
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

        private void showImplementationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StartCodeFile(sender, "FilePath", "LineStart");

        }

        /// <summary>
        /// Copy all selected Interface/Function names to Clipboard
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void copyInterfaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopyCellValuesToClipboard(sender, "Interface");
        }
        private void copyCalleeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopyCellValuesToClipboard(sender, "FileNameCallee");
        }

        /// <summary>
        /// Copy the cell according to name of all selected rows to Clipboard
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="name"></param>
        private void CopyCellValuesToClipboard(object sender, string name)
        {
            // Get the control that is displaying this context menu
            DataGridView grid = GetDataGridView(sender);
            string text = "";
            string delimiter = "";
            var functions = (from f in grid.SelectedRows.Cast<DataGridViewRow>()
                            orderby f.Cells[name].Value.ToString()
                            select f.Cells[name].Value.ToString()).Distinct();

            foreach (var c in functions)
            {
                var function = (string) c;
                text = $"{text}{delimiter}{function}";
                delimiter = "\r\n";
            }
            Clipboard.SetText(text);
        }

        /// <summary>
        /// Open the file according to column name with the editor
        /// Copies function name to clipboard
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="columnName"></param>
        /// <param name="lineNumberName"></param>
        private void StartCodeFile(object sender, string columnName, string lineNumberName= "")
        {
            // Get the control that is displaying this context menu
            DataGridView grid = GetDataGridView(sender);
            var row = grid.SelectedRows[0];
            string filePath = row.Cells[columnName].Value.ToString();
            string lineNumber = lineNumberName != ""
                ? $":{row.Cells[lineNumberName].Value.ToString()} -g"
                : "";
            filePath = Path.Combine(_folderRoot, filePath);
            HoUtil.StartApp($"Code", $"{filePath}{lineNumber}");
            // Copy Function name to Clipboard
            string functionName = row.Cells["Implementation"].Value.ToString().Trim() != ""
                ? row.Cells["Implementation"].Value.ToString()
                : row.Cells["Interface"].Value.ToString();
            Clipboard.SetText(functionName);

        }

        private DataGridView GetDataGridView(object sender)
        {
            return dataGridView1;
        }
    }
}
