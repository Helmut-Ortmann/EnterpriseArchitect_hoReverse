using System;
using System.Windows.Forms;
using System.Xml;
using System.Text.RegularExpressions;
using System.IO;

namespace hoUtil
{
    public class Util
    {
        public Util()
        {

        }

        public static void ChangeClassNameToSynonyms(EA.Repository rep, EA.Element el)
        {
            if (el.Type.Equals("Class"))
            {
                // check if property 'Synonym' exists
                foreach (EA.TaggedValue tag in el.TaggedValues)
                {
                    if (tag.Name == "typeSynonyms")
                    {
                        if (tag.Value != el.Name)
                        {
                            el.Name = tag.Value;
                            el.Update();
                            break;
                        }
                    }
                }
                foreach (EA.Element elNested in el.Elements)
                {
                    ChangeClassNameToSynonyms(rep, elNested);
                }

            }
        }
        public static void ChangePackageClassNameToSynonyms(EA.Repository rep, EA.Package pkg)
        {
            // All elements in package
            foreach (EA.Element el in pkg.Elements)
            {
                if (el.Type.Equals("Class"))
                {
                    // class nested
                    ChangeClassNameToSynonyms(rep, el);
                }
            }
            // all packages in packages
            foreach (EA.Package pkgNested in pkg.Packages)
            {
                // package nested
                ChangePackageClassNameToSynonyms(rep, pkgNested);
            }
        }


        public static bool UpdateClass(EA.Repository rep, EA.Element el)
        {

            foreach (EA.Attribute a in el.Attributes)
            {
                UpdateAttribute(rep, a);
            }
            foreach (EA.Method m in el.Methods)
            {
                UpdateMethod(rep, m);
            }

            // over all nested classes
            foreach (EA.Element e in el.Elements)
            {
                UpdateClass(rep, e);
            }
            return true;
        }


        public static bool UpdateAttribute(EA.Repository rep, EA.Attribute a)
        {
            // no classifier defined
            if (a.ClassifierID == 0)
            {
                // find type from type_name
                int id = GetTypeId(rep, a.Type);
                if (id > 0)
                {
                    a.ClassifierID = id;
                    bool error = a.Update();
                    if (!error)
                    {
                        MessageBox.Show("Error write Attribute", a.GetLastError());
                        // Error occurred
                        return false;
                    }
                }
            }
            return true;
        }

        // Update Method Types
        public static bool UpdateMethod(EA.Repository rep, EA.Method m)
        {

            int id;

            // over all parameters
            foreach (EA.Parameter par in m.Parameters)
            {
                if ((par.ClassifierID == "") || (par.ClassifierID == "0"))
                {
                    // find type from type_name
                    id = GetTypeId(rep, par.Type);
                    if (id > 0)
                    {
                        par.ClassifierID = id.ToString();
                        bool error = par.Update();
                        if (!error)
                        {
                            MessageBox.Show("Error write Parameter", m.GetLastError());
                            return false;
                            // Error occurred
                        }
                    }

                }

            }
            // no classifier defined
            if ((m.ClassifierID == "") || (m.ClassifierID == "0"))
            {
                // find type from type_name
                id = GetTypeId(rep, m.ReturnType);
                if (id > 0)
                {
                    m.ClassifierID = id.ToString();
                    bool error = m.Update();
                    if (!error)
                    {
                        MessageBox.Show("Error write Method", m.GetLastError());
                        return false;
                        // Error occurred
                    }
                }
            }
            return true;
        }



        // Find type for name
        // 1. Search for name
        // 2. Search for Synonyms
        public static int GetTypeId(EA.Repository rep, string name)
        {
            int intReturn = 0;
            //
            // delete an '*' at the end of the variable name
            name = name.Replace("*", "");
            // remove a 'const ' from start of string
            // remove a 'volatile ' from start of string
            name = name.Replace("const", "");
            name = name.Replace("volatile", "");
            name = name.Trim();


            if (name.Equals("void") || name.Equals("void*")) return 0;
            string query = @"SELECT
                            o.object_id As OBJECT_ID
                            FROM  t_object  o
                            INNER  JOIN  t_objectproperties  p ON  o.object_id  =  p.object_id
                            where property = 'typeSynonyms' AND
                                  Object_Type in ('Class','PrimitiveType','DataType')  AND
                                  p.value = '" + name + "' " +
                            @" UNION
                               Select o.object_id
                               From t_object o
                                        where Object_Type in ('Class','PrimitiveType','DataType') AND name = '" + name + "' ";
            string str = rep.SQLQuery(query);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(str);

            XmlNode operationGuidNode = xmlDoc.SelectSingleNode("//OBJECT_ID");
            if (operationGuidNode != null)
            {
                intReturn = Convert.ToInt32(operationGuidNode.InnerText);
            }     

            return intReturn;
        }
       

        // Find the calling operation from a Call Operation Action
        public static EA.Method GetOperationFromAction(EA.Repository rep, EA.Element action) {
            EA.Method method = null;
            string query = @"select o.Classifier_guid AS CLASSIFIER_GUID
                      from t_object o 
                      where o.Object_ID = " + action.ElementID.ToString();
           string str = rep.SQLQuery(query);
           XmlDocument xmlDoc = new XmlDocument();
           xmlDoc.LoadXml(str);

           XmlNode operationGuidNode = xmlDoc.SelectSingleNode("//CLASSIFIER_GUID");
           if (operationGuidNode != null)
           {
               string guid = operationGuidNode.InnerText;
               method = rep.GetMethodByGuid(guid);
           }
           return method;           
        }

        // Find the calling operation from a Call Operation Action
        public static string GetClassifierGuid(EA.Repository rep,string guid)
        {
            string query = @"select o.Classifier_guid AS CLASSIFIER_GUID
                      from t_object o 
                      where o.EA_GUID = '" + guid + "'";
            string str = rep.SQLQuery(query);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(str);

            XmlNode operationGuidNode = xmlDoc.SelectSingleNode("//CLASSIFIER_GUID");
            guid = "";
            if (operationGuidNode != null)
            {
                guid = operationGuidNode.InnerText;
            }
            return guid;
        }

        

        // Gets the trigger associated with the element
        public static string GetTrigger(EA.Repository rep, string guid)
        {
            string query = @"select x.Description AS TRIGGER_GUID
                      from t_xref x 
                      where x.Client = '" + guid + "'    AND behavior = 'trigger' ";
            string str = rep.SQLQuery(query);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(str);

            XmlNode operationGuidNode = xmlDoc.SelectSingleNode("//TRIGGER_GUID");
            guid = "";
            if (operationGuidNode != null)
            {
                guid = operationGuidNode.InnerText;
            }
            return guid;
        }
        // Gets the signal associated with the element
        public static string GetSignal(EA.Repository rep, string guid)
        {
            string query = @"select x.Description AS SIGNAL_GUID
                      from t_xref x 
                      where x.Client = '" + guid + "'    AND behavior = 'event' ";
            string str = rep.SQLQuery(query);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(str);

            XmlNode operationGuidNode = xmlDoc.SelectSingleNode("//SIGNAL_GUID");
            guid = "";
            if (operationGuidNode != null)
            {
                guid = operationGuidNode.InnerText;
            }
            return guid;
        }
        // set "ShowBeh=1; in operation field StyleEx

        public static Boolean SetShowBehaviorInDiagram(EA.Repository rep, EA.Method m)
        {
            string updateStr = @"update t_operation set StyleEx = 'ShowBeh=1;'"  +
                       " where operationID = " + m.MethodID.ToString();
            rep.Execute(updateStr);
            return true;
        }

        public static Boolean SetFrameLinksToDiagram(EA.Repository rep, EA.Element frm, EA.Diagram dia)
        {
            string updateStr = @"update t_object set pdata1 = "+ dia.DiagramID + 
                       " where object_ID = " + frm.ElementID.ToString();
            rep.Execute(updateStr);
            return true;
        }        
        
        
        public static Boolean SetDiagramHasAttchaedLink(EA.Repository rep, EA.Element el)
        {
            string updateStr = @"update t_object set pdata1 = 'Diagram Note' " +
                       " where object_ID = " + el.ElementID.ToString() ;
            rep.Execute(updateStr);
            return true;
        }
        public static Boolean SetVcFlags (EA.Repository rep, EA.Package pkg, string flags)
        {
            string updateStr = @"update t_package set packageflags = '" + flags +"' " +
                       " where package_ID = " + pkg.PackageID.ToString();
            rep.Execute(updateStr);
            return true;
        }

        public static Boolean SetElementHasAttchaedLink(EA.Repository rep, EA.Element el, EA.Element elNote)
        {
            string updateStr = @"update t_object set pdata1 = 'Element Note', pdata2 = '" + el.ElementID.ToString()  + "', pdata4='Yes' " +
           " where object_ID = " + elNote.ElementID.ToString() ;
            rep.Execute(updateStr);


            return true;
        }

        public static Boolean SetBehaviorForOperation(EA.Repository rep, EA.Method op, EA.Element act)
        {
            
            string updateStr = @"update t_operation set behaviour = '" + act.ElementGUID + "' " +
           " where operationID = " + op.MethodID.ToString();
            rep.Execute(updateStr);


            return true;
        }
        


        // Find the operation from Activity / State Machine
        public static EA.Method GetOperationFromBrehavior(EA.Repository rep, EA.Element el)
        {
            EA.Method method = null;
            string query = "";
            string conString = GetConnectionString(rep); // due to shortcuts
            if (conString.Contains("DBType=3"))
            {   // Oracle DB
                query = 
                    @"select op.ea_guid AS EA_GUID
                      from t_operation op 
                      where Cast(op.Behaviour As Varchar2(38)) = '" + el.ElementGUID + "' ";
            }
            if (conString.Contains("DBType=1"))
                // SQL Server
            {    query = 
                      @"select op.ea_guid AS EA_GUID
                        from t_operation op 
                        where Substring(op.Behaviour,1,38) = '" + el.ElementGUID + "'" ;

            }

            if (conString.Contains(".eap"))
                // SQL Server
            {     query = 
                      @"select op.ea_guid AS EA_GUID
                        from t_operation op 
                        where op.Behaviour = '" + el.ElementGUID + "'" ;

            }
            if ((! conString.Contains("DBType=1")) &&  // SQL Server, DBType=0 MySQL
               (!  conString.Contains("DBType=3")) &&  // Oracle
               (!  conString.Contains(".eap")))// Access
            {
                query =
                  @"select op.ea_guid AS EA_GUID
                        from t_operation op 
                        where op.Behaviour = '" + el.ElementGUID + "'";

            }

            string str = rep.SQLQuery(query);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(str);

            XmlNode operationGuidNode = xmlDoc.SelectSingleNode("//EA_GUID");
            if (operationGuidNode != null)
            {
                string guid = operationGuidNode.InnerText;
                method = rep.GetMethodByGuid(guid);
            }
            return method;
        }

//        // read PDATA1
//        public static EA.Element getPDATA(EA.Repository rep, int ID)
//        {
//            EA.Element el = null;
//            string query = "";
//            query =
//                    @"select pdata1 AS PDATA1
//                      from t_object o 
//                      where Cast(op.Behaviour As Varchar2(38)) = '" + el.ElementGUID + "'";

//            if (rep.ConnectionString.Contains("DBType=3"))
//            {   // Oracle DB
//                query =
//                    @"select op.ea_guid AS EA_GUID
//                      from t_operation op 
//                      where Cast(op.Behaviour As Varchar2(38)) = '" + el.ElementGUID + "'";
//            }
//            if (rep.ConnectionString.Contains("DBType=1"))
//            // SQL Server
//            {
//                query =
//                     @"select op.ea_guid AS EA_GUID
//                        from t_operation op 
//                        where Substring(op.Behaviour,1,38) = '" + el.ElementGUID + "'";

//            }

//            if (rep.ConnectionString.Contains(".eap"))
//            // SQL Server
//            {
//                query =
//                    @"select op.ea_guid AS EA_GUID
//                        from t_operation op 
//                        where op.Behaviour = '" + el.ElementGUID + "'";

//            }
//            if ((!rep.ConnectionString.Contains("DBType=1")) &&  // SQL Server, DBType=0 MySQL
//               (!rep.ConnectionString.Contains("DBType=3")) &&  // Oracle
//               (!rep.ConnectionString.Contains(".eap")))// Access
//            {
//                query =
//                  @"select op.ea_guid AS EA_GUID
//                        from t_operation op 
//                        where op.Behaviour = '" + el.ElementGUID + "'";

//            }

//            string str = rep.SQLQuery(query);
//            XmlDocument XmlDoc = new XmlDocument();
//            XmlDoc.LoadXml(str);

//            XmlNode operationGUIDNode = XmlDoc.SelectSingleNode("//EA_GUID");
//            if (operationGUIDNode != null)
//            {
//                string GUID = operationGUIDNode.InnerText;
//                method = rep.GetMethodByGuid(GUID);
//            }
//            return method;
//        }



        public static EA.Method GetOperationFromConnector(EA.Repository rep, EA.Connector con)
        {
            EA.Method method = null;
            string query = "";
            if (GetConnectionString(rep).Contains("DBType=3"))
                //pdat3: 'Activity','Sequence', (..)
            {   // Oracle DB
                query =
                    @"select description AS EA_GUID
                      from t_xref x 
                      where Cast(x.client As Varchar2(38)) = '" + con.ConnectorGUID + "'" +
                                                                " AND Behavior = 'effect' ";
            }
            if (GetConnectionString(rep).Contains("DBType=1"))
            {   // SQL Server

                query =
                      @"select description AS EA_GUID
                        from t_xref x 
                        where Substring(x.client,1,38) = " + "'" + con.ConnectorGUID + "'" +
                           " AND Behavior = 'effect' "                   
                                          ;
            }
            if (GetConnectionString(rep).Contains(".eap"))
            {

                query =
                      @"select description AS EA_GUID
                        from t_xref x 
                        where client = " + "'" + con.ConnectorGUID + "'" +
                           " AND Behavior = 'effect' "
                                          ;
            }
            if ((! GetConnectionString(rep).Contains("DBType=1")) &&  // SQL Server, DBType=0 MySQL
                (! GetConnectionString(rep).Contains("DBType=3")) &&  // Oracle
                (! GetConnectionString(rep).Contains(".eap")))// Access
            {
                query =
                @"select description AS EA_GUID
                        from t_xref x 
                        where client = " + "'" + con.ConnectorGUID + "'" +
                             " AND Behavior = 'effect' "
                                            ;

            }


            string str = rep.SQLQuery(query);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(str);

            //string type = "";
            //XmlNode pdat3Node = XmlDoc.SelectSingleNode("//PDAT3");
            //if (pdat3Node != null)
            //{
            //    type = pdat3Node.InnerText;
                
            //}
            //if ( type.EndsWith(")")) // Operation
            //{ 
            string guid = null;
                XmlNode operationGuidNode = xmlDoc.SelectSingleNode("//EA_GUID");
                if (operationGuidNode != null)
                {
                    guid = operationGuidNode.InnerText;
                    method = rep.GetMethodByGuid(guid);
                }
                if (method == null)
                {

                     OpenBehaviorForElement(rep, rep.GetElementByGuid(guid));
                }
            //}

            return method;
        }
        public static void UpdateVc(EA.Repository rep, EA.Package pkg)
        {
            if (pkg.IsVersionControlled)
            {
                // find                  VC=...;
                // replace by:           VC=currentState();
                string flags = pkg.Flags;
                Regex pattern = new Regex(@"VC=[^;]+;");
                Match regMatch = pattern.Match(flags);
                while (regMatch.Success)
                {
                    // delete old string
                    flags = flags.Replace(regMatch.Value, "");
                    regMatch = regMatch.NextMatch();
                    // add new string
                }
                flags = flags + "VC=" + GetVCstate(pkg, false) + ";";
                try
                {
                    SetVcFlags(rep, pkg, flags);
                }
                catch (Exception e)
                {
                    string s = e.Message + " ;" + pkg.GetLastError();
                    s = s + "!";
                }


            }
            // recursive package
            foreach (EA.Package pkg1 in pkg.Packages)
            {
                UpdateVc(rep, pkg1);
            }
        }

        //------------------------------------------------------------------------------------------
        // resetVCRecursive   If package is controls: Reset packageflags field of package, work for all packages recursive 
        //------------------------------------------------------------------------------------------
        // packageflags:  Recurse=0;VCCFG=unchanged;
        public static void ResetVcRecursive(EA.Repository rep, EA.Package pkg)
        {
            ResetVc(rep, pkg);
            foreach (EA.Package p in pkg.Packages)
            {
                ResetVc(rep, pkg);
            }
        }
        //------------------------------------------------------------------------------------------
        // resetVC   If package is controls: 
        //------------------------------------------------------------------------------------------
        // - Reset packageflags field of package 
        // - Check path to *.xml file 
        //------------------------------------------------------------------------------------------
        // packageflags:  Recurse=0;VCCFG=unchanged;
        public static void ResetVc(EA.Repository rep, EA.Package pkg)
        {
            if (pkg.IsVersionControlled)
            {
                // check path to *.xml file
                string filePath = GetFilePath(rep, pkg);
                FileInfo theFile = new FileInfo(filePath);
                if  (!theFile.Exists)
                {
                    MessageBox.Show( "XMLPath:" + pkg.XMLPath +
                        "\nPath to sandbox:\n" + filePath +
                        "\nPackageFlags:\n" + pkg.Flags +
                        "\n\n Try update sandbox/local directory or\n" +  
                        "\n 1. Checkout correct *.xml file" +
                        "\n 2. Update the wrong paths of xml file" +
                        "\n 3. Checkin" +
                        "\n 4. Deactivate Version Control for package in EA" +
                        "\n 5. Import Package",
                        "*.xml file for Package" + pkg.Name + " not found!");                }


                // find                  VC=...;
                string flags = pkg.Flags;
                Regex pattern = new Regex(@"VCCFG=[^;]+;");
                Match regMatch = pattern.Match(flags);
                if (regMatch.Success)
                {
                    // delete old string
                    flags = @"Recurse=0;" + regMatch.Value;
                }
                else
                {
                    return;
                }
                // write flags
                try
                {
                    SetVcFlags(rep, pkg, flags);
                }
                catch (Exception e)
                {
                    string s = e.Message + " ;" + pkg.GetLastError();
                    s = s + "!";
                }


            }
            // recursive package
            //foreach (EA.Package pkg1 in pkg.Packages)
            //{
            //    updateVC(rep, pkg1);
            //}
        }
        public static string GetVCstate(EA.Package pkg, Boolean isLong) {
                        long state = 0;
                        string[] checkedOutStatusLong = { "Uncontrolled",
                                                      "Checked in",
                                                      "Checked out to this user",
                                                      "Read only version",
                                                      "Checked out to another user",
                                                      "Offline checked in",
                                                      "Offline checked out by user",
                                                      "Offline checked out by other user", 
                                                      "Deleted" };
                        string[] checkedOutStatusShort = { "Uncontrolled",
                                                      "Checked in",
                                                      "Checked out",
                                                      "Read only",
                                                      "Checked out",
                                                      "Offline checked in",
                                                      "Offline checked out",
                                                      "Offline checked out", 
                                                      "Deleted" };

                        try
                        {
                            state = pkg.VersionControlGetStatus();
                        }
                        catch (Exception e)
                        {
                            if (isLong) return "VC State Error: " + e.Message;
                            else return "State Error";
                        }

                        if (isLong) return checkedOutStatusLong[state];
                        else return checkedOutStatusShort[state];
        }
        public static string GetFilePath(EA.Repository rep, EA.Package pkg)
        {
            string path = "";
            // Get VCCFG=...;
            Regex pattern = new Regex(@"VCCFG=[^;]+");
            Match regMatch = pattern.Match(pkg.Flags);
            if (regMatch.Success)
            {
                // get VCCFG
                var uniqueId = regMatch.Value.Substring(6);
                // get path for UiqueId
                Environment.CurrentDirectory = Environment.GetEnvironmentVariable("appdata");
                string s1 = @"Sparx Systems\EA\paths.txt";
                TextReader tr = new StreamReader(s1);
                string line;
                pattern = new Regex(@"(id=" + uniqueId + @").+(path=[^;]+)");
                while ((line = tr.ReadLine()) != null)
                {

                    regMatch = pattern.Match(line);
                    if (regMatch.Success)
                    {
                        path = regMatch.Groups[2] + @"\" + pkg.XMLPath;
                        path = path.Substring(5);
                        break;
                    }


                }
                tr.Close();
                if (path == "")
                {
                    rep.WriteOutput("Debug", "VCCFG=... not found in" + s1 + " " + pkg.Name, 0);
                }
            }
            else
            {
                rep.WriteOutput("Debug", "VCCFG=... not found:" + pkg.Name, 0);
            }

            return path;
        }
       
        public static Boolean GetLatest(EA.Repository rep, EA.Package pkg, Boolean recursive, ref int count, int level, ref int errorCount)
        {
            if (pkg.IsControlled)
            {
                level = level + 1;
                // check if checked out

                string path = GetFilePath(rep, pkg);
                string fText = "";
                //rep.WriteOutput("Debug", "Path:" + pkg.Name + path, 0);
                string sLevel = new string(' ', level * 2);
                rep.WriteOutput("Debug", sLevel + (count+1).ToString(",0") + " Work for:" + path, 0);
                if (path != "")
                {
                    count = count + 1;
                    rep.ShowInProjectView(pkg);
                    // delete a potential write protection
                    try
                    {
                        FileInfo fileInfo = new FileInfo(path);
                        FileAttributes attributes = (FileAttributes)(fileInfo.Attributes - FileAttributes.ReadOnly);
                        System.IO.File.SetAttributes(fileInfo.FullName, attributes);
                        System.IO.File.Delete(path);
                    }
                    catch (FileNotFoundException e)
                    {
                        fText = path + " " + e.Message;
                        rep.WriteOutput("Debug", fText, 0);
                        errorCount = errorCount + 1;
                    }
                    catch (DirectoryNotFoundException e)
                    {
                        fText = path + " " + e.Message;
                        rep.WriteOutput("Debug", fText, 0);
                        errorCount = errorCount + 1;
                    }
                    // get latest
                    try
                    {
                        // to make sure pkg is the correct reference
                        // new load of pkg after GetLatest
                        string pkgGuid = pkg.PackageGUID;
                        pkg.VersionControlGetLatest(true);
                        pkg = rep.GetPackageByGuid(pkgGuid);
                        count = count + 1;
                    }
                    catch
                    {
                        fText = path + " " + pkg.GetLastError();
                        rep.WriteOutput("Debug", fText, 0);
                        errorCount = errorCount + 1;
                    }

                }
                else
                {
                    fText = pkg.XMLPath + " invalid path";
                    rep.WriteOutput("Debug", fText, 0);
                    errorCount = errorCount + 1;

                }
            }

            //rep.WriteOutput("Debug", "Recursive:" +recursive.ToString(), 0);
            if (recursive)
            {
                //rep.WriteOutput("Debug","Recursive count:" + pkg.Packages.Count.ToString(), 0);
                // over all contained packages
                foreach (EA.Package pkgNested in pkg.Packages)
                {
                    //rep.WriteOutput("Debug", "Recursive:"+ pkgNested.Name, 0);
                    GetLatest(rep, pkgNested, true, ref count, level, ref errorCount);

                }
            }
            return true;

        }
        public static string GetConnectionString(EA.Repository rep) {
            string s = rep.ConnectionString;
            if (s.Contains("DBType="))
            {
                return s;
            }
            else
            {
                FileInfo f = new FileInfo(s);
                if (f.Length > 1025)
                {
                    return s;
                }
                else
                {
                    return System.IO.File.ReadAllText(s);
                }
            }
            

        }
        public static void OpenBehaviorForElement(EA.Repository repository, EA.Element el)
        {
            // find the diagram
            if (el.Diagrams.Count > 0)
            {
                // get the diagram
                EA.Diagram dia = (EA.Diagram)el.Diagrams.GetAt(0);
                // open diagram
                repository.OpenDiagram(dia.DiagramID);
            }
            // no diagram found, select element
            repository.ShowInProjectView(el);
        }
    }
}
