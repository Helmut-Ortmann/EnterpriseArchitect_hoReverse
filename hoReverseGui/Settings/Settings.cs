using System;
using System.Windows.Forms;
using System.Configuration;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using hoReverse.HistoryList;
using System.Runtime.InteropServices;
using hoReverse.Reverse.EaAddinShortcuts;
using System.Text.RegularExpressions;
using System.Reflection;
using hoReverse.Services;
using GlobalHotkeys;
using hoReverse.Connectors;


// ReSharper disable once CheckNamespace
namespace hoReverse.Settings
{
    /// <summary>
    /// Description of NavigatorSettings.
    /// </summary>
    /// 

    //----------------------------------------------------------------------------------
    // Maintaining of settings in %appdata%\ho\hoReverse
    //----------------------------------------------------------------------------------
    // Default settings in AddinControl.dll.config
    // Make sure to copy the default settings in project output.
    public class AddinSettings
    {

        // File path of configuration file
        // %APPDATA%ho\hoReverse\user.config
        public string ConfigFilePath { get; }
        // %APPDATA%ho\hoReverse\
        public string ConfigFolderPath { get; }

        public readonly EaAddinShortcutSearch[] ShortcutsSearch;
        
        // Configuration 5 services
        public readonly List<ServicesCallConfig> ShortcutsServices;
        // all available services
        public readonly List<ServiceCall> AllServices = new List<ServiceCall>();

        public readonly List<GlobalKeysConfig.GlobalKeysServiceConfig> GlobalServiceKeys;
        public readonly List<GlobalKeysConfig.GlobalKeysSearchConfig> GlobalSearchKeys;

        // Connectors
        public readonly LogicalConnectors LogicalConnectors = new LogicalConnectors();
        public readonly ActivityConnectors ActivityConnectors = new ActivityConnectors();

        
        const int MaxSavedEntries = 50;
        private Configuration DefaultConfig { get; set; }
        public Configuration CurrentConfig { get; set; }
        public AddinSettings()
        {

           

        Configuration roamingConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoaming);

            //the roamingConfig now get a path such as C:\Users\<user>\AppData\Roaming\Sparx_Systems_Pty_Ltd\DefaultDomain_Path_2epjiwj3etsq5yyljkyqqi2yc4elkrkf\9,_2,_0,_921\user.config
            // which I don't like. So we move up three directories and then add a directory for the EA Navigator so that we get
            // C:\Users\<user>\AppData\Roaming\GeertBellekens\EANavigator\user.config
            string configFileName = System.IO.Path.GetFileName(roamingConfig.FilePath);
            string configDirectory = System.IO.Directory.GetParent(roamingConfig.FilePath).Parent.Parent.Parent.FullName;

            // The path to configuration file 'c:\users\<user>\AppData\Roaming\ho\hoReverse\user.config'
            ConfigFolderPath = configDirectory + @"\ho\hoReverse\";
            ConfigFilePath = ConfigFolderPath + configFileName;

            // Map the roaming configuration file. This
            // enables the application to access 
            // the configuration file using the
            // System.Configuration.Configuration class
            ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap {ExeConfigFilename = ConfigFilePath};
            // Get the mapped configuration file.
            CurrentConfig = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);
            //merge the default settings
            //MessageBox.Show("Initializing settings merging");
            this.MergeDefaultSettings();
            //MessageBox.Show("Initializing settings finished");
            this.ShortcutsSearch = GetShortcutsQuery();
            this.ShortcutsServices = GetServices();
            this.GlobalServiceKeys = GetGlobalKeysService();
            this.GlobalSearchKeys = GetGlobalKeysSearch();

            GetConnector(LogicalConnectors);
            GetConnector(ActivityConnectors);
            GetAllServices();
            UpdateServices();
        }

        /// <summary>
        /// gets the default settings config.
        /// </summary>
        private void GetDefaultSettings()
        {
            string defaultConfigFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            DefaultConfig = ConfigurationManager.OpenExeConfiguration(defaultConfigFilePath);
        }
        /// <summary>
        /// merge the default settings with the current config.
        /// </summary>
        private void MergeDefaultSettings()
        {
            if (this.DefaultConfig == null)
            {
                this.GetDefaultSettings();
            }
            //defaultConfig.AppSettings.Settings["menuOwnerEnabled"].Value
            if (DefaultConfig.AppSettings.Settings.Count == 0)
            {
                MessageBox.Show(@"No default settings in '" + DefaultConfig.FilePath + @"' found!", @"Installation wasn't successful!");
            }
            foreach (KeyValueConfigurationElement configEntry in DefaultConfig.AppSettings.Settings)
            {
                if (!CurrentConfig.AppSettings.Settings.AllKeys.Contains(configEntry.Key))
                {
                    CurrentConfig.AppSettings.Settings.Add(configEntry.Key, configEntry.Value);
                }
            }
            // save the configuration
            CurrentConfig.Save();
        }
        //public bool isOptionEnabled(UML.UMLItem parentElement, string option)
        //{
        //    //default
        //    return true;
        //}
        /// <summary>
        /// returns true when the selecting an element in the project browser is the default action on double click.
        /// </summary>
        /// <returns>true when the selecting an element in the project browser is the default action on double click.</returns>

        #region Properties
        public bool ShowHistory
        {
            get
            {
                if (bool.TryParse(this.CurrentConfig.AppSettings.Settings["ShowHistory"].Value, out bool result))
                {
                    return result;
                }
                else
                {
                    return true;
                }
            }
            set
            {
                this.CurrentConfig.AppSettings.Settings["ShowHistory"].Value = value.ToString();

            }

        }
        public bool ShowBookmark
        {
            get
            {
                if (bool.TryParse(this.CurrentConfig.AppSettings.Settings["ShowBookmark"].Value, out var result))
                {
                    return result;
                }
                else
                {
                    return true;
                }
            }
            set
            {
                this.CurrentConfig.AppSettings.Settings["ShowBookmark"].Value = value.ToString();

            }

        }

        /// <summary>
        /// In reverse engineering use a CallBehavior Action for a function call instead of a CallOperation Action.
        /// </summary>
        public bool UseCallBehaviorAction
        {
            get
            {
                if (bool.TryParse(this.CurrentConfig.AppSettings.Settings["UseCallBehaviorAction"].Value, out var result))
                {
                    return result;
                }
                else
                {
                    return true;
                }
            }
            set { this.CurrentConfig.AppSettings.Settings["UseCallBehaviorAction"].Value = value.ToString(); }
        }


        public bool FileManagerIsTotalCommander
        {
            get
            {
                if (bool.TryParse(this.CurrentConfig.AppSettings.Settings["FileManagerIsTotalCommander"].Value, out var result))
                {
                    return result;
                }
                else
                {
                    return true;
                }
            }
            set
            {
                this.CurrentConfig.AppSettings.Settings["FileManagerIsTotalCommander"].Value = value.ToString();

            }

        }
        /// <summary>
        /// Check if folder exists
        /// </summary>
        /// <returns></returns>
        public bool IsFolderPathCSourceCode()
        {
           if (System.IO.Directory.Exists(FolderPathCSourceCode)) return true;
            MessageBox.Show($@"You can set the C/C++ Directory in Settings{Environment.NewLine}Directory: '{FolderPathCSourceCode}' doesn't exists!", 
                @"C/C++ Source code folder doesn't exists, break!!!");
            return false;

        }
        /// <summary>
        /// Folder path to C/C++ source code, used to find the VS Code symbol database
        /// Roaming\Code\User\workspaceStorage\hash\ms-vscode.cpptools\.BROWSE.VC.DB
        /// </summary>
        public string FolderPathCSourceCode
        {
            get
            {
                if (this.CurrentConfig.AppSettings.Settings["FolderPathCSourceCode"].Value == null)
                {
                    return @"d:\hoData\Projects\00Current\ZF\Work\Source\";
                }
                else
                {
                    return (this.CurrentConfig.AppSettings.Settings["FolderPathCSourceCode"].Value);
                }
            }
            set
            {
                this.CurrentConfig.AppSettings.Settings["FolderPathCSourceCode"].Value = value;

            }
        }
        public string DiagramSearchName
        {
            get
            {
                return "Diagram Name";
            }
            set
            {
                this.CurrentConfig.AppSettings.Settings["DiagramNameSearchName"].Value = value;

            }
        }
        public string SimpleSearchName
        {
            get => "Simple";
            set
            {
                CurrentConfig.AppSettings.Settings["SimpleSearchName"].Value = value;

            }
        }
        public string RecentModifiedDiagramsSearch
        {
            get
            {
                return "Recently Modified Diagrams";
                
            }
            set
            {
                CurrentConfig.AppSettings.Settings["RecentModifiedSearchName"].Value = value;

            }
        }

        public string QuickSearchName
        {
            get
            {
                if (this.CurrentConfig.AppSettings.Settings["QuickSearchName"].Value == null) {
                    return "Quick Search";
                }else {
                return (this.CurrentConfig.AppSettings.Settings["QuickSearchName"].Value );
                }
            }
            set
            {
                this.CurrentConfig.AppSettings.Settings["QuickSearchName"].Value =value;
               
            }
        }
        public string ActivityLineStyle
        {
            get
            {
                if (this.CurrentConfig.AppSettings.Settings["ActivityLineStyle"].Value == null)
                {
                    return "LV";
                }
                else
                {
                    return (this.CurrentConfig.AppSettings.Settings["ActivityLineStyle"].Value);
                }
            }
            set
            {
                this.CurrentConfig.AppSettings.Settings["ActivityLineStyle"].Value = value;

            }
        }
        public string StatechartLineStyle
        {
            get
            {
                if (this.CurrentConfig.AppSettings.Settings["StateLineStyle"].Value == null)
                {
                    return "B";
                }
                else
                {
                    return (this.CurrentConfig.AppSettings.Settings["StateLineStyle"].Value);
                }
            }
            set
            {
                this.CurrentConfig.AppSettings.Settings["StateLineStyle"].Value = value;

            }
        }
        public string ProductName
        {
            get
            {
                if (this.CurrentConfig.AppSettings.Settings["ProductName"].Value == null)
                {
                    return "hoReverse";
                }
                else
                {
                    return (this.CurrentConfig.AppSettings.Settings["ProductName"].Value);
                }
            }
            set
            {
                this.CurrentConfig.AppSettings.Settings["ProductName"].Value = value;

            }
        }
        public bool WithTabWindow
        {
            get
            {
                if (bool.TryParse(this.CurrentConfig.AppSettings.Settings["WithTabWindow"].Value, out var result))
                {
                    return result;
                }
                else
                {
                    return true;
                }
            }
            set
            {
                CurrentConfig.AppSettings.Settings["WithTabWindow"].Value = value.ToString();

            }
        }

        private void SetShortcuts(EaAddinShortcut[] l) {
            for (int i = 0; i< l.Length;i++)
            {
                if (l[i] == null) continue;
                if (l[i] is EaAddinShortcutSearch el)
                {
                    string basicKey = "Key" + el.keyPos;
                    this.CurrentConfig.AppSettings.Settings[basicKey + "Text"].Value = el.keyText;
                    this.CurrentConfig.AppSettings.Settings[basicKey + "Type"].Value = "Search";
                    this.CurrentConfig.AppSettings.Settings[basicKey + "Par1"].Value = el.keySearchName;
                    this.CurrentConfig.AppSettings.Settings[basicKey + "Par2"].Value = el.keySearchTerm;
                    this.CurrentConfig.AppSettings.Settings[basicKey + "Tooltip"].Value = el.keySearchTooltip;
                }
            }

        }

        private void SetGlobalKeySearch(List<GlobalKeysConfig.GlobalKeysSearchConfig> l) {
            for (int i = 0; i< l.Count;i++)
            {
                if (l[i] == null) continue;
                    GlobalKeysConfig.GlobalKeysSearchConfig el = l[i];
                    string basicKey = "globalKeySearch" + (i+1).ToString();
                    this.CurrentConfig.AppSettings.Settings[basicKey + "Key"].Value = el.Key;
                    this.CurrentConfig.AppSettings.Settings[basicKey + "Modifier1"].Value = el.Modifier1;
                    this.CurrentConfig.AppSettings.Settings[basicKey + "Modifier2"].Value = el.Modifier2;
                    this.CurrentConfig.AppSettings.Settings[basicKey + "Modifier3"].Value = el.Modifier3;
                    this.CurrentConfig.AppSettings.Settings[basicKey + "Modifier4"].Value = el.Modifier4;
                    this.CurrentConfig.AppSettings.Settings[basicKey + "SearchName"].Value = el.SearchName;
                    this.CurrentConfig.AppSettings.Settings[basicKey + "SearchTerm"].Value = el.SearchTerm;
                    this.CurrentConfig.AppSettings.Settings[basicKey + "Tooltip"].Value = el.Tooltip;
            }

        }

        private void SetGlobalKeyService(List<GlobalKeysConfig.GlobalKeysServiceConfig> l)
        {
            
            for (int i = 0; i < l.Count; i++)
            {
                if (l[i] == null) continue;
                GlobalKeysConfig.GlobalKeysServiceConfig el = l[i];
                string basicKey = "globalKeyService" + (i+1).ToString();
                this.CurrentConfig.AppSettings.Settings[basicKey + "Key"].Value = el.Key;
                this.CurrentConfig.AppSettings.Settings[basicKey + "Modifier1"].Value = el.Modifier1;
                this.CurrentConfig.AppSettings.Settings[basicKey + "Modifier2"].Value = el.Modifier2;
                this.CurrentConfig.AppSettings.Settings[basicKey + "Modifier3"].Value = el.Modifier3;
                this.CurrentConfig.AppSettings.Settings[basicKey + "Modifier4"].Value = el.Modifier4;
                this.CurrentConfig.AppSettings.Settings[basicKey + "GUID"].Value = el.Guid;
            }

        }

        private void GetConnector(DiagramConnector l)
        {
            string diagramType = l.DiagramType;
            string type = "";
            string lineStyle = "";
            string stereotype = "";
            bool isDefault = false;

            foreach (KeyValueConfigurationElement configEntry in CurrentConfig.AppSettings.Settings)
            {
                var sKey = configEntry.Key;
                string regex = diagramType +"Connector([0-9]+)([a-zA-Z_0-9]+)";
                Match match = Regex.Match(sKey, regex);
                if (match.Success)
                {
                    switch (match.Groups[2].Value)
                    {
                        case "Type":
                            type = configEntry.Value;
                            break;
                        case "Stereotype":
                            stereotype = configEntry.Value;
                            break;
                        case "LineStyle":
                            lineStyle = configEntry.Value;
                            break;
                        case "IsDefault":
                            isDefault = false;
                            if (configEntry.Value == "True") isDefault = true;

                            break;
                        case "IsEnabled":
                            bool isEnabled = configEntry.Value == "True";
                            l.Add(new Connector(type, stereotype, lineStyle, isDefault, isEnabled));
                            break;
                    }
                }
            }
        }

        private void SetConnector(DiagramConnector l)
        {

            string diagramType = l.DiagramType;
            string basicKey;
            for (int i = 0; i < l.Count; i++)
            {
                if (l[i] == null) continue;
                Connector el = l[i];
                basicKey = diagramType + "Connector" + (i + 1).ToString();

                var key = basicKey+ "Type";
                if (! this.CurrentConfig.AppSettings.Settings.AllKeys.Contains(key))
                this.CurrentConfig.AppSettings.Settings.Add(key, el.Type); 
                else  this.CurrentConfig.AppSettings.Settings[key].Value = el.Type; 

                key = basicKey + "Stereotype";
                if (! this.CurrentConfig.AppSettings.Settings.AllKeys.Contains(key))
                    this.CurrentConfig.AppSettings.Settings.Add(key, el.Stereotype);
                else this.CurrentConfig.AppSettings.Settings[key].Value = el.Stereotype;

                key = basicKey + "LineStyle";
                if (!this.CurrentConfig.AppSettings.Settings.AllKeys.Contains(key))
                    this.CurrentConfig.AppSettings.Settings.Add(key, el.LineStyle);
                else this.CurrentConfig.AppSettings.Settings[key].Value = el.LineStyle;


                key = basicKey + "IsDefault";
                if (! this.CurrentConfig.AppSettings.Settings.AllKeys.Contains(key))
                    this.CurrentConfig.AppSettings.Settings.Add(key, el.IsDefault.ToString());
                else this.CurrentConfig.AppSettings.Settings[key].Value = el.IsDefault.ToString();

                key = basicKey + "IsEnabled";
                if (! this.CurrentConfig.AppSettings.Settings.AllKeys.Contains(key))
                    this.CurrentConfig.AppSettings.Settings.Add(key, el.IsEnabled.ToString());
                else this.CurrentConfig.AppSettings.Settings[key].Value = el.IsEnabled.ToString();
                               
            }
            // delete unused entries
            int index = l.Count +1;
            while (true)
            {
            basicKey = diagramType + "Connector" + index.ToString();
            if (this.CurrentConfig.AppSettings.Settings.AllKeys.Contains(basicKey+"Type"))
            {
                this.CurrentConfig.AppSettings.Settings.Remove(basicKey + "IsEnabled");
                this.CurrentConfig.AppSettings.Settings.Remove(basicKey + "IsDefault");
                this.CurrentConfig.AppSettings.Settings.Remove(basicKey + "Stereotype");
                this.CurrentConfig.AppSettings.Settings.Remove(basicKey + "Type");
                index = index + 1;
            }

            else {break;}
            }

        }


        private EaAddinShortcutSearch[] GetShortcutsQuery()
        {
            int pos = 0;
            string text = "";
            string type = "";
            string par1 = "";
            string par2 ="";
            EaAddinShortcutSearch[] l = new EaAddinShortcutSearch[10];
            foreach (KeyValueConfigurationElement configEntry in CurrentConfig.AppSettings.Settings)
            {
                var sKey = configEntry.Key;
                string regex = @"key([0-9]+)([a-zA-Z_0-9]+)";
                Match match = Regex.Match(sKey, regex);
                if (match.Success)
                {
                    int posValue = Convert.ToInt16(match.Groups[1].Value);
                    switch (match.Groups[2].Value) {
                        case "Type":
                            type = configEntry.Value;
                            break;
                        case "Text":
                            text = configEntry.Value;
                            break;
                        case "Par1":
                            par1 = configEntry.Value;
                            break;
                        case "Par2":
                            par2 = configEntry.Value;
                            break;
                        case "Tooltip":
                            switch (type){
                                case "Search":
                                    l[pos] = new EaAddinShortcutSearch(posValue, text, par1, par2, configEntry.Value);
                                    pos = pos + 1;
                                    break;
                            }
                            break;
                    }
                }
            }
            
            return l;
        }
        private List<ServicesCallConfig> GetServices()
        {
            List<ServicesCallConfig> l = new List<ServicesCallConfig>();
            string guid = "";
            foreach (KeyValueConfigurationElement configEntry in CurrentConfig.AppSettings.Settings)
            {
                var sKey = configEntry.Key;
                string regex = @"service([0-9]+)([a-zA-Z_0-9]+)";
                Match match = Regex.Match(sKey, regex);
                if (match.Success)
                {
                    int pos = Convert.ToInt16(match.Groups[1].Value);
                    switch (match.Groups[2].Value)
                    {
                        case "GUID":
                            guid = configEntry.Value;
                            break;
                        case "Text":
                            var text = configEntry.Value;
                            l.Add(new ServicesCallConfig(pos, guid, text));
                            break;
                    }
                }
            }
            return l;
          
        }
        private void GetAllServices()
        {
            Type type = typeof(HoService);
            AllServices.Add(new ServiceCall(null, "{B93C105E-64BC-4D9C-B92F-3DDF0C9150E6}", "-- no --", "no service selected", false));
            foreach (MethodInfo method in type.GetMethods())
            {

                foreach (Attribute attr in method.GetCustomAttributes(true))
                {
                    if (attr is ServiceOperationAttribute serviceOperation)
                    {
                        AllServices.Add(new ServiceCall(method, serviceOperation.Guid, serviceOperation.Description, serviceOperation.Help, serviceOperation.IsTextRequired));
                    }
                }
            }
            AllServices.Sort(new ServicesCallDescriptionComparer());
        }
        private List<GlobalKeysConfig.GlobalKeysServiceConfig> GetGlobalKeysService()
         {
             List<GlobalKeysConfig.GlobalKeysServiceConfig> l = new List<GlobalKeysConfig.GlobalKeysServiceConfig>();
             string key = "";
             string modifier1 = "";
             string modifier2 = "";
             string modifier3 = "";
             string modifier4 = "";

             foreach (KeyValueConfigurationElement configEntry in CurrentConfig.AppSettings.Settings)
             {
                 var sKey = configEntry.Key;
                 string regex = @"globalKeyService([0-9]+)([a-zA-Z_0-9]+)";
                 Match match = Regex.Match(sKey, regex);
                 if (match.Success)
                 {
                     switch (match.Groups[2].Value)
                     {
                         case "Key":
                             key = configEntry.Value;
                             break;
                         case "Modifier1":
                             modifier1 = configEntry.Value;
                             break;
                         case "Modifier2":
                             modifier2 = configEntry.Value;
                             break;
                         case "Modifier3":
                             modifier3 = configEntry.Value;
                             break;
                         case "Modifier4":
                             modifier4 = configEntry.Value;
                             break;
                         case "GUID":
                             var guid = configEntry.Value;
                             l.Add(new GlobalKeysConfig.GlobalKeysServiceConfig(key,modifier1,modifier2,modifier3,modifier4,"", guid, "",false));
                             break;
                     }
                 }
             }
             return l;
            //globalServiceKeys.Add(new GlobalKeysConfig.GlobalKeysServiceConfig("F", "Ctrl", "No", "No", "No","Help","","",false));
            //globalServiceKeys.Add(new GlobalKeysConfig.GlobalKeysServiceConfig("A", "Shift", "No", "No", "No", "Help", "", "", false));
            //globalServiceKeys.Add(new GlobalKeysConfig.GlobalKeysServiceConfig("B", "Win", "No", "No", "No", "Help", "", "", false));
            //globalServiceKeys.Add(new GlobalKeysConfig.GlobalKeysServiceConfig("C", "Alt", "No", "No", "No", "Help", "", "", false));
            //globalServiceKeys.Add(new GlobalKeysConfig.GlobalKeysServiceConfig("D", "No", "No", "No", "No", "Help", "", "", false));
        }
        private List<GlobalKeysConfig.GlobalKeysSearchConfig> GetGlobalKeysSearch()
        {
            List<GlobalKeysConfig.GlobalKeysSearchConfig> l = new List<GlobalKeysConfig.GlobalKeysSearchConfig>();
            string key = "";
            string modifier1 = "";
            string modifier2 = "";
            string modifier3 = "";
            string modifier4 = "";
            string searchName = "";
            string searchTerm = "";

            foreach (KeyValueConfigurationElement configEntry in CurrentConfig.AppSettings.Settings)
            {
                var sKey = configEntry.Key;
                string regex = @"globalKeySearch([0-9]+)([a-zA-Z_0-9]+)";
                Match match = Regex.Match(sKey, regex);
                if (match.Success)
                {
                    switch (match.Groups[2].Value)
                    {
                        case "Key":
                            key = configEntry.Value;
                            break;
                        case "Modifier1":
                            modifier1 = configEntry.Value;
                            break;
                        case "Modifier2":
                            modifier2 = configEntry.Value;
                            break;
                        case "Modifier3":
                            modifier3 = configEntry.Value;
                            break;
                        case "Modifier4":
                            modifier4 = configEntry.Value;
                            break;
                        case "SearchName":
                            searchName = configEntry.Value;
                            break;
                        case "SearchTerm":
                            searchTerm = configEntry.Value;
                            break;
                        case "Tooltip":
                            var tooltip = configEntry.Value;

                            l.Add(new GlobalKeysConfig.GlobalKeysSearchConfig(key, modifier1, modifier2, modifier3, modifier4, tooltip, 
                                searchName, searchTerm));
                            break;
                    }
                }
            }
            return l;
            

        }
        public void UpdateServices()
        {
            foreach (ServicesCallConfig service in ShortcutsServices)
            {
                if (service.Guid != "{B93C105E-64BC-4D9C-B92F-3DDF0C9150E6}")
                {
                    //int index = allServices.BinarySearch(new EaServices.ServiceCall(null, service.GUID, "","", false), new EaServices.ServicesCallGUIDComparer());
                    foreach (ServiceCall s in AllServices) {
                        if (service.Guid == s.Guid)
                        {
                            service.Method = s.Method;
                            service.Help = s.Help;
                            service.Description = s.Description;
                            break;
                        }
                    }
                    
                }

            }
            foreach (GlobalKeysConfig.GlobalKeysServiceConfig service in GlobalServiceKeys)  
            {
                if (service.Guid != "{B93C105E-64BC-4D9C-B92F-3DDF0C9150E6}")
                {
                    //int index = allServices.BinarySearch(new EaServices.ServiceCall(null, service.GUID, "","", false), new EaServices.ServicesCallGUIDComparer());
                    foreach (ServiceCall s in AllServices)
                    {
                        if (service.Guid == s.Guid)
                        {
                            service.Method = s.Method;
                            service.Tooltip = s.Help;
                            break;
                        }
                    }

                }

            }
        }

        private void SetServices(List<ServicesCallConfig> l)
        {
            for (int i = 0; i < l.Count; i++)
            {
                if (l[i] == null) continue;

                    ServicesCallConfig el = l[i];
                    string basicKey = "service" + (i+1).ToString();
                    this.CurrentConfig.AppSettings.Settings[basicKey + "GUID"].Value = el.Guid;
                    this.CurrentConfig.AppSettings.Settings[basicKey + "Text"].Value = el.ButtonText;
            }

        }

        // get list of bookmarks for a project
        public List<int> GetBookmarks(string guid)
        {
            List<int> l = new List<int>(50);
            foreach (KeyValueConfigurationElement configEntry in CurrentConfig.AppSettings.Settings)
            {
                if (configEntry.Key.Contains("bookmark"))
                {
                    if (configEntry.Value.StartsWith(guid))
                    {
                        string id = configEntry.Value.Substring(38);
                        if (id != "") 
                                l.Add(Convert.ToInt32(id));
                    }
                }
            }

            return l;
        }

        // set list of bookmarks for a project
        public void SetBookmarks(string guid, ObservableCollection<EaHistoryListEntry> l)
        {
            int i = 0;
            int iMax = l.Count;
            foreach (KeyValueConfigurationElement configEntry in CurrentConfig.AppSettings.Settings)
            {
                if (configEntry.Key.Contains("bookmark"))
                {
                    if (configEntry.Value.StartsWith(guid) | configEntry.Value == "")
                    {
                        if (i < iMax)
                        {
                            configEntry.Value = guid + l[i];
                            i = i + 1;
                        }
                        else { configEntry.Value = ""; }
                    }
                }
            }
            this.Save();

        }
        #endregion

        // set list of bookmarks for a project
        public void ResetHistory(string listType)
        {
            foreach (KeyValueConfigurationElement configEntry in CurrentConfig.AppSettings.Settings)
            {
                if (configEntry.Key.Contains(listType)) configEntry.Value = "";

            }
            this.Save();

        }

        /// <summary>
        /// Reset configuration to default settings. The old configuration is stored to 'user.config.tmp'.
        /// This is done by deleting the configuration file.
        /// </summary>
        /// <returns></returns>
        public string Reset()
        {
            // .net standard configuration file
            string filePath = CurrentConfig.FilePath;
            CurrentConfig.SaveAs($@"{filePath}.tmp", ConfigurationSaveMode.Full, forceSaveAll: true);
            File.Delete(filePath);

            // Settings.json
            string fileSource = Path.Combine(Path.GetDirectoryName(filePath), "Settings.json");
            string fileTarget = Path.Combine(Path.GetDirectoryName(filePath), "Settings.json.tmp");
            File.Copy(fileSource, fileTarget, true);
            File.Delete(fileSource);

            return filePath;
        }
        /// <summary>
        /// saves the settings to the config file
        /// </summary>
        public void Save()
        {
            this.SetShortcuts(ShortcutsSearch);
            this.SetServices(ShortcutsServices);
            this.SetGlobalKeySearch(GlobalSearchKeys);
            this.SetGlobalKeyService(GlobalServiceKeys);

            this.SetConnector(LogicalConnectors);
            this.SetConnector(ActivityConnectors);
            this.CurrentConfig.Save();
        }

        public void Refresh()
        {
            ExeConfigurationFileMap configFileMap =
                new ExeConfigurationFileMap {ExeConfigFilename = CurrentConfig.FilePath};
            // Get the mapped configuration file.
            CurrentConfig = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);
        }
        //------------------------------------------------------------------//
        // get list of elements for a project
        public ObservableCollection<EaHistoryListEntry> GetHistory(string listType, string guid)
        {

            ObservableCollection<EaHistoryListEntry> l = new ObservableCollection<EaHistoryListEntry>();
            foreach (KeyValueConfigurationElement configEntry in CurrentConfig.AppSettings.Settings)
            {
                if (configEntry.Key.Contains(listType))
                {
                    if (configEntry.Value.StartsWith(guid))
                    {
                        if (configEntry.Value.Length > 77)
                        {
                            string objectGuid = configEntry.Value.Substring(38, 38);
                            string s = configEntry.Value.Substring(76);
                            string bookmarkName = configEntry.Value.Substring(85);
                            EA.ObjectType type = EA.ObjectType.otNone;
                            if (s.Contains("Diagram")) type = EA.ObjectType.otDiagram;
                            if (s.Contains("Element")) type = EA.ObjectType.otElement;
                            if (s.Contains("Package")) type = EA.ObjectType.otPackage;
                            if (objectGuid != "" & type != EA.ObjectType.otNone)
                            {
                                l.Add(new EaHistoryListEntry(type, objectGuid, bookmarkName));
                            }
                        }
                    }
                }
            }

            return l;
        }
        // set list of bookmarks for a project
        //listType: 'diagram','bookmark'
        public void SetHistory(string listType, string guid, ObservableCollection<EaHistoryListEntry> l)
        {
            int i = l.Count - MaxSavedEntries;// last elements
            if (i < 0) i = 0;
            int iMax = l.Count;
            foreach (KeyValueConfigurationElement configEntry in CurrentConfig.AppSettings.Settings)
            {
                if (configEntry.Key.Contains(listType))
                {
                    if (configEntry.Value.StartsWith(guid) | configEntry.Value == "")
                    {
                        if (i < iMax)
                        {
                            configEntry.Value = guid  + l[i].Guid + l[i].EaTyp+l[i].BookmarkName;
                            i = i + 1;
                        }
                                 
                    }
                }
            }
            try
            {
                this.Save();
            }
            catch (ConfigurationErrorsException e)
            {
                // Error: Other application had changed file, ignore this error
                int hr = Marshal.GetHRForException(e);
                if (hr != -2146232062)
                {
                    MessageBox.Show(e.ToString(), "Error storing user.config");
                }
            }
          }
        /// <summary>
        /// Create AppDataFolder if not exists
        /// </summary>
        /// <param name="configFileName"></param>
        private void CreateAppDataFolder(string configFileName)
        {
            if (!Directory.Exists(ConfigFolderPath))
            {
                try
                {
                    System.IO.Directory.CreateDirectory(ConfigFolderPath);
                }
                catch (Exception e)
                {
                    MessageBox.Show(
                        $"Can't create Config Data Folder:{Environment.NewLine}'{configFileName}'{Environment.NewLine}{Environment.NewLine}{e}",
                        @"Can't get create Config Folder!");
                }
            }
        }



    }
   
}

