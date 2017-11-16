using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EaServices.src.Auto.Analyze
{
    public partial class FrmFunctions : Form
    {

        string _vcSymbolDataBase;
        private string _folderRoot;
        private DataTable _dtFunctions;

        public FrmFunctions()
        {
            InitializeComponent();
        }

        private void ShowFolder() { 
        
             
        }

        private void InitFunctions(string vcSymbolDataBase, string folderRoot, DataTable dtFunctions)
        {
            _vcSymbolDataBase = vcSymbolDataBase;
            _folderRoot = folderRoot;
            _dtFunctions = dtFunctions;


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

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
