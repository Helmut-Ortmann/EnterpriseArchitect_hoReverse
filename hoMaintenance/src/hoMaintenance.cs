using System;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Diagnostics;

//using ho_Tools.Utils;
using hoUtil;
using MksUtil;
using System.Reflection;

namespace hoMaintenance
{
    public class HoMaintenanceClass : EAAddinFramework.EAAddinBase
    {
        //string logInUser = null;
        //UserRights logInUserRights = UserRights.ADMIN;
        //enum UserRights
        //{
        //    STANDARD,
        //    ADMIN
        //}

        // define menu constants
        const string MenuName = "-hoMaintenance";
        public static string Release = "xx.xx.xx.xx"; // overwritten by EA_OnPostInitialized

        const string MenuMksUndoCheckOut = "MKS undo Checkout";
        const string MenuMksGetHistory = "MKS get History";
        const string MenuMksGetNewest = "MKS get newest (Head Revision, LoadXML)";

        const string MenuCopyGuidToClipboard = "Copy GUID / Select Statement to Clipboard";
        const string MenuDeviderVc = "-------------VC-----------------------------";
        const string MenuGetVcLatestRecursive = "Get VC latest member recursive (Member Revision)";
        const string MenuGetVcLatest = "Get VC latest member (Member Revision)";
        const string MenuResetVcMode = "Reset VC state of controlled package";
        const string MenuResetVcModeRecursive = "&Reset VC state of controlled package (nested)";
 
        const string MenuGetVcMode = "Get VC state";
        const string MenuGetVcModeAll = "Get VC state all Packages";

        const string MenuDeviderMaintenance = "-------------Maintenance-------------------------";
        const string MenuChangeClassNameToSynonym = "ChangeNameToSynonyms";
        const string MenuDeleteExternalReference = "DeleteExternalReference";
        const string MenuDeleteExternalReferenceRecursive = "DeleteExternalReferenceRecursive";

        const string MenuAbout = "About + Help";

        const string MenuDevider = "-----------------------------------------------"; 


        public enum DisplayMode
        {
            Behavior,
            Method
        }


        /// <summary>
        /// constructor where we set the menuheader and menuOptions
        /// </summary>
        public HoMaintenanceClass()
            : base()
        {
            this.menuHeader = MenuName;
            this.menuOptions = new string[] { 
                //--------------------------  Maintenance --------------------------//
                MenuCopyGuidToClipboard,
                MenuDeviderMaintenance,
                MenuChangeClassNameToSynonym,
                MenuDeleteExternalReference,
                MenuDeleteExternalReferenceRecursive,
            //---------------MKS --------------------------------------------
            MenuMksUndoCheckOut,
                MenuMksGetHistory,
                MenuMksGetNewest,
                //---------------VC ---------------------------------------------------------------------
                MenuDeviderVc,
                MenuGetVcLatest,MenuGetVcLatestRecursive, MenuGetVcMode, MenuGetVcModeAll, MenuResetVcMode, MenuResetVcModeRecursive,
    
                //---------------------------- About -------------------------------//
                MenuDevider, MenuAbout };
        }
        public override void EA_OnPostInitialized(EA.Repository rep)
        {
            Release = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
            rep.WriteOutput("System", Release, 0);

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
            //if (Repository.IsSecurityEnabled)
            //{
            //    logInUser = Repository.GetCurrentLoginUser();
            //    if ((logInUser.Contains("deexortm")) ||
            //         (logInUser.Contains("admin")) ||
            //         (logInUser.Equals(""))
            //        ) logInUserRights = UserRights.ADMIN;
            //}
            return "a string";
        }
        /// <summary>
        /// Called once Menu has been opened to see what menu items should active.
        /// </summary>
        /// <param name="repository">the repository</param>
        /// <param name="location">the location of the menu</param>
        /// <param name="MenuName">the name of the menu</param>
        /// <param name="itemName">the name of the menu item</param>
        /// <param name="isEnabled">boolean indicating whethe the menu item is enabled</param>
        /// <param name="isChecked">boolean indicating whether the menu is checked</param>
        public override void EA_GetMenuState(EA.Repository repository, string location, string MenuName, string itemName, ref bool isEnabled, ref bool isChecked)
        {
            if (IsProjectOpen(repository))
            {
                switch (itemName)
                {


                    case MenuMksGetNewest:
                        isChecked = false;
                        break;
                
                case MenuMksGetHistory:
                        isChecked = false;
                        break;

                case MenuMksUndoCheckOut:
                        isChecked = false;
                        break;

                case MenuChangeClassNameToSynonym:
                        isChecked = false;
                        break;
                
                case MenuGetVcLatest:
                        isChecked = false;
                        break;

                case MenuGetVcLatestRecursive:
                        isChecked = false;
                        break;

                 case MenuResetVcMode:
                        isChecked = false;
                        break;

                 case MenuResetVcModeRecursive:
                        isChecked = false;
                        break;

                    case MenuGetVcModeAll:
                        isChecked = false;
                        break;

                    case MenuGetVcMode:
                        isChecked = false;
                        break;


                    case MenuCopyGuidToClipboard:
                        isChecked = false;
                        break;


                    case MenuDeleteExternalReference:
                        isChecked = false;
                        break;
                    case MenuDeleteExternalReferenceRecursive:
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
        /// <param name="MenuName">the name of the menu</param>
        /// <param name="itemName">the name of the selected menu item</param>
        public override void EA_MenuClick(EA.Repository repository, string location, string MenuName, string itemName)
        {
            EA.Package pkg = null;
            EA.ObjectType oType = repository.GetContextItemType();
            EA.Diagram diaCurrent = repository.GetCurrentDiagram();
            EA.Connector conCurrent = null;
            EA.Element el = null;


            if (diaCurrent != null)
            {
                conCurrent = diaCurrent.SelectedConnector;
            }
            switch (itemName)
            {

                case MenuAbout:
                    About fAbout = new About();
                    fAbout.setVersion(Release); // set release / version
                    fAbout.ShowDialog();

                    break;


                case MenuMksGetNewest:
                    if (oType.Equals(EA.ObjectType.otPackage))
                    {
                        pkg = (EA.Package)repository.GetContextObject();
                        Mks mks = new Mks(repository, pkg);
                        mks.GetNewest();
                    }

                    break;

                case MenuMksGetHistory:
                    // if controlled package
                    if (oType.Equals(EA.ObjectType.otPackage))
                    {
                        pkg = (EA.Package)repository.GetContextObject();
                        Mks mks = new Mks(repository, pkg);
                        MessageBox.Show(mks.ViewHistory(), "mks");
                    }
                    break;


                case MenuMksUndoCheckOut:
                    // if controlled package
                    if (oType.Equals(EA.ObjectType.otPackage))
                    {
                        pkg = (EA.Package)repository.GetContextObject();
                        Mks mks = new Mks(repository, pkg);
                        string msg = mks.UndoCheckout();
                        if (msg != "")
                        {
                            MessageBox.Show(mks.UndoCheckout(), "mks");
                        }

                    }
                    break;
                // Change name to synomym
                // - Package, recursive
                // - Class
                case MenuChangeClassNameToSynonym:
                    // Class recursive
                    if (oType.Equals(EA.ObjectType.otElement))
                    {
                        el = (EA.Element)repository.GetContextObject();
                        Util.ChangeClassNameToSynonyms(repository, el);
                    }
                    // Package recursiv
                    if (oType.Equals(EA.ObjectType.otPackage))
                    {
                        pkg = (EA.Package)repository.GetContextObject();
                        Util.ChangePackageClassNameToSynonyms(repository, pkg);
                    }

                    break;



                // 

                //   If package is controlled: 
                //   - reset packageflags to "Recurse=0;VCCFG=unchanged";
                case MenuResetVcMode:
                    if (oType.Equals(EA.ObjectType.otPackage))
                    {
                        pkg = (EA.Package)repository.GetContextObject();
                        Util.ResetVc(repository, pkg);
                    }
                    break;

                //   For all nested packages:
                //   If package is controlled: 
                //   - reset packageflags to "Recurse=0;VCCFG=unchanged";
                case MenuResetVcModeRecursive:
                    if (oType.Equals(EA.ObjectType.otPackage))
                    {
                        pkg = (EA.Package)repository.GetContextObject();
                        Util.ResetVcRecursive(repository, pkg);
                    }
                    break;



                case MenuGetVcLatest:
                    if (oType.Equals(EA.ObjectType.otPackage))
                    {
                        // start preparation
                        int count = 0;
                        int errorCount = 0;
                        DateTime startTime = DateTime.Now;

                        repository.CreateOutputTab("Debug");
                        repository.EnsureOutputVisible("Debug");
                        repository.WriteOutput("Debug", "Start GetLatest", 0);
                        pkg = (EA.Package)repository.GetContextObject();
                        Util.GetLatest(repository, pkg, false, ref count, 0, ref errorCount);
                        string s = "";
                        if (errorCount > 0) s = " with " + errorCount.ToString() + " errors";

                        // finished
                        TimeSpan span = DateTime.Now - startTime;
                        repository.WriteOutput("Debug", "End GetLatest " + span.Minutes + " minutes. " + s, 0);

                    }

                    break;
                case MenuGetVcLatestRecursive:
                    if (oType.Equals(EA.ObjectType.otPackage) || oType.Equals(EA.ObjectType.otNone))
                    {
                        // start preparation
                        int count = 0;
                        int errorCount = 0;
                        DateTime startTime = DateTime.Now;

                        repository.CreateOutputTab("Debug");
                        repository.EnsureOutputVisible("Debug");
                        repository.WriteOutput("Debug", "Start GetLatestRecursive", 0);
                        pkg = (EA.Package)repository.GetContextObject();
                        Util.GetLatest(repository, pkg, true, ref count, 0, ref errorCount);
                        string s = "";
                        if (errorCount > 0) s = " with " + errorCount.ToString() + " errors";

                        // finished
                        TimeSpan span = DateTime.Now - startTime;

                        repository.WriteOutput("Debug", "End GetLatestRecursive in " + span.Hours + ":" + span.Minutes + " hh:mm. " + s, 0);



                    }

                    break;
                case MenuGetVcModeAll:
                    //Repository.VersionControlResynchPkgStatuses(false);
                    // over all packages
                    foreach (EA.Package pkg1 in repository.Models)
                    {
                        Util.UpdateVc(repository, pkg1);
                    }

                    break;

                case MenuGetVcMode:
                    if (oType.Equals(EA.ObjectType.otPackage))
                    {
                        pkg = (EA.Package)repository.GetContextObject();
                        // Get the revision
                        Regex pattern = new Regex(@"Revision:[^\$]+");
                        Match regMatch = pattern.Match(pkg.Notes);
                        string revision = "";
                        if (regMatch.Success)
                        {
                            // get Revision
                            revision = regMatch.Value;
                            // add new string
                        }
                        // Get date
                        pattern = new Regex(@"Date:[^\$]+");
                        regMatch = pattern.Match(pkg.Notes);
                        string date = "";
                        if (regMatch.Success)
                        {
                            // get Revision
                            date = regMatch.Value;
                            // add new string
                        }
                        string msg = revision + "  " +
                            date + "\r\n" +
                            "Path: " + Util.GetFilePath(repository, pkg) +
                            "\r\n\r\n" + pkg.Flags + "\r\n" +
                            Util.GetVCstate(pkg, true);

                        MessageBox.Show(msg, "State");
                    }
                    break;



                case MenuCopyGuidToClipboard:
                    string str = "";
                    string str1 = "";
                    string str2 = "";

                    if (conCurrent != null)
                    {// Connector 
                        EA.Connector con = conCurrent;
                        str = con.ConnectorGUID + " " + con.Name + ' ' + con.Type + "\r\n" +

                            "\r\n Connector: Select ea_guid As CLASSGUID, connector_type As CLASSTYPE,* from t_connector con where ea_guid = '" + con.ConnectorGUID + "'" +

                            "\r\n\r\nSelect o.ea_guid As CLASSGUID, o.object_type As CLASSTYPE,o.name As Name, o.object_type AS ObjectType, o.PDATA1, o.Stereotype, " +
                            "\r\n                       con.Name, con.connector_type, con.Stereotype, con.ea_guid As ConnectorGUID, dia.Name As DiagrammName, dia.ea_GUID As DiagramGUID," +
                            "\r\n                       o.ea_guid, o.Classifier_GUID,o.Classifier " +
                        "\r\nfrom (((t_connector con INNER JOIN t_object o              on con.start_object_id   = o.object_id) " +
                        "\r\nINNER JOIN t_diagramlinks diaLink  on con.connector_id      = diaLink.connectorid ) " +
                        "\r\nINNER JOIN t_diagram dia           on diaLink.DiagramId     = dia.Diagram_ID) " +
                        "\r\nINNER JOIN t_diagramobjects diaObj on diaObj.diagram_ID     = dia.Diagram_ID and o.object_id = diaObj.object_id " +
                        "\r\nwhere         con.ea_guid = '" + con.ConnectorGUID + "' " +
                        "\r\nAND dialink.Hidden  = 0 ";
                        Clipboard.SetText(str);
                        break;
                    }
                    if (oType.Equals(EA.ObjectType.otElement))
                    {// Element 
                        el = (EA.Element)repository.GetContextObject();
                        string pdata1 = el.get_MiscData(0);
                        string pdata1String = "";
                        if (pdata1.EndsWith("}"))
                        {
                            pdata1String = "/" + pdata1;
                        }
                        else
                        {
                            pdata1 = "";
                            pdata1String = "";
                        }
                        string classifier = Util.GetClassifierGuid(repository, el.ElementGUID);
                        str = el.ElementGUID + ":" + classifier + pdata1String + " " + el.Name + ' ' + el.Type + "\r\n" +
                         "\r\nSelect ea_guid As CLASSGUID, object_type As CLASSTYPE,* from t_object o where ea_guid = '" + el.ElementGUID + "'";
                        if (classifier != "")
                        {
                            if (el.Type.Equals("ActionPin"))
                            {
                                str = str + "\r\n Typ:\r\nSelect ea_guid As CLASSGUID, 'Parameter' As CLASSTYPE,* from t_operationparams op where ea_guid = '" + classifier + "'";
                            }
                            else {
                                str = str + "\r\n Typ:\r\nSelect ea_guid As CLASSGUID, object_type As CLASSTYPE,* from t_object o where ea_guid = '" + classifier + "'";
                            }
                        }
                        if (pdata1 != "")
                        {
                            str = str + "\r\n PDATA1:  Select ea_guid As CLASSGUID, object_type As CLASSTYPE,* from t_object o where ea_guid = '" + pdata1 + "'";
                        }

                        // Look for diagram object
                        EA.Diagram curDia = repository.GetCurrentDiagram();
                        if (curDia != null)
                        {
                            foreach (EA.DiagramObject diaObj in curDia.DiagramObjects)
                            {
                                if (diaObj.ElementID == el.ElementID)
                                {
                                    str = str + "\r\n\r\n" +
                                        "select * from t_diagramobjects where object_id = " + diaObj.ElementID.ToString();
                                    break;

                                }
                            }
                        }

                    }

                    if (oType.Equals(EA.ObjectType.otDiagram))
                    {// Element 
                        EA.Diagram dia = (EA.Diagram)repository.GetContextObject();
                        str = dia.DiagramGUID + " " + dia.Name + ' ' + dia.Type + "\r\n" +
                               "\r\nSelect ea_guid As CLASSGUID, diagram_type As CLASSTYPE,* from t_diagram dia where ea_guid = '" + dia.DiagramGUID + "'";
                    }
                    if (oType.Equals(EA.ObjectType.otPackage))
                    {// Element 
                        pkg = (EA.Package)repository.GetContextObject();
                        str = pkg.PackageGUID + " " + pkg.Name + ' ' + " Package " + "\r\n" +
                         "\r\nSelect ea_guid As CLASSGUID, 'Package' As CLASSTYPE,* from t_package pkg where ea_guid = '" + pkg.PackageGUID + "'";

                    }
                    if (oType.Equals(EA.ObjectType.otAttribute))
                    {// Element 
                        str1 = "LEFT JOIN  t_object typAttr on (attr.Classifier = typAttr.object_id)";
                        if (repository.ConnectionString.Contains(".eap"))
                        {
                            str1 = "LEFT JOIN  t_object typAttr on (attr.Classifier = Format(typAttr.object_id))";

                        }
                        EA.Attribute attr = (EA.Attribute)repository.GetContextObject();
                        str = attr.AttributeID + " " + attr.Name + ' ' + " Attribute " + "\r\n" +
                              "\r\n " +
                              "\r\nSelect ea_guid As CLASSGUID, 'Attribute' As CLASSTYPE,* from t_attribute attr where ea_guid = '" + attr.AttributeGUID + "'" +
                                "\r\n Class has Attributes:" +
                                "\r\nSelect attr.ea_guid As CLASSGUID, 'Attribute' As CLASSTYPE, " +
                                "\r\n       o.Name As Class, o.object_type, " +
                                "\r\n       attr.Name As AttrName, attr.Type As Type, " +
                                "\r\n       typAttr.Name " +
                                "\r\n   from (t_object o INNER JOIN t_attribute attr on (o.object_id = attr.object_id)) " +
                                "\r\n                   " + str1 +
                                "\r\n   where attr.ea_guid = '" + attr.AttributeGUID + "'";
                    }
                    if (oType.Equals(EA.ObjectType.otMethod))
                    {// Element 
                        str1 = "LEFT JOIN t_object parTyp on (par.classifier = parTyp.object_id))";
                        str2 = "LEFT JOIN t_object opTyp on (op.classifier = opTyp.object_id)";
                        if (repository.ConnectionString.Contains(".eap"))
                        {
                            str1 = " LEFT JOIN t_object parTyp on (par.classifier = Format(parTyp.object_id))) ";
                            str2 = " LEFT JOIN t_object opTyp  on (op.classifier  = Format(opTyp.object_id))";
                        }

                        EA.Method op = (EA.Method)repository.GetContextObject();
                        str = op.MethodGUID + " " + op.Name + ' ' + " Operation " +
                              "\r\nOperation may have type " +
                              "\r\nSelect op.ea_guid As CLASSGUID, 'Operation' As CLASSTYPE,opTyp As OperationType, op.Name As OperationName, typ.Name As TypName,*" +
                              "\r\n   from t_operation op LEFT JOIN t_object typ on (op.classifier = typ.object_id)" +
                              "\r\n   where op.ea_guid = '" + op.MethodGUID + "';" +
                              "\r\n\r\nClass has Operation " +
                              "\r\nSelect op.ea_guid As CLASSGUID, 'Operation' As CLASSTYPE,* " +
                              "\r\n    from t_operation op INNER JOIN t_object o on (o.object_id = op.object_id)" +
                              "\r\n    where op.ea_guid = '" + op.MethodGUID + "';" +
                              "\r\n\r\nClass has Operation has Parameters/Typ and may have operationtype" +
                              "\r\nSelect op.ea_guid As CLASSGUID, 'Operation' As CLASSTYPE,op.Name As ClassName, op.Name As OperationName, opTyp.Name As OperationTyp, par.Name As ParName,parTyp.name As ParTypeName " +
                              "\r\n   from ((t_operation op INNER JOIN t_operationparams par on (op.OperationID = par.OperationID) )" +
                              "\r\n                        " + str1 +
                              "\r\n                        " + str2 +
                              "\r\n   where op.ea_guid = '" + op.MethodGUID + "' " +
                               "\r\n  Order by par.Pos ";

                    }
                    Clipboard.SetText(str);
                    break;

                // delete undefined referencee
                case MenuDeleteExternalReference:
                    if (diaCurrent != null)
                    {
                        foreach (EA.DiagramObject diaObj in diaCurrent.SelectedObjects)
                        {
                            EA.Element el1 = repository.GetElementByID(diaObj.ElementID);
                            string s = String.Format("External Reference of diagram '{0}', name:'{1}' ID={2} GUID='{3}' deleted", diaCurrent.Name, el1.Name, el1.ElementID, el1.ElementGUID);
                            repository.WriteOutput("System", s, 0);
                            DeleteUndefinedReference(repository, el1);

                        }
                    }
                    else {
                        foreach (EA.Element el1 in repository.GetTreeSelectedElements())
                        {
                            string s = String.Format("External Reference of tree, name:'{0}' ID={1} GUID='{2}' deleted", el1.Name, el1.ElementID, el1.ElementGUID);
                            repository.WriteOutput("System", s, 0);
                            DeleteUndefinedReference(repository, el1);
                        }
                    }
                    break;

                // Recursive delete undefined referencee
                case MenuDeleteExternalReferenceRecursive:
                    pkg = null;
                    if (oType.Equals(EA.ObjectType.otNone))
                    {
                        oType = repository.GetTreeSelectedItemType();
                        if (oType.Equals(EA.ObjectType.otPackage)) pkg = (EA.Package)repository.GetTreeSelectedObject();
                        if (oType.Equals(EA.ObjectType.otElement)) el = (EA.Element)repository.GetTreeSelectedObject();

                    }
                    else
                    {


                        if (oType.Equals(EA.ObjectType.otPackage)) pkg = (EA.Package)repository.GetContextObject();
                        if (oType.Equals(EA.ObjectType.otElement)) el = (EA.Element)repository.GetContextObject();
                    }
                    if (pkg != null)
                        RecursivePackages.doRecursivePkg(repository,
                            pkg,
                            null,                           // Packages
                            SetDeleteUndefinedReference,    // Elements
                            null,                           // Diagrams
                            null);
                    if (el != null)
                        RecursivePackages.doRecursiveEl(repository,
                            el,
                            SetDeleteUndefinedReference,  // Elements
                            null,                         // Diagrams
                            null);

                    break;
            
            }
            
        }
        private static void SetDeleteUndefinedReference(EA.Repository repository, EA.Element el, string[] s)
        {
            DeleteUndefinedReference(repository, el, false);
        }
        /// <summary>
        /// Delete undefined Reference (former ExternalReference).The element subtype is "1001". It searches for 
        /// </summary>
        /// <returns></returns>
        /// Search for External references
        /// Select ea_guid As CLASSGUID, object_type As CLASSTYPE,ea_GUID As el_guid, name As el_name, * from t_object o where NType=1001
        private static void DeleteUndefinedReference(EA.Repository repository, EA.Element el, Boolean dialog = true, Boolean completePackage = false)
        {
            //if (el.Type.Equals("Boundary"))
            if (el.Subtype == 1001 && completePackage == false)  
            {
                EA.Package pkg = repository.GetPackageByID(el.PackageID);

                // Not version controlled, checked out to this user
                int versionControlState = pkg.VersionControlGetStatus();
                if (versionControlState == 0 || versionControlState == 2)
                {
                   
                    // delete External Reference
                    for (short idx = (short)(pkg.Elements.Count - 1); idx >=0; idx--)
                    {
                        if ( (el.ElementID == ((EA.Element)pkg.Elements.GetAt(idx)).ElementID) || (completePackage && el.Subtype == 1001) )
                        {
                            // with or without dialog
                            try
                            {
                                if (dialog)
                                {
                                    DialogResult res = MessageBox.Show("Delete element '" + el.Name +"'", "Delete element", MessageBoxButtons.YesNo);
                                    if (res == DialogResult.Yes)
                                    {
                                        pkg.Elements.DeleteAt(idx, false);
                                        if (completePackage == false) break;
                                    }
                                }
                                else
                                { pkg.Elements.DeleteAt(idx, false);
                                   if (completePackage == false) break;
                                }

                            }
                            catch { }

                        }

                    }
                    pkg.Update();
                }
                else
                {
                    string s = String.Format("Delete element '{0}' Version control state={1} not possible!", el.Name, versionControlState);
                    MessageBox.Show(s, "Package can't be changed, checked in?, break;");
                }
            }

            return;
        }




        // display behavior or definition for selected element
        private static void DisplayForSelectedElement(EA.Repository repository, DisplayMode showBehavior)
        {
            EA.ObjectType oType = repository.GetContextItemType();
            // Method found
            if (oType.Equals(EA.ObjectType.otMethod))
            {
                // display behavior for method
                BehaviorForOperation(repository, (EA.Method)repository.GetContextObject());

            }
            if (oType.Equals(EA.ObjectType.otDiagram))
            {
                // find parent element
                EA.Diagram dia = (EA.Diagram)repository.GetContextObject();
                if (dia.ParentID > 0)
                {
                    // find parent element
                    EA.Element parentEl = repository.GetElementByID(dia.ParentID);
                    //
                    ViewFromBehavior(repository, parentEl, showBehavior);
                }
                else
                {
                    // open diagram
                    repository.OpenDiagram(dia.DiagramID);
                }
            }


            // Connector / Message found
            if (oType.Equals(EA.ObjectType.otConnector))
            {
                EA.Connector con = (EA.Connector)repository.GetContextObject();
                if (con.Type.Equals("StateFlow"))
                {

                    EA.Method m = Util.GetOperationFromConnector(repository, con);
                    if (m != null)
                    {
                        if (showBehavior.Equals(DisplayMode.Behavior))
                        {
                            BehaviorForOperation(repository, m);
                        }
                        else
                        {
                            repository.ShowInProjectView(m);
                        }

                    }


                }

                if (con.Type.Equals("Sequence"))
                {
                    // If name is of the form: OperationName(..) the the operation is associated to an method
                    string opName = con.Name;
                    if (opName.EndsWith(")"))
                    {
                        // extract the name
                        int pos = opName.IndexOf("(");
                        opName = opName.Substring(0, pos);
                        EA.Element el = repository.GetElementByID(con.SupplierID);
                        // find operation by name
                        foreach (EA.Method op in el.Methods)
                        {
                            if (op.Name == opName)
                            {
                                BehaviorForOperation(repository, op);
                            }
                        }
                        if ((el.Type.Equals("Sequence") || el.Type.Equals("Object")) && el.ClassfierID > 0)
                        {
                            el = (EA.Element)repository.GetElementByID(el.ClassifierID);
                            foreach (EA.Method op in el.Methods)
                            {
                                if (op.Name == opName)
                                {
                                    if (showBehavior.Equals(DisplayMode.Behavior))
                                    {
                                        BehaviorForOperation(repository, op);
                                    }
                                    else
                                    {
                                        repository.ShowInProjectView(op);
                                    }

                                }
                            }
                        }

                    }
                }
            }

            // Element
            if (oType.Equals(EA.ObjectType.otElement))
            {
                EA.Element el = (EA.Element)repository.GetContextObject();

                if (el.Type.Equals("Activity"))
                {
                    // Open Behavior for Activity
                    Util.OpenBehaviorForElement(repository, el);


                }
                if (el.Type.Equals("Action"))
                {
                    foreach (EA.CustomProperty custproperty in el.CustomProperties)
                    {
                        if (custproperty.Name.Equals("kind") && custproperty.Value.Equals("CallOperation"))
                        {
                            ShowFromElement(repository, el, showBehavior);
                        }
                        if (custproperty.Name.Equals("kind") && custproperty.Value.Equals("CallBehavior"))
                        {
                            el = repository.GetElementByID(el.ClassfierID);
                            Util.OpenBehaviorForElement(repository, el);
                        }
                    }

                }
                if (el.Type.Equals("Activity") || el.Type.Equals("StateMachine") || el.Type.Equals("Interaction"))
                {
                    ViewFromBehavior(repository, el, showBehavior);
                }
            }
        }

        private static void ShowFromElement(EA.Repository repository, EA.Element el, DisplayMode showBehavior)
        {
            EA.Method method = Util.GetOperationFromAction(repository, el);
            if (method != null)
            {
                if (showBehavior.Equals(DisplayMode.Behavior))
                {
                    BehaviorForOperation(repository, method);
                }
                else
                {
                    repository.ShowInProjectView(method);
                }
            }
        }

        private static void ViewFromBehavior(EA.Repository repository, EA.Element el, DisplayMode showBehavior)
        {
            EA.Method method = Util.GetOperationFromBrehavior(repository, el);
            if (method != null)
            {
                if (showBehavior.Equals(DisplayMode.Behavior))
                {
                    BehaviorForOperation(repository, method);
                }
                else
                {
                    repository.ShowInProjectView(method);
                }
            }
        }

        private static void BehaviorForOperation(EA.Repository repository, EA.Method method)
        {
            string behavior = method.Behavior;
            if (behavior.StartsWith("{") & behavior.EndsWith("}"))
            {
                // get object according to behavior
                EA.Element el = repository.GetElementByGuid(behavior);
                // Activity
                if (el == null) { }
                else
                {
                    if (el.Type.Equals("Activity") || el.Type.Equals("Interaction") || el.Type.Equals("StateMachine"))
                    {
                        Util.OpenBehaviorForElement(repository, el);
                    }
                }
            }
        }

       
        /// <summary>
        /// Called when EA start model validation. Just shows a message box
        /// </summary>
        /// <param name="repository">the repository</param>
        /// <param name="args">the arguments</param>
        public override void EA_OnStartValidation(EA.Repository repository, object args)
        {
            MessageBox.Show("Validation started");
        }
        /// <summary>
        /// Called when EA ends model validation. Just shows a message box
        /// </summary>
        /// <param name="repository">the repository</param>
        /// <param name="args">the arguments</param>
        public override void EA_OnEndValidation(EA.Repository repository, object args)
        {
            MessageBox.Show("Validation ended");
        }
        /// <summary>
        /// called when the selected item changes
        /// This operation will show the guid of the selected element in the eaControl
        /// </summary>
        /// <param name="repository">the EA.Repository</param>
        /// <param name="guid">the guid of the selected item</param>
        /// <param name="ot">the object type of the selected item</param>
        public override void EA_OnContextItemChanged(EA.Repository repository, string guid, EA.ObjectType ot)
        { }
        /// <summary>
        /// Say Hello to the world
        /// </summary>


    }
}
