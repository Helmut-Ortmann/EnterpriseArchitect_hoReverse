using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using EaServices.Auto.Analyze;
using hoLinqToSql.LinqUtils;
using hoReverse.hoUtils;
using hoReverse.hoUtils.WiKiRefs;

// ReSharper disable once CheckNamespace
namespace hoReverse.Services.AutoCpp.Analyze
{
    public partial class FrmFunctions : Form
    {

        string _vcSymbolDataBase;
        private string _folderRoot;

        readonly BindingSource _bsFunctions = new BindingSource();

        // Print document
        Bitmap _memoryImage;
        private readonly PrintDocument _printDocument1 = new PrintDocument();

        #region helperBringTop
        // Bring window to top helper
        private void BringTotop()
        {
            // is an icon?
            if (IsIconic(this.Handle))
                ShowWindowAsync(this.Handle, SW_RESTORE);

            SetForegroundWindow(this.Handle);
        }

        [DllImport("user32.dll")]
        private static extern
            bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern
            bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern
            bool IsIconic(IntPtr hWnd);

        private const int SW_RESTORE = 9;
        #endregion


        public FrmFunctions()
        {
            InitializeComponent();

            // Parameterize printing
            _printDocument1.DefaultPageSettings.Landscape = true;
            // Margin: bottom, left, right, top
            Margins margins = new Margins(0, 50, 0, 0);
            _printDocument1.DefaultPageSettings.Margins = margins;

            // ReSharper disable once RedundantDelegateCreation
            _printDocument1.PrintPage += new PrintPageEventHandler(printDocument1_PrintPage);
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
            dataGridView1.Columns[12].Visible = false;
            chkOnlyImplementations.Checked = true;
            BringTotop();

            //dataGridView1.Columns[6].Visible = false;
            //dataGridView1.Columns[7].Visible = false;

        }

        private void InitFunctions(string vcSymbolDataBase, string folderRoot, DataTable dtFunctions)
        {
            _vcSymbolDataBase = vcSymbolDataBase;
            _folderRoot = folderRoot;
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

            // enter filter and check for exceptions
            try
            {
                _bsFunctions.Filter = filter;
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

hoReverse always adds a wildcard at string end! hoReverse combines filters by ' AND '

Not allowed are wildcard '*' or '%' amidst the filter string.


{e}",

                    "The filter you have defined is invalid!");
                _bsFunctions.Filter = "";
            }



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
        /// <summary>
        /// Print the screen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CaptureScreen();
            _printDocument1.Print();
        }

        /// <summary>
        /// Capture the screen to print the screen
        /// </summary>
        private void CaptureScreen()
        {
            Graphics myGraphics = this.CreateGraphics();
            Size s = this.Size;
            _memoryImage = new Bitmap(s.Width, s.Height, myGraphics);
            Graphics memoryGraphics = Graphics.FromImage(_memoryImage);
            memoryGraphics.CopyFromScreen(this.Location.X, this.Location.Y, 0, 0, s);
        }
        /// <summary>
        /// Print and scale document on one page.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void printDocument1_PrintPage(System.Object sender,
            System.Drawing.Printing.PrintPageEventArgs e)
        {
            // One page rectangle
            Rectangle m = e.MarginBounds;

            // Adapt high or wide scaling
            if (_memoryImage.Width / (double)_memoryImage.Height > m.Width / (double)m.Height) // image is wider
            {
                m.Height = (int)(_memoryImage.Height / (double)_memoryImage.Width * m.Width);
            }
            else
            {
                m.Width = (int)(_memoryImage.Width / (double)_memoryImage.Height * m.Height);
            }
            e.Graphics.DrawImage(_memoryImage, m);
        }

        private void analyzeCCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WikiRef.WikiAnalyzeC();
        }

        private void FrmFunctions_Shown(object sender, EventArgs e)
        {
            this.TopMost = true;
        }
    }
}
