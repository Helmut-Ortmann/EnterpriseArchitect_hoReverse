using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using DataModels.VcSymbols;
using EA;
using hoLinqToSql.LinqUtils;
using hoReverse.hoUtils;
using hoReverse.Services.AutoCpp.Analyze;
using LinqToDB.DataProvider;
using File = System.IO.File;
// ReSharper disable RedundantAnonymousTypePropertyName

// ReSharper disable once CheckNamespace
namespace hoReverse.Services.AutoCpp
{
    public partial class AutoCpp
    {
        /// <summary>
        /// Show all interfaces (provided/required) for this Component/Class
        /// - Provided (declaration on in header file
        /// -- Defined functions which start with Component-Name
        /// -- Defined functions per macro (#define NAME_functionname implName)
        /// - Required
        /// -- Called functions defined outside the component
        /// -- Takes macros in account
        /// </summary>
        /// <param name="el"></param>
        /// <param name="folderRoot">Root folder patch of C/C++ source code</param>
        /// <returns></returns>
        public bool ShowInterfacesOfElement(EA.Element el, string folderRoot)
        {
            // get connection string of repository
            // the provider to connect to database like Access, ..
            _folderRoot = folderRoot;
            string connectionString = LinqUtil.GetConnectionString(folderRoot, out IDataProvider provider);
            using (BROWSEVCDB db = new BROWSEVCDB(provider, connectionString))
            {
                // Estimate file name of component
                var folderNameOfClass = GetFileNameOfComponent(el,  db);
                if (folderNameOfClass == "") return false;

                // estimate file names of component
                // Component and Module implementation file names beneath folder
                IQueryable<string> fileNamesOfClassTree = from f in db.Files
                    where f.Name.StartsWith(folderNameOfClass) && ( f.LeafName.EndsWith(".c") || f.LeafName.ToLower().EndsWith(".cpp"))
                    select f.Name;

                // Get all functions of implementation
                // store it to list to avoid SQL with something not possible via SQL
                var allFunctionsImpl = (from f in db.CodeItems
                    join file in db.Files on f.FileId equals file.Id
                    where f.Kind == 22 && (file.LeafName.ToLower().EndsWith(".c") || file.LeafName.ToLower().EndsWith(".cpp"))
                    select new ImplFunctionItem("", f.Name, file.Name, (int)f.StartLine,(int)f.StartColumn,(int)f.EndLine, (int)f.EndColumn)).ToList() ;

                

                IEnumerable < ImplFunctionItem > allCompImplementations = (
                        // Implemented Interfaces (Macro with Interface and implementation with different name)
                        from m in _macros
                        join f in allFunctionsImpl on m.Key equals f.Implementation
                        where m.Value.ToLower().StartsWith($"{el.Name.ToLower()}_")
                        select new ImplFunctionItem(m.Value, m.Key, f.FilePath, f.LineStart, f.ColumnStart, f.LineEnd, f.ColumnEnd))
                    .Union
                    (from f in allFunctionsImpl

                     where f.Implementation.ToLower().StartsWith($"{el.Name.ToLower()}_")
                     where _macros.All(m => m.Key != f.Implementation)
                     select new ImplFunctionItem(f.Implementation, f.Implementation, f.FilePath, f.LineStart, f.ColumnStart, f.LineEnd, f.ColumnEnd))
                    .Union
                    // macros without implementation
                    (from m in _macros
                     where m.Value.ToLower().StartsWith($"{el.Name.ToLower()}_") &&
                           allFunctionsImpl.All(f => m.Key != f.Implementation)
                     select new ImplFunctionItem(m.Value, m.Key, "", 0, 0, 0, 0));


                // get all implementations (C_Functions) 
                // - Macro with Implementation 
                // - Implementation without Macro
                // - Macro without implementation
                IEnumerable<ImplFunctionItem> allImplementations = (
                        // Implemented Interfaces (Macro with Interface and implementation with different name)
                        from m in _macros
                        join f in allFunctionsImpl on m.Key equals f.Implementation
                        select new ImplFunctionItem(m.Value, m.Key, f.FilePath, f.LineStart, f.ColumnStart, f.LineEnd, f.ColumnEnd))
                    .Union
                    (from f in allFunctionsImpl
                     where _macros.All(m => m.Key != f.Implementation)
                     select new ImplFunctionItem(f.Implementation, f.Implementation, f.FilePath, f.LineStart, f.ColumnStart, f.LineEnd, f.ColumnEnd))
                    .Union
                    // macros without implementation
                    (from m in _macros
                        where allFunctionsImpl.All(f => m.Key != f.Implementation)
                        select new ImplFunctionItem(m.Value, m.Key, "", 0, 0, 0, 0));


                //-----------------------------------------

                DataTable dtProvidedInterface = ShowProvidedInterface(db, folderNameOfClass, fileNamesOfClassTree, allCompImplementations);
                DataTable dtRequiredInterface = ShowRequiredInterface(db, folderNameOfClass, fileNamesOfClassTree, allImplementations);



                // new component
                if (_frm == null || _frm.IsDisposed)
                {
                    _frm = new FrmComponentFunctions(connectionString, Rep, el, folderRoot, folderNameOfClass, dtProvidedInterface, dtRequiredInterface);
                    _frm.Show();
                }
                else
                {
                    _frm.ChangeComponent(connectionString, Rep, el, _folderRoot, folderNameOfClass, dtProvidedInterface, dtRequiredInterface);
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
        /// <param name="allImplementations"></param>
        /// <returns></returns>
        private static DataTable ShowRequiredInterface(BROWSEVCDB db,
            string folderNameOfClass,
            IQueryable<string> filesPathOfClassTree,
            IEnumerable<ImplFunctionItem> allImplementations)
        {
            // Estimate all possible function calls of passed files
            Regex rx = new Regex(@"(\b[A-Z]\w*_\w*)\s*\(", RegexOptions.IgnoreCase);
            List<CallFunctionItem> lFunctionCalls = new List<CallFunctionItem>();
            foreach (var file in filesPathOfClassTree)
            {
                Match match = rx.Match(HoUtil.ReadAllText(file));
                while (match.Success)
                {
                    lFunctionCalls.Add(new CallFunctionItem(match.Groups[1].Value, file));
                    match = match.NextMatch();
                }
            }
           

            // ignore the following function names (beginning)
            string[] ignoreList =
            {
                "Rte_Read", "Rte_Write","Rte_Invalidate",
                "L2A_Rte_Read", "L2A_Rte_Write","L2A_Rte_Invalidate",
                "L2B_Rte_Read", "L2B_Rte_Write","L2B_Rte_Invalidate",
                "L2C_Rte_Read", "L2C_Rte_Write","L2C_Rte_Invalidate"

            };

            // filter only function implementation
            // - not current folder/subfolder (current component, required)
            // - not to ignore according to ignore list
            var filteredFunctions = (from f in lFunctionCalls
                join fAll in allImplementations on f.Function equals fAll.Implementation
                where (!fAll.FilePath.StartsWith(folderNameOfClass)) 
                      && ignoreList.All(l => !f.Function.StartsWith(l))  // handle ignore list
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
                if (!File.Exists(f.FilePathImplementation))
                {
                    MessageBox.Show($"File:\r\n{f.FilePathImplementation}\r\n\r\nRoot:\r\n{_folderRoot}",
                        "Can't open implementation of required interface, break!!!");
                    break;
                }
                string[] codeLines = File.ReadAllLines(f.FilePathImplementation);
                // declaration ends with ';'
                // implementation ends with '}'
                //codeLines[f.Line - 1].Dump();
                if (f.LineEnd > 0 && f.ColumnEnd > 0)
                {
                    string line = codeLines[f.LineEnd - 1];
                    if (line.Length > 1)
                    {
                        if (line.Substring(f.ColumnEnd - 1, 1) != ";")
                        {
                            filteredImplemtedFunctions.Add(new ImplFunctionItem(
                                f.Interface, 
                                f.Interface == f.Implementation ? "" : f.Implementation,
                                // no root path
                                f.FilePathImplementation.Length > _folderRoot.Length ? f.FilePathImplementation.Substring(_folderRoot.Length) : "",
                                f.FilePathCallee.Length > _folderRoot.Length ? f.FilePathCallee.Substring(_folderRoot.Length): ""));
                        }
                    }
                }
            }
            return filteredImplemtedFunctions.ToDataTable();
        }
        /// <summary>
        /// Generate provided Interface
        /// </summary>
        /// <param name="db"></param>
        /// <param name="folderNameOfClass"></param>
        /// <param name="fileNamesOfClassTree"></param>
        /// <param name="allCompImplementations"></param>
        /// <returns></returns>
        private static DataTable ShowProvidedInterface(BROWSEVCDB db, 
            string folderNameOfClass, 
            IQueryable<string> fileNamesOfClassTree, 
            IEnumerable<ImplFunctionItem> allCompImplementations)
        {

            var compImplementations = (from f in allCompImplementations
                where f.FilePath.StartsWith(folderNameOfClass) || f.FilePath == ""
                select new
                {
                    Imp = new ImplFunctionItem(f.Interface, f.Implementation, f.FilePath),
                    RxImplementation = new Regex($@"\b{f.Implementation}\s*\("),
                    RxInterface = new Regex($@"\b{f.Interface}\s*\(")
                }).ToArray();

            // over all files except Class/Component Tree (files not part of component/class/sub folder)
            IQueryable<string> fileNamesCalledImplementation = (from f in db.Files
                where !fileNamesOfClassTree.Any(x => x == f.Name) &&
                      (f.LeafName.ToLower().EndsWith(".c") || f.LeafName.ToLower().EndsWith(".cpp"))
                select f.Name).Distinct();

            foreach (var fileName in fileNamesCalledImplementation)
            {
                if (!File.Exists(fileName))
                {
                    MessageBox.Show($"File:\r\n{fileName}\r\n\r\nRoot:\r\n{_folderRoot}",
                        "Can't open implementation of provided interface, break!!!");
                    break;
                }
                string code = HoUtil.ReadAllText(fileName);
                code = hoService.DeleteComment(code);
                foreach (var f1 in compImplementations)
                {
                    if (f1.RxImplementation.IsMatch(code) || f1.RxInterface.IsMatch(code))
                    {
                        //string found = match.Groups[0].Value; 
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
                    Implementation = f.Imp.Implementation == f.Imp.Interface ? "" : f.Imp.Implementation,
                    FileName = f.Imp.FileName,
                    FileNameCallee = f.Imp.FileNameCallee,
                    // no root path
                    FilePath = f.Imp.FilePath.Length > _folderRoot.Length ? f.Imp.FilePath.Substring(_folderRoot.Length) : "",
                    FilePathCallee = f.Imp.FilePathCallee.Length > _folderRoot.Length ? f.Imp.FilePathCallee.Substring(_folderRoot.Length) : "",
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
                MessageBox.Show("Checked file extensions (*.c,*.h,*.hpp,*.cpp)",
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
        /// Inventory Macros like '#define AMM_MyFuntion AMM_MyImplementation' 
        /// </summary>
        /// <param name="backgroundWorker">Background worker to update progress or null</param>
        /// <param name="folderPathCSourceCode">The C/C++ root folder</param>
        /// <param name="pathRoot">The path of the root folder to inventory</param>
        /// <returns></returns>
        public bool InventoryMacros(System.ComponentModel.BackgroundWorker backgroundWorker, string folderPathCSourceCode, string pathRoot = "")
        {


            // get connection string of repository
            _folderRoot = folderPathCSourceCode;
            string connectionString = LinqUtil.GetConnectionString(_folderRoot, out var provider, withErrorMessage:true);
            if (connectionString == "") return false;
            using (var db = new DataModels.VcSymbols.BROWSEVCDB(provider, connectionString))
            {
                //// estimate root path
                //// Find: '\RTE\RTE.C' and go back
                //if (String.IsNullOrWhiteSpace(pathRoot))
                //{
                //    pathRoot = (from f in db.Files
                //        where f.LeafName == "RTE.C"
                //        select f.Name).FirstOrDefault();
                //    if (String.IsNullOrWhiteSpace(pathRoot))
                //    {
                //        MessageBox.Show($"Cant find file 'RTE.C' in\r\n{connectionString} ", "Can't determine root path of source code.");
                //        return false;
                //    }
                //    pathRoot = Path.GetDirectoryName(pathRoot);
                //    pathRoot = Directory.GetParent(pathRoot).FullName;

                //}
                // Estimates macros which concerns functions
                var macros = (from m in db.CodeItems
                    join file in db.Files on m.FileId equals file.Id
                    where m.Kind == 33 && file.Name.Contains(pathRoot) && ( file.LeafName.EndsWith(".h") || file.LeafName.EndsWith(".hpp"))
                          &&  ! ( m.Name.StartsWith("_") || m.Name.EndsWith("_H") || m.Name.Contains("_SEC_" ) )
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
                string[] code = { "" };

                // capture the following macros
                // #define <<interface>> <<implementation>>
                Regex rxIsFunctioName = new Regex(@"^[A-Z_a-z]\w*$");
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
                        // check if functionName
                        if (rxIsFunctioName.IsMatch(lText[2].Trim()))
                        { 
                            string key = lText[2];
                            // add macro value as key
                            if (!_macros.ContainsKey(key)) _macros.Add(key, m.MacroName);
                        }
                    }
                }


            }
            return true;
        }

        private readonly Dictionary<string, string> _macros = new Dictionary<string, string>();
        private FrmComponentFunctions _frm;
        private readonly EA.Element _component;

        // VSCode SQLite Database for symbols to ease finding
        // the folder name is a hash to the folder, it only changes when the folder name changes.
        // Access by: 'Data Source=".."' // path of the db
        // - ADODB
        // - LINQ (linq2db)
        // d:\hoData\Projects\00Current\ZF\Work\Source\
        // private static readonly string dataSource = @"c:\Users\helmu_000\AppData\Roaming\Code\User\workspaceStorage\aa695e4b2b69e4df2595f987547a5da3\ms-vscode.cpptools\.BROWSE.VC.DB";
        // private static string dataSource = @"c:\Users\uidr5387\AppData\Roaming\Code\User\workspaceStorage\26045e663446b5f8d692303182313101\ms-vscode.cpptools\.BROWSE.VC.DB";
        // _folderPathCSourceCode = @"d:\hoData\Projects\00Current\ZF\Work\Source\";
        private static string _folderRoot = @"";
    }
}
