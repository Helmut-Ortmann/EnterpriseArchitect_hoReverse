using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using EaServices.Doors;
using EaServices.Doors.ReqIfs;
using hoReverse.Settings;
using hoReverse.HistoryList;
using hoReverse.Services;
using hoReverse.hoUtils;
using hoReverse.hoUtils.Cutils;
using hoReverse.hoUtils.Diagrams;
using hoReverse.Reverse.EaAddinShortcuts;
using hoReverse.hoUtils.WiKiRefs;
using hoLinqToSql.LinqUtils;

//using hoReverse.Services.AutoCpp;
using File = System.IO.File;
using EaServices.Doors.ReqIfs.Inventory;

using hoUtils.BulkChange;

//using MoreLinq;


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
        readonly string Tab = @"\t";

        private readonly string _version = "3.16.0"; 
        // The last MenuItem the mouse hovered upon.
        private ToolStripMenuItem _lastMenuItem;

        private EA.Repository _repository;
        private EaHistoryList _history;
        private EaHistoryList _bookmark;
        private AddinSettings _addinSettings;

        //private AutoCpp _autoCpp ;
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
        private ToolStripSeparator toolStripSeparator19;
        private ToolStripMenuItem sQLWildcardsToolStripMenuItem;
        private ToolStripMenuItem reqIFToolStripMenuItem;
        private ToolStripMenuItem generateIncludesFromCodeSnippetToolStripMenuItem;
        private ToolStripMenuItem _reqIfMenuItem;
        private ToolStripMenuItem InfoReqIfInquiryToolStripMenuItem;
        private ToolStripMenuItem InfoReqIfInquiryValidationToolStripMenuItem;
        private ToolStripMenuItem _diagramSearchToolStripMenuItem;
        private ToolStripMenuItem _simpleSearchToolStripMenuItem;
        private ToolStripMenuItem _recentlyModifiedDiagramsToolStripMenuItem;
        private ToolStripMenuItem _actionQMToolStripMenuItem;
        private ToolStripMenuItem _actionASILAToolStripMenuItem;
        private ToolStripMenuItem _actionASILBToolStripMenuItem;
        private ToolStripMenuItem _actionASILCToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator20;
        private ToolStripMenuItem moveToPackageToolStripMenuItem;
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
                
                    if (backgroundWorker.IsBusy) backgroundWorker.CancelAsync();
                    _autoCppIsRequested = false;

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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HoReverseGui));
            _btnLh = new Button();
            _btnLv = new Button();
            _btnTv = new Button();
            BtnTh = new Button();
            _btnOs = new Button();
            _toolTip = new ToolTip(components);
            _btnDisplayBehavior = new Button();
            _btnLocateOperation = new Button();
            _btnAddElementNote = new Button();
            _btnAddConstraint = new Button();
            _btnLocateType = new Button();
            _btnFindUsage = new Button();
            _btnDisplaySpecification = new Button();
            _btnComposite = new Button();
            _btnOr = new Button();
            _btnA = new Button();
            _btnD = new Button();
            _btnC = new Button();
            _btnUpdateActivityParameter = new Button();
            _btnBack = new Button();
            _btnFrwrd = new Button();
            _btnBookmarkAdd = new Button();
            _btnBookmarkRemove = new Button();
            _btnBookmarkBack = new Button();
            _btnBookmarkFrwrd = new Button();
            _btnInsert = new Button();
            _btnAction = new Button();
            _btnDecision = new Button();
            _btnMerge = new Button();
            _btnDecisionFromText = new Button();
            _btnBookmark = new Button();
            _btnHistory = new Button();
            _btnActivityCompositeFromText = new Button();
            _btnActivity = new Button();
            _btnNoteFromText = new Button();
            _btnFinal = new Button();
            _btnBezier = new Button();
            _contextMenuStripTextField = new ContextMenuStrip(components);
            _quickSearchToolStripMenuItem = new ToolStripMenuItem();
            _diagramSearchToolStripMenuItem = new ToolStripMenuItem();
            _simpleSearchToolStripMenuItem = new ToolStripMenuItem();
            _recentlyModifiedDiagramsToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            _actionQMToolStripMenuItem = new ToolStripMenuItem();
            _actionASILAToolStripMenuItem = new ToolStripMenuItem();
            _actionASILBToolStripMenuItem = new ToolStripMenuItem();
            _actionASILCToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator20 = new ToolStripSeparator();
            endIfMacroToolStripMenuItem = new ToolStripMenuItem();
            externToolStripMenuItem = new ToolStripMenuItem();
            _deleteToolStripMenuItemTextField = new ToolStripMenuItem();
            _insertBeneathNodeToolStripMenuItem = new ToolStripMenuItem();
            _addActivityToolStripMenuItem = new ToolStripMenuItem();
            _addCompositeActivityToolStripMenuItem = new ToolStripMenuItem();
            _addFinalToolStripMenuItem = new ToolStripMenuItem();
            _addMergeToolStripMenuItem = new ToolStripMenuItem();
            _showAllPortsToolStripMenuItem = new ToolStripMenuItem();
            _insertTextIntoNodeToolStripMenuItem = new ToolStripMenuItem();
            _btnNoMerge = new Button();
            _btnSplitNodes = new Button();
            _btnSplitAll = new Button();
            _btnWriteText = new Button();
            _btnGuardNo = new Button();
            _btnGuardYes = new Button();
            _btnGuardSpace = new Button();
            _btnFeatureUp = new Button();
            _btnFeatureDown = new Button();
            _btnAddNoteAndLink = new Button();
            _btnCopy = new Button();
            progressBar1 = new ProgressBar();
            _txtUserText = new EnterTextBox();
            _menuStrip1 = new MenuStrip();
            _fileToolStripMenuItem = new ToolStripMenuItem();
            _saveToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator7 = new ToolStripSeparator();
            _settingsToolStripMenuItem = new ToolStripMenuItem();
            _setting2ConnectorToolStripMenuItem = new ToolStripMenuItem();
            settingsDiagramStylesToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator5 = new ToolStripSeparator();
            reloadSettingsToolStripMenuItem = new ToolStripMenuItem();
            resetFactorySettingsToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator9 = new ToolStripSeparator();
            _clearToolStripMenuItem = new ToolStripMenuItem();
            _doToolStripMenuItem = new ToolStripMenuItem();
            _createActivityForOperationToolStripMenuItem = new ToolStripMenuItem();
            _updateMethodParametersToolStripMenuItem = new ToolStripMenuItem();
            _toolStripSeparator3 = new ToolStripSeparator();
            _showFolderToolStripMenuItem = new ToolStripMenuItem();
            setFolderToolStripMenuItem = new ToolStripMenuItem();
            _toolStripSeparator4 = new ToolStripSeparator();
            _copyGuidSqlToClipboardToolStripMenuItem = new ToolStripMenuItem();
            moveToPackageToolStripMenuItem = new ToolStripMenuItem();
            _createSharedMemoryToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator4 = new ToolStripSeparator();
            standardDiagramToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator8 = new ToolStripSeparator();
            moveUsageToElementToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator6 = new ToolStripSeparator();
            sortAlphabeticToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator11 = new ToolStripSeparator();
            _codeToolStripMenuItem = new ToolStripMenuItem();
            _insertAttributeToolStripMenuItem = new ToolStripMenuItem();
            _insertTypedefStructToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator10 = new ToolStripSeparator();
            _insertFunctionToolStripMenuItem = new ToolStripMenuItem();
            insertFunctionMakeDuplicatesToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator3 = new ToolStripSeparator();
            _updateActionToolStripMenuItem = new ToolStripMenuItem();
            _toolStripSeparator6 = new ToolStripSeparator();
            _deleteInvisibleuseRealizationDependenciesToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            _generateComponentPortsToolStripMenuItem = new ToolStripMenuItem();
            _hideAllPortsToolStripMenuItem = new ToolStripMenuItem();
            _showAllPortsActivityParametersToolStripMenuItem = new ToolStripMenuItem();
            _toolStripSeparator7 = new ToolStripSeparator();
            _inserToolStripMenuItem = new ToolStripMenuItem();
            generateIncludesFromCodeSnippetToolStripMenuItem = new ToolStripMenuItem();
            _toolStripSeparator8 = new ToolStripSeparator();
            _setMacroToolStripMenuItem = new ToolStripMenuItem();
            _addMacroToolStripMenuItem = new ToolStripMenuItem();
            _delMacroToolStripMenuItem = new ToolStripMenuItem();
            _toolStripSeparator = new ToolStripSeparator();
            _copyReleaseInformationToClipboardToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator16 = new ToolStripSeparator();
            _reqIfMenuItem = new ToolStripMenuItem();
            InfoReqIfInquiryToolStripMenuItem = new ToolStripMenuItem();
            InfoReqIfInquiryValidationToolStripMenuItem = new ToolStripMenuItem();
            _autoToolStripMenuItem = new ToolStripMenuItem();
            modulesToolStripMenuItem = new ToolStripMenuItem();
            inventoryToolStripMenuItem = new ToolStripMenuItem();
            _getToolStripMenuItem = new ToolStripMenuItem();
            makeRunnableToolStripMenuItem = new ToolStripMenuItem();
            makeServicePortToolStripMenuItem = new ToolStripMenuItem();
            makeCalloutToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator12 = new ToolStripSeparator();
            showExternalComponentFunctionsToolStripMenuItem = new ToolStripMenuItem();
            showProvidedRequiredFunctionsForSourceToolStripMenuItem = new ToolStripMenuItem();
            showFunctionsToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator13 = new ToolStripSeparator();
            showSymbolDataBaseFoldersToolStripMenuItem = new ToolStripMenuItem();
            _versionControlToolStripMenuItem = new ToolStripMenuItem();
            _svnLogToolStripMenuItem = new ToolStripMenuItem();
            _svnTortoiseRepobrowserToolStripMenuItem = new ToolStripMenuItem();
            _showDirectoryToolStripMenuItem = new ToolStripMenuItem();
            _toolStripSeparator1 = new ToolStripSeparator();
            _getVcLatestrecursiveToolStripMenuItem = new ToolStripMenuItem();
            _setSvnKeywordsToolStripMenuItem = new ToolStripMenuItem();
            _setSvnTaggedValuesToolStripMenuItem1 = new ToolStripMenuItem();
            _setSvnTaggedValuesToolStripMenuItem = new ToolStripMenuItem();
            _toolStripSeparator2 = new ToolStripSeparator();
            _changeXmlPathToolStripMenuItem1 = new ToolStripMenuItem();
            _toolStripSeparator5 = new ToolStripSeparator();
            _maintenanceToolStripMenuItem = new ToolStripMenuItem();
            _vCGetStateToolStripMenuItem = new ToolStripMenuItem();
            _vCResyncToolStripMenuItem = new ToolStripMenuItem();
            _vCxmiReconsileToolStripMenuItem = new ToolStripMenuItem();
            _vCRemoveToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator17 = new ToolStripSeparator();
            doorsImportcsvToolStripMenuItem = new ToolStripMenuItem();
            doorsImportcsvWithFileDialogToolStripMenuItem = new ToolStripMenuItem();
            checkDOORSRequirementsToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator18 = new ToolStripSeparator();
            importBySettingsToolStripMenuItem = new ToolStripMenuItem();
            importDoorsReqIFBySettingsToolStripMenuItem = new ToolStripMenuItem();
            importReqIFBySettings3ToolStripMenuItem = new ToolStripMenuItem();
            importReqIFBySettingsToolStripMenuItem = new ToolStripMenuItem();
            importReqIFBySettings5ToolStripMenuItem = new ToolStripMenuItem();
            _helpToolStripMenuItem = new ToolStripMenuItem();
            _aboutToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator15 = new ToolStripSeparator();
            _helpF1ToolStripMenuItem = new ToolStripMenuItem();
            readmeToolStripMenuItem = new ToolStripMenuItem();
            repoToolStripMenuItem = new ToolStripMenuItem();
            hoToolsToolStripMenuItem = new ToolStripMenuItem();
            lineStyleToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator19 = new ToolStripSeparator();
            sQLWildcardsToolStripMenuItem = new ToolStripMenuItem();
            reqIFToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator14 = new ToolStripSeparator();
            analyzeCCToolStripMenuItem = new ToolStripMenuItem();
            helpToolStripMenuItem = new ToolStripMenuItem();
            _toolStripContainer1 = new ToolStripContainer();
            _toolStrip6 = new ToolStrip();
            _toolStripBtn11 = new ToolStripButton();
            _toolStripBtn12 = new ToolStripButton();
            _toolStripBtn13 = new ToolStripButton();
            _toolStripBtn14 = new ToolStripButton();
            _toolStripBtn15 = new ToolStripButton();
            _toolStrip1 = new ToolStrip();
            _toolStripBtn1 = new ToolStripButton();
            _toolStripBtn2 = new ToolStripButton();
            _toolStripBtn3 = new ToolStripButton();
            _toolStripBtn4 = new ToolStripButton();
            _toolStripBtn5 = new ToolStripButton();
            _toolTip1 = new ToolTip(components);
            backgroundWorker = new System.ComponentModel.BackgroundWorker();
            _contextMenuStripTextField.SuspendLayout();
            _menuStrip1.SuspendLayout();
            _toolStripContainer1.TopToolStripPanel.SuspendLayout();
            _toolStripContainer1.SuspendLayout();
            _toolStrip6.SuspendLayout();
            _toolStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // _btnLh
            // 
            _btnLh.Location = new Point(125, 420);
            _btnLh.Margin = new Padding(8);
            _btnLh.Name = "_btnLh";
            _btnLh.Size = new Size(108, 65);
            _btnLh.TabIndex = 0;
            _btnLh.Text = "LH";
            _toolTip.SetToolTip(_btnLh, "Lateral Horizontal");
            _btnLh.UseVisualStyleBackColor = true;
            _btnLh.Click += new EventHandler(BtnLH_Click);
            // 
            // _btnLv
            // 
            _btnLv.Location = new Point(5, 420);
            _btnLv.Margin = new Padding(8);
            _btnLv.Name = "_btnLv";
            _btnLv.Size = new Size(112, 65);
            _btnLv.TabIndex = 2;
            _btnLv.Text = "LV";
            _toolTip.SetToolTip(_btnLv, "Lateral Vertical");
            _btnLv.UseVisualStyleBackColor = true;
            _btnLv.Click += new EventHandler(BtnLV_Click);
            // 
            // _btnTv
            // 
            _btnTv.Location = new Point(248, 420);
            _btnTv.Margin = new Padding(8);
            _btnTv.Name = "_btnTv";
            _btnTv.Size = new Size(88, 65);
            _btnTv.TabIndex = 4;
            _btnTv.Text = "TV";
            _toolTip.SetToolTip(_btnTv, "Tree Vertical");
            _btnTv.UseVisualStyleBackColor = true;
            _btnTv.Click += new EventHandler(BtnTV_Click);
            // 
            // BtnTh
            // 
            BtnTh.Location = new Point(358, 420);
            BtnTh.Margin = new Padding(8);
            BtnTh.Name = "BtnTh";
            BtnTh.Size = new Size(102, 65);
            BtnTh.TabIndex = 3;
            BtnTh.Text = "TH";
            _toolTip.SetToolTip(BtnTh, "Tree Horizontal");
            BtnTh.UseVisualStyleBackColor = true;
            BtnTh.Click += new EventHandler(BtnTH_Click);
            // 
            // _btnOs
            // 
            _btnOs.Location = new Point(5, 348);
            _btnOs.Margin = new Padding(8);
            _btnOs.Name = "_btnOs";
            _btnOs.Size = new Size(78, 65);
            _btnOs.TabIndex = 5;
            _btnOs.Text = "OS";
            _toolTip.SetToolTip(_btnOs, "Orthogonal Square");
            _btnOs.UseVisualStyleBackColor = true;
            _btnOs.Click += new EventHandler(BtnOS_Click);
            // 
            // _btnDisplayBehavior
            // 
            _btnDisplayBehavior.Location = new Point(5, 512);
            _btnDisplayBehavior.Margin = new Padding(8);
            _btnDisplayBehavior.Name = "_btnDisplayBehavior";
            _btnDisplayBehavior.Size = new Size(330, 65);
            _btnDisplayBehavior.TabIndex = 7;
            _btnDisplayBehavior.Text = "DisplayBehavior";
            _toolTip.SetToolTip(_btnDisplayBehavior, "Display behavior of an operation (activity, statemachine, interaction)");
            _btnDisplayBehavior.UseVisualStyleBackColor = true;
            _btnDisplayBehavior.Click += new EventHandler(BtnDisplayBehavior_Click);
            // 
            // _btnLocateOperation
            // 
            _btnLocateOperation.Location = new Point(5, 588);
            _btnLocateOperation.Margin = new Padding(8);
            _btnLocateOperation.Name = "_btnLocateOperation";
            _btnLocateOperation.Size = new Size(330, 65);
            _btnLocateOperation.TabIndex = 8;
            _btnLocateOperation.Text = "Locate Operation";
            _toolTip.SetToolTip(_btnLocateOperation, "Locate the linked operation for a behavior (statechart, activity, interaction)");
            _btnLocateOperation.UseVisualStyleBackColor = true;
            _btnLocateOperation.Click += new EventHandler(BtnLocateOperation_Click);
            // 
            // _btnAddElementNote
            // 
            _btnAddElementNote.Location = new Point(202, 768);
            _btnAddElementNote.Margin = new Padding(8);
            _btnAddElementNote.Name = "_btnAddElementNote";
            _btnAddElementNote.Size = new Size(155, 65);
            _btnAddElementNote.TabIndex = 9;
            _btnAddElementNote.Text = "Note";
            _toolTip.SetToolTip(_btnAddElementNote, "Add Note to selected: \r\n- Elements\r\n- Connector\r\n- Diagram if nothing is selected" +
        "\r\n\r\nIt takes the text from the input field.\r\n\r\nThe note is free editable.");
            _btnAddElementNote.UseVisualStyleBackColor = true;
            _btnAddElementNote.Click += new EventHandler(_btnAddElementNote_Click);
            // 
            // _btnAddConstraint
            // 
            _btnAddConstraint.Location = new Point(372, 768);
            _btnAddConstraint.Margin = new Padding(8);
            _btnAddConstraint.Name = "_btnAddConstraint";
            _btnAddConstraint.Size = new Size(245, 65);
            _btnAddConstraint.TabIndex = 10;
            _btnAddConstraint.Text = "Constraint";
            _toolTip.SetToolTip(_btnAddConstraint, "Add Constraint to selected: \r\n- Elements\r\n- Connector\r\n- Diagram if nothing selec" +
        "ted\r\n\r\nIt takes the text of the input field.\r\nThe constraint is free editable.");
            _btnAddConstraint.UseVisualStyleBackColor = true;
            _btnAddConstraint.Click += new EventHandler(_btnAddConstraint_Click);
            // 
            // _btnLocateType
            // 
            _btnLocateType.Location = new Point(5, 660);
            _btnLocateType.Margin = new Padding(8);
            _btnLocateType.Name = "_btnLocateType";
            _btnLocateType.Size = new Size(330, 65);
            _btnLocateType.TabIndex = 11;
            _btnLocateType.Text = @"Locate Type";
            _toolTip.SetToolTip(_btnLocateType, "Locate to the type of the selected element");
            _btnLocateType.UseVisualStyleBackColor = true;
            _btnLocateType.Click += new EventHandler(BtnLocateType_Click);
            // 
            // _btnFindUsage
            // 
            _btnFindUsage.Location = new Point(358, 588);
            _btnFindUsage.Margin = new Padding(8);
            _btnFindUsage.Name = "_btnFindUsage";
            _btnFindUsage.Size = new Size(245, 65);
            _btnFindUsage.TabIndex = 12;
            _btnFindUsage.Text = @"Find Usage";
            _toolTip.SetToolTip(_btnFindUsage, "Find the usage of the selected element");
            _btnFindUsage.UseVisualStyleBackColor = true;
            _btnFindUsage.Click += new EventHandler(BtnFindUsage_Click);
            // 
            // _btnDisplaySpecification
            // 
            _btnDisplaySpecification.Location = new Point(358, 512);
            _btnDisplaySpecification.Margin = new Padding(8);
            _btnDisplaySpecification.Name = "_btnDisplaySpecification";
            _btnDisplaySpecification.Size = new Size(245, 65);
            _btnDisplaySpecification.TabIndex = 13;
            _btnDisplaySpecification.Text = @"Specification";
            _toolTip.SetToolTip(_btnDisplaySpecification, "Display the Specification according to file property");
            _btnDisplaySpecification.UseVisualStyleBackColor = true;
            _btnDisplaySpecification.Click += new EventHandler(BtnShowSpecification_Click);
            // 
            // _btnComposite
            // 
            _btnComposite.Location = new Point(358, 660);
            _btnComposite.Margin = new Padding(8);
            _btnComposite.Name = "_btnComposite";
            _btnComposite.Size = new Size(245, 65);
            _btnComposite.TabIndex = 16;
            _btnComposite.Text = "Composite";
            _toolTip.SetToolTip(_btnComposite, "Navigate between Element and Composite Diagram");
            _btnComposite.UseVisualStyleBackColor = true;
            _btnComposite.Click += new EventHandler(BtnComposite_Click);
            // 
            // _btnOr
            // 
            _btnOr.Location = new Point(108, 348);
            _btnOr.Margin = new Padding(8);
            _btnOr.Name = "_btnOr";
            _btnOr.Size = new Size(78, 65);
            _btnOr.TabIndex = 17;
            _btnOr.Text = "OR";
            _toolTip.SetToolTip(_btnOr, "Orthogonal Rounded");
            _btnOr.UseVisualStyleBackColor = true;
            _btnOr.Click += new EventHandler(BtnOR_Click);
            // 
            // _btnA
            // 
            _btnA.Location = new Point(695, 420);
            _btnA.Margin = new Padding(8);
            _btnA.Name = "_btnA";
            _btnA.Size = new Size(95, 65);
            _btnA.TabIndex = 18;
            _btnA.Text = "A";
            _toolTip.SetToolTip(_btnA, "Orthogonal Rounded");
            _btnA.UseVisualStyleBackColor = true;
            _btnA.Click += new EventHandler(BtnA_Click);
            // 
            // _btnD
            // 
            _btnD.Location = new Point(585, 420);
            _btnD.Margin = new Padding(8);
            _btnD.Name = "_btnD";
            _btnD.Size = new Size(95, 65);
            _btnD.TabIndex = 19;
            _btnD.Text = "D";
            _toolTip.SetToolTip(_btnD, "Direct");
            _btnD.UseVisualStyleBackColor = true;
            _btnD.Click += new EventHandler(BtnD_Click);
            // 
            // _btnC
            // 
            _btnC.Location = new Point(475, 420);
            _btnC.Margin = new Padding(8);
            _btnC.Name = "_btnC";
            _btnC.Size = new Size(95, 65);
            _btnC.TabIndex = 20;
            _btnC.Text = "C";
            _toolTip.SetToolTip(_btnC, "Custom line");
            _btnC.UseVisualStyleBackColor = true;
            _btnC.Click += new EventHandler(BtnC_Click);
            // 
            // _btnUpdateActivityParameter
            // 
            _btnUpdateActivityParameter.Location = new Point(8, 852);
            _btnUpdateActivityParameter.Margin = new Padding(8);
            _btnUpdateActivityParameter.Name = "_btnUpdateActivityParameter";
            _btnUpdateActivityParameter.Size = new Size(268, 65);
            _btnUpdateActivityParameter.TabIndex = 22;
            _btnUpdateActivityParameter.Text = "Update Parameter";
            _toolTip.SetToolTip(_btnUpdateActivityParameter, "Update Operation and Activity Parameter from operation");
            _btnUpdateActivityParameter.UseVisualStyleBackColor = true;
            _btnUpdateActivityParameter.Click += new EventHandler(BtnUpdateActivityParameter_Click);
            // 
            // _btnBack
            // 
            _btnBack.Location = new Point(375, 995);
            _btnBack.Margin = new Padding(8);
            _btnBack.Name = "_btnBack";
            _btnBack.Size = new Size(50, 65);
            _btnBack.TabIndex = 23;
            _btnBack.Text = "<";
            _toolTip.SetToolTip(_btnBack, "Diagram back");
            _btnBack.UseVisualStyleBackColor = true;
            _btnBack.Click += new EventHandler(BtnBack_Click);
            // 
            // _btnFrwrd
            // 
            _btnFrwrd.Location = new Point(442, 995);
            _btnFrwrd.Margin = new Padding(8);
            _btnFrwrd.Name = "_btnFrwrd";
            _btnFrwrd.Size = new Size(52, 65);
            _btnFrwrd.TabIndex = 24;
            _btnFrwrd.Text = ">";
            _toolTip.SetToolTip(_btnFrwrd, "Diagram forward");
            _btnFrwrd.UseVisualStyleBackColor = true;
            _btnFrwrd.Click += new EventHandler(BtnFrwrd_Click);
            // 
            // _btnBookmarkAdd
            // 
            _btnBookmarkAdd.Location = new Point(222, 932);
            _btnBookmarkAdd.Margin = new Padding(8);
            _btnBookmarkAdd.Name = "_btnBookmarkAdd";
            _btnBookmarkAdd.Size = new Size(52, 65);
            _btnBookmarkAdd.TabIndex = 27;
            _btnBookmarkAdd.Text = "+";
            _toolTip.SetToolTip(_btnBookmarkAdd, "Add bookmark");
            _btnBookmarkAdd.UseVisualStyleBackColor = true;
            _btnBookmarkAdd.Click += new EventHandler(BtnBookmarkAdd_Click);
            // 
            // _btnBookmarkRemove
            // 
            _btnBookmarkRemove.Location = new Point(295, 932);
            _btnBookmarkRemove.Margin = new Padding(8);
            _btnBookmarkRemove.Name = "_btnBookmarkRemove";
            _btnBookmarkRemove.Size = new Size(52, 65);
            _btnBookmarkRemove.TabIndex = 28;
            _btnBookmarkRemove.Text = "-";
            _toolTip.SetToolTip(_btnBookmarkRemove, "Remove bookmark");
            _btnBookmarkRemove.UseVisualStyleBackColor = true;
            _btnBookmarkRemove.Click += new EventHandler(BtnBookmarkRemove_Click);
            // 
            // _btnBookmarkBack
            // 
            _btnBookmarkBack.Location = new Point(375, 932);
            _btnBookmarkBack.Margin = new Padding(8);
            _btnBookmarkBack.Name = "_btnBookmarkBack";
            _btnBookmarkBack.Size = new Size(50, 65);
            _btnBookmarkBack.TabIndex = 29;
            _btnBookmarkBack.Text = "<";
            _toolTip.SetToolTip(_btnBookmarkBack, "Back in bookmark history");
            _btnBookmarkBack.UseVisualStyleBackColor = true;
            _btnBookmarkBack.Click += new EventHandler(BtnBookmarkBack_Click);
            // 
            // _btnBookmarkFrwrd
            // 
            _btnBookmarkFrwrd.Location = new Point(440, 932);
            _btnBookmarkFrwrd.Margin = new Padding(8);
            _btnBookmarkFrwrd.Name = "_btnBookmarkFrwrd";
            _btnBookmarkFrwrd.Size = new Size(52, 65);
            _btnBookmarkFrwrd.TabIndex = 30;
            _btnBookmarkFrwrd.Text = ">";
            _toolTip.SetToolTip(_btnBookmarkFrwrd, "Forward in bookmark history");
            _btnBookmarkFrwrd.UseVisualStyleBackColor = true;
            _btnBookmarkFrwrd.Click += new EventHandler(BtnBookmarkFrwrd_Click);
            // 
            // _btnInsert
            // 
            _btnInsert.Location = new Point(212, 130);
            _btnInsert.Margin = new Padding(8);
            _btnInsert.Name = "_btnInsert";
            _btnInsert.Size = new Size(58, 65);
            _btnInsert.TabIndex = 37;
            _btnInsert.Text = "I";
            _toolTip.SetToolTip(_btnInsert, resources.GetString("_btnInsert.ToolTip"));
            _btnInsert.UseVisualStyleBackColor = true;
            _btnInsert.Click += new EventHandler(BtnInsert_Click);
            // 
            // _btnAction
            // 
            _btnAction.Location = new Point(922, 682);
            _btnAction.Margin = new Padding(8);
            _btnAction.Name = "_btnAction";
            _btnAction.Size = new Size(58, 62);
            _btnAction.TabIndex = 35;
            _btnAction.Text = "A";
            _toolTip.SetToolTip(_btnAction, "Create an action beneath selected object");
            _btnAction.UseVisualStyleBackColor = true;
            _btnAction.Visible = false;
            // 
            // _btnDecision
            // 
            _btnDecision.Location = new Point(922, 798);
            _btnDecision.Margin = new Padding(8);
            _btnDecision.Name = "_btnDecision";
            _btnDecision.Size = new Size(58, 62);
            _btnDecision.TabIndex = 36;
            _btnDecision.Text = "D";
            _toolTip.SetToolTip(_btnDecision, "Create a decision beneath selected object");
            _btnDecision.UseVisualStyleBackColor = true;
            _btnDecision.Visible = false;
            // 
            // _btnMerge
            // 
            _btnMerge.Location = new Point(135, 200);
            _btnMerge.Margin = new Padding(8);
            _btnMerge.Name = "_btnMerge";
            _btnMerge.Size = new Size(58, 62);
            _btnMerge.TabIndex = 38;
            _btnMerge.Text = "M";
            _toolTip.SetToolTip(_btnMerge, "Create a Merge beneath selected object");
            _btnMerge.UseVisualStyleBackColor = true;
            _btnMerge.Click += new EventHandler(BtnMerge_Click);
            // 
            // _btnDecisionFromText
            // 
            _btnDecisionFromText.Location = new Point(922, 872);
            _btnDecisionFromText.Margin = new Padding(8);
            _btnDecisionFromText.Name = "_btnDecisionFromText";
            _btnDecisionFromText.Size = new Size(58, 62);
            _btnDecisionFromText.TabIndex = 39;
            _btnDecisionFromText.Text = "D";
            _toolTip.SetToolTip(_btnDecisionFromText, "Create Decision with text beneath selected element");
            _btnDecisionFromText.UseVisualStyleBackColor = true;
            _btnDecisionFromText.Visible = false;
            // 
            // _btnBookmark
            // 
            _btnBookmark.Location = new Point(5, 932);
            _btnBookmark.Margin = new Padding(8);
            _btnBookmark.Name = "_btnBookmark";
            _btnBookmark.Size = new Size(198, 65);
            _btnBookmark.TabIndex = 34;
            _btnBookmark.Text = "Bookmarks:";
            _toolTip.SetToolTip(_btnBookmark, "Show bookmarks");
            _btnBookmark.UseVisualStyleBackColor = true;
            _btnBookmark.Click += new EventHandler(BtnBookmarks_Click);
            // 
            // _btnHistory
            // 
            _btnHistory.Location = new Point(5, 1008);
            _btnHistory.Margin = new Padding(8);
            _btnHistory.Name = "_btnHistory";
            _btnHistory.Size = new Size(198, 65);
            _btnHistory.TabIndex = 42;
            _btnHistory.Text = "History:";
            _toolTip.SetToolTip(_btnHistory, "Show history");
            _btnHistory.UseVisualStyleBackColor = true;
            _btnHistory.Click += new EventHandler(BtnHistory_Click);
            // 
            // _btnActivityCompositeFromText
            // 
            _btnActivityCompositeFromText.Location = new Point(8, 130);
            _btnActivityCompositeFromText.Margin = new Padding(8);
            _btnActivityCompositeFromText.Name = "_btnActivityCompositeFromText";
            _btnActivityCompositeFromText.Size = new Size(118, 65);
            _btnActivityCompositeFromText.TabIndex = 43;
            _btnActivityCompositeFromText.Text = "ActC";
            _toolTip.SetToolTip(_btnActivityCompositeFromText, resources.GetString("_btnActivityCompositeFromText.ToolTip"));
            _btnActivityCompositeFromText.UseVisualStyleBackColor = true;
            _btnActivityCompositeFromText.Click += new EventHandler(BtnActivityCompositeFromText_Click);
            // 
            // _btnActivity
            // 
            _btnActivity.Location = new Point(8, 200);
            _btnActivity.Margin = new Padding(8);
            _btnActivity.Name = "_btnActivity";
            _btnActivity.Size = new Size(118, 62);
            _btnActivity.TabIndex = 44;
            _btnActivity.Text = "Act";
            _toolTip.SetToolTip(_btnActivity, "Create an Activity beneath selected object.\r\n\r\nThis is useful for e.g. FOR or WHI" +
        "LE loop");
            _btnActivity.UseVisualStyleBackColor = true;
            _btnActivity.Click += new EventHandler(BtnActivityFromText_Click);
            // 
            // _btnNoteFromText
            // 
            _btnNoteFromText.Location = new Point(618, 512);
            _btnNoteFromText.Margin = new Padding(8);
            _btnNoteFromText.Name = "_btnNoteFromText";
            _btnNoteFromText.Size = new Size(172, 62);
            _btnNoteFromText.TabIndex = 45;
            _btnNoteFromText.Text = "N";
            _toolTip.SetToolTip(_btnNoteFromText, "Insert text into Element Note.\r\n\r\nIt remove \"//\", \'/*\' and \'*/\'");
            _btnNoteFromText.UseVisualStyleBackColor = true;
            _btnNoteFromText.Visible = false;
            _btnNoteFromText.Click += new EventHandler(BtnNoteFromText_Click);
            // 
            // _btnFinal
            // 
            _btnFinal.Location = new Point(140, 130);
            _btnFinal.Margin = new Padding(8);
            _btnFinal.Name = "_btnFinal";
            _btnFinal.Size = new Size(58, 65);
            _btnFinal.TabIndex = 46;
            _btnFinal.Text = "F";
            _toolTip.SetToolTip(_btnFinal, resources.GetString("_btnFinal.ToolTip"));
            _btnFinal.UseVisualStyleBackColor = true;
            _btnFinal.Click += new EventHandler(BtnFinal_Click);
            // 
            // _btnBezier
            // 
            _btnBezier.Location = new Point(202, 348);
            _btnBezier.Margin = new Padding(8);
            _btnBezier.Name = "_btnBezier";
            _btnBezier.Size = new Size(40, 65);
            _btnBezier.TabIndex = 48;
            _btnBezier.Text = "B";
            _toolTip.SetToolTip(_btnBezier, "Linestyle Bezier");
            _btnBezier.UseVisualStyleBackColor = true;
            _btnBezier.Click += new EventHandler(BtnBezier_Click);
            // 
            // _contextMenuStripTextField
            // 
            _contextMenuStripTextField.ImageScalingSize = new Size(20, 20);
            _contextMenuStripTextField.Items.AddRange(new ToolStripItem[] {
            _quickSearchToolStripMenuItem,
            _diagramSearchToolStripMenuItem,
            _simpleSearchToolStripMenuItem,
            _recentlyModifiedDiagramsToolStripMenuItem,
            toolStripSeparator2,
            _actionQMToolStripMenuItem,
            _actionASILAToolStripMenuItem,
            _actionASILBToolStripMenuItem,
            _actionASILCToolStripMenuItem,
            toolStripSeparator20,
            endIfMacroToolStripMenuItem,
            externToolStripMenuItem,
            _deleteToolStripMenuItemTextField,
            _insertBeneathNodeToolStripMenuItem,
            _addActivityToolStripMenuItem,
            _addCompositeActivityToolStripMenuItem,
            _addFinalToolStripMenuItem,
            _addMergeToolStripMenuItem,
            _showAllPortsToolStripMenuItem,
            _insertTextIntoNodeToolStripMenuItem});
            _contextMenuStripTextField.Name = "_contextMenuStripTextField";
            _contextMenuStripTextField.Size = new Size(491, 880);
            _toolTip.SetToolTip(_contextMenuStripTextField, "Show all ports of selected classifier");
            // 
            // _quickSearchToolStripMenuItem
            // 
            _quickSearchToolStripMenuItem.Name = "_quickSearchToolStripMenuItem";
            _quickSearchToolStripMenuItem.Size = new Size(490, 48);
            _quickSearchToolStripMenuItem.Text = "&Quick Search";
            _quickSearchToolStripMenuItem.ToolTipText = "Call the quick search.";
            _quickSearchToolStripMenuItem.Click += new EventHandler(QuickSearchToolStripMenuItem_Click);
            // 
            // _diagramSearchToolStripMenuItem
            // 
            _diagramSearchToolStripMenuItem.Name = "_diagramSearchToolStripMenuItem";
            _diagramSearchToolStripMenuItem.Size = new Size(490, 48);
            _diagramSearchToolStripMenuItem.Text = "Diagram Search";
            _diagramSearchToolStripMenuItem.Click += new EventHandler(DiagramSearchToolStripMenuItem_Click);
            // 
            // _simpleSearchToolStripMenuItem
            // 
            _simpleSearchToolStripMenuItem.Name = "_simpleSearchToolStripMenuItem";
            _simpleSearchToolStripMenuItem.Size = new Size(490, 48);
            _simpleSearchToolStripMenuItem.Text = "Simple Search";
            _simpleSearchToolStripMenuItem.Click += new EventHandler(SimpleSearchToolStripMenuItem_Click);
            // 
            // _recentlyModifiedDiagramsToolStripMenuItem
            // 
            _recentlyModifiedDiagramsToolStripMenuItem.Name = "_recentlyModifiedDiagramsToolStripMenuItem";
            _recentlyModifiedDiagramsToolStripMenuItem.Size = new Size(490, 48);
            _recentlyModifiedDiagramsToolStripMenuItem.Text = "RecentlyModifiedDiagrams";
            _recentlyModifiedDiagramsToolStripMenuItem.Click += new EventHandler(RecentlyModifedDiagramsToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(487, 6);
            // 
            // _actionQMToolStripMenuItem
            // 
            _actionQMToolStripMenuItem.Name = "_actionQMToolStripMenuItem";
            _actionQMToolStripMenuItem.Size = new Size(490, 48);
            _actionQMToolStripMenuItem.Text = "QM";
            _actionQMToolStripMenuItem.Click += new EventHandler(QmMenuItem_Click);
            // 
            // _actionASILAToolStripMenuItem
            // 
            _actionASILAToolStripMenuItem.Name = "_actionASILAToolStripMenuItem";
            _actionASILAToolStripMenuItem.Size = new Size(490, 48);
            _actionASILAToolStripMenuItem.Text = "ASIL-A";
            _actionASILAToolStripMenuItem.Click += new EventHandler(AsilAMenuItem_Click);
            // 
            // _actionASILBToolStripMenuItem
            // 
            _actionASILBToolStripMenuItem.Name = "_actionASILBToolStripMenuItem";
            _actionASILBToolStripMenuItem.Size = new Size(490, 48);
            _actionASILBToolStripMenuItem.Text = "ASIL-B";
            _actionASILBToolStripMenuItem.Click += new EventHandler(AsilBMenuItem_Click);
            // 
            // _actionASILCToolStripMenuItem
            // 
            _actionASILCToolStripMenuItem.Name = "_actionASILCToolStripMenuItem";
            _actionASILCToolStripMenuItem.Size = new Size(490, 48);
            _actionASILCToolStripMenuItem.Text = "ASIL-C";
            _actionASILCToolStripMenuItem.Click += new EventHandler(AsilCMenuItem_Click);
            // 
            // toolStripSeparator20
            // 
            toolStripSeparator20.Name = "toolStripSeparator20";
            toolStripSeparator20.Size = new Size(487, 6);
            // 
            // endIfMacroToolStripMenuItem
            // 
            endIfMacroToolStripMenuItem.Name = "endIfMacroToolStripMenuItem";
            endIfMacroToolStripMenuItem.Size = new Size(490, 48);
            endIfMacroToolStripMenuItem.Text = "#endif";
            endIfMacroToolStripMenuItem.ToolTipText = "Write #endif into selected element";
            endIfMacroToolStripMenuItem.Click += new EventHandler(EndifMacroToolStripMenuItem_Click);
            // 
            // externToolStripMenuItem
            // 
            externToolStripMenuItem.Name = "externToolStripMenuItem";
            externToolStripMenuItem.Size = new Size(490, 48);
            externToolStripMenuItem.Text = "extern Function/Variable";
            externToolStripMenuItem.ToolTipText = "Set the stereotype <<extern>> for function/variable";
            externToolStripMenuItem.Click += new EventHandler(ExternToolStripMenuItem_Click);
            // 
            // _deleteToolStripMenuItemTextField
            // 
            _deleteToolStripMenuItemTextField.Name = "_deleteToolStripMenuItemTextField";
            _deleteToolStripMenuItemTextField.Size = new Size(490, 48);
            _deleteToolStripMenuItemTextField.Text = "&Delete";
            _deleteToolStripMenuItemTextField.ToolTipText = "Delete the text box.";
            _deleteToolStripMenuItemTextField.Click += new EventHandler(DeleteToolStripMenuItemTextField_Click);
            // 
            // _insertBeneathNodeToolStripMenuItem
            // 
            _insertBeneathNodeToolStripMenuItem.Name = "_insertBeneathNodeToolStripMenuItem";
            _insertBeneathNodeToolStripMenuItem.Size = new Size(490, 48);
            _insertBeneathNodeToolStripMenuItem.Text = "&Insert Code";
            _insertBeneathNodeToolStripMenuItem.ToolTipText = "Insert Code beneatch selected node in Activity Diagram";
            _insertBeneathNodeToolStripMenuItem.Click += new EventHandler(InsertBeneathNodeToolStripMenuItem_Click);
            // 
            // _addActivityToolStripMenuItem
            // 
            _addActivityToolStripMenuItem.Name = "_addActivityToolStripMenuItem";
            _addActivityToolStripMenuItem.Size = new Size(490, 48);
            _addActivityToolStripMenuItem.Text = "&Activity";
            _addActivityToolStripMenuItem.ToolTipText = "Add Activity beneath selected Node";
            _addActivityToolStripMenuItem.Click += new EventHandler(AddActivityToolStripMenuItem_Click);
            // 
            // _addCompositeActivityToolStripMenuItem
            // 
            _addCompositeActivityToolStripMenuItem.Name = "_addCompositeActivityToolStripMenuItem";
            _addCompositeActivityToolStripMenuItem.Size = new Size(490, 48);
            _addCompositeActivityToolStripMenuItem.Text = "&Composite Activity";
            _addCompositeActivityToolStripMenuItem.ToolTipText = "Add Composite Activity beneatch selected Node";
            _addCompositeActivityToolStripMenuItem.Click += new EventHandler(AddCompositeActivityToolStripMenuItem_Click);
            // 
            // _addFinalToolStripMenuItem
            // 
            _addFinalToolStripMenuItem.Name = "_addFinalToolStripMenuItem";
            _addFinalToolStripMenuItem.Size = new Size(490, 48);
            _addFinalToolStripMenuItem.Text = "&Final";
            _addFinalToolStripMenuItem.ToolTipText = "Add Final beneath selected Node";
            _addFinalToolStripMenuItem.Click += new EventHandler(AddFinalToolStripMenuItem_Click);
            // 
            // _addMergeToolStripMenuItem
            // 
            _addMergeToolStripMenuItem.Name = "_addMergeToolStripMenuItem";
            _addMergeToolStripMenuItem.Size = new Size(490, 48);
            _addMergeToolStripMenuItem.Text = "&Merge";
            _addMergeToolStripMenuItem.ToolTipText = "Add merge beneath selected node";
            _addMergeToolStripMenuItem.Click += new EventHandler(AddMergeToolStripMenuItem_Click);
            // 
            // _showAllPortsToolStripMenuItem
            // 
            _showAllPortsToolStripMenuItem.Name = "_showAllPortsToolStripMenuItem";
            _showAllPortsToolStripMenuItem.Size = new Size(490, 48);
            _showAllPortsToolStripMenuItem.Text = "&Show all ports";
            _showAllPortsToolStripMenuItem.ToolTipText = "Show all ports of selected classifier";
            _showAllPortsToolStripMenuItem.Click += new EventHandler(ShowAllPortsToolStripMenuItem_Click);
            // 
            // _insertTextIntoNodeToolStripMenuItem
            // 
            _insertTextIntoNodeToolStripMenuItem.Name = "_insertTextIntoNodeToolStripMenuItem";
            _insertTextIntoNodeToolStripMenuItem.Size = new Size(490, 48);
            _insertTextIntoNodeToolStripMenuItem.Text = "&Insert text into Element Notes";
            _insertTextIntoNodeToolStripMenuItem.ToolTipText = "Insert text into seleted Element Notes";
            _insertTextIntoNodeToolStripMenuItem.Click += new EventHandler(InsertTextIntoNodeToolStripMenuItem_Click);
            // 
            // _btnNoMerge
            // 
            _btnNoMerge.Location = new Point(202, 200);
            _btnNoMerge.Margin = new Padding(8);
            _btnNoMerge.Name = "_btnNoMerge";
            _btnNoMerge.Size = new Size(82, 62);
            _btnNoMerge.TabIndex = 49;
            _btnNoMerge.Text = "nM";
            _toolTip.SetToolTip(_btnNoMerge, "Create a no Merge beneath/right  selected object");
            _btnNoMerge.UseVisualStyleBackColor = true;
            _btnNoMerge.Click += new EventHandler(BtnNoMerge_Click);
            // 
            // _btnSplitNodes
            // 
            _btnSplitNodes.Location = new Point(328, 270);
            _btnSplitNodes.Margin = new Padding(8);
            _btnSplitNodes.Name = "_btnSplitNodes";
            _btnSplitNodes.Size = new Size(58, 62);
            _btnSplitNodes.TabIndex = 54;
            _btnSplitNodes.Text = "S";
            _toolTip.SetToolTip(_btnSplitNodes, resources.GetString("_btnSplitNodes.ToolTip"));
            _btnSplitNodes.UseVisualStyleBackColor = true;
            _btnSplitNodes.Click += new EventHandler(BtnSplitNodes_Click);
            // 
            // _btnSplitAll
            // 
            _btnSplitAll.Location = new Point(248, 270);
            _btnSplitAll.Margin = new Padding(8);
            _btnSplitAll.Name = "_btnSplitAll";
            _btnSplitAll.Size = new Size(72, 62);
            _btnSplitAll.TabIndex = 55;
            _btnSplitAll.Text = "SA";
            _toolTip.SetToolTip(_btnSplitAll, "Split / disconnect all nodes around the last selected element.\r\n\r\n- Select an Ele" +
        "ment\r\n- Click on SA (disconnect all nodes)\r\n-  hoReverse disconnect all connecto" +
        "rs from this element.");
            _btnSplitAll.UseVisualStyleBackColor = true;
            _btnSplitAll.Click += new EventHandler(BtnSplitAll_Click);
            // 
            // _btnWriteText
            // 
            _btnWriteText.Location = new Point(288, 138);
            _btnWriteText.Margin = new Padding(8);
            _btnWriteText.Name = "_btnWriteText";
            _btnWriteText.Size = new Size(95, 65);
            _btnWriteText.TabIndex = 56;
            _btnWriteText.Text = "WT";
            _toolTip.SetToolTip(_btnWriteText, resources.GetString("_btnWriteText.ToolTip"));
            _btnWriteText.UseVisualStyleBackColor = true;
            _btnWriteText.Click += new EventHandler(_btnWriteText_Click);
            // 
            // _btnGuardNo
            // 
            _btnGuardNo.Location = new Point(8, 270);
            _btnGuardNo.Margin = new Padding(8);
            _btnGuardNo.Name = "_btnGuardNo";
            _btnGuardNo.Size = new Size(72, 62);
            _btnGuardNo.TabIndex = 53;
            _btnGuardNo.Text = "[N]";
            _toolTip.SetToolTip(_btnGuardNo, "Make a [no] Guard for an existing Control Flow\r\nConnect two selected Diagram node" +
        " with a [no] Control Flow\r\n");
            _btnGuardNo.UseVisualStyleBackColor = true;
            _btnGuardNo.Click += new EventHandler(BtnNoGuard_Click);
            // 
            // _btnGuardYes
            // 
            _btnGuardYes.Location = new Point(92, 270);
            _btnGuardYes.Margin = new Padding(8);
            _btnGuardYes.Name = "_btnGuardYes";
            _btnGuardYes.Size = new Size(72, 62);
            _btnGuardYes.TabIndex = 53;
            _btnGuardYes.Text = "[Y]";
            _toolTip.SetToolTip(_btnGuardYes, "Make a [yes] Guard for an existing Control Flow\r\nConnect two selected Diagram nod" +
        "e with a [yes] Control Flow");
            _btnGuardYes.UseVisualStyleBackColor = true;
            _btnGuardYes.Click += new EventHandler(BtnYesGuard_Click);
            // 
            // _btnGuardSpace
            // 
            _btnGuardSpace.Location = new Point(172, 270);
            _btnGuardSpace.Margin = new Padding(8);
            _btnGuardSpace.Name = "_btnGuardSpace";
            _btnGuardSpace.Size = new Size(65, 62);
            _btnGuardSpace.TabIndex = 53;
            _btnGuardSpace.Text = "[]";
            _toolTip.SetToolTip(_btnGuardSpace, "Set connector to Blank if exists or\r\njoin selected diagram nodes with last select" +
        "ed node.");
            _btnGuardSpace.UseVisualStyleBackColor = true;
            _btnGuardSpace.Click += new EventHandler(BtnBlankGuard_Click);
            // 
            // _btnFeatureUp
            // 
            _btnFeatureUp.Image = ((Image)(resources.GetObject("_btnFeatureUp.Image")));
            _btnFeatureUp.Location = new Point(252, 342);
            _btnFeatureUp.Margin = new Padding(0);
            _btnFeatureUp.Name = "_btnFeatureUp";
            _btnFeatureUp.Size = new Size(50, 62);
            _btnFeatureUp.TabIndex = 55;
            _toolTip.SetToolTip(_btnFeatureUp, "Feature (Attribute, Method) up\r\n\r\nNote:\r\nIn settings the automatic ordering has t" +
        "o be disabled (Feture, Attribute, Method/Operation).\r\n");
            _btnFeatureUp.UseVisualStyleBackColor = true;
            _btnFeatureUp.Click += new EventHandler(BtnFeatureUp_Click);
            // 
            // _btnFeatureDown
            // 
            _btnFeatureDown.Image = ((Image)(resources.GetObject("_btnFeatureDown.Image")));
            _btnFeatureDown.Location = new Point(328, 342);
            _btnFeatureDown.Margin = new Padding(0);
            _btnFeatureDown.Name = "_btnFeatureDown";
            _btnFeatureDown.Size = new Size(55, 62);
            _btnFeatureDown.TabIndex = 55;
            _toolTip.SetToolTip(_btnFeatureDown, "Feature (Attribute, Method) down\r\n\r\nNote:\r\nIn settings the automatic ordering has" +
        " to be disabled (Feature, Attribute, Method/Operation).");
            _btnFeatureDown.UseVisualStyleBackColor = true;
            _btnFeatureDown.Click += new EventHandler(BtnFeatureDown_Click);
            // 
            // _btnAddNoteAndLink
            // 
            _btnAddNoteAndLink.Location = new Point(8, 768);
            _btnAddNoteAndLink.Margin = new Padding(8);
            _btnAddNoteAndLink.Name = "_btnAddNoteAndLink";
            _btnAddNoteAndLink.Size = new Size(178, 65);
            _btnAddNoteAndLink.TabIndex = 9;
            _btnAddNoteAndLink.Text = "Feature";
            _toolTip.SetToolTip(_btnAddNoteAndLink, resources.GetString("_btnAddNoteAndLink.ToolTip"));
            _btnAddNoteAndLink.UseVisualStyleBackColor = true;
            _btnAddNoteAndLink.Click += new EventHandler(_btnAddNoteAndLink_Click);
            // 
            // _btnCopy
            // 
            _btnCopy.Location = new Point(288, 200);
            _btnCopy.Margin = new Padding(8);
            _btnCopy.Name = "_btnCopy";
            _btnCopy.Size = new Size(95, 65);
            _btnCopy.TabIndex = 19;
            _btnCopy.Text = "RT";
            _toolTip.SetToolTip(_btnCopy, resources.GetString("_btnCopy.ToolTip"));
            _btnCopy.UseVisualStyleBackColor = true;
            _btnCopy.Click += new EventHandler(_btnCopy_Click);
            // 
            // progressBar1
            // 
            progressBar1.Location = new Point(0, 52);
            progressBar1.Margin = new Padding(8);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(545, 5);
            progressBar1.TabIndex = 57;
            _toolTip.SetToolTip(progressBar1, "Show progress of initializing C-Macros");
            progressBar1.Visible = false;
            // 
            // _txtUserText
            // 
            _txtUserText.AcceptsReturn = true;
            _txtUserText.AcceptsTab = true;
            _txtUserText.AllowDrop = true;
            _txtUserText.ContextMenuStrip = _contextMenuStripTextField;
            _txtUserText.Font = new Font("Courier New", 8.25F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            _txtUserText.Location = new Point(400, 125);
            _txtUserText.Margin = new Padding(8);
            _txtUserText.Multiline = true;
            _txtUserText.Name = "_txtUserText";
            _txtUserText.ScrollBars = ScrollBars.Both;
            _txtUserText.Size = new Size(1732, 274);
            _txtUserText.TabIndex = 14;
            _toolTip.SetToolTip(_txtUserText, "Code:\r\n1. Enter Code\r\n2. Double click to insert text/code\r\n3. Ctrl+Enter for new " +
        "line\r\n4. Shft+Enter run Query\r\n\r\nMake sure a code line is terminated by a semico" +
        "lon as in C.");
            _txtUserText.WordWrap = false;
            _txtUserText.KeyDown += new KeyEventHandler(TxtUserText_KeyDown);
            _txtUserText.MouseDoubleClick += new MouseEventHandler(TxtUserText_MouseDoubleClick);
            // 
            // _menuStrip1
            // 
            _menuStrip1.AllowDrop = true;
            _menuStrip1.GripMargin = new Padding(2, 2, 0, 2);
            _menuStrip1.ImageScalingSize = new Size(20, 20);
            _menuStrip1.Items.AddRange(new ToolStripItem[] {
            _fileToolStripMenuItem,
            _doToolStripMenuItem,
            _codeToolStripMenuItem,
            _reqIfMenuItem,
            _autoToolStripMenuItem,
            _versionControlToolStripMenuItem,
            _maintenanceToolStripMenuItem,
            _helpToolStripMenuItem,
            helpToolStripMenuItem});
            _menuStrip1.Location = new Point(0, 0);
            _menuStrip1.Name = "_menuStrip1";
            _menuStrip1.RenderMode = ToolStripRenderMode.System;
            _menuStrip1.Size = new Size(2188, 60);
            _menuStrip1.TabIndex = 41;
            _menuStrip1.Text = "menuStrip1";
            // 
            // _fileToolStripMenuItem
            // 
            _fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
            _saveToolStripMenuItem,
            toolStripSeparator7,
            _settingsToolStripMenuItem,
            _setting2ConnectorToolStripMenuItem,
            settingsDiagramStylesToolStripMenuItem,
            toolStripSeparator5,
            reloadSettingsToolStripMenuItem,
            resetFactorySettingsToolStripMenuItem,
            toolStripSeparator9,
            _clearToolStripMenuItem});
            _fileToolStripMenuItem.Name = "_fileToolStripMenuItem";
            _fileToolStripMenuItem.Size = new Size(87, 50);
            _fileToolStripMenuItem.Text = "&File";
            _fileToolStripMenuItem.ToolTipText = "Reload the setting. \r\n- Settings.json (DiagramStyles)";
            // 
            // _saveToolStripMenuItem
            // 
            _saveToolStripMenuItem.Name = "_saveToolStripMenuItem";
            _saveToolStripMenuItem.Size = new Size(936, 54);
            _saveToolStripMenuItem.Text = "&Save";
            _saveToolStripMenuItem.ToolTipText = "Save bookmarks and history";
            _saveToolStripMenuItem.Click += new EventHandler(SaveToolStripMenuItem_Click);
            // 
            // toolStripSeparator7
            // 
            toolStripSeparator7.Name = "toolStripSeparator7";
            toolStripSeparator7.Size = new Size(933, 6);
            // 
            // _settingsToolStripMenuItem
            // 
            _settingsToolStripMenuItem.Name = "_settingsToolStripMenuItem";
            _settingsToolStripMenuItem.Size = new Size(936, 54);
            _settingsToolStripMenuItem.Text = "Settings";
            _settingsToolStripMenuItem.ToolTipText = "Opens the setting menu";
            _settingsToolStripMenuItem.Click += new EventHandler(SettingsToolStripMenuItem_Click);
            // 
            // _setting2ConnectorToolStripMenuItem
            // 
            _setting2ConnectorToolStripMenuItem.Name = "_setting2ConnectorToolStripMenuItem";
            _setting2ConnectorToolStripMenuItem.Size = new Size(936, 54);
            _setting2ConnectorToolStripMenuItem.Text = "Setting: Default Linestyle";
            _setting2ConnectorToolStripMenuItem.ToolTipText = "Set the default Linestyle for diagrams";
            _setting2ConnectorToolStripMenuItem.Click += new EventHandler(Setting2ConnectorToolStripMenuItem_Click);
            // 
            // settingsDiagramStylesToolStripMenuItem
            // 
            settingsDiagramStylesToolStripMenuItem.Name = "settingsDiagramStylesToolStripMenuItem";
            settingsDiagramStylesToolStripMenuItem.Size = new Size(936, 54);
            settingsDiagramStylesToolStripMenuItem.Text = "Settings: ReqIf, \'Bulk change\', Styles & more (Settings.json)";
            settingsDiagramStylesToolStripMenuItem.ToolTipText = resources.GetString("settingsDiagramStylesToolStripMenuItem.ToolTipText");
            settingsDiagramStylesToolStripMenuItem.Click += new EventHandler(SettingsDiagramStylesToolStripMenuItem_Click);
            // 
            // toolStripSeparator5
            // 
            toolStripSeparator5.Name = "toolStripSeparator5";
            toolStripSeparator5.Size = new Size(933, 6);
            // 
            // reloadSettingsToolStripMenuItem
            // 
            reloadSettingsToolStripMenuItem.Name = "reloadSettingsToolStripMenuItem";
            reloadSettingsToolStripMenuItem.Size = new Size(936, 54);
            reloadSettingsToolStripMenuItem.Text = "Reload Settings from Settings.json";
            reloadSettingsToolStripMenuItem.ToolTipText = "Load Settings.json from %appdata%\\ho\\hoReverse\\Settings.json";
            reloadSettingsToolStripMenuItem.Click += new EventHandler(ReloadSettingsToolStripMenuItem_Click);
            // 
            // resetFactorySettingsToolStripMenuItem
            // 
            resetFactorySettingsToolStripMenuItem.Name = "resetFactorySettingsToolStripMenuItem";
            resetFactorySettingsToolStripMenuItem.Size = new Size(936, 54);
            resetFactorySettingsToolStripMenuItem.Text = "ResetFactorySettings";
            resetFactorySettingsToolStripMenuItem.ToolTipText = "Reset the user.config to reset to delivery configuration.\r\n\r\nPlease restart. hoRe" +
    "verse will  create a new user.config with the default settings.";
            resetFactorySettingsToolStripMenuItem.Click += new EventHandler(ResetFactorySettingsToolStripMenuItem_Click);
            // 
            // toolStripSeparator9
            // 
            toolStripSeparator9.Name = "toolStripSeparator9";
            toolStripSeparator9.Size = new Size(933, 6);
            // 
            // _clearToolStripMenuItem
            // 
            _clearToolStripMenuItem.Name = "_clearToolStripMenuItem";
            _clearToolStripMenuItem.Size = new Size(936, 54);
            _clearToolStripMenuItem.Text = "Clear Diagram History and Bookmarks";
            _clearToolStripMenuItem.ToolTipText = "Delete all history and diagram entries for all projects.";
            _clearToolStripMenuItem.Click += new EventHandler(ClearToolStripMenuItem_Click);
            // 
            // _doToolStripMenuItem
            // 
            _doToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
            _createActivityForOperationToolStripMenuItem,
            _updateMethodParametersToolStripMenuItem,
            _toolStripSeparator3,
            _showFolderToolStripMenuItem,
            setFolderToolStripMenuItem,
            _toolStripSeparator4,
            _copyGuidSqlToClipboardToolStripMenuItem,
            moveToPackageToolStripMenuItem,
            _createSharedMemoryToolStripMenuItem,
            toolStripSeparator4,
            standardDiagramToolStripMenuItem,
            toolStripSeparator8,
            moveUsageToElementToolStripMenuItem,
            toolStripSeparator6,
            sortAlphabeticToolStripMenuItem,
            toolStripSeparator11});
            _doToolStripMenuItem.Name = "_doToolStripMenuItem";
            _doToolStripMenuItem.Size = new Size(81, 50);
            _doToolStripMenuItem.Text = "&Do";
            // 
            // _createActivityForOperationToolStripMenuItem
            // 
            _createActivityForOperationToolStripMenuItem.Name = "_createActivityForOperationToolStripMenuItem";
            _createActivityForOperationToolStripMenuItem.Size = new Size(795, 54);
            _createActivityForOperationToolStripMenuItem.Text = "&Create/Update Activity for Operation";
            _createActivityForOperationToolStripMenuItem.ToolTipText = resources.GetString("_createActivityForOperationToolStripMenuItem.ToolTipText");
            _createActivityForOperationToolStripMenuItem.Click += new EventHandler(CreateActivityForOperationToolStripMenuItem_Click);
            // 
            // _updateMethodParametersToolStripMenuItem
            // 
            _updateMethodParametersToolStripMenuItem.Name = "_updateMethodParametersToolStripMenuItem";
            _updateMethodParametersToolStripMenuItem.Size = new Size(795, 54);
            _updateMethodParametersToolStripMenuItem.Text = "&Update Activity from Method";
            _updateMethodParametersToolStripMenuItem.ToolTipText = "Updates the Activities according to selected contexts by:\r\n- Activity Name\r\n- Act" +
    "ivity Parameter\r\n";
            _updateMethodParametersToolStripMenuItem.Visible = false;
            _updateMethodParametersToolStripMenuItem.Click += new EventHandler(UpdateMethodParametersToolStripMenuItem_Click);
            // 
            // _toolStripSeparator3
            // 
            _toolStripSeparator3.Name = "_toolStripSeparator3";
            _toolStripSeparator3.Size = new Size(792, 6);
            // 
            // _showFolderToolStripMenuItem
            // 
            _showFolderToolStripMenuItem.Name = "_showFolderToolStripMenuItem";
            _showFolderToolStripMenuItem.Size = new Size(795, 54);
            _showFolderToolStripMenuItem.Text = "&Show folder";
            _showFolderToolStripMenuItem.ToolTipText = "Show folder (sourse file, controled package)";
            _showFolderToolStripMenuItem.Click += new EventHandler(ShowFolderToolStripMenuItem_Click);
            // 
            // setFolderToolStripMenuItem
            // 
            setFolderToolStripMenuItem.Name = "setFolderToolStripMenuItem";
            setFolderToolStripMenuItem.Size = new Size(795, 54);
            setFolderToolStripMenuItem.Text = "Set Folder";
            setFolderToolStripMenuItem.ToolTipText = "Set the Folder of a package  to easily access code.\r\n\r\nThe folder is used for imp" +
    "lementations. \r\nSo make sure you have assigned a Package language like \r\nC,C++.";
            setFolderToolStripMenuItem.Click += new EventHandler(SetFolderToolStripMenuItem_Click);
            // 
            // _toolStripSeparator4
            // 
            _toolStripSeparator4.Name = "_toolStripSeparator4";
            _toolStripSeparator4.Size = new Size(792, 6);
            // 
            // _copyGuidSqlToClipboardToolStripMenuItem
            // 
            _copyGuidSqlToClipboardToolStripMenuItem.Name = "_copyGuidSqlToClipboardToolStripMenuItem";
            _copyGuidSqlToClipboardToolStripMenuItem.Size = new Size(795, 54);
            _copyGuidSqlToClipboardToolStripMenuItem.Text = "&Copy GUID + SQL to clipboard";
            _copyGuidSqlToClipboardToolStripMenuItem.Click += new EventHandler(CopyGuidSqlToClipboardToolStripMenuItem_Click);
            // 
            // moveToPackageToolStripMenuItem
            // 
            moveToPackageToolStripMenuItem.Name = "moveToPackageToolStripMenuItem";
            moveToPackageToolStripMenuItem.Size = new Size(795, 54);
            moveToPackageToolStripMenuItem.Text = "Move elements to Browser (Package, Element)";
            moveToPackageToolStripMenuItem.ToolTipText = resources.GetString("moveToPackageToolStripMenuItem.ToolTipText");
            moveToPackageToolStripMenuItem.Click += new EventHandler(moveDiagramElementToToolStripMenuItem_Click);
            // 
            // _createSharedMemoryToolStripMenuItem
            // 
            _createSharedMemoryToolStripMenuItem.Name = "_createSharedMemoryToolStripMenuItem";
            _createSharedMemoryToolStripMenuItem.Size = new Size(795, 54);
            _createSharedMemoryToolStripMenuItem.Text = "&Create Shared Memory for Package";
            _createSharedMemoryToolStripMenuItem.ToolTipText = "Create shared memory from:\r\n#define SP_SHM_HW_MIC_START     0x40008000u\r\n#define " +
    "SP_SHM_HW_MIC_END       0x400083FFu\r\nas class+interface shared memory and:\r\nthe " +
    "associated Realisation dependency.\r\n";
            _createSharedMemoryToolStripMenuItem.Click += new EventHandler(CreateSharedMemoryToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new Size(792, 6);
            // 
            // standardDiagramToolStripMenuItem
            // 
            standardDiagramToolStripMenuItem.Name = "standardDiagramToolStripMenuItem";
            standardDiagramToolStripMenuItem.Size = new Size(795, 54);
            standardDiagramToolStripMenuItem.Text = "StandardDiagram (recursive)";
            standardDiagramToolStripMenuItem.ToolTipText = "Sets the diagram standards for selected:\r\n- Diagram\r\n- Element, recursive\r\n- Pack" +
    "age, recursive,\r\n\r\nParameters:\r\n- Diagram fit to one page\r\n- No qualifiers\r\n- Ou" +
    "tput Parameters with name and parameter";
            standardDiagramToolStripMenuItem.Click += new EventHandler(StandardDiagramToolStripMenuItem_Click);
            // 
            // toolStripSeparator8
            // 
            toolStripSeparator8.Name = "toolStripSeparator8";
            toolStripSeparator8.Size = new Size(792, 6);
            // 
            // moveUsageToElementToolStripMenuItem
            // 
            moveUsageToElementToolStripMenuItem.Name = "moveUsageToElementToolStripMenuItem";
            moveUsageToElementToolStripMenuItem.Size = new Size(795, 54);
            moveUsageToElementToolStripMenuItem.Text = "Move element usage to element";
            moveUsageToElementToolStripMenuItem.ToolTipText = resources.GetString("moveUsageToElementToolStripMenuItem.ToolTipText");
            moveUsageToElementToolStripMenuItem.Click += new EventHandler(MoveUsageToElementToolStripMenuItem_Click);
            // 
            // toolStripSeparator6
            // 
            toolStripSeparator6.Name = "toolStripSeparator6";
            toolStripSeparator6.Size = new Size(792, 6);
            // 
            // sortAlphabeticToolStripMenuItem
            // 
            sortAlphabeticToolStripMenuItem.Name = "sortAlphabeticToolStripMenuItem";
            sortAlphabeticToolStripMenuItem.Size = new Size(795, 54);
            sortAlphabeticToolStripMenuItem.Text = "Sort alphabetic";
            sortAlphabeticToolStripMenuItem.ToolTipText = "Sort the selected diagram elements in alphabetic order:\r\n- Ports, Pins, Parameter" +
    "s\r\n- Elements, Packages\r\n\r\nIt ignores:\r\n- ProvidedInterface\r\n- RequiredInterface" +
    "";
            sortAlphabeticToolStripMenuItem.Click += new EventHandler(SortAlphabeticToolStripMenuItem_Click);
            // 
            // toolStripSeparator11
            // 
            toolStripSeparator11.Name = "toolStripSeparator11";
            toolStripSeparator11.Size = new Size(792, 6);
            // 
            // _codeToolStripMenuItem
            // 
            _codeToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
            _insertAttributeToolStripMenuItem,
            _insertTypedefStructToolStripMenuItem,
            toolStripSeparator10,
            _insertFunctionToolStripMenuItem,
            insertFunctionMakeDuplicatesToolStripMenuItem,
            toolStripSeparator3,
            _updateActionToolStripMenuItem,
            _toolStripSeparator6,
            _deleteInvisibleuseRealizationDependenciesToolStripMenuItem,
            toolStripSeparator1,
            _generateComponentPortsToolStripMenuItem,
            _hideAllPortsToolStripMenuItem,
            _showAllPortsActivityParametersToolStripMenuItem,
            _toolStripSeparator7,
            _inserToolStripMenuItem,
            generateIncludesFromCodeSnippetToolStripMenuItem,
            _toolStripSeparator8,
            _setMacroToolStripMenuItem,
            _addMacroToolStripMenuItem,
            _delMacroToolStripMenuItem,
            _toolStripSeparator,
            _copyReleaseInformationToClipboardToolStripMenuItem,
            toolStripSeparator16});
            _codeToolStripMenuItem.Name = "_codeToolStripMenuItem";
            _codeToolStripMenuItem.Size = new Size(113, 50);
            _codeToolStripMenuItem.Text = "&Code";
            _codeToolStripMenuItem.ToolTipText = "Update Action:\r\n- Type: CallOperation \r\n- Link: To operation";
            // 
            // _insertAttributeToolStripMenuItem
            // 
            _insertAttributeToolStripMenuItem.Name = "_insertAttributeToolStripMenuItem";
            _insertAttributeToolStripMenuItem.Size = new Size(716, 54);
            _insertAttributeToolStripMenuItem.Text = "&Insert / update  attributes";
            _insertAttributeToolStripMenuItem.ToolTipText = resources.GetString("_insertAttributeToolStripMenuItem.ToolTipText");
            _insertAttributeToolStripMenuItem.Click += new EventHandler(InsertAttributeToolStripMenuItem_Click);
            // 
            // _insertTypedefStructToolStripMenuItem
            // 
            _insertTypedefStructToolStripMenuItem.Name = "_insertTypedefStructToolStripMenuItem";
            _insertTypedefStructToolStripMenuItem.Size = new Size(716, 54);
            _insertTypedefStructToolStripMenuItem.Text = "Create typedef struct/union/enum";
            _insertTypedefStructToolStripMenuItem.ToolTipText = resources.GetString("_insertTypedefStructToolStripMenuItem.ToolTipText");
            _insertTypedefStructToolStripMenuItem.Click += new EventHandler(InsertTypedefStructToolStripMenuItem_Click);
            // 
            // toolStripSeparator10
            // 
            toolStripSeparator10.Name = "toolStripSeparator10";
            toolStripSeparator10.Size = new Size(713, 6);
            // 
            // _insertFunctionToolStripMenuItem
            // 
            _insertFunctionToolStripMenuItem.Name = "_insertFunctionToolStripMenuItem";
            _insertFunctionToolStripMenuItem.Size = new Size(716, 54);
            _insertFunctionToolStripMenuItem.Text = "&Insert/ update function";
            _insertFunctionToolStripMenuItem.ToolTipText = "Insert & updates C- functions from code\r\n\r\nIf the function exists hoReverse updat" +
    "es parameter and return value.";
            _insertFunctionToolStripMenuItem.Click += new EventHandler(InsertFunctionToolStripMenuItem_Click);
            // 
            // insertFunctionMakeDuplicatesToolStripMenuItem
            // 
            insertFunctionMakeDuplicatesToolStripMenuItem.Name = "insertFunctionMakeDuplicatesToolStripMenuItem";
            insertFunctionMakeDuplicatesToolStripMenuItem.Size = new Size(716, 54);
            insertFunctionMakeDuplicatesToolStripMenuItem.Text = "Insert function (make duplicates)";
            insertFunctionMakeDuplicatesToolStripMenuItem.ToolTipText = "Insert C- functions from code\r\n\r\nIf the function exists hoReverse inserts a new f" +
    "unction.";
            insertFunctionMakeDuplicatesToolStripMenuItem.Click += new EventHandler(InsertFunctionMakeDuplicatesToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(713, 6);
            // 
            // _updateActionToolStripMenuItem
            // 
            _updateActionToolStripMenuItem.Name = "_updateActionToolStripMenuItem";
            _updateActionToolStripMenuItem.Size = new Size(716, 54);
            _updateActionToolStripMenuItem.Text = "UpdateOperationInAction";
            _updateActionToolStripMenuItem.ToolTipText = "Update Operation for Action. Select Action.\r\n\r\nIt tries no link to an operation w" +
    "ith no \'extern\' stereotype. \r\nIf this doesn\'t work it link to operation regardle" +
    "ss of stereotype.";
            _updateActionToolStripMenuItem.Click += new EventHandler(_updateActionToolStripMenuItem_Click);
            // 
            // _toolStripSeparator6
            // 
            _toolStripSeparator6.Name = "_toolStripSeparator6";
            _toolStripSeparator6.Size = new Size(713, 6);
            // 
            // _deleteInvisibleuseRealizationDependenciesToolStripMenuItem
            // 
            _deleteInvisibleuseRealizationDependenciesToolStripMenuItem.Name = "_deleteInvisibleuseRealizationDependenciesToolStripMenuItem";
            _deleteInvisibleuseRealizationDependenciesToolStripMenuItem.Size = new Size(716, 54);
            _deleteInvisibleuseRealizationDependenciesToolStripMenuItem.Text = "Delete invisible <<use>> dependencies";
            _deleteInvisibleuseRealizationDependenciesToolStripMenuItem.ToolTipText = "Delete for selected Class / Interface <<use>> dependencies which are not linked t" +
    "o diagramobjects on the current diagram.";
            _deleteInvisibleuseRealizationDependenciesToolStripMenuItem.Click += new EventHandler(DeleteInvisibleuseRealizationDependenciesToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(713, 6);
            // 
            // _generateComponentPortsToolStripMenuItem
            // 
            _generateComponentPortsToolStripMenuItem.Name = "_generateComponentPortsToolStripMenuItem";
            _generateComponentPortsToolStripMenuItem.Size = new Size(716, 54);
            _generateComponentPortsToolStripMenuItem.Text = "&GenerateComponentPorts";
            _generateComponentPortsToolStripMenuItem.ToolTipText = resources.GetString("_generateComponentPortsToolStripMenuItem.ToolTipText");
            _generateComponentPortsToolStripMenuItem.Click += new EventHandler(GenerateComponentPortsToolStripMenuItem_Click);
            // 
            // _hideAllPortsToolStripMenuItem
            // 
            _hideAllPortsToolStripMenuItem.Name = "_hideAllPortsToolStripMenuItem";
            _hideAllPortsToolStripMenuItem.Size = new Size(716, 54);
            _hideAllPortsToolStripMenuItem.Text = "Hide all ports";
            _hideAllPortsToolStripMenuItem.ToolTipText = "Hide the ports of the selected element.";
            _hideAllPortsToolStripMenuItem.Click += new EventHandler(_hideAllPortsToolStripMenuItem_Click);
            // 
            // _showAllPortsActivityParametersToolStripMenuItem
            // 
            _showAllPortsActivityParametersToolStripMenuItem.Name = "_showAllPortsActivityParametersToolStripMenuItem";
            _showAllPortsActivityParametersToolStripMenuItem.Size = new Size(716, 54);
            _showAllPortsActivityParametersToolStripMenuItem.Text = "Show all ports / activity parameters";
            _showAllPortsActivityParametersToolStripMenuItem.ToolTipText = "Show all ports/activity parameters  for classifier (Component, Activity)";
            _showAllPortsActivityParametersToolStripMenuItem.Click += new EventHandler(ShowAllPortsActivityParametersToolStripMenuItem_Click);
            // 
            // _toolStripSeparator7
            // 
            _toolStripSeparator7.Name = "_toolStripSeparator7";
            _toolStripSeparator7.Size = new Size(713, 6);
            // 
            // _inserToolStripMenuItem
            // 
            _inserToolStripMenuItem.Name = "_inserToolStripMenuItem";
            _inserToolStripMenuItem.Size = new Size(716, 54);
            _inserToolStripMenuItem.Text = "Generate Include for classifier from File";
            _inserToolStripMenuItem.ToolTipText = resources.GetString("_inserToolStripMenuItem.ToolTipText");
            _inserToolStripMenuItem.Click += new EventHandler(_generateIncludeForClassifierToolStripMenuItem_Click);
            // 
            // generateIncludesFromCodeSnippetToolStripMenuItem
            // 
            generateIncludesFromCodeSnippetToolStripMenuItem.Name = "generateIncludesFromCodeSnippetToolStripMenuItem";
            generateIncludesFromCodeSnippetToolStripMenuItem.Size = new Size(716, 54);
            generateIncludesFromCodeSnippetToolStripMenuItem.Text = "Generate Includes from code snippet";
            generateIncludesFromCodeSnippetToolStripMenuItem.ToolTipText = resources.GetString("generateIncludesFromCodeSnippetToolStripMenuItem.ToolTipText");
            generateIncludesFromCodeSnippetToolStripMenuItem.Click += new EventHandler(GenerateIncludeForClassifierFromSnippetToolStripMenuItem_Click);
            // 
            // _toolStripSeparator8
            // 
            _toolStripSeparator8.Name = "_toolStripSeparator8";
            _toolStripSeparator8.Size = new Size(713, 6);
            // 
            // _setMacroToolStripMenuItem
            // 
            _setMacroToolStripMenuItem.Name = "_setMacroToolStripMenuItem";
            _setMacroToolStripMenuItem.Size = new Size(716, 54);
            _setMacroToolStripMenuItem.Text = "Set Macro";
            _setMacroToolStripMenuItem.ToolTipText = resources.GetString("_setMacroToolStripMenuItem.ToolTipText");
            _setMacroToolStripMenuItem.Click += new EventHandler(SetMacroToolStripMenuItem_Click);
            // 
            // _addMacroToolStripMenuItem
            // 
            _addMacroToolStripMenuItem.Name = "_addMacroToolStripMenuItem";
            _addMacroToolStripMenuItem.Size = new Size(716, 54);
            _addMacroToolStripMenuItem.Text = "Add Macro";
            _addMacroToolStripMenuItem.ToolTipText = resources.GetString("_addMacroToolStripMenuItem.ToolTipText");
            _addMacroToolStripMenuItem.Click += new EventHandler(AddMacroToolStripMenuItem_Click);
            // 
            // _delMacroToolStripMenuItem
            // 
            _delMacroToolStripMenuItem.Name = "_delMacroToolStripMenuItem";
            _delMacroToolStripMenuItem.Size = new Size(716, 54);
            _delMacroToolStripMenuItem.Text = "Del Macro";
            _delMacroToolStripMenuItem.ToolTipText = "Delete all macros/stereotypes for the selected item.";
            _delMacroToolStripMenuItem.Click += new EventHandler(DelMacroToolStripMenuItem_Click);
            // 
            // _toolStripSeparator
            // 
            _toolStripSeparator.Name = "_toolStripSeparator";
            _toolStripSeparator.Size = new Size(713, 6);
            // 
            // _copyReleaseInformationToClipboardToolStripMenuItem
            // 
            _copyReleaseInformationToClipboardToolStripMenuItem.Name = "_copyReleaseInformationToClipboardToolStripMenuItem";
            _copyReleaseInformationToClipboardToolStripMenuItem.Size = new Size(716, 54);
            _copyReleaseInformationToClipboardToolStripMenuItem.Text = "&Copy release information to Clipboard";
            _copyReleaseInformationToClipboardToolStripMenuItem.ToolTipText = "Copy release information of *.c and *.h files to Clipboard:\r\n- Select Module\r\n- F" +
    "or all elements on the diagram the release information is printed";
            _copyReleaseInformationToClipboardToolStripMenuItem.Click += new EventHandler(CopyReleaseInformationToClipboardToolStripMenuItem_Click);
            // 
            // toolStripSeparator16
            // 
            toolStripSeparator16.Name = "toolStripSeparator16";
            toolStripSeparator16.Size = new Size(713, 6);
            // 
            // _reqIfMenuItem
            // 
            _reqIfMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
            InfoReqIfInquiryToolStripMenuItem,
            InfoReqIfInquiryValidationToolStripMenuItem});
            _reqIfMenuItem.Name = "_reqIfMenuItem";
            _reqIfMenuItem.Size = new Size(116, 50);
            _reqIfMenuItem.Text = "ReqIF";
            _reqIfMenuItem.ToolTipText = "Info for a ReqIF file\r\n\r\nFor a file, e.g. *.reqif, or *.reqifz it outputs the ent" +
    "ries with:\r\n- File-Name\r\n- Identifier\r\n- Number of requirements\r\n- Number of lin" +
    "ks";
            // 
            // InfoReqIfInquiryToolStripMenuItem
            // 
            InfoReqIfInquiryToolStripMenuItem.Name = "InfoReqIfInquiryToolStripMenuItem";
            InfoReqIfInquiryToolStripMenuItem.Size = new Size(630, 54);
            InfoReqIfInquiryToolStripMenuItem.Text = "Info *.reqif/*.reqifz -file";
            InfoReqIfInquiryToolStripMenuItem.ToolTipText = "Inquiry ReqIF (Folder or file)\r\n\r\nFiles:\r\n- *.reqifz\r\n- *.reqif\r\n\r\nFolder:\r\n\r\nNo " +
    "validation of reqif.";
            InfoReqIfInquiryToolStripMenuItem.Click += new EventHandler(InfoReqIfInquiryToolStripMenuItem_Click);
            // 
            // InfoReqIfInquiryValidationToolStripMenuItem
            // 
            InfoReqIfInquiryValidationToolStripMenuItem.Name = "InfoReqIfInquiryValidationToolStripMenuItem";
            InfoReqIfInquiryValidationToolStripMenuItem.Size = new Size(630, 54);
            InfoReqIfInquiryValidationToolStripMenuItem.Text = "Info *.reqif/*.reqifz with validation";
            InfoReqIfInquiryValidationToolStripMenuItem.ToolTipText = "Inquiry ReqIF (Folder or file)\r\n\r\nFiles:\r\n- *.reqifz\r\n- *.reqif\r\n\r\nFolder:\r\n\r\nWit" +
    "h validation of reqif.\r\n";
            InfoReqIfInquiryValidationToolStripMenuItem.Click += new EventHandler(InfoReqIfInquiryValidationToolStripMenuItem_Click);
            // 
            // _autoToolStripMenuItem
            // 
            _autoToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
            modulesToolStripMenuItem,
            inventoryToolStripMenuItem,
            _getToolStripMenuItem,
            makeRunnableToolStripMenuItem,
            makeServicePortToolStripMenuItem,
            makeCalloutToolStripMenuItem,
            toolStripSeparator12,
            showExternalComponentFunctionsToolStripMenuItem,
            showProvidedRequiredFunctionsForSourceToolStripMenuItem,
            showFunctionsToolStripMenuItem,
            toolStripSeparator13,
            showSymbolDataBaseFoldersToolStripMenuItem});
            _autoToolStripMenuItem.Name = "_autoToolStripMenuItem";
            _autoToolStripMenuItem.Size = new Size(111, 50);
            _autoToolStripMenuItem.Text = "C-DB";
            _autoToolStripMenuItem.ToolTipText = "Tools to use the C/C++ Database supported by Microsoft VS Code.\r\n- Provided and r" +
    "equired Interfaces\r\n- Functions and Macros";
            // 
            // modulesToolStripMenuItem
            // 
            modulesToolStripMenuItem.Name = "modulesToolStripMenuItem";
            modulesToolStripMenuItem.Size = new Size(872, 54);
            modulesToolStripMenuItem.Text = "Generate";
            modulesToolStripMenuItem.Visible = false;
            // 
            // inventoryToolStripMenuItem
            // 
            inventoryToolStripMenuItem.Name = "inventoryToolStripMenuItem";
            inventoryToolStripMenuItem.Size = new Size(872, 54);
            inventoryToolStripMenuItem.Text = "Inventory";
            inventoryToolStripMenuItem.Visible = false;
            // 
            // _getToolStripMenuItem
            // 
            _getToolStripMenuItem.Name = "_getToolStripMenuItem";
            _getToolStripMenuItem.Size = new Size(872, 54);
            _getToolStripMenuItem.Text = "GetExternalFunctions";
            _getToolStripMenuItem.Visible = false;
            // 
            // makeRunnableToolStripMenuItem
            // 
            makeRunnableToolStripMenuItem.Name = "makeRunnableToolStripMenuItem";
            makeRunnableToolStripMenuItem.Size = new Size(872, 54);
            makeRunnableToolStripMenuItem.Text = "MakeRunnablePort";
            makeRunnableToolStripMenuItem.ToolTipText = "Makes an Service Autosar Port\\r\\n\\r\\nSelect one or more ports.";
            makeRunnableToolStripMenuItem.Visible = false;
            makeRunnableToolStripMenuItem.Click += new EventHandler(MakeRunnableToolStripMenuItem_Click);
            // 
            // makeServicePortToolStripMenuItem
            // 
            makeServicePortToolStripMenuItem.Name = "makeServicePortToolStripMenuItem";
            makeServicePortToolStripMenuItem.Size = new Size(872, 54);
            makeServicePortToolStripMenuItem.Text = "MakeServicePort";
            makeServicePortToolStripMenuItem.Visible = false;
            makeServicePortToolStripMenuItem.Click += new EventHandler(MakeServicePortToolStripMenuItem_Click);
            // 
            // makeCalloutToolStripMenuItem
            // 
            makeCalloutToolStripMenuItem.Name = "makeCalloutToolStripMenuItem";
            makeCalloutToolStripMenuItem.Size = new Size(872, 54);
            makeCalloutToolStripMenuItem.Text = "MakeCalloutPort";
            makeCalloutToolStripMenuItem.Visible = false;
            makeCalloutToolStripMenuItem.Click += new EventHandler(MakeCalloutToolStripMenuItem_Click);
            // 
            // toolStripSeparator12
            // 
            toolStripSeparator12.Name = "toolStripSeparator12";
            toolStripSeparator12.Size = new Size(869, 6);
            // 
            // showExternalComponentFunctionsToolStripMenuItem
            // 
            showExternalComponentFunctionsToolStripMenuItem.Name = "showExternalComponentFunctionsToolStripMenuItem";
            showExternalComponentFunctionsToolStripMenuItem.Size = new Size(872, 54);
            showExternalComponentFunctionsToolStripMenuItem.Text = "Show Provided / Required Functions for EA-Element";
            showExternalComponentFunctionsToolStripMenuItem.ToolTipText = resources.GetString("showExternalComponentFunctionsToolStripMenuItem.ToolTipText");
            // 
            // showProvidedRequiredFunctionsForSourceToolStripMenuItem
            // 
            showProvidedRequiredFunctionsForSourceToolStripMenuItem.Name = "showProvidedRequiredFunctionsForSourceToolStripMenuItem";
            showProvidedRequiredFunctionsForSourceToolStripMenuItem.Size = new Size(872, 54);
            showProvidedRequiredFunctionsForSourceToolStripMenuItem.Text = "Show Provided / Required Functions for File/Folder";
            showProvidedRequiredFunctionsForSourceToolStripMenuItem.ToolTipText = resources.GetString("showProvidedRequiredFunctionsForSourceToolStripMenuItem.ToolTipText");
            // 
            // showFunctionsToolStripMenuItem
            // 
            showFunctionsToolStripMenuItem.Name = "showFunctionsToolStripMenuItem";
            showFunctionsToolStripMenuItem.Size = new Size(872, 54);
            showFunctionsToolStripMenuItem.Text = "Show all Functions";
            showFunctionsToolStripMenuItem.ToolTipText = "Shows all functions and macros\r\n\r\nIt requires:\r\n- VC Code symbol database\r\n- C/C+" +
    "+ Code with up to date VC Code symbol database";
            // 
            // toolStripSeparator13
            // 
            toolStripSeparator13.Name = "toolStripSeparator13";
            toolStripSeparator13.Size = new Size(869, 6);
            // 
            // showSymbolDataBaseFoldersToolStripMenuItem
            // 
            showSymbolDataBaseFoldersToolStripMenuItem.Name = "showSymbolDataBaseFoldersToolStripMenuItem";
            showSymbolDataBaseFoldersToolStripMenuItem.Size = new Size(872, 54);
            showSymbolDataBaseFoldersToolStripMenuItem.Text = "Show Symbol VC-Code DataBase Folders";
            showSymbolDataBaseFoldersToolStripMenuItem.ToolTipText = "Show the folder with the VC-Cide Symbol database.\r\n\r\nIn case of unknown issues de" +
    "lete the whole folder. VS-Code will recreate it!";
            showSymbolDataBaseFoldersToolStripMenuItem.Click += new EventHandler(ShowSymbolDataBaseFoldersToolStripMenuItem_Click);
            // 
            // _versionControlToolStripMenuItem
            // 
            _versionControlToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
            _svnLogToolStripMenuItem,
            _svnTortoiseRepobrowserToolStripMenuItem,
            _showDirectoryToolStripMenuItem,
            _toolStripSeparator1,
            _getVcLatestrecursiveToolStripMenuItem,
            _setSvnKeywordsToolStripMenuItem,
            _setSvnTaggedValuesToolStripMenuItem1,
            _setSvnTaggedValuesToolStripMenuItem,
            _toolStripSeparator2,
            _changeXmlPathToolStripMenuItem1,
            _toolStripSeparator5});
            _versionControlToolStripMenuItem.Name = "_versionControlToolStripMenuItem";
            _versionControlToolStripMenuItem.Size = new Size(79, 50);
            _versionControlToolStripMenuItem.Text = "&VC";
            _versionControlToolStripMenuItem.ToolTipText = "VersionControl (most SVN):\r\n\r\nSets the svn keywords:\r\n- svnDoc, svnRevision\r\nfor " +
    "a package.";
            // 
            // _svnLogToolStripMenuItem
            // 
            _svnLogToolStripMenuItem.Name = "_svnLogToolStripMenuItem";
            _svnLogToolStripMenuItem.Size = new Size(731, 54);
            _svnLogToolStripMenuItem.Text = "&Show Tortoise Log";
            _svnLogToolStripMenuItem.ToolTipText = "Opend the Tortoise Log window";
            _svnLogToolStripMenuItem.Click += new EventHandler(SvnLogToolStripMenuItem_Click);
            // 
            // _svnTortoiseRepobrowserToolStripMenuItem
            // 
            _svnTortoiseRepobrowserToolStripMenuItem.Name = "_svnTortoiseRepobrowserToolStripMenuItem";
            _svnTortoiseRepobrowserToolStripMenuItem.Size = new Size(731, 54);
            _svnTortoiseRepobrowserToolStripMenuItem.Text = "&Show Tortoise Repo Browser";
            _svnTortoiseRepobrowserToolStripMenuItem.ToolTipText = "Opens the Tortoise Repo Browser for the selected package";
            _svnTortoiseRepobrowserToolStripMenuItem.Click += new EventHandler(SvnTortoiseRepobrowserToolStripMenuItem_Click);
            // 
            // _showDirectoryToolStripMenuItem
            // 
            _showDirectoryToolStripMenuItem.Name = "_showDirectoryToolStripMenuItem";
            _showDirectoryToolStripMenuItem.Size = new Size(731, 54);
            _showDirectoryToolStripMenuItem.Text = "&Show Directory (VC or Code)";
            _showDirectoryToolStripMenuItem.ToolTipText = "Show Version Control directory of *.xml file or oh code";
            _showDirectoryToolStripMenuItem.Click += new EventHandler(ShowDirectoryToolStripMenuItem_Click);
            // 
            // _toolStripSeparator1
            // 
            _toolStripSeparator1.Name = "_toolStripSeparator1";
            _toolStripSeparator1.Size = new Size(728, 6);
            // 
            // _getVcLatestrecursiveToolStripMenuItem
            // 
            _getVcLatestrecursiveToolStripMenuItem.Name = "_getVcLatestrecursiveToolStripMenuItem";
            _getVcLatestrecursiveToolStripMenuItem.Size = new Size(731, 54);
            _getVcLatestrecursiveToolStripMenuItem.Text = "&GetVCLatest (recursive)";
            _getVcLatestrecursiveToolStripMenuItem.ToolTipText = "GetAllLatest for package (recursive)";
            _getVcLatestrecursiveToolStripMenuItem.Click += new EventHandler(GetVcLastestRecursiveToolStripMenuItem_Click);
            // 
            // _setSvnKeywordsToolStripMenuItem
            // 
            _setSvnKeywordsToolStripMenuItem.Name = "_setSvnKeywordsToolStripMenuItem";
            _setSvnKeywordsToolStripMenuItem.Size = new Size(731, 54);
            _setSvnKeywordsToolStripMenuItem.Text = "&Set svn keywords";
            _setSvnKeywordsToolStripMenuItem.ToolTipText = "Set the svn Module Package keywords for a VC package";
            _setSvnKeywordsToolStripMenuItem.Click += new EventHandler(SetSvnKeywordsToolStripMenuItem_Click);
            // 
            // _setSvnTaggedValuesToolStripMenuItem1
            // 
            _setSvnTaggedValuesToolStripMenuItem1.Name = "_setSvnTaggedValuesToolStripMenuItem1";
            _setSvnTaggedValuesToolStripMenuItem1.Size = new Size(731, 54);
            _setSvnTaggedValuesToolStripMenuItem1.Text = "&Set svn Module Tagged Values";
            _setSvnTaggedValuesToolStripMenuItem1.ToolTipText = "Set module package tagged values of a module package.\r\n\r\nModule Package\r\n   Archi" +
    "ctecture\r\n      Structure\r\n         Module\r\n\r\nTags:\r\nsvnDate\r\nsvnRevision";
            _setSvnTaggedValuesToolStripMenuItem1.Click += new EventHandler(SetSvnTaggedValuesToolStripMenuItem1_Click);
            // 
            // _setSvnTaggedValuesToolStripMenuItem
            // 
            _setSvnTaggedValuesToolStripMenuItem.Name = "_setSvnTaggedValuesToolStripMenuItem";
            _setSvnTaggedValuesToolStripMenuItem.Size = new Size(731, 54);
            _setSvnTaggedValuesToolStripMenuItem.Text = "&Set svn Module Tagged Values (recursive)";
            _setSvnTaggedValuesToolStripMenuItem.ToolTipText = "Sets svn module package Tagged Values for a Version controlled Package (recursive" +
    ") which is a module package. \r\n\r\nA module package contains subpackages Architect" +
    "ure\\Structure\\Module and Behavior\r\n";
            _setSvnTaggedValuesToolStripMenuItem.Click += new EventHandler(SetSvnTaggedValuesToolStripMenuItem_Click);
            // 
            // _toolStripSeparator2
            // 
            _toolStripSeparator2.Name = "_toolStripSeparator2";
            _toolStripSeparator2.Size = new Size(728, 6);
            // 
            // _changeXmlPathToolStripMenuItem1
            // 
            _changeXmlPathToolStripMenuItem1.Name = "_changeXmlPathToolStripMenuItem1";
            _changeXmlPathToolStripMenuItem1.Size = new Size(731, 54);
            _changeXmlPathToolStripMenuItem1.Text = "&ChangeXmlPath";
            _changeXmlPathToolStripMenuItem1.ToolTipText = resources.GetString("_changeXmlPathToolStripMenuItem1.ToolTipText");
            _changeXmlPathToolStripMenuItem1.Click += new EventHandler(ChangeXmlPathToolStripMenuItem_Click);
            // 
            // _toolStripSeparator5
            // 
            _toolStripSeparator5.Name = "_toolStripSeparator5";
            _toolStripSeparator5.Size = new Size(728, 6);
            // 
            // _maintenanceToolStripMenuItem
            // 
            _maintenanceToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
            _vCGetStateToolStripMenuItem,
            _vCResyncToolStripMenuItem,
            _vCxmiReconsileToolStripMenuItem,
            _vCRemoveToolStripMenuItem,
            toolStripSeparator17,
            doorsImportcsvToolStripMenuItem,
            doorsImportcsvWithFileDialogToolStripMenuItem,
            checkDOORSRequirementsToolStripMenuItem,
            toolStripSeparator18,
            importBySettingsToolStripMenuItem,
            importDoorsReqIFBySettingsToolStripMenuItem,
            importReqIFBySettings3ToolStripMenuItem,
            importReqIFBySettingsToolStripMenuItem,
            importReqIFBySettings5ToolStripMenuItem});
            _maintenanceToolStripMenuItem.Name = "_maintenanceToolStripMenuItem";
            _maintenanceToolStripMenuItem.Size = new Size(213, 50);
            _maintenanceToolStripMenuItem.Text = "Maintenance";
            _maintenanceToolStripMenuItem.ToolTipText = "Maintenance, experimental";
            // 
            // _vCGetStateToolStripMenuItem
            // 
            _vCGetStateToolStripMenuItem.Name = "_vCGetStateToolStripMenuItem";
            _vCGetStateToolStripMenuItem.Size = new Size(636, 54);
            _vCGetStateToolStripMenuItem.Text = "&VC get state";
            _vCGetStateToolStripMenuItem.ToolTipText = "Show the VC package state in a messagage box.\r\n- How has checked out the package";
            _vCGetStateToolStripMenuItem.Click += new EventHandler(VCGetStateToolStripMenuItem_Click);
            // 
            // _vCResyncToolStripMenuItem
            // 
            _vCResyncToolStripMenuItem.Name = "_vCResyncToolStripMenuItem";
            _vCResyncToolStripMenuItem.Size = new Size(636, 54);
            _vCResyncToolStripMenuItem.Text = "&VC Resync";
            _vCResyncToolStripMenuItem.ToolTipText = "Resynchronice svn VC package state for package(recursive).\r\n- Select Package\r\n- S" +
    "elect Model for whole Model (root package)";
            _vCResyncToolStripMenuItem.Click += new EventHandler(VCResyncToolStripMenuItem_Click);
            // 
            // _vCxmiReconsileToolStripMenuItem
            // 
            _vCxmiReconsileToolStripMenuItem.Name = "_vCxmiReconsileToolStripMenuItem";
            _vCxmiReconsileToolStripMenuItem.Size = new Size(636, 54);
            _vCxmiReconsileToolStripMenuItem.Text = "VC XMI reconsile";
            _vCxmiReconsileToolStripMenuItem.ToolTipText = "Scan all XMI packages and reconsile deleted objects or connectors.";
            _vCxmiReconsileToolStripMenuItem.Click += new EventHandler(VCXMIReconsileToolStripMenuItem_Click);
            // 
            // _vCRemoveToolStripMenuItem
            // 
            _vCRemoveToolStripMenuItem.Name = "_vCRemoveToolStripMenuItem";
            _vCRemoveToolStripMenuItem.Size = new Size(636, 54);
            _vCRemoveToolStripMenuItem.Text = "VC Remove";
            _vCRemoveToolStripMenuItem.Click += new EventHandler(_vCRemoveToolStripMenuItem_Click);
            // 
            // toolStripSeparator17
            // 
            toolStripSeparator17.Name = "toolStripSeparator17";
            toolStripSeparator17.Size = new Size(633, 6);
            // 
            // doorsImportcsvToolStripMenuItem
            // 
            doorsImportcsvToolStripMenuItem.Name = "doorsImportcsvToolStripMenuItem";
            doorsImportcsvToolStripMenuItem.Size = new Size(636, 54);
            doorsImportcsvToolStripMenuItem.Text = "Doors Import *.csv";
            doorsImportcsvToolStripMenuItem.ToolTipText = resources.GetString("doorsImportcsvToolStripMenuItem.ToolTipText");
            doorsImportcsvToolStripMenuItem.Click += new EventHandler(DoorsImportcsvToolStripMenuItem_Click);
            // 
            // doorsImportcsvWithFileDialogToolStripMenuItem
            // 
            doorsImportcsvWithFileDialogToolStripMenuItem.Name = "doorsImportcsvWithFileDialogToolStripMenuItem";
            doorsImportcsvWithFileDialogToolStripMenuItem.Size = new Size(636, 54);
            doorsImportcsvWithFileDialogToolStripMenuItem.Text = "Doors Import *.csv with file Dialog";
            doorsImportcsvWithFileDialogToolStripMenuItem.ToolTipText = resources.GetString("doorsImportcsvWithFileDialogToolStripMenuItem.ToolTipText");
            doorsImportcsvWithFileDialogToolStripMenuItem.Click += new EventHandler(DoorsImportcsvWithFileDialogToolStripMenuItem_Click);
            // 
            // checkDOORSRequirementsToolStripMenuItem
            // 
            checkDOORSRequirementsToolStripMenuItem.Name = "checkDOORSRequirementsToolStripMenuItem";
            checkDOORSRequirementsToolStripMenuItem.Size = new Size(636, 54);
            checkDOORSRequirementsToolStripMenuItem.Text = "Check DOORS Requirements";
            checkDOORSRequirementsToolStripMenuItem.ToolTipText = "Select a package with imported DOORS requirements and run the check.\r\n\r\nIt shows:" +
    "\r\n- All not unique DOORS Requirements";
            checkDOORSRequirementsToolStripMenuItem.Click += new EventHandler(CheckDOORSRequirementsToolStripMenuItem_Click);
            // 
            // toolStripSeparator18
            // 
            toolStripSeparator18.Name = "toolStripSeparator18";
            toolStripSeparator18.Size = new Size(633, 6);
            // 
            // importBySettingsToolStripMenuItem
            // 
            importBySettingsToolStripMenuItem.Name = "importBySettingsToolStripMenuItem";
            importBySettingsToolStripMenuItem.Size = new Size(636, 54);
            importBySettingsToolStripMenuItem.Text = "ImportBySettings";
            importBySettingsToolStripMenuItem.ToolTipText = "Import specified by Settings.json, Chapter \'Import\'\r\n\r\nCurrently supported:\r\n- DO" +
    "ORS *.csv format\r\n\r\nThe function works in background and you can proceed writing" +
    ".";
            importBySettingsToolStripMenuItem.Click += new EventHandler(ImportBySettingsToolStripMenuItem_Click);
            // 
            // importDoorsReqIFBySettingsToolStripMenuItem
            // 
            importDoorsReqIFBySettingsToolStripMenuItem.Name = "importDoorsReqIFBySettingsToolStripMenuItem";
            importDoorsReqIFBySettingsToolStripMenuItem.Size = new Size(636, 54);
            importDoorsReqIFBySettingsToolStripMenuItem.Text = "ImportDoorsReqIFBySettings";
            importDoorsReqIFBySettingsToolStripMenuItem.Click += new EventHandler(ImportDoorsReqIFBySettingsToolStripMenuItem_Click);
            // 
            // importReqIFBySettings3ToolStripMenuItem
            // 
            importReqIFBySettings3ToolStripMenuItem.Name = "importReqIFBySettings3ToolStripMenuItem";
            importReqIFBySettings3ToolStripMenuItem.Size = new Size(636, 54);
            importReqIFBySettings3ToolStripMenuItem.Text = "ImportReqIFBySettings 3";
            importReqIFBySettings3ToolStripMenuItem.Click += new EventHandler(ImportReqIFBySettings3ToolStripMenuItem_Click_1);
            // 
            // importReqIFBySettingsToolStripMenuItem
            // 
            importReqIFBySettingsToolStripMenuItem.Name = "importReqIFBySettingsToolStripMenuItem";
            importReqIFBySettingsToolStripMenuItem.Size = new Size(636, 54);
            importReqIFBySettingsToolStripMenuItem.Text = "ImportReqIFBySettings 4";
            importReqIFBySettingsToolStripMenuItem.Click += new EventHandler(ImportReqIFBySettings4ToolStripMenuItem_Click);
            // 
            // importReqIFBySettings5ToolStripMenuItem
            // 
            importReqIFBySettings5ToolStripMenuItem.Name = "importReqIFBySettings5ToolStripMenuItem";
            importReqIFBySettings5ToolStripMenuItem.Size = new Size(636, 54);
            importReqIFBySettings5ToolStripMenuItem.Text = "ImportReqIFBySettings 5";
            importReqIFBySettings5ToolStripMenuItem.Click += new EventHandler(ImportReqIFBySettings5ToolStripMenuItem_Click);
            // 
            // _helpToolStripMenuItem
            // 
            _helpToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
            _aboutToolStripMenuItem,
            toolStripSeparator15,
            _helpF1ToolStripMenuItem,
            readmeToolStripMenuItem,
            repoToolStripMenuItem,
            hoToolsToolStripMenuItem,
            lineStyleToolStripMenuItem,
            toolStripSeparator19,
            sQLWildcardsToolStripMenuItem,
            reqIFToolStripMenuItem,
            toolStripSeparator14,
            analyzeCCToolStripMenuItem});
            _helpToolStripMenuItem.Name = "_helpToolStripMenuItem";
            _helpToolStripMenuItem.Size = new Size(104, 50);
            _helpToolStripMenuItem.Text = "&Help";
            // 
            // _aboutToolStripMenuItem
            // 
            _aboutToolStripMenuItem.Name = "_aboutToolStripMenuItem";
            _aboutToolStripMenuItem.Size = new Size(379, 54);
            _aboutToolStripMenuItem.Text = "&About";
            _aboutToolStripMenuItem.Click += new EventHandler(AboutToolStripMenuItem_Click);
            // 
            // toolStripSeparator15
            // 
            toolStripSeparator15.Name = "toolStripSeparator15";
            toolStripSeparator15.Size = new Size(376, 6);
            // 
            // _helpF1ToolStripMenuItem
            // 
            _helpF1ToolStripMenuItem.Name = "_helpF1ToolStripMenuItem";
            _helpF1ToolStripMenuItem.Size = new Size(379, 54);
            _helpF1ToolStripMenuItem.Text = "&Help / WiKi";
            _helpF1ToolStripMenuItem.ToolTipText = "Show help / WiKi";
            _helpF1ToolStripMenuItem.Click += new EventHandler(HelpF1ToolStripMenuItem_Click);
            // 
            // readmeToolStripMenuItem
            // 
            readmeToolStripMenuItem.Name = "readmeToolStripMenuItem";
            readmeToolStripMenuItem.Size = new Size(379, 54);
            readmeToolStripMenuItem.Text = "Readme";
            readmeToolStripMenuItem.ToolTipText = "Show readme";
            readmeToolStripMenuItem.Click += new EventHandler(ReadmeToolStripMenuItem_Click);
            // 
            // repoToolStripMenuItem
            // 
            repoToolStripMenuItem.Name = "repoToolStripMenuItem";
            repoToolStripMenuItem.Size = new Size(379, 54);
            repoToolStripMenuItem.Text = "Repo";
            repoToolStripMenuItem.ToolTipText = "Show GitHub repository";
            repoToolStripMenuItem.Click += new EventHandler(RepoToolStripMenuItem_Click);
            // 
            // hoToolsToolStripMenuItem
            // 
            hoToolsToolStripMenuItem.Name = "hoToolsToolStripMenuItem";
            hoToolsToolStripMenuItem.Size = new Size(379, 54);
            hoToolsToolStripMenuItem.Text = "hoTools";
            hoToolsToolStripMenuItem.ToolTipText = "Show WiKi hoTools";
            hoToolsToolStripMenuItem.Click += new EventHandler(HoToolsToolStripMenuItem_Click);
            // 
            // lineStyleToolStripMenuItem
            // 
            lineStyleToolStripMenuItem.Name = "lineStyleToolStripMenuItem";
            lineStyleToolStripMenuItem.Size = new Size(379, 54);
            lineStyleToolStripMenuItem.Text = "LineStyle";
            lineStyleToolStripMenuItem.ToolTipText = "Show WiKi LineStyle";
            lineStyleToolStripMenuItem.Click += new EventHandler(LineStyleToolStripMenuItem_Click);
            // 
            // toolStripSeparator19
            // 
            toolStripSeparator19.Name = "toolStripSeparator19";
            toolStripSeparator19.Size = new Size(376, 6);
            // 
            // sQLWildcardsToolStripMenuItem
            // 
            sQLWildcardsToolStripMenuItem.Name = "sQLWildcardsToolStripMenuItem";
            sQLWildcardsToolStripMenuItem.Size = new Size(379, 54);
            sQLWildcardsToolStripMenuItem.Text = "SQL Wildcards";
            sQLWildcardsToolStripMenuItem.Click += new EventHandler(SQLWildcardsToolStripMenuItem_Click);
            // 
            // reqIFToolStripMenuItem
            // 
            reqIFToolStripMenuItem.Name = "reqIFToolStripMenuItem";
            reqIFToolStripMenuItem.Size = new Size(379, 54);
            reqIFToolStripMenuItem.Text = "ReqIF";
            reqIFToolStripMenuItem.Click += new EventHandler(ReqIFToolStripMenuItem_Click);
            // 
            // toolStripSeparator14
            // 
            toolStripSeparator14.Name = "toolStripSeparator14";
            toolStripSeparator14.Size = new Size(376, 6);
            // 
            // analyzeCCToolStripMenuItem
            // 
            analyzeCCToolStripMenuItem.Name = "analyzeCCToolStripMenuItem";
            analyzeCCToolStripMenuItem.Size = new Size(379, 54);
            analyzeCCToolStripMenuItem.Text = "AnalyzeC/C++";
            analyzeCCToolStripMenuItem.Click += new EventHandler(AnalyzeCCToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            helpToolStripMenuItem.Size = new Size(55, 50);
            helpToolStripMenuItem.Text = "?";
            helpToolStripMenuItem.ToolTipText = "WiKi";
            helpToolStripMenuItem.Click += new EventHandler(HelpToolStripMenuItem_Click);
            // 
            // _toolStripContainer1
            // 
            _toolStripContainer1.BottomToolStripPanelVisible = false;
            // 
            // _toolStripContainer1.ContentPanel
            // 
            _toolStripContainer1.ContentPanel.Margin = new Padding(8);
            _toolStripContainer1.ContentPanel.Size = new Size(1910, 0);
            _toolStripContainer1.LeftToolStripPanelVisible = false;
            _toolStripContainer1.Location = new Point(8, 72);
            _toolStripContainer1.Margin = new Padding(8);
            _toolStripContainer1.Name = "_toolStripContainer1";
            _toolStripContainer1.RightToolStripPanelVisible = false;
            _toolStripContainer1.Size = new Size(1910, 62);
            _toolStripContainer1.TabIndex = 52;
            _toolStripContainer1.Text = "toolStripContainer1";
            // 
            // _toolStripContainer1.TopToolStripPanel
            // 
            _toolStripContainer1.TopToolStripPanel.Controls.Add(_toolStrip6);
            // 
            // _toolStrip6
            // 
            _toolStrip6.Dock = DockStyle.None;
            _toolStrip6.ImageScalingSize = new Size(20, 20);
            _toolStrip6.Items.AddRange(new ToolStripItem[] {
            _toolStripBtn11,
            _toolStripBtn12,
            _toolStripBtn13,
            _toolStripBtn14,
            _toolStripBtn15});
            _toolStrip6.Location = new Point(8, 0);
            _toolStrip6.Name = "_toolStrip6";
            _toolStrip6.Size = new Size(316, 62);
            _toolStrip6.TabIndex = 4;
            // 
            // _toolStripBtn11
            // 
            _toolStripBtn11.DisplayStyle = ToolStripItemDisplayStyle.Text;
            _toolStripBtn11.Image = ((Image)(resources.GetObject("_toolStripBtn11.Image")));
            _toolStripBtn11.ImageTransparentColor = Color.Magenta;
            _toolStripBtn11.Name = "_toolStripBtn11";
            _toolStripBtn11.Size = new Size(58, 55);
            _toolStripBtn11.Text = "1";
            _toolStripBtn11.Click += new EventHandler(ToolStripBtn11_Click);
            // 
            // _toolStripBtn12
            // 
            _toolStripBtn12.DisplayStyle = ToolStripItemDisplayStyle.Text;
            _toolStripBtn12.Image = ((Image)(resources.GetObject("_toolStripBtn12.Image")));
            _toolStripBtn12.ImageTransparentColor = Color.Magenta;
            _toolStripBtn12.Name = "_toolStripBtn12";
            _toolStripBtn12.Size = new Size(58, 55);
            _toolStripBtn12.Text = "2";
            _toolStripBtn12.Click += new EventHandler(ToolStripBtn12_Click);
            // 
            // _toolStripBtn13
            // 
            _toolStripBtn13.DisplayStyle = ToolStripItemDisplayStyle.Text;
            _toolStripBtn13.Image = ((Image)(resources.GetObject("_toolStripBtn13.Image")));
            _toolStripBtn13.ImageTransparentColor = Color.Magenta;
            _toolStripBtn13.Name = "_toolStripBtn13";
            _toolStripBtn13.Size = new Size(58, 55);
            _toolStripBtn13.Text = "3";
            _toolStripBtn13.Click += new EventHandler(ToolStripBtn13_Click);
            // 
            // _toolStripBtn14
            // 
            _toolStripBtn14.DisplayStyle = ToolStripItemDisplayStyle.Text;
            _toolStripBtn14.Image = ((Image)(resources.GetObject("_toolStripBtn14.Image")));
            _toolStripBtn14.ImageTransparentColor = Color.Magenta;
            _toolStripBtn14.Name = "_toolStripBtn14";
            _toolStripBtn14.Size = new Size(58, 55);
            _toolStripBtn14.Text = "4";
            _toolStripBtn14.Click += new EventHandler(ToolStripBtn14_Click);
            // 
            // _toolStripBtn15
            // 
            _toolStripBtn15.DisplayStyle = ToolStripItemDisplayStyle.Text;
            _toolStripBtn15.Image = ((Image)(resources.GetObject("_toolStripBtn15.Image")));
            _toolStripBtn15.ImageTransparentColor = Color.Magenta;
            _toolStripBtn15.Name = "_toolStripBtn15";
            _toolStripBtn15.Size = new Size(58, 55);
            _toolStripBtn15.Text = "5";
            _toolStripBtn15.Click += new EventHandler(ToolStripBtn15_Click);
            // 
            // _toolStrip1
            // 
            _toolStrip1.Dock = DockStyle.None;
            _toolStrip1.ImageScalingSize = new Size(20, 20);
            _toolStrip1.Items.AddRange(new ToolStripItem[] {
            _toolStripBtn1,
            _toolStripBtn2,
            _toolStripBtn3,
            _toolStripBtn4,
            _toolStripBtn5});
            _toolStrip1.Location = new Point(375, 72);
            _toolStrip1.Name = "_toolStrip1";
            _toolStrip1.Padding = new Padding(0, 0, 5, 0);
            _toolStrip1.Size = new Size(319, 62);
            _toolStrip1.TabIndex = 0;
            _toolStrip1.ItemClicked += new ToolStripItemClickedEventHandler(ToolStrip1_ItemClicked);
            // 
            // _toolStripBtn1
            // 
            _toolStripBtn1.DisplayStyle = ToolStripItemDisplayStyle.Text;
            _toolStripBtn1.Image = ((Image)(resources.GetObject("_toolStripBtn1.Image")));
            _toolStripBtn1.ImageTransparentColor = Color.Magenta;
            _toolStripBtn1.Name = "_toolStripBtn1";
            _toolStripBtn1.Size = new Size(58, 55);
            _toolStripBtn1.Text = "1";
            _toolStripBtn1.Click += new EventHandler(ToolStripBtn1_Click);
            // 
            // _toolStripBtn2
            // 
            _toolStripBtn2.DisplayStyle = ToolStripItemDisplayStyle.Text;
            _toolStripBtn2.Image = ((Image)(resources.GetObject("_toolStripBtn2.Image")));
            _toolStripBtn2.ImageTransparentColor = Color.Magenta;
            _toolStripBtn2.Name = "_toolStripBtn2";
            _toolStripBtn2.Size = new Size(58, 55);
            _toolStripBtn2.Text = "2";
            _toolStripBtn2.Click += new EventHandler(ToolStripBtn2_Click);
            // 
            // _toolStripBtn3
            // 
            _toolStripBtn3.DisplayStyle = ToolStripItemDisplayStyle.Text;
            _toolStripBtn3.Image = ((Image)(resources.GetObject("_toolStripBtn3.Image")));
            _toolStripBtn3.ImageTransparentColor = Color.Magenta;
            _toolStripBtn3.Name = "_toolStripBtn3";
            _toolStripBtn3.Size = new Size(58, 55);
            _toolStripBtn3.Text = "3";
            _toolStripBtn3.Click += new EventHandler(ToolStripBtn3_Click);
            // 
            // _toolStripBtn4
            // 
            _toolStripBtn4.DisplayStyle = ToolStripItemDisplayStyle.Text;
            _toolStripBtn4.Image = ((Image)(resources.GetObject("_toolStripBtn4.Image")));
            _toolStripBtn4.ImageTransparentColor = Color.Magenta;
            _toolStripBtn4.Name = "_toolStripBtn4";
            _toolStripBtn4.Size = new Size(58, 55);
            _toolStripBtn4.Text = "4";
            _toolStripBtn4.Click += new EventHandler(ToolStripBtn4_Click);
            // 
            // _toolStripBtn5
            // 
            _toolStripBtn5.DisplayStyle = ToolStripItemDisplayStyle.Text;
            _toolStripBtn5.Image = ((Image)(resources.GetObject("_toolStripBtn5.Image")));
            _toolStripBtn5.ImageTransparentColor = Color.Magenta;
            _toolStripBtn5.Name = "_toolStripBtn5";
            _toolStripBtn5.Size = new Size(58, 55);
            _toolStripBtn5.Text = "5";
            _toolStripBtn5.Click += new EventHandler(ToolStripBtn5_Click);
            // 
            // backgroundWorker
            // 
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.WorkerSupportsCancellation = true;
            backgroundWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(BackgroundWorker_ProgressChanged);
            backgroundWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(BackgroundWorker_RunWorkerCompleted);
            // 
            // HoReverseGui
            // 
            AutoScaleDimensions = new SizeF(240F, 240F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Controls.Add(progressBar1);
            Controls.Add(_btnWriteText);
            Controls.Add(_toolStrip1);
            Controls.Add(_btnFeatureDown);
            Controls.Add(_btnFeatureUp);
            Controls.Add(_btnSplitAll);
            Controls.Add(_btnSplitNodes);
            Controls.Add(_btnGuardSpace);
            Controls.Add(_btnGuardYes);
            Controls.Add(_btnGuardNo);
            Controls.Add(_toolStripContainer1);
            Controls.Add(_menuStrip1);
            Controls.Add(_btnActivityCompositeFromText);
            Controls.Add(_btnDecisionFromText);
            Controls.Add(_btnUpdateActivityParameter);
            Controls.Add(_btnNoMerge);
            Controls.Add(_btnBezier);
            Controls.Add(_btnFinal);
            Controls.Add(_btnActivity);
            Controls.Add(_btnNoteFromText);
            Controls.Add(_btnDecision);
            Controls.Add(_btnHistory);
            Controls.Add(_btnAction);
            Controls.Add(_btnMerge);
            Controls.Add(_btnInsert);
            Controls.Add(BtnTh);
            Controls.Add(_btnLv);
            Controls.Add(_btnBookmark);
            Controls.Add(_btnBookmarkFrwrd);
            Controls.Add(_btnBookmarkBack);
            Controls.Add(_btnBookmarkRemove);
            Controls.Add(_btnBookmarkAdd);
            Controls.Add(_btnFrwrd);
            Controls.Add(_btnBack);
            Controls.Add(_btnLh);
            Controls.Add(_btnC);
            Controls.Add(_btnCopy);
            Controls.Add(_btnD);
            Controls.Add(_btnA);
            Controls.Add(_btnOr);
            Controls.Add(_btnComposite);
            Controls.Add(_txtUserText);
            Controls.Add(_btnDisplaySpecification);
            Controls.Add(_btnFindUsage);
            Controls.Add(_btnLocateType);
            Controls.Add(_btnAddConstraint);
            Controls.Add(_btnAddNoteAndLink);
            Controls.Add(_btnAddElementNote);
            Controls.Add(_btnLocateOperation);
            Controls.Add(_btnDisplayBehavior);
            Controls.Add(_btnOs);
            Controls.Add(_btnTv);
            Margin = new Padding(8);
            Name = "HoReverseGui";
            Size = new Size(2188, 1170);
            _toolTip.SetToolTip(this, "Progress capture all C/C++-Macros");
            Load += new EventHandler(AddinControl_Load);
            _contextMenuStripTextField.ResumeLayout(false);
            _menuStrip1.ResumeLayout(false);
            _menuStrip1.PerformLayout();
            _toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            _toolStripContainer1.TopToolStripPanel.PerformLayout();
            _toolStripContainer1.ResumeLayout(false);
            _toolStripContainer1.PerformLayout();
            _toolStrip6.ResumeLayout(false);
            _toolStrip6.PerformLayout();
            _toolStrip1.ResumeLayout(false);
            _toolStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

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
                                MessageBox.Show($@"Current length: {s.Length}", @"Length of guard > 252 characters, guard truncated!!");
                                s = s.Substring(0, 252);
                            }
                            con.TransitionGuard = s;
                        }
                        else
                        {
                            if (s.Length > 252)
                            {
                                MessageBox.Show($@"Current length: {s.Length}", @"Length of name > 252 characters, name truncated!!");
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
        /// 'I' Insert for selected diagram node:
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

        /// <summary>
        /// Create Activity with an Activity Diagram beneath the Activity. Make also this Activity diagram to a composite Diagram of the Activity.
        /// - Node in Diagram selected: Create Activity and connect it with the previously selected node
        /// - No Node in Diagram selected. Create Activity in the selected Tree Element (Package or Element)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        /// <summary>
        /// Create :
        /// Final object if a DiagramObject is selected
        /// Init object if a Diagram is the ContextObject and no DiagramObject is selected.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void BtnFinal_Click(object sender, EventArgs e)
        {

           EA.Element el = HoUtil.GetElementFromContextObject(_repository);
            // ActivityInitial at the top of the diagram
            if (el == null)
            {
                // Init node
                HoService.InsertDiagramElement(_repository, "StateNode", "100");
                _txtUserText.Text = @"Initialize";
                return;
            }
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

        // Double left/right Click
        private void TxtUserText_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            // force cr lf at line end
            _txtUserText.Text = Clipboard.GetText().Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\r\n");

            switch (e.Button)
            {
                case MouseButtons.Left:
                    break;
                // Double mouse click right runs the search 'Quick Search'
                case MouseButtons.Right:
                    var searchName = "Quick Search";
                    var searchTerm = "";
                    try
                    {
                        // run SQL search and display in Search Window
                        _repository.RunModelSearch(searchName, _txtUserText.Text, "", "");
                        return;
                    }
                    catch (Exception )
                    {
                        MessageBox.Show($@"Can't find search!{Environment.NewLine}{Environment.NewLine}- MDG hoTools.. enabled?{Environment.NewLine}- SQL path in Settings correct?{Environment.NewLine}- LINQPad support enabled (Settings General)? :{Environment.NewLine}'{searchName}' '{searchTerm}'{Environment.NewLine}'{Environment.NewLine}'{Environment.NewLine}- EA Search{Environment.NewLine}{Environment.NewLine}Note:{Environment.NewLine}- Define path in File, Settings{Environment.NewLine}- LINQPad needs a license and has to be installed!{Environment.NewLine}{Environment.NewLine}{e}",
                            $@"Error start search.");
                        return ;
                    }
                    break;



            }

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
                "Sybase.AdoNet.AseClient.dll",
                "SQLite.Interop.dll",
                "System.Data.SQLite.dll",
                "OpenMcdf.dll",
                "ReqIFSharp.dll",
                "ClosedXml.dll",
                "DocumentFormat.OpenXml.dll",
                "KBCsv.dll",
                "KBCsv.Extensions.Data.dll",
                "SautinSoft.RtfToHtml.dll",
                "SautinSoft.HtmlToRtf.dll"

            };
            HoUtil.AboutMessage($"V{_version}\r\n\r\nC - Reverse Engineering Workbench",
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
        private void DiagramSearchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HoService.RunQuickSearch(_repository, _addinSettings.DiagramSearchName, _txtUserText.Text.Replace($"{Environment.NewLine}", ""));
        }
        private void SimpleSearchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HoService.RunQuickSearch(_repository, _addinSettings.SimpleSearchName, _txtUserText.Text);
        }
        private void RecentlyModifedDiagramsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HoService.RunQuickSearch(_repository, _addinSettings.RecentModifiedDiagramsSearch, "");
        }
        /// <summary>
        /// Set the Action/Activity to QM
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void QmMenuItem_Click(object sender, EventArgs e)
        {
            SetCriticality("Action", "QM");
            SetCriticality("Activity", "QM");
        }
        /// <summary>
        /// Set the Action/Activity to QM
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AsilAMenuItem_Click(object sender, EventArgs e)
        {
            SetCriticality("Action", "ASIL A");
            SetCriticality("Activity", "ASIL A");
        }
        /// <summary>
        /// Set the Action/Activity to QM
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AsilBMenuItem_Click(object sender, EventArgs e)
        {
            SetCriticality("Action", "ASIL B");
            SetCriticality("Activity", "ASIL B");
        }
        /// <summary>
        /// Set the Action/Activity to QM
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AsilCMenuItem_Click(object sender, EventArgs e)
        {
            SetCriticality("Action", "ASIL C");
            SetCriticality("Activity", "ASIL C");
        }

        private void SetCriticality(string type, string criticality)
        {
            string stereotype = $"ZF_LE::LE {type}";
            TvItem t1 = new TvItem {Name = $"ZF_LE::LE {type}::Criticality", Value = criticality};

            HoService.BulkElementChangeWrapper(_repository,
                new List<string>(new[] {$"{type}"}),
                new List<string>(new[] {""}),
                new List<string>(new[] {$"{stereotype}"}),
                new List<TvItem>(new[] {t1}),
                new List<string>(new[] {""}));
        }

        /// <summary>
        /// Set the Action/Activity to QM
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void QmMenuItem_Click(object sender, EventArgs e)
        //{
        //    IList<string> tagList = new List<string>(new[] { "Action" });
        //    TvItem t1 = new TvItem { Name = "ZF_LE::LE Action::Criticality", Value = "ASIL-A" };

        //    HoService.BulkElementChangeWrapper(_repository,
        //        new List<string>(new[] { "Action" }),
        //        new List<string>(new[] { "" }),
        //        new List<string>(new[] { "ZF_LE::LE Action" }),
        //        new List<TvItem>(new[] { t1 }),
        //        new List<string>(new[] { "" }));

        //    HoService.BulkElementChangeWrapper(_repository,
        //        new List<string>(new[] { "Activity" }),
        //        new List<string>(new[] { "" }),
        //        new List<string>(new[] { "ZF_LE::LE Action" }),
        //        new List<TvItem>(new[] { t1 }),
        //        new List<string>(new[] { "" }));

        //}
       

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
                // Importing Settings from 'Json.settings'
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

                // Add menu entries to handele
                // ReqIF & more
                AddDoMenuImportExportRoundtrip();
            }

                catch (Exception e1)
            {
                MessageBox.Show($@"'{_jasonFilePath}'

{e1}", @"Error loading 'Settings.json'");
            }
        }
        /// <summary>
        /// Add Import/Export/Roundtrip settings *.csv, ReqIF from Settings.json
        /// It shows the menu item if they are plausible: Function, InputFile, RoundtripFile, ExportFile
        /// </summary>
        private void AddDoMenuImportExportRoundtrip()
        {
            if (_importSettings.ImportSettings == null)
            {
                MessageBox.Show($@"Chapter '""Importer"":' is defect or missing in Settings.Json

See File, Settings.json (current)
https://github.com/Helmut-Ortmann/EnterpriseArchitect_hoReverse/wiki/ReqIf

Consider resetting json to factory settings (menu File)

If you don't need to import/export ReqIF & Co, you can ignore this message!!
", @"No 'Import Specification' ReqIF & co is configured!");
            }
            else
            {
                //-------------------------------------------------------------------------------------
                // Importer: Import *.csv, ReqIf from according to specification in Settings.Json
                var importBySettings = (from item in _importSettings.ImportSettings
                    where !String.IsNullOrWhiteSpace(item.InputFile) && 
                          (item.AllowedOperation & FileImportSettingsItem.AllowedOperationsType.Import) == FileImportSettingsItem.AllowedOperationsType.Import
                    orderby item.ListNo.PadLeft(4, '0')
                    select item).Distinct().ToList();


                _doToolStripMenuItem.DropDownItems.Add(new ToolStripSeparator());
                _doToolStripMenuItem.DropDownItems.Add(_importSettings.ConstructImporterMenuItems(
                    importBySettings,
                    //_importSettings.ImportSettings,
                    "Import *.csv, ReqIF",
                    "Import Requirements (*.csv, ReqIF, DOORS ReqIf)",
                    Importer_Click,
                    MenuItemHover_MouseHover, // Optional register the mouse hover event 
                    "Locate Package", MenuItemContext_MouseDown));


                var roundtripBySettings = (from item in _importSettings.ImportSettings
                    where !String.IsNullOrWhiteSpace(item.InputFile) &&
                          !String.IsNullOrWhiteSpace(item.RoundtripFile) &&
                          (item.AllowedOperation & FileImportSettingsItem.AllowedOperationsType.Roundtrip) == FileImportSettingsItem.AllowedOperationsType.Roundtrip &&
                          (item.ImportType == FileImportSettingsItem.ImportTypes.DoorsReqIf ||
                           item.ImportType == FileImportSettingsItem.ImportTypes.ReqIf
                           )
                           orderby item.ListNo.PadLeft(4,'0')
                           select item).Distinct().ToList();

                _doToolStripMenuItem.DropDownItems.Add(_importSettings.ConstructImporterMenuItems(
                    roundtripBySettings,
                    "Roundtrip ReqIF",
                    "Roundtrip Requirements ( ReqIF export roundtrip attributes)",
                    Roundtrip_Click,
                    MenuItemHover_MouseHover, // Optional register the mouse hover event 
                    "Locate Package", MenuItemContext_MouseDown));

                //--------------------------------------------------
                // Add List of Export ReqIF items to do menue
                // Subset of all Import items
                // Filter import settings for to export items 
                // - An input or export file has to be defied
                var exportBySettings = (from item in _importSettings.ImportSettings
                    where (!String.IsNullOrWhiteSpace(item.ExportFile)) &&
                          (item.AllowedOperation & FileImportSettingsItem.AllowedOperationsType.Export) == FileImportSettingsItem.AllowedOperationsType.Export &&
                          (item.ImportType == FileImportSettingsItem.ImportTypes.DoorsReqIf ||
                           item.ImportType == FileImportSettingsItem.ImportTypes.ReqIf)
                           orderby item.ListNo.PadLeft(4, '0')
                           select item).Distinct().ToList();

                _doToolStripMenuItem.DropDownItems.Add(_importSettings.ConstructImporterMenuItems(
                    exportBySettings,
                    "Export ReqIF",
                    "Export Requirements ( ReqIF export all)",
                    Export_Click,
                    MenuItemHover_MouseHover, // Optional register the mouse hover event 
                    "Locate Package", MenuItemContext_MouseDown));
                _doMenuDiagramStyleInserted = true;
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
                HoService.GenerateUseInterfacesFromFile(_repository);
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
        /// <summary>
        /// Generate "Usage" Interface for selected Class/Interface of Diagram node from text input (code snippet)
        /// - Creates/Reuse existing Interfaces 
        /// - Creates a node of the Interface to connect to<param name="sender"></param>
        /// - Make a Usage Connector from Class/Interface to Interface
        /// </summary>
        private void GenerateIncludeForClassifierFromSnippetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                HoService.GenerateUseInterfacesFromInput(_repository,_txtUserText.Text);
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
            HoService.AddElementsToDiagram(_repository, "Note", connectorLinkType: "", bound:false, _txtUserText.Text);
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
            HoService.AddElementsToDiagram(_repository, "Constraint", connectorLinkType: "", bound:false, _txtUserText.Text);
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
        private void Importer_Click(object sender, EventArgs e)
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
        /// Event Export  ReqIF,.. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Roundtrip_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            Debug.Assert(item != null, nameof(item) + " != null");
            if (item?.Tag == null) return;
            FileImportSettingsItem roundtripElement = (FileImportSettingsItem)item.Tag;
            if (int.TryParse(roundtripElement.ListNo, out var listNumber))
                RoundtripBySettings(listNumber, withMessage: true);
            else MessageBox.Show($@"{roundtripElement.ListNo}", $@"ListNo in Importer settings.json invalid");

        }
        /// <summary>
        /// Event Export  ReqIF,.. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Export_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            Debug.Assert(item != null, nameof(item) + " != null");
            if (item?.Tag == null) return;
            FileImportSettingsItem exportElement = (FileImportSettingsItem)item.Tag;
            if (int.TryParse(exportElement.ListNo, out var listNumber))
                ExportBySettings(listNumber, withMessage: true);
            else MessageBox.Show($@"{exportElement.ListNo}", $@"ListNo in Importer settings.json invalid");

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
            string guid = importElement.PackageGuidList[0].Guid;
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
            Text = e.ProgressPercentage.ToString();
        }

        
        private void ShowSymbolDataBaseFoldersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HoUtil.StartApp("explorer.exe", VcDbUtilities.GetVcPathSymbolDataBases());
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
        private void DoorsImportcsvToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EA.ObjectType type = _repository.GetContextItem(out var item);
            if (type != EA.ObjectType.otPackage) return;

            EA.Package pkg = (EA.Package) item;
            
            string filePath = @"c:\ho\ownCloud\shared\BLE_Sens_SWACommaSeparated.csv";
            if (!File.Exists(filePath))
            {
                MessageBox.Show($@"{filePath}", @"*.ReqIF or *.csv to import requirements doesn't exists");
                return;
            }
            
            EnableImportDialog(false);
            Cursor.Current = Cursors.WaitCursor;
            // Generate Requirements
            DoorsCsv doorsModule = new DoorsCsv(_repository, pkg, filePath);
            doorsModule.ImportForFile("Requirement","","");
            EnableImportDialog(true);


            Cursor.Current = Cursors.Default;

        }
        /// <summary>
        /// Import *.csv file with requirements with a file dialog to select the *.csv file into the selected package. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DoorsImportcsvWithFileDialogToolStripMenuItem_Click(object sender, EventArgs e)
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
            doorsModule.ImportForFile("Requirement","","");
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
            DoorsModule doorsModule = new DoorsModule(_repository, pkg);
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
        private void ImportBySettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImportBySettings(1);

  
            MessageBox.Show(@"See File 1, settings for the import definitions.",@"Import DOORS *.csv Requirements finished.");
        }

        private void ImportDoorsReqIFBySettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImportBySettings(2);
            MessageBox.Show(@"See File 2, settings for the import definitions.",@"Import DOORS *.reqIf Requirements finished.");
        }

        

        private void ImportReqIFBySettings4ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImportBySettings(4);
            MessageBox.Show(@"See File 4, settings for the import definitions.",@"Import ReqIf *.reqIf Requirements finished.");
        }
        private void ImportReqIFBySettings5ToolStripMenuItem_Click(object sender, EventArgs e)
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
            List<ReqIfLog> reqIfLogList = new List<ReqIfLog>();
            if (_repository == null || String.IsNullOrEmpty(_repository.ConnectionString))
            {
                MessageBox.Show("", @"No repository loaded, break!!");
                return false;
            }
            DateTime startTime = DateTime.Now;
            Cursor.Current = Cursors.WaitCursor;

            EnableImportDialog(false);
            DoorsModule doorsModule = new DoorsModule(_jasonFilePath, _repository, reqIfLogList);
            bool result = doorsModule.ImportBySetting(listNumber);
            EnableImportDialog(true);
            Cursor.Current = Cursors.Default;

            // finished
            TimeSpan span = DateTime.Now - startTime;
            string duration = $"{span.Minutes}:{span.Seconds} minutes:seconds";
            if (withMessage && result)
                MessageBox.Show($@"Duration: {duration}

See Chapter: 'Importer' in Settings.Json (%APPDATA%ho/../Settings.json)

Clipboard contains the imported Modules/Specifications as csv", $@"{reqIfLogList} Modules/Specifications imported by list={listNumber}, finished.");
            // make a csv
            var del = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator;
            var textCb = (from item in reqIfLogList
                select $@"{item.File}{del}{item.ModuleId}{del}{item.PkgGuid}{del}{item.PkgName}{del}{item.Comment}").ToArray();
            try
            {
                Clipboard.SetText(String.Join(";", textCb));
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch (Exception )
            {
               
            }
           
            return result;
        }
        /// <summary>
        /// Roundtrip by settings
        /// </summary>
        /// <param name="listNumber"></param>
        /// <param name="withMessage"></param>
        private bool RoundtripBySettings(int listNumber, bool withMessage = false)
        {
            if (_repository == null || String.IsNullOrEmpty(_repository.ConnectionString))
            {
                MessageBox.Show("", @"No repository loaded, break!!");
                return false;
            }
            DateTime startTime = DateTime.Now;
            Cursor.Current = Cursors.WaitCursor;

            //EnableImportDialog(false);
            DoorsModule doorsModule = new DoorsModule(_jasonFilePath, _repository);
            bool result = doorsModule.RoundtripBySetting(listNumber);
            //EnableImportDialog(true);


            Cursor.Current = Cursors.Default;
            // finished
            TimeSpan span = DateTime.Now - startTime;
            string duration = $"{span.Minutes}:{span.Seconds} minutes:seconds";
            if (withMessage && result)
                MessageBox.Show($@"Duration: {duration}

See Chapter: 'Importer' in Settings.Json (%APPDATA%ho/../Settings.json)", $@"Roundtrip by list={listNumber}, finished.");
            return result;
        }
        /// <summary>
        /// Export ReqIF by settings
        /// </summary>
        /// <param name="listNumber"></param>
        /// <param name="withMessage"></param>
        private bool ExportBySettings(int listNumber, bool withMessage = false)
        {
            if (_repository == null || String.IsNullOrEmpty(_repository.ConnectionString))
            {
                MessageBox.Show("", @"No repository loaded, break!!");
                return false;
            }
            DateTime startTime = DateTime.Now;
            Cursor.Current = Cursors.WaitCursor;

            //EnableImportDialog(false);
            DoorsModule doorsModule = new DoorsModule(_jasonFilePath, _repository);
            bool result = doorsModule.ExportBySetting(listNumber);


            //EnableImportDialog(true);
            Cursor.Current = Cursors.Default;
            // finished
            TimeSpan span = DateTime.Now - startTime;
            string duration = $"{span.Minutes}:{span.Seconds} minutes:seconds";
            if (withMessage && result)
                MessageBox.Show($@"Duration: {duration}

See Chapter: 'Importer' in Settings.Json (%APPDATA%ho/../Settings.json)", $@"Exported by list={listNumber}, finished.");
            return result;
        }


        private void ImportReqIFBySettings3ToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
           ImportBySettings(3);
            MessageBox.Show(@"See File 3, settings for the import definitions.",@"Import ReqIf *.reqIf Requirements finished.");

        }

        private void HoToolsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WikiRef.WikiHoTools();
        }

        private void SQLWildcardsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WikiRef.Wildcards();
        }

        private void ReqIFToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WikiRef.ReqIF();
        }

        private void InfoReqIfInquiryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InquiryReqIF(false);
        }
        /// <summary>
        /// Inquiry a reqIF file, choose ReqIF file/directory by Dialog
        /// </summary>
        /// <param name="validate"></param>
        private void InquiryReqIF(bool validate=false)
        {
            string folderSelection = "Folder Selection (click no file)";
            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                //InitialDirectory = @"D:\",
                Title = @"Input *.reqifz file or folder with *.reqifz/*.reqif files",

                ValidateNames = false,
                CheckFileExists = false,
                CheckPathExists = true,
                FileName = folderSelection,

            // DefaultExt = "reqifz",
            Filter = @"Reqifz files(*.reqifz, *.reqif)|*.reqifz;*.reqif| Folders|*|All files(*.*)|*.*",
            FilterIndex = 1,
            RestoreDirectory = true,

                ReadOnlyChecked = false,
                ShowReadOnly = false
            };

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Cursor.Current = Cursors.WaitCursor;
                string file = openFileDialog1.FileName;

                // folder selected
                if (file.Contains(folderSelection)) file = Path.GetDirectoryName(file);

                DataTable dt = ReqIfInventory.Inventory(file, validate);
                string xml = dt == null ? Xml.MakeEmptyXml() : Xml.MakeXmlFromDataTable(dt);
                _repository.RunModelSearch("", "", "", xml);

                //var csv = dt.Rows.Cast<DataRow>().Select(
                //                 x=>new {
                //                            File = x.Field<string>("File") , 
                //                            Id =x.Field<string>("Id")
                //                     }
                //                 )
                //    .ToDelimitedString(System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator);
                //Clipboard.SetText(csv);
                
               Cursor.Current = Cursors.Default;
            }
        }

        private void InfoReqIfInquiryValidationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InquiryReqIF(true);
        }

        /// <summary>
        /// Move the selected diagram elements to the selected package or element in browser
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void moveDiagramElementToToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HoService.DiagramObjectMove(_repository);
            
        }
    }
}

