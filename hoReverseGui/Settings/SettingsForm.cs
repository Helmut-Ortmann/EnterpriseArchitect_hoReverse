using System;
using System.Collections.Generic;
using System.Windows.Forms;
using hoReverse.Reverse.EaAddinShortcuts;
using hoReverse.Reverse;
using GlobalHotkeys;


// ReSharper disable once CheckNamespace
namespace hoReverse.Settings
{
    public partial class SettingsForm : Form
    {
        private readonly AddinSettings _settings;
        private readonly HoReverseGui _hoReverseGui;

        public SettingsForm(AddinSettings settings, HoReverseGui hoReverseGui)
        {
            InitializeComponent();

            this._settings = settings;
            this._hoReverseGui = hoReverseGui;
            string[] items = new string[]{"A Automatic","C Custom","D Direct","B Bezier",
                "LV Lateral Vertical","LH Lateral Horizontal","no","OR Orthogonal Rounded",
                "OS Orthogonal Square","TH Tree Horizontal","TV Tree Vertical"};
            string[] itemsActivity = new string[items.Length];
            items.CopyTo(itemsActivity, 0);
            string[] itemsState = new string[items.Length];
            items.CopyTo(itemsState, 0);

            this.cboActivityLineStyle.DataSource = itemsActivity;
            this.cboStateLineStyle.DataSource = itemsState;
            this.cboActivityLineStyle.Text = settings.ActivityLineStyle;
            this.cboStateLineStyle.Text = settings.StatechartLineStyle;

            this.txtQuickSearchName.Text = settings.QuickSearchName;
            this.cbxFileManagerIsTotalCommander.Checked = settings.FileManagerIsTotalCommander;

            this._cbxWithBookmarks.Checked = settings.ShowBookmark;
            this._cbxWithHistory.Checked = settings.ShowHistory;

            // load shortcuts
            EaAddinShortcutSearch sh = _settings.ShortcutsSearch[0];
            txtBtn1Text.Text = sh.keyText;
            txtBtn1SearchName.Text = sh.keySearchName;
            txtBtn1SearchTerm.Text =sh.keySearchTerm;
            txtBtn1SearchTooltip.Text = sh.keySearchTooltip;

            sh = _settings.ShortcutsSearch[1];
            txtBtn2Text.Text = sh.keyText;
            txtBtn2SearchName.Text = sh.keySearchName;
            txtBtn2SearchTerm.Text = sh.keySearchTerm;
            txtBtn2SearchTooltip.Text = sh.keySearchTooltip;

            sh = _settings.ShortcutsSearch[2];
            txtBtn3Text.Text = sh.keyText;
            txtBtn3SearchName.Text = sh.keySearchName;
            txtBtn3SearchTerm.Text = sh.keySearchTerm;
            txtBtn3SearchTooltip.Text = sh.keySearchTooltip;

            sh = _settings.ShortcutsSearch[3];
            txtBtn4Text.Text = sh.keyText;
            txtBtn4SearchName.Text = sh.keySearchName;
            txtBtn4SearchTerm.Text = sh.keySearchTerm;
            txtBtn4SearchTooltip.Text = sh.keySearchTooltip;

            sh = _settings.ShortcutsSearch[4];
            txtBtn5Text.Text = sh.keyText;
            txtBtn5SearchName.Text = sh.keySearchName;
            txtBtn5SearchTerm.Text = sh.keySearchTerm;
            txtBtn5SearchTooltip.Text = sh.keySearchTooltip;




            List<hoReverse.Services.ServiceCall> lServices1 = new List<hoReverse.Services.ServiceCall>();
            List<hoReverse.Services.ServiceCall> lServices2 = new List<hoReverse.Services.ServiceCall>();
            List<hoReverse.Services.ServiceCall> lServices3 = new List<hoReverse.Services.ServiceCall>();
            List<hoReverse.Services.ServiceCall> lServices4 = new List<hoReverse.Services.ServiceCall>();
            List<hoReverse.Services.ServiceCall> lServices11 = new List<hoReverse.Services.ServiceCall>();
            List<hoReverse.Services.ServiceCall> lServices12 = new List<hoReverse.Services.ServiceCall>();
            List<hoReverse.Services.ServiceCall> lServices13 = new List<hoReverse.Services.ServiceCall>();
            List<hoReverse.Services.ServiceCall> lServices14 = new List<hoReverse.Services.ServiceCall>();
            List<hoReverse.Services.ServiceCall> lServices15 = new List<hoReverse.Services.ServiceCall>();


            foreach (hoReverse.Services.ServiceCall service in _settings.AllServices)
            {
                lServices1.Add(service);
                lServices2.Add(service);
                lServices3.Add(service);
                lServices4.Add(service);
                lServices11.Add(service);
                lServices12.Add(service);
                lServices13.Add(service);
                lServices14.Add(service);
                lServices15.Add(service);
            }


            cmbService1.DataSource = _settings.AllServices;
            cmbService1.DisplayMember = "Description";
            cmbService1.ValueMember = "GUID";
            cmbService1.SelectedValue = _settings.ShortcutsServices[0].Guid;
            txtService1Tooltip.Text = _settings.ShortcutsServices[0].Help;
            txtButton1TextService.Text = _settings.ShortcutsServices[0].ButtonText;


            cmbService2.DataSource = lServices1;
            cmbService2.DisplayMember = "Description";
            cmbService2.ValueMember = "GUID";
            cmbService2.SelectedValue = _settings.ShortcutsServices[1].Guid;
            txtService2Tooltip.Text = _settings.ShortcutsServices[1].Help;
            txtButton2TextService.Text = _settings.ShortcutsServices[1].ButtonText;

            cmbService3.DataSource = lServices2;
            cmbService3.DisplayMember = "Description";
            cmbService3.ValueMember = "GUID";
            cmbService3.SelectedValue = _settings.ShortcutsServices[2].Guid;
            txtService3Tooltip.Text = _settings.ShortcutsServices[2].Help;
            txtButton3TextService.Text = _settings.ShortcutsServices[2].ButtonText;


            cmbService4.DataSource = lServices3;
            cmbService4.DisplayMember = "Description";
            cmbService4.ValueMember = "GUID";
            cmbService4.SelectedValue = _settings.ShortcutsServices[3].Guid;
            txtService4Tooltip.Text = _settings.ShortcutsServices[3].Help;
            txtButton4TextService.Text = _settings.ShortcutsServices[3].ButtonText;

            cmbService5.DataSource = lServices4;
            cmbService5.DisplayMember = "Description";
            cmbService5.ValueMember = "GUID";
            cmbService5.SelectedValue = _settings.ShortcutsServices[4].Guid;
            txtService5Tooltip.Text = _settings.ShortcutsServices[4].Help;
            txtButton5TextService.Text = _settings.ShortcutsServices[4].ButtonText;

            #region Global Shortcuts Service
            // Global Keys/Shortcuts
            cmbGlobalKey1Service.DataSource = lServices11;
            cmbGlobalKey1Service.DisplayMember = "Description";
            cmbGlobalKey1Service.ValueMember = "GUID";
            cmbGlobalKey1Service.SelectedValue = _settings.GlobalServiceKeys[0].Guid;
            //cmbGlobalKey1Service.Text = _settings.shortcutsServices[0].Help;


            cmbGlobalKey2Service.DataSource = lServices12;
            cmbGlobalKey2Service.DisplayMember = "Description";
            cmbGlobalKey2Service.ValueMember = "GUID";
            cmbGlobalKey2Service.SelectedValue = _settings.GlobalServiceKeys[1].Guid;
            //cmbGlobalKey2Service.Text = _settings.shortcutsServices[1].Help;

            cmbGlobalKey3Service.DataSource = lServices13;
            cmbGlobalKey3Service.DisplayMember = "Description";
            cmbGlobalKey3Service.ValueMember = "GUID";
            cmbGlobalKey3Service.SelectedValue = _settings.GlobalServiceKeys[2].Guid;
            //cmbGlobalKey3Service.Text = _settings.shortcutsServices[2].Help;


            cmbGlobalKey4Service.DataSource = lServices14;
            cmbGlobalKey4Service.DisplayMember = "Description";
            cmbGlobalKey4Service.ValueMember = "GUID";
            cmbGlobalKey4Service.SelectedValue = _settings.GlobalServiceKeys[3].Guid;
            //cmbGlobalKey4Service.Text = _settings.shortcutsServices[3].Help;


            cmbGlobalKey5Service.DataSource = lServices15;
            cmbGlobalKey5Service.DisplayMember = "Description";
            cmbGlobalKey5Service.ValueMember = "GUID";
            cmbGlobalKey5Service.SelectedValue = _settings.GlobalServiceKeys[4].Guid;
            //cmbGlobalKey5Service.Text = _settings.shortcutsServices[4].Help;

            cmbGlobalKey1Tooltip.Text = _settings.GlobalServiceKeys[0].Tooltip;
            cmbGlobalKey2Tooltip.Text = _settings.GlobalServiceKeys[1].Tooltip;
            cmbGlobalKey3Tooltip.Text = _settings.GlobalServiceKeys[2].Tooltip;
            cmbGlobalKey4Tooltip.Text = _settings.GlobalServiceKeys[3].Tooltip;
            cmbGlobalKey5Tooltip.Text = _settings.GlobalServiceKeys[4].Tooltip;

            Dictionary<string, Keys> lGlobalKeys = GlobalKeysConfig.GetKeys();
            Dictionary<string, Modifiers> lGlobalModifiers = GlobalKeysConfig.GetModifiers();

            // Hot key services
            cmbGlobalKeyService1Key.DataSource = new BindingSource(lGlobalKeys, null);
            cmbGlobalKeyService1Key.DisplayMember = "Key";
            cmbGlobalKeyService1Key.ValueMember = "Key";

            cmbGlobalKeyService1Mod1.DataSource = new BindingSource(lGlobalModifiers, null);
            cmbGlobalKeyService1Mod1.DisplayMember = "Key";
            cmbGlobalKeyService1Mod1.ValueMember = "Key";

            cmbGlobalKeyService1Mod2.DataSource = new BindingSource(lGlobalModifiers, null);
            cmbGlobalKeyService1Mod2.DisplayMember = "Key";
            cmbGlobalKeyService1Mod2.ValueMember = "Key";

            cmbGlobalKeyService1Mod3.DataSource = new BindingSource(lGlobalModifiers, null);
            cmbGlobalKeyService1Mod3.DisplayMember = "Key";
            cmbGlobalKeyService1Mod3.ValueMember = "Key";

            cmbGlobalKeyService1Mod4.DataSource = new BindingSource(lGlobalModifiers, null);
            cmbGlobalKeyService1Mod4.DisplayMember = "Key";
            cmbGlobalKeyService1Mod4.ValueMember = "Key";

            cmbGlobalKeyService2Key.DataSource = new BindingSource(lGlobalKeys, null);
            cmbGlobalKeyService2Key.DisplayMember = "Key";
            cmbGlobalKeyService2Key.ValueMember = "Key";

            cmbGlobalKeyService2Mod1.DataSource = new BindingSource(lGlobalModifiers, null);
            cmbGlobalKeyService2Mod1.DisplayMember = "Key";
            cmbGlobalKeyService2Mod1.ValueMember = "Key";

            cmbGlobalKeyService2Mod2.DataSource = new BindingSource(lGlobalModifiers, null);
            cmbGlobalKeyService2Mod2.DisplayMember = "Key";
            cmbGlobalKeyService2Mod2.ValueMember = "Key";

            cmbGlobalKeyService2Mod3.DataSource = new BindingSource(lGlobalModifiers, null);
            cmbGlobalKeyService2Mod3.DisplayMember = "Key";
            cmbGlobalKeyService2Mod3.ValueMember = "Key";

            cmbGlobalKeyService2Mod4.DataSource = new BindingSource(lGlobalModifiers, null);
            cmbGlobalKeyService2Mod4.DisplayMember = "Key";
            cmbGlobalKeyService2Mod4.ValueMember = "Key";

            cmbGlobalKeyService3Key.DataSource = new BindingSource(lGlobalKeys, null);
            cmbGlobalKeyService3Key.DisplayMember = "Key";
            cmbGlobalKeyService3Key.ValueMember = "Key";

            cmbGlobalKeyService3Mod1.DataSource = new BindingSource(lGlobalModifiers, null);
            cmbGlobalKeyService3Mod1.DisplayMember = "Key";
            cmbGlobalKeyService3Mod1.ValueMember = "Key";

            cmbGlobalKeyService3Mod2.DataSource = new BindingSource(lGlobalModifiers, null);
            cmbGlobalKeyService3Mod2.DisplayMember = "Key";
            cmbGlobalKeyService3Mod2.ValueMember = "Key";

            cmbGlobalKeyService3Mod3.DataSource = new BindingSource(lGlobalModifiers, null);
            cmbGlobalKeyService3Mod3.DisplayMember = "Key";
            cmbGlobalKeyService3Mod3.ValueMember = "Key";

            cmbGlobalKeyService3Mod4.DataSource = new BindingSource(lGlobalModifiers, null);
            cmbGlobalKeyService3Mod4.DisplayMember = "Key";
            cmbGlobalKeyService3Mod4.ValueMember = "Key";

            cmbGlobalKeyService4Key.DataSource = new BindingSource(lGlobalKeys, null);
            cmbGlobalKeyService4Key.DisplayMember = "Key";
            cmbGlobalKeyService4Key.ValueMember = "Key";

            cmbGlobalKeyService4Mod1.DataSource = new BindingSource(lGlobalModifiers, null);
            cmbGlobalKeyService4Mod1.DisplayMember = "Key";
            cmbGlobalKeyService4Mod1.ValueMember = "Key";

            cmbGlobalKeyService4Mod2.DataSource = new BindingSource(lGlobalModifiers, null);
            cmbGlobalKeyService4Mod2.DisplayMember = "Key";
            cmbGlobalKeyService4Mod2.ValueMember = "Key";

            cmbGlobalKeyService4Mod3.DataSource = new BindingSource(lGlobalModifiers, null);
            cmbGlobalKeyService4Mod3.DisplayMember = "Key";
            cmbGlobalKeyService4Mod3.ValueMember = "Key";

            cmbGlobalKeyService4Mod4.DataSource = new BindingSource(lGlobalModifiers, null);
            cmbGlobalKeyService4Mod4.DisplayMember = "Key";
            cmbGlobalKeyService4Mod4.ValueMember = "Key";

            cmbGlobalKeyService5Key.DataSource = new BindingSource(lGlobalKeys, null);
            cmbGlobalKeyService5Key.DisplayMember = "Key";
            cmbGlobalKeyService5Key.ValueMember = "Key";

            cmbGlobalKeyService5Mod1.DataSource = new BindingSource(lGlobalModifiers, null);
            cmbGlobalKeyService5Mod1.DisplayMember = "Key";
            cmbGlobalKeyService5Mod1.ValueMember = "Key";

            cmbGlobalKeyService5Mod2.DataSource = new BindingSource(lGlobalModifiers, null);
            cmbGlobalKeyService5Mod2.DisplayMember = "Key";
            cmbGlobalKeyService5Mod2.ValueMember = "Key";

            cmbGlobalKeyService5Mod3.DataSource = new BindingSource(lGlobalModifiers, null);
            cmbGlobalKeyService5Mod3.DisplayMember = "Key";
            cmbGlobalKeyService5Mod3.ValueMember = "Key";

            cmbGlobalKeyService5Mod4.DataSource = new BindingSource(lGlobalModifiers, null);
            cmbGlobalKeyService5Mod4.DisplayMember = "Key";
            cmbGlobalKeyService5Mod4.ValueMember = "Key";

            cmbGlobalKeyService1Key.SelectedValue = _settings.GlobalServiceKeys[0].Key;
            cmbGlobalKeyService1Mod1.SelectedValue = _settings.GlobalServiceKeys[0].Modifier1;
            cmbGlobalKeyService1Mod2.SelectedValue = _settings.GlobalServiceKeys[0].Modifier2;
            cmbGlobalKeyService1Mod3.SelectedValue = _settings.GlobalServiceKeys[0].Modifier3;
            cmbGlobalKeyService1Mod4.SelectedValue = _settings.GlobalServiceKeys[0].Modifier4;

            cmbGlobalKeyService2Key.SelectedValue = _settings.GlobalServiceKeys[1].Key;
            cmbGlobalKeyService2Mod1.SelectedValue = _settings.GlobalServiceKeys[1].Modifier1;
            cmbGlobalKeyService2Mod2.SelectedValue = _settings.GlobalServiceKeys[1].Modifier2;
            cmbGlobalKeyService2Mod3.SelectedValue = _settings.GlobalServiceKeys[1].Modifier3;
            cmbGlobalKeyService2Mod4.SelectedValue = _settings.GlobalServiceKeys[1].Modifier4;

            cmbGlobalKeyService3Key.SelectedValue = _settings.GlobalServiceKeys[2].Key;
            cmbGlobalKeyService3Mod1.SelectedValue = _settings.GlobalServiceKeys[2].Modifier1;
            cmbGlobalKeyService3Mod2.SelectedValue = _settings.GlobalServiceKeys[2].Modifier2;
            cmbGlobalKeyService3Mod3.SelectedValue = _settings.GlobalServiceKeys[2].Modifier3;
            cmbGlobalKeyService3Mod4.SelectedValue = _settings.GlobalServiceKeys[2].Modifier4;

            cmbGlobalKeyService4Key.SelectedValue = _settings.GlobalServiceKeys[3].Key;
            cmbGlobalKeyService4Mod1.SelectedValue = _settings.GlobalServiceKeys[3].Modifier1;
            cmbGlobalKeyService4Mod2.SelectedValue = _settings.GlobalServiceKeys[3].Modifier2;
            cmbGlobalKeyService4Mod3.SelectedValue = _settings.GlobalServiceKeys[3].Modifier3;
            cmbGlobalKeyService4Mod4.SelectedValue = _settings.GlobalServiceKeys[3].Modifier4;

            cmbGlobalKeyService5Key.SelectedValue = _settings.GlobalServiceKeys[4].Key;
            cmbGlobalKeyService5Mod1.SelectedValue = _settings.GlobalServiceKeys[4].Modifier1;
            cmbGlobalKeyService5Mod2.SelectedValue = _settings.GlobalServiceKeys[4].Modifier2;
            cmbGlobalKeyService5Mod3.SelectedValue = _settings.GlobalServiceKeys[4].Modifier3;
            cmbGlobalKeyService5Mod4.SelectedValue = _settings.GlobalServiceKeys[4].Modifier4;
            #endregion

            // Search
            #region Global Key Search
            // Search
            cmbGlobalKeySearch1Key.DataSource = new BindingSource(lGlobalKeys, null);
            cmbGlobalKeySearch1Key.DisplayMember = "Key";
            cmbGlobalKeySearch1Key.ValueMember = "Key";

            cmbGlobalKeySearch1Mod1.DataSource = new BindingSource(lGlobalModifiers, null);
            cmbGlobalKeySearch1Mod1.DisplayMember = "Key";
            cmbGlobalKeySearch1Mod1.ValueMember = "Key";

            cmbGlobalKeySearch1Mod2.DataSource = new BindingSource(lGlobalModifiers, null);
            cmbGlobalKeySearch1Mod2.DisplayMember = "Key";
            cmbGlobalKeySearch1Mod2.ValueMember = "Key";

            cmbGlobalKeySearch1Mod3.DataSource = new BindingSource(lGlobalModifiers, null);
            cmbGlobalKeySearch1Mod3.DisplayMember = "Key";
            cmbGlobalKeySearch1Mod3.ValueMember = "Key";

            cmbGlobalKeySearch1Mod4.DataSource = new BindingSource(lGlobalModifiers, null);
            cmbGlobalKeySearch1Mod4.DisplayMember = "Key";
            cmbGlobalKeySearch1Mod4.ValueMember = "Key";

            cmbGlobalKeySearch2Key.DataSource = new BindingSource(lGlobalKeys, null);
            cmbGlobalKeySearch2Key.DisplayMember = "Key";
            cmbGlobalKeySearch2Key.ValueMember = "Key";

            cmbGlobalKeySearch2Mod1.DataSource = new BindingSource(lGlobalModifiers, null);
            cmbGlobalKeySearch2Mod1.DisplayMember = "Key";
            cmbGlobalKeySearch2Mod1.ValueMember = "Key";

            cmbGlobalKeySearch2Mod2.DataSource = new BindingSource(lGlobalModifiers, null);
            cmbGlobalKeySearch2Mod2.DisplayMember = "Key";
            cmbGlobalKeySearch2Mod2.ValueMember = "Key";

            cmbGlobalKeySearch2Mod3.DataSource = new BindingSource(lGlobalModifiers, null);
            cmbGlobalKeySearch2Mod3.DisplayMember = "Key";
            cmbGlobalKeySearch2Mod3.ValueMember = "Key";

            cmbGlobalKeySearch2Mod4.DataSource = new BindingSource(lGlobalModifiers, null);
            cmbGlobalKeySearch2Mod4.DisplayMember = "Key";
            cmbGlobalKeySearch2Mod4.ValueMember = "Key";

            cmbGlobalKeySearch3Key.DataSource = new BindingSource(lGlobalKeys, null);
            cmbGlobalKeySearch3Key.DisplayMember = "Key";
            cmbGlobalKeySearch3Key.ValueMember = "Key";

            cmbGlobalKeySearch3Mod1.DataSource = new BindingSource(lGlobalModifiers, null);
            cmbGlobalKeySearch3Mod1.DisplayMember = "Key";
            cmbGlobalKeySearch3Mod1.ValueMember = "Key";

            cmbGlobalKeySearch3Mod2.DataSource = new BindingSource(lGlobalModifiers, null);
            cmbGlobalKeySearch3Mod2.DisplayMember = "Key";
            cmbGlobalKeySearch3Mod2.ValueMember = "Key";

            cmbGlobalKeySearch3Mod3.DataSource = new BindingSource(lGlobalModifiers, null);
            cmbGlobalKeySearch3Mod3.DisplayMember = "Key";
            cmbGlobalKeySearch3Mod3.ValueMember = "Key";

            cmbGlobalKeySearch3Mod4.DataSource = new BindingSource(lGlobalModifiers, null);
            cmbGlobalKeySearch3Mod4.DisplayMember = "Key";
            cmbGlobalKeySearch3Mod4.ValueMember = "Key";

            cmbGlobalKeySearch4Key.DataSource = new BindingSource(lGlobalKeys, null);
            cmbGlobalKeySearch4Key.DisplayMember = "Key";
            cmbGlobalKeySearch4Key.ValueMember = "Key";

            cmbGlobalKeySearch4Mod1.DataSource = new BindingSource(lGlobalModifiers, null);
            cmbGlobalKeySearch4Mod1.DisplayMember = "Key";
            cmbGlobalKeySearch4Mod1.ValueMember = "Key";

            cmbGlobalKeySearch4Mod2.DataSource = new BindingSource(lGlobalModifiers, null);
            cmbGlobalKeySearch4Mod2.DisplayMember = "Key";
            cmbGlobalKeySearch4Mod2.ValueMember = "Key";

            cmbGlobalKeySearch4Mod3.DataSource = new BindingSource(lGlobalModifiers, null);
            cmbGlobalKeySearch4Mod3.DisplayMember = "Key";
            cmbGlobalKeySearch4Mod3.ValueMember = "Key";

            cmbGlobalKeySearch4Mod4.DataSource = new BindingSource(lGlobalModifiers, null);
            cmbGlobalKeySearch4Mod4.DisplayMember = "Key";
            cmbGlobalKeySearch4Mod4.ValueMember = "Key";

            cmbGlobalKeySearch5Key.DataSource = new BindingSource(lGlobalKeys, null);
            cmbGlobalKeySearch5Key.DisplayMember = "Key";
            cmbGlobalKeySearch5Key.ValueMember = "Key";

            cmbGlobalKeySearch5Mod1.DataSource = new BindingSource(lGlobalModifiers, null);
            cmbGlobalKeySearch5Mod1.DisplayMember = "Key";
            cmbGlobalKeySearch5Mod1.ValueMember = "Key";

            cmbGlobalKeySearch5Mod2.DataSource = new BindingSource(lGlobalModifiers, null);
            cmbGlobalKeySearch5Mod2.DisplayMember = "Key";
            cmbGlobalKeySearch5Mod2.ValueMember = "Key";

            cmbGlobalKeySearch5Mod3.DataSource = new BindingSource(lGlobalModifiers, null);
            cmbGlobalKeySearch5Mod3.DisplayMember = "Key";
            cmbGlobalKeySearch5Mod3.ValueMember = "Key";

            cmbGlobalKeySearch5Mod4.DataSource = new BindingSource(lGlobalModifiers, null);
            cmbGlobalKeySearch5Mod4.DisplayMember = "Key";
            cmbGlobalKeySearch5Mod4.ValueMember = "Key";

            cmbGlobalKeySearch1Key.SelectedValue = _settings.GlobalSearchKeys[0].Key;
            cmbGlobalKeySearch1Mod1.SelectedValue = _settings.GlobalSearchKeys[0].Modifier1;
            cmbGlobalKeySearch1Mod2.SelectedValue = _settings.GlobalSearchKeys[0].Modifier2;
            cmbGlobalKeySearch1Mod3.SelectedValue = _settings.GlobalSearchKeys[0].Modifier3;
            cmbGlobalKeySearch1Mod4.SelectedValue = _settings.GlobalSearchKeys[0].Modifier4;
            cmbGlobalKeySearch1Tooltip.Text = _settings.GlobalSearchKeys[0].Tooltip;
            cmbGlobalKeySearch1SearchName.Text = _settings.GlobalSearchKeys[0].SearchName;
            cmbGlobalKeySearch1SearchTerm.Text = _settings.GlobalSearchKeys[0].SearchTerm;

            cmbGlobalKeySearch2Key.SelectedValue = _settings.GlobalSearchKeys[1].Key;
            cmbGlobalKeySearch2Mod1.SelectedValue = _settings.GlobalSearchKeys[1].Modifier1;
            cmbGlobalKeySearch2Mod2.SelectedValue = _settings.GlobalSearchKeys[1].Modifier2;
            cmbGlobalKeySearch2Mod3.SelectedValue = _settings.GlobalSearchKeys[1].Modifier3;
            cmbGlobalKeySearch2Mod4.SelectedValue = _settings.GlobalSearchKeys[1].Modifier4;
            cmbGlobalKeySearch2Tooltip.Text = _settings.GlobalSearchKeys[1].Tooltip;
            cmbGlobalKeySearch2SearchName.Text = _settings.GlobalSearchKeys[1].SearchName;
            cmbGlobalKeySearch2SearchTerm.Text = _settings.GlobalSearchKeys[1].SearchTerm;

            cmbGlobalKeySearch3Key.SelectedValue = _settings.GlobalSearchKeys[2].Key;
            cmbGlobalKeySearch3Mod1.SelectedValue = _settings.GlobalSearchKeys[2].Modifier1;
            cmbGlobalKeySearch3Mod2.SelectedValue = _settings.GlobalSearchKeys[2].Modifier2;
            cmbGlobalKeySearch3Mod3.SelectedValue = _settings.GlobalSearchKeys[2].Modifier3;
            cmbGlobalKeySearch3Mod4.SelectedValue = _settings.GlobalSearchKeys[2].Modifier4;
            cmbGlobalKeySearch3Tooltip.Text = _settings.GlobalSearchKeys[2].Tooltip;
            cmbGlobalKeySearch3SearchName.Text = _settings.GlobalSearchKeys[2].SearchName;
            cmbGlobalKeySearch3SearchTerm.Text = _settings.GlobalSearchKeys[2].SearchTerm;

            cmbGlobalKeySearch4Key.SelectedValue = _settings.GlobalSearchKeys[3].Key;
            cmbGlobalKeySearch4Mod1.SelectedValue = _settings.GlobalSearchKeys[3].Modifier1;
            cmbGlobalKeySearch4Mod2.SelectedValue = _settings.GlobalSearchKeys[3].Modifier2;
            cmbGlobalKeySearch4Mod3.SelectedValue = _settings.GlobalSearchKeys[3].Modifier3;
            cmbGlobalKeySearch4Mod4.SelectedValue = _settings.GlobalSearchKeys[3].Modifier4;
            cmbGlobalKeySearch4Tooltip.Text = _settings.GlobalSearchKeys[3].Tooltip;
            cmbGlobalKeySearch4SearchName.Text = _settings.GlobalSearchKeys[3].SearchName;
            cmbGlobalKeySearch4SearchTerm.Text = _settings.GlobalSearchKeys[3].SearchTerm;

            cmbGlobalKeySearch5Key.SelectedValue = _settings.GlobalSearchKeys[4].Key;
            cmbGlobalKeySearch5Mod1.SelectedValue = _settings.GlobalSearchKeys[4].Modifier1;
            cmbGlobalKeySearch5Mod2.SelectedValue = _settings.GlobalSearchKeys[4].Modifier2;
            cmbGlobalKeySearch5Mod3.SelectedValue = _settings.GlobalSearchKeys[4].Modifier3;
            cmbGlobalKeySearch5Mod4.SelectedValue = _settings.GlobalSearchKeys[4].Modifier4;
            cmbGlobalKeySearch5Tooltip.Text = _settings.GlobalSearchKeys[4].Tooltip;
            cmbGlobalKeySearch5SearchName.Text = _settings.GlobalSearchKeys[4].SearchName;
            cmbGlobalKeySearch5SearchTerm.Text = _settings.GlobalSearchKeys[4].SearchTerm;
            #endregion Global Key Search



        }

        // ReSharper disable once UnusedMember.Local
        private int ServiceGetIndex(List<hoReverse.Services.ServiceCall> lServices, string guid)
        {
            for (int i=0; i < lServices.Count; i = i+1)
            {
                if (_settings.ShortcutsServices[0].Guid == lServices[i].Guid) return i;
            }
            return -1;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            _settings.ActivityLineStyle = cboActivityLineStyle.Text;
            _settings.StatechartLineStyle = cboStateLineStyle.Text;

            // store shortcuts
            EaAddinShortcutSearch sh = _settings.ShortcutsSearch[0];
            sh.keyText = txtBtn1Text.Text;
            sh.keySearchName = txtBtn1SearchName.Text;
            sh.keySearchTerm = txtBtn1SearchTerm.Text;
            sh.keySearchTooltip = txtBtn1SearchTooltip.Text;
            _settings.ShortcutsSearch[0] = sh;

            sh = _settings.ShortcutsSearch[1];
            sh.keyText = txtBtn2Text.Text;
            sh.keySearchName = txtBtn2SearchName.Text;
            sh.keySearchTerm = txtBtn2SearchTerm.Text;
            sh.keySearchTooltip = txtBtn2SearchTooltip.Text;
            _settings.ShortcutsSearch[1] = sh;

            sh = _settings.ShortcutsSearch[2];
            sh.keyText = txtBtn3Text.Text;
            sh.keySearchName = txtBtn3SearchName.Text;
            sh.keySearchTerm = txtBtn3SearchTerm.Text;
            sh.keySearchTooltip = txtBtn3SearchTooltip.Text;
            _settings.ShortcutsSearch[2] = sh;

            sh = _settings.ShortcutsSearch[3];
            sh.keyText = txtBtn4Text.Text;
            sh.keySearchName = txtBtn4SearchName.Text;
            sh.keySearchTerm = txtBtn4SearchTerm.Text;
            sh.keySearchTooltip = txtBtn4SearchTooltip.Text;
            _settings.ShortcutsSearch[3] = sh;

            sh = _settings.ShortcutsSearch[4];
            sh.keyText = txtBtn5Text.Text;
            sh.keySearchName = txtBtn5SearchName.Text;
            sh.keySearchTerm = txtBtn5SearchTerm.Text;
            sh.keySearchTooltip = txtBtn5SearchTooltip.Text;
            _settings.ShortcutsSearch[4] = sh;

            _settings.ShortcutsServices[0].Guid = cmbService1.SelectedValue?.ToString() ?? "";
            _settings.ShortcutsServices[0].ButtonText = txtButton1TextService.Text;
            _settings.ShortcutsServices[0].Description = cmbService1.Text;
            _settings.ShortcutsServices[1].Guid = cmbService2.SelectedValue?.ToString() ?? "";
            _settings.ShortcutsServices[1].ButtonText = txtButton2TextService.Text;
            _settings.ShortcutsServices[1].Description = cmbService2.Text;
            _settings.ShortcutsServices[2].Guid = cmbService3.SelectedValue?.ToString() ?? "";
            _settings.ShortcutsServices[2].ButtonText = txtButton3TextService.Text;
            _settings.ShortcutsServices[2].Description = cmbService3.Text;
            _settings.ShortcutsServices[3].Guid = cmbService4.SelectedValue?.ToString() ?? "";
            _settings.ShortcutsServices[3].ButtonText = txtButton4TextService.Text;
            _settings.ShortcutsServices[4].Guid = cmbService5.SelectedValue?.ToString() ?? "";
            _settings.ShortcutsServices[4].ButtonText = txtButton5TextService.Text;
            _settings.ShortcutsServices[4].Description = cmbService5.Text;

            #region store global services
            // Global Services via hot key
             _settings.GlobalServiceKeys[0].Key = cmbGlobalKeyService1Key.SelectedValue.ToString();
             _settings.GlobalServiceKeys[0].Modifier1 = cmbGlobalKeyService1Mod1.SelectedValue.ToString();
             _settings.GlobalServiceKeys[0].Modifier2 = cmbGlobalKeyService1Mod2.SelectedValue.ToString();
             _settings.GlobalServiceKeys[0].Modifier3 = cmbGlobalKeyService1Mod3.SelectedValue.ToString();
             _settings.GlobalServiceKeys[0].Modifier4 = cmbGlobalKeyService1Mod4.SelectedValue.ToString();
             _settings.GlobalServiceKeys[0].Guid = cmbGlobalKey1Service.SelectedValue?.ToString() ?? "{B93C105E-64BC-4D9C-B92F-3DDF0C9150E6}";

            _settings.GlobalServiceKeys[1].Key = cmbGlobalKeyService2Key.SelectedValue.ToString();
             _settings.GlobalServiceKeys[1].Modifier1 = cmbGlobalKeyService2Mod1.SelectedValue.ToString();
             _settings.GlobalServiceKeys[1].Modifier2 = cmbGlobalKeyService2Mod2.SelectedValue.ToString();
             _settings.GlobalServiceKeys[1].Modifier3 = cmbGlobalKeyService2Mod3.SelectedValue.ToString();
             _settings.GlobalServiceKeys[1].Modifier4 = cmbGlobalKeyService2Mod4.SelectedValue.ToString();
             _settings.GlobalServiceKeys[1].Guid = cmbGlobalKey2Service.SelectedValue?.ToString() ?? "{B93C105E-64BC-4D9C-B92F-3DDF0C9150E6}";

             _settings.GlobalServiceKeys[2].Key = cmbGlobalKeyService3Key.SelectedValue.ToString();
             _settings.GlobalServiceKeys[2].Modifier1 = cmbGlobalKeyService3Mod1.SelectedValue.ToString();
             _settings.GlobalServiceKeys[2].Modifier2 = cmbGlobalKeyService3Mod2.SelectedValue.ToString();
             _settings.GlobalServiceKeys[2].Modifier3 = cmbGlobalKeyService3Mod3.SelectedValue.ToString();
             _settings.GlobalServiceKeys[2].Modifier4 = cmbGlobalKeyService3Mod4.SelectedValue.ToString();
            _settings.GlobalServiceKeys[2].Guid = cmbGlobalKey3Service.SelectedValue?.ToString() ?? "{B93C105E-64BC-4D9C-B92F-3DDF0C9150E6}";

            _settings.GlobalServiceKeys[3].Key = cmbGlobalKeyService4Key.SelectedValue.ToString();
             _settings.GlobalServiceKeys[3].Modifier1 = cmbGlobalKeyService4Mod1.SelectedValue.ToString();
             _settings.GlobalServiceKeys[3].Modifier2 = cmbGlobalKeyService4Mod2.SelectedValue.ToString();
             _settings.GlobalServiceKeys[3].Modifier3 = cmbGlobalKeyService4Mod3.SelectedValue.ToString();
             _settings.GlobalServiceKeys[3].Modifier4 = cmbGlobalKeyService4Mod4.SelectedValue.ToString();
            _settings.GlobalServiceKeys[3].Guid = cmbGlobalKey4Service.SelectedValue?.ToString() ?? "{B93C105E-64BC-4D9C-B92F-3DDF0C9150E6}";

            _settings.GlobalServiceKeys[4].Key = cmbGlobalKeyService5Key.SelectedValue.ToString();
             _settings.GlobalServiceKeys[4].Modifier1 = cmbGlobalKeyService5Mod1.SelectedValue.ToString();
             _settings.GlobalServiceKeys[4].Modifier2 = cmbGlobalKeyService5Mod2.SelectedValue.ToString();
             _settings.GlobalServiceKeys[4].Modifier3 = cmbGlobalKeyService5Mod3.SelectedValue.ToString();
             _settings.GlobalServiceKeys[4].Modifier4 = cmbGlobalKeyService5Mod4.SelectedValue.ToString();
            _settings.GlobalServiceKeys[4].Guid = cmbGlobalKey5Service.SelectedValue?.ToString() ?? "{B93C105E-64BC-4D9C-B92F-3DDF0C9150E6}";

            #endregion

             #region store global searches
             // Global Searches via hot key
             _settings.GlobalSearchKeys[0].Key = cmbGlobalKeySearch1Key.SelectedValue.ToString();
             _settings.GlobalSearchKeys[0].Modifier1 = cmbGlobalKeySearch1Mod1.SelectedValue.ToString();
             _settings.GlobalSearchKeys[0].Modifier2 = cmbGlobalKeySearch1Mod2.SelectedValue.ToString();
             _settings.GlobalSearchKeys[0].Modifier3 = cmbGlobalKeySearch1Mod3.SelectedValue.ToString();
             _settings.GlobalSearchKeys[0].Modifier4 = cmbGlobalKeySearch1Mod4.SelectedValue.ToString();
             _settings.GlobalSearchKeys[0].SearchName = cmbGlobalKeySearch1SearchName.Text;
             _settings.GlobalSearchKeys[0].SearchTerm = cmbGlobalKeySearch1SearchTerm.Text;

             _settings.GlobalSearchKeys[1].Key = cmbGlobalKeySearch2Key.SelectedValue.ToString();
             _settings.GlobalSearchKeys[1].Modifier1 = cmbGlobalKeySearch2Mod1.SelectedValue.ToString();
             _settings.GlobalSearchKeys[1].Modifier2 = cmbGlobalKeySearch2Mod2.SelectedValue.ToString();
             _settings.GlobalSearchKeys[1].Modifier3 = cmbGlobalKeySearch2Mod3.SelectedValue.ToString();
             _settings.GlobalSearchKeys[1].Modifier4 = cmbGlobalKeySearch2Mod4.SelectedValue.ToString();
             _settings.GlobalSearchKeys[1].SearchName = cmbGlobalKeySearch2SearchName.Text;
             _settings.GlobalSearchKeys[1].SearchTerm = cmbGlobalKeySearch2SearchTerm.Text;

             _settings.GlobalSearchKeys[2].Key = cmbGlobalKeySearch3Key.SelectedValue.ToString();
             _settings.GlobalSearchKeys[2].Modifier1 = cmbGlobalKeySearch3Mod1.SelectedValue.ToString();
             _settings.GlobalSearchKeys[2].Modifier2 = cmbGlobalKeySearch3Mod2.SelectedValue.ToString();
             _settings.GlobalSearchKeys[2].Modifier3 = cmbGlobalKeySearch3Mod3.SelectedValue.ToString();
             _settings.GlobalSearchKeys[2].Modifier4 = cmbGlobalKeySearch3Mod4.SelectedValue.ToString();
             _settings.GlobalSearchKeys[2].SearchName = cmbGlobalKeySearch3SearchName.Text;
             _settings.GlobalSearchKeys[2].SearchTerm = cmbGlobalKeySearch3SearchTerm.Text;

             _settings.GlobalSearchKeys[3].Key = cmbGlobalKeySearch4Key.SelectedValue.ToString();
             _settings.GlobalSearchKeys[3].Modifier1 = cmbGlobalKeySearch4Mod1.SelectedValue.ToString();
             _settings.GlobalSearchKeys[3].Modifier2 = cmbGlobalKeySearch4Mod2.SelectedValue.ToString();
             _settings.GlobalSearchKeys[3].Modifier3 = cmbGlobalKeySearch4Mod3.SelectedValue.ToString();
             _settings.GlobalSearchKeys[3].Modifier4 = cmbGlobalKeySearch4Mod4.SelectedValue.ToString();
             _settings.GlobalSearchKeys[3].SearchName = cmbGlobalKeySearch4SearchName.Text;
             _settings.GlobalSearchKeys[3].SearchTerm = cmbGlobalKeySearch4SearchTerm.Text;

             _settings.GlobalSearchKeys[4].Key = cmbGlobalKeySearch5Key.SelectedValue.ToString();
             _settings.GlobalSearchKeys[4].Modifier1 = cmbGlobalKeySearch5Mod1.SelectedValue.ToString();
             _settings.GlobalSearchKeys[4].Modifier2 = cmbGlobalKeySearch5Mod2.SelectedValue.ToString();
             _settings.GlobalSearchKeys[4].Modifier3 = cmbGlobalKeySearch5Mod3.SelectedValue.ToString();
             _settings.GlobalSearchKeys[4].Modifier4 = cmbGlobalKeySearch5Mod4.SelectedValue.ToString();
             _settings.GlobalSearchKeys[4].SearchName = cmbGlobalKeySearch5SearchName.Text;
             _settings.GlobalSearchKeys[4].SearchTerm = cmbGlobalKeySearch5SearchTerm.Text;
            #endregion



             _settings.QuickSearchName = txtQuickSearchName.Text;
            _settings.FileManagerIsTotalCommander = cbxFileManagerIsTotalCommander.Checked;
            _settings.ShowBookmark = _cbxWithBookmarks.Checked;
            _settings.ShowHistory = _cbxWithHistory.Checked;

            _settings.UpdateServices(); // update dynamic informations like method, texts from configuration
            this._settings.Save();
            _hoReverseGui.ParameterizeShortCutsQueries(); // sets the shortcuts
            _hoReverseGui.ParameterizeShortCutsServices(); // sets the shortcuts

            this.Close();

        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void txtBtn1Text_TextChanged(object sender, EventArgs e)
        {

        }

        private void cmbGlobalKey1Tooltip_TextChanged(object sender, EventArgs e)
        {

        }

        private void SettingsForm_Shown(object sender, EventArgs e)
        {
            TopMost = true;
        }
    }
}
