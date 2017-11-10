using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using DataModels.VcSymbols;
using EA;
using hoLinqToSql.LinqUtils;
using hoReverse.Services.AutoCpp.Analyze;
using LinqToDB.DataProvider;
using File = System.IO.File;

// ReSharper disable once CheckNamespace
namespace hoReverse.Services.AutoCpp
{
    public partial class AutoCpp
    {
        /// <summary>
        /// Show all external functions for this Component/Class
        /// - Provided
        /// - Required
        /// </summary>
        /// <param name="el"></param>
        /// <returns></returns>
        public bool ShowExternalFunctions(EA.Element el)
        {
            // get connection string of repository
            // the provider to connect to database like Access, ..
            string connectionString = LinqUtil.GetConnectionString(ConnectionString, out IDataProvider provider);
            using (BROWSEVCDB db = new BROWSEVCDB(provider, connectionString))
            {
                // Estimate file name of component
                var folderNameOfClass = GetFileNameOfComponent(el,  db);

                // estimate file names of component
                // Component and Module implementation file names beneath folder
                IQueryable<string> fileNamesOfClassTree = from f in db.Files
                    where f.Name.StartsWith(folderNameOfClass) && ( f.LeafName.EndsWith(".c") || f.LeafName.ToLower().EndsWith(".cpp"))
                    select f.Name;

                // Get all functions of implementation
                var allFunctionsImpl = (from f in db.CodeItems
                    join file in db.Files on f.FileId equals file.Id
                    where f.Kind == 22 && (file.LeafName.ToLower().EndsWith(".c") || file.LeafName.ToLower().EndsWith(".cpp"))
                    select new ImplFunctionItem("", f.Name, file.Name, (int)f.StartLine,(int)f.StartColumn,(int)f.EndLine, (int)f.EndColumn)).ToList() ;


                //var function1 = db.CodeItems.ToList();
                //var functions11 = (from f in function1
                //    join m in _macros on f.Name equals m.Key
                //    where f.Name.ToLower().StartsWith(el.Name.ToLower())
                //    select f.Name).ToList().ToDataTable();
                IEnumerable<ImplFunctionItem> allCompImplementations = (
                        // Implemented Interfaces (Macro with Interface and implementation with different name)
                        from m in _macros
                        join f in allFunctionsImpl on m.Key equals f.Implementation
                        where m.Value.ToLower().StartsWith(el.Name.ToLower())
                        select new ImplFunctionItem(m.Value, m.Key, f.FilePath, f.LineStart, f.ColumnStart, f.LineEnd, f.ColumnEnd))
                    .Union
                    (from f in allFunctionsImpl

                        where f.Implementation.StartsWith(el.Name)
                        select new ImplFunctionItem(f.Implementation, f.Implementation, f.FilePath,f.LineStart,f.ColumnStart,f.LineEnd,f.ColumnEnd))
                    .Union
                    // macros without implementation
                    (from m in _macros
                        where m.Value.ToLower().StartsWith(el.Name.ToLower()) &&
                              allFunctionsImpl.All(f => m.Key != f.Implementation)
                        select new ImplFunctionItem(m.Value, m.Key, "",0,0,0,0));


                //-----------------------------------------

                DataTable dtProvidedInterface = GenProvidedInterface(db, folderNameOfClass, fileNamesOfClassTree, allCompImplementations);
                DataTable dtRequiredInterface = GenRequiredInterface(db, folderNameOfClass, fileNamesOfClassTree, allCompImplementations);



                // new component
                if (_frm == null || _frm.IsDisposed)
                {
                    _frm = new FrmComponentFunctions(el, folderNameOfClass, dtProvidedInterface, dtRequiredInterface);
                    _frm.Show();
                }
                else
                {
                    _frm.ChangeComponent(el, folderNameOfClass, dtProvidedInterface, dtRequiredInterface);
                    _frm.Show();
                }
                //frm.ShowDialog();
               
                return true;

            }
        }
        /// <summary>
        /// Get all required Interfaces of Component (root folder of component)
        /// </summary>
        /// <param name="db"></param>
        /// <param name="folderNameOfClass"></param>
        /// <param name="filesPathOfClassTree"></param>
        /// <param name="allCompImplementations"></param>
        /// <returns></returns>
        private static DataTable GenRequiredInterface(BROWSEVCDB db,
            string folderNameOfClass,
            IQueryable<string> filesPathOfClassTree,
            IEnumerable<ImplFunctionItem> allCompImplementations)
        {
            // Estimate all possible function calls of passed files
            Regex rx = new Regex(@"(\b[A-Z]\w*_\w*)\s*\(", RegexOptions.IgnoreCase);
            List<CallFunctionItem> lFunctionCalls = new List<CallFunctionItem>();
            foreach (var file in filesPathOfClassTree)
            {
                Match match = rx.Match(File.ReadAllText(file));
                while (match.Success)
                {
                    lFunctionCalls.Add(new CallFunctionItem(match.Groups[1].Value, file));
                    match = match.NextMatch();
                }
            }
            // ignore the following function names (begging)
            string[] ignoreList = new string[] { "Rte_Read", "Rte_Write" };

            // filter only function implementation
            // - not current folder/subfolder (current component, required)
            // - not ignore of ignore ist
            var filteredFunctions = (from f in lFunctionCalls
                join fAll in allCompImplementations on f equals fAll.Implementation
                where (!fAll.FileName.StartsWith(folderNameOfClass)) && ignoreList.All(l => !f.StartsWith(l))
                orderby fAll.Implementation
                select new
                {
                    Interface = fAll.Interface,
                    Implementation = fAll.Implementation,
                    FilePathImplementation = fAll.FilePath,
                    FilePathCallee = f.FilePath, // no implementation available yet
                    isCalled = false,
                    LineEnd = fAll.LineEnd,
                    ColumnEnd = fAll.ColumnEnd
                }).Distinct();


            // check if filtered functions are implemented
            List<ImplFunctionItem> filteredImplemtedFunctions = new List<ImplFunctionItem>();
            foreach (var f in filteredFunctions)
            {
                string[] codeLines = File.ReadAllLines(f.FilePathImplementation);
                // declaration ends with ';'
                // implementation ends with '}'
                //codeLines[f.Line - 1].Dump();
                if (f.LineEnd > 0 && f.ColumnEnd > 0)
                {
                    if (codeLines[f.LineEnd - 1].Substring((int) f.ColumnEnd - 1, 1) != ";")
                    {
                        filteredImplemtedFunctions.Add(new ImplFunctionItem(f.Interface, f.Implementation, f.FilePathImplementation,
                            f.FilePathCallee));
                    }
                }
            }
            return filteredImplemtedFunctions.ToDataTable();





            return null;
        }

        private static DataTable GenProvidedInterface(BROWSEVCDB db, 
            string folderNameOfClass, 
            IQueryable<string> fileNamesOfClassTree, 
            IEnumerable<ImplFunctionItem> allCompImplementations)
        {

            var compImplementations = (from f in allCompImplementations
                where f.FilePath.StartsWith(folderNameOfClass) || f.FilePath == ""
                select new
                {
                    Imp = new ImplFunctionItem(f.Interface, f.Implementation, f.FilePath),
                    RX = new Regex($@"\b{f.Implementation}\s*\(")
                }).ToArray();

            // over all files except Class/Component Tree (files not part of component/class/sub folder)
            IQueryable<string> fileNamesCalledImplementation = (from f in db.Files
                where !fileNamesOfClassTree.Any(x => x == f.Name) &&
                      (f.LeafName.ToLower().EndsWith(".c") || f.LeafName.ToLower().EndsWith(".cpp"))
                select f.Name).Distinct();

            foreach (var fileName in fileNamesCalledImplementation)
            {
                string code = File.ReadAllText(fileName);
                code = hoService.DeleteComment(code);
                foreach (var f1 in compImplementations)
                {
                    if (f1.RX.IsMatch(code))
                    {
                        //string found = match.Groups[0].Value; 
                        f1.Imp.IsCalled = true;
                        f1.Imp.FilePathCallee = fileName;

                    }
                }
            }
            // Sort: Function, FileName
            var outputList = (from f in compImplementations
                orderby f.Imp.Interface, f.Imp.Implementation
                select new
                {
                    Interface = f.Imp.Interface,
                    Implementation = f.Imp.Implementation,
                    FileName = f.Imp.FileName,
                    FileNameCalleee = f.Imp.FileNameCallee,
                    FilePathImplementation = f.Imp.FilePath,
                    FilePathCalle = f.Imp.FilePathCallee,
                    isCalled = f.Imp.IsCalled
                }).Distinct();

            return outputList.ToDataTable();

        }


        /// <summary>
        /// Get File name of component
        /// </summary>
        /// <param name="el"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        private static string GetFileNameOfComponent(Element el, BROWSEVCDB db)
        {
            string fileNameOfClass = (from f in db.Files
                where f.LeafName.ToLower() == $"{el.Name.ToLower()}.c" || f.LeafName.ToLower() == $"{el.Name.ToLower()}.h" ||
                      f.LeafName.ToLower() == $"{el.Name.ToLower()}.cpp" || f.LeafName.ToLower() == $"{el.Name.ToLower()}.hpp"
                select f.Name).FirstOrDefault();
            if (fileNameOfClass == null)
            {
                MessageBox.Show($"Checked file extensions (*.c,*.h,*.hpp,*.cpp)",
                    $"Cant't find source for '{el.Name}', Break!!");
                return "";
            }

            string folderNameOfClass = Path.GetDirectoryName(fileNameOfClass);
            if (Path.GetFileName(folderNameOfClass)?.ToLower() != el.Name.ToLower())
                folderNameOfClass = Directory.GetParent(folderNameOfClass).FullName;
            if (Path.GetFileName(folderNameOfClass).ToLower() != el.Name.ToLower())
                folderNameOfClass = Directory.GetParent(folderNameOfClass).FullName;
            if (Path.GetFileName(folderNameOfClass).ToLower() != el.Name.ToLower())
                folderNameOfClass = Directory.GetParent(folderNameOfClass).FullName;

            if (Path.GetFileName(folderNameOfClass).ToLower() != el.Name.ToLower())
            {
                MessageBox.Show($"Checked file extensions (*.c,*.h,*.hpp,*.cpp)\r\nLast checked:{folderNameOfClass}",
                    $"Cant't find source for '{el.Name}', Break!!");
                return "";
            }
            return folderNameOfClass;
        }

        /// <summary>
        /// Inventory paths
        /// </summary>
        /// <param name="backgroundWorker">Background worker to update progress or null</param>
        /// <param name="pathRoot">The path of the root folder to inventory</param>
        /// <returns></returns>
        public bool InventoryMacros(System.ComponentModel.BackgroundWorker backgroundWorker, string pathRoot = "")
        {


            // get connection string of repository
            IDataProvider provider; // the provider to connect to database like Access, ..
            string connectionString = LinqUtil.GetConnectionString(ConnectionString, out provider);
            using (var db = new DataModels.VcSymbols.BROWSEVCDB(provider, connectionString))
            {
                // estimate root path
                // Find: '\RTE\RTE.C' and go back
                if (String.IsNullOrWhiteSpace(pathRoot))
                {
                    pathRoot = (from f in db.Files
                        where f.LeafName == "RTE.C"
                        select f.Name).FirstOrDefault();
                    if (String.IsNullOrWhiteSpace(pathRoot))
                    {
                        MessageBox.Show($"Cant find file 'RTE.C' in\r\n{connectionString} ", "Can't determine root path of source code.");
                        return false;
                    }
                    pathRoot = Path.GetDirectoryName(pathRoot);
                    pathRoot = Directory.GetParent(pathRoot).FullName;

                }
                // Estimates macros which concerns functions
                var macros = (from m in db.CodeItems
                    join file in db.Files on m.FileId equals file.Id
                    where m.Kind == 33 && file.Name.Contains(pathRoot) && (file.LeafName.EndsWith(".h") || file.LeafName.EndsWith(".hpp"))
                    orderby file.Name
                    select new { MacroName = m.Name, FilePath = file.Name, FileName = file.LeafName, m.StartLine, m.StartColumn, m.EndLine, m.EndColumn }).Distinct();

                int step =1;
                int count=0;
                if (backgroundWorker != null)
                {
                    step = macros.Count() / 50;
                    count = step;
                    backgroundWorker.ReportProgress(2);
                }
                _macros.Clear();
                string fileLast = "";
                string[] code = new string[] { "" };
                foreach (var m in macros)
                {
                    if (backgroundWorker != null)
                    {
                        count += 1;
                        if (count % step == 0) backgroundWorker.ReportProgress(count/step);
                    }
                    // get file content if file changed
                    if (fileLast != m.FilePath)
                    {
                        fileLast = m.FilePath;
                        code = File.ReadAllLines(m.FilePath);
                    }
                    string text = code[m.StartLine - 1].Substring((int)m.StartColumn - 1);
                    string[] lText = text.Split(' ');
                    if (lText.Count() == 3)
                    {
                        string key = lText[2];
                        if (!_macros.ContainsKey(key))
                            _macros.Add(key, m.MacroName );
                    }
                }


            }
            return true;
        }

        private readonly Dictionary<string, string> _macros = new Dictionary<string, string>();
        private FrmComponentFunctions _frm;
        private readonly EA.Element _component;
    }
}
