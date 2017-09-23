using System;
using System.Collections.Generic;
using System.Windows.Forms;
using hoReverse.hoUtils.SQL;

// ReSharper disable once CheckNamespace
namespace hoReverse.Services.Dlg
{

    public partial class DlgUser : Form
    {
        readonly List<string> _users;
        private string _user = "";
        private readonly bool _isSecurityEnabled;

        public DlgUser(EA.Repository rep)
        {
            var sql = new UtilSql(rep);
            InitializeComponent();
            if (rep.IsSecurityEnabled)
            {
                _isSecurityEnabled = true;

                // check if user has the rights to manage users
                if (sql.UserHasPermission(rep.GetCurrentLoginUser(true), 1))
                {
                    _users = sql.GetUsers();
                    txtStatus.Text = "Security is enabled: Choose user";
                }
                else
                {
                    txtStatus.Text = "Security is enabled: Only person with 'Manage User' are allowed to change users!";

                    MessageBox.Show("User has no 'Manage Users' right", "Insufficient user rights");
                    btnOk.Enabled = false;
                }

            }
            else
            {
                _users = sql.GetUsers();
                txtStatus.Text = "Security isn't enabled: Choose or enter your desired author name!";
            }
            
            
            cmbUser.Text = _user;
            cmbUser.DataSource = _users;
        }
        #region property User
        public string User {
               set  
               { 
                   _user = value;
                    cmbUser.Text = value;
               }
               get => _user;
        }
        #endregion

        private void btnOk_Click(object sender, EventArgs e)
        {
            _user = cmbUser.Text;
            if (_isSecurityEnabled & ! _users.Contains(cmbUser.Text )) {
                _user = ""; 
            }
            

        }

    }
    
}
