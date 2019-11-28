using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using EA;
using hoReverse.hoUtils.svnUtil;
using Attribute = EA.Attribute;
using File = System.IO.File;

// ReSharper disable once CheckNamespace
namespace hoReverse.hoUtils
{
    public static class HoUtil
    {
        public static string ObjectTypeToString(ObjectType objectType)
        {
            switch (objectType)
            {
                case ObjectType.otPackage:
                    return "Package";
                case ObjectType.otElement:
                    return "Element";
                case ObjectType.otDiagram:
                    return "Diagram";
                case ObjectType.otMethod:
                    return "Operation";
                case ObjectType.otAttribute:
                    return "Attribute";
                default:
                    return "unknown object type";
            }
        }

        /// <summary>
        /// Get the current selected Element. 
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="noErrorMessage"></param>
        /// <returns></returns>
        public static EA.Element GetElementFromContextObject(EA.Repository rep, bool noErrorMessage=true)
        {
            EA.Element el = null;
            EA.Diagram dia;
            EA.ObjectType objectType = rep.GetContextItemType();
            switch (objectType)
            {
                case EA.ObjectType.otAttribute:
                    EA.Attribute a = (EA.Attribute) rep.GetContextObject();
                    el = rep.GetElementByID(a.ParentID);
                    break;
                case ObjectType.otMethod:
                    EA.Method m = (EA.Method) rep.GetContextObject();
                    el = rep.GetElementByID(m.ParentID);
                    break;
                case ObjectType.otElement:
                    el = (EA.Element) rep.GetContextObject();
                    break;
                case ObjectType.otDiagram:
                    dia = rep.GetCurrentDiagram();
                    if (dia?.SelectedObjects.Count == 1)
                    {
                        EA.DiagramObject objSelected = (EA.DiagramObject) dia.SelectedObjects.GetAt(0);
                        el = rep.GetElementByID(objSelected.ElementID);
                    }
                    break;
                case ObjectType.otNone:
                    dia = rep.GetCurrentDiagram();
                    if (dia?.SelectedObjects.Count == 1)
                    {
                        EA.DiagramObject objSelected = (EA.DiagramObject) dia.SelectedObjects.GetAt(0);
                        el = rep.GetElementByID(objSelected.ElementID);
                    }
                    break;
                default:
                    if (! noErrorMessage) MessageBox.Show("No Element selected");
                    break;
            }
            return el;
        }

        /// <summary>
        /// Start file
        /// </summary>
        /// <param name="filePath"></param>
        public static void StartFile(string filePath)
        {
            try
            {
                // start file with the program defined in Windows for this file
                Process.Start(filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($@"{ex.Message}:{Environment.NewLine}'{filePath}'",
                    $@"Can't open file {Path.GetFileName(filePath)}");
            }
        }

        /// <summary>
        /// Start a program with parameters. Make sure the '%PATH%' Environment variable is correctly set.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="par"></param>
        public static void StartApp(string app, string par)
        {
            Process p = new Process
            {
                StartInfo =
                {
                    FileName = app,
                    Arguments = par,
                    WindowStyle = ProcessWindowStyle.Hidden
        }
            };
            try
            {
                p.Start();

            }
            catch (Exception e)
            {
                MessageBox.Show(p.StartInfo.FileName + " " +
                                p.StartInfo.Arguments + "\n\n" +
                                "Have you set the %path% environment variable?\n\n" + e,
                    $"Can't start '{app}'!");
            }
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public static string GetWildCard(EA.Repository rep)
        {
            string cnString = rep.ConnectionString.ToUpper();

            if (cnString.EndsWith(".EAP") || cnString.Contains(".EAPX"))
            {
                FileInfo f = new FileInfo(cnString);
                if (f.Length > 20000) return "*";
                TextReader tr = new StreamReader(cnString);
                // ReSharper disable once PossibleNullReferenceException
                string shortcut = tr.ReadLine().ToUpper();
                tr.Close();
                if (shortcut.Contains(".EAP") || shortcut.Contains(".EAPX")) return "*";
                if (shortcut.Contains("DBTYPE=")) return "%";
                return "";

            }
            return "%";
        }

        public static void SetSequenceNumber(EA.Repository rep, EA.Diagram dia,
            EA.DiagramObject obj, string sequence)
        {
            if (obj == null) return;

            string updateStr = @"update t_DiagramObjects set sequence = " + sequence +
                               " where diagram_id = " + dia.DiagramID +
                               " AND instance_id = " + obj.InstanceID;

            rep.Execute(updateStr);
        }

        public static void AddSequenceNumber(Repository rep, Diagram dia)
        {

            string updateStr = @"update t_DiagramObjects set sequence = sequence + 1 " +
                               " where diagram_id = " + dia.DiagramID;

            rep.Execute(updateStr);
        }

        public static int GetHighestSequenceNoFromDiagram(Repository rep, Diagram dia)
        {
            int sequenceNumber = 0;
            string query = @"select sequence from t_diagramobjects do " +
                           "  where do.Diagram_ID = " + dia.DiagramID +
                           "  order by 1 desc";
            string str = rep.SQLQuery(query);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(str);

            XmlNode operationGuidNode = xmlDoc.SelectSingleNode("//SEQUENCE_NUMBER");
            if (operationGuidNode != null)
            {
                sequenceNumber = Convert.ToInt32(operationGuidNode.InnerText);
            }
            return sequenceNumber;
        }



        //--------------------------------------------------------------------------------------------------------------
        // setLineStyleForLink  Set line style for a digram link
        //--------------------------------------------------------------------------------------------------------------
        // linestyle
        // LH = "Line Style: Lateral Horizontal";
        // LV = "Line Style: Lateral Vertical";
        // TH  = "Line Style: Tree Horizontal";
        // TV = "Line Style: Tree Vertical";
        // OS = "Line Style: Orthogonal Square";
        // OR =              Orthogonal Round
        // A =               Automatic
        // D =               Direct
        // C =               Customer
        // B =               Bezier
        // NO=               make nothing
        public static void SetLineStyleForDiagramLink(string lineStyle, DiagramLink link)
        {
            lineStyle = lineStyle + "  ";
            if (lineStyle.Substring(0, 2).ToUpper() == "NO") return;
            if (lineStyle.Substring(0, 2) == "TH") lineStyle = "H ";
            if (lineStyle.Substring(0, 2) == "TV") lineStyle = "V ";
            if (lineStyle.Substring(0, 1) == "D") link.Style = "Mode=1;EOID=A36C0F5C;SOID=3ECFB522;Color=-1;LWidth=0;";
            else if (lineStyle.Substring(0, 1) == "A")
                link.Style = "Mode=2;EOID=A36C0F5C;SOID=3ECFB522;Color=-1;LWidth=0;";
            else if (lineStyle.Substring(0, 1) == "C")
                link.Style = "Mode=3;EOID=A36C0F5C;SOID=3ECFB522;Color=-1;LWidth=0;";
            else if (lineStyle.Substring(0, 1) == "B")
                link.Style = "Mode=8;EOID=61B36ED5;SOID=08967F1E;Color=-1;LWidth=0;";
            else
            {
                link.Style = "Mode=3;EOID=A36C0F5C;SOID=3ECFB522;Color=-1;LWidth=0;TREE=" +
                             lineStyle.Trim() + ";";

            }
            link.Update();
        }


        //--------------------------------------------------------------------------------------------------------------
        // SetLineStyleDiagramObjectsAndConnectors  Set line style for diagram objects and connectors
        //--------------------------------------------------------------------------------------------------------------
        // linestyle
        // LH = "Line Style: Lateral Horizontal";
        // LV = "Line Style: Lateral Vertical";
        // TH  = "Line Style: Tree Horizontal";
        // TV = "Line Style: Tree Vertical";
        // OS = "Line Style: Orthogonal Square";
        // OR =              Orthogonal Round
        // A =               Automatic
        // D =               Direct
        // C =               Customer 
        // B =               Bezier
        public static void SetLineStyleDiagramObjectsAndConnectors(EA.Repository rep, EA.Diagram d, string lineStyle)
        {
            EA.Collection selectedObjects = d.SelectedObjects;
            EA.Connector selectedConnector = d.SelectedConnector;
            // store current diagram
            rep.SaveDiagram(d.DiagramID);
            foreach (EA.DiagramLink link in d.DiagramLinks)
            {
                if (link.IsHidden == false)
                {

                    // check if connector is connected with diagram object
                    EA.Connector c = rep.GetConnectorByID(link.ConnectorID);
                    foreach (EA.DiagramObject dObject in d.SelectedObjects)
                    {
                        if (c.ClientID == dObject.ElementID | c.SupplierID == dObject.ElementID)
                        {

                            SetLineStyleForDiagramLink(lineStyle, link);
                        }
                    }
                    if (c.ConnectorID == selectedConnector?.ConnectorID)
                    {
                        SetLineStyleForDiagramLink(lineStyle, link);
                    }
                }
            }
            rep.ReloadDiagram(d.DiagramID);
            if (selectedConnector != null) d.SelectedConnector = selectedConnector;
            foreach (EA.DiagramObject dObject in selectedObjects)
            {
                //d.SelectedObjects.AddNew(el.ElementID.ToString(), el.Type);
                d.SelectedObjects.AddNew(dObject.ElementID.ToString(), dObject.ObjectType.ToString());
            }
            //d.Update();
            d.SelectedObjects.Refresh();
        }

        //--------------------------------------------------------------------------------------------------------------
        // SetLineStyleDiagram  Set line style for a diagram (all visible connectors)
        //--------------------------------------------------------------------------------------------------------------
        // linestyle
        // LH = "Line Style: Lateral Horizontal";
        // LV = "Line Style: Lateral Vertical";
        // TH  = "Line Style: Tree Horizontal";
        // TV = "Line Style: Tree Vertical";
        // OS = "Line Style: Orthogonal Square";
        // OR =              Orthogonal Round
        // A =               Automatic
        // D =               Direct
        // C =               Customer       


        public static void SetLineStyleDiagram(Repository rep, Diagram d, string lineStyle)
        {
            // store current diagram
            rep.SaveDiagram(d.DiagramID);
            // all links
            foreach (DiagramLink link in d.DiagramLinks)
            {
                if (link.IsHidden == false)
                {
                    SetLineStyleForDiagramLink(lineStyle, link);
                }

            }
            rep.ReloadDiagram(d.DiagramID);
        }


        private static void ChangeClassNameToSynonyms(Repository rep, EA.Element el)
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

        public static void ChangePackageClassNameToSynonyms(Repository rep, Package pkg)
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
            foreach (Package pkgNested in pkg.Packages)
            {
                // package nested
                ChangePackageClassNameToSynonyms(rep, pkgNested);
            }
        }


        // ReSharper disable once UnusedMethodReturnValue.Global
        public static bool UpdateClass(Repository rep, EA.Element el)
        {

            foreach (Attribute a in el.Attributes)
            {
                UpdateAttribute(rep, a);
            }
            foreach (Method m in el.Methods)
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

        // ReSharper disable once UnusedMethodReturnValue.Global
        public static bool UpdatePackage(Repository rep, Package pkg)
        {
            foreach (EA.Element el in pkg.Elements)
            {
                UpdateClass(rep, el);
            }
            foreach (Package pkg1 in pkg.Packages)
            {
                UpdatePackage(rep, pkg1);
            }

            return true;
        }



        // ReSharper disable once UnusedMethodReturnValue.Global
        public static bool UpdateAttribute(Repository rep, Attribute a)
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
        // ReSharper disable once UnusedMethodReturnValue.Global
        public static bool UpdateMethod(Repository rep, Method m)
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
                        // Error occured
                    }
                }
            }
            return true;
        }



        // Find type for name
        // 1. Search for name (if type contains a '*' search for type with '*' and for type without '*'
        // 2. Search for Synonyms
        public static int GetTypeId(Repository rep, string name)
        {
            int intReturn = 0;
            //Boolean isPointer = false;
            //if (name.Contains("*")) isPointer = true;
            //
            // delete an '*' at the end of the type name

            // remove a 'const ' from start of string
            // remove a 'volatile ' from start of string
            name = name.Replace("const", "");
            name = name.Replace("volatile", "");
            //name = name.Replace("*", "");
            name = name.Trim();

//            if (isPointer) {
//                string queryIsPointer = @"SELECT o.object_id As OBJECT_ID
//                            FROM  t_object  o
//                            INNER  JOIN  t_objectproperties  p ON  o.object_id  =  p.object_id
//                            where property = 'typeSynonyms' AND
//                                  Object_Type in ('Class','PrimitiveType','DataType','Enumeration')  AND
//                                  p.value = '" + name + "*' " +
//                            @" UNION
//                               Select o.object_id
//                               From t_object o
//                                        where Object_Type in ('Class','PrimitiveType','DataType','Enumeration') AND name = '" + name + "*' ";
//                string strIsPointer = rep.SQLQuery(queryIsPointer);
//                XmlDocument XmlDocIsPointer = new XmlDocument();
//                XmlDocIsPointer.LoadXml(strIsPointer);

//                XmlNode operationGUIDNodeIsPointer = XmlDocIsPointer.SelectSingleNode("//OBJECT_ID");
//                if (operationGUIDNodeIsPointer != null)
//                {
//                    intReturn = Convert.ToInt32(operationGUIDNodeIsPointer.InnerText);
//                }     
//            }

            if (intReturn == 0)
            {
                //if (name.Equals("void") || name.Equals("void*")) return 0;
                string query = @"SELECT o.object_id As OBJECT_ID
                            FROM  t_object  o
                            INNER  JOIN  t_objectproperties  p ON  o.object_id  =  p.object_id
                            where property = 'typeSynonyms' AND
                                  Object_Type in ('Class','PrimitiveType','DataType','Enumeration')  AND
                                  p.value = '" + name + "' " +
                               @" UNION
                               Select o.object_id
                               From t_object o
                                        where Object_Type in ('Class','PrimitiveType','DataType','Enumeration') AND name = '" +
                               name + "' ";
                string str = rep.SQLQuery(query);
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(str);

                XmlNode operationGuidNode = xmlDoc.SelectSingleNode("//OBJECT_ID");
                if (operationGuidNode != null)
                {
                    intReturn = Convert.ToInt32(operationGuidNode.InnerText);
                }
            }


            return intReturn;
        }

        public static int GetTypeFromName(Repository rep, ref string name, ref string type)
        {
            var id = GetTypeId(rep, type);
            if (id == 0 & type.Contains("*"))
            {
                type = type.Remove(type.IndexOf("*", StringComparison.Ordinal), 1);
                name = "*" + name;
                id = GetTypeId(rep, type);
                if (id == 0 & type.Contains("*"))
                {
                    type = type.Replace("*", "");
                    name = "*" + name;
                    id = GetTypeId(rep, type);
                }
            }


            return id;

        }

        //------------------------------------------------------------------------------------------------------------------------------------
        // Find the Parameter of a Activity
        //------------------------------------------------------------------------------------------------------------------------------------
        // par Parameter of Operation (only if isReturn = false)
        // act Activity
        // Parameter wird aufgrund des Alias-Namens gefunden
        //
        // 
        public static EA.Element GetParameterFromActivity(Repository rep, EA.Parameter par, EA.Element act,
            Boolean isReturn = false)
        {

            string aliasName;
            if (isReturn)
            {
                aliasName = "return:";
            }
            else
            {
                aliasName = "par_" + par.Position;
            }

            EA.Element parTrgt = null;
            string query = @"select o2.ea_guid AS CLASSIFIER_GUID
                      from t_object o1 INNER JOIN t_object o2 on ( o2.parentID = o1.object_id)
                      where o1.Object_ID = " + act.ElementID +
                           " AND  o2.Alias like '" + aliasName + GetWildCard(rep) + "'";
            string str = rep.SQLQuery(query);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(str);

            XmlNode operationGuidNode = xmlDoc.SelectSingleNode("//CLASSIFIER_GUID");
            if (operationGuidNode != null)
            {
                string guid = operationGuidNode.InnerText;
                parTrgt = rep.GetElementByGuid(guid);
            }
            return parTrgt;
        }

        // Find the calling operation from a Call Operation Action
        public static Method GetOperationFromAction(Repository rep, EA.Element action)
        {
            Method method = null;
            string query = @"select o.Classifier_guid AS CLASSIFIER_GUID
                      from t_object o 
                      where o.Object_ID = " + action.ElementID;
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
        public static string GetParameterType(Repository rep, string actionPinGuid)
        {
            string query = @"SELECT par.type AS OPTYPE 
			    from t_object o  inner join t_operationparams par on (o.classifier_guid = par.ea_guid)
                where o.ea_guid = '" + actionPinGuid + "' ";
            string str = rep.SQLQuery(query);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(str);

            XmlNode typeGuidNode = xmlDoc.SelectSingleNode("//OPTYPE");
            if (typeGuidNode != null)
            {
                return typeGuidNode.InnerText;

            }
            return "";
        }


        // Find the calling operation from a Call Operation Action
        public static Method GetOperationFromCallAction(Repository rep, EA.Element obj)
        {
            string wildCard = GetWildCard(rep);
            string query =
                @"SELECT op.ea_guid AS OPERATION from (t_object o inner join t_operation op on (o.classifier_guid = op.ea_guid))
               inner join t_xref x on (x.client = o.ea_guid)
			   where x.name = 'CustomProperties' and
			             x.description like '" + wildCard + "CallOperation" + wildCard +
                "' and o.object_id = " + obj.ElementID;
            string str = rep.SQLQuery(query);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(str);

            XmlNode operationGuidNode = xmlDoc.SelectSingleNode("//OPERATION");
            if (operationGuidNode != null)
            {
                var guid = operationGuidNode.InnerText;
                return rep.GetMethodByGuid(guid);
            }
            return null;
        }

        // Find the calling operation from a Call Operation Action
        public static string GetClassifierGuid(Repository rep, string guid)
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


        // Gets the trigger associated with the connector / element
        public static string GetTrigger(Repository rep, string guid)
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
        public static string GetSignal(Repository rep, string guid)
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

        // Gets the composite element for a diagram GUID
        public static string GetElementFromCompositeDiagram(Repository rep, string diagramGuid)
        {
            string query = @"select o.ea_guid AS COMPOSITE_GUID
                      from t_xref x INNER JOIN t_object o on (x.client = o.ea_guid and type = 'element property')
                      where x.supplier = '" + diagramGuid + "'    ";
            string str = rep.SQLQuery(query);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(str);

            XmlNode operationGuidNode = xmlDoc.SelectSingleNode("//COMPOSITE_GUID");
            diagramGuid = "";
            if (operationGuidNode != null)
            {
                diagramGuid = operationGuidNode.InnerText;
            }
            return diagramGuid;
        }
        // set "ShowBeh=1; in operation field StyleEx

        // ReSharper disable once UnusedMethodReturnValue.Global
        public static Boolean SetShowBehaviorInDiagram(Repository rep, Method m)
        {
            string updateStr = @"update t_operation set StyleEx = 'ShowBeh=1;'" +
                               " where operationID = " + m.MethodID;
            rep.Execute(updateStr);
            return true;
        }

        // ReSharper disable once UnusedMethodReturnValue.Global
        public static Boolean SetFrameLinksToDiagram(Repository rep, EA.Element frm, Diagram dia)
        {
            string updateStr = @"update t_object set pdata1 = " + dia.DiagramID +
                               " where object_ID = " + frm.ElementID;
            rep.Execute(updateStr);
            return true;
        }

        /// <summary>
        /// Set Activity as Composite Diagram
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="el"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        // ReSharper disable once UnusedMethodReturnValue.Global
        public static Boolean SetActivityCompositeDiagram(Repository rep, EA.Element el, string s)
        {
            string updateStr = @"update t_object set pdata1 = '" + s + "', ntype = 8 " +
                               " where object_ID = " + el.ElementID;
            rep.Execute(updateStr);
            return true;
        }

        // ReSharper disable once UnusedMethodReturnValue.Global
        public static Boolean SetElementPdata1(Repository rep, EA.Element el, string s)
        {
            string updateStr = @"update t_object set pdata1 = '" + s + "' " +
                               " where object_ID = " + el.ElementID;
            rep.Execute(updateStr);
            return true;
        }

        // ReSharper disable once UnusedMethodReturnValue.Global
        public static bool SetElementPdata3(Repository rep, EA.Element el, string s)
        {
            string updateStr = @"update t_object set pdata3 = '" + s + "' " +
                               " where object_ID = " + el.ElementID;
            rep.Execute(updateStr);
            return true;
        }





        // ReSharper disable once UnusedMethodReturnValue.Global
        public static Boolean SetConnectorGuard(Repository rep, int connectorId, string connectorGuard)
        {

            string updateStr = @"update t_connector set pdata2 = '" + connectorGuard + "' " +
                               " where Connector_Id = " + connectorId;
            rep.Execute(updateStr);


            return true;
        }

        // ReSharper disable once UnusedMethodReturnValue.Global
        public static Boolean SetDiagramHasAttachedLink(Repository rep, EA.Element el)
        {
            SetElementPdata1(rep, el, "Diagram Note");
            return true;
        }

        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedMethodReturnValue.Global
        public static Boolean SetVcFlags(Repository rep, Package pkg, string flags)
        {
            string updateStr = @"update t_package set packageflags = '" + flags + "' " +
                               " where package_ID = " + pkg.PackageID;
            rep.Execute(updateStr);
            return true;
        }

        // ReSharper disable once UnusedMethodReturnValue.Global
        public static Boolean SetElementHasAttachedLink(Repository rep, EA.Element el, EA.Element elNote)
        {
            string updateStr = @"update t_object set pdata1 = 'Element Note', pdata2 = '" + el.ElementID +
                               "', pdata4='Yes' " +
                               " where object_ID = " + elNote.ElementID;
            rep.Execute(updateStr);


            return true;
        }

        // ReSharper disable once UnusedMethodReturnValue.Global
        public static Boolean SetBehaviorForOperation(Repository rep, Method op, EA.Element act)
        {

            string updateStr = @"update t_operation set behaviour = '" + act.ElementGUID + "' " +
                               " where operationID = " + op.MethodID;
            rep.Execute(updateStr);


            return true;
        }

        public static string GetDiagramObjectLabel(Repository rep, int objectId, int diagramId, int instanceId)
        {
            string attributeName = "OBJECT_STYLE";
            string query = @"select ObjectStyle AS " + attributeName +
                           @" from t_diagramobjects
                      where Object_ID = " + objectId + @" AND 
                            Diagram_ID = " + diagramId + @" AND 
                            Instance_ID = " + instanceId;

            return GetSingleSqlValue(rep, query, attributeName);
        }

        private static string GetSingleSqlValue(Repository rep, string query, string attributeName)
        {
            string s = "";
            string str = rep.SQLQuery(query);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(str);

            XmlNode node = xmlDoc.SelectSingleNode("//" + attributeName);
            if (node != null)
            {
                s = node.InnerText;
            }
            return s;
        }

        // ReSharper disable once UnusedMethodReturnValue.Global
        public static Boolean SetDiagramObjectLabel(Repository rep, int objectId, int diagramId, int instanceId,
            string s)
        {

            string updateStr = @"update t_diagramObjects set ObjectStyle = '" + s + "' " +
                               " where Object_ID = " + objectId + " AND " +
                               " Diagram_ID = " + diagramId + " AND " +
                               " Instance_ID = " + instanceId;

            rep.Execute(updateStr);


            return true;
        }

        // Find the operation from Activity / State Machine
        // it excludes operations in state machines
        public static Method GetOperationFromBrehavior(Repository rep, EA.Element el)
        {
            Method method = null;
            string query = "";
            string conString = GetConnectionString(rep); // due to shortcuts
            if (conString.Contains("DBType=3"))
            {
                // Oracle DB
                query =
                    @"select op.ea_guid AS EA_GUID
                      from t_operation op 
                      where Cast(op.Behaviour As Varchar2(38)) = '" + el.ElementGUID + "' " +
                    " AND (Type is Null or Type not in ('do','entry','exit'))";
            }
            if (conString.Contains("DBType=1"))
                // SQL Server
            {
                query =
                    @"select op.ea_guid AS EA_GUID
                        from t_operation op 
                        where Substring(op.Behaviour,1,38) = '" + el.ElementGUID + "'" +
                    " AND (Type is Null or Type not in ('do','entry','exit'))";

            }

            if (conString.Contains(".eap"))
                // SQL Server
            {
                query =
                    @"select op.ea_guid AS EA_GUID
                        from t_operation op 
                        where op.Behaviour = '" + el.ElementGUID + "'" +
                    " AND ( Type is Null or Type not in ('do','entry','exit'))";

            }
            if ((!conString.Contains("DBType=1")) && // SQL Server, DBType=0 MySQL
                (!conString.Contains("DBType=3")) && // Oracle
                (!conString.Contains(".eap"))) // Access
            {
                query =
                    @"select op.ea_guid AS EA_GUID
                        from t_operation op 
                        where op.Behaviour = '" + el.ElementGUID + "'" +
                    " AND (Type is Null or Type not in ('do','entry','exit'))";

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



        public static Method GetOperationFromConnector(Repository rep, Connector con)
        {
            Method method = null;
            string query = "";
            if (GetConnectionString(rep).Contains("DBType=3"))
                //pdat3: 'Activity','Sequence', (..)
            {
                // Oracle DB
                query =
                    @"select description AS EA_GUID
                      from t_xref x 
                      where Cast(x.client As Varchar2(38)) = '" + con.ConnectorGUID + "'" +
                    " AND Behavior = 'effect' ";
            }
            if (GetConnectionString(rep).Contains("DBType=1"))
            {
                // SQL Server

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
            if ((!GetConnectionString(rep).Contains("DBType=1")) && // SQL Server, DBType=0 MySQL
                (!GetConnectionString(rep).Contains("DBType=3")) && // Oracle
                (!GetConnectionString(rep).Contains(".eap"))) // Access
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

                if (guid != null) OpenBehaviorForElement(rep, rep.GetElementByGuid(guid));
            }
            //}

            return method;
        }

        /// <summary>
        /// Update VC (Version Control state of a controlled package:
        /// - Returns user name of user who have checked out the package
        /// - Updates the package flags
        /// </summary>
        /// <param name="rep">Repository</param>
        /// <param name="pkg">Package to check</param>
        // ReSharper disable once UnusedMethodReturnValue.Global
        public static string UpdateVc(Repository rep, Package pkg)
        {
            string userNameLockedPackage = "";
            if (pkg.IsVersionControlled)
            {
                // find                  VC=...;
                // replace by:           VC=currentState();
                string flags = pkg.Flags;

                // remove check out flags
                flags = Regex.Replace(flags, @"VC=[^;]*;", "");
                flags = Regex.Replace(flags, @"CheckedOutTo=[^;]*;", "");


                svn svnHandle = new svn(rep, pkg);
                userNameLockedPackage = svnHandle.getLockingUser();
                if (userNameLockedPackage != "") flags = flags + "CheckedOutTo=" + userNameLockedPackage + ";";
                try
                {
                    SetVcFlags(rep, pkg, flags);
                    rep.ShowInProjectView(pkg);
                }
                catch (Exception e)
                {
                    string s = e.Message + " ;" + pkg.GetLastError();
                    s = s + "!";
                    MessageBox.Show(s, "Error VC");
                }


            }
            return userNameLockedPackage;
        }

        //------------------------------------------------------------------------------------------
        // resetVCRecursive   If package is controlled: Reset packageflags field of package, work for all packages recursive 
        //------------------------------------------------------------------------------------------
        // package flags:  Recurse=0;VCCFG=unchanged;
        public static void ResetVcRecursive(Repository rep, Package pkg)
        {
            ResetVc(rep, pkg);
            foreach (Package p in pkg.Packages)
            {
                ResetVc(rep, p);
            }
        }

        //------------------------------------------------------------------------------------------
        // resetVC   If package is controlled: Reset package flags field of package 
        //------------------------------------------------------------------------------------------
        // package flags:  Recurse=0;VCCFG=unchanged;
        // ReSharper disable once MemberCanBePrivate.Global
        public static void ResetVc(Repository rep, Package pkg)
        {
            if (pkg.IsVersionControlled)
            {
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
                    MessageBox.Show(e.ToString(), @"Error Version Control");
                }


            }
            // recursive package
            //foreach (EA.Package pkg1 in pkg.Packages)
            //{
            //    updateVC(rep, pkg1);
            //}
        }

        public static string GetVCstate(Repository rep, Package pkg, Boolean isLong)
        {
            // ReSharper disable once UnusedVariable
            string[] checkedOutStatusLong =
            {
                "Uncontrolled",
                "Checked in",
                "Checked out to this user",
                "Read only version",
                "Checked out to another user",
                "Offline checked in",
                "Offline checked out by user",
                "Offline checked out by other user",
                "Deleted"
            };
            // ReSharper disable once UnusedVariable
            string[] checkedOutStatusShort =
            {
                "Uncontrolled",
                "Checked in",
                "Checked out",
                "Read only",
                "Checked out",
                "Offline checked in",
                "Offline checked out",
                "Offline checked out",
                "Deleted"
            };

            try
            {
                svn svnHandle = new svn(rep, pkg);
                var s = svnHandle.getLockingUser();
                if (s != "") s = "CheckedOutTo=" + s;
                else s = "Checked in";
                return s;
                //state = pkg.VersionControlGetStatus();
            }
            catch (Exception e)
            {
                if (isLong) return "VC State Error: " + e.Message;
                return "State Error";
            }

        }

        /// <summary>
        /// Get file path for a string. It regards the local file path definition in %APPDATA%. To do so it needs the GenType (C,C++,..)
        /// </summary>
        /// <param name="genType"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        // ReSharper disable once UnusedParameter.Global
        public static string GetFilePath(string genType, string path)
        {
            // check if a local path is defined
            Match m = Regex.Match(path, @"%[^%]*");
            if (m.Success)
            {
                var localPathVar = m.Value.Substring(1);
                // get path for localDir
                // ReSharper disable once AssignNullToNotNullAttribute
                Environment.CurrentDirectory = Environment.GetEnvironmentVariable("appdata");
                string s1 = @"Sparx Systems\EA\paths.txt";
                TextReader tr = new StreamReader(s1);
                string line;
                Regex pattern = new Regex(@"(type=" + genType + ";id=" + localPathVar + @").+(path=[^;]+)");
                while ((line = tr.ReadLine()) != null)
                {
                    var regMatch = pattern.Match(line);
                    if (regMatch.Success)
                    {
                        path = path.Replace("%" + localPathVar + "%", "");
                        path = regMatch.Groups[2] + @"\" + path;
                        path = path.Substring(5);
                        path = path.Replace(@"\\", @"\");
                        break;
                    }
                }
                tr.Close();
            }
            return path;


        }

        /// <summary>
        /// Get file path for element. It uses the local path definitions.
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="el"></param>
        /// <returns></returns>
        public static string GetGenFilePathElement( EA.Element el)
        {
            return GetFilePath(el.Gentype, el.Genfile);

        }

        public static string GetVccRootPath(Repository rep, Package pkg)
        {
            string rootPath = "";
            Regex pattern = new Regex(@"VCCFG=[^;]+");
            Match regMatch = pattern.Match(pkg.Flags);
            if (regMatch.Success)
            {
                // get VCCFG
                var uniqueId = regMatch.Value.Substring(6);
                // get path for UiqueId
                // ReSharper disable once AssignNullToNotNullAttribute
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
                        rootPath = regMatch.Groups[2].Value;
                        rootPath = rootPath.Substring(5);
                        break;
                    }
                }
                tr.Close();
                if (rootPath == "")
                {
                    rep.WriteOutput("Debug", "VCCFG=... not found in" + s1 + " " + pkg.Name, 0);
                }
                return rootPath;
            }
            rep.WriteOutput("Debug", "VCCFG=... not found:" + pkg.Name, 0);
            return "";
        }

        public static string GetVccFilePath(Repository rep, Package pkg)
        {
            string rootPath = GetVccRootPath(rep, pkg);
            var path = rootPath + @"\" + pkg.XMLPath;
            return path;
        }

        // ReSharper disable once UnusedMethodReturnValue.Global
        public static Boolean GetLatest(Repository rep, Package pkg, Boolean recursive, ref int count, int level,
            ref int errorCount)
        {
            if (pkg.IsControlled)
            {
                level = level + 1;
                // check if checked out

                string path = GetVccFilePath(rep, pkg);
                string fText;
                //rep.WriteOutput("Debug", "Path:" + pkg.Name + path, 0);
                string sLevel = new string(' ', level * 2);
                rep.WriteOutput("Debug", sLevel + (count + 1).ToString(",0") + " Work for:" + path, 0);
                if (path != "")
                {
                    count = count + 1;
                    rep.ShowInProjectView(pkg);
                    // delete a potential write protection
                    try
                    {
                        FileInfo fileInfo = new FileInfo(path);
                        FileAttributes attributes = (FileAttributes) (fileInfo.Attributes - FileAttributes.ReadOnly);
                        File.SetAttributes(fileInfo.FullName, attributes);
                        File.Delete(path);
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
                foreach (Package pkgNested in pkg.Packages)
                {
                    //rep.WriteOutput("Debug", "Recursive:"+ pkgNested.Name, 0);
                    GetLatest(rep, pkgNested, true, ref count, level, ref errorCount);

                }
            }
            return true;

        }

        // ReSharper disable once MemberCanBePrivate.Global
        public static string GetConnectionString(Repository rep)
        {
            string s = rep.ConnectionString;
            if (s.Contains("DBType="))
            {
                return s;
            }
            FileInfo f = new FileInfo(s);
            if (f.Length > 1025)
            {
                return s;
            }
            return HoUtil.ReadAllText(s);
        }

        public static void OpenBehaviorForElement(Repository repository, EA.Element el)
        {
            // find the diagram
            if (el.Diagrams.Count > 0)
            {
                // get the diagram
                Diagram dia = (Diagram) el.Diagrams.GetAt(0);
                // open diagram
                repository.OpenDiagram(dia.DiagramID);
            }
            // no diagram found, select element
            repository.ShowInProjectView(el);
        }

        // ReSharper disable once UnusedMethodReturnValue.Global
        public static Boolean SetXmlPath(Repository rep, string guid, string path)
        {

            string updateStr = @"update t_package set XMLPath = '" + path +
                               "' where ea_guid = '" + guid + "' ";

            rep.Execute(updateStr);


            return true;
        }

        public static void SetReadOnlyAttribute(string fullName, bool readOnly)
        {
            FileInfo filePath = new FileInfo(fullName);
            FileAttributes attribute;
            if (readOnly)
                attribute = filePath.Attributes | FileAttributes.ReadOnly;
            else
                attribute = (FileAttributes) (filePath.Attributes - FileAttributes.ReadOnly);

            File.SetAttributes(filePath.FullName, attribute);
        }

        #region visualizePortForDiagramobject

        /// <summary>
        /// Visualize port with or without interface (required/provided) for diagram object
        /// return: true = port was newly shown
        ///         false= part was already shown
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="pos"></param>
        /// <param name="dia"></param>
        /// <param name="diaObjSource"></param>
        /// <param name="port"></param>
        /// <param name="portInterface"></param>
        public static bool VisualizePortForDiagramobject(EA.Repository rep, int pos, EA.Diagram dia,
            EA.DiagramObject diaObjSource, EA.Element port, EA.Element portInterface)
        {
            // check if port already exists
            foreach (EA.DiagramObject diaObjPort in dia.DiagramObjects)
            {
                if (diaObjPort.ElementID == port.ElementID) return false;
            }

            // visualize ports
            // visualize ports
            int portPosDistance = 35;
            int portPosStart = 35;
            int portLength = 15;  // fix

            // calculate target position of port
            int leftPort = diaObjSource.right - portLength / 2;
            int rightPort = leftPort + portLength;

            int top = diaObjSource.top;
            int topPort = top - portPosStart - pos * portPosDistance;
            int bottomPort = topPort - portLength;

            // diagram object can't host port (not tall enough)
            // make diagram object taller to host all ports
            if (bottomPort <= diaObjSource.bottom)
            {
                diaObjSource.bottom = diaObjSource.bottom - 30;
                diaObjSource.Update();
            }

            string positionPort = $"l={leftPort};r={rightPort};t={topPort};b={bottomPort};";
            EA.DiagramObject diaObjectPort =
                (EA.DiagramObject) dia.DiagramObjects.AddNew(positionPort, "");
            if (port.Type.Equals("Port"))
            {
                // not showing label
                //diaObject.Style = "LBL=CX=97:CY=13:OX=45:OY=0:HDN=1:BLD=0:ITA=0:UND=0:CLR=-1:ALN=0:ALT=0:ROT=0;";
                string ox = "OX=23:";
                if (port.EmbeddedElements.Count > 0) ox = "OX=80:"; // more to the right side
                diaObjectPort.Style = $@"LBL=CX=200:CY=12:{ox}OY=1:HDN=0:BLD=0:ITA=0:UND=0:CLR=-1:ALN=0:ALT=0:ROT=0;";
            }
            else
            {

                // not showing label
                diaObjectPort.Style = "LBL=CX=97:CY=13:OX=39:OY=3:HDN=0:BLD=0:ITA=0:UND=0:CLR=-1:ALN=0:ALT=0:ROT=0;";
            }
            diaObjectPort.ElementID = port.ElementID;
            diaObjectPort.Update();
            dia.DiagramObjects.Refresh();
            rep.ReloadDiagram(dia.DiagramID);

            //----------------------------------------------------------------------------
            // Show of port: Embedded Interface/Port
            if (portInterface == null) return true;

            // visualize interface
            string positionInterface = $"l={rightPort - 2};r={rightPort + 40};t={topPort - 1};b={bottomPort - 1};";
            EA.DiagramObject diaObjectPortInterface =
                (EA.DiagramObject) dia.DiagramObjects.AddNew(positionInterface, "");
            //(EA.DiagramObject)dia.DiagramObjects.AddNew(position, EA.ObjectType.otElement.ToString());

            // diaObject2.Style = "LBL=CX=69:CY=13:OX=45:OY=0:HDN=0:BLD=0:ITA=0:UND=0:CLR=-1:ALN=0:ALT=0:ROT=0;";
            // HDN=0 Label visible
            // HDN=1 Label invisible
            // PType=1: Type Shown
            // CX = nn; Name Position
            // OX = nn; Label Position, -nn = Left, +nn = Right
            diaObjectPortInterface.Style = "LBL=CX=69:CY=13:OX=45:OY=0:HDN=1:BLD=0:ITA=0:UND=0:CLR=-1:ALN=0:ALT=0:ROT=0;";
            diaObjectPortInterface.ElementID = portInterface.ElementID;
            try
            {
                diaObjectPortInterface.Update();
            }
            catch
            {
//                MessageBox.Show($@"ElementID: '{diaObject2.ElementID}'
//ObjectType:'{diaObject2.ObjectType}'
//Type:'{diaObject2.GetType()}'
//Style: '{diaObject2.Style}'

//{e.Message}",
//                    "Error create embedded port with Interface on Diagram.");
            }
            dia.DiagramObjects.Refresh(); // first update element than refresh collection 
            return true;

        }

        #endregion

        // ReSharper disable once UnusedMethodReturnValue.Global
        public static DiagramLink GetDiagramLinkFromConnector(Diagram dia, int connectorId)
        {
            foreach (DiagramLink link in dia.DiagramLinks)
            {
                if (link.ConnectorID == connectorId)
                {
                    return link;
                }
            }
            return null;
        }



        // Find the operation from Activity / State Machine
        // it excludes operations in state machines
        public static Package GetModelDocumentFromPackage(Repository rep, Package pkg)
        {
            Package pkg1 = null;
            string repositoryType = rep.RepositoryType();

            // get object_ID of package
            var query = @"select pkg.ea_GUID AS EA_GUID " +
                        @" from (((t_object o  INNER JOIN t_attribute a on (o.object_ID = a.Object_ID AND a.type = 'Package')) " +
                        @"     INNER JOIN t_package pkg on (pkg.Package_ID = o.Package_ID)) " +
                        @"		  INNER JOIN t_object o1 on (cstr(o1.object_id) = a.classifier)) " +
                        @" where o1.ea_guid = '" + pkg.PackageGUID + "' ";


            if (repositoryType == "JET")
            {
                query = @"select pkg.ea_GUID AS EA_GUID " +
                        @" from (((t_object o  INNER JOIN t_attribute a on (o.object_ID = a.Object_ID AND a.type = 'Package')) " +
                        @"     INNER JOIN t_package pkg on (pkg.Package_ID = o.Package_ID)) " +
                        @"		  INNER JOIN t_object o1 on (cstr(o1.object_id) = a.classifier)) " +
                        @" where o1.ea_guid = '" + pkg.PackageGUID + "' ";
            }
            if (repositoryType == "SQLSVR")
                // SQL Server
            {
                query = @"select pkg.ea_GUID AS EA_GUID " +
                        @" from (((t_object o  INNER JOIN t_attribute a on (o.object_ID = a.Object_ID AND a.type = 'Package')) " +
                        @"     INNER JOIN t_package pkg on (pkg.Package_ID = o.Package_ID)) " +
                        @"		  INNER JOIN t_object o1 on o1.object_id = Cast(a.classifier As Int)) " +
                        @" where o1.ea_guid = '" + pkg.PackageGUID + "' ";

            }



            string str = rep.SQLQuery(query);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(str);

            XmlNode operationGuidNode = xmlDoc.SelectSingleNode("//EA_GUID");

            if (operationGuidNode != null)
            {
                string guid = operationGuidNode.InnerText;
                pkg1 = rep.GetPackageByGuid(guid);
            }
            return pkg1;
        }

        public static Package GetFirstControlledPackage(Repository rep, Package pkg)
        {
            if (pkg.IsControlled) return pkg;
            var pkgId = pkg.ParentID;
            if (pkgId == 0) return null;
            pkg = GetFirstControlledPackage(rep, rep.GetPackageByID(pkgId));
            return pkg;

        }

        /// <summary>
        /// Attach a Model element to another Model element
        /// pdata1= 'Element Note'  (what to attach to)
        ///         'Link Notes'    (link to connector)
        /// pdata2= ElementID to attach to if element
        /// pdata4= 'Yes' if link to element
        ///         idref1=ID connector if link to connector
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="el"></param>
        /// <param name="elNote"></param>
        /// <returns></returns>
        // ReSharper disable once UnusedMethodReturnValue.Global
        public static bool SetElementHasAttachedElementLink(Repository rep, EA.Element el, EA.Element elNote)
        {
            return SetElementLink(rep, elNote.ElementID, "Element Note", el.ElementID, "", "Yes", 0);
        }

        /// Set Element Link to:
        /// - Object
        /// - Diagram
        /// - Connector
        /// Attach a Model element to another Model item (Element, Connector, Diagram)
        /// pdata1= Attach Note to feature of (aka connectorLinkType)
        ///         'Diagram Note' Notes of the Diagram 
        ///         'Element Note' Notes of the Element
        ///         'Link Notes'   Notes of a Connector
        ///         'Attribute'    Notes of the Attribute
        ///         'Operation'    Notes of the Operation
        /// pdata2= ElementID to attach to if object
        /// pdata3= Feature name if feature, else blank
        /// pdata4= 'Yes' if link to object
        ///         'idref1=connectorID;' connector if link to connector
        /// ntype   0 Standard
        ///         1 connect to connector according to 'idref1=connectorID;'
        public static bool SetElementLink(Repository rep, int elId, string pdata1, int pdata2, string pdata3,
            string pdata4, int ntype)
        {
            string delimiter = "";
            string pdata1Value = "";
            if (! String.IsNullOrWhiteSpace(pdata1))
            {
                pdata1Value = $"pdata1 = '{pdata1}'";
                delimiter = ", ";
            }
            // ID of the object (Element, Connector, Attribute, Operation,..)
            string pdata2Value = "";
            if (pdata2 > 0)
            {
                pdata2Value = $@"{delimiter} pdata2 = {pdata2}";
                delimiter = ", ";
            }

            // Feature name
            string pdata3Value = "";
            if (pdata3 != "")
            {
                pdata3Value = $@"{delimiter} pdata3 = '{pdata3}'";
                delimiter = ", ";
            }
            string pdata4Value = "";
            if (pdata4 != "")
            {
                pdata4Value = $@"{delimiter} pdata4 = '{pdata4}'";
                delimiter = ", ";
            }

            string updateStr = $@"update t_object set {pdata1Value} {pdata2Value} {pdata3Value} {pdata4Value} {delimiter} NTYPE={ntype}" +
                               $@" where object_ID = {elId} ";
            rep.Execute(updateStr);
            return true;
        }

        /// <summary>
        /// Set in Element the connector it is connected to. You can also specify if the description is bound to the connector description.
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="con"></param>
        /// <param name="elNote"></param>
        /// <param name="bound"></param>
        /// <returns></returns>
        public static bool SetElementHasAttachedConnectorLink(Repository rep, Connector con, EA.Element elNote,
             bool bound=true)
        {
            var connectorLinkType = bound == false ? "" : "Link Notes";
            return SetElementLink(rep, elNote.ElementID, connectorLinkType, 0, "", $@"idref1={con.ConnectorID}", 1);
        }

        /// <summary>
        /// Set Diagram Style to fit into a page.
        /// </summary>
        /// <param name="dia"></param>
        public static void SetDiagramStyleFitToPage(Diagram dia)
        {
            // set Diagram Style = scale to fit one page
            string t = dia.ExtendedStyle;
            t = Regex.Replace(t, "PPgs.cx=[0-9]", "PPgs.cx=1");
            t = Regex.Replace(t, "PPgs.cy=[0-9]", "PPgs.cy=1");
            t = Regex.Replace(t, "ScalePI=[0-9]", "ScalePI=1");
            dia.ExtendedStyle = t;
            dia.Update();
        }

        /// <summary>
        /// OutputAboutMessage.
        /// </summary>
        /// <param name="description"></param>
        /// <param name="caption"></param>
        /// <param name="lDllNames"></param>
        public static void AboutMessage(string description, string caption, string[] lDllNames, EA.Repository rep, string pathSettings)
        {
            string pathRoot = Assembly.GetExecutingAssembly().Location;
            pathRoot = Path.GetDirectoryName(pathRoot);

            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            


            description = $@"{description}

Helmut.Ortmann@hoModeler.de
(+49) 172 / 51 79 16 7

{"EA Library Version:",-32}:{rep.LibraryVersion}
{"ConnectionString:", -32}:{rep.ConnectionString}
{"InstallationPath:",-32}:{pathRoot}

{"SettingsPath:",-32}:{pathSettings}

";
            foreach (string dllName in lDllNames)
            {
                try
                {
                    string pathDll = Path.Combine(new[] { pathRoot, dllName });
                    description = dllName.Length > 20 
                        ? $"{description}- {dllName,-26}\t: V{FileVersionInfo.GetVersionInfo(pathDll).FileVersion}{Environment.NewLine}" 
                        : $"{description}- {dllName,-26}\t\t: V{FileVersionInfo.GetVersionInfo(pathDll).FileVersion}{Environment.NewLine}";
                }
                //
                catch (Exception)
                {
                    description = dllName.Length > 20 
                        ? $"{description}- {dllName,-26}\t: dll not found{Environment.NewLine}" 
                        : $"{description}- {dllName,-26}\t\t: dll not found{Environment.NewLine}";
                }
            }
            MessageBox.Show(description, caption);
        }

        /// <summary>
        /// Read file according to filePath.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string ReadAllText(string filePath)
        {
            try
            {
                return File.ReadAllText(filePath);
            }
            catch (Exception e)
            {
                MessageBox.Show($@"Can't read '{filePath}'

{e}", @"Can't read file");
                return "";
            }
        }
        /// <summary>
        /// Open Directory of a directory or a file with Explorer or Totalcommander.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="isTotalCommander"></param>
        public static void ShowFolder(string path, bool isTotalCommander=false)
        {
            path = Path.GetDirectoryName(path);

            if (isTotalCommander)
                StartApp(@"totalcmd.exe", "/o " + path);
            else
                StartApp(@"Explorer.exe", "/e, " + path);
        }
        /// <summary>
        /// Delete file with error handling
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static bool TryToDeleteFile(string fileName)
        {
            try
            {
                // A.
                // Try to delete the file.
                if (File.Exists(fileName)) File.Delete(fileName);
                return true;
            }
            catch (IOException)
            {
                // B.
                // We could not delete the file.
                return false;
            }
        }
    }
}
