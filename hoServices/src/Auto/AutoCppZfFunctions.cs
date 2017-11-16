using System.Data;
using DataModels.VcSymbols;
using EaServices.src.Auto.Analyze;
using hoLinqToSql.LinqUtils;
using LinqToDB.DataProvider;

// ReSharper disable once CheckNamespace
namespace hoReverse.Services.AutoCpp
{
    public partial class AutoCpp
    {
        public bool ShowFunctions(string folderPathCSourceCode)
        {

            FrmFunctions _frmFunctions = null;
            DataTable _dtFunctions = null;

            // get connection string of repository
            // the provider to connect to database like Access, ..
            _folderPathCSourceCode = folderPathCSourceCode;
            string connectionString = LinqUtil.GetConnectionString(folderPathCSourceCode, out IDataProvider provider);
            using (BROWSEVCDB db = new BROWSEVCDB(provider, connectionString))
            {


                // new component
                if (_frmFunctions == null || _frmFunctions.IsDisposed)
                {

                    _frmFunctions.Show();
                }
                else
                {
                    _frmFunctions.ChangeFolder(connectionString, folderPathCSourceCode, _dtFunctions);
                    _frmFunctions.Show();
                }
            }


            return true;

        }

    }
}
