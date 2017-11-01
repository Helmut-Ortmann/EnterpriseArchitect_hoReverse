using System;
using System.Data.Common;
using System.Data.SQLite;
using System.Windows.Forms;

// ReSharper disable once CheckNamespace
namespace hoReverse.hoUtils.DB
{
    /// <summary>
    /// Access to SQL via ADODB SQLite driver. 
    /// - Driver
    /// -- https://system.data.sqlite.org
    /// -- Supports ADODB, LINQ, EF6
    /// - SQLiteBrowser
    /// -- https://github.com/sqlitebrowser/
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public class SQLite
    {
        private readonly string _dataSource;
        //private SQLiteDataReader _reader;
        private DbDataReader _reader;
        private readonly SQLiteCommand _command;
        /// <summary>
        /// Constructor to create an ADODB connection to SQL Lite
        /// </summary>
        /// <param name="dataSource"></param>
        public SQLite(string dataSource)
        {
            _dataSource = dataSource;
            try
            {
                //  Run nugetPackage console: Install-Package System.Data.SQLite
                //  Error: Can't load  ‘SQLite.Interop.dll’. 
                SQLiteConnection connection = new SQLiteConnection
                {
                    ConnectionString = "Data Source=" + dataSource
                };

                connection.Open();
                _command = new SQLiteCommand(connection);
            }
            catch (Exception e)
            {
                MessageBox.Show($"Can't read VS Code file '.BROWSE.VC.DB' from\r\n'{_dataSource}'\r\n\r\n{e}", 
                    "Can't read VS Code DB");
            }

        }

        public DbDataReader ExecuteSql(string sql)
        {
            try
            {
                _command.CommandText = sql;
                _reader = _command.ExecuteReader();
                return _reader;
            }
            catch (Exception e)
            {
                MessageBox.Show($"Error SQL {sql}'\r\n\r\n{e}",
                    "Can't execute SQL on VS Code SQLite DB '.BROWSE.VC.DB'");
                return null;
            }
        }

        public void EndSql()
        {
            _reader.Close();
            _reader.Dispose();
        }

        public string DataSource
        {
            get { return _dataSource; }
        }

        public DbDataReader Reader
        {
            get { return _reader; }
        }
    }
}
