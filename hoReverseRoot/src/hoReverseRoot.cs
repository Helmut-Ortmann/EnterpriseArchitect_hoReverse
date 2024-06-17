using System;
using System.Windows.Forms;
using System.Collections.Generic;
using hoReverse.Reverse;
using hoReverse.Settings;
using hoReverse.HistoryList;
using hoReverse.History;
using hoReverse.Services;
using hoReverse.hoUtils;
using hoReverse.hoUtils.Appls;
using hoReverse.hoUtils.ActionPins;
using GlobalHotkeys;

namespace hoReverse
{
    // ReSharper disable once ClassNeverInstantiated.Global
    // ReSharper disable once InconsistentNaming
    public partial class hoReverseRoot : EAAddinFramework.EAAddinBase
    {
        //string logInUser = null;
        //UserRights logInUserRights = UserRights.ADMIN;
        //enum UserRights
        //{
        //    STANDARD,
        //    ADMIN
        //}
        public static string Release = "3.14.0"; // not overwritten by EA_OnPostInitialized
        // EA Addin specification in registry:
        // Key:   hoReverse
        // Value: hoReverse.ReverseRoot
        private string _progId = "hoReverse.ReverseRoot";


        EaHistory _history;// diagram history
        EaBookmark _bookmark; // bookmarks

        static HoReverseGui _hoReverseGuiStatic;
        HoReverseGui _hoReverseGui;
        HistoryGui _historyControl;
        HistoryGui _bookmarkControl;

        static AddinSettings _addinSettingsStatic;
        readonly AddinSettings _addinSettings;
        EA.Repository _repository;
        static EA.Repository _repositoryStatic;

        // define menu constants
        // ReSharper disable once UnusedMember.Local
        const string MenuName = "-hoReverseWB";

        const string MenuShowWindow = "Show Window";

        const string MenuDeviderVc = "------------- Version Control --------------------------";
        const string MenuGetAllLatest = "Get All Latest (recursive)";
        const string MenuSetXmlPath = "Set XML path for controlled package";
        const string MenuSetSvnPackageTaggedValues = "Set svn Package Tagged Values";
        const string MenuSetSvnPackageTaggedValuesRecursive = "Set svn Package Tagged Values (recursive)";
        const string MenuSetSvnKeywords = "Set svn keywords";
        const string MenuShowTortoiseLog = "Show Tortois Log";
        const string MenuShowTortoiseRepoBrowser = "Show Tortois Repo Browser";

        const string MenuDeviderMiscellaneous = "------------- Miscellaneous  ---------------------------";
        
        const string MenuShowFolder = "Show folder";
        const string MenuCopyGuidToClipboard = "Copy GUID / Select Statement to Clipboard";
        const string MenuUpdateOperationTypes = "Update operation types";

        const string MenuDisplayBehavior = "Display Behavior";
        const string MenuDisplayMethodDefinition = "Locate Operation";
        const string MenuLocateType = "Locate Type";
        const string MenuLocateCompositeElementorDiagram = "Locate CompositeElementOfDiagram";
        const string MenuLocateCompositeDiagramOfElement = "Locate CompositeDiagramOfElement";
        const string MenuShowSpecification = "Show Specification";
        const string MenuUnlockDiagram = "UnlockDiagram";


        // ReSharper disable once UnusedMember.Local
        const string MenuDeviderLineStyleDia = "---------------Line style Diagram-----------------";
        const string MenuLineStyleDiaLh = "Line Style Diagram (Object): Lateral Horizontal";
        const string MenuLineStyleDiaLv = "Line Style Diagram (Object): Lateral Vertical";
        const string MenuLineStyleDiaTh = "Line Style Diagram (Object): Tree Horizontal";
        const string MenuLineStyleDiaTv = "Line Style Diagram (Object): Tree Vertical";
        const string MenuLineStyleDiaOs = "Line Style Diagram (Object): Orthogonal Square";

        const string MenuDeviderActivity = "-------------Create Activity for Operation ---------------------------";
        const string MenuCreateActivityForOperation = "Create Activity for Operation (select operation or class)";
        const string MenuUpdateOperationParameter = "Update Operation Parameter for Activity and Operation (select Package(recursive), Activity, Class, Interface or Operation)";
        const string MenuUpdateActionPin = "Update Action Pin for Package (recursive)";

        const string MenuDeviderInteraction = "-------------Create Interaction for Operation ---------------------------";
        const string MenuCreateInteractionForOperation = "&Create Interaction for Operation (select operation or class)  ";

        const string MenuDeviderStateMachine = "-------------Create Statemachine for Operation ---------------------------";
        const string MenuCreateStateMachineForOperation = "&Create Statemachine for Operation (select operation)  ";


        // ReSharper disable once UnusedMember.Local
        const string MenuCorrectTypes = "-------------Correct Type ---------------------------";
        const string MenuCorrectType = "Correct types of Attribute, Function (selected Attribute, Function, Class or Package)";

        const string MenuDeviderCopyPast = "-------------Move links---------------------------";
        const string MenuCopyLinksToClipboard = "Copy Links to Clipboard";
        const string MenuPasteLinksFromClipboard = "Paste Links from Clipboard";

        const string MenuDeviderNote = "-------------Note      ---------------------------";
        const string MenuAddLinkedNote = "Add linked Note";
        const string MenuAddLinkedDiagramNote = "Add linked Diagram Note";

        const string MenuUsage = "Find Usage";
        const string MenuAbout = "About";
        const string MenuHelp = "Help";
        const string MenuDevider = "-----------------------------------------------";


        /// <summary>
        /// constructor where we set the menuheader and menuOptions
        /// </summary>
        public hoReverseRoot()
        {
            try
            {
                _addinSettings = new AddinSettings();
                _addinSettingsStatic = _addinSettings;
                if (_addinSettings == null) MessageBox.Show(@"user.config couldn't been installed!", @"Installation error");
            } catch (Exception e)
            {
                MessageBox.Show(@"Error initializing settings:"+ e);
            }
            menuHeader = "-" + _addinSettings.ProductName;
            menuOptions = new string[] { 
                //-------------------------- General  --------------------------//
                //    menuLocateCompositeElementorDiagram,
                //-------------------------- LineStyle --------------------------//
                                        
                //-------------------------- Activity --------------------------//
                MenuDeviderActivity,
                MenuCreateActivityForOperation, MenuUpdateOperationParameter, 
                //menuUpdateActionPin,
                //-------------------------- Interaction --------------------------//
                MenuDeviderInteraction,
                MenuCreateInteractionForOperation,
                //-------------------------- Interaction --------------------------//
                MenuDeviderStateMachine,
                MenuCreateStateMachineForOperation,
                //-------------------------- Correct Types ------------------------//
                //menuCorrectTypes, menuCorrectType, 
                //-------------------------- Add Note -----------------------------//
                MenuDeviderNote,
                MenuAddLinkedNote,MenuAddLinkedDiagramNote,
                //--------------------------- Miscellaneous ----------------------------------//
                MenuDeviderMiscellaneous,
                MenuShowFolder,
                MenuCopyGuidToClipboard,
                //menuUpdateOperationTypes,
                

                //---------------------------- VC ------------------------------------------//
                MenuDeviderVc,
                MenuGetAllLatest,
                MenuSetXmlPath,
                MenuSetSvnPackageTaggedValues,
                MenuSetSvnPackageTaggedValuesRecursive,
                MenuSetSvnKeywords, 
                MenuShowTortoiseLog,
                MenuShowTortoiseRepoBrowser, 
 
                //-------------------------- Move links ---------------------------//
                MenuDeviderCopyPast,
                MenuCopyLinksToClipboard, MenuPasteLinksFromClipboard, 

                MenuShowWindow,    
                //---------------------------- About -------------------------------//
                MenuDevider, MenuAbout, MenuHelp };
        }
        /// <summary>
        /// EA_Connect events enable Add-Ins to identify their type and to respond to Enterprise Architect start up.
        /// This event occurs when Enterprise Architect first loads your Add-In. Enterprise Architect itself is loading at this time so that while a Repository object is supplied, there is limited information that you can extract from it.
        /// The chief uses for EA_Connect are in initializing global Add-In data and for identifying the Add-In as an MDG Add-In.
        /// Also look at EA_Disconnect.
        /// </summary>
        /// <param name="repository">An EA.Repository object representing the currently open Enterprise Architect model.
        /// Poll its members to retrieve model data and user interface status information.</param>
        /// <returns>String identifying a specialized type of Add-In: 
        /// - "MDG" : MDG Add-Ins receive MDG Events and extra menu options.
        /// - "" : None-specialized Add-In.</returns>
        public override string EA_Connect(EA.Repository repository)
        {
            HotkeyHandlers.SetupGlobalHotkeys();
            if (repository.IsSecurityEnabled)
            {
                //logInUser = Repository.GetCurrentLoginUser(false);
                //if ((logInUser.Contains("deexortm")) ||
                //     (logInUser.Contains("admin")) ||
                //     (logInUser.Equals(""))
                //    ) logInUserRights = UserRights.ADMIN;
            }
            return "a string";
        }
        internal static class HotkeyHandlers
        {
            public static void SetupGlobalHotkeys()
            {
                List<Hotkey> hotkeys = new List<Hotkey>
            {
                //new Hotkey(Keys.J, Modifiers.Ctrl | Modifiers.Shift, HandleHotKey1),
                //new Hotkey(Keys.S, Modifiers.Ctrl | Modifiers.Shift, HandleHotKey2),
                //new Hotkey(Keys.H, Modifiers.Ctrl | Modifiers.Shift, HandleHotKey3),
                //new Hotkey(Keys.V, Modifiers.Ctrl | Modifiers.Shift, HandleHotKey4),
                //new Hotkey(Keys.S, Modifiers.Ctrl | Modifiers.Win | Modifiers.Alt, HandleHotKey2),
                //new Hotkey(Keys.A, Modifiers.NoMod, HandleHotKey2)
            };
                Dictionary<string, Keys> keys = GlobalKeysConfig.GetKeys();
                Dictionary<string, Modifiers> modifiers = GlobalKeysConfig.GetModifiers();
                Keys key;
                Modifiers modifier1;
                Modifiers modifier2;
                Modifiers modifier3;
                Modifiers modifier4;

                for (int i = 0; i < _addinSettingsStatic.GlobalSearchKeys.Count; i=i+1 )
                {
                    GlobalKeysConfig.GlobalKeysSearchConfig search = _addinSettingsStatic.GlobalSearchKeys[i];
                    if (search.Key != "None" & search.SearchName != "")
                    {
                        keys.TryGetValue(search.Key, out key);
                        modifiers.TryGetValue(search.Modifier1, out modifier1);
                        modifiers.TryGetValue(search.Modifier2, out modifier2);
                        modifiers.TryGetValue(search.Modifier3, out modifier3);
                        modifiers.TryGetValue(search.Modifier4, out modifier4);
                        switch (i)
                        {
                            case 0:
                                hotkeys.Add(new Hotkey(key, modifier1 | modifier2 | modifier3 | modifier4, HandleGlobalKeySearch0));
                                break;
                            case 1:
                                hotkeys.Add(new Hotkey(key, modifier1 | modifier2 | modifier3 | modifier4, HandleGlobalKeySearch1));
                                break;
                            case 2:
                                hotkeys.Add(new Hotkey(key, modifier1 | modifier2 | modifier3 | modifier4, HandleGlobalKeySearch2));
                                break;
                            case 3:
                                hotkeys.Add(new Hotkey(key, modifier1 | modifier2 | modifier3 | modifier4, HandleGlobalKeySearch3));
                                break;
                            case 4:
                                hotkeys.Add(new Hotkey(key, modifier1 | modifier2 | modifier3 | modifier4, HandleGlobalKeySearch4));
                                break;
                        }
                    }

                }
                for (int i = 0; i < _addinSettingsStatic.GlobalServiceKeys.Count; i = i + 1)
                {
                    GlobalKeysConfig.GlobalKeysServiceConfig service = _addinSettingsStatic.GlobalServiceKeys[i];
                    if (service.Key != "None" & service.Guid != "")
                    {
                        keys.TryGetValue(service.Key, out key);
                        modifiers.TryGetValue(service.Modifier1, out modifier1);
                        modifiers.TryGetValue(service.Modifier2, out modifier2);
                        modifiers.TryGetValue(service.Modifier3, out modifier3);
                        modifiers.TryGetValue(service.Modifier4, out modifier4);
                        switch (i)
                        {
                            case 0:
                                hotkeys.Add(new Hotkey(key, modifier1 | modifier2 | modifier3 | modifier4, HandleGlobalKeyService0));
                                break;
                            case 1:
                                hotkeys.Add(new Hotkey(key, modifier1 | modifier2 | modifier3 | modifier4, HandleGlobalKeyService1));
                                break;
                            case 2:
                                hotkeys.Add(new Hotkey(key, modifier1 | modifier2 | modifier3 | modifier4, HandleGlobalKeyService2));
                                break;
                            case 3:
                                hotkeys.Add(new Hotkey(key, modifier1 | modifier2 | modifier3 | modifier4, HandleGlobalKeyService3));
                                break;
                            case 4:
                                hotkeys.Add(new Hotkey(key, modifier1 | modifier2 | modifier3 | modifier4, HandleGlobalKeyService4));
                                break;
                        }
                    }

                }
                Form hotkeyForm = new InvisibleHotKeyForm(hotkeys);
                hotkeyForm.Show();
            }

            private static void RunGlobalKeySearch(int pos)
            {
                
                    GlobalKeysConfig.GlobalKeysSearchConfig sh = _addinSettingsStatic.GlobalSearchKeys[pos];
                    if (sh.SearchName == "") return;
                    try
                    {
                        _repositoryStatic.RunModelSearch(sh.SearchName, sh.SearchTerm, "", "");
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.ToString(), @"Error start search '" + sh.SearchName +
                           @" " + sh.SearchTerm + @"'");
                    }
            }

            private static void RunGlobalKeyService(int pos)
            {
                GlobalKeysConfig.GlobalKeysServiceConfig sh = _addinSettingsStatic.GlobalServiceKeys[pos];
                    if (sh.Method == null) return;
                    sh.Invoke(_repositoryStatic, _hoReverseGuiStatic.GetText());
            }

            private static void HandleGlobalKeySearch0()
            {
                RunGlobalKeySearch(0); 
            }
            private static void HandleGlobalKeySearch1()
            {
                RunGlobalKeySearch(1);
            }
            private static void HandleGlobalKeySearch2()
            {
                RunGlobalKeySearch(2);
            }
            private static void HandleGlobalKeySearch3()
            {
                RunGlobalKeySearch(3);
            }
            private static void HandleGlobalKeySearch4()
            {
                RunGlobalKeySearch(4);
            }
            private static void HandleGlobalKeyService0()
            {
                RunGlobalKeyService(0);
            }
            private static void HandleGlobalKeyService1()
            {
                RunGlobalKeyService(1);
            }
            private static void HandleGlobalKeyService2()
            {
                RunGlobalKeyService(2);
            }
            private static void HandleGlobalKeyService3()
            {
                RunGlobalKeyService(3);
            }
            private static void HandleGlobalKeyService4()
            {
                RunGlobalKeyService(4);
            }
                      
        }


        public override void EA_OnPostInitialized(EA.Repository rep)
        {

             // Release =  FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
             _repository = rep;
             _repositoryStatic = rep;
             ShowWindow();
        }

        /// <summary>
        /// EA_OnPostNewConnector notifies Add-Ins that a new connector has been created on a diagram. It enables Add-Ins to modify the connector upon creation.
        /// This event occurs after a user has dragged a new connector from the Toolbox or Resources window onto a diagram. The notification is provided immediately after the connector is added to the model. Set Repository.SuppressEADialogs to true to suppress Enterprise Architect from showing its default dialogs.
        /// Also look at EA_OnPreNewConnector.
        /// Note: By script created connectors don't call this event
        /// </summary>
        /// <param name="repository">An EA.Repository object representing the currently open Enterprise Architect model.
        /// Poll its members to retrieve model data and user interface status information.</param>
        /// <param name="eventProperties"></param>
        /// <returns>Return True if the connector has been updated during this notification. Return False otherwise.</returns>
        public override bool EA_OnPostNewConnector(EA.Repository repository, EA.EventProperties eventProperties)
        {
            EA.EventProperty eventProperty = eventProperties.Get(0);
            string s = (string)eventProperty.Value;
            int connectorId = int.Parse(s);
            string lineStyle;
            EA.Diagram dia = repository.GetCurrentDiagram();
            if (dia == null) return false;  // e.g. relationship matrix
            switch (dia.Type) 
            {
                case "Activity":
                    HoUtil.GetDiagramLinkFromConnector(dia, connectorId);
                    lineStyle = _addinSettings.ActivityLineStyle.Substring(0, 2).ToUpper();
                    if (lineStyle == "NO" ) return false;

                    // get connector source
                    var c = _repository.GetConnectorByID(connectorId);
                    EA.Element el = _repository.GetElementByID(c.ClientID);
                    if ( el.Name != "" & el.Type == "Decision")
                    {
                        HoUtil.SetConnectorGuard(_repository, connectorId, "no");
                    }

                    HoService.SetLineStyle(repository, lineStyle);
                    return true;

                case "Statechart":
                    HoUtil.GetDiagramLinkFromConnector(dia, connectorId);
                    lineStyle = _addinSettings.StatechartLineStyle.Substring(0, 2).ToUpper();
                    if (lineStyle == "NO" ) return false;
                    HoService.SetLineStyle(repository, lineStyle);
                    return true;

                case "Logical":
                    //return updateLineStyle(Repository, dia, connectorID, _AddinSettings.StatechartLineStyle.Substring(0, 2));
                    return false;

                case "Custom":
                    //return updateLineStyle(Repository, dia, connectorID, _AddinSettings.StatechartLineStyle.Substring(0, 2));
                    return false;

                case "Component":
                    //return updateLineStyle(Repository, dia, connectorID, _AddinSettings.StatechartLineStyle.Substring(0, 2));
                    return false;

                case "Deployment":
                    //return updateLineStyle(Repository, dia, connectorID, _AddinSettings.StatechartLineStyle.Substring(0, 2));
                    return false;

                case "Package":
                    //return updateLineStyle(Repository, dia, connectorID, _AddinSettings.StatechartLineStyle.Substring(0, 2));
                    return false;

                case "UseCase":
                    return false;

                default:
                    return false;

            }

        }

       public override void EA_FileOpen(EA.Repository repository)
        {
            InitializeForRepository(repository);
            // open last diagram
            EaHistoryListEntry entry = _history?.GetLatest();
            if (entry != null)
                Reverse.HoReverseGui.OpenDiagram(_repository, entry.Guid, true);
            //base.EA_FileOpen(Repository);
        }
        public override void EA_FileClose(EA.Repository repository)
        {
            _hoReverseGui.Save(); // save settings
            _repository = null;
            _history = null;  
            _bookmark = null;
            
          
        }

        private void InitializeForRepository(EA.Repository rep)
        {
            _repository = rep;
            _repositoryStatic = rep;
            _hoReverseGui.Repository = rep;
            if (_addinSettings.ShowHistory)
            {
                _history = new EaHistory(_repository, _addinSettings);
                _hoReverseGui.History = _history;
                // initialize diagram history
                _historyControl.setRepository(_repositoryStatic);
                _historyControl.setHistory(_history);
                _historyControl.show();
            }
            if (_addinSettings.ShowBookmark)
            {
                _bookmark = new EaBookmark(_repository, _addinSettings);
                _hoReverseGui.Bookmark = _bookmark;
                // initialize bookmark history
                _bookmarkControl.setRepository(_repositoryStatic);
                _bookmarkControl.setHistory(_bookmark);
                _bookmarkControl.show();
            }
        }
        /// <summary>
        /// called when the selected item changes
        /// This operation will show the guid of the selected element in the eaControl
        /// </summary>
        /// <param name="repository">the EA.Repository</param>
        /// <param name="guid">the guid of the selected item</param>
        /// <param name="ot">the object type of the selected item</param>
        public override void EA_OnContextItemChanged(EA.Repository repository, string guid, EA.ObjectType ot)
        {
            

        }
        //-----------------------------------------------------------------------------
        // a diagram has been opened
        //-----------------------------------------------------------------------------
        public override void EA_OnPostOpenDiagram(EA.Repository repository, int diagramId)
        {
            //initializeForRepository(Repository);
            if (_history == null) return; // not initialized
            EA.Diagram dia = _repository.GetDiagramByID(diagramId);
            _history.Add(EA.ObjectType.otDiagram, dia.DiagramGUID);

        }


        
        /// <summary>
        /// Called once Menu has been opened to see what menu items should active.
        /// </summary>
        /// <param name="repository">the repository</param>
        /// <param name="location">the location of the menu</param>
        /// <param name="menuName">the name of the menu</param>
        /// <param name="itemName">the name of the menu item</param>
        /// <param name="isEnabled">boolean indicating whethe the menu item is enabled</param>
        /// <param name="isChecked">boolean indicating whether the menu is checked</param>
        public override void EA_GetMenuState(EA.Repository repository, string location, string menuName, string itemName, ref bool isEnabled, ref bool isChecked)
        {
            if (IsProjectOpen(repository))
            {
                switch (itemName)
                {

                    case     MenuUpdateOperationTypes:
                        isChecked = false;
                        break;

                    case MenuHelp:
                        isChecked = false;
                        break;

                    case MenuSetXmlPath:
                        isChecked = false;
                        break;

                    case MenuGetAllLatest:
                        isChecked = false;
                        break;

                    case MenuSetSvnPackageTaggedValues:
                        isChecked = false;
                        break;

                    case MenuSetSvnPackageTaggedValuesRecursive:
                        isChecked = false;
                        break;

                    case MenuSetSvnKeywords:
                        isChecked = false;
                        break;

                    case MenuShowTortoiseLog:
                        isChecked = false;
                        break;

                    case MenuShowTortoiseRepoBrowser:
                        isChecked = false;
                        break;


                    case MenuShowFolder:
                        isChecked = false;
                        break;

                    case MenuCopyGuidToClipboard:
                        isChecked = false;
                        break;
                        
                    case MenuShowWindow:
                        isChecked = false;
                        break;
                    case MenuShowSpecification:
                        isChecked = false;
                        break;

                    case MenuUnlockDiagram:
                        isChecked = false;
                        break;

                    case MenuLineStyleDiaTh:
                        isChecked = false;
                        break;
                    case MenuLineStyleDiaTv:
                        isChecked = false;
                        break;
                    case MenuLineStyleDiaLh:
                        isChecked = false;
                        break;
                    case MenuLineStyleDiaLv:
                        isChecked = false;
                        break;
                    case MenuLineStyleDiaOs:
                        isChecked = false;
                        break;

                    case MenuLocateCompositeElementorDiagram:
                        isChecked = false;
                        break;

                    case MenuLocateCompositeDiagramOfElement:
                        isChecked = false;
                        break;


                    case MenuUsage:
                        isChecked = false;
                        break;

                    case MenuCreateInteractionForOperation:
                        isChecked = false;
                        break;

                    case MenuCreateStateMachineForOperation:
                        isChecked = false;
                        break;

                    case MenuCorrectType:
                        isChecked = false;
                        break;

                    case MenuDisplayBehavior:
                        isChecked = false;
                        break;


                    case MenuUpdateActionPin:
                        isChecked = false;
                        break;

                    case MenuUpdateOperationParameter:
                        isChecked = false;
                        break;

                    case MenuCreateActivityForOperation:
                        isChecked = false;
                        break;

                    case MenuDisplayMethodDefinition:
                        isChecked = false;
                        break;


                    case MenuLocateType:
                        isChecked = false;
                        break;


                    case MenuCopyLinksToClipboard:
                        isChecked = false;
                        break;

                    case MenuPasteLinksFromClipboard:
                        isChecked = false;
                        break;


                    case MenuAddLinkedNote:
                        isChecked = false;
                        break;

                    case MenuAddLinkedDiagramNote:
                        isChecked = false;
                        break;

                    case MenuAbout:
                        isChecked = false;
                        break;

                    // there shouldn't be any other, but just in case disable it.
                    default:
                        isEnabled = false;
                        break;
                }
            }
            else
            {
                // If no open project, disable all menu options
                isEnabled = false;
            }
        }

        /// <summary>
        /// Called when user makes a selection in the menu.
        /// This is your main exit point to the rest of your Add-in
        /// </summary>
        /// <param name="repository">the repository</param>
        /// <param name="location">the location of the menu</param>
        /// <param name="menuName">the name of the menu</param>
        /// <param name="itemName">the name of the selected menu item</param>
        public override void EA_MenuClick(EA.Repository repository, string location, string menuName, string itemName)
        {
            EA.ObjectType oType = repository.GetContextItemType();
            EA.Diagram diaCurrent = repository.GetCurrentDiagram();
            EA.Element el;
            EA.Package pkg;

            if (diaCurrent != null)
            {
            }
            switch (itemName)
            {

                case MenuShowWindow:
                    ShowWindow();
                    break;

                case MenuUpdateOperationTypes:
                    HoService.ReconcileOperationTypesWrapper(repository);
                    break;

                case MenuSetXmlPath:
                    HoService.SetNewXmlPath(repository);
                    break;

                case  MenuGetAllLatest:
                    HoService.GetVcLatestRecursive(repository);
                     break;

                case MenuSetSvnPackageTaggedValues:
                    if (!oType.Equals(EA.ObjectType.otPackage)) return;

                     pkg = (EA.Package)repository.GetContextObject();
                     HoService.SetDirectoryTaggedValues(_repository, pkg);
                     break;

                case MenuSetSvnPackageTaggedValuesRecursive:
                     HoService.SetTaggedValueGui(repository);
                     break;

                case MenuSetSvnKeywords:
                    if (!oType.Equals(EA.ObjectType.otPackage)) return;

                    pkg = (EA.Package)repository.GetContextObject();
                    HoService.SetSvnProperty(_repository, pkg);
                     break;
                
                case MenuShowTortoiseLog:
                     if (!oType.Equals(EA.ObjectType.otPackage)) return;

                    pkg = (EA.Package)repository.GetContextObject();
                    HoService.GotoSvnLog(_repository, pkg);
                     break;

                case MenuShowTortoiseRepoBrowser:
                     if (!oType.Equals(EA.ObjectType.otPackage)) return;

                    pkg = (EA.Package)repository.GetContextObject();
                    HoService.GotoSvnBrowser(_repository, pkg);
                     break;


                case MenuShowFolder:
                     HoService.ShowFolder(_repository, isTotalCommander: _addinSettings.FileManagerIsTotalCommander);
                     break;

                case MenuAbout:
                    ShowAbout();
                    break;

                case MenuHelp:
                    HoUtil.StartFile(HoService.GetAssemblyPath() + "\\" + "hoReverse.chm");
                    break;

                // Line style: Lateral Horizontal 
                case MenuLineStyleDiaLh:
                    HoService.SetLineStyle(repository, "LH");

                    break;
                // Line style: Lateral Vertical 
                case MenuLineStyleDiaLv:
                    // all connections of diagram
                    HoService.SetLineStyle(repository, "LV");
                    break;
                // Line style: Tree Vertical 
                case MenuLineStyleDiaTv:
                    HoService.SetLineStyle(repository, "V");

                    break;

                // Line style: Tree Horizental 
                case MenuLineStyleDiaTh:
                    HoService.SetLineStyle(repository, "H");

                    break;
                // Line style: Orthogonal square 
                case MenuLineStyleDiaOs:
                    HoService.SetLineStyle(repository, "OS");

                    break;


                //if (ItemName == menuHelp)
                //{
                //    Help fHelp = new Help();
                //    fHelp.ShowDialog();
                //    return;
                //}
                case MenuUnlockDiagram:
                    if (oType.Equals(EA.ObjectType.otDiagram))
                    {
                        try
                        {
                            string sql = @"update t_diagram set locked = 0" +
                           " where diagram_ID = " + diaCurrent.DiagramID.ToString();
                            repository.Execute(sql);
                            // reload view
                            repository.ReloadDiagram(diaCurrent.DiagramID);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }

                    break;

                // Start specification (file parameter)
                case MenuShowSpecification:
                    HoService.ShowSpecification(repository);

                    break;

                // Create Interaction for selected operation or class (all operations)
                case MenuCreateInteractionForOperation:
                    // Check selected Elements in tree
                    if (oType.Equals(EA.ObjectType.otMethod))
                    {
                        EA.Method m = (EA.Method)repository.GetContextObject();
                        // test multiple selection

                        // Create Activity
                        Appl.CreateInteractionForOperation(repository, m);

                    }
                    if (oType.Equals(EA.ObjectType.otElement))
                    {
                        EA.Element cls = (EA.Element)repository.GetContextObject();
                        // over all operations of class
                        foreach (EA.Method m in cls.Methods)
                        {
                            // Create Activity
                            Appl.CreateInteractionForOperation(repository, m);

                        }
                    }

                    break;

                // Create Interaction for selected operation or class (all operations)
                case MenuCreateStateMachineForOperation:
                    // Check selected Elements in tree
                    if (oType.Equals(EA.ObjectType.otMethod))
                    {
                        EA.Method m = (EA.Method)repository.GetContextObject();
                        // test multiple selection

                        // Create State Machine
                        Appl.CreateStateMachineForOperation(repository, m);

                    }
                    break;



                case MenuLocateCompositeElementorDiagram:
                    HoService.NavigateComposite(repository);
                    break;

                // 
                case MenuCorrectType:
                    if (oType.Equals(EA.ObjectType.otAttribute))
                    {
                        EA.Attribute a = (EA.Attribute)repository.GetContextObject();

                        HoUtil.UpdateAttribute(repository, a);
                    }

                    if (oType.Equals(EA.ObjectType.otMethod))
                    {
                        EA.Method m = (EA.Method)repository.GetContextObject();

                        HoUtil.UpdateMethod(repository, m);
                    }
                    if (oType.Equals(EA.ObjectType.otElement))
                    {
                        el = (EA.Element)repository.GetContextObject();
                        HoUtil.UpdateClass(repository, el);
                    }
                    if (oType.Equals(EA.ObjectType.otPackage))
                    {
                        pkg = (EA.Package)repository.GetContextObject();
                        HoUtil.UpdatePackage(repository, pkg);
                    }
                    break;


                case MenuCreateActivityForOperation:
                    
                    try
                    {
                        Cursor.Current = Cursors.WaitCursor;
                        HoService.ReconcileOperationTypesWrapper(_repository);
                        HoService.CreateActivityForOperation(repository);
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
                    break;

                // get Parameter for Activity
                case MenuUpdateOperationParameter:
                    HoService.ReconcileOperationTypesWrapper(_repository);
                    HoService.UpdateActivityParameter(repository);
                    try
                    {
                        Cursor.Current = Cursors.WaitCursor;
                        HoService.ReconcileOperationTypesWrapper(_repository);
                        HoService.UpdateActivityParameter(repository);
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
                    break;

                case MenuUpdateActionPin:
                    if (oType.Equals(EA.ObjectType.otPackage))
                    {
                        pkg = (EA.Package)repository.GetContextObject();
                        ActionPin.UpdateActionPinForPackage(repository, pkg);
                    }
                    if (oType.Equals(EA.ObjectType.otElement))
                    {
                        el = (EA.Element)repository.GetContextObject();
                        ActionPin.UpdateActionPinForElement(repository, el);
                    }
                    break;


                case MenuAddLinkedDiagramNote:
                    HoService.AddDiagramNote(repository);

                    break;

                case MenuAddLinkedNote:
                    HoService.AddElementNote(repository);

                    break;

                case MenuLocateType:
                    HoService.LocateType(repository);

                    break;

                case MenuUsage:
                    HoService.FindUsage(repository);

                    break;

                case MenuPasteLinksFromClipboard:
                    if (oType.Equals(EA.ObjectType.otElement)) // only Element
                    {
                        el = (EA.Element)repository.GetContextObject();
                        string conStr = Clipboard.GetText();  // get Clippboard
                        if (conStr.StartsWith("{") && conStr.Substring(37,1)=="}" && conStr.EndsWith("\r\n")) {
                            repository.CreateOutputTab("DEBUG");
                            repository.EnsureOutputVisible("DEBUG");
                            int countError = 0;
                            int countInserted = 0;
                            string[] conStrList = conStr.Split('\n');
                            foreach (string line in conStrList)
                            {
                                if (line.Length > 38)
                                {
                                    string guid = line.Substring(0, 38);
                                    EA.Connector con = repository.GetConnectorByGuid(guid);

                                    // Client/Source
                                    if (line.Substring(38, 1) == "C")
                                    {
                                        try
                                        {
                                            con.ClientID = el.ElementID;
                                            con.Update();
                                            countInserted = countInserted + 1;
                                        }
                                        catch
                                        {
                                            countError = countError + 1;
                                            EA.Element el1 = repository.GetElementByID(con.SupplierID);
                                            string fText = $"Error Name {el1.Name}, Error={con.GetLastError()}";
                                            repository.WriteOutput("Debug", fText, 0);
                                        }
                                    }
                                    //Supplier / Target
                                    if (line.Substring(38, 1) == "S")
                                    {
                                        try
                                        {

                                            con.SupplierID = el.ElementID;
                                            con.Update();
                                            countInserted = countInserted + 1;
                                        }
                                        catch
                                        {
                                            countError = countError + 1;
                                            EA.Element el1 = repository.GetElementByID(con.ClientID);
                                            string fText = $"Error Name {el1.Name}, Error={con.GetLastError()}";
                                            repository.WriteOutput("Debug", fText, 0);
                                        }

                                    }
                                }
                            }
                            // update diagram
                            EA.Diagram dia = repository.GetCurrentDiagram();
                            if (dia != null)
                            {
                                try
                                {
                                    dia.Update();
                                    repository.ReloadDiagram(dia.DiagramID);
                                }
                                // ReSharper disable once EmptyGeneralCatchClause
                                catch
                                {
                                }

                            }
                            MessageBox.Show($@"Kopiert:{countInserted}
Errors:{countError}");
                        }



                    }
                    break;

                case MenuCopyGuidToClipboard:
                    HoService.CopyGuidSqlToClipboard(repository);
                    break;


                // put on Clipboard
                // 'ConnectorGUID', 'Client' if element is a client/source in this dependency
                // 'ConnectorGUID', 'Supplier' if element is a supplier/target in this dependency

                case MenuCopyLinksToClipboard:
                    if (oType.Equals(EA.ObjectType.otElement)) // only Element
                    {
                        el = (EA.Element)repository.GetContextObject();
                        string conStr = "";
                        foreach (EA.Connector con in el.Connectors)
                        {
                            conStr = conStr + con.ConnectorGUID;
                            // check if client or supllier
                            if (con.ClientID == el.ElementID) conStr = conStr + "Client  \r\n";
                            if (con.SupplierID == el.ElementID) conStr = conStr + "Supplier\r\n";

                        }
                        if (conStr.Length > 0)
                        {
                            Clipboard.SetText(conStr);
                        }
                    }
                    break;

                case MenuDisplayMethodDefinition:
                    HoService.DisplayOperationForSelectedElement(repository, HoService.DisplayMode.Method);
                    break;

                case MenuDisplayBehavior:
                    HoService.DisplayOperationForSelectedElement(repository, HoService.DisplayMode.Behavior);
                    break;


            }
        }
        

        

        



        private static void ShowAbout()
        {
            About fAbout = new About();
            fAbout.ShowDialog();
        }

   
        /// <summary>
        /// Show Addin Tab as configured:
        /// - With History
        /// - With Bookmarks
        /// </summary>
        private void ShowWindow()
        {

            if (_hoReverseGui == null)

                try
                {
                    string tabName = _addinSettings.ProductName;
                        
                    _hoReverseGui = (HoReverseGui)_repository.AddWindow(tabName, "hoReverse.ReverseGui");
                    _hoReverseGuiStatic = _hoReverseGui;

                    if (_addinSettings.ShowBookmark)  _bookmarkControl = (HistoryGui)_repository.AddWindow("Bookmark", "hoReverse.HistoryGui");
                    if (_addinSettings.ShowHistory) _historyControl = (HistoryGui)_repository.AddWindow("History", "hoReverse.HistoryGui");
                    
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            if (_hoReverseGui == null) MessageBox.Show(@"Prog-id='hoReverse.ReverseGui'",
                @"Addin hoReverseGUI could not be instantiated");
            else
            {
                _hoReverseGui.Repository = _repository;
                if (_addinSettings.ShowHistory) _hoReverseGui.History = _history;
                if (_addinSettings.ShowBookmark) _hoReverseGui.Bookmark = _bookmark;
                _hoReverseGui.Release = Release;


                _hoReverseGui.AddinSettings = _addinSettings;
                 _hoReverseGui.Focus();
                _hoReverseGui.Show();

                               
            }
        }

        
        
        
        
         
        
        void BtnRemoveAll_Click(object sender, EventArgs e)
        {
            _history?.RemoveAll();
        }
  
        void BtnBookmarkAdd_Click(object sender, EventArgs e)
        {
            EA.ObjectType ot = _repository.GetContextItemType();
            string guid = "";
            switch (ot){
                case EA.ObjectType.otDiagram:
                    EA.Diagram dia  = (EA.Diagram)_repository.GetContextObject();
                    guid = dia.DiagramGUID;
                    _bookmark.Add(ot, guid);
                    break;
                case EA.ObjectType.otPackage:
                    EA.Package pkg = (EA.Package)_repository.GetContextObject();
                    guid = pkg.PackageGUID;
                    _bookmark.Add(ot, guid);
                    break;
                case EA.ObjectType.otElement:
                    EA.Element el = (EA.Element)_repository.GetContextObject();
                    guid = el.ElementGUID;
                    _bookmark.Add(ot, guid);
                    break;
            }


        }
        void BtnBookmarkRemove_Click(object sender, EventArgs e)
        {
            EA.ObjectType ot = _repository.GetContextItemType();
            string guid = "";
            if (ot.Equals(EA.ObjectType.otDiagram))
            {
                EA.Diagram dia = (EA.Diagram)_repository.GetContextObject();
                guid = dia.DiagramGUID;
            }
            _bookmark.Remove(ot, guid);
        }

        void BtnBookmarkRemoveAll_Click(object sender, EventArgs e)
        {
            _bookmark.RemoveAll();
        }

        void BtnFinal_Click(object sender, EventArgs e)
        {
            HoService.CreateDiagramObjectFromContext(_repository, "", "StateNode", "101");

        }

        void BtnAction_Click(object sender, EventArgs e)
        {
            HoService.CreateDiagramObjectFromContext(_repository, "", "Action", "");

        }
        
        void BtnDecision_Click(object sender, EventArgs e)
        {
            HoService.CreateDiagramObjectFromContext(_repository, "", "Decision", "");
        }
        void BtnMerge_Click(object sender, EventArgs e)
        {
            HoService.CreateDiagramObjectFromContext(_repository, "", "MergeNode", "");
        }

        ///////////////////////////////////////////
        // Activity Diagram
        //-----------------------------------------
        // call operation
        // action
        // decision with: #if, if, while, do at start
        /// ///////////////////////////////////////
        /// Class/Component Diagram
        /// ---------------------------------------
        /// - realize/use interface  (Class/Interface)
        /// - Port/required & provided interface
        /// 

        

        



        /// <summary>
        /// Called when EA start model validation. Just shows a message box
        /// </summary>
        /// <param name="repository">the repository</param>
        /// <param name="args">the arguments</param>
        public override void EA_OnStartValidation(EA.Repository repository, object args)
        {
            MessageBox.Show(@"Validation started");
        }
        /// <summary>
        /// Called when EA ends model validation. Just shows a message box
        /// </summary>
        /// <param name="repository">the repository</param>
        /// <param name="args">the arguments</param>
        public override void EA_OnEndValidation(EA.Repository repository, object args)
        {
            MessageBox.Show(@"Validation ended");
        }
     }
}
