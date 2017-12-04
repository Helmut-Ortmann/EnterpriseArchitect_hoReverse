using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using DataModels.VcSymbols;
using hoReverse.Services.AutoCpp.Analyze;
using hoLinqToSql.LinqUtils;
using LinqToDB.DataProvider;

// ReSharper disable once CheckNamespace
namespace hoReverse.Services.AutoCpp
{
    public partial class AutoCpp
    {
        FrmFunctions _frmFunctions;
        DataTable _dtFunctions;
        public bool ShowFunctions(string folderPathCSourceCode)
        {

           

            // get connection string of repository
            // the provider to connect to database like Access, ..
            _folderRoot = folderPathCSourceCode;
            string connectionString = LinqUtil.GetConnectionString(folderPathCSourceCode, out IDataProvider provider, withErrorMessage:true);
            if (connectionString == "") return false;
            using (BROWSEVCDB db = new BROWSEVCDB(provider, connectionString))
            {

                // Get all functions of implementation
                // store it to list to avoid SQL with something not possible via SQL
                var allFunctionsImpl = (from f in db.CodeItems
                                        join file in db.Files on f.FileId equals file.Id
                                        where f.Kind == 22 && (file.LeafName.ToLower().EndsWith(".c") || file.LeafName.ToLower().EndsWith(".cpp"))
                                        select new ImplFunctionItem("", f.Name, file.Name, (int)f.StartLine, (int)f.StartColumn, (int)f.EndLine, (int)f.EndColumn)).ToList();



               // get all implementations (C_Functions) 
                // - Macro with Implementation 
                // - Implementation without Macro
                // - Macro without implementation
                IEnumerable<ImplFunctionItem> allImplementations = (
                        // Implemented Interfaces (Macro with Interface and implementation with different name)
                        from m in _macros
                        join f in allFunctionsImpl on m.Key equals f.Implementation
                        select new ImplFunctionItem(m.Value, m.Key, f.FilePath, f.LineStart, f.ColumnStart, f.LineEnd, f.ColumnEnd, true))
                    .Union
                    (from f in allFunctionsImpl
                     where _macros.All(m => m.Key != f.Implementation)
                     select new ImplFunctionItem(f.Implementation, "", f.FilePath.Substring(_folderRoot.Length), f.LineStart, f.ColumnStart, f.LineEnd, f.ColumnEnd,false))
                    .Union
                    // macros without implementation
                    (from m in _macros
                     where allFunctionsImpl.All(f => m.Key != f.Implementation)
                     select new ImplFunctionItem(m.Value, m.Key, "", 0, 0, 0, 0, true));

                string[] ignoreList= {@"_"};
                _dtFunctions = (from f in allImplementations
                          where ignoreList.All(l => ! f.Interface.StartsWith(l))  // handle ignore list
                          orderby (f.Interface)
                          select f)
                          .ToDataTable();







                // new component
                if (_frmFunctions == null || _frmFunctions.IsDisposed)
                {

                    _frmFunctions = new FrmFunctions();
                }
                _frmFunctions.ChangeFolder(connectionString, folderPathCSourceCode, _dtFunctions);
                _frmFunctions.Show();
            }


            return true;

        }

    }
}
