using System;
using System.Windows.Forms;
using hoReverse.Settings;
using hoReverse.Reverse;
using hoReverse.Connectors;
//using Settings2;

// ReSharper disable once CheckNamespace
namespace hoReverse.Settings2
{
    public partial class Settings2Forms : Form
    {
        private readonly AddinSettings _settings;

        public Settings2Forms(AddinSettings settings, HoReverseGui hoReverseGui)
        {
            InitializeComponent();
            this._settings = settings;

            // Logical Diagram connectors
            var defaultLogicalConnectorTypes = _settings.LogicalConnectors.GetConnectorTypes();
            cmbLogicalType.DataSource = defaultLogicalConnectorTypes;
            var defaultLogicalStereotypesTypes = _settings.LogicalConnectors.GetStandardStereotypes();
            cmbLogicalStereotype.DataSource = defaultLogicalStereotypesTypes;
            cmbLogicalLineStyle.DataSource = Connector.GetLineStyle();
            chkBoxLogicalIsDefault.Checked = false;

           
            grdLogicalConnectorType.DataPropertyName = "Type";
            grdLogicalStereotype.DataPropertyName = "Stereotype";
            grdLogicalIsDefault.DataPropertyName = "IsDefault";
            grdLogical.AutoGenerateColumns = false;
            grdLogical.DataSource = _settings.LogicalConnectors;

            // Activity Diagram connectors
            var defaultActivityConnectorTypes = _settings.ActivityConnectors.GetConnectorTypes();
            cmbActivityType.DataSource = defaultActivityConnectorTypes;
            var defaultActivityStereotypesTypes = _settings.ActivityConnectors.GetStandardStereotypes();
            cmbActivityStereotype.DataSource = defaultActivityStereotypesTypes;
            cmbActivityLineStyle.DataSource = Connector.GetLineStyle();
            chkBoxActivityIsDefault.Checked = false;


            grdActivityConnectorType.DataPropertyName = "Type";
            grdActivityStereotype.DataPropertyName = "Stereotype";
            grdActivityIsDefault.DataPropertyName = "IsDefault";
            grdActivity.AutoGenerateColumns = false;
            grdActivity.DataSource = _settings.ActivityConnectors;
       }

        private void btnLogicalAdd_Click(object sender, EventArgs e)
        {
            _settings.LogicalConnectors.Add(new Connector(cmbLogicalType.Text, cmbLogicalStereotype.Text, cmbLogicalLineStyle.Text, chkBoxLogicalIsDefault.Checked));
        }

        private void btnLogicalRemove_Click(object sender, EventArgs e)
        {
            for (int i=grdLogical.SelectedRows.Count -1; i >=0; i = i-1)
            {
                int index = grdLogical.SelectedRows[i].Index;
                _settings.LogicalConnectors.RemoveAt(index);
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this._settings.Save();
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnActivityAdd_Click(object sender, EventArgs e)
        {
            _settings.ActivityConnectors.Add(new Connector(cmbActivityType.Text, cmbActivityStereotype.Text, cmbActivityLineStyle.Text, chkBoxActivityIsDefault.Checked));
        }

        private void btnActivityRemove_Click(object sender, EventArgs e)
        {
            for (int i = grdActivity.SelectedRows.Count - 1; i >= 0; i = i - 1)
            {
                int index = grdActivity.SelectedRows[i].Index;
                _settings.ActivityConnectors.RemoveAt(index);
            }
        }

        private void Settings2Forms_Shown(object sender, EventArgs e)
        {
            TopMost = true;
        }
    }
}
