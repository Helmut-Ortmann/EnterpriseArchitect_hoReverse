using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using EaServices.Auto.Analyze;
using hoLinqToSql.LinqUtils;
using hoReverse.hoUtils;
using hoReverse.hoUtils.WiKiRefs;

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
        // ReSharper disable once NotAccessedField.Local
        private EA.Repository _rep;

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


        public FrmComponentFunctions(string vcSymbolDataBase, EA.Repository rep, EA.Element component, string folderRoot, string folderComponent, DataTable dtProvidedInterfaces, DataTable dtRequiredInterfaces)
        {
            InitializeComponent();
            InitComponent(vcSymbolDataBase, rep, component, folderRoot, folderComponent, dtProvidedInterfaces, dtRequiredInterfaces);

            // Parameterize printing
            _printDocument1.DefaultPageSettings.Landscape = true;
            // Margin: bottom, left, right, top
            Margins margins = new Margins(0, 50, 0, 0);
            _printDocument1.DefaultPageSettings.Margins = margins;

            // ReSharper disable once RedundantDelegateCreation
            _printDocument1.PrintPage += new PrintPageEventHandler(PrintDocument1_PrintPage);
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
            if (_component != null)
            {
                txtComponent.Text = _component.Name;
                txtGuid.Text = _component.ElementGUID;
            }
            else
            {
                txtComponent.Text = Path.GetFileName(_folderCodeComponent);
                txtGuid.Text = "N.A.";
            }

            
            txtFolderRoot.Text = _folderCodeComponent;
            //txtFolderRoot.Text = _folderCodeRoot;
            txtVcSymbolDb.Text = LinqUtil.GetDataSourceFromConnectionString(_vcSymbolDataBase);
            chkOnlyMacros.Checked = false;
            chkOnlyCalledInterfaces.Checked = true;

            grdProvidedInterfaces.DataSource = _bsProvidedInterfaces;
            grdRequiredInterfaces.DataSource = _bsRequiredInterfaces;
            if (grdProvidedInterfaces.ColumnCount > 6)
            {

                grdProvidedInterfaces.Columns[0].Width = 250;
                grdProvidedInterfaces.Columns[1].Width = 50;
                grdProvidedInterfaces.Columns[2].Width = 160;
                grdProvidedInterfaces.Columns[3].Width = 160;
                grdProvidedInterfaces.Columns[4].Width = 260;
                grdProvidedInterfaces.Columns[5].Width = 260;
                grdProvidedInterfaces.Columns[6].Width = 50;
                // set columns headings
                grdProvidedInterfaces.Columns[0].HeaderText = "Prov. Interface";
                grdProvidedInterfaces.Columns[1].HeaderText = "Implementation";
                grdProvidedInterfaces.Columns[2].HeaderText = "File Implementation";
                grdProvidedInterfaces.Columns[3].HeaderText = "File Callee";
                grdProvidedInterfaces.Columns[4].HeaderText = "Path Implementation";
                grdProvidedInterfaces.Columns[5].HeaderText = "Path Callee";
                grdProvidedInterfaces.Columns[6].HeaderText = "Is Called";
                grdProvidedInterfaces.Columns[6].Visible = false;
                grdProvidedInterfaces.Columns[7].Visible = false;

            }
            if (grdRequiredInterfaces.ColumnCount > 6)
            {
                grdRequiredInterfaces.Columns[0].Width = 250;
                grdRequiredInterfaces.Columns[1].Width = 50;
                grdRequiredInterfaces.Columns[2].Width = 160;
                grdRequiredInterfaces.Columns[3].Width = 160;
                grdRequiredInterfaces.Columns[4].Width = 260;
                grdRequiredInterfaces.Columns[5].Width = 260;
                grdRequiredInterfaces.Columns[6].Width = 50;
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
                grdRequiredInterfaces.Columns[12].Visible = false;



            }
            FilterGrid();
            BringTotop();
            

        }

        /// <summary>
        /// Initialize component data for form
        /// </summary>
        /// <param name="vcSymbolDataBase"></param>
        /// <param name="rep"></param>
        /// <param name="component"></param>
        /// <param name="folderCodeRoot"></param>
        /// <param name="folderCodeComponent"></param>
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
        /// <param name="folderComponent"></param>
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
                    
                    @"The filter you have defined is invalid!");
                _bsProvidedInterfaces.Filter = "";
                _bsRequiredInterfaces.Filter = "";
            }
            
        }
        


        private void BtnOk_Click(object sender, System.EventArgs e)
        {
            this.Close();
        }

        private void BtnCancel_Click(object sender, System.EventArgs e)
        {
            this.Close();
        }
        /// <summary>
        /// Filter changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChkOnlyMacros_CheckedChanged(object sender, System.EventArgs e)
        {
            FilterGrid();
        }
        /// <summary>
        /// Filter changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChkOnlyCalledInterfaces_CheckedChanged(object sender, System.EventArgs e)
        {
            FilterGrid();
        }

        private void TxtFilterPathCallee_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                FilterGrid();
                e.Handled = true;
            }
        }

        private void AboutToolStripMenuItem_Click(object sender, System.EventArgs e)
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
            MessageBox.Show(description.Trim(), @"Analyze Code your EA model is based on!");

        }

        private void FilterToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            string httpFilter = "https://msdn.microsoft.com/en-us/library/system.data.datacolumn.expression%28v=vs.110%29.aspx?f=255&MSPPError=-2147217396";
            Process.Start(httpFilter);
        }

        private void ShowCalleeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StartCodeFile(sender, "FilePathCallee");
        }
        private void ShowImplementationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StartCodeFile(sender, "FilePath",  "LineStart");
            

        }
        
        /// <summary>
        /// Copy all selected Interface/Function names to Clipboard
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopyInterfaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopyCellValuesToClipboard(sender, "Interface");
        }
        private void CopyCalleeToolStripMenuItem_Click(object sender, EventArgs e)
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
                var function = c;
                text = $"{text}{delimiter}{function}";
                delimiter = "\r\n";
            }
            Clipboard.SetText(text);
        }

        /// <summary>
        /// Open the file according to column name with the editor.
        /// Copies the function name to Clipboard
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="columnNameFilePath"></param>
        /// <param name="columnNameLineNumber"></param>
        private void StartCodeFile(object sender, string columnNameFilePath, string columnNameLineNumber = "")
        {
            // Get the control that is displaying this context menu
            DataGridView grid = GetDataGridView(sender);
            var row = grid.SelectedRows[0];
            string filePath = row.Cells[columnNameFilePath].Value.ToString();
            filePath = Path.Combine(_folderCodeRoot, filePath);

            // Copy Function name to Clipboard
            string functionName = row.Cells["Implementation"].Value.ToString().Trim() != ""
                ? row.Cells["Implementation"].Value.ToString()
                : row.Cells["Interface"].Value.ToString();
            Clipboard.SetText(functionName);

            // Estimate line number
            var lineNumber = columnNameLineNumber == "" 
                ? GetLineNumber(filePath, functionName).ToString() 
                : row.Cells[columnNameLineNumber].Value.ToString();

            lineNumber = lineNumber != "-1"
                ? $":{lineNumber} -g"
                : "";
            HoUtil.StartApp($"Code", $"{filePath}{lineNumber}");
           



        }

        private int GetLineNumber(string file, string functionName)
        {
            string code = HoUtil.ReadAllText(file);
            Regex rx = new Regex($@"\b{functionName}\b\s*\([^;{{}}]*;");
            Match match = rx.Match(code);
            if (match.Success)
            {
                return code.Take(match.Groups[0].Index).Count(c => c == '\n') + 1;
            }
            else
            {
                return -1;
            }
        }

        private DataGridView GetDataGridView(object sender)
        {
            // Try to cast the sender to a ToolStripItem
            if (sender is ToolStripItem menuItem)
            {
                // Retrieve the ContextMenuStrip that owns this ToolStripItem
                if (menuItem.Owner is ContextMenuStrip owner)
                {
                    // Get the control that is displaying this context menu
                    return (DataGridView) owner.SourceControl;
                }
            }
            return null;
        }

        /// <summary>
        /// Print the screen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PrintToolStripMenuItem_Click(object sender, EventArgs e)
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
        private void PrintDocument1_PrintPage(System.Object sender,
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

        private void AnalyzeCCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WikiRef.WikiAnalyzeC();
        }

        private void FrmComponentFunctions_Shown(object sender, EventArgs e)
        {
            this.TopMost = true;
        }

        /// <summary>
        /// Output the Column Header/Technical name as Tooltip. It uses the data column of the associated data source. 
        /// Disable Tooltip in the grid (ShowCellToolTips = false;).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        // Enter a cell
        private void GrdInterfaces_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1 && e.ColumnIndex != -1)

            {
                DataGridView grid = (DataGridView) sender;
                var dataPropertyName = grid.Columns[e.ColumnIndex].DataPropertyName;
               toolTip1.SetToolTip((DataGridView) sender, dataPropertyName);
            }
            else
            {
                toolTip1.Hide((DataGridView) sender);
            }

        }

        private void FilterToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/Helmut-Ortmann/EnterpriseArchitect_hoReverse/wiki/Analyze#filter");

        }
    }
}
