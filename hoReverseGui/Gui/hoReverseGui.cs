using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using EaServices.Doors;
using EA;
using hoReverse.Settings;
using hoReverse.HistoryList;
using hoReverse.Services;
using hoReverse.hoUtils;
using hoReverse.hoUtils.Cutils;
using hoReverse.hoUtils.Diagrams;
using hoReverse.Reverse.EaAddinShortcuts;
using hoReverse.hoUtils.WiKiRefs;

using hoReverse.Services.AutoCpp;
using File = System.IO.File;
using hoLinqToSql.LinqUtils;

using hoUtils.BulkChange;


// ReSharper disable RedundantDelegateCreation

//--------------------------------------------------------------------------------
// First ActiveX Control
//--------------------------------------------------------------------------------


// ReSharper disable once CheckNamespace
namespace hoReverse.Reverse
{
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    [Guid("C845E70C-EDC5-4D80-8F78-5F4DBF96C8ED")]
    [ProgId("hoReverse.ReverseGui")]
    [ComDefaultInterface(typeof(IhoReverseGui))]

    public class HoReverseGui : UserControl, IhoReverseGui, IObjectSafety
    {
        string Tab = @"\t";
        // The last MenuItem the mouse hovered upon.
        private ToolStripMenuItem _lastMenuItem;

        private EA.Repository _repository;
        private EaHistoryList _history;
        private EaHistoryList _bookmark;
        private AddinSettings _addinSettings;

        private AutoCpp _autoCpp ;
        private bool _autoCppIsRunning;
        private bool _autoCppIsRequested;

        // Handling Settings.json
        // - Diagram Styles
        // - Bulk changes of EA items
        private const string JasonFile = @"Settings.json";
        private string _jasonFilePath;

        private DiagramFormat _diagramStyle;
        private ImportSetting _importSettings;


        // Do Menu entries already inserted
        private bool _doMenuDiagramStyleInserted;

        private string _mGuid = "";

        //private EaHistory m_historyNew = null;
        //private EaBookmark m_bookmarkNew = null;

        private SettingsForm _frmSettings;
        private Settings2.Settings2Forms _frmSettings2;

        private WpfDiagram.Diagram _wpfDiagram;
        private Button _btnLh;
        private Button _btnLv;
        private Button _btnTv;
        public Button BtnTh;
        private Button _btnOs;
        private ToolTip _toolTip;
        private System.ComponentModel.IContainer components;
        private Button _btnDisplayBehavior;
        private Button _btnLocateOperation;
        private Button _btnAddElementNote;
        private Button _btnAddConstraint;
        private Button _btnLocateType;
        private Button _btnFindUsage;
        private EnterTextBox _txtUserText;
        private Button _btnDisplaySpecification;
        private Button _btnComposite;
        private Button _btnOr;
        private Button _btnA;
        private Button _btnD;
        private Button _btnC;
        private Button _btnUpdateActivityParameter;
        private Button _btnBack;
        private Button _btnFrwrd;
        private Button _btnBookmarkAdd;
        private Button _btnBookmarkRemove;
        private Button _btnBookmarkBack;
        private Button _btnBookmarkFrwrd;
        private Button _btnBookmark;
        private Button _btnAction;
        private Button _btnDecision;
        private Button _btnInsert;
        private Button _btnMerge;
        private Button _btnDecisionFromText;
        private MenuStrip _menuStrip1;
        private ToolStripMenuItem _fileToolStripMenuItem;
        private ToolStripMenuItem _saveToolStripMenuItem;
        private ToolStripMenuItem _settingsToolStripMenuItem;
        private ToolStripMenuItem _clearToolStripMenuItem;
        private ToolStripMenuItem _helpToolStripMenuItem;
        private ToolStripMenuItem _aboutToolStripMenuItem;
        private ToolStripMenuItem _doToolStripMenuItem;
        private ToolStripMenuItem _createActivityForOperationToolStripMenuItem;
        private Button _btnHistory;
        private Button _btnActivityCompositeFromText;
        private Button _btnActivity;
        private Button _btnNoteFromText;
        private Button _btnFinal;
        private ToolStripMenuItem _showFolderToolStripMenuItem;
        private ToolStripMenuItem _copyGuidSqlToClipboardToolStripMenuItem;
        private ToolStripMenuItem _createSharedMemoryToolStripMenuItem;
        private ToolStripMenuItem _updateMethodParametersToolStripMenuItem;
        private ToolStripMenuItem _helpF1ToolStripMenuItem;
        private Button _btnBezier;
        private ContextMenuStrip _contextMenuStripTextField;
        private ToolStripMenuItem _deleteToolStripMenuItemTextField;
        private ToolStripMenuItem _insertBeneathNodeToolStripMenuItem;
        private ToolStripMenuItem _quickSearchToolStripMenuItem;
        private ToolStripMenuItem _addCompositeActivityToolStripMenuItem;
        private ToolStripMenuItem _addActivityToolStripMenuItem;
        private ToolStripMenuItem _showAllPortsToolStripMenuItem;
        private ToolStripMenuItem _addFinalToolStripMenuItem;
        private ToolStripMenuItem _addMergeToolStripMenuItem;
        private ToolStripMenuItem _insertTextIntoNodeToolStripMenuItem;
        private Button _btnNoMerge;
        private ToolStripMenuItem _versionControlToolStripMenuItem;
        private ToolStripMenuItem _getVcLatestrecursiveToolStripMenuItem;
        private ToolStripMenuItem _changeXmlPathToolStripMenuItem1;
        private ToolStripMenuItem _codeToolStripMenuItem;
        private ToolStripMenuItem _insertAttributeToolStripMenuItem;
        private ToolStripMenuItem _insertFunctionToolStripMenuItem;
        private ToolStripMenuItem _insertTypedefStructToolStripMenuItem;
        private ToolStripContainer _toolStripContainer1;
        private ToolStrip _toolStrip1;
        private ToolStripButton _toolStripBtn1;
        private ToolStripButton _toolStripBtn2;
        private ToolStripButton _toolStripBtn3;
        private ToolStrip _toolStrip6;
        private ToolStripButton _toolStripBtn11;
        private ToolStripButton _toolStripBtn12;
        private ToolStripButton _toolStripBtn13;
        private ToolStripButton _toolStripBtn14;
        private ToolStripButton _toolStripBtn15;
        private ToolStripMenuItem _setSvnTaggedValuesToolStripMenuItem;
        private ToolStripMenuItem _setSvnKeywordsToolStripMenuItem;
        private ToolStripMenuItem _svnLogToolStripMenuItem;
        private ToolStripMenuItem _svnTortoiseRepobrowserToolStripMenuItem;
        private ToolStripSeparator _toolStripSeparator1;
        private ToolStripSeparator _toolStripSeparator2;
        private ToolStripSeparator _toolStripSeparator3;
        private ToolStripSeparator _toolStripSeparator4;
        private ToolStripMenuItem _setSvnTaggedValuesToolStripMenuItem1;
        private ToolStripMenuItem _showDirectoryToolStripMenuItem;
        private ToolStripSeparator _toolStripSeparator5;
        private Button _btnSplitNodes;
        private Button _btnSplitAll;
        private ToolStripMenuItem _deleteInvisibleuseRealizationDependenciesToolStripMenuItem;
        private ToolStripMenuItem _generateComponentPortsToolStripMenuItem;
        private ToolStripButton _toolStripBtn5;
        private ToolStripButton _toolStripBtn4;
        private ToolStripSeparator _toolStripSeparator6;
        private ToolStripSeparator _toolStripSeparator7;
        private ToolStripMenuItem _maintenanceToolStripMenuItem;
        private ToolStripMenuItem _vCResyncToolStripMenuItem;
        private ToolStripMenuItem _vCxmiReconsileToolStripMenuItem;
        private ToolStripMenuItem _vCGetStateToolStripMenuItem;
        private ToolStripMenuItem _copyReleaseInformationToClipboardToolStripMenuItem;
        private ToolStripMenuItem _showAllPortsActivityParametersToolStripMenuItem;
        private ToolStripSeparator _toolStripSeparator8;
        private ToolStripMenuItem _setting2ConnectorToolStripMenuItem;
        private Button _btnWriteText;
        private ToolStripMenuItem _setMacroToolStripMenuItem;
        private ToolStripMenuItem _addMacroToolStripMenuItem;
        private ToolStripMenuItem _delMacroToolStripMenuItem;
        private ToolStripSeparator _toolStripSeparator;
        private ToolStripMenuItem _inserToolStripMenuItem;
        private Button _btnGuardNo;
        private Button _btnGuardYes;
        private Button _btnGuardSpace;
        private Button _btnFeatureUp;
        private Button _btnFeatureDown;
        private ToolStripMenuItem setFolderToolStripMenuItem;
        private Button _btnAddNoteAndLink;
        private Button _btnCopy;
        private ToolStripMenuItem endIfMacroToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem externToolStripMenuItem;
        private ToolStripMenuItem _updateActionToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripMenuItem standardDiagramToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem _hideAllPortsToolStripMenuItem;
        private ToolStripMenuItem settingsDiagramStylesToolStripMenuItem;
        private ToolStripMenuItem reloadSettingsToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator5;
        private ToolStripMenuItem resetFactorySettingsToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator7;
        private ToolStripSeparator toolStripSeparator9;
        private ToolStripSeparator toolStripSeparator10;
        private ToolStripMenuItem insertFunctionMakeDuplicatesToolStripMenuItem;
        private ToolStripMenuItem _autoToolStripMenuItem;
        private ToolStripMenuItem modulesToolStripMenuItem;
        private ToolStripMenuItem _vCRemoveToolStripMenuItem;
        private ToolStripMenuItem inventoryToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator8;
        private ToolStripMenuItem moveUsageToElementToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator6;
        private ToolStripMenuItem sortAlphabeticToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator11;
        private ToolStripMenuItem readmeToolStripMenuItem;
        private ToolStripMenuItem repoToolStripMenuItem;
        private ToolStripMenuItem helpToolStripMenuItem;
        private ToolStripMenuItem hoToolsToolStripMenuItem;
        private ToolStripMenuItem lineStyleToolStripMenuItem;
        private ToolStripMenuItem _getToolStripMenuItem;
        private ToolStripMenuItem makeRunnableToolStripMenuItem;
        private ToolStripMenuItem makeServicePortToolStripMenuItem;
        private ToolStripMenuItem makeCalloutToolStripMenuItem;
        private ToolStripMenuItem showExternalComponentFunctionsToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator12;
        private System.ComponentModel.BackgroundWorker backgroundWorker;
        private ProgressBar progressBar1;
        private ToolStripMenuItem showFunctionsToolStripMenuItem;
        private ToolStripMenuItem showSymbolDataBaseFoldersToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator13;
        private ToolStripMenuItem analyzeCCToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator14;
        private ToolStripSeparator toolStripSeparator15;
        private ToolStripMenuItem showProvidedRequiredFunctionsForSourceToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator16;
        private ToolStripMenuItem doorsImportcsvToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator17;
        private ToolStripMenuItem doorsImportcsvWithFileDialogToolStripMenuItem;
        private ToolStripMenuItem checkDOORSRequirementsToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator18;
        private ToolStripMenuItem importBySettingsToolStripMenuItem;
        private ToolStripMenuItem importDoorsReqIFBySettingsToolStripMenuItem;
        private ToolStripMenuItem importReqIFBySettingsToolStripMenuItem;
        private ToolStripMenuItem importReqIFBySettings3ToolStripMenuItem;
        private ToolStripMenuItem importReqIFBySettings5ToolStripMenuItem;
        private ToolTip _toolTip1;


        public void Close()
        {
            Save();
        }

        public void Save()
        {
            _bookmark?.SetHistory("bookmark", _mGuid, _bookmark.GetAll());
            _history?.SetHistory("history", _mGuid, _history.GetAll());
            _wpfDiagram?.Close();
            _wpfDiagram = null;
        }

        #region GetSetter
        /// <summary>
        /// After getting the Setting the configuration can be done
        /// </summary>
        public AddinSettings AddinSettings
        {
            private get { return _addinSettings; }
            set
            {
                _addinSettings = value;
                _clearToolStripMenuItem.Visible = false;
                if (_addinSettings.ShowBookmark | _addinSettings.ShowHistory) _clearToolStripMenuItem.Visible = true;

                ParameterizeShortCutsQueries();
                ParameterizeShortCutsServices();
                GetValueSettingsFromJson();

            }
        }

        #endregion

        public void ParameterizeShortCutsQueries()
        {
            for (int pos = 0; pos < _addinSettings.ShortcutsSearch.Length; pos++)
            {
                if (_addinSettings.ShortcutsSearch[pos] == null) continue;
                EaAddinShortcutSearch shortcut = _addinSettings.ShortcutsSearch[pos];
                switch (pos)
                {
                    case 0:
                        _toolStripBtn11.Text = shortcut.keyText;
                        _toolStripBtn11.ToolTipText = shortcut.HelpTextLog;
                        break;
                    case 1:
                        _toolStripBtn12.Text = shortcut.keyText;
                        _toolStripBtn12.ToolTipText = shortcut.HelpTextLog;
                        break;
                    case 2:
                        _toolStripBtn13.Text = shortcut.keyText;
                        _toolStripBtn13.ToolTipText = shortcut.HelpTextLog;
                        break;
                    case 3:
                        _toolStripBtn14.Text = shortcut.keyText;
                        _toolStripBtn14.ToolTipText = shortcut.HelpTextLog;
                        break;
                    case 4:
                        _toolStripBtn15.Text = shortcut.keyText;
                        _toolStripBtn15.ToolTipText = shortcut.HelpTextLog;
                        break;

                }
            }
        }

        public void ParameterizeShortCutsServices()
        {
            for (int pos = 0; pos < _addinSettings.ShortcutsServices.Count; pos++)
            {
                if (_addinSettings.ShortcutsServices[pos] == null) continue;
                ServicesCallConfig service = _addinSettings.ShortcutsServices[pos];
                switch (pos)
                {
                    case 0:
                        _toolStripBtn1.Text = service.ButtonText;
                        _toolStripBtn1.ToolTipText = service.HelpTextLong;
                        break;
                    case 1:
                        _toolStripBtn2.Text = service.ButtonText;
                        _toolStripBtn2.ToolTipText = service.HelpTextLong;
                        break;
                    case 2:
                        _toolStripBtn3.Text = service.ButtonText;
                        _toolStripBtn3.ToolTipText = service.HelpTextLong;
                        break;
                    case 3:
                        _toolStripBtn4.Text = service.ButtonText;
                        _toolStripBtn4.ToolTipText = service.HelpTextLong;
                        break;
                    case 4:
                        _toolStripBtn5.Text = service.ButtonText;
                        _toolStripBtn5.ToolTipText = service.HelpTextLong;
                        break;

                }
            }
        }

        public static bool OpenDiagram(EA.Repository rep, string guid, bool locateDiagramInBrowser = false)
        {
            if (guid == "") return false;
            try
            {
                EA.Diagram dia = (EA.Diagram) rep.GetDiagramByGuid(guid);
                rep.OpenDiagram(dia.DiagramID);

                if (locateDiagramInBrowser)
                {
                    rep.ShowInProjectView(dia);
                }
                return true;
            }
            catch //(Exception e)
            {
                return false;
            }
        }

        #region IActiveX Members

        public string getName()
        {
            return "hoReverse";
        }

        public string GetText()
        {
            return _txtUserText.Text;
        }

        public EA.Repository Repository
        {
            set
            {
                _repository = value;
                _mGuid = _repository.ProjectGUID;
                progressBar1.Value = 0;
                // check for ZF
                _autoToolStripMenuItem.Visible = false;
                progressBar1.Visible = false;
                if (_repository.ConnectionString.Contains("WLE") )
                {
                    // Check if source folder exists
                    if (!_addinSettings.IsFolderPathCSourceCode()) return;
                    _autoToolStripMenuItem.Visible = true;
                    progressBar1.Visible = true;

                    // Create/Update autoCpp generator
                    if (_autoCpp == null) _autoCpp = new AutoCpp(_repository);
                    else _autoCpp.Rep = _repository;


                    // initialize macros in background task
                    if (_autoCppIsRunning)
                    {
                        _autoCppIsRequested = true;
                        backgroundWorker.CancelAsync();
                    }
                    else
                    {
                        _autoCppIsRunning = true;
                        backgroundWorker.RunWorkerAsync();
                    }

                }
                else
                {   // Not with ZF
                    if (backgroundWorker.IsBusy) backgroundWorker.CancelAsync();
                    _autoCppIsRequested = false;
                }

            }
        }

        public EaHistory History
        {
            set => _history = value;
        }

        public EaHistoryList Bookmark
        {
            set => _bookmark = value;
        }

        public string Release
        {
            set { }
        }

        public HoReverseGui()
        {
            InitializeComponent();

        }

        public AddinSettings GetAddinSettings()
        {
            return AddinSettings;
        }

        public int ShowDialog()
        {
            MessageBox.Show(@"MessageBox text");
            return 0;
        }

        #endregion

        #region EventHandler

        void BtnDisplayBehavior_Click(object sender, EventArgs e)
        {
            HoService.DisplayOperationForSelectedElement(_repository, HoService.DisplayMode.Behavior);
        }

        void BtnLocateOperation_Click(object sender, EventArgs e)
        {
            HoService.DisplayOperationForSelectedElement(_repository, HoService.DisplayMode.Method);
        }




        void BtnLocateType_Click(object sender, EventArgs e)
        {
            HoService.LocateType(_repository);
        }



        void BtnShowSpecification_Click(object sender, EventArgs e)
        {
            HoService.ShowSpecification(_repository);
        }


        void BtnUpdateActivityParameter_Click(object sender, EventArgs e)
        {


            try
            {
                Cursor.Current = Cursors.WaitCursor;
                HoService.ReconcileOperationTypesWrapper(_repository);
                HoService.UpdateActivityParameter(_repository);
                Cursor.Current = Cursors.Default;
            }
            catch (Exception e11)
            {
                MessageBox.Show(e11.ToString(), @"Error Insert Function");
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        void BtnFindUsage_Click(object sender, EventArgs e)
        {
            HoService.FindUsage(_repository);
        }


        //---------------------------------------------------------------------------------------------------------------
        // linestyle
        // LH = "Line Style: Lateral Horizontal";
        // LV = "Line Style: Lateral Vertical";
        // TH = "Line Style: Tree Horizontal";
        // TV = "Line Style: Tree Vertical";
        // OS = "Line Style: Orthogonal Square";

        void BtnLH_Click(object sender, EventArgs e)
        {
            HoService.SetLineStyle(_repository, "LH");
        }

        void BtnLV_Click(object sender, EventArgs e)
        {
            HoService.SetLineStyle(_repository, "LV");
        }


        void BtnTH_Click(object sender, EventArgs e)
        {
            HoService.SetLineStyle(_repository, "TH");
        }

        void BtnTV_Click(object sender, EventArgs e)
        {
            HoService.SetLineStyle(_repository, "TV");
        }

        void BtnOS_Click(object sender, EventArgs e)
        {
            HoService.SetLineStyle(_repository, "OS");
        }

        // Shift + Enter runs a search
        void TxtUserText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && e.Shift)
            {
                HoService.RunQuickSearch(_repository, _addinSettings.QuickSearchName, _txtUserText.Text);
                e.Handled = true;
            }
        }

        #endregion

        #region IObjectSafety Members

        [Flags]
        private enum ObjectSafetyOptions
        {
            InterfacesafeForUntrustedCaller = 0x00000001,
            InterfacesafeForUntrustedData = 0x00000002,

            // ReSharper disable once UnusedMember.Local
            InterfaceUsesDispex = 0x00000004,

            // ReSharper disable once UnusedMember.Local
            InterfaceUsesSecurityManager = 0x00000008
        };

        public int GetInterfaceSafetyOptions(ref Guid riid, out int pdwSupportedOptions, out int pdwEnabledOptions)
        {
            ObjectSafetyOptions mOptions = ObjectSafetyOptions.InterfacesafeForUntrustedCaller |
                                           ObjectSafetyOptions.InterfacesafeForUntrustedData;
            pdwSupportedOptions = (int) mOptions;
            pdwEnabledOptions = (int) mOptions;
            return 0;
        }

        public int SetInterfaceSafetyOptions(ref Guid riid, int dwOptionSetMask, int dwEnabledOptions)
        {
            return 0;
        }

        #endregion

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HoReverseGui));
            this._btnLh = new System.Windows.Forms.Button();
            this._btnLv = new System.Windows.Forms.Button();
            this._btnTv = new System.Windows.Forms.Button();
            this.BtnTh = new System.Windows.Forms.Button();
            this._btnOs = new System.Windows.Forms.Button();
            this._toolTip = new System.Windows.Forms.ToolTip(this.components);
            this._btnDisplayBehavior = new System.Windows.Forms.Button();
            this._btnLocateOperation = new System.Windows.Forms.Button();
            this._btnAddElementNote = new System.Windows.Forms.Button();
            this._btnAddConstraint = new System.Windows.Forms.Button();
            this._btnLocateType = new System.Windows.Forms.Button();
            this._btnFindUsage = new System.Windows.Forms.Button();
            this._btnDisplaySpecification = new System.Windows.Forms.Button();
            this._btnComposite = new System.Windows.Forms.Button();
            this._btnOr = new System.Windows.Forms.Button();
            this._btnA = new System.Windows.Forms.Button();
            this._btnD = new System.Windows.Forms.Button();
            this._btnC = new System.Windows.Forms.Button();
            this._btnUpdateActivityParameter = new System.Windows.Forms.Button();
            this._btnBack = new System.Windows.Forms.Button();
            this._btnFrwrd = new System.Windows.Forms.Button();
            this._btnBookmarkAdd = new System.Windows.Forms.Button();
            this._btnBookmarkRemove = new System.Windows.Forms.Button();
            this._btnBookmarkBack = new System.Windows.Forms.Button();
            this._btnBookmarkFrwrd = new System.Windows.Forms.Button();
            this._btnInsert = new System.Windows.Forms.Button();
            this._btnAction = new System.Windows.Forms.Button();
            this._btnDecision = new System.Windows.Forms.Button();
            this._btnMerge = new System.Windows.Forms.Button();
            this._btnDecisionFromText = new System.Windows.Forms.Button();
            this._btnBookmark = new System.Windows.Forms.Button();
            this._btnHistory = new System.Windows.Forms.Button();
            this._btnActivityCompositeFromText = new System.Windows.Forms.Button();
            this._btnActivity = new System.Windows.Forms.Button();
            this._btnNoteFromText = new System.Windows.Forms.Button();
            this._btnFinal = new System.Windows.Forms.Button();
            this._btnBezier = new System.Windows.Forms.Button();
            this._contextMenuStripTextField = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.endIfMacroToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.externToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this._deleteToolStripMenuItemTextField = new System.Windows.Forms.ToolStripMenuItem();
            this._quickSearchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._insertBeneathNodeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._addActivityToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._addCompositeActivityToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._addFinalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._addMergeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._showAllPortsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._insertTextIntoNodeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._btnNoMerge = new System.Windows.Forms.Button();
            this._btnSplitNodes = new System.Windows.Forms.Button();
            this._btnSplitAll = new System.Windows.Forms.Button();
            this._btnWriteText = new System.Windows.Forms.Button();
            this._btnGuardNo = new System.Windows.Forms.Button();
            this._btnGuardYes = new System.Windows.Forms.Button();
            this._btnGuardSpace = new System.Windows.Forms.Button();
            this._btnFeatureUp = new System.Windows.Forms.Button();
            this._btnFeatureDown = new System.Windows.Forms.Button();
            this._btnAddNoteAndLink = new System.Windows.Forms.Button();
            this._btnCopy = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this._txtUserText = new hoReverse.Reverse.EnterTextBox();
            this._menuStrip1 = new System.Windows.Forms.MenuStrip();
            this._fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this._settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._setting2ConnectorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsDiagramStylesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.reloadSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetFactorySettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this._clearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._doToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._createActivityForOperationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._updateMethodParametersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this._showFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this._copyGuidSqlToClipboardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._createSharedMemoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.standardDiagramToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.moveUsageToElementToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.sortAlphabeticToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator11 = new System.Windows.Forms.ToolStripSeparator();
            this._codeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._insertAttributeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._insertTypedefStructToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            this._insertFunctionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.insertFunctionMakeDuplicatesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this._updateActionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this._deleteInvisibleuseRealizationDependenciesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this._generateComponentPortsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._hideAllPortsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._showAllPortsActivityParametersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this._inserToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this._setMacroToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._addMacroToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._delMacroToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._toolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            this._copyReleaseInformationToClipboardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator16 = new System.Windows.Forms.ToolStripSeparator();
            this._autoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.modulesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.inventoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._getToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.makeRunnableToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.makeServicePortToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.makeCalloutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator12 = new System.Windows.Forms.ToolStripSeparator();
            this.showExternalComponentFunctionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showProvidedRequiredFunctionsForSourceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showFunctionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator13 = new System.Windows.Forms.ToolStripSeparator();
            this.showSymbolDataBaseFoldersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._versionControlToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._svnLogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._svnTortoiseRepobrowserToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._showDirectoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this._getVcLatestrecursiveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._setSvnKeywordsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._setSvnTaggedValuesToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this._setSvnTaggedValuesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this._changeXmlPathToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this._toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this._maintenanceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._vCGetStateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._vCResyncToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._vCxmiReconsileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._vCRemoveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator17 = new System.Windows.Forms.ToolStripSeparator();
            this.doorsImportcsvToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.doorsImportcsvWithFileDialogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkDOORSRequirementsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator18 = new System.Windows.Forms.ToolStripSeparator();
            this.importBySettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importDoorsReqIFBySettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importReqIFBySettings3ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importReqIFBySettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importReqIFBySettings5ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator15 = new System.Windows.Forms.ToolStripSeparator();
            this._helpF1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.readmeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.repoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hoToolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lineStyleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator14 = new System.Windows.Forms.ToolStripSeparator();
            this.analyzeCCToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this._toolStrip6 = new System.Windows.Forms.ToolStrip();
            this._toolStripBtn11 = new System.Windows.Forms.ToolStripButton();
            this._toolStripBtn12 = new System.Windows.Forms.ToolStripButton();
            this._toolStripBtn13 = new System.Windows.Forms.ToolStripButton();
            this._toolStripBtn14 = new System.Windows.Forms.ToolStripButton();
            this._toolStripBtn15 = new System.Windows.Forms.ToolStripButton();
            this._toolStrip1 = new System.Windows.Forms.ToolStrip();
            this._toolStripBtn1 = new System.Windows.Forms.ToolStripButton();
            this._toolStripBtn2 = new System.Windows.Forms.ToolStripButton();
            this._toolStripBtn3 = new System.Windows.Forms.ToolStripButton();
            this._toolStripBtn4 = new System.Windows.Forms.ToolStripButton();
            this._toolStripBtn5 = new System.Windows.Forms.ToolStripButton();
            this._toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.backgroundWorker = new System.ComponentModel.BackgroundWorker();
            this._contextMenuStripTextField.SuspendLayout();
            this._menuStrip1.SuspendLayout();
            this._toolStripContainer1.TopToolStripPanel.SuspendLayout();
            this._toolStripContainer1.SuspendLayout();
            this._toolStrip6.SuspendLayout();
            this._toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // _btnLh
            // 
            this._btnLh.Location = new System.Drawing.Point(50, 168);
            this._btnLh.Name = "_btnLh";
            this._btnLh.Size = new System.Drawing.Size(43, 26);
            this._btnLh.TabIndex = 0;
            this._btnLh.Text = "LH";
            this._toolTip.SetToolTip(this._btnLh, "Lateral Horizontal");
            this._btnLh.UseVisualStyleBackColor = true;
            this._btnLh.Click += new System.EventHandler(this.BtnLH_Click);
            // 
            // _btnLv
            // 
            this._btnLv.Location = new System.Drawing.Point(2, 168);
            this._btnLv.Name = "_btnLv";
            this._btnLv.Size = new System.Drawing.Size(45, 26);
            this._btnLv.TabIndex = 2;
            this._btnLv.Text = "LV";
            this._toolTip.SetToolTip(this._btnLv, "Lateral Vertical");
            this._btnLv.UseVisualStyleBackColor = true;
            this._btnLv.Click += new System.EventHandler(this.BtnLV_Click);
            // 
            // _btnTv
            // 
            this._btnTv.Location = new System.Drawing.Point(99, 168);
            this._btnTv.Name = "_btnTv";
            this._btnTv.Size = new System.Drawing.Size(35, 26);
            this._btnTv.TabIndex = 4;
            this._btnTv.Text = "TV";
            this._toolTip.SetToolTip(this._btnTv, "Tree Vertical");
            this._btnTv.UseVisualStyleBackColor = true;
            this._btnTv.Click += new System.EventHandler(this.BtnTV_Click);
            // 
            // BtnTh
            // 
            this.BtnTh.Location = new System.Drawing.Point(143, 168);
            this.BtnTh.Name = "BtnTh";
            this.BtnTh.Size = new System.Drawing.Size(41, 26);
            this.BtnTh.TabIndex = 3;
            this.BtnTh.Text = "TH";
            this._toolTip.SetToolTip(this.BtnTh, "Tree Horizontal");
            this.BtnTh.UseVisualStyleBackColor = true;
            this.BtnTh.Click += new System.EventHandler(this.BtnTH_Click);
            // 
            // _btnOs
            // 
            this._btnOs.Location = new System.Drawing.Point(2, 139);
            this._btnOs.Name = "_btnOs";
            this._btnOs.Size = new System.Drawing.Size(31, 26);
            this._btnOs.TabIndex = 5;
            this._btnOs.Text = "OS";
            this._toolTip.SetToolTip(this._btnOs, "Orthogonal Square");
            this._btnOs.UseVisualStyleBackColor = true;
            this._btnOs.Click += new System.EventHandler(this.BtnOS_Click);
            // 
            // _btnDisplayBehavior
            // 
            this._btnDisplayBehavior.Location = new System.Drawing.Point(2, 205);
            this._btnDisplayBehavior.Name = "_btnDisplayBehavior";
            this._btnDisplayBehavior.Size = new System.Drawing.Size(132, 26);
            this._btnDisplayBehavior.TabIndex = 7;
            this._btnDisplayBehavior.Text = "DisplayBehavior";
            this._toolTip.SetToolTip(this._btnDisplayBehavior, "Display behavior of an operation (activity, statemachine, interaction)");
            this._btnDisplayBehavior.UseVisualStyleBackColor = true;
            this._btnDisplayBehavior.Click += new System.EventHandler(this.BtnDisplayBehavior_Click);
            // 
            // _btnLocateOperation
            // 
            this._btnLocateOperation.Location = new System.Drawing.Point(2, 235);
            this._btnLocateOperation.Name = "_btnLocateOperation";
            this._btnLocateOperation.Size = new System.Drawing.Size(132, 26);
            this._btnLocateOperation.TabIndex = 8;
            this._btnLocateOperation.Text = "Locate Operation";
            this._toolTip.SetToolTip(this._btnLocateOperation, "Locate the linked operation for a behavior (statechart, activity, interaction)");
            this._btnLocateOperation.UseVisualStyleBackColor = true;
            this._btnLocateOperation.Click += new System.EventHandler(this.BtnLocateOperation_Click);
            // 
            // _btnAddElementNote
            // 
            this._btnAddElementNote.Location = new System.Drawing.Point(81, 307);
            this._btnAddElementNote.Name = "_btnAddElementNote";
            this._btnAddElementNote.Size = new System.Drawing.Size(62, 26);
            this._btnAddElementNote.TabIndex = 9;
            this._btnAddElementNote.Text = "Note";
            this._toolTip.SetToolTip(this._btnAddElementNote, "Add Note to selected: \r\n- Elements\r\n- Connector\r\n- Diagram if nothing is selected" +
        "\r\n\r\nThe note is free editable.");
            this._btnAddElementNote.UseVisualStyleBackColor = true;
            this._btnAddElementNote.Click += new System.EventHandler(this._btnAddElementNote_Click);
            // 
            // _btnAddConstraint
            // 
            this._btnAddConstraint.Location = new System.Drawing.Point(149, 307);
            this._btnAddConstraint.Name = "_btnAddConstraint";
            this._btnAddConstraint.Size = new System.Drawing.Size(98, 26);
            this._btnAddConstraint.TabIndex = 10;
            this._btnAddConstraint.Text = "Constraint";
            this._toolTip.SetToolTip(this._btnAddConstraint, "Add Constraint to selected: \r\n- Elements\r\n- Connector\r\n- Diagram if nothing selec" +
        "ted\r\n\r\nThe constraint is free editable.");
            this._btnAddConstraint.UseVisualStyleBackColor = true;
            this._btnAddConstraint.Click += new System.EventHandler(this._btnAddConstraint_Click);
            // 
            // _btnLocateType
            // 
            this._btnLocateType.Location = new System.Drawing.Point(2, 264);
            this._btnLocateType.Name = "_btnLocateType";
            this._btnLocateType.Size = new System.Drawing.Size(132, 26);
            this._btnLocateType.TabIndex = 11;
            this._btnLocateType.Text = "Locate Type";
            this._toolTip.SetToolTip(this._btnLocateType, "Locate to the type of the selected element");
            this._btnLocateType.UseVisualStyleBackColor = true;
            this._btnLocateType.Click += new System.EventHandler(this.BtnLocateType_Click);
            // 
            // _btnFindUsage
            // 
            this._btnFindUsage.Location = new System.Drawing.Point(143, 235);
            this._btnFindUsage.Name = "_btnFindUsage";
            this._btnFindUsage.Size = new System.Drawing.Size(98, 26);
            this._btnFindUsage.TabIndex = 12;
            this._btnFindUsage.Text = "Find Usage";
            this._toolTip.SetToolTip(this._btnFindUsage, "Find the usage of the selected element");
            this._btnFindUsage.UseVisualStyleBackColor = true;
            this._btnFindUsage.Click += new System.EventHandler(this.BtnFindUsage_Click);
            // 
            // _btnDisplaySpecification
            // 
            this._btnDisplaySpecification.Location = new System.Drawing.Point(143, 205);
            this._btnDisplaySpecification.Name = "_btnDisplaySpecification";
            this._btnDisplaySpecification.Size = new System.Drawing.Size(98, 26);
            this._btnDisplaySpecification.TabIndex = 13;
            this._btnDisplaySpecification.Text = "Specification";
            this._toolTip.SetToolTip(this._btnDisplaySpecification, "Display the Specification according to file property");
            this._btnDisplaySpecification.UseVisualStyleBackColor = true;
            this._btnDisplaySpecification.Click += new System.EventHandler(this.BtnShowSpecification_Click);
            // 
            // _btnComposite
            // 
            this._btnComposite.Location = new System.Drawing.Point(143, 264);
            this._btnComposite.Name = "_btnComposite";
            this._btnComposite.Size = new System.Drawing.Size(98, 26);
            this._btnComposite.TabIndex = 16;
            this._btnComposite.Text = "Composite";
            this._toolTip.SetToolTip(this._btnComposite, "Navigate between Element and Composite Diagram");
            this._btnComposite.UseVisualStyleBackColor = true;
            this._btnComposite.Click += new System.EventHandler(this.BtnComposite_Click);
            // 
            // _btnOr
            // 
            this._btnOr.Location = new System.Drawing.Point(43, 139);
            this._btnOr.Name = "_btnOr";
            this._btnOr.Size = new System.Drawing.Size(31, 26);
            this._btnOr.TabIndex = 17;
            this._btnOr.Text = "OR";
            this._toolTip.SetToolTip(this._btnOr, "Orthogonal Rounded");
            this._btnOr.UseVisualStyleBackColor = true;
            this._btnOr.Click += new System.EventHandler(this.BtnOR_Click);
            // 
            // _btnA
            // 
            this._btnA.Location = new System.Drawing.Point(278, 168);
            this._btnA.Name = "_btnA";
            this._btnA.Size = new System.Drawing.Size(38, 26);
            this._btnA.TabIndex = 18;
            this._btnA.Text = "A";
            this._toolTip.SetToolTip(this._btnA, "Orthogonal Rounded");
            this._btnA.UseVisualStyleBackColor = true;
            this._btnA.Click += new System.EventHandler(this.BtnA_Click);
            // 
            // _btnD
            // 
            this._btnD.Location = new System.Drawing.Point(234, 168);
            this._btnD.Name = "_btnD";
            this._btnD.Size = new System.Drawing.Size(38, 26);
            this._btnD.TabIndex = 19;
            this._btnD.Text = "D";
            this._toolTip.SetToolTip(this._btnD, "Direct");
            this._btnD.UseVisualStyleBackColor = true;
            this._btnD.Click += new System.EventHandler(this.BtnD_Click);
            // 
            // _btnC
            // 
            this._btnC.Location = new System.Drawing.Point(190, 168);
            this._btnC.Name = "_btnC";
            this._btnC.Size = new System.Drawing.Size(38, 26);
            this._btnC.TabIndex = 20;
            this._btnC.Text = "C";
            this._toolTip.SetToolTip(this._btnC, "Custom line");
            this._btnC.UseVisualStyleBackColor = true;
            this._btnC.Click += new System.EventHandler(this.BtnC_Click);
            // 
            // _btnUpdateActivityParameter
            // 
            this._btnUpdateActivityParameter.Location = new System.Drawing.Point(3, 341);
            this._btnUpdateActivityParameter.Name = "_btnUpdateActivityParameter";
            this._btnUpdateActivityParameter.Size = new System.Drawing.Size(107, 26);
            this._btnUpdateActivityParameter.TabIndex = 22;
            this._btnUpdateActivityParameter.Text = "Update Parameter";
            this._toolTip.SetToolTip(this._btnUpdateActivityParameter, "Update Operation and Activity Parameter from operation");
            this._btnUpdateActivityParameter.UseVisualStyleBackColor = true;
            this._btnUpdateActivityParameter.Click += new System.EventHandler(this.BtnUpdateActivityParameter_Click);
            // 
            // _btnBack
            // 
            this._btnBack.Location = new System.Drawing.Point(150, 398);
            this._btnBack.Name = "_btnBack";
            this._btnBack.Size = new System.Drawing.Size(20, 26);
            this._btnBack.TabIndex = 23;
            this._btnBack.Text = "<";
            this._toolTip.SetToolTip(this._btnBack, "Diagram back");
            this._btnBack.UseVisualStyleBackColor = true;
            this._btnBack.Click += new System.EventHandler(this.BtnBack_Click);
            // 
            // _btnFrwrd
            // 
            this._btnFrwrd.Location = new System.Drawing.Point(177, 398);
            this._btnFrwrd.Name = "_btnFrwrd";
            this._btnFrwrd.Size = new System.Drawing.Size(21, 26);
            this._btnFrwrd.TabIndex = 24;
            this._btnFrwrd.Text = ">";
            this._toolTip.SetToolTip(this._btnFrwrd, "Diagram forward");
            this._btnFrwrd.UseVisualStyleBackColor = true;
            this._btnFrwrd.Click += new System.EventHandler(this.BtnFrwrd_Click);
            // 
            // _btnBookmarkAdd
            // 
            this._btnBookmarkAdd.Location = new System.Drawing.Point(89, 373);
            this._btnBookmarkAdd.Name = "_btnBookmarkAdd";
            this._btnBookmarkAdd.Size = new System.Drawing.Size(21, 26);
            this._btnBookmarkAdd.TabIndex = 27;
            this._btnBookmarkAdd.Text = "+";
            this._toolTip.SetToolTip(this._btnBookmarkAdd, "Add bookmark");
            this._btnBookmarkAdd.UseVisualStyleBackColor = true;
            this._btnBookmarkAdd.Click += new System.EventHandler(this.BtnBookmarkAdd_Click);
            // 
            // _btnBookmarkRemove
            // 
            this._btnBookmarkRemove.Location = new System.Drawing.Point(118, 373);
            this._btnBookmarkRemove.Name = "_btnBookmarkRemove";
            this._btnBookmarkRemove.Size = new System.Drawing.Size(21, 26);
            this._btnBookmarkRemove.TabIndex = 28;
            this._btnBookmarkRemove.Text = "-";
            this._toolTip.SetToolTip(this._btnBookmarkRemove, "Remove bookmark");
            this._btnBookmarkRemove.UseVisualStyleBackColor = true;
            this._btnBookmarkRemove.Click += new System.EventHandler(this.BtnBookmarkRemove_Click);
            // 
            // _btnBookmarkBack
            // 
            this._btnBookmarkBack.Location = new System.Drawing.Point(150, 373);
            this._btnBookmarkBack.Name = "_btnBookmarkBack";
            this._btnBookmarkBack.Size = new System.Drawing.Size(20, 26);
            this._btnBookmarkBack.TabIndex = 29;
            this._btnBookmarkBack.Text = "<";
            this._toolTip.SetToolTip(this._btnBookmarkBack, "Back in bookmark history");
            this._btnBookmarkBack.UseVisualStyleBackColor = true;
            this._btnBookmarkBack.Click += new System.EventHandler(this.BtnBookmarkBack_Click);
            // 
            // _btnBookmarkFrwrd
            // 
            this._btnBookmarkFrwrd.Location = new System.Drawing.Point(176, 373);
            this._btnBookmarkFrwrd.Name = "_btnBookmarkFrwrd";
            this._btnBookmarkFrwrd.Size = new System.Drawing.Size(21, 26);
            this._btnBookmarkFrwrd.TabIndex = 30;
            this._btnBookmarkFrwrd.Text = ">";
            this._toolTip.SetToolTip(this._btnBookmarkFrwrd, "Forward in bookmark history");
            this._btnBookmarkFrwrd.UseVisualStyleBackColor = true;
            this._btnBookmarkFrwrd.Click += new System.EventHandler(this.BtnBookmarkFrwrd_Click);
            // 
            // _btnInsert
            // 
            this._btnInsert.Location = new System.Drawing.Point(85, 52);
            this._btnInsert.Name = "_btnInsert";
            this._btnInsert.Size = new System.Drawing.Size(23, 26);
            this._btnInsert.TabIndex = 37;
            this._btnInsert.Text = "I";
            this._toolTip.SetToolTip(this._btnInsert, resources.GetString("_btnInsert.ToolTip"));
            this._btnInsert.UseVisualStyleBackColor = true;
            this._btnInsert.Click += new System.EventHandler(this.BtnInsert_Click);
            // 
            // _btnAction
            // 
            this._btnAction.Location = new System.Drawing.Point(369, 273);
            this._btnAction.Name = "_btnAction";
            this._btnAction.Size = new System.Drawing.Size(23, 25);
            this._btnAction.TabIndex = 35;
            this._btnAction.Text = "A";
            this._toolTip.SetToolTip(this._btnAction, "Create an action beneath selected object");
            this._btnAction.UseVisualStyleBackColor = true;
            this._btnAction.Visible = false;
            // 
            // _btnDecision
            // 
            this._btnDecision.Location = new System.Drawing.Point(369, 319);
            this._btnDecision.Name = "_btnDecision";
            this._btnDecision.Size = new System.Drawing.Size(23, 25);
            this._btnDecision.TabIndex = 36;
            this._btnDecision.Text = "D";
            this._toolTip.SetToolTip(this._btnDecision, "Create a decision beneath selected object");
            this._btnDecision.UseVisualStyleBackColor = true;
            this._btnDecision.Visible = false;
            // 
            // _btnMerge
            // 
            this._btnMerge.Location = new System.Drawing.Point(54, 80);
            this._btnMerge.Name = "_btnMerge";
            this._btnMerge.Size = new System.Drawing.Size(23, 25);
            this._btnMerge.TabIndex = 38;
            this._btnMerge.Text = "M";
            this._toolTip.SetToolTip(this._btnMerge, "Create a Merge beneath selected object");
            this._btnMerge.UseVisualStyleBackColor = true;
            this._btnMerge.Click += new System.EventHandler(this.BtnMerge_Click);
            // 
            // _btnDecisionFromText
            // 
            this._btnDecisionFromText.Location = new System.Drawing.Point(369, 349);
            this._btnDecisionFromText.Name = "_btnDecisionFromText";
            this._btnDecisionFromText.Size = new System.Drawing.Size(23, 25);
            this._btnDecisionFromText.TabIndex = 39;
            this._btnDecisionFromText.Text = "D";
            this._toolTip.SetToolTip(this._btnDecisionFromText, "Create Decision with text beneath selected element");
            this._btnDecisionFromText.UseVisualStyleBackColor = true;
            this._btnDecisionFromText.Visible = false;
            // 
            // _btnBookmark
            // 
            this._btnBookmark.Location = new System.Drawing.Point(2, 373);
            this._btnBookmark.Name = "_btnBookmark";
            this._btnBookmark.Size = new System.Drawing.Size(79, 26);
            this._btnBookmark.TabIndex = 34;
            this._btnBookmark.Text = "Bookmarks:";
            this._toolTip.SetToolTip(this._btnBookmark, "Show bookmarks");
            this._btnBookmark.UseVisualStyleBackColor = true;
            this._btnBookmark.Click += new System.EventHandler(this.BtnBookmarks_Click);
            // 
            // _btnHistory
            // 
            this._btnHistory.Location = new System.Drawing.Point(2, 403);
            this._btnHistory.Name = "_btnHistory";
            this._btnHistory.Size = new System.Drawing.Size(79, 26);
            this._btnHistory.TabIndex = 42;
            this._btnHistory.Text = "History:";
            this._toolTip.SetToolTip(this._btnHistory, "Show history");
            this._btnHistory.UseVisualStyleBackColor = true;
            this._btnHistory.Click += new System.EventHandler(this.BtnHistory_Click);
            // 
            // _btnActivityCompositeFromText
            // 
            this._btnActivityCompositeFromText.Location = new System.Drawing.Point(3, 52);
            this._btnActivityCompositeFromText.Name = "_btnActivityCompositeFromText";
            this._btnActivityCompositeFromText.Size = new System.Drawing.Size(47, 26);
            this._btnActivityCompositeFromText.TabIndex = 43;
            this._btnActivityCompositeFromText.Text = "ActC";
            this._toolTip.SetToolTip(this._btnActivityCompositeFromText, "Create Activity with Composite Diagram and text beneath selected element");
            this._btnActivityCompositeFromText.UseVisualStyleBackColor = true;
            this._btnActivityCompositeFromText.Click += new System.EventHandler(this.BtnActivityCompositeFromText_Click);
            // 
            // _btnActivity
            // 
            this._btnActivity.Location = new System.Drawing.Point(3, 80);
            this._btnActivity.Name = "_btnActivity";
            this._btnActivity.Size = new System.Drawing.Size(47, 25);
            this._btnActivity.TabIndex = 44;
            this._btnActivity.Text = "Act";
            this._toolTip.SetToolTip(this._btnActivity, "Create an Activity beneath selected object.\r\n\r\nThis is useful for e.g. FOR or WHI" +
        "LE loop");
            this._btnActivity.UseVisualStyleBackColor = true;
            this._btnActivity.Click += new System.EventHandler(this.BtnActivityFromText_Click);
            // 
            // _btnNoteFromText
            // 
            this._btnNoteFromText.Location = new System.Drawing.Point(247, 205);
            this._btnNoteFromText.Name = "_btnNoteFromText";
            this._btnNoteFromText.Size = new System.Drawing.Size(69, 25);
            this._btnNoteFromText.TabIndex = 45;
            this._btnNoteFromText.Text = "N";
            this._toolTip.SetToolTip(this._btnNoteFromText, "Insert text into Element Note.\r\n\r\nIt remove \"//\", \'/*\' and \'*/\'");
            this._btnNoteFromText.UseVisualStyleBackColor = true;
            this._btnNoteFromText.Visible = false;
            this._btnNoteFromText.Click += new System.EventHandler(this.BtnNoteFromText_Click);
            // 
            // _btnFinal
            // 
            this._btnFinal.Location = new System.Drawing.Point(56, 52);
            this._btnFinal.Name = "_btnFinal";
            this._btnFinal.Size = new System.Drawing.Size(23, 26);
            this._btnFinal.TabIndex = 46;
            this._btnFinal.Text = "F";
            this._toolTip.SetToolTip(this._btnFinal, "If Behavior: Create a Final beneath the selected object. After that, it shrinks t" +
        "he Activity to fit the enclosed Nodes.\r\n\r\nIf Structural Element: Insert & update" +
        "s C- functions from code.");
            this._btnFinal.UseVisualStyleBackColor = true;
            this._btnFinal.Click += new System.EventHandler(this.BtnFinal_Click);
            // 
            // _btnBezier
            // 
            this._btnBezier.Location = new System.Drawing.Point(81, 139);
            this._btnBezier.Name = "_btnBezier";
            this._btnBezier.Size = new System.Drawing.Size(16, 26);
            this._btnBezier.TabIndex = 48;
            this._btnBezier.Text = "B";
            this._toolTip.SetToolTip(this._btnBezier, "Linestyle Bezier");
            this._btnBezier.UseVisualStyleBackColor = true;
            this._btnBezier.Click += new System.EventHandler(this.BtnBezier_Click);
            // 
            // _contextMenuStripTextField
            // 
            this._contextMenuStripTextField.ImageScalingSize = new System.Drawing.Size(20, 20);
            this._contextMenuStripTextField.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.endIfMacroToolStripMenuItem,
            this.externToolStripMenuItem,
            this.toolStripSeparator2,
            this._deleteToolStripMenuItemTextField,
            this._quickSearchToolStripMenuItem,
            this._insertBeneathNodeToolStripMenuItem,
            this._addActivityToolStripMenuItem,
            this._addCompositeActivityToolStripMenuItem,
            this._addFinalToolStripMenuItem,
            this._addMergeToolStripMenuItem,
            this._showAllPortsToolStripMenuItem,
            this._insertTextIntoNodeToolStripMenuItem});
            this._contextMenuStripTextField.Name = "_contextMenuStripTextField";
            this._contextMenuStripTextField.Size = new System.Drawing.Size(230, 252);
            this._toolTip.SetToolTip(this._contextMenuStripTextField, "Show all ports of selected classifier");
            // 
            // endIfMacroToolStripMenuItem
            // 
            this.endIfMacroToolStripMenuItem.Name = "endIfMacroToolStripMenuItem";
            this.endIfMacroToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            this.endIfMacroToolStripMenuItem.Text = "#endif";
            this.endIfMacroToolStripMenuItem.ToolTipText = "Write #endif into selected element";
            this.endIfMacroToolStripMenuItem.Click += new System.EventHandler(this.EndifMacroToolStripMenuItem_Click);
            // 
            // externToolStripMenuItem
            // 
            this.externToolStripMenuItem.Name = "externToolStripMenuItem";
            this.externToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            this.externToolStripMenuItem.Text = "extern Function/Variable";
            this.externToolStripMenuItem.ToolTipText = "Set the stereotype <<extern>> for function/variable";
            this.externToolStripMenuItem.Click += new System.EventHandler(this.ExternToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(226, 6);
            // 
            // _deleteToolStripMenuItemTextField
            // 
            this._deleteToolStripMenuItemTextField.Name = "_deleteToolStripMenuItemTextField";
            this._deleteToolStripMenuItemTextField.Size = new System.Drawing.Size(229, 22);
            this._deleteToolStripMenuItemTextField.Text = "&Delete";
            this._deleteToolStripMenuItemTextField.ToolTipText = "Delete the text box.";
            this._deleteToolStripMenuItemTextField.Click += new System.EventHandler(this.DeleteToolStripMenuItemTextField_Click);
            // 
            // _quickSearchToolStripMenuItem
            // 
            this._quickSearchToolStripMenuItem.Name = "_quickSearchToolStripMenuItem";
            this._quickSearchToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            this._quickSearchToolStripMenuItem.Text = "&Quick Search";
            this._quickSearchToolStripMenuItem.ToolTipText = "Call the quick search.";
            this._quickSearchToolStripMenuItem.Click += new System.EventHandler(this.QuickSearchToolStripMenuItem_Click);
            // 
            // _insertBeneathNodeToolStripMenuItem
            // 
            this._insertBeneathNodeToolStripMenuItem.Name = "_insertBeneathNodeToolStripMenuItem";
            this._insertBeneathNodeToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            this._insertBeneathNodeToolStripMenuItem.Text = "&Insert Code";
            this._insertBeneathNodeToolStripMenuItem.ToolTipText = "Insert Code beneatch selected node in Activity Diagram";
            this._insertBeneathNodeToolStripMenuItem.Click += new System.EventHandler(this.InsertBeneathNodeToolStripMenuItem_Click);
            // 
            // _addActivityToolStripMenuItem
            // 
            this._addActivityToolStripMenuItem.Name = "_addActivityToolStripMenuItem";
            this._addActivityToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            this._addActivityToolStripMenuItem.Text = "&Activity";
            this._addActivityToolStripMenuItem.ToolTipText = "Add Activity beneath selected Node";
            this._addActivityToolStripMenuItem.Click += new System.EventHandler(this.AddActivityToolStripMenuItem_Click);
            // 
            // _addCompositeActivityToolStripMenuItem
            // 
            this._addCompositeActivityToolStripMenuItem.Name = "_addCompositeActivityToolStripMenuItem";
            this._addCompositeActivityToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            this._addCompositeActivityToolStripMenuItem.Text = "&Composite Activity";
            this._addCompositeActivityToolStripMenuItem.ToolTipText = "Add Composite Activity beneatch selected Node";
            this._addCompositeActivityToolStripMenuItem.Click += new System.EventHandler(this.AddCompositeActivityToolStripMenuItem_Click);
            // 
            // _addFinalToolStripMenuItem
            // 
            this._addFinalToolStripMenuItem.Name = "_addFinalToolStripMenuItem";
            this._addFinalToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            this._addFinalToolStripMenuItem.Text = "&Final";
            this._addFinalToolStripMenuItem.ToolTipText = "Add Final beneath selected Node";
            this._addFinalToolStripMenuItem.Click += new System.EventHandler(this.AddFinalToolStripMenuItem_Click);
            // 
            // _addMergeToolStripMenuItem
            // 
            this._addMergeToolStripMenuItem.Name = "_addMergeToolStripMenuItem";
            this._addMergeToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            this._addMergeToolStripMenuItem.Text = "&Merge";
            this._addMergeToolStripMenuItem.ToolTipText = "Add merge beneath selected node";
            this._addMergeToolStripMenuItem.Click += new System.EventHandler(this.AddMergeToolStripMenuItem_Click);
            // 
            // _showAllPortsToolStripMenuItem
            // 
            this._showAllPortsToolStripMenuItem.Name = "_showAllPortsToolStripMenuItem";
            this._showAllPortsToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            this._showAllPortsToolStripMenuItem.Text = "&Show all ports";
            this._showAllPortsToolStripMenuItem.ToolTipText = "Show all ports of selected classifier";
            this._showAllPortsToolStripMenuItem.Click += new System.EventHandler(this.ShowAllPortsToolStripMenuItem_Click);
            // 
            // _insertTextIntoNodeToolStripMenuItem
            // 
            this._insertTextIntoNodeToolStripMenuItem.Name = "_insertTextIntoNodeToolStripMenuItem";
            this._insertTextIntoNodeToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            this._insertTextIntoNodeToolStripMenuItem.Text = "&Insert text into Element Notes";
            this._insertTextIntoNodeToolStripMenuItem.ToolTipText = "Insert text into seleted Element Notes";
            this._insertTextIntoNodeToolStripMenuItem.Click += new System.EventHandler(this.InsertTextIntoNodeToolStripMenuItem_Click);
            // 
            // _btnNoMerge
            // 
            this._btnNoMerge.Location = new System.Drawing.Point(81, 80);
            this._btnNoMerge.Name = "_btnNoMerge";
            this._btnNoMerge.Size = new System.Drawing.Size(33, 25);
            this._btnNoMerge.TabIndex = 49;
            this._btnNoMerge.Text = "nM";
            this._toolTip.SetToolTip(this._btnNoMerge, "Create a no Merge beneath/right  selected object");
            this._btnNoMerge.UseVisualStyleBackColor = true;
            this._btnNoMerge.Click += new System.EventHandler(this.BtnNoMerge_Click);
            // 
            // _btnSplitNodes
            // 
            this._btnSplitNodes.Location = new System.Drawing.Point(131, 108);
            this._btnSplitNodes.Name = "_btnSplitNodes";
            this._btnSplitNodes.Size = new System.Drawing.Size(23, 25);
            this._btnSplitNodes.TabIndex = 54;
            this._btnSplitNodes.Text = "S";
            this._toolTip.SetToolTip(this._btnSplitNodes, resources.GetString("_btnSplitNodes.ToolTip"));
            this._btnSplitNodes.UseVisualStyleBackColor = true;
            this._btnSplitNodes.Click += new System.EventHandler(this.BtnSplitNodes_Click);
            // 
            // _btnSplitAll
            // 
            this._btnSplitAll.Location = new System.Drawing.Point(99, 108);
            this._btnSplitAll.Name = "_btnSplitAll";
            this._btnSplitAll.Size = new System.Drawing.Size(29, 25);
            this._btnSplitAll.TabIndex = 55;
            this._btnSplitAll.Text = "SA";
            this._toolTip.SetToolTip(this._btnSplitAll, "Split / disconnect all nodes around the last selected element.\r\n\r\n- Select an Ele" +
        "ment\r\n- Click on SA (disconnect all nodes)\r\n-  hoReverse disconnect all connecto" +
        "rs from this element.");
            this._btnSplitAll.UseVisualStyleBackColor = true;
            this._btnSplitAll.Click += new System.EventHandler(this.BtnSplitAll_Click);
            // 
            // _btnWriteText
            // 
            this._btnWriteText.Location = new System.Drawing.Point(115, 55);
            this._btnWriteText.Name = "_btnWriteText";
            this._btnWriteText.Size = new System.Drawing.Size(38, 26);
            this._btnWriteText.TabIndex = 56;
            this._btnWriteText.Text = "WT";
            this._toolTip.SetToolTip(this._btnWriteText, resources.GetString("_btnWriteText.ToolTip"));
            this._btnWriteText.UseVisualStyleBackColor = true;
            this._btnWriteText.Click += new System.EventHandler(this._btnWriteText_Click);
            // 
            // _btnGuardNo
            // 
            this._btnGuardNo.Location = new System.Drawing.Point(3, 108);
            this._btnGuardNo.Name = "_btnGuardNo";
            this._btnGuardNo.Size = new System.Drawing.Size(29, 25);
            this._btnGuardNo.TabIndex = 53;
            this._btnGuardNo.Text = "[N]";
            this._toolTip.SetToolTip(this._btnGuardNo, "Make a [no] Guard for an existing Control Flow\r\nConnect two selected Diagram node" +
        " with a [no] Control Flow\r\n");
            this._btnGuardNo.UseVisualStyleBackColor = true;
            this._btnGuardNo.Click += new System.EventHandler(this.BtnNoGuard_Click);
            // 
            // _btnGuardYes
            // 
            this._btnGuardYes.Location = new System.Drawing.Point(37, 108);
            this._btnGuardYes.Name = "_btnGuardYes";
            this._btnGuardYes.Size = new System.Drawing.Size(29, 25);
            this._btnGuardYes.TabIndex = 53;
            this._btnGuardYes.Text = "[Y]";
            this._toolTip.SetToolTip(this._btnGuardYes, "Make a [yes] Guard for an existing Control Flow\r\nConnect two selected Diagram nod" +
        "e with a [yes] Control Flow");
            this._btnGuardYes.UseVisualStyleBackColor = true;
            this._btnGuardYes.Click += new System.EventHandler(this.BtnYesGuard_Click);
            // 
            // _btnGuardSpace
            // 
            this._btnGuardSpace.Location = new System.Drawing.Point(69, 108);
            this._btnGuardSpace.Name = "_btnGuardSpace";
            this._btnGuardSpace.Size = new System.Drawing.Size(26, 25);
            this._btnGuardSpace.TabIndex = 53;
            this._btnGuardSpace.Text = "[]";
            this._toolTip.SetToolTip(this._btnGuardSpace, "Set connector to Blank if exists or\r\njoin selected diagram nodes with last select" +
        "ed node.");
            this._btnGuardSpace.UseVisualStyleBackColor = true;
            this._btnGuardSpace.Click += new System.EventHandler(this.BtnBlankGuard_Click);
            // 
            // _btnFeatureUp
            // 
            this._btnFeatureUp.Image = ((System.Drawing.Image)(resources.GetObject("_btnFeatureUp.Image")));
            this._btnFeatureUp.Location = new System.Drawing.Point(101, 137);
            this._btnFeatureUp.Margin = new System.Windows.Forms.Padding(0);
            this._btnFeatureUp.Name = "_btnFeatureUp";
            this._btnFeatureUp.Size = new System.Drawing.Size(20, 25);
            this._btnFeatureUp.TabIndex = 55;
            this._toolTip.SetToolTip(this._btnFeatureUp, "Feature (Attribute, Method) up\r\n\r\nNote:\r\nIn settings the automatic ordering has t" +
        "o be disabled (Feture, Attribute, Method/Operation).\r\n");
            this._btnFeatureUp.UseVisualStyleBackColor = true;
            this._btnFeatureUp.Click += new System.EventHandler(this.BtnFeatureUp_Click);
            // 
            // _btnFeatureDown
            // 
            this._btnFeatureDown.Image = ((System.Drawing.Image)(resources.GetObject("_btnFeatureDown.Image")));
            this._btnFeatureDown.Location = new System.Drawing.Point(131, 137);
            this._btnFeatureDown.Margin = new System.Windows.Forms.Padding(0);
            this._btnFeatureDown.Name = "_btnFeatureDown";
            this._btnFeatureDown.Size = new System.Drawing.Size(22, 25);
            this._btnFeatureDown.TabIndex = 55;
            this._toolTip.SetToolTip(this._btnFeatureDown, "Feature (Attribute, Method) down\r\n\r\nNote:\r\nIn settings the automatic ordering has" +
        " to be disabled (Feature, Attribute, Method/Operation).");
            this._btnFeatureDown.UseVisualStyleBackColor = true;
            this._btnFeatureDown.Click += new System.EventHandler(this.BtnFeatureDown_Click);
            // 
            // _btnAddNoteAndLink
            // 
            this._btnAddNoteAndLink.Location = new System.Drawing.Point(3, 307);
            this._btnAddNoteAndLink.Name = "_btnAddNoteAndLink";
            this._btnAddNoteAndLink.Size = new System.Drawing.Size(71, 26);
            this._btnAddNoteAndLink.TabIndex = 9;
            this._btnAddNoteAndLink.Text = "Feature";
            this._toolTip.SetToolTip(this._btnAddNoteAndLink, resources.GetString("_btnAddNoteAndLink.ToolTip"));
            this._btnAddNoteAndLink.UseVisualStyleBackColor = true;
            this._btnAddNoteAndLink.Click += new System.EventHandler(this._btnAddNoteAndLink_Click);
            // 
            // _btnCopy
            // 
            this._btnCopy.Location = new System.Drawing.Point(115, 80);
            this._btnCopy.Name = "_btnCopy";
            this._btnCopy.Size = new System.Drawing.Size(38, 26);
            this._btnCopy.TabIndex = 19;
            this._btnCopy.Text = "RT";
            this._toolTip.SetToolTip(this._btnCopy, resources.GetString("_btnCopy.ToolTip"));
            this._btnCopy.UseVisualStyleBackColor = true;
            this._btnCopy.Click += new System.EventHandler(this._btnCopy_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(0, 21);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(218, 2);
            this.progressBar1.TabIndex = 57;
            this._toolTip.SetToolTip(this.progressBar1, "Show progress of initializing C-Macros");
            this.progressBar1.Visible = false;
            // 
            // _txtUserText
            // 
            this._txtUserText.AcceptsReturn = true;
            this._txtUserText.AcceptsTab = true;
            this._txtUserText.AllowDrop = true;
            this._txtUserText.ContextMenuStrip = this._contextMenuStripTextField;
            this._txtUserText.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._txtUserText.Location = new System.Drawing.Point(160, 50);
            this._txtUserText.Multiline = true;
            this._txtUserText.Name = "_txtUserText";
            this._txtUserText.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this._txtUserText.Size = new System.Drawing.Size(695, 112);
            this._txtUserText.TabIndex = 14;
            this._toolTip.SetToolTip(this._txtUserText, "Code:\r\n1. Enter Code\r\n2. Double click to insert text/code\r\n3. Ctrl+Enter for new " +
        "line\r\n4. Shft+Enter run Query\r\n\r\nMake sure a code line is terminated by a semico" +
        "lon as in C.");
            this._txtUserText.WordWrap = false;
            this._txtUserText.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TxtUserText_KeyDown);
            this._txtUserText.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.TxtUserText_MouseDoubleClick);
            // 
            // _menuStrip1
            // 
            this._menuStrip1.AllowDrop = true;
            this._menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this._menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._fileToolStripMenuItem,
            this._doToolStripMenuItem,
            this._codeToolStripMenuItem,
            this._autoToolStripMenuItem,
            this._versionControlToolStripMenuItem,
            this._maintenanceToolStripMenuItem,
            this._helpToolStripMenuItem,
            this.helpToolStripMenuItem});
            this._menuStrip1.Location = new System.Drawing.Point(0, 0);
            this._menuStrip1.Name = "_menuStrip1";
            this._menuStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this._menuStrip1.Size = new System.Drawing.Size(875, 24);
            this._menuStrip1.TabIndex = 41;
            this._menuStrip1.Text = "menuStrip1";
            // 
            // _fileToolStripMenuItem
            // 
            this._fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._saveToolStripMenuItem,
            this.toolStripSeparator7,
            this._settingsToolStripMenuItem,
            this._setting2ConnectorToolStripMenuItem,
            this.settingsDiagramStylesToolStripMenuItem,
            this.toolStripSeparator5,
            this.reloadSettingsToolStripMenuItem,
            this.resetFactorySettingsToolStripMenuItem,
            this.toolStripSeparator9,
            this._clearToolStripMenuItem});
            this._fileToolStripMenuItem.Name = "_fileToolStripMenuItem";
            this._fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this._fileToolStripMenuItem.Text = "&File";
            this._fileToolStripMenuItem.ToolTipText = "Reload the setting. \r\n- Settings.json (DiagramStyles)";
            // 
            // _saveToolStripMenuItem
            // 
            this._saveToolStripMenuItem.Name = "_saveToolStripMenuItem";
            this._saveToolStripMenuItem.Size = new System.Drawing.Size(374, 22);
            this._saveToolStripMenuItem.Text = "&Save";
            this._saveToolStripMenuItem.ToolTipText = "Save bookmarks and history";
            this._saveToolStripMenuItem.Click += new System.EventHandler(this.SaveToolStripMenuItem_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(371, 6);
            // 
            // _settingsToolStripMenuItem
            // 
            this._settingsToolStripMenuItem.Name = "_settingsToolStripMenuItem";
            this._settingsToolStripMenuItem.Size = new System.Drawing.Size(374, 22);
            this._settingsToolStripMenuItem.Text = "Settings";
            this._settingsToolStripMenuItem.ToolTipText = "Opens the setting menu";
            this._settingsToolStripMenuItem.Click += new System.EventHandler(this.SettingsToolStripMenuItem_Click);
            // 
            // _setting2ConnectorToolStripMenuItem
            // 
            this._setting2ConnectorToolStripMenuItem.Name = "_setting2ConnectorToolStripMenuItem";
            this._setting2ConnectorToolStripMenuItem.Size = new System.Drawing.Size(374, 22);
            this._setting2ConnectorToolStripMenuItem.Text = "Setting: Default Linestyle";
            this._setting2ConnectorToolStripMenuItem.ToolTipText = "Set the default Linestyle for diagrams";
            this._setting2ConnectorToolStripMenuItem.Click += new System.EventHandler(this.Setting2ConnectorToolStripMenuItem_Click);
            // 
            // settingsDiagramStylesToolStripMenuItem
            // 
            this.settingsDiagramStylesToolStripMenuItem.Name = "settingsDiagramStylesToolStripMenuItem";
            this.settingsDiagramStylesToolStripMenuItem.Size = new System.Drawing.Size(374, 22);
            this.settingsDiagramStylesToolStripMenuItem.Text = "Settings: ReqIf, \'Bulk change\', Styles & more (Settings.json)";
            this.settingsDiagramStylesToolStripMenuItem.ToolTipText = resources.GetString("settingsDiagramStylesToolStripMenuItem.ToolTipText");
            this.settingsDiagramStylesToolStripMenuItem.Click += new System.EventHandler(this.SettingsDiagramStylesToolStripMenuItem_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(371, 6);
            // 
            // reloadSettingsToolStripMenuItem
            // 
            this.reloadSettingsToolStripMenuItem.Name = "reloadSettingsToolStripMenuItem";
            this.reloadSettingsToolStripMenuItem.Size = new System.Drawing.Size(374, 22);
            this.reloadSettingsToolStripMenuItem.Text = "Reload Settings from Settings.json";
            this.reloadSettingsToolStripMenuItem.ToolTipText = "Load Settings.json from %appdata%\\ho\\hoReverse\\Settings.json";
            this.reloadSettingsToolStripMenuItem.Click += new System.EventHandler(this.ReloadSettingsToolStripMenuItem_Click);
            // 
            // resetFactorySettingsToolStripMenuItem
            // 
            this.resetFactorySettingsToolStripMenuItem.Name = "resetFactorySettingsToolStripMenuItem";
            this.resetFactorySettingsToolStripMenuItem.Size = new System.Drawing.Size(374, 22);
            this.resetFactorySettingsToolStripMenuItem.Text = "ResetFactorySettings";
            this.resetFactorySettingsToolStripMenuItem.ToolTipText = "Reset the user.config to reset to delivery configuration.\r\n\r\nPlease restart. hoRe" +
    "verse will  create a new user.config with the default settings.";
            this.resetFactorySettingsToolStripMenuItem.Click += new System.EventHandler(this.ResetFactorySettingsToolStripMenuItem_Click);
            // 
            // toolStripSeparator9
            // 
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            this.toolStripSeparator9.Size = new System.Drawing.Size(371, 6);
            // 
            // _clearToolStripMenuItem
            // 
            this._clearToolStripMenuItem.Name = "_clearToolStripMenuItem";
            this._clearToolStripMenuItem.Size = new System.Drawing.Size(374, 22);
            this._clearToolStripMenuItem.Text = "Clear Diagram History and Bookmarks";
            this._clearToolStripMenuItem.ToolTipText = "Delete all history and diagram entries for all projects.";
            this._clearToolStripMenuItem.Click += new System.EventHandler(this.ClearToolStripMenuItem_Click);
            // 
            // _doToolStripMenuItem
            // 
            this._doToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._createActivityForOperationToolStripMenuItem,
            this._updateMethodParametersToolStripMenuItem,
            this._toolStripSeparator3,
            this._showFolderToolStripMenuItem,
            this.setFolderToolStripMenuItem,
            this._toolStripSeparator4,
            this._copyGuidSqlToClipboardToolStripMenuItem,
            this._createSharedMemoryToolStripMenuItem,
            this.toolStripSeparator4,
            this.standardDiagramToolStripMenuItem,
            this.toolStripSeparator8,
            this.moveUsageToElementToolStripMenuItem,
            this.toolStripSeparator6,
            this.sortAlphabeticToolStripMenuItem,
            this.toolStripSeparator11});
            this._doToolStripMenuItem.Name = "_doToolStripMenuItem";
            this._doToolStripMenuItem.Size = new System.Drawing.Size(34, 20);
            this._doToolStripMenuItem.Text = "&Do";
            // 
            // _createActivityForOperationToolStripMenuItem
            // 
            this._createActivityForOperationToolStripMenuItem.Name = "_createActivityForOperationToolStripMenuItem";
            this._createActivityForOperationToolStripMenuItem.Size = new System.Drawing.Size(268, 22);
            this._createActivityForOperationToolStripMenuItem.Text = "&Create/Update Activity for Operation";
            this._createActivityForOperationToolStripMenuItem.ToolTipText = resources.GetString("_createActivityForOperationToolStripMenuItem.ToolTipText");
            this._createActivityForOperationToolStripMenuItem.Click += new System.EventHandler(this.CreateActivityForOperationToolStripMenuItem_Click);
            // 
            // _updateMethodParametersToolStripMenuItem
            // 
            this._updateMethodParametersToolStripMenuItem.Name = "_updateMethodParametersToolStripMenuItem";
            this._updateMethodParametersToolStripMenuItem.Size = new System.Drawing.Size(268, 22);
            this._updateMethodParametersToolStripMenuItem.Text = "&Update Activity from Method";
            this._updateMethodParametersToolStripMenuItem.ToolTipText = "Updates the Activities according to selected contexts by:\r\n- Activity Name\r\n- Act" +
    "ivity Parameter\r\n";
            this._updateMethodParametersToolStripMenuItem.Click += new System.EventHandler(this.UpdateMethodParametersToolStripMenuItem_Click);
            // 
            // _toolStripSeparator3
            // 
            this._toolStripSeparator3.Name = "_toolStripSeparator3";
            this._toolStripSeparator3.Size = new System.Drawing.Size(265, 6);
            // 
            // _showFolderToolStripMenuItem
            // 
            this._showFolderToolStripMenuItem.Name = "_showFolderToolStripMenuItem";
            this._showFolderToolStripMenuItem.Size = new System.Drawing.Size(268, 22);
            this._showFolderToolStripMenuItem.Text = "&Show folder";
            this._showFolderToolStripMenuItem.ToolTipText = "Show folder (sourse file, controled package)";
            this._showFolderToolStripMenuItem.Click += new System.EventHandler(this.ShowFolderToolStripMenuItem_Click);
            // 
            // setFolderToolStripMenuItem
            // 
            this.setFolderToolStripMenuItem.Name = "setFolderToolStripMenuItem";
            this.setFolderToolStripMenuItem.Size = new System.Drawing.Size(268, 22);
            this.setFolderToolStripMenuItem.Text = "Set Folder";
            this.setFolderToolStripMenuItem.ToolTipText = "Set the Folder of a package  to easily access code.\r\n\r\nThe folder is used for imp" +
    "lementations. \r\nSo make sure you have assigned a Package language like \r\nC,C++.";
            this.setFolderToolStripMenuItem.Click += new System.EventHandler(this.SetFolderToolStripMenuItem_Click);
            // 
            // _toolStripSeparator4
            // 
            this._toolStripSeparator4.Name = "_toolStripSeparator4";
            this._toolStripSeparator4.Size = new System.Drawing.Size(265, 6);
            // 
            // _copyGuidSqlToClipboardToolStripMenuItem
            // 
            this._copyGuidSqlToClipboardToolStripMenuItem.Name = "_copyGuidSqlToClipboardToolStripMenuItem";
            this._copyGuidSqlToClipboardToolStripMenuItem.Size = new System.Drawing.Size(268, 22);
            this._copyGuidSqlToClipboardToolStripMenuItem.Text = "&Copy GUID + SQL to clipboard";
            this._copyGuidSqlToClipboardToolStripMenuItem.Click += new System.EventHandler(this.CopyGuidSqlToClipboardToolStripMenuItem_Click);
            // 
            // _createSharedMemoryToolStripMenuItem
            // 
            this._createSharedMemoryToolStripMenuItem.Name = "_createSharedMemoryToolStripMenuItem";
            this._createSharedMemoryToolStripMenuItem.Size = new System.Drawing.Size(268, 22);
            this._createSharedMemoryToolStripMenuItem.Text = "&Create Shared Memory for Package";
            this._createSharedMemoryToolStripMenuItem.ToolTipText = "Create shared memory from:\r\n#define SP_SHM_HW_MIC_START     0x40008000u\r\n#define " +
    "SP_SHM_HW_MIC_END       0x400083FFu\r\nas class+interface shared memory and:\r\nthe " +
    "associated Realisation dependency.\r\n";
            this._createSharedMemoryToolStripMenuItem.Click += new System.EventHandler(this.CreateSharedMemoryToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(265, 6);
            // 
            // standardDiagramToolStripMenuItem
            // 
            this.standardDiagramToolStripMenuItem.Name = "standardDiagramToolStripMenuItem";
            this.standardDiagramToolStripMenuItem.Size = new System.Drawing.Size(268, 22);
            this.standardDiagramToolStripMenuItem.Text = "StandardDiagram (recursive)";
            this.standardDiagramToolStripMenuItem.ToolTipText = "Sets the diagram standards for selected:\r\n- Diagram\r\n- Element, recursive\r\n- Pack" +
    "age, recursive,\r\n\r\nParameters:\r\n- Diagram fit to one page\r\n- No qualifiers\r\n- Ou" +
    "tput Parameters with name and parameter";
            this.standardDiagramToolStripMenuItem.Click += new System.EventHandler(this.StandardDiagramToolStripMenuItem_Click);
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(265, 6);
            // 
            // moveUsageToElementToolStripMenuItem
            // 
            this.moveUsageToElementToolStripMenuItem.Name = "moveUsageToElementToolStripMenuItem";
            this.moveUsageToElementToolStripMenuItem.Size = new System.Drawing.Size(268, 22);
            this.moveUsageToElementToolStripMenuItem.Text = "Move usage to element";
            this.moveUsageToElementToolStripMenuItem.ToolTipText = resources.GetString("moveUsageToElementToolStripMenuItem.ToolTipText");
            this.moveUsageToElementToolStripMenuItem.Click += new System.EventHandler(this.MoveUsageToElementToolStripMenuItem_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(265, 6);
            // 
            // sortAlphabeticToolStripMenuItem
            // 
            this.sortAlphabeticToolStripMenuItem.Name = "sortAlphabeticToolStripMenuItem";
            this.sortAlphabeticToolStripMenuItem.Size = new System.Drawing.Size(268, 22);
            this.sortAlphabeticToolStripMenuItem.Text = "Sort alphabetic";
            this.sortAlphabeticToolStripMenuItem.ToolTipText = "Sort the selected diagram elements in alphabetic order:\r\n- Ports, Pins, Parameter" +
    "s\r\n- Elements, Packages\r\n\r\nIt ignores:\r\n- ProvidedInterface\r\n- RequiredInterface" +
    "";
            this.sortAlphabeticToolStripMenuItem.Click += new System.EventHandler(this.SortAlphabeticToolStripMenuItem_Click);
            // 
            // toolStripSeparator11
            // 
            this.toolStripSeparator11.Name = "toolStripSeparator11";
            this.toolStripSeparator11.Size = new System.Drawing.Size(265, 6);
            // 
            // _codeToolStripMenuItem
            // 
            this._codeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._insertAttributeToolStripMenuItem,
            this._insertTypedefStructToolStripMenuItem,
            this.toolStripSeparator10,
            this._insertFunctionToolStripMenuItem,
            this.insertFunctionMakeDuplicatesToolStripMenuItem,
            this.toolStripSeparator3,
            this._updateActionToolStripMenuItem,
            this._toolStripSeparator6,
            this._deleteInvisibleuseRealizationDependenciesToolStripMenuItem,
            this.toolStripSeparator1,
            this._generateComponentPortsToolStripMenuItem,
            this._hideAllPortsToolStripMenuItem,
            this._showAllPortsActivityParametersToolStripMenuItem,
            this._toolStripSeparator7,
            this._inserToolStripMenuItem,
            this._toolStripSeparator8,
            this._setMacroToolStripMenuItem,
            this._addMacroToolStripMenuItem,
            this._delMacroToolStripMenuItem,
            this._toolStripSeparator,
            this._copyReleaseInformationToClipboardToolStripMenuItem,
            this.toolStripSeparator16});
            this._codeToolStripMenuItem.Name = "_codeToolStripMenuItem";
            this._codeToolStripMenuItem.Size = new System.Drawing.Size(47, 20);
            this._codeToolStripMenuItem.Text = "&Code";
            this._codeToolStripMenuItem.ToolTipText = "Update Action:\r\n- Type: CallOperation \r\n- Link: To operation";
            // 
            // _insertAttributeToolStripMenuItem
            // 
            this._insertAttributeToolStripMenuItem.Name = "_insertAttributeToolStripMenuItem";
            this._insertAttributeToolStripMenuItem.Size = new System.Drawing.Size(276, 22);
            this._insertAttributeToolStripMenuItem.Text = "&Insert / update  attributes";
            this._insertAttributeToolStripMenuItem.ToolTipText = resources.GetString("_insertAttributeToolStripMenuItem.ToolTipText");
            this._insertAttributeToolStripMenuItem.Click += new System.EventHandler(this.InsertAttributeToolStripMenuItem_Click);
            // 
            // _insertTypedefStructToolStripMenuItem
            // 
            this._insertTypedefStructToolStripMenuItem.Name = "_insertTypedefStructToolStripMenuItem";
            this._insertTypedefStructToolStripMenuItem.Size = new System.Drawing.Size(276, 22);
            this._insertTypedefStructToolStripMenuItem.Text = "Create typedef struct/union/enum";
            this._insertTypedefStructToolStripMenuItem.ToolTipText = resources.GetString("_insertTypedefStructToolStripMenuItem.ToolTipText");
            this._insertTypedefStructToolStripMenuItem.Click += new System.EventHandler(this.InsertTypedefStructToolStripMenuItem_Click);
            // 
            // toolStripSeparator10
            // 
            this.toolStripSeparator10.Name = "toolStripSeparator10";
            this.toolStripSeparator10.Size = new System.Drawing.Size(273, 6);
            // 
            // _insertFunctionToolStripMenuItem
            // 
            this._insertFunctionToolStripMenuItem.Name = "_insertFunctionToolStripMenuItem";
            this._insertFunctionToolStripMenuItem.Size = new System.Drawing.Size(276, 22);
            this._insertFunctionToolStripMenuItem.Text = "&Insert/ update function";
            this._insertFunctionToolStripMenuItem.ToolTipText = "Insert & updates C- functions from code\r\n\r\nIf the function exists hoReverse updat" +
    "es parameter and return value.";
            this._insertFunctionToolStripMenuItem.Click += new System.EventHandler(this.InsertFunctionToolStripMenuItem_Click);
            // 
            // insertFunctionMakeDuplicatesToolStripMenuItem
            // 
            this.insertFunctionMakeDuplicatesToolStripMenuItem.Name = "insertFunctionMakeDuplicatesToolStripMenuItem";
            this.insertFunctionMakeDuplicatesToolStripMenuItem.Size = new System.Drawing.Size(276, 22);
            this.insertFunctionMakeDuplicatesToolStripMenuItem.Text = "Insert function (make duplicates)";
            this.insertFunctionMakeDuplicatesToolStripMenuItem.ToolTipText = "Insert C- functions from code\r\n\r\nIf the function exists hoReverse inserts a new f" +
    "unction.";
            this.insertFunctionMakeDuplicatesToolStripMenuItem.Click += new System.EventHandler(this.InsertFunctionMakeDuplicatesToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(273, 6);
            // 
            // _updateActionToolStripMenuItem
            // 
            this._updateActionToolStripMenuItem.Name = "_updateActionToolStripMenuItem";
            this._updateActionToolStripMenuItem.Size = new System.Drawing.Size(276, 22);
            this._updateActionToolStripMenuItem.Text = "UpdateOperationInAction";
            this._updateActionToolStripMenuItem.ToolTipText = "Update Operation for Action. Select Action.\r\n\r\nIt tries no link to an operation w" +
    "ith no \'extern\' stereotype. \r\nIf this doesn\'t work it link to operation regardle" +
    "ss of stereotype.";
            this._updateActionToolStripMenuItem.Click += new System.EventHandler(this._updateActionToolStripMenuItem_Click);
            // 
            // _toolStripSeparator6
            // 
            this._toolStripSeparator6.Name = "_toolStripSeparator6";
            this._toolStripSeparator6.Size = new System.Drawing.Size(273, 6);
            // 
            // _deleteInvisibleuseRealizationDependenciesToolStripMenuItem
            // 
            this._deleteInvisibleuseRealizationDependenciesToolStripMenuItem.Name = "_deleteInvisibleuseRealizationDependenciesToolStripMenuItem";
            this._deleteInvisibleuseRealizationDependenciesToolStripMenuItem.Size = new System.Drawing.Size(276, 22);
            this._deleteInvisibleuseRealizationDependenciesToolStripMenuItem.Text = "Delete invisible <<use> dependencies";
            this._deleteInvisibleuseRealizationDependenciesToolStripMenuItem.ToolTipText = "Delete for selected Class / Interface <<use>> dependencies which are not linked t" +
    "o diagramobjects on the current diagram.";
            this._deleteInvisibleuseRealizationDependenciesToolStripMenuItem.Click += new System.EventHandler(this.DeleteInvisibleuseRealizationDependenciesToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(273, 6);
            // 
            // _generateComponentPortsToolStripMenuItem
            // 
            this._generateComponentPortsToolStripMenuItem.Name = "_generateComponentPortsToolStripMenuItem";
            this._generateComponentPortsToolStripMenuItem.Size = new System.Drawing.Size(276, 22);
            this._generateComponentPortsToolStripMenuItem.Text = "&GenerateComponentPorts";
            this._generateComponentPortsToolStripMenuItem.ToolTipText = resources.GetString("_generateComponentPortsToolStripMenuItem.ToolTipText");
            this._generateComponentPortsToolStripMenuItem.Click += new System.EventHandler(this.GenerateComponentPortsToolStripMenuItem_Click);
            // 
            // _hideAllPortsToolStripMenuItem
            // 
            this._hideAllPortsToolStripMenuItem.Name = "_hideAllPortsToolStripMenuItem";
            this._hideAllPortsToolStripMenuItem.Size = new System.Drawing.Size(276, 22);
            this._hideAllPortsToolStripMenuItem.Text = "Hide all ports";
            this._hideAllPortsToolStripMenuItem.ToolTipText = "Hide the ports of the selected element.";
            this._hideAllPortsToolStripMenuItem.Click += new System.EventHandler(this._hideAllPortsToolStripMenuItem_Click);
            // 
            // _showAllPortsActivityParametersToolStripMenuItem
            // 
            this._showAllPortsActivityParametersToolStripMenuItem.Name = "_showAllPortsActivityParametersToolStripMenuItem";
            this._showAllPortsActivityParametersToolStripMenuItem.Size = new System.Drawing.Size(276, 22);
            this._showAllPortsActivityParametersToolStripMenuItem.Text = "Show all ports / activity parameters";
            this._showAllPortsActivityParametersToolStripMenuItem.ToolTipText = "Show all ports/activity parameters  for classifier (Component, Activity)";
            this._showAllPortsActivityParametersToolStripMenuItem.Click += new System.EventHandler(this.ShowAllPortsActivityParametersToolStripMenuItem_Click);
            // 
            // _toolStripSeparator7
            // 
            this._toolStripSeparator7.Name = "_toolStripSeparator7";
            this._toolStripSeparator7.Size = new System.Drawing.Size(273, 6);
            // 
            // _inserToolStripMenuItem
            // 
            this._inserToolStripMenuItem.Name = "_inserToolStripMenuItem";
            this._inserToolStripMenuItem.Size = new System.Drawing.Size(276, 22);
            this._inserToolStripMenuItem.Text = "Generate Include for classifier";
            this._inserToolStripMenuItem.ToolTipText = resources.GetString("_inserToolStripMenuItem.ToolTipText");
            this._inserToolStripMenuItem.Click += new System.EventHandler(this._generateIncludeForClassifierToolStripMenuItem_Click);
            // 
            // _toolStripSeparator8
            // 
            this._toolStripSeparator8.Name = "_toolStripSeparator8";
            this._toolStripSeparator8.Size = new System.Drawing.Size(273, 6);
            // 
            // _setMacroToolStripMenuItem
            // 
            this._setMacroToolStripMenuItem.Name = "_setMacroToolStripMenuItem";
            this._setMacroToolStripMenuItem.Size = new System.Drawing.Size(276, 22);
            this._setMacroToolStripMenuItem.Text = "Set Macro";
            this._setMacroToolStripMenuItem.ToolTipText = resources.GetString("_setMacroToolStripMenuItem.ToolTipText");
            this._setMacroToolStripMenuItem.Click += new System.EventHandler(this.SetMacroToolStripMenuItem_Click);
            // 
            // _addMacroToolStripMenuItem
            // 
            this._addMacroToolStripMenuItem.Name = "_addMacroToolStripMenuItem";
            this._addMacroToolStripMenuItem.Size = new System.Drawing.Size(276, 22);
            this._addMacroToolStripMenuItem.Text = "Add Macro";
            this._addMacroToolStripMenuItem.ToolTipText = resources.GetString("_addMacroToolStripMenuItem.ToolTipText");
            this._addMacroToolStripMenuItem.Click += new System.EventHandler(this.AddMacroToolStripMenuItem_Click);
            // 
            // _delMacroToolStripMenuItem
            // 
            this._delMacroToolStripMenuItem.Name = "_delMacroToolStripMenuItem";
            this._delMacroToolStripMenuItem.Size = new System.Drawing.Size(276, 22);
            this._delMacroToolStripMenuItem.Text = "Del Macro";
            this._delMacroToolStripMenuItem.ToolTipText = "Delete all macros/stereotypes for the selected item.";
            this._delMacroToolStripMenuItem.Click += new System.EventHandler(this.DelMacroToolStripMenuItem_Click);
            // 
            // _toolStripSeparator
            // 
            this._toolStripSeparator.Name = "_toolStripSeparator";
            this._toolStripSeparator.Size = new System.Drawing.Size(273, 6);
            // 
            // _copyReleaseInformationToClipboardToolStripMenuItem
            // 
            this._copyReleaseInformationToClipboardToolStripMenuItem.Name = "_copyReleaseInformationToClipboardToolStripMenuItem";
            this._copyReleaseInformationToClipboardToolStripMenuItem.Size = new System.Drawing.Size(276, 22);
            this._copyReleaseInformationToClipboardToolStripMenuItem.Text = "&Copy release information to Clipboard";
            this._copyReleaseInformationToClipboardToolStripMenuItem.ToolTipText = "Copy release information of *.c and *.h files to Clipboard:\r\n- Select Module\r\n- F" +
    "or all elements on the diagram the release information is printed";
            this._copyReleaseInformationToClipboardToolStripMenuItem.Click += new System.EventHandler(this.CopyReleaseInformationToClipboardToolStripMenuItem_Click);
            // 
            // toolStripSeparator16
            // 
            this.toolStripSeparator16.Name = "toolStripSeparator16";
            this.toolStripSeparator16.Size = new System.Drawing.Size(273, 6);
            // 
            // _autoToolStripMenuItem
            // 
            this._autoToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.modulesToolStripMenuItem,
            this.inventoryToolStripMenuItem,
            this._getToolStripMenuItem,
            this.makeRunnableToolStripMenuItem,
            this.makeServicePortToolStripMenuItem,
            this.makeCalloutToolStripMenuItem,
            this.toolStripSeparator12,
            this.showExternalComponentFunctionsToolStripMenuItem,
            this.showProvidedRequiredFunctionsForSourceToolStripMenuItem,
            this.showFunctionsToolStripMenuItem,
            this.toolStripSeparator13,
            this.showSymbolDataBaseFoldersToolStripMenuItem});
            this._autoToolStripMenuItem.Name = "_autoToolStripMenuItem";
            this._autoToolStripMenuItem.Size = new System.Drawing.Size(47, 20);
            this._autoToolStripMenuItem.Text = "C-DB";
            this._autoToolStripMenuItem.ToolTipText = "Tools to use the C/C++ Database supported by Microsoft VS Code.\r\n- Provided and r" +
    "equired Interfaces\r\n- Functions and Macros";
            // 
            // modulesToolStripMenuItem
            // 
            this.modulesToolStripMenuItem.Name = "modulesToolStripMenuItem";
            this.modulesToolStripMenuItem.Size = new System.Drawing.Size(349, 22);
            this.modulesToolStripMenuItem.Text = "Generate";
            this.modulesToolStripMenuItem.Visible = false;
            this.modulesToolStripMenuItem.Click += new System.EventHandler(this.GenerateModulesToolStripMenuItem_Click);
            // 
            // inventoryToolStripMenuItem
            // 
            this.inventoryToolStripMenuItem.Name = "inventoryToolStripMenuItem";
            this.inventoryToolStripMenuItem.Size = new System.Drawing.Size(349, 22);
            this.inventoryToolStripMenuItem.Text = "Inventory";
            this.inventoryToolStripMenuItem.Visible = false;
            this.inventoryToolStripMenuItem.Click += new System.EventHandler(this.InventoryToolStripMenuItem_Click);
            // 
            // _getToolStripMenuItem
            // 
            this._getToolStripMenuItem.Name = "_getToolStripMenuItem";
            this._getToolStripMenuItem.Size = new System.Drawing.Size(349, 22);
            this._getToolStripMenuItem.Text = "GetExternalFunctions";
            this._getToolStripMenuItem.Visible = false;
            this._getToolStripMenuItem.Click += new System.EventHandler(this.GetToolStripMenuItem_Click);
            // 
            // makeRunnableToolStripMenuItem
            // 
            this.makeRunnableToolStripMenuItem.Name = "makeRunnableToolStripMenuItem";
            this.makeRunnableToolStripMenuItem.Size = new System.Drawing.Size(349, 22);
            this.makeRunnableToolStripMenuItem.Text = "MakeRunnablePort";
            this.makeRunnableToolStripMenuItem.ToolTipText = "Makes an Service Autosar Port\r\n\r\nSelect one or more ports.";
            this.makeRunnableToolStripMenuItem.Visible = false;
            this.makeRunnableToolStripMenuItem.Click += new System.EventHandler(this.MakeRunnableToolStripMenuItem_Click);
            // 
            // makeServicePortToolStripMenuItem
            // 
            this.makeServicePortToolStripMenuItem.Name = "makeServicePortToolStripMenuItem";
            this.makeServicePortToolStripMenuItem.Size = new System.Drawing.Size(349, 22);
            this.makeServicePortToolStripMenuItem.Text = "MakeServicePort";
            this.makeServicePortToolStripMenuItem.Visible = false;
            this.makeServicePortToolStripMenuItem.Click += new System.EventHandler(this.MakeServicePortToolStripMenuItem_Click);
            // 
            // makeCalloutToolStripMenuItem
            // 
            this.makeCalloutToolStripMenuItem.Name = "makeCalloutToolStripMenuItem";
            this.makeCalloutToolStripMenuItem.Size = new System.Drawing.Size(349, 22);
            this.makeCalloutToolStripMenuItem.Text = "MakeCalloutPort";
            this.makeCalloutToolStripMenuItem.Visible = false;
            this.makeCalloutToolStripMenuItem.Click += new System.EventHandler(this.MakeCalloutToolStripMenuItem_Click);
            // 
            // toolStripSeparator12
            // 
            this.toolStripSeparator12.Name = "toolStripSeparator12";
            this.toolStripSeparator12.Size = new System.Drawing.Size(346, 6);
            // 
            // showExternalComponentFunctionsToolStripMenuItem
            // 
            this.showExternalComponentFunctionsToolStripMenuItem.Name = "showExternalComponentFunctionsToolStripMenuItem";
            this.showExternalComponentFunctionsToolStripMenuItem.Size = new System.Drawing.Size(349, 22);
            this.showExternalComponentFunctionsToolStripMenuItem.Text = "Show Provided / Required Functions for EA-Element";
            this.showExternalComponentFunctionsToolStripMenuItem.ToolTipText = resources.GetString("showExternalComponentFunctionsToolStripMenuItem.ToolTipText");
            this.showExternalComponentFunctionsToolStripMenuItem.Click += new System.EventHandler(this.ShowExternalComponentFunctionsToolStripMenuItem_Click);
            // 
            // showProvidedRequiredFunctionsForSourceToolStripMenuItem
            // 
            this.showProvidedRequiredFunctionsForSourceToolStripMenuItem.Name = "showProvidedRequiredFunctionsForSourceToolStripMenuItem";
            this.showProvidedRequiredFunctionsForSourceToolStripMenuItem.Size = new System.Drawing.Size(349, 22);
            this.showProvidedRequiredFunctionsForSourceToolStripMenuItem.Text = "Show Provided / Required Functions for File/Folder";
            this.showProvidedRequiredFunctionsForSourceToolStripMenuItem.ToolTipText = resources.GetString("showProvidedRequiredFunctionsForSourceToolStripMenuItem.ToolTipText");
            this.showProvidedRequiredFunctionsForSourceToolStripMenuItem.Click += new System.EventHandler(this.ShowProvidedRequiredFunctionsForSourceToolStripMenuItem_Click);
            // 
            // showFunctionsToolStripMenuItem
            // 
            this.showFunctionsToolStripMenuItem.Name = "showFunctionsToolStripMenuItem";
            this.showFunctionsToolStripMenuItem.Size = new System.Drawing.Size(349, 22);
            this.showFunctionsToolStripMenuItem.Text = "Show all Functions";
            this.showFunctionsToolStripMenuItem.ToolTipText = "Shows all functions and macros\r\n\r\nIt requires:\r\n- VC Code symbol database\r\n- C/C+" +
    "+ Code with up to date VC Code symbol database";
            this.showFunctionsToolStripMenuItem.Click += new System.EventHandler(this.ShowFunctionsToolStripMenuItem_Click);
            // 
            // toolStripSeparator13
            // 
            this.toolStripSeparator13.Name = "toolStripSeparator13";
            this.toolStripSeparator13.Size = new System.Drawing.Size(346, 6);
            // 
            // showSymbolDataBaseFoldersToolStripMenuItem
            // 
            this.showSymbolDataBaseFoldersToolStripMenuItem.Name = "showSymbolDataBaseFoldersToolStripMenuItem";
            this.showSymbolDataBaseFoldersToolStripMenuItem.Size = new System.Drawing.Size(349, 22);
            this.showSymbolDataBaseFoldersToolStripMenuItem.Text = "Show Symbol VC-Code DataBase Folders";
            this.showSymbolDataBaseFoldersToolStripMenuItem.ToolTipText = "Show the folder with the VC-Cide Symbol database.\r\n\r\nIn case of unknown issues de" +
    "lete the whole folder. VS-Code will recreate it!";
            this.showSymbolDataBaseFoldersToolStripMenuItem.Click += new System.EventHandler(this.ShowSymbolDataBaseFoldersToolStripMenuItem_Click);
            // 
            // _versionControlToolStripMenuItem
            // 
            this._versionControlToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._svnLogToolStripMenuItem,
            this._svnTortoiseRepobrowserToolStripMenuItem,
            this._showDirectoryToolStripMenuItem,
            this._toolStripSeparator1,
            this._getVcLatestrecursiveToolStripMenuItem,
            this._setSvnKeywordsToolStripMenuItem,
            this._setSvnTaggedValuesToolStripMenuItem1,
            this._setSvnTaggedValuesToolStripMenuItem,
            this._toolStripSeparator2,
            this._changeXmlPathToolStripMenuItem1,
            this._toolStripSeparator5});
            this._versionControlToolStripMenuItem.Name = "_versionControlToolStripMenuItem";
            this._versionControlToolStripMenuItem.Size = new System.Drawing.Size(97, 20);
            this._versionControlToolStripMenuItem.Text = "&VersionControl";
            this._versionControlToolStripMenuItem.ToolTipText = "Sets the svn keywords:\r\n- svnDoc, svnRevision\r\nfor a package.";
            // 
            // _svnLogToolStripMenuItem
            // 
            this._svnLogToolStripMenuItem.Name = "_svnLogToolStripMenuItem";
            this._svnLogToolStripMenuItem.Size = new System.Drawing.Size(291, 22);
            this._svnLogToolStripMenuItem.Text = "&Show Tortoise Log";
            this._svnLogToolStripMenuItem.ToolTipText = "Opend the Tortoise Log window";
            this._svnLogToolStripMenuItem.Click += new System.EventHandler(this.SvnLogToolStripMenuItem_Click);
            // 
            // _svnTortoiseRepobrowserToolStripMenuItem
            // 
            this._svnTortoiseRepobrowserToolStripMenuItem.Name = "_svnTortoiseRepobrowserToolStripMenuItem";
            this._svnTortoiseRepobrowserToolStripMenuItem.Size = new System.Drawing.Size(291, 22);
            this._svnTortoiseRepobrowserToolStripMenuItem.Text = "&Show Tortoise Repo Browser";
            this._svnTortoiseRepobrowserToolStripMenuItem.ToolTipText = "Opens the Tortoise Repo Browser for the selected package";
            this._svnTortoiseRepobrowserToolStripMenuItem.Click += new System.EventHandler(this.SvnTortoiseRepobrowserToolStripMenuItem_Click);
            // 
            // _showDirectoryToolStripMenuItem
            // 
            this._showDirectoryToolStripMenuItem.Name = "_showDirectoryToolStripMenuItem";
            this._showDirectoryToolStripMenuItem.Size = new System.Drawing.Size(291, 22);
            this._showDirectoryToolStripMenuItem.Text = "&Show Directory (VC or Code)";
            this._showDirectoryToolStripMenuItem.ToolTipText = "Show Version Control directory of *.xml file or oh code";
            this._showDirectoryToolStripMenuItem.Click += new System.EventHandler(this.ShowDirectoryToolStripMenuItem_Click);
            // 
            // _toolStripSeparator1
            // 
            this._toolStripSeparator1.Name = "_toolStripSeparator1";
            this._toolStripSeparator1.Size = new System.Drawing.Size(288, 6);
            // 
            // _getVcLatestrecursiveToolStripMenuItem
            // 
            this._getVcLatestrecursiveToolStripMenuItem.Name = "_getVcLatestrecursiveToolStripMenuItem";
            this._getVcLatestrecursiveToolStripMenuItem.Size = new System.Drawing.Size(291, 22);
            this._getVcLatestrecursiveToolStripMenuItem.Text = "&GetVCLatest (recursive)";
            this._getVcLatestrecursiveToolStripMenuItem.ToolTipText = "GetAllLatest for package (recursive)";
            this._getVcLatestrecursiveToolStripMenuItem.Click += new System.EventHandler(this.GetVcLastestRecursiveToolStripMenuItem_Click);
            // 
            // _setSvnKeywordsToolStripMenuItem
            // 
            this._setSvnKeywordsToolStripMenuItem.Name = "_setSvnKeywordsToolStripMenuItem";
            this._setSvnKeywordsToolStripMenuItem.Size = new System.Drawing.Size(291, 22);
            this._setSvnKeywordsToolStripMenuItem.Text = "&Set svn keywords";
            this._setSvnKeywordsToolStripMenuItem.ToolTipText = "Set the svn Module Package keywords for a VC package";
            this._setSvnKeywordsToolStripMenuItem.Click += new System.EventHandler(this.SetSvnKeywordsToolStripMenuItem_Click);
            // 
            // _setSvnTaggedValuesToolStripMenuItem1
            // 
            this._setSvnTaggedValuesToolStripMenuItem1.Name = "_setSvnTaggedValuesToolStripMenuItem1";
            this._setSvnTaggedValuesToolStripMenuItem1.Size = new System.Drawing.Size(291, 22);
            this._setSvnTaggedValuesToolStripMenuItem1.Text = "&Set svn Module Tagged Values";
            this._setSvnTaggedValuesToolStripMenuItem1.ToolTipText = "Set module package tagged values of a module package.\r\n\r\nModule Package\r\n   Archi" +
    "ctecture\r\n      Structure\r\n         Module\r\n\r\nTags:\r\nsvnDate\r\nsvnRevision";
            this._setSvnTaggedValuesToolStripMenuItem1.Click += new System.EventHandler(this.SetSvnTaggedValuesToolStripMenuItem1_Click);
            // 
            // _setSvnTaggedValuesToolStripMenuItem
            // 
            this._setSvnTaggedValuesToolStripMenuItem.Name = "_setSvnTaggedValuesToolStripMenuItem";
            this._setSvnTaggedValuesToolStripMenuItem.Size = new System.Drawing.Size(291, 22);
            this._setSvnTaggedValuesToolStripMenuItem.Text = "&Set svn Module Tagged Values (recursive)";
            this._setSvnTaggedValuesToolStripMenuItem.ToolTipText = "Sets svn module package Tagged Values for a Version controlled Package (recursive" +
    ") which is a module package. \r\n\r\nA module package contains subpackages Architect" +
    "ure\\Structure\\Module and Behavior\r\n";
            this._setSvnTaggedValuesToolStripMenuItem.Click += new System.EventHandler(this.SetSvnTaggedValuesToolStripMenuItem_Click);
            // 
            // _toolStripSeparator2
            // 
            this._toolStripSeparator2.Name = "_toolStripSeparator2";
            this._toolStripSeparator2.Size = new System.Drawing.Size(288, 6);
            // 
            // _changeXmlPathToolStripMenuItem1
            // 
            this._changeXmlPathToolStripMenuItem1.Name = "_changeXmlPathToolStripMenuItem1";
            this._changeXmlPathToolStripMenuItem1.Size = new System.Drawing.Size(291, 22);
            this._changeXmlPathToolStripMenuItem1.Text = "&ChangeXmlPath";
            this._changeXmlPathToolStripMenuItem1.ToolTipText = resources.GetString("_changeXmlPathToolStripMenuItem1.ToolTipText");
            this._changeXmlPathToolStripMenuItem1.Click += new System.EventHandler(this.ChangeXmlPathToolStripMenuItem_Click);
            // 
            // _toolStripSeparator5
            // 
            this._toolStripSeparator5.Name = "_toolStripSeparator5";
            this._toolStripSeparator5.Size = new System.Drawing.Size(288, 6);
            // 
            // _maintenanceToolStripMenuItem
            // 
            this._maintenanceToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._vCGetStateToolStripMenuItem,
            this._vCResyncToolStripMenuItem,
            this._vCxmiReconsileToolStripMenuItem,
            this._vCRemoveToolStripMenuItem,
            this.toolStripSeparator17,
            this.doorsImportcsvToolStripMenuItem,
            this.doorsImportcsvWithFileDialogToolStripMenuItem,
            this.checkDOORSRequirementsToolStripMenuItem,
            this.toolStripSeparator18,
            this.importBySettingsToolStripMenuItem,
            this.importDoorsReqIFBySettingsToolStripMenuItem,
            this.importReqIFBySettings3ToolStripMenuItem,
            this.importReqIFBySettingsToolStripMenuItem,
            this.importReqIFBySettings5ToolStripMenuItem});
            this._maintenanceToolStripMenuItem.Name = "_maintenanceToolStripMenuItem";
            this._maintenanceToolStripMenuItem.Size = new System.Drawing.Size(88, 20);
            this._maintenanceToolStripMenuItem.Text = "Maintenance";
            this._maintenanceToolStripMenuItem.ToolTipText = "Experimental";
            // 
            // _vCGetStateToolStripMenuItem
            // 
            this._vCGetStateToolStripMenuItem.Name = "_vCGetStateToolStripMenuItem";
            this._vCGetStateToolStripMenuItem.Size = new System.Drawing.Size(254, 22);
            this._vCGetStateToolStripMenuItem.Text = "&VC get state";
            this._vCGetStateToolStripMenuItem.ToolTipText = "Show the VC package state in a messagage box.\r\n- How has checked out the package";
            this._vCGetStateToolStripMenuItem.Click += new System.EventHandler(this.VCGetStateToolStripMenuItem_Click);
            // 
            // _vCResyncToolStripMenuItem
            // 
            this._vCResyncToolStripMenuItem.Name = "_vCResyncToolStripMenuItem";
            this._vCResyncToolStripMenuItem.Size = new System.Drawing.Size(254, 22);
            this._vCResyncToolStripMenuItem.Text = "&VC Resync";
            this._vCResyncToolStripMenuItem.ToolTipText = "Resynchronice svn VC package state for package(recursive).\r\n- Select Package\r\n- S" +
    "elect Model for whole Model (root package)";
            this._vCResyncToolStripMenuItem.Click += new System.EventHandler(this.VCResyncToolStripMenuItem_Click);
            // 
            // _vCxmiReconsileToolStripMenuItem
            // 
            this._vCxmiReconsileToolStripMenuItem.Name = "_vCxmiReconsileToolStripMenuItem";
            this._vCxmiReconsileToolStripMenuItem.Size = new System.Drawing.Size(254, 22);
            this._vCxmiReconsileToolStripMenuItem.Text = "VC XMI reconsile";
            this._vCxmiReconsileToolStripMenuItem.ToolTipText = "Scan all XMI packages and reconsile deleted objects or connectors.";
            this._vCxmiReconsileToolStripMenuItem.Click += new System.EventHandler(this.VCXMIReconsileToolStripMenuItem_Click);
            // 
            // _vCRemoveToolStripMenuItem
            // 
            this._vCRemoveToolStripMenuItem.Name = "_vCRemoveToolStripMenuItem";
            this._vCRemoveToolStripMenuItem.Size = new System.Drawing.Size(254, 22);
            this._vCRemoveToolStripMenuItem.Text = "VC Remove";
            this._vCRemoveToolStripMenuItem.Click += new System.EventHandler(this._vCRemoveToolStripMenuItem_Click);
            // 
            // toolStripSeparator17
            // 
            this.toolStripSeparator17.Name = "toolStripSeparator17";
            this.toolStripSeparator17.Size = new System.Drawing.Size(251, 6);
            // 
            // doorsImportcsvToolStripMenuItem
            // 
            this.doorsImportcsvToolStripMenuItem.Name = "doorsImportcsvToolStripMenuItem";
            this.doorsImportcsvToolStripMenuItem.Size = new System.Drawing.Size(254, 22);
            this.doorsImportcsvToolStripMenuItem.Text = "Doors Import *.csv";
            this.doorsImportcsvToolStripMenuItem.ToolTipText = resources.GetString("doorsImportcsvToolStripMenuItem.ToolTipText");
            this.doorsImportcsvToolStripMenuItem.Click += new System.EventHandler(this.DoorsImportcsvToolStripMenuItem_Click);
            // 
            // doorsImportcsvWithFileDialogToolStripMenuItem
            // 
            this.doorsImportcsvWithFileDialogToolStripMenuItem.Name = "doorsImportcsvWithFileDialogToolStripMenuItem";
            this.doorsImportcsvWithFileDialogToolStripMenuItem.Size = new System.Drawing.Size(254, 22);
            this.doorsImportcsvWithFileDialogToolStripMenuItem.Text = "Doors Import *.csv with file Dialog";
            this.doorsImportcsvWithFileDialogToolStripMenuItem.ToolTipText = resources.GetString("doorsImportcsvWithFileDialogToolStripMenuItem.ToolTipText");
            this.doorsImportcsvWithFileDialogToolStripMenuItem.Click += new System.EventHandler(this.DoorsImportcsvWithFileDialogToolStripMenuItem_Click);
            // 
            // checkDOORSRequirementsToolStripMenuItem
            // 
            this.checkDOORSRequirementsToolStripMenuItem.Name = "checkDOORSRequirementsToolStripMenuItem";
            this.checkDOORSRequirementsToolStripMenuItem.Size = new System.Drawing.Size(254, 22);
            this.checkDOORSRequirementsToolStripMenuItem.Text = "Check DOORS Requirements";
            this.checkDOORSRequirementsToolStripMenuItem.ToolTipText = "Select a package with imported DOORS requirements and run the check.\r\n\r\nIt shows:" +
    "\r\n- All not unique DOORS Requirements";
            this.checkDOORSRequirementsToolStripMenuItem.Click += new System.EventHandler(this.CheckDOORSRequirementsToolStripMenuItem_Click);
            // 
            // toolStripSeparator18
            // 
            this.toolStripSeparator18.Name = "toolStripSeparator18";
            this.toolStripSeparator18.Size = new System.Drawing.Size(251, 6);
            // 
            // importBySettingsToolStripMenuItem
            // 
            this.importBySettingsToolStripMenuItem.Name = "importBySettingsToolStripMenuItem";
            this.importBySettingsToolStripMenuItem.Size = new System.Drawing.Size(254, 22);
            this.importBySettingsToolStripMenuItem.Text = "ImportBySettings";
            this.importBySettingsToolStripMenuItem.ToolTipText = "Import specified by Settings.json, Chapter \'Import\'\r\n\r\nCurrently supported:\r\n- DO" +
    "ORS *.csv format\r\n\r\nThe function works in background and you can proceed writing" +
    ".";
            this.importBySettingsToolStripMenuItem.Click += new System.EventHandler(this.ImportBySettingsToolStripMenuItem_Click);
            // 
            // importDoorsReqIFBySettingsToolStripMenuItem
            // 
            this.importDoorsReqIFBySettingsToolStripMenuItem.Name = "importDoorsReqIFBySettingsToolStripMenuItem";
            this.importDoorsReqIFBySettingsToolStripMenuItem.Size = new System.Drawing.Size(254, 22);
            this.importDoorsReqIFBySettingsToolStripMenuItem.Text = "ImportDoorsReqIFBySettings";
            this.importDoorsReqIFBySettingsToolStripMenuItem.Click += new System.EventHandler(this.ImportDoorsReqIFBySettingsToolStripMenuItem_Click);
            // 
            // importReqIFBySettings3ToolStripMenuItem
            // 
            this.importReqIFBySettings3ToolStripMenuItem.Name = "importReqIFBySettings3ToolStripMenuItem";
            this.importReqIFBySettings3ToolStripMenuItem.Size = new System.Drawing.Size(254, 22);
            this.importReqIFBySettings3ToolStripMenuItem.Text = "ImportReqIFBySettings 3";
            this.importReqIFBySettings3ToolStripMenuItem.Click += new System.EventHandler(this.ImportReqIFBySettings3ToolStripMenuItem_Click_1);
            // 
            // importReqIFBySettingsToolStripMenuItem
            // 
            this.importReqIFBySettingsToolStripMenuItem.Name = "importReqIFBySettingsToolStripMenuItem";
            this.importReqIFBySettingsToolStripMenuItem.Size = new System.Drawing.Size(254, 22);
            this.importReqIFBySettingsToolStripMenuItem.Text = "ImportReqIFBySettings 4";
            this.importReqIFBySettingsToolStripMenuItem.Click += new System.EventHandler(this.ImportReqIFBySettings4ToolStripMenuItem_Click);
            // 
            // importReqIFBySettings5ToolStripMenuItem
            // 
            this.importReqIFBySettings5ToolStripMenuItem.Name = "importReqIFBySettings5ToolStripMenuItem";
            this.importReqIFBySettings5ToolStripMenuItem.Size = new System.Drawing.Size(254, 22);
            this.importReqIFBySettings5ToolStripMenuItem.Text = "ImportReqIFBySettings 5";
            this.importReqIFBySettings5ToolStripMenuItem.Click += new System.EventHandler(this.ImportReqIFBySettings5ToolStripMenuItem_Click);
            // 
            // _helpToolStripMenuItem
            // 
            this._helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._aboutToolStripMenuItem,
            this.toolStripSeparator15,
            this._helpF1ToolStripMenuItem,
            this.readmeToolStripMenuItem,
            this.repoToolStripMenuItem,
            this.hoToolsToolStripMenuItem,
            this.lineStyleToolStripMenuItem,
            this.toolStripSeparator14,
            this.analyzeCCToolStripMenuItem});
            this._helpToolStripMenuItem.Name = "_helpToolStripMenuItem";
            this._helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this._helpToolStripMenuItem.Text = "&Help";
            // 
            // _aboutToolStripMenuItem
            // 
            this._aboutToolStripMenuItem.Name = "_aboutToolStripMenuItem";
            this._aboutToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this._aboutToolStripMenuItem.Text = "&About";
            this._aboutToolStripMenuItem.Click += new System.EventHandler(this.AboutToolStripMenuItem_Click);
            // 
            // toolStripSeparator15
            // 
            this.toolStripSeparator15.Name = "toolStripSeparator15";
            this.toolStripSeparator15.Size = new System.Drawing.Size(149, 6);
            // 
            // _helpF1ToolStripMenuItem
            // 
            this._helpF1ToolStripMenuItem.Name = "_helpF1ToolStripMenuItem";
            this._helpF1ToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this._helpF1ToolStripMenuItem.Text = "&Help / WiKi";
            this._helpF1ToolStripMenuItem.ToolTipText = "Show help / WiKi";
            this._helpF1ToolStripMenuItem.Click += new System.EventHandler(this.HelpF1ToolStripMenuItem_Click);
            // 
            // readmeToolStripMenuItem
            // 
            this.readmeToolStripMenuItem.Name = "readmeToolStripMenuItem";
            this.readmeToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.readmeToolStripMenuItem.Text = "Readme";
            this.readmeToolStripMenuItem.ToolTipText = "Show readme";
            this.readmeToolStripMenuItem.Click += new System.EventHandler(this.ReadmeToolStripMenuItem_Click);
            // 
            // repoToolStripMenuItem
            // 
            this.repoToolStripMenuItem.Name = "repoToolStripMenuItem";
            this.repoToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.repoToolStripMenuItem.Text = "Repo";
            this.repoToolStripMenuItem.ToolTipText = "Show GitHub repository";
            this.repoToolStripMenuItem.Click += new System.EventHandler(this.RepoToolStripMenuItem_Click);
            // 
            // hoToolsToolStripMenuItem
            // 
            this.hoToolsToolStripMenuItem.Name = "hoToolsToolStripMenuItem";
            this.hoToolsToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.hoToolsToolStripMenuItem.Text = "hoTools";
            this.hoToolsToolStripMenuItem.ToolTipText = "Show WiKi hoTools";
            // 
            // lineStyleToolStripMenuItem
            // 
            this.lineStyleToolStripMenuItem.Name = "lineStyleToolStripMenuItem";
            this.lineStyleToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.lineStyleToolStripMenuItem.Text = "LineStyle";
            this.lineStyleToolStripMenuItem.ToolTipText = "Show WiKi LineStyle";
            this.lineStyleToolStripMenuItem.Click += new System.EventHandler(this.LineStyleToolStripMenuItem_Click);
            // 
            // toolStripSeparator14
            // 
            this.toolStripSeparator14.Name = "toolStripSeparator14";
            this.toolStripSeparator14.Size = new System.Drawing.Size(149, 6);
            // 
            // analyzeCCToolStripMenuItem
            // 
            this.analyzeCCToolStripMenuItem.Name = "analyzeCCToolStripMenuItem";
            this.analyzeCCToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.analyzeCCToolStripMenuItem.Text = "AnalyzeC/C++";
            this.analyzeCCToolStripMenuItem.Click += new System.EventHandler(this.AnalyzeCCToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(24, 20);
            this.helpToolStripMenuItem.Text = "?";
            this.helpToolStripMenuItem.ToolTipText = "WiKi";
            this.helpToolStripMenuItem.Click += new System.EventHandler(this.HelpToolStripMenuItem_Click);
            // 
            // _toolStripContainer1
            // 
            this._toolStripContainer1.BottomToolStripPanelVisible = false;
            // 
            // _toolStripContainer1.ContentPanel
            // 
            this._toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(764, 0);
            this._toolStripContainer1.LeftToolStripPanelVisible = false;
            this._toolStripContainer1.Location = new System.Drawing.Point(3, 29);
            this._toolStripContainer1.Name = "_toolStripContainer1";
            this._toolStripContainer1.RightToolStripPanelVisible = false;
            this._toolStripContainer1.Size = new System.Drawing.Size(764, 25);
            this._toolStripContainer1.TabIndex = 52;
            this._toolStripContainer1.Text = "toolStripContainer1";
            // 
            // _toolStripContainer1.TopToolStripPanel
            // 
            this._toolStripContainer1.TopToolStripPanel.Controls.Add(this._toolStrip6);
            // 
            // _toolStrip6
            // 
            this._toolStrip6.Dock = System.Windows.Forms.DockStyle.None;
            this._toolStrip6.ImageScalingSize = new System.Drawing.Size(20, 20);
            this._toolStrip6.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._toolStripBtn11,
            this._toolStripBtn12,
            this._toolStripBtn13,
            this._toolStripBtn14,
            this._toolStripBtn15});
            this._toolStrip6.Location = new System.Drawing.Point(3, 0);
            this._toolStrip6.Name = "_toolStrip6";
            this._toolStrip6.Size = new System.Drawing.Size(127, 25);
            this._toolStrip6.TabIndex = 4;
            // 
            // _toolStripBtn11
            // 
            this._toolStripBtn11.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._toolStripBtn11.Image = ((System.Drawing.Image)(resources.GetObject("_toolStripBtn11.Image")));
            this._toolStripBtn11.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._toolStripBtn11.Name = "_toolStripBtn11";
            this._toolStripBtn11.Size = new System.Drawing.Size(23, 22);
            this._toolStripBtn11.Text = "1";
            this._toolStripBtn11.Click += new System.EventHandler(this.ToolStripBtn11_Click);
            // 
            // _toolStripBtn12
            // 
            this._toolStripBtn12.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._toolStripBtn12.Image = ((System.Drawing.Image)(resources.GetObject("_toolStripBtn12.Image")));
            this._toolStripBtn12.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._toolStripBtn12.Name = "_toolStripBtn12";
            this._toolStripBtn12.Size = new System.Drawing.Size(23, 22);
            this._toolStripBtn12.Text = "2";
            this._toolStripBtn12.Click += new System.EventHandler(this.ToolStripBtn12_Click);
            // 
            // _toolStripBtn13
            // 
            this._toolStripBtn13.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._toolStripBtn13.Image = ((System.Drawing.Image)(resources.GetObject("_toolStripBtn13.Image")));
            this._toolStripBtn13.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._toolStripBtn13.Name = "_toolStripBtn13";
            this._toolStripBtn13.Size = new System.Drawing.Size(23, 22);
            this._toolStripBtn13.Text = "3";
            this._toolStripBtn13.Click += new System.EventHandler(this.ToolStripBtn13_Click);
            // 
            // _toolStripBtn14
            // 
            this._toolStripBtn14.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._toolStripBtn14.Image = ((System.Drawing.Image)(resources.GetObject("_toolStripBtn14.Image")));
            this._toolStripBtn14.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._toolStripBtn14.Name = "_toolStripBtn14";
            this._toolStripBtn14.Size = new System.Drawing.Size(23, 22);
            this._toolStripBtn14.Text = "4";
            this._toolStripBtn14.Click += new System.EventHandler(this.ToolStripBtn14_Click);
            // 
            // _toolStripBtn15
            // 
            this._toolStripBtn15.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._toolStripBtn15.Image = ((System.Drawing.Image)(resources.GetObject("_toolStripBtn15.Image")));
            this._toolStripBtn15.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._toolStripBtn15.Name = "_toolStripBtn15";
            this._toolStripBtn15.Size = new System.Drawing.Size(23, 22);
            this._toolStripBtn15.Text = "5";
            this._toolStripBtn15.Click += new System.EventHandler(this.ToolStripBtn15_Click);
            // 
            // _toolStrip1
            // 
            this._toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this._toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this._toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._toolStripBtn1,
            this._toolStripBtn2,
            this._toolStripBtn3,
            this._toolStripBtn4,
            this._toolStripBtn5});
            this._toolStrip1.Location = new System.Drawing.Point(150, 29);
            this._toolStrip1.Name = "_toolStrip1";
            this._toolStrip1.Size = new System.Drawing.Size(127, 25);
            this._toolStrip1.TabIndex = 0;
            this._toolStrip1.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.ToolStrip1_ItemClicked);
            // 
            // _toolStripBtn1
            // 
            this._toolStripBtn1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._toolStripBtn1.Image = ((System.Drawing.Image)(resources.GetObject("_toolStripBtn1.Image")));
            this._toolStripBtn1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._toolStripBtn1.Name = "_toolStripBtn1";
            this._toolStripBtn1.Size = new System.Drawing.Size(23, 22);
            this._toolStripBtn1.Text = "1";
            this._toolStripBtn1.Click += new System.EventHandler(this.ToolStripBtn1_Click);
            // 
            // _toolStripBtn2
            // 
            this._toolStripBtn2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._toolStripBtn2.Image = ((System.Drawing.Image)(resources.GetObject("_toolStripBtn2.Image")));
            this._toolStripBtn2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._toolStripBtn2.Name = "_toolStripBtn2";
            this._toolStripBtn2.Size = new System.Drawing.Size(23, 22);
            this._toolStripBtn2.Text = "2";
            this._toolStripBtn2.Click += new System.EventHandler(this.ToolStripBtn2_Click);
            // 
            // _toolStripBtn3
            // 
            this._toolStripBtn3.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._toolStripBtn3.Image = ((System.Drawing.Image)(resources.GetObject("_toolStripBtn3.Image")));
            this._toolStripBtn3.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._toolStripBtn3.Name = "_toolStripBtn3";
            this._toolStripBtn3.Size = new System.Drawing.Size(23, 22);
            this._toolStripBtn3.Text = "3";
            this._toolStripBtn3.Click += new System.EventHandler(this.ToolStripBtn3_Click);
            // 
            // _toolStripBtn4
            // 
            this._toolStripBtn4.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._toolStripBtn4.Image = ((System.Drawing.Image)(resources.GetObject("_toolStripBtn4.Image")));
            this._toolStripBtn4.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._toolStripBtn4.Name = "_toolStripBtn4";
            this._toolStripBtn4.Size = new System.Drawing.Size(23, 22);
            this._toolStripBtn4.Text = "4";
            this._toolStripBtn4.Click += new System.EventHandler(this.ToolStripBtn4_Click);
            // 
            // _toolStripBtn5
            // 
            this._toolStripBtn5.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._toolStripBtn5.Image = ((System.Drawing.Image)(resources.GetObject("_toolStripBtn5.Image")));
            this._toolStripBtn5.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._toolStripBtn5.Name = "_toolStripBtn5";
            this._toolStripBtn5.Size = new System.Drawing.Size(23, 22);
            this._toolStripBtn5.Text = "5";
            this._toolStripBtn5.Click += new System.EventHandler(this.ToolStripBtn5_Click);
            // 
            // backgroundWorker
            // 
            this.backgroundWorker.WorkerReportsProgress = true;
            this.backgroundWorker.WorkerSupportsCancellation = true;
            this.backgroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.BackgroundWorker_DoWork);
            this.backgroundWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.BackgroundWorker_ProgressChanged);
            this.backgroundWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.BackgroundWorker_RunWorkerCompleted);
            // 
            // HoReverseGui
            // 
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this._btnWriteText);
            this.Controls.Add(this._toolStrip1);
            this.Controls.Add(this._btnFeatureDown);
            this.Controls.Add(this._btnFeatureUp);
            this.Controls.Add(this._btnSplitAll);
            this.Controls.Add(this._btnSplitNodes);
            this.Controls.Add(this._btnGuardSpace);
            this.Controls.Add(this._btnGuardYes);
            this.Controls.Add(this._btnGuardNo);
            this.Controls.Add(this._toolStripContainer1);
            this.Controls.Add(this._menuStrip1);
            this.Controls.Add(this._btnActivityCompositeFromText);
            this.Controls.Add(this._btnDecisionFromText);
            this.Controls.Add(this._btnUpdateActivityParameter);
            this.Controls.Add(this._btnNoMerge);
            this.Controls.Add(this._btnBezier);
            this.Controls.Add(this._btnFinal);
            this.Controls.Add(this._btnActivity);
            this.Controls.Add(this._btnNoteFromText);
            this.Controls.Add(this._btnDecision);
            this.Controls.Add(this._btnHistory);
            this.Controls.Add(this._btnAction);
            this.Controls.Add(this._btnMerge);
            this.Controls.Add(this._btnInsert);
            this.Controls.Add(this.BtnTh);
            this.Controls.Add(this._btnLv);
            this.Controls.Add(this._btnBookmark);
            this.Controls.Add(this._btnBookmarkFrwrd);
            this.Controls.Add(this._btnBookmarkBack);
            this.Controls.Add(this._btnBookmarkRemove);
            this.Controls.Add(this._btnBookmarkAdd);
            this.Controls.Add(this._btnFrwrd);
            this.Controls.Add(this._btnBack);
            this.Controls.Add(this._btnLh);
            this.Controls.Add(this._btnC);
            this.Controls.Add(this._btnCopy);
            this.Controls.Add(this._btnD);
            this.Controls.Add(this._btnA);
            this.Controls.Add(this._btnOr);
            this.Controls.Add(this._btnComposite);
            this.Controls.Add(this._txtUserText);
            this.Controls.Add(this._btnDisplaySpecification);
            this.Controls.Add(this._btnFindUsage);
            this.Controls.Add(this._btnLocateType);
            this.Controls.Add(this._btnAddConstraint);
            this.Controls.Add(this._btnAddNoteAndLink);
            this.Controls.Add(this._btnAddElementNote);
            this.Controls.Add(this._btnLocateOperation);
            this.Controls.Add(this._btnDisplayBehavior);
            this.Controls.Add(this._btnOs);
            this.Controls.Add(this._btnTv);
            this.Name = "HoReverseGui";
            this.Size = new System.Drawing.Size(875, 468);
            this._toolTip.SetToolTip(this, "Progress capture all C/C++-Macros");
            this.Load += new System.EventHandler(this.AddinControl_Load);
            this._contextMenuStripTextField.ResumeLayout(false);
            this._menuStrip1.ResumeLayout(false);
            this._menuStrip1.PerformLayout();
            this._toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            this._toolStripContainer1.TopToolStripPanel.PerformLayout();
            this._toolStripContainer1.ResumeLayout(false);
            this._toolStripContainer1.PerformLayout();
            this._toolStrip6.ResumeLayout(false);
            this._toolStrip6.PerformLayout();
            this._toolStrip1.ResumeLayout(false);
            this._toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }


        /// <summary>
        /// Write text into selected element or connector (Name or guard (ControlFlow, Transition)
        /// Stick to one line, delete '\' at line end.
        /// 
        /// See also: Read from Text
        /// </summary>
        private void WriteFromText(string text)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                EA.Diagram dia = _repository.GetCurrentDiagram();
                if (dia == null) return;

                // don't use comma
                string s = text.Replace($"{Environment.NewLine}", " ");
 
                s = s.Replace(@"//", "");
                s = s.Replace(@"/*", "");
                s = s.Replace(@"*/", "");
                s = s.Replace(@"       ", " ");
                s = s.Replace(@"     ", " ");
                s = s.Replace(@"    ", " ");
                s = s.Replace(@"    ", " ");
                s = s.Replace(@"   ", " ");
                s = s.Replace(@"  ", " ").Trim();


                // Element to change
                EA.Element elSource = HoUtil.GetElementFromContextObject(_repository);
                if (elSource != null)
                {
                    if (s.Length > 252)
                    {
                        MessageBox.Show($@"Currunt length: {s.Length}", @"Length of name > 252 characters, name truncated!!");
                        s = s.Substring(0, 252);
                    }
                    elSource.Name = s;
                    elSource.Update();
                }
                // Connector to change
                else
                {
                    // Update connector (Name or Guard, dependent of connector type)
                    if (dia.SelectedConnector != null)
                    {
                        EA.Connector con = dia.SelectedConnector;
                        if ("ControlFlow ObjectFlow StateFlow".Contains(con.Type))
                        {
                            s = s.Replace(@"case ", ""); // no 'case ' (easy switch case formatting)
                            s = s.Replace(@":", "").Trim();

                            if (s.Length > 252)
                            {
                                MessageBox.Show($@"Currunt length: {s.Length}", @"Length of guard > 252 characters, guard truncated!!");
                                s = s.Substring(0, 252);
                            }
                            con.TransitionGuard = s;
                        }
                        else
                        {
                            if (s.Length > 252)
                            {
                                MessageBox.Show($@"Currunt length: {s.Length}", @"Length of name > 252 characters, name truncated!!");
                                s = s.Substring(0, 252);
                                con.Name = s;
                            }
                        }
                        con.Update();
                        _repository.ReloadDiagram(dia.DiagramID);

                    }
                }
                Cursor.Current = Cursors.Default;
            }
            catch (Exception e10)
            {
                MessageBox.Show(e10.ToString(), @"Error insert Attributes");
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }
        

        /// <summary>
        /// Insert for selected diagram node:
        /// - Behavior if Activity
        /// - Used/Realized interfaces if Class, Component
        /// </summary>
        private void InsertFromText()
        {
            EA.Diagram dia = _repository.GetCurrentDiagram();
            if (dia == null) return;

            // In activity it 
            if (dia.Type == "Activity")
            {
                HoService.InsertInActivtyDiagram(_repository, _txtUserText.Text, _addinSettings.UseCallBehaviorAction);
            }
            else
            {
                HoService.InsertInterface(_repository, dia, _txtUserText.Text);
            }
        }





        private void BtnNoteFromText_Click(object sender, EventArgs e)
        {
            HoService.CreateNoteFromText(_repository, _txtUserText.Text);
        }


        private void BtnActivityCompositeFromText_Click(object sender, EventArgs e)
        {
            HoService.CreateCompositeActivityFromText(_repository, Cutil.RemoveCasts(_txtUserText.Text.Trim()));

        }

        /// <summary>
        /// Insert Activity from text. This is used for e.g.: 'Loops'
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnActivityFromText_Click(object sender, EventArgs e)
        {
            HoService.CreateActivityFromText(_repository, Cutil.RemoveCasts(_txtUserText.Text.Trim()));
        }

        void BtnC_Click(object sender, EventArgs e)
        {
            HoService.SetLineStyle(_repository, "C");
        }


        void BtnD_Click(object sender, EventArgs e)
        {
            HoService.SetLineStyle(_repository, "D");
        }



        void BtnA_Click(object sender, EventArgs e)
        {
            HoService.SetLineStyle(_repository, "A");
        }


        void BtnOR_Click(object sender, EventArgs e)
        {
            HoService.SetLineStyle(_repository, "OR");
        }

        private void BtnBezier_Click(object sender, EventArgs e)
        {
            HoService.SetLineStyle(_repository, "B");
        }

        private void BtnComposite_Click(object sender, EventArgs e)
        {
            HoService.NavigateComposite(_repository);
        }

        private void BtnBack_Click(object sender, EventArgs e)
        {
            _history.Back();
        }

        private void BtnFrwrd_Click(object sender, EventArgs e)
        {
            _history.Frwrd();
        }

        //------- Bookmark ----------------------------//
        private void BtnBookmarkAdd_Click(object sender, EventArgs e)
        {
            EA.ObjectType ot = _repository.GetContextItemType();
            string guid;
            switch (ot)
            {
                case EA.ObjectType.otDiagram:
                    EA.Diagram dia = (EA.Diagram) _repository.GetContextObject();
                    guid = dia.DiagramGUID;
                    _bookmark.Add(ot, guid);
                    break;
                case EA.ObjectType.otPackage:
                    EA.Package pkg = (EA.Package) _repository.GetContextObject();
                    guid = pkg.PackageGUID;
                    _bookmark.Add(ot, guid);
                    break;
                case EA.ObjectType.otElement:
                    EA.Element el = (EA.Element) _repository.GetContextObject();
                    guid = el.ElementGUID;
                    _bookmark.Add(ot, guid);
                    break;
            }
        }

        private void BtnBookmarkRemove_Click(object sender, EventArgs e)
        {
            EA.ObjectType ot = _repository.GetContextItemType();
            string guid = "";
            if (ot.Equals(EA.ObjectType.otDiagram))
            {
                EA.Diagram dia = (EA.Diagram) _repository.GetContextObject();
                guid = dia.DiagramGUID;
            }
            _bookmark.Remove(ot, guid);
        }

        private void BtnBookmarkBack_Click(object sender, EventArgs e)
        {
            _bookmark.Back();
        }

        private void BtnBookmarkFrwrd_Click(object sender, EventArgs e)
        {
            _bookmark.Frwrd();
        }

        private void BtnBookmarks_Click(object sender, EventArgs e)
        {
            ShowHistory();
        }

        private void ShowHistory()
        {
            if (_wpfDiagram == null)
            {
                _wpfDiagram = new WpfDiagram.Diagram(_repository, _history, _bookmark);
                _wpfDiagram.Show();
            }
            else
            {
                if (!_wpfDiagram.IsLoaded)
                {
                    _wpfDiagram = new WpfDiagram.Diagram(_repository, _history, _bookmark);
                    _wpfDiagram.Show();
                }
                else _wpfDiagram.Activate();
            }
        }

        private void BtnHistory_Click(object sender, EventArgs e)
        {
            ShowHistory();
        }

        private void BtnFinal_Click(object sender, EventArgs e)
        {
            EA.Element el = HoUtil.GetElementFromContextObject(_repository);
            if (el == null) return;
            if ("Interface Class Component".Contains(el.Type))
                HoService.CreateOperationsFromTextService(_repository, _txtUserText.Text);
            else HoService.InsertDiagramElementAndConnect(_repository, "StateNode", "101");


        }

        private void BtnAction_Click(object sender, EventArgs e)
        {
            HoService.CreateDiagramObjectFromContext(_repository, "", "Action", "");

        }

        private void BtnDecision_Click(object sender, EventArgs e)
        {
            HoService.CreateDiagramObjectFromContext(_repository, "", "Decision", "");

        }

        private void BtnMerge_Click(object sender, EventArgs e)
        {
            HoService.InsertDiagramElementAndConnect(_repository, "MergeNode", "");
        }
        /// <summary>
        /// Insert from Entry field:
        /// - Code converted to Activity Diagram Items
        /// - If context element is Class, Interface or Component insert Attributes, Structures or Enums 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnInsert_Click(object sender, EventArgs e)
        {


            EA.Element el = HoUtil.GetElementFromContextObject(_repository);
            if (el == null) return;
            try
            {

                // Insert Attributes, Structures, Enums
                if ("Class Interface Component".Contains(el.Type))
                    HoService.InsertAttributeService(_repository, _txtUserText.Text);
                // Insert Behavior
                else InsertFromText();
            }
            catch (Exception e10)
            {
                MessageBox.Show(e10.ToString(), @"Error insert Attributes");
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }



        }

        private void BtnActionFromText_Click(object sender, EventArgs e)
        {
            string name = CallOperationAction.RemoveUnwantedStringsFromText(_txtUserText.Text.Trim());
            HoService.CreateDiagramObjectFromContext(_repository, name, "Action", "");

        }

        private void BtnDecisionFromText_Click(object sender, EventArgs e)
        {
            string decisionName = _txtUserText.Text.Trim();
            HoService.CreateDecisionFromText(_repository, decisionName);
        }


        private void TxtUserText_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            // force cr lf at line end
            _txtUserText.Text = Clipboard.GetText().Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\r\n");

        }

        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Save();
        }

        private void ClearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _history.RemoveAll();
            _bookmark.RemoveAll();
        }

        private void SettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _frmSettings = new SettingsForm(AddinSettings, this);
            _frmSettings.ShowDialog(this);
        }


        /// <summary>
        /// Outputs the About message with the releases of all used *.dlls
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string[] dllNames = new string[]
            {
                "hoReverseGui.dll",
                "hoReverseRoot.dll",
                "hoServices.dll",
                "hoLinqToSql.dll",
                "hoUtils.dll",
                "HtmlAgilityPack.dll",
                "Newtonsoft.Json.dll",
                "linq2db.dll",
                "MariGold.HtmlParser.dll",
                "MariGold.OpenXHTML.dll",
                "MySql.Data.dll",
                "Microsoft.SqlServer.Types.dll",
                "Oracle.ManagedDataAccess.dll",
                "Npgsql.dll",
                "Sybase.AdoNet2.AseClient.dll",
                "SQLite.Interop.dll",
                "System.Data.SQLite.dll",
                "OpenMcdf.dll",
                "ReqIFSharp.dll",
                "ClosedXml.dll",
                "DocumentFormat.OpenXml.dll",
                "KBCsv.dll",
                "KBCsv.Extensions.Data.dll"

            };
            HoUtil.AboutMessage("C - Reverse Engineering Workbench",
                "About hoReverse Workbench",
                dllNames,
                _repository,
                _addinSettings.ConfigFilePath);

        }


        private void CreateActivityForOperationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HoService.CreateActivityForOperation(_repository);
        }

        private void BtnShowPort_Click(object sender, EventArgs e)
        {
            HoService.ShowEmbeddedElementsGui(_repository, "");
        }

        // show folder for:
        // - controlled package
        // - source file
        private void ShowFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HoService.ShowFolder(_repository, isTotalCommander: AddinSettings.FileManagerIsTotalCommander);
        }


        private void GetVcLastestRecursiveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HoService.GetVcLatestRecursive(_repository);
        }

        private void CopyGuidSqlToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HoService.CopyGuidSqlToClipboard(_repository);
        }

        private void CreateSharedMemoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HoService.CreateSharedMemoryFromText(_repository, _txtUserText.Text);
        }


        private void UpdateMethodParametersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HoService.UpdateActivityMethodParameterWrapper(_repository);
        }

        private void HelpF1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Open GitHub Wiki
            WikiRef.Wiki();
        }

        private void DeleteToolStripMenuItemTextField_Click(object sender, EventArgs e)
        {
            _txtUserText.Text = "";
        }

        private void QuickSearchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HoService.RunQuickSearch(_repository, _addinSettings.QuickSearchName, _txtUserText.Text);
        }

        private void InsertTextIntoNodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HoService.CreateNoteFromText(_repository, _txtUserText.Text);
        }

        private void InsertBeneathNodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InsertFromText();
        }

        private void AddCompositeActivityToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string name = CallOperationAction.RemoveUnwantedStringsFromText(_txtUserText.Text.Trim());
            HoService.CreateDiagramObjectFromContext(_repository, name, "Activity", "Comp=yes");
        }

        private void AddActivityToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string name = CallOperationAction.RemoveUnwantedStringsFromText(_txtUserText.Text.Trim());
            HoService.CreateDiagramObjectFromContext(_repository, name, "Activity", "Comp=no");
        }

        private void AddFinalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HoService.CreateDiagramObjectFromContext(_repository, "", "StateNode", "101");
        }

        private void AddMergeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HoService.CreateDiagramObjectFromContext(_repository, "", "MergeNode", "");
        }

        private void ShowAllPortsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HoService.ShowEmbeddedElementsGui(_repository);
        }

        // no/else merge
        private void BtnNoMerge_Click(object sender, EventArgs e)
        {
            //EaService.createDiagramObjectFromContext(m_repository, "", "MergeNode", "no");
            HoService.InsertDiagramElementAndConnect(_repository, "MergeNode", "", "no");
        }


        // load Addin Control
        private void AddinControl_Load(object sender, EventArgs e)
        {
            
        }

        /// <summary>
        /// Get the values from the 'Settings.json' file and update the File Menu to accomplish bulk change be Menu
        /// - DiagramTypes
        /// </summary>

        private void GetValueSettingsFromJson()
        {
            try
            {
                // If Settings.json don't exists: Copy delivery Setting.json file to settings folder
                // ReSharper disable once AssignNullToNotNullAttribute
                string sourceSettingsPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    JasonFile);
                string targetSettingsPath = AddinSettings.ConfigFolderPath + "Settings.json";

                // Copy delivery Setting.json file to settings folder 
                if (!File.Exists(targetSettingsPath))
                {
                    File.Copy(sourceSettingsPath, targetSettingsPath);
                }

                // Add Diagram Style 
                // ReSharper disable once AssignNullToNotNullAttribute
                _jasonFilePath = targetSettingsPath;

                _importSettings = new ImportSetting(_jasonFilePath);
                _diagramStyle = new DiagramFormat(_jasonFilePath);
                HoService.DiagramStyle = _diagramStyle;

               

                // check if the menu entries already exists
                if (_doMenuDiagramStyleInserted)
                {
                    for (int i = 0; i < 7; i++)
                    {
                        int index = _doToolStripMenuItem.DropDownItems.Count - 1;
                        _doToolStripMenuItem.DropDownItems.RemoveAt(index);

                    }


                }
                else
                {
                    _doToolStripMenuItem.DropDownItems.Add(new ToolStripSeparator());
                }
                // Change Diagram Styles
                _doToolStripMenuItem.DropDownItems.Add(_diagramStyle.ConstructStyleToolStripMenuDiagram(
                    _diagramStyle.DiagramStyleItems,
                    "Bulk Diagram Style/Theme Recursive",
                    "Bulk Change the Diagram Style/Theme recursive\r\nSelect\r\n-Package \r\n-Element \r\n-Diagrams",
                    ChangeDiagramStyleRecursiv_Click));
                _doToolStripMenuItem.DropDownItems.Add(_diagramStyle.ConstructStyleToolStripMenuDiagram(
                    _diagramStyle.DiagramStyleItems,
                    "Bulk Change Diagram Style/Theme",
                    "Bulk Change the Diagram/Theme Style\r\nSelect\r\n-Package \r\n-Element \r\n-Diagrams",
                    ChangeDiagramStylePackage_Click));
                _doToolStripMenuItem.DropDownItems.Add(new ToolStripSeparator());
                // Change Diagram Object Styles
                _doToolStripMenuItem.DropDownItems.Add(_diagramStyle.ConstructStyleToolStripMenuDiagram(
                    _diagramStyle.DiagramObjectStyleItems,
                    "Change DiagramObject Style",
                    "Change the DiagramObject Style\r\nSelect\r\n-Diagram \r\n-DiagramObject/Node",
                    ChangeDiagramObjectStylePackage_Click));
                // Change Diagram Link Styles
                _doToolStripMenuItem.DropDownItems.Add(_diagramStyle.ConstructStyleToolStripMenuDiagram(
                    _diagramStyle.DiagramLinkStyleItems,
                    "Change DiagramLink Style",
                    "Change the DiagramLink Style\r\nSelect\r\n-Diagram \r\n-DiagramLink",
                    ChangeDiagramLinkStylePackage_Click));
                _doToolStripMenuItem.DropDownItems.Add(new ToolStripSeparator());
                //-------------------------------------------------------------------------------------
                // Bulk change EA items
                _doToolStripMenuItem.DropDownItems.Add(_diagramStyle.ConstructStyleToolStripMenuDiagram(
                    _diagramStyle.BulkElementItems,
                    "Bulk item change",
                    "Change selected EA items\r\nSelect\r\n-Diagram objects",
                    BulkChangeEaItems_Click));
                _doToolStripMenuItem.DropDownItems.Add(_diagramStyle.ConstructStyleToolStripMenuDiagram(
                    _diagramStyle.BulkElementItems,
                    "Bulk item change Package recursive",
                    "Change selected EA items in Package (recursive)\r\nSelect\r\n-Package",
                    BulkChangeEaItemsRecursive_Click));

                //-------------------------------------------------------------------------------------
                // Importer: Import *.csv, ReqIf from according to specification in Settings.Json
                _doToolStripMenuItem.DropDownItems.Add(new ToolStripSeparator());
                _doToolStripMenuItem.DropDownItems.Add(_importSettings.ConstructImporterMenuItems(
                    _importSettings.ImportSettings,
                    "Import *.csv, ReqIF",
                    "Import Requirements (*.csv, ReqIF, DOORS ReqIf)",
                    Importer_Click,
                    MenuItemHover_MouseHover,  // Optional register the mouse hover event 
                    "Locate Package",  MenuItemContext_MouseDown));





                _doMenuDiagramStyleInserted = true;
            }
            catch (Exception e1)
            {
                MessageBox.Show($@"'{_jasonFilePath}'

{e1}", @"Error loading 'Settings.json'");
            }
        }

        private void ChangeXmlPathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HoService.SetNewXmlPath(_repository);

        }

        private void InsertAttributeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HoService.InsertAttributeService(_repository, _txtUserText.Text);

        }



        private void InsertTypedefStructToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HoService.CreateTypeDefStructFromTextService(_repository, _txtUserText.Text);
        }


        


        private void ToolStripBtn1_Click(object sender, EventArgs e)
        {
            RunService(0);


        }

        private void ToolStripBtn2_Click(object sender, EventArgs e)
        {
            RunService(1);
        }

        private void ToolStripBtn3_Click(object sender, EventArgs e)
        {
            RunService(2);

        }

        private void ToolStripBtn4_Click(object sender, EventArgs e)
        {

            RunService(3);

        }

        private void ToolStripBtn5_Click(object sender, EventArgs e)
        {

            RunService(4);

        }

        private void VcReconsileService(EA.Repository rep)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                HoService.VcReconcile(rep);
                Cursor.Current = Cursors.Default;
            }
            catch (Exception e11)
            {
                MessageBox.Show(e11.ToString(), @"Error VcReconsile");
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

        }


        private void RunSearch(int pos)
        {
            if (_addinSettings.ShortcutsSearch[pos] is EaAddinShortcutSearch sh)
            {
                if (sh.keySearchName == "") return;
                try
                {
                    // Use Search Term from Configuration or from GUI
                    string searchTerm = sh.keySearchTerm.Trim();
                    if (searchTerm.ToLower().Equals("<search term>")) searchTerm = _txtUserText.Text;
                    _repository.RunModelSearch(sh.keySearchName, searchTerm, "", "");
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString(), @"Error start search '" + sh.keySearchName +
                                                  @" " + sh.keySearchTerm + @"'");
                }
            }
        }

        private void RunService(int pos)
        {
            if (_addinSettings.ShortcutsServices[pos] is ServicesCallConfig sh)
            {
                if (sh.Method == null) return;
                sh.Invoke(_repository, _txtUserText.Text);

            }
        }

        private void ToolStripBtn11_Click(object sender, EventArgs e)
        {
            RunSearch(0);

        }

        private void ToolStripBtn12_Click(object sender, EventArgs e)
        {
            RunSearch(1);
        }

        private void ToolStripBtn13_Click(object sender, EventArgs e)
        {
            RunSearch(2);
        }

        private void ToolStripBtn14_Click(object sender, EventArgs e)
        {
            RunSearch(3);
        }

        private void ToolStripBtn15_Click(object sender, EventArgs e)
        {
            RunSearch(4);
        }

        private void SetSvnTaggedValuesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HoService.SetTaggedValueGui(_repository);



        }

        private void SetSvnKeywordsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EA.ObjectType oType = _repository.GetContextItemType();
            if (!oType.Equals(EA.ObjectType.otPackage)) return;

            EA.Package pkg = (EA.Package) _repository.GetContextObject();
            HoService.SetSvnProperty(_repository, pkg);
        }

        private void SvnLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EA.ObjectType oType = _repository.GetContextItemType();
            if (!oType.Equals(EA.ObjectType.otPackage)) return;

            EA.Package pkg = (EA.Package) _repository.GetContextObject();
            HoService.GotoSvnLog(_repository, pkg);
        }

        private void SvnTortoiseRepobrowserToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EA.ObjectType oType = _repository.GetContextItemType();
            if (!oType.Equals(EA.ObjectType.otPackage)) return;

            EA.Package pkg = (EA.Package) _repository.GetContextObject();
            HoService.GotoSvnBrowser(_repository, pkg);
        }

        private void ShowDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HoService.ShowFolder(_repository, isTotalCommander: _addinSettings.FileManagerIsTotalCommander);
        }

        private void SetSvnTaggedValuesToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            EA.ObjectType oType = _repository.GetContextItemType();
            if (!oType.Equals(EA.ObjectType.otPackage)) return;

            EA.Package pkg = (EA.Package) _repository.GetContextObject();
            HoService.SetDirectoryTaggedValues(_repository, pkg);
        }

        private void ToolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void BtnNoGuard_Click(object sender, EventArgs e)
        {
            HoService.NoGuard(_repository, "no");
        }

        private void BtnYesGuard_Click(object sender, EventArgs e)
        {
            HoService.NoGuard(_repository, "yes");
        }

        private void BtnBlankGuard_Click(object sender, EventArgs e)
        {
            HoService.NoGuard(_repository, "");
        }


        private void BtnJoinNodes_Click(object sender, EventArgs e)
        {
            HoService.JoinDiagramObjectsToLastSelected(_repository);
        }

        private void BtnSplitNodes_Click(object sender, EventArgs e)
        {
            HoService.SplitDiagramObjectsToLastSelected(_repository);
        }

        private void MakeNestedOfToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HoService.MakeNested(_repository);
        }

        private void BtnSplitAll_Click(object sender, EventArgs e)
        {
            HoService.SplitAllDiagramObjectsToLastSelected(_repository);
        }

        private void DeleteInvisibleuseRealizationDependenciesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                HoService.DeleteInvisibleUseRealizationDependencies(_repository);
                Cursor.Current = Cursors.Default;
            }
            catch (Exception e10)
            {
                MessageBox.Show(e10.ToString(), @"Error insert Attributes");
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

        }
        /// <summary>
        /// Generate Component ports for selected Component in Diagram
        /// - It uses all Classes of Diagram
        /// - It uses all Interfaces of Diagram
        /// - It skips '_i' interfaces (private)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GenerateComponentPortsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HoService.GenerateComponentPortsService(_repository);


        }

        private void VCResyncToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HoService.UpdateVcStateOfSelectedPackageRecursiveService(_repository);

        }

        private void VCXMIReconsileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HoService.VcReconcile(_repository);
        }

        private void VCGetStateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HoService.VcGetState(_repository);

        }
        




        private void CopyReleaseInformationToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HoService.CopyReleaseInfoOfModuleService(_repository);
        }

        private void ShowAllPortsActivityParametersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HoService.ShowEmbeddedElementsGui(_repository, "");
        }

        private void Setting2ConnectorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _frmSettings2 = new Settings2.Settings2Forms(AddinSettings, this);
            _frmSettings2.ShowDialog(this);
        }

        /// <summary>
        /// Write text into the selected element/connector
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _btnWriteText_Click(object sender, EventArgs e)
        {
            WriteFromText(_txtUserText.Text);

        }

        private void AddMacroToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                HoService.AddMacroFromText(_repository, _txtUserText.Text);
                Cursor.Current = Cursors.Default;
            }
            catch (Exception e10)
            {
                MessageBox.Show(e10.ToString(), @"Error add macro/stereotypes");
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

        }

        private void SetMacroToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string s = _txtUserText.Text.Trim();
            if (s == "") s = "define";
            SetMacro(s);


        }

        /// <summary>
        /// Delete Macro
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DelMacroToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetMacro("");

        }

        private void _generateIncludeForClassifierToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                HoService.GenerateUseInterface(_repository);
                Cursor.Current = Cursors.Default;
            }
            catch (Exception e10)
            {
                MessageBox.Show(e10.ToString(), @"Error generating used interfaces for selected Class/Interface");
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

        }

        private void BtnFeatureUp_Click(object sender, EventArgs e)
        {
            HoService.FeatureUp(_repository);
        }


        private void BtnFeatureDown_Click(object sender, EventArgs e)
        {
            HoService.FeatureDown(_repository);
        }

        private void SetFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HoService.SetFolder(_repository);
        }

        /// <summary>
        /// Show feature:
        /// Add Note and link to feature
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _btnAddNoteAndLink_Click(object sender, EventArgs e)
        {
            HoService.AddElementsToDiagram(_repository, "Note", connectorLinkType: "Element Note", bound:true);
        }
        /// <summary>
        /// Show Note:
        /// Add a Note to selected
        /// - Element/Package<para/>
        /// - Diagram
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _btnAddElementNote_Click(object sender, EventArgs e)
        {
            HoService.AddElementsToDiagram(_repository, "Note", connectorLinkType: "", bound:false);
        }
        /// <summary>
        /// Show constraint
        /// Add a Constraint to selected
        /// - Element/Package<para/>
        /// - Diagram
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _btnAddConstraint_Click(object sender, EventArgs e)
        {
            HoService.AddElementsToDiagram(_repository, "Constraint", connectorLinkType: "", bound:false);
        }

        /// <summary>
        /// Copy selected Element text to clipboard
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _btnCopy_Click(object sender, EventArgs e)
        {
            
            _txtUserText.Text = HoService.CopyContextNameToClipboard(_repository);
        }

        private void EndifMacroToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WriteFromText("#endif");
        }

        private void ExternToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string s = "extern";
            SetMacro(s);
        }


        /// <summary>
        /// Set macro to the specified value. If no value is specified use 'define' as stereotype
        /// </summary>
        /// <param name="s"></param>
        private void SetMacro(string s)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                HoService.SetMacroFromText(_repository, s);
                Cursor.Current = Cursors.Default;
            }
            catch (Exception e10)
            {
                MessageBox.Show(e10.ToString(), @"Error set macro/stereotypes");
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void _updateActionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HoService.UpdateAction(_repository);
        }



        private void _hideAllPortsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HoService.HideEmbeddedElements(_repository);
        }

        private void StandardDiagramToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // the styles to set, don't use semicolon
            string[] styleEx = new string[3];
            styleEx[0] = "HideQuals=1,PPgs.cx=1,PPgs.cy=1,ScalePI=1,OpParams=2";
            styleEx[1] = "";
            styleEx[2] = "";

            HoService.ChangeDiagramStyle(_repository,  styleEx, ChangeScope.PackageRecursive);

        }

       

       // Change style recursive
        void ChangeDiagramStyleRecursiv_Click(object sender, EventArgs e)
        {
            ChangeDiagramStyle(sender, ChangeScope.PackageRecursive);
        }
        void ChangeDiagramStylePackage_Click(object sender, EventArgs e)
        {
            ChangeDiagramStyle(sender, ChangeScope.Package);
        }
        // Change diagram style recursive
        void ChangeDiagramObjectStylePackage_Click(object sender, EventArgs e)
        {
            ChangeDiagramObjectStyle(sender, ChangeScope.Package);
        }
        // Change diagram style recursive
        void ChangeDiagramLinkStylePackage_Click(object sender, EventArgs e)
        {
            ChangeDiagramLinkStyle(sender, ChangeScope.Package);
        }

        // Bulk change EA items 
        void BulkChangeEaItems_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            Debug.Assert(item != null, nameof(item) + " != null");
            if (item?.Tag == null) return;
            BulkElementItem bulkElement = (BulkElementItem)item.Tag;
            BulkItemChange.BulkChange(_repository,  bulkElement);
        }
        void BulkChangeEaItemsRecursive_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            Debug.Assert(item != null, nameof(item) + " != null");
            if (item?.Tag == null) return;
            BulkElementItem bulkElement = (BulkElementItem)item.Tag;
            BulkItemChange.BulkChangeRecursive(_repository, bulkElement);

        }


        /// <summary>
        /// Event Import *.csv, ReqIF,.. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Importer_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            Debug.Assert(item != null, nameof(item) + " != null");
            if (item?.Tag == null) return;
            FileImportSettingsItem importElement = (FileImportSettingsItem)item.Tag;
            if (int.TryParse(importElement.ListNo, out var listNumber))
                ImportBySettings(listNumber,withMessage:true);
            else MessageBox.Show($@"{importElement.ListNo}", $@"ListNo in Importer settings.json invalid");

        }
        /// <summary>
        /// Locate package of to imported file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ImporterLocatePackage_Click(object sender, EventArgs e)
        {
            if (!(sender is MenuItem mItem)) return;

            // use the last menu item
            if (_lastMenuItem?.Tag == null) return;
            FileImportSettingsItem importElement = (FileImportSettingsItem)_lastMenuItem?.Tag;

            // locate package 
            if (importElement.PackageGuidList.Count == 0)
            {
                MessageBox.Show($@"{importElement.ListNo}", $@"Guid in Importer settings.json invalid");
                return;
            }
            string guid = importElement.PackageGuidList[0];
            EA.Package pkg = _repository.GetPackageByGuid(guid);
            if (pkg == null)
            {
                MessageBox.Show($@"{importElement.ListNo}", $@"Guid in Importer settings.json invalid");
                return;
            }
            _repository.ShowInProjectView(pkg);
        }

        /// <summary>
        /// Handle MouseDown events from menue items to output a Package context menue.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MenuItemContext_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) return;

            ToolStripMenuItem mId = (ToolStripMenuItem)sender;

            ContextMenu tsmiContext = new ContextMenu();

            MenuItem item1 = new MenuItem {Text = @"Locate Package"};


            tsmiContext.MenuItems.Add(item1);

            item1.Click += new EventHandler(ImporterLocatePackage_Click);

            //hndPass = mID.Text;
            tsmiContext.Show(_menuStrip1, _menuStrip1.PointToClient(new Point(Cursor.Position.X, Cursor.Position.Y)));
        }
        /// <summary>
        /// Remember the last menu item the mouse hovers over.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MenuItemHover_MouseHover(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item)
                _lastMenuItem = item;
        }

        /// <summary>
        /// Change diagram object style for selected diagram objects or all diagram objects if nothing selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="changeScope"></param>
        private void ChangeDiagramObjectStyle(object sender, ChangeScope changeScope)
        {


            ToolStripMenuItem item = sender as ToolStripMenuItem;
            Debug.Assert(item != null, nameof(item) + " != null");
            if (item?.Tag == null) return;
            DiagramObjectStyleItem style = (DiagramObjectStyleItem)item.Tag;

            HoService.DiagramObjectStyleWrapper(_repository, style.Type, style.Style, style.Property, changeScope);

        }

        private void ChangeDiagramStyle(object sender, ChangeScope changeScope)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem; //((ToolStripMenuItem) sender).Tag; DiagramStyleItem
            Debug.Assert(item != null, nameof(item) + " != null");
            if (item?.Tag == null) return;
            DiagramStyleItem style = (DiagramStyleItem) item.Tag;

            // [0] StyleEx
            // [1] PDATA
            // [2] Properties
            // [3] Diagram types
            string[] styleEx = new string[4];
            styleEx[0] = style.StyleEx;
            styleEx[1] = style.Pdata;
            styleEx[2] = style.Property;
            styleEx[3] = style.Type;
            HoService.ChangeDiagramStyle(_repository, styleEx, changeScope);
        }

        private void SettingsDiagramStylesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HoUtil.StartFile(_jasonFilePath);
        }

        private void ReloadSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GetValueSettingsFromJson();
        }

        private void ResetFactorySettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                $@"hoTools deletes 'User.config' and 'Settings.json'. After Restart hoTools, you have the initial settings.

You find the saved settings in: '{Path.GetDirectoryName(AddinSettings.CurrentConfig.FilePath)}':
- User.Config.tmp
- Settings.Json.tmp
",
                @"Do you want to reset your configuration?", MessageBoxButtons.OKCancel);

            if (result == DialogResult.OK)
            {
                string filePath = AddinSettings.Reset();
                MessageBox.Show(
                    $@"'User.config' and 'Settings.json' from {
                            Path.GetDirectoryName(filePath)
                        } saved to *.tmp´and deleted.

Please restart EA. During restart hoTools loads the default settings.",
                    @"Configuration reset to default. Please Restart!");
            }
        }

        private void InsertFunctionToolStripMenuItem_Click(object sender, EventArgs e)
        {

            HoService.CreateOperationsFromTextService(_repository, _txtUserText.Text);

        }
        private void InsertFunctionMakeDuplicatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HoService.CreateOperationsFromTextService(_repository, _txtUserText.Text, makeDuplicateOperations:true);
        }
        /// <summary>
        /// Change diagram link style for selected diagram links 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="changeScope"></param>
        private void ChangeDiagramLinkStyle(object sender, ChangeScope changeScope)
        {


            ToolStripMenuItem item = sender as ToolStripMenuItem;
            if (item?.Tag == null) return;
            DiagramLinkStyleItem style = (DiagramLinkStyleItem)item.Tag;

            HoService.DiagramLinkStyleWrapper(_repository, style.Type, style.Style, style.Property, changeScope);

        }

        private void GenerateModulesToolStripMenuItem_Click(object sender, EventArgs e)
        {



            DateTime startDate = DateTime.Now;
            Cursor.Current = Cursors.WaitCursor;

            // Get selected package
            // Only for new elements
            EA.ObjectType objectType = _repository.GetContextItem(out var contextPackage);
            if (!objectType.Equals(EA.ObjectType.otPackage))
            {
                MessageBox.Show(@"Select a package for the newly created stuff, break!!!");
                return;
            }
            EA.Package pkg = (EA.Package)contextPackage;
            if (pkg.IsModel)
            {
                MessageBox.Show(@"Don't select a Model (root package), break!!!");
                return;
            }
            AutoCpp autoCpp = new AutoCpp(_repository, pkg );


            autoCpp.InventoryInterfaces();
            autoCpp.InventoryDesignInterfaces();
            int newAttributes = autoCpp.GenerateInterfaces();


            //autoCpp.InventoryFiles();
            //autoCpp.Generate();
            string duration = startDate.Subtract(DateTime.Now).Duration().ToString(@"mm\:ss");

            MessageBox.Show($@"Function count_____:{Tab}{Tab}{autoCpp.Functions.FunctionList.Count,20:N0}
File count/created/deleted:{Tab}{autoCpp.Files.FileList.Count,20:N0}/{autoCpp.CreatedInterfaces}/{autoCpp.DeletedInterfaces}
Functions not found:{Tab}{Tab}{autoCpp.FunctionsNotFound.Count,20:N0}
Functions added:{Tab}{Tab}{newAttributes,20:N0}

Duration:__________:{Tab}{Tab}{Tab}{duration} mm:ss",@"Generation finished");
            Cursor.Current = Cursors.Default;

        }

        private void InventoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DateTime startDate = DateTime.Now;
            Cursor.Current = Cursors.WaitCursor;

            // Get selected package
            // Only for new elements
            EA.ObjectType objectType = _repository.GetContextItem(out var contextPackage);
            if (!objectType.Equals(EA.ObjectType.otPackage))
            {
                MessageBox.Show(@"Select a package for the newly created stuff, break!!!");
                return;
            }
            EA.Package pkg = (EA.Package)contextPackage;
            if (pkg.IsModel)
            {
                MessageBox.Show(@"Don't select a Model (root package), break!!!");
                return;
            }
            AutoCpp autoCpp = new AutoCpp(_repository, pkg);


            autoCpp.InventoryInterfaces();
            autoCpp.InventoryDesignInterfaces();
            int newAttributes;
            newAttributes = autoCpp.GenerateInterfaces();


            //autoCpp.InventoryFiles();
            //autoCpp.Generate();
            string duration = startDate.Subtract(DateTime.Now).Duration().ToString(@"mm\:ss");

            MessageBox.Show($"Function count_____:\t\t{autoCpp.Functions.FunctionList.Count,20:N0}\r\n" +
                            $"File count/created/deleted:\t{autoCpp.Files.FileList.Count,20:N0}/{autoCpp.CreatedInterfaces}/{autoCpp.DeletedInterfaces}\r\n" +
                            $"Functions not found:\t\t{autoCpp.FunctionsNotFound.Count,20:N0}\r\n" +
                            $"Functions added:\t\t{newAttributes,20:N0}\r\n" +
                            $"Duration:__________:\t\t\t{duration} mm:ss", "Generation finished");
            Cursor.Current = Cursors.Default;

        }

        private void _vCRemoveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (EA.ObjectType.otPackage == _repository.GetContextItem(out var pkg))
            {
                HoService.VcControlRemove((EA.Package)pkg);
            }
        }
        /// <summary>
        /// Move usage of an element to a target element.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MoveUsageToElementToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HoService.MoveUsage(_repository);
        }

        private void SortAlphabeticToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HoService.SortAlphabetic(_repository);
        }

        private void RepoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WikiRef.Repo();
        }

        private void ReadmeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WikiRef.ReadMe();
        }

        private void HelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WikiRef.Wiki();
        }

        private void LineStyleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WikiRef.WikiLineStyle();
        }


        /// <summary>
        /// Get external function for selected Component, Class
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_repository.GetContextItem(out var eaObject) == ObjectType.otElement)
            {
                EA.Element component = (EA.Element) eaObject;
                if (component.Type == "Component" || component.Type == "Class")
                {
                    var generator = new AutoCpp(_repository, component);
                    generator.GenExternalFuntionsOfComponent();
                }
            }
        }


        /// <summary>
        /// Set stereotype of Port to 'Runnable'
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MakeRunnableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetStereotype("Runnable");

        }
        private void MakeServicePortToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetStereotype("Service");
        }

        private void MakeCalloutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetStereotype(@"Callout");
        }

        private void SetStereotype(string stereotype)
        {
            var eaDia = new EaDiagram(_repository);
            if (eaDia.SelectedObjectsCount > 1)
            {
                foreach (var port in eaDia.SelElements)
                {
                    if (port.Type == "Port")
                    {
                        port.Stereotype = stereotype;
                        port.Update();
                    }
                }
            }
            else
            {
                EA.ObjectType type = _repository.GetContextItem(out var obj);
                if (type == EA.ObjectType.otElement)
                {
                    EA.Element port = (EA.Element)obj;
                    if (port.Type == "Port")
                    {
                        port.Stereotype = stereotype;
                        port.Update();
                    }
                }
            }
        }

        /// <summary>
        /// Show external functions for selected Component / Class
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowExternalComponentFunctionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_autoCppIsRunning)
            {
                MessageBox.Show($@"The Code inventory is {progressBar1.Value}% finished", @"Code inventory not finished, retry!");

            }
            else
            {
                if (!_addinSettings.IsFolderPathCSourceCode()) return;
                Cursor.Current = Cursors.WaitCursor;
                EA.ObjectType type = _repository.GetContextItem(out var obj);
                if (type == EA.ObjectType.otElement)
                {
                    EA.Element element = (EA.Element) obj;
                    if (element.Type == "Component" || element.Type == "Class")
                    {
                        _autoCpp.ShowInterfacesOfElement(element,_addinSettings.FolderPathCSourceCode);
                    }
                }
                Cursor.Current = Cursors.Default;
            }

        }
        /// <summary>
        /// Show Provided/Required Functions for Folder or File which you choose with File-Dialog
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowProvidedRequiredFunctionsForSourceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_autoCppIsRunning)
            {
                MessageBox.Show($@"The Code inventory is {progressBar1.Value}% finished", @"Code inventory not finished, retry!");

            }
            else
            {
                if (!_addinSettings.IsFolderPathCSourceCode()) return;
                Cursor.Current = Cursors.WaitCursor;
                string fileFolderName = GetFolderOrFile(_addinSettings.FolderPathCSourceCode);

                if (fileFolderName != "")
                    _autoCpp.ShowInterfacesOfElement(null, _addinSettings.FolderPathCSourceCode, fileFolderName);

                Cursor.Current = Cursors.Default;
            }

        }
        /// <summary>
        /// Background worker to run capture macros task in background
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackgroundWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            // it updates the progress by sending the percentage:
            // backgroundWorker.ReportProgress(percentage);
            _autoCpp.InventoryMacros(backgroundWorker, _addinSettings.FolderPathCSourceCode);
            backgroundWorker.ReportProgress(100);
        }
        /// <summary>
        /// Completed capture macro task. Check if another request is pending and process is
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackgroundWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            // Handle completed
            //if (e.Cancelled)
            //{
            //    lblStatus.Text = "Process was cancelled";
            //}
            //else if (e.Error != null)
            //{
            //    lblStatus.Text = "There was an error running the process. The thread aborted";
            //}
            //else
            //{
            //    lblStatus.Text = "Process was completed";
            //}

            _autoCppIsRunning = false;
            if (_autoCppIsRequested)
            {
                _autoCppIsRequested = false;
                backgroundWorker.ReportProgress(10);
                backgroundWorker.RunWorkerAsync();
            }
            else _autoCppIsRunning = false;


        }
        // Report progress bar changed
        private void BackgroundWorker_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
            // Set the text.
            this.Text = e.ProgressPercentage.ToString();
        }

        /// <summary>
        /// Show functions of Source code
        /// - Functions
        /// - Macros which refer to functions
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowFunctionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!_addinSettings.IsFolderPathCSourceCode()) return;
            if (_autoCppIsRunning)
            {
                MessageBox.Show($@"The Code inventory is {progressBar1.Value}% finished", @"Code inventory not finished, retry!");

            }
            else
            {
                Cursor.Current = Cursors.WaitCursor;
                _autoCpp.ShowFunctions(_addinSettings.FolderPathCSourceCode);
                Cursor.Current = Cursors.Default;
            }
        }

        private void ShowSymbolDataBaseFoldersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            hoUtils.HoUtil.StartApp("explorer.exe", VcDbUtilities.GetVcPathSymbolDataBases());
        }

        private void AnalyzeCCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WikiRef.WikiAnalyzeC(); 
        }

        /// <summary>
        /// Get file of folder by file dialog. It return the file relative to the root folder in upper cases (like symbol database).
        /// </summary>
        /// <param name="rootFolder"></param>
        /// <returns></returns>
        private string GetFolderOrFile(string rootFolder)
        {
            var dlg = new OpenFileDialog
            {
                ValidateNames = false,
                CheckFileExists = false,
                CheckPathExists = true,
                FileName = "Folder Selection.",
                InitialDirectory = rootFolder,
                RestoreDirectory = true,
                Title = @"Open folder or *.c|*.cpp file to analyze",
                DefaultExt = ".c",
                Filter = @"C-files (*.c)|*.c|CPP-Files (*.cpp)|*.cpp|All files and folders|*"
            };
            if (dlg.ShowDialog() == DialogResult.OK) return dlg.FileName.ToUpper();
            return "";



        }
        /// <summary>
        /// Import *.csv from DOORS into current package as requirements
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void DoorsImportcsvToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EA.ObjectType type = _repository.GetContextItem(out var item);
            if (type != EA.ObjectType.otPackage) return;

            EA.Package pkg = (EA.Package) item;
            
            string filePath = @"c:\ho\ownCloud\shared\BLE_Sens_SWACommaSeperated.csv";
            if (!File.Exists(filePath))
            {
                MessageBox.Show($@"{filePath}", @"*.csv to import DOORS requirements doesn't exists");
                return;
            }

            EnableImportDialog(false);
            Cursor.Current = Cursors.WaitCursor;
            // Generate Requirements
            DoorsCsv doorsModule = new DoorsCsv(_repository, pkg, filePath);
            doorsModule.ImportUpdateRequirements("Requirement","","");
            EnableImportDialog(true);


            Cursor.Current = Cursors.Default;

        }
        /// <summary>
        /// Import *.csv file with requirements with a file dialog to select the *.csv file into the selected package. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void DoorsImportcsvWithFileDialogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EA.ObjectType type = _repository.GetContextItem(out var item);
            if (type != EA.ObjectType.otPackage) return;

            EA.Package pkg = (EA.Package) item;
            
            string filePath = @"c:\ho\ownCloud\shared\BLE_Sens_SWACommaSeperated.csv";
            OpenFileDialog theDialog =
                new OpenFileDialog
                {
                    Title = @"Open DOORS requirement *.csv File, comma separated",
                    Filter = @"CSV files|*.csv",
                    InitialDirectory = @"C:\"
                };
            if (theDialog.ShowDialog() != DialogResult.OK) return;

            filePath = theDialog.FileName;


            if (!File.Exists(filePath))
            {
                MessageBox.Show($@"{filePath}", @"*.csv to import DOORS requirements doesn't exists");
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            EnableImportDialog(false);
            // Generate Requirements
           DoorsCsv doorsModule = new DoorsCsv( _repository, pkg, filePath);
            doorsModule.ImportUpdateRequirements("Requirement","","");
            EnableImportDialog(true);
            Cursor.Current = Cursors.Default;

        }

        /// <summary>
        /// Check Requirements of selected package.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckDOORSRequirementsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EA.ObjectType type = _repository.GetContextItem(out var item);
            if (type != EA.ObjectType.otPackage) return;

            EA.Package pkg = (EA.Package) item;
            EaServices.Doors.DoorsModule doorsModule = new EaServices.Doors.DoorsModule(_repository, pkg);
            doorsModule.CheckRequirements();

        }
        

        /// <summary>
        /// Enable dialogs for Import functions
        /// </summary>
        /// <param name="isEnabled"></param>
        private void EnableImportDialog(bool isEnabled)
        {
            doorsImportcsvToolStripMenuItem.Enabled = isEnabled;
            doorsImportcsvWithFileDialogToolStripMenuItem.Enabled = isEnabled;
            checkDOORSRequirementsToolStripMenuItem.Enabled = isEnabled;
            importBySettingsToolStripMenuItem.Enabled = isEnabled;
        }
        /// <summary>
        /// Import according to Setting.json definitions
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ImportBySettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImportBySettings(1);

  
            MessageBox.Show(@"See File 1, settings for the import definitions.",@"Import DOORS *.csv Requirements finished.");
        }

        private async void ImportDoorsReqIFBySettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImportBySettings(2);
            MessageBox.Show(@"See File 2, settings for the import definitions.",@"Import DOORS *.reqIf Requirements finished.");
        }

        

        private async void ImportReqIFBySettings4ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImportBySettings(4);
            MessageBox.Show(@"See File 4, settings for the import definitions.",@"Import ReqIf *.reqIf Requirements finished.");
        }
        private async void ImportReqIFBySettings5ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImportBySettings(5);
            MessageBox.Show(@"See File 5, settings for the import definitions.",@"Import ReqIf *.reqIf Requirements finished.");
        }


        /// <summary>
        /// Import by settings
        /// </summary>
        /// <param name="listNumber"></param>
        /// <param name="withMessage"></param>
        private bool ImportBySettings(int listNumber, bool withMessage=false)
        {
            if (_repository == null || String.IsNullOrEmpty(_repository.ConnectionString))
            {
                MessageBox.Show("", @"No repository loaded, break!!");
                return false;
            }
            Cursor.Current = Cursors.WaitCursor;

            EnableImportDialog(false);
            DoorsModule doorsModule = new EaServices.Doors.DoorsModule(_jasonFilePath, _repository);
            bool result = doorsModule.ImportBySetting(listNumber);
            EnableImportDialog(true);
            Cursor.Current = Cursors.Default;
            if (withMessage && result)  MessageBox.Show(@"See Chapter: 'Importer' in Settings.Json (%APPDATA%ho/../Settings.json)", $@"Imported list={listNumber}, finished.");
            return result;
        }

        private void ImportReqIFBySettings3ToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
           ImportBySettings(3);
            MessageBox.Show(@"See File 3, settings for the import definitions.",@"Import ReqIf *.reqIf Requirements finished.");

        }

    }
}

