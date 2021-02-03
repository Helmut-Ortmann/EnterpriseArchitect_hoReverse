using System;
using System.Windows.Forms;
using System.Drawing;
using System.Text.RegularExpressions;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using DocumentFormat.OpenXml.Packaging;
using EaServices.MOVE;
using EA;
using hoReverse.hoUtils.ODBC;
using hoReverse.hoUtils;
using hoReverse.hoUtils.Cutils;
using hoReverse.hoUtils.ActivityParameter;
using hoReverse.hoUtils.Appls;
using hoReverse.hoUtils.Favorites;
using hoReverse.hoUtils.svnUtil;
using hoReverse.Services.Dlg;
using hoReverse.Extension;
using hoReverse.EAServicesPort;
using hoReverse.hoUtils.Diagrams;
using Attribute = System.Attribute;
using Connector = hoReverse.Connectors.Connector;
using File = System.IO.File;
using TaggedValue = hoReverse.hoUtils.TaggedValue;
using hoReverse.hoUtil.EaCollection;
using hoReverse.hoUtilsVC;
using CustomProperty = EA.CustomProperty;
using DiagramObject = EA.DiagramObject;
using EaServices.AddInSearch;

// ReSharper disable once CheckNamespace
namespace hoReverse.Services
{
    #region Definition of Service Attribute
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false) ]

    // Attribute to define services which might be called without parameters.
    public class ServiceOperationAttribute : Attribute 
    {
        public ServiceOperationAttribute(String guid, String description, String help, bool isTextRequired = false)
        {
            Description = description;
            Guid = guid;
            Help = help;
            IsTextRequired = isTextRequired;
        }

        public bool IsTextRequired { get; }
        public String Description { get; }
        public String Help { get; }

        public String Guid { get; }
    }
    #endregion

    // ReSharper disable once InconsistentNaming
    [SuppressMessage("ReSharper", "RedundantArgumentDefaultValue")]
    public static partial class HoService
    {
        public static string Release = "1.0.01";
        private const string EmbeddedElementTypes = "Port Parameter Pin";


        // define menu constants
        public enum DisplayMode
        {
            Behavior,
            Method
        }
        public static DiagramFormat DiagramStyle;



        #region runQuickSearch

        //---------------------------------------------------------------------------------------------------------------
        // Search for Elements, Operation, Attributes, GUID
        public static void RunQuickSearch(Repository rep, string searchName, string searchString)
        {
            // get the search vom setting
            try
            {
                rep.RunModelSearch(searchName, searchString, "", "");
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.ToString(), $@"Search name:'{searchName}'");
            }
        }

        #endregion

        /// <summary>
        /// Sort diagram elements alphabetic
        /// </summary>
        /// <param name="rep"></param>
        [ServiceOperation("{52298A07-AA0E-459C-9DBA-22CCBE0EA838}",
            "Sort diagram elements in alphabetic order", // Description
            "Select all Elements/Packages/Embedded Elements to sort", //Tooltip
            isTextRequired: false)]
        // ReSharper disable once UnusedMember.Global
        // dynamical usage as configurable service by reflection
        public static void SortAlphabetic(Repository rep)
        {
            EaDiagram curDiagram = new EaDiagram(rep);
            if (curDiagram.Dia == null) return;

            rep.SaveDiagram(curDiagram.Dia.DiagramID);
            // two objects selected
            if (curDiagram.SelectedObjectsCount < 2 || curDiagram.SelElements.Count < 2)
            {
                MessageBox.Show(@"Ports, Parameter, Pins supported", @"Select at least two elements on the diagram!");
                return;
            }
            EaCollectionDiagramObjects diaCol = new EaCollectionDiagramObjects(curDiagram);
            diaCol.SortAlphabetic();
            rep.ReloadDiagram(curDiagram.Dia.DiagramID);
            curDiagram.ReloadSelectedObjectsAndConnector();

        }
        /// <summary>
        /// Move Clipboard items to selected package. Use a Search and copy the search results to clipboard.
        /// </summary>
        /// <param name="rep"></param>
        [ServiceOperation("{BB133FEA-AEDD-4C8F-BBC1-A59F85F4624C}",
            "Move EA clipboard items form Search to selected Package", // Description
            "Select the package to move the EA clipboard items into. The clipboard items must contain a GUID or an Object-ID (left columns).", //Tooltip
            isTextRequired: false)]
        public static void MoveClipboardItemsToPackage(Repository rep)
        {
            if (rep.GetContextItemType() != EA.ObjectType.otPackage) return;
            EA.Package pkg = (EA.Package)rep.GetContextObject();

            string[] clipboardText = Clipboard.GetText().Split(new[] { Environment.NewLine },
                StringSplitOptions.RemoveEmptyEntries);
            if (clipboardText.Length < 2)
            {
                MessageBox.Show("", @"Clipboard doesn't contain a row");
                return;
            }

            // skip heading
            clipboardText = clipboardText.Skip(1).Take(clipboardText.Length - 1).ToArray();
            string[] del = new[] { System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator };

            var firstLine = clipboardText.FirstOrDefault();
            if (String.IsNullOrEmpty(firstLine))
            {
                MessageBox.Show("", @"Clipboard empty");
                return;
            }
            var firstLineAsArray = firstLine.Split(del, StringSplitOptions.None);
            bool found = false;
            bool isObjectId = false;
            int column = -1;
            foreach (var col in firstLineAsArray)
            {
                column += 1;
                // Check GUID 
                if (col.StartsWith("{") && col.EndsWith("}"))
                {
                    isObjectId = false;
                    found = true;
                    break;
                }
                if (Int32.TryParse(col, out int j))
                {
                    isObjectId = true;
                    found = true;
                    break;

                }
            }

            if (found == false)
            {
                MessageBox.Show($@"Delimiter: '{del[0]}'

Clipboard first data line:
{firstLine}", @"Can't read an object_id or a GUID from Clipboard");
                return;
            }

            int countCopied = 0;
            foreach (var row in clipboardText)
            {
                var rowAsArray = row.Split(del, StringSplitOptions.None);
                if (isObjectId)
                {
                    if (int.TryParse(rowAsArray[column], out int id))
                    {
                        EA.Element el = rep.GetElementByID(id);
                        if (el == null) continue;
                        el.PackageID = pkg.PackageID;
                        el.Update();
                        countCopied += 1;
                    }
                }
                else
                {
                    if (rowAsArray[column].StartsWith("{") && rowAsArray[column].EndsWith("}"))
                    {
                        EA.Element el = rep.GetElementByGuid(rowAsArray[column]);
                        if (el == null) continue;
                        el.PackageID = pkg.PackageID;
                        el.Update();
                        countCopied += 1;
                    }

                }
            }
            // update package viewS
            rep.RefreshModelView(pkg.PackageID);
            MessageBox.Show($@"Package: {pkg.Name}", $@"{countCopied} elements from Clipboard copied to selected/contect package");

        }
        /// <summary>
        /// Copy FQ (Full Qualified) Name to ClipBoard
        /// </summary>
        /// <param name="rep"></param>
        [ServiceOperation("{B48B4D01-2BC5-4C2E-A9E2-4DB6336D4CCE}", "Copy FQ to Clipboard",
            "Copy FQ (Full Qualified) Name to Clipboard (Package, Element, Diagram, Attribute, Operation)", isTextRequired: false)]
        // ReSharper disable once UnusedMember.Global
        // dynamical usage as configurable service by reflection
        public static void CopyFqToClipboard(Repository rep)
        {
            string strFq = "";
            ObjectType type = rep.GetContextItem(out var o);
            try
            {
                switch (type)
                {
                    case ObjectType.otElement:
                        strFq = ((Element) o).FQName;
                        break;

                    case ObjectType.otModel:
                        strFq = ((Package) o).Name;
                        break;

                    case ObjectType.otPackage:
                        Element el = rep.GetElementByGuid(((Package) o).PackageGUID);
                        if (el == null) strFq = ((Package) o).Name;
                        else strFq = el.FQName;
                        break;

                    case ObjectType.otDiagram:
                        Diagram dia = (Diagram) o;
                        if (dia.ParentID != 0)
                        {
                            strFq = $"{rep.GetElementByID(dia.ParentID).FQName}.{dia.Name}";
                        }
                        else
                        {
                            string guid = rep.GetPackageByID(dia.PackageID).PackageGUID;
                            strFq = $"{rep.GetElementByGuid(guid).FQName}.{dia.Name}";
                        }
                        break;
                    case ObjectType.otAttribute:
                        EA.Attribute a = (EA.Attribute) o;
                        strFq = $"{rep.GetElementByID(a.ParentID).FQName}.{a.Name}";
                        break;
                    case ObjectType.otMethod:
                        Method m = (Method) o;
                        strFq = $"{rep.GetElementByID(m.ParentID).FQName}.{m.Name}";
                        break;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show($@"{e}", @"FQ name not implemented in your EA version!");
                strFq = "";
            }

            if (String.IsNullOrWhiteSpace(strFq)) Clipboard.Clear();
            else Clipboard.SetText(strFq);
        }
        /// <summary>
        /// Move usage of source element to target element
        /// </summary>
        /// <param name="rep"></param>
        [ServiceOperation("{BB133FEA-AEDD-4C8F-BBC1-A59F85F4624C}",
            "Move usage to element", // Description
            "Click source Element, then target Element to move usage to target", //Tooltip
            isTextRequired: false)]
        // ReSharper disable once UnusedMember.Global
        // dynamical usage as configurable service by reflection
        public static void MoveUsage(Repository rep)
        {
            EaDiagram curDiagram = new EaDiagram(rep);
            if (curDiagram.Dia == null) return;

            rep.SaveDiagram(curDiagram.Dia.DiagramID);
            // two objects selected
            if (curDiagram.SelectedObjectsCount != 2 || curDiagram.SelElements.Count != 2)
            {
                MessageBox.Show(@"First Element: Source of move connections and appearances
Second Element: Target of move connections and appearances", @"Select two elements on the diagram!");
                return;
            }
            Element source = curDiagram.SelElements[1];
            Element target = curDiagram.SelElements[0];

            Odbc odbc = new Odbc(rep);

            Move.MoveClassifier(rep, curDiagram.Dia, source, target);
            odbc.Close();
            // update current diagram
            rep.ReloadDiagram(curDiagram.Dia.DiagramID);

        }
        /// <summary>
        /// Copy Stereotypes to ClipBoard
        /// </summary>
        /// <param name="rep"></param>
        [ServiceOperation("{484BEAF5-C72C-4ABA-8202-08230B4D2AAA}", "Copy Stereotypes to ClipBoard",
            "Copy Stereotypes (FDStereotype+StereotypeEx) to ClipBoard", isTextRequired: false)]
        // ReSharper disable once UnusedMember.Global
        // dynamical usage as configurable service by reflection
        public static void CopyStereotypesToClipboard(Repository rep)
        {
            Element el;
            string strStereo = "";
            ObjectType type = rep.GetContextItem(out object o);
            switch (type)
            {
                case ObjectType.otElement:
                    el = (Element)o;
                    strStereo = el.FQStereotype + "\r\n" + el.StereotypeEx;
                    break;

                case ObjectType.otModel:
                    strStereo = ((Package)o).StereotypeEx;
                    break;

                case ObjectType.otPackage:
                    el = rep.GetElementByGuid(((Package)o).PackageGUID);
                    strStereo = el == null ? ((Package)o).StereotypeEx : el.FQStereotype + "\r\n" + el.StereotypeEx;
                    break;
                case ObjectType.otDiagram:
                    strStereo = ((Diagram)o).StereotypeEx;
                    break;
                case ObjectType.otAttribute:
                    strStereo = ((EA.Attribute)o).FQStereotype + "\r\n" + ((EA.Attribute)o).StereotypeEx;
                    break;
                case ObjectType.otMethod:
                    strStereo = ((Method)o).FQStereotype + "\r\n" + ((Method)o).StereotypeEx;
                    break;

                case ObjectType.otConnector:
                    EA.Connector con = (EA.Connector)o;
                    strStereo = con.FQStereotype + "\r\n" + con.StereotypeEx; 
                    break;
            }

            if (String.IsNullOrWhiteSpace(strStereo)) Clipboard.Clear();
            else Clipboard.SetText(strStereo);
        }

        /// <summary>
        /// Copy GUID to ClipBoard
        /// </summary>
        /// <param name="rep"></param>
        [ServiceOperation("{02ECEF56-5EF8-45D7-9CB8-4A67B5D9AC2F}", "Copy GUID to Clipboard",
            "Copy GUID to Clipboard (Package, Element, Diagram, Attribute, Operation, Connector, Parameter)", isTextRequired: false)]
        // ReSharper disable once UnusedMember.Global
        // dynamical usage as configurable service by reflection
        public static void CopyGuidToClipboard(Repository rep)
        {
            string strGuid = "";
            ObjectType type = rep.GetContextItem(out var o);
            switch (type)
            {
                case ObjectType.otElement:
                    strGuid = ((Element)o).ElementGUID;
                    break;
                case ObjectType.otPackage:
                    strGuid = ((Package)o).PackageGUID;
                    break;
                case ObjectType.otDiagram:
                    strGuid = ((Diagram)o).DiagramGUID;
                    break;
                case ObjectType.otAttribute:
                    strGuid = ((EA.Attribute)o).AttributeGUID;
                    break;
                case ObjectType.otMethod:
                    strGuid = ((Method)o).MethodGUID;
                    break;

                case ObjectType.otConnector:
                    strGuid = ((EA.Connector)o).ConnectorGUID;
                    break;
                case ObjectType.otModel:
                    strGuid = ((Package)o).PackageGUID;
                    break;
                case ObjectType.otParameter:
                    strGuid = ((Parameter)o).ParameterGUID;
                    break;
                case EA.ObjectType.otDiagramObject:
                    strGuid = $"{((EA.DiagramObject) o).InstanceGUID}";
                    break;

            }

            if (String.IsNullOrWhiteSpace(strGuid)) Clipboard.Clear();
            else Clipboard.SetText(strGuid);
        }
        /// <summary>
        /// Copy ID to ClipBoard
        /// </summary>
        /// <param name="rep"></param>
        [ServiceOperation("{363D3671-8C85-4DEA-8C41-D54CA3F3DAA1}", "Copy ID to Clipboard",
            "Copy ID to Clipboard (Package, Element, Diagram, Attribute, Operation, Connector, Parameter)",
            isTextRequired: false)]
        // ReSharper disable once UnusedMember.Global
        // dynamical usage as configurable service by reflection
        public static void CopyIdToClipboard(Repository rep)
        {
            string id = "";
            EA.ObjectType type = rep.GetContextItem(out var o);
            switch (type)
            {
                case EA.ObjectType.otElement:
                    id = $"{((EA.Element) o).ElementID}";
                    break;
                case EA.ObjectType.otPackage:
                    id = $"{((EA.Package) o).PackageID}";
                    break;
                case EA.ObjectType.otDiagram:
                    id = $"{((EA.Diagram) o).DiagramID}";
                    break;
                case EA.ObjectType.otAttribute:
                    id = $"{((EA.Attribute) o).AttributeID}";
                    break;
                case EA.ObjectType.otMethod:
                    id = $"{((EA.Method) o).MethodID}";
                    break;

                case EA.ObjectType.otConnector:
                    id = $"{((EA.Connector) o).ConnectorID}";
                    break;
                case EA.ObjectType.otModel:
                    id = $"{((EA.Package) o).PackageID}";
                    break;
                case EA.ObjectType.otDiagramObject:
                    id = $"{((EA.DiagramObject) o).InstanceID}";
                    break;
                case EA.ObjectType.otDiagramLink:
                    id = $"{((EA.DiagramLink) o).InstanceID}";
                    break;
               

            }

            if (String.IsNullOrWhiteSpace(id)) Clipboard.Clear();
            else Clipboard.SetText(id);
        }
        /// <summary>
        /// Copy Review info to ClipBoard
        /// </summary>
        /// <param name="rep"></param>
        [ServiceOperation("{28A7501A-ADDC-4250-9E05-E3561840CF3B}", "Copy Review Info: Type, Name, GUID to Clipboard",
            "Select (Package, Element, Diagram, Attribute, Operation, Connector, Parameter)",
            isTextRequired: false)]
        // ReSharper disable once UnusedMember.Global
        // dynamical usage as configurable service by reflection
        public static void CopyNamweTypeGuidToClipboard(Repository rep)
        {
            string strGuid = "";
            string type = "";
            string name = "";
            EA.ObjectType objType = rep.GetContextItem(out var o);
            switch (objType)
            {
                case EA.ObjectType.otElement:
                    strGuid = ((EA.Element)o).ElementGUID;
                    type = ((EA.Element)o).Type;
                    name = ((EA.Element)o).Name;
                    break;
                case EA.ObjectType.otPackage:
                    strGuid = ((EA.Package)o).PackageGUID;
                    type = "Package";
                    name = ((EA.Package)o).Name;
                    break;
                case EA.ObjectType.otDiagram:
                    strGuid = ((EA.Diagram)o).DiagramGUID;
                    type = $"Diagram {((EA.Diagram)o).Type}";
                    name = $"{((EA.Diagram)o).Name}";
                    break;
                case EA.ObjectType.otAttribute:
                    strGuid = ((EA.Attribute)o).AttributeGUID;
                    name = $"{((EA.Attribute)o).Name}";
                    type = "Attribute";
                    break;
                case EA.ObjectType.otMethod:
                    strGuid = ((EA.Method)o).MethodGUID;
                    name = $"{((EA.Method)o).Name}";
                    type = "Method";
                    break;

                case EA.ObjectType.otConnector:
                    strGuid = ((EA.Connector)o).ConnectorGUID;
                    type = $"{((EA.Connector)o).Type}";
                    name = $"{((EA.Connector)o).Name}";
                    break;
                case EA.ObjectType.otModel:
                    strGuid = ((EA.Package)o).PackageGUID;
                    name = $"{((EA.Package)o).Name}";
                    type = "Model";
                    break;
                case EA.ObjectType.otParameter:
                    strGuid = ((EA.Parameter)o).ParameterGUID;
                    name = $"{((EA.Parameter)o).Name}";
                    type = "Parameter";
                    break;

            }

            if (String.IsNullOrWhiteSpace(strGuid)) Clipboard.Clear();
            else Clipboard.SetText($"{type}='{name}' {strGuid}");
        }

        #region addDiagramNote
        /// <summary>
        /// Add Diagram Note
        /// </summary>
        /// <param name="rep"></param>
        public static void AddDiagramNote(Repository rep)
        {
            ObjectType oType = rep.GetContextItemType();
            if (oType.Equals(ObjectType.otDiagram))
            {
                Diagram dia = rep.GetCurrentDiagram();
                Package pkg = rep.GetPackageByID(dia.PackageID);
                if (pkg.IsProtected || dia.IsLocked) return;

                // save diagram
                rep.SaveDiagram(dia.DiagramID);

                Element elNote;
                try
                {
                    elNote = (Element) pkg.Elements.AddNew("", "Note");
                    elNote.Update();
                    pkg.Update();
                }
                catch
                {
                    return;
                }

                // add element to diagram
                // "l=200;r=400;t=200;b=600;"

                // get the position of the Element

                // get the position of the Element
                int left = (dia.cx / 2) - 100;
                int right = left + 200;
                int top = dia.cy - 150;
                int bottom = top + 120;
                if (dia.DiagramObjects.Count > 0)
                {
                    List<EA.DiagramObject> lDiaObj = EaDiagram.MakeObjectListFrmCollection(dia.DiagramObjects);

                    right = (from r in lDiaObj
                        select r.right).Max();
                    left = (from r in lDiaObj
                        select r.left).Min();
                    //left = right - 200 > 0? right - 200:0 ;
                    left = (left + (right - left) / 2) - 100;
                    left = left < 0 ? 20 : left;
                    right = left + 220;

                    top = (from r in lDiaObj
                              select r.bottom).Min() - 30;
                    bottom = top - 120;

                }

                string position = "l=" + left + ";r=" + right + ";t=" + top + ";b=" +
                                  bottom + ";";

                DiagramObject diaObject = (DiagramObject) dia.DiagramObjects.AddNew(position, "");
                dia.Update();
                diaObject.ElementID = elNote.ElementID;
                diaObject.Update();
                pkg.Elements.Refresh();

                HoUtil.SetDiagramHasAttachedLink(rep, elNote);
                rep.ReloadDiagram(dia.DiagramID);

            }
        }

        #endregion

        #region ChangeAuthorPackage

        private static void ChangeAuthorPackage(Repository rep, Package pkg, string[] args)
        {
            Element el = rep.GetElementByGuid(pkg.PackageGUID);
            el.Author = args[0];
            el.Update();
        }

        #endregion

        #region changeAuthorElement

        private static void ChangeAuthorElement(Repository rep, Element el, string[] args)
        {
            el.Author = args[0];
            el.Update();
        }

        #endregion

        #region changeAuthorDiagram

        private static void ChangeAuthorDiagram(Repository rep, Diagram dia, string[] args)
        {
            dia.Author = args[0];
            dia.Update();
        }

        #endregion

        [ServiceOperation("{EC487B04-98DC-42B3-9A50-D0880CECD9AB}", "Search nested elements like requirements",
            "Select Package, Element, all nested Elements and their Tagged Values are shown", isTextRequired: false)]

        public static void SearchNestedElements(Repository rep)
        {

            string xmlString = AddInSearches.SearchObjectsNested(rep, "");
            rep.RunModelSearch("", "", "", xmlString);

        }


        [ServiceOperation("{8704186B-665F-4E4A-95B4-31E1232A63FE}", "Search for + Copy to Clipboard selected item name, if Action try to find operation",
            "Select Package, Element, Attribute, Operation. If Action it tries to extract a possible function name", isTextRequired: false)]

        public static void SearchForName(Repository rep)
        {
            string name = GetNamesFromSelectedItems(rep);
            // run search if name found
            if (name != "")
            {
                Clipboard.SetText(name);
                // run search
                rep.RunModelSearch("Quick Search", name,"","");
            }

        }

        [ServiceOperation("{A73A53B3-6D6D-46AF-B6F2-DA252870D998}", "Copy sorted name(s) of selected items or connector, if Action try to find operation.",
            "Select Package(s), Element(s), Attribute, Operation. If Action it tries to extract a possible function name. If Connector use Guard (Flow, Transition). Multiple selection is only possible on Diagrams.", isTextRequired: false)]
        public static string CopyContextNameToClipboard(Repository rep)
        {
            string txt = GetNamesFromSelectedItems(rep);
            if (! String.IsNullOrWhiteSpace(txt)) Clipboard.SetText(txt);
            return txt;
        }

        /// <summary>
        /// Get name from context element or the selected diagram items. 
        /// If Diagram items (Element or Package) then sort them
        /// If Action try to extract the operation name.
        /// If Connector use Name or Guard if ControlFlow, DataFlow or transition
        /// </summary>
        /// <param name="rep"></param>
        /// <returns></returns>
        private static string GetNamesFromSelectedItems(Repository rep)
        {
            string names = "";
            ObjectType type = rep.GetContextItemType();
            switch (type)
            {
                case ObjectType.otElement:
                    names = NamensFromSelectedElements(rep, type);
                    break;
                case ObjectType.otDiagram:
                    names = ((Diagram) rep.GetContextObject()).Name;
                    break;
                case ObjectType.otPackage:
                    names = NamensFromSelectedElements(rep, type);
                    break;
                case ObjectType.otAttribute:
                    names = ((EA.Attribute) rep.GetContextObject()).Name;
                    break;
                case ObjectType.otMethod:
                    names = ((Method) rep.GetContextObject()).Name;
                    break;
                case ObjectType.otParameter:
                    names = ((Parameter) rep.GetContextObject()).Name;
                    break;
                case ObjectType.otDatatype:
                    names = ((Datatype) rep.GetContextObject()).Name;
                    break;

                case ObjectType.otConnector:
                    EA.Connector con = (EA.Connector) rep.GetContextObject();
                    string guard = con.TransitionGuard.Trim();
                    if ("ControlFlow ObjectFlow StateFlow".Contains(con.Type) && guard != "") names = guard;
                    else names = con.Name;
                    break;

                case ObjectType.otIssue:
                    names = ((Issue)rep.GetContextObject()).Name;
                    break;
                case ObjectType.otTest:
                    names = ((Test)rep.GetContextObject()).Name;
                    break;
                case ObjectType.otTask:
                    names = ((Task)rep.GetContextObject()).Name;
                    break;
                case ObjectType.otScenario:
                    names = ((Scenario)rep.GetContextObject()).Name;
                    break;
                case ObjectType.otClient:
                    names = ((Client)rep.GetContextObject()).Name;
                    break;
                case ObjectType.otAuthor:
                    names = ((Author)rep.GetContextObject()).Name;
                    break;
                case ObjectType.otProjectResource:
                    names = ((ProjectResource)rep.GetContextObject()).Name;
                    break;
                case ObjectType.otRequirement:
                    names = ((Requirement)rep.GetContextObject()).Name;
                    break;
                case ObjectType.otRisk:
                    names = ((Risk)rep.GetContextObject()).Name;
                    break;
                case ObjectType.otEffort:
                    names = ((Effort)rep.GetContextObject()).Name;
                    break;
                case ObjectType.otMetric:
                    names = ((Metric)rep.GetContextObject()).Name;
                    break;
            }
            return names;
        }


        /// <summary>
        /// Get names from selected item(s) and sort them alphabetic
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private static string NamensFromSelectedElements(Repository rep, ObjectType type)
        {
            var eaDia = new EaDiagram(rep);
            var names = "";
            if (eaDia.SelectedObjectsCount > 1)
            {
                // sort names
                var r = from o in eaDia.SelObjects
                    orderby NameFromElement(rep.GetElementByID(o.ElementID))
                    select NameFromElement(rep.GetElementByID(o.ElementID));
                string delimiter = "";
                foreach (var name in r)
                {
                    names = $"{names}{delimiter}{name}";
                    delimiter = "\r\n";
                }
            }
            else
            {
                names = type == ObjectType.otElement 
                    ? NameFromElement((Element) rep.GetContextObject()) 
                    : ((Package)rep.GetContextObject()).Name;
            }
            return names;
        }

        /// <summary>
        /// Get name from Element. If Action it handles CallOperation action by not copying the braces
        /// </summary>
        /// <param name="el"></param>
        /// <returns></returns>
        private static string NameFromElement(Element el)
        {
            // possible Action which contains a function
            string name = el.Name;
            if (el.Type == "Action" && el.Name.EndsWith(")") && el.Name.Contains("("))
            {
                // xxxxx( , , ) // extract function name
                Regex rx = new Regex(@"\s*(\w*)\s*\([^\)]*\)");
                Match match = rx.Match(name);
                if (match.Groups.Count == 2)
                {
                    name = match.Groups[1].Value;
                }
            }

            return name;
        }

        #region change User Recursive

        [ServiceOperation("{F0038D4B-CCAA-4F05-9401-AAAADF431ECB}", "Change user of package/element recursive",
            "Select package or element", isTextRequired: false)]
        public static void ChangeUserRecursive(Repository rep)
        {
            // get the user
            string[] s = {""};
            string oldAuthor;
            Element el = null;
            Package pkg = null;
            Diagram dia = null;
            ObjectType oType = rep.GetContextItemType();

            // get the element
            switch (oType)
            {
                case ObjectType.otPackage:
                    pkg = (Package)rep.GetContextObject();
                    el = rep.GetElementByGuid(pkg.PackageGUID);
                    oldAuthor = el.Author;
                    break;
                case ObjectType.otElement:
                    el = (Element)rep.GetContextObject();
                    oldAuthor = el.Author;
                    break;
                case ObjectType.otDiagram:
                    dia = (Diagram)rep.GetContextObject();
                    oldAuthor = dia.Author;
                    break;
                default:
                    return;
            }
            // ask for new user
            DlgUser dlg = new DlgUser(rep) {User = oldAuthor};
            dlg.ShowDialog();
            s[0] = dlg.User;
            if (s[0] == "")
            {
                MessageBox.Show($@"Author:'{s[0]}'", @"no or invalid user");
                return;
            }
            switch (oType)
            {
                case ObjectType.otPackage:
                    RecursivePackages.DoRecursivePkg(rep, pkg, ChangeAuthorPackage, ChangeAuthorElement,
                        ChangeAuthorDiagram, s, ChangeScope.PackageRecursive);
                    MessageBox.Show($@"New author:'{s[0]}'", @"Author changed for packages/elements (recursive)");
                    break;
                case ObjectType.otElement:
                    RecursivePackages.DoRecursiveEl(rep, el, ChangeAuthorElement, ChangeAuthorDiagram, s, ChangeScope.PackageRecursive);
                    MessageBox.Show($@"New author:'{ s[0]} '", @"Author changed for elements (recursive)");
                    break;
                case ObjectType.otDiagram:
                    ChangeAuthorDiagram(rep, dia, s);
                    MessageBox.Show($@"New author:' {s[0]}'", @"Author changed for diagram");
                    break;
                default:
                    return;
            }


        }

        #endregion

        #region change User

        [ServiceOperation("{4161D769-825F-494A-9389-962CC1C16E4F}", "Change Author of package/element",
            "Select package or element", isTextRequired: false)]
        public static void ChangeAuthor(Repository rep)
        {

            string[] args = {""};
            string oldAuthor;
            Element el = null;
            Package pkg = null;
            Diagram dia = null;
            ObjectType oType = rep.GetContextItemType();

            // get the element
            switch (oType)
            {
                case ObjectType.otPackage:
                    pkg = (Package)rep.GetContextObject();
                    el = rep.GetElementByGuid(pkg.PackageGUID);
                    oldAuthor = el.Author;
                    break;
                case ObjectType.otElement:
                    el = (Element)rep.GetContextObject();
                    oldAuthor = el.Author;
                    break;
                case ObjectType.otDiagram:
                    dia = (Diagram)rep.GetContextObject();
                    oldAuthor = dia.Author;
                    break;
                default:
                    return;
            }
            // ask for new user
            DlgUser dlg = new DlgUser(rep) {User = oldAuthor};
            dlg.ShowDialog();
            args[0] = dlg.User;
            if (args[0] == "")
            {
                MessageBox.Show($@"Author:'{args[0]}'", @"no or invalid user");
                return;
            }
            switch (oType)
            {
                case ObjectType.otPackage:
                    ChangeAuthorPackage(rep, pkg, args);
                    MessageBox.Show($@"New author:'{args[0]}'", @"Author changed for package");
                    break;
                case ObjectType.otElement:
                    ChangeAuthorElement(rep, el, args);
                    MessageBox.Show($@"New author:'{args[0]}'", @"Author changed for element");
                    break;
                case ObjectType.otDiagram:
                    ChangeAuthorDiagram(rep, dia, args);
                    MessageBox.Show(@"New author:'{args[0]}'", @"Author changed for element");
                    break;
                default:
                    return;
            }


        }

        #endregion


        // Set folder of package for easy access of implementation.
        [ServiceOperation("{B326B602-88F3-46E9-8EB5-9BF4F747FCB4}", "Set package folder of implementation",
            "Select package to set the implementation folder", isTextRequired: false)]
        public static void SetFolder(Repository rep)
        {

            switch (rep.GetContextItemType())
            {
                case ObjectType.otPackage:

                    Package pkg = (Package)rep.GetContextObject();
                    string folderPath = pkg.CodePath;
                    // try to infer the right folder from package class/interfaces
                    if (folderPath.Trim() == "")
                    {
                        foreach (Element el in pkg.Elements)
                        {
                            if ("Interface Component Class".Contains(el.Type))
                            {
                                if (el.Genfile != "")
                                {
                                    folderPath = Path.GetDirectoryName(HoUtil.GetGenFilePathElement(el));
                                    break;
                                }
                            }
                        }
                    }

                    using (var fbd = new FolderBrowserDialog())
                    {
                        fbd.SelectedPath = folderPath;
                        DialogResult result = fbd.ShowDialog();

                        if (result == DialogResult.OK && !String.IsNullOrWhiteSpace(fbd.SelectedPath))
                        {

                            pkg.CodePath = fbd.SelectedPath;
                            pkg.Update();
                        }
                    }

                    break;
            }
        }

        #region ShowFolder

        // show folder for:
        // - controlled package
        // - file
        [ServiceOperation("{C007C59A-FABA-4280-9B66-5AD10ACB4B13}", "Show folder of *.xml, *.h,*.c",
            "Select VC controlled package or element with file path", isTextRequired: false)]
        public static void ShowFolder(Repository rep, bool isTotalCommander = false)
        {
            string path = "";
            ObjectType oType = rep.GetContextItemType();
            switch (oType)
            {
                case ObjectType.otPackage:
                    Package pkg = (Package)rep.GetContextObject();
                    if (pkg.CodePath.Trim() != "")
                    {
                        // consider gentype (C,C++,..)
                        Element el1 = rep.GetElementByGuid(pkg.PackageGUID);
                        if (el1.Gentype == "")
                        {
                            MessageBox.Show(@"Package has no language configured. Please select a language!");
                            return;
                        }
                        path = HoUtil.GetFilePath(el1.Gentype, pkg.CodePath);
                    }
                    else
                    {
                        if (pkg.IsControlled)
                        {
                            path = HoUtil.GetVccFilePath(rep, pkg);
                            // remove filename
                            path = Regex.Replace(path, @"[a-zA-Z0-9\s_:.]*\.xml", "");
                        }
                    }
                    if (path == "") return;


                    if (isTotalCommander)
                        HoUtil.StartApp(@"totalcmd.exe", "/o " + path);
                    else
                        HoUtil.StartApp(@"Explorer.exe", "/e, " + path);
                    break;

                case ObjectType.otElement:
                    Element el = (Element)rep.GetContextObject();
                    path = HoUtil.GetGenFilePathElement(el);
                    // remove filename
                    path = Regex.Replace(path, @"[a-zA-Z0-9\s_:.]*\.[a-zA-Z0-9]{0,4}$", "");

                    if (isTotalCommander)
                        HoUtil.StartApp(@"totalcmd.exe", "/o " + path);
                    else
                        HoUtil.StartApp(@"Explorer.exe", "/e, " + path);

                    break;
            }
        }

        #endregion

        #region CreateActivityForOperation

        [ServiceOperation("{17D09C06-8FAE-4D76-B808-5EC2362B1953}", "Create/Update Activity for Operation, Class/Interface",
            "Select Package, Class/Interface, Activity (only update) or operation", isTextRequired: false)]
        public static void CreateActivityForOperation(Repository rep)
        {
            rep.EnableUIUpdates = false;
            rep.BatchAppend = true;
            ObjectType oType = rep.GetContextItemType();
            switch (oType)
            {
                case ObjectType.otMethod:
                    Method m = (Method) rep.GetContextObject();

                    // Create Activity at the end
                    Element el = rep.GetElementByID(m.ParentID);
                    Package pkg = rep.GetPackageByID(el.PackageID);
                    int pos = pkg.Packages.Count + 1;
                    ActivityPar.CreateActivityForOperation(rep, m, pos);

                    rep.BatchAppend = false;
                    rep.EnableUIUpdates = true;
                    rep.RefreshModelView(el.PackageID);

                    rep.ShowInProjectView(m);
                    break;

                case ObjectType.otElement:
                    el = (Element) rep.GetContextObject();
                    if (el.Locked) return;

                    if (el.Type == "Activity")
                    {
                        UpdateActivityMethodParameterWrapper(rep);
                    }
                    else
                    {
                        CreateActivityForOperationsInElement(rep, el);
                    }
                    rep.BatchAppend = false;
                    rep.EnableUIUpdates = true;
                    rep.RefreshModelView(el.PackageID);

                    rep.ShowInProjectView(el);
                    break;

                case ObjectType.otPackage:
                    pkg = (Package) rep.GetContextObject();
                    CreateActivityForOperationsInPackage(rep, pkg);
                    // update sort order of packages
                    rep.BatchAppend = false;
                    rep.EnableUIUpdates = true;
                    rep.RefreshModelView(pkg.PackageID);
                    rep.ShowInProjectView(pkg);
                    break;
            }
            rep.BatchAppend = false;
            rep.EnableUIUpdates = true;
        }

        #endregion

        private static void CreateActivityForOperationsInElement(Repository rep, Element el)
        {
            if (el.Locked) return;
            Package pkg = rep.GetPackageByID(el.PackageID);
            int treePos = pkg.Packages.Count + 1;
            foreach (Method m1 in el.Methods)
            {
                // Create Activity
                ActivityPar.CreateActivityForOperation(rep, m1, treePos);
                treePos = treePos + 1;

            }

        }
        public static void CreateCompositeActivityFromText(Repository rep, string s)
        {
            s = DeleteCommentStrings(s);
            string name = CallOperationAction.RemoveUnwantedStringsFromText(s);
            string guardString = "";
            if (name.StartsWith("case",StringComparison.Ordinal))
            {
                name = name.Substring(5).Trim();
                name = name.Replace(":", "").Trim();
                guardString = name;
            }
            CreateDiagramObjectFromContext(rep, name, "Activity", "Comp=yes",
                    guardString:guardString
            );
        }
        /// <summary>
        /// Add one or more stereotypes to the context element. Supported are:
        /// - Attribute
        /// - Operation
        /// - Connector
        /// - Element
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="s">Comma separated stereotypes</param>
        public static void AddMacroFromText(Repository rep, string s)
        {
            s = s.Replace(" ", "").Replace("(", "").Replace(")", "");
            if (s == "") s = "define";
            switch (rep.GetContextItemType())
            {
                case ObjectType.otElement:
                    EA.Attribute el = (EA.Attribute)rep.GetContextObject();
                    el.StereotypeEx = UpdateStereotype(s, el.StereotypeEx);
                    el.Update();
                    break;
                case ObjectType.otAttribute:
                    EA.Attribute a = (EA.Attribute)rep.GetContextObject();
                    a.StereotypeEx = UpdateStereotype(s, a.StereotypeEx);
                    a.Update();
                    break;
                case ObjectType.otMethod:
                    Method o = (Method)rep.GetContextObject();
                    o.StereotypeEx = UpdateStereotype(s, o.StereotypeEx);
                    o.Update();
                    break;
                case ObjectType.otConnector:
                    EA.Connector c = (EA.Connector)rep.GetContextObject();
                    c.StereotypeEx = UpdateStereotype(s, c.StereotypeEx);
                    c.Update();
                    break;
            }
        }
        /// <summary>
        /// Set one or more stereotypes to the context element. Supported are:
        /// - Attribute
        /// - Operation
        /// - Connector
        /// - Element
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="s">Comma separated stereotypes</param>
        public static void SetMacroFromText(Repository rep, string s)
        {
            s = s.Replace(" ", "").Replace("(","").Replace(")", "");

            Diagram dia = rep.GetCurrentDiagram();
            if (dia != null)
            {
                rep.SaveDiagram(dia.DiagramID);
            }
            switch (rep.GetContextItemType())
            {
                case ObjectType.otElement:
                    Element el = (Element)rep.GetContextObject();
                    el.StereotypeEx = s;
                    el.Update();
                    break;
                case ObjectType.otAttribute:
                    EA.Attribute a = (EA.Attribute)rep.GetContextObject();
                    a.StereotypeEx = s;
                    a.Update();
                    break;
                case ObjectType.otMethod:
                    Method o = (Method)rep.GetContextObject();
                    o.StereotypeEx = s;
                    o.Update();
                    break;
                case ObjectType.otConnector:
                    EA.Connector c = (EA.Connector)rep.GetContextObject();
                    c.StereotypeEx = s;
                    c.Update();
                    break;
            }
            if (dia != null)
            {
                rep.ReloadDiagram(dia.DiagramID);
            }
        }
        /// <summary>
        /// Update Stereotype by a comma separated string of stereotypes
        /// </summary>
        /// <param name="newStereotypes"></param>
        /// <param name="oldStereotypes"></param>
        /// <returns></returns>
        private static string UpdateStereotype(string newStereotypes, string oldStereotypes)
        {
            string stereotypes = oldStereotypes.Trim();
            string[] lStereotypesNew = newStereotypes.Split(',');
            string[] lStereotypesOld = oldStereotypes.Split(',');
            foreach (var stereotype in lStereotypesNew)
            {
                // add stereotype if not already contained
                if (lStereotypesOld.Contains(stereotype)) continue;
                stereotypes = stereotypes == "" ? stereotype: $"{stereotypes},{stereotype}";
            }
            return stereotypes;

        }


        /// <summary>
        /// Create Activity from text
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="s"></param>
        public static void CreateActivityFromText(Repository rep, string s)
        {
            s = DeleteComment(s);
            string activityName = CallOperationAction.RemoveBody(s);
            activityName = CallOperationAction.RemoveUnwantedStringsFromText(activityName);
            // remove all between '{' and '}'
            string guardString = "";
            if (activityName.StartsWith("case", StringComparison.Ordinal))
            {
                activityName = activityName.Substring(5).Trim();
                activityName = activityName.Replace(":", "").Trim();
                guardString = activityName;
            }
            // Create Activity and get new createt Diagram Object 
            CreateDiagramObjectFromContext(rep, activityName, "Activity", "Comp=no",
                guardString: guardString);

            // Create Activity body
            string activityBody = CallOperationAction.GetBody(s);

            activityBody = CallOperationAction.RemoveUnwantedStringsFromText(activityBody,deleteMultipleSpaces:true, isLoop:true);
            // Insert the remaining stuff into Activity beneath init
            InsertInActivtyDiagram(rep, activityBody);
        }

        private static void CreateActivityForOperationsInPackage(Repository rep, Package pkg)
        {
            foreach (Element el in pkg.Elements)
            {
                CreateActivityForOperationsInElement(rep, el);

            }
            foreach (Package pkg1 in pkg.Packages)
            {
                CreateActivityForOperationsInPackage(rep, pkg1);
            }
            
        }
        private static bool LocateTextOrFrame(Repository rep, Element el)
        {
            if (el.Type == "Text")
            {
                string s = el.MiscData[0];
                int id = Convert.ToInt32(s);
                Diagram dia = rep.GetDiagramByID(id);
                rep.ShowInProjectView(dia);
                return true;
            }
            // display the original diagram on what the frame is based
            if (el.Type == "UMLDiagram")
            {
                int id = Convert.ToInt32(el.MiscData[0]);
                Diagram dia = rep.GetDiagramByID(id);
                rep.ShowInProjectView(dia);
                return true;

            }
            return false;
        }
        #region HideAllEmbeddedElementI

        /// <summary>
        /// Hide all embedded Elements for:
        /// - selected nodes
        /// - all if nothing is selected
        /// </summary>
        /// <param name="rep"></param>
        [ServiceOperation("{5ED3DABA-367E-4575-A161-D79F838A5A17}", "Hide Ports, Pins, Parameter",
            "Selected Diagram Objects or all", isTextRequired: false)]
        public static void HideEmbeddedElements(
            Repository rep)
        {
            Cursor.Current = Cursors.WaitCursor;
            // remember Diagram data of current selected diagram
            var eaDia = new EaDiagram(rep);
            if (eaDia.Dia == null) return;
            // Save to avoid indifferent states
            rep.SaveDiagram(eaDia.Dia.DiagramID);


            // over all selected elements, simulation for loop by foreach
            int count = -1;
            foreach (DiagramObject unused in eaDia.SelObjects)
            {
                count = count + 1;
                var elSource = eaDia.SelElements[count];
                if (elSource.IsEmbeddedElement())
                {
                    // selected element was port
                    RemoveEmbeddedElementFromDiagram(eaDia.Dia, elSource);
                }
                else
                {
                    // selected element was "Element"
                    foreach (Element embeddedElement in elSource.EmbeddedElements)
                    {
                        if (embeddedElement.IsEmbeddedElement())
                        {
                            RemoveEmbeddedElementFromDiagram(eaDia.Dia, embeddedElement);
                        }
                    }
                }




            }
            // display changes
            rep.ReloadDiagram(eaDia.Dia.DiagramID);
            eaDia.ReloadSelectedObjectsAndConnector();
            Cursor.Current = Cursors.Default;
        }

        #endregion
        #region UpdateEmbeddedElementStyle

        private static void UpdateEmbeddedElementStyle(Repository rep, PortServices.LabelStyle style)
        {
            Cursor.Current = Cursors.WaitCursor;
            // remember Diagram data of current selected diagram
            var eaDia = new EaDiagram(rep);
            if (eaDia.Dia == null) return;
            // Save to avoid indifferent states
            rep.SaveDiagram(eaDia.Dia.DiagramID);

            // over all selected elements
            int count = -1;
            foreach (DiagramObject diaObj in eaDia.SelObjects)
            {
                count = count + 1;
                var elSource = eaDia.SelElements[count];
                if (elSource.IsEmbeddedElement() |
                    "ProvidedInterface RequiredInterface".Contains(elSource.Type))
                {

                    PortServices.DoChangeLabelStyle(diaObj, style);
                }
                else
                {
                    // selected element was "Element"
                    foreach (Element embeddedElement in elSource.EmbeddedElements)
                    {
                        if (embeddedElement.IsEmbeddedElement())
                        {
                            var diagramObject = eaDia.Dia.GetDiagramObjectByID(embeddedElement.ElementID, "");
                            if (diagramObject == null) continue;
                            PortServices.DoChangeLabelStyle(diagramObject, style);
                        }
                    }
                }




            }
            // display changes
            rep.ReloadDiagram(eaDia.Dia.DiagramID);
            eaDia.ReloadSelectedObjectsAndConnector();
            Cursor.Current = Cursors.Default;
        }

        #endregion
        /// <summary>
        /// Delete the embedded element from Diagram (Port, Parameter, Pin). It removes it recursive
        /// </summary>
        /// <param name="dia"></param>
        /// <param name="embeddedElement"></param>
        private static void RemoveEmbeddedElementFromDiagram(Diagram dia, Element embeddedElement)
        {
            // delete recursive embedded elements
            foreach (Element el in embeddedElement.EmbeddedElements)
            {
                RemoveEmbeddedElementFromDiagram(dia, el);
            }
            // delete the embedded element from diagram
            for (int i = dia.DiagramObjects.Count - 1; i >= 0; i -= 1)
            {
                var obj = (DiagramObject)dia.DiagramObjects.GetAt((short)i);
                if (obj.ElementID == embeddedElement.ElementID)
                {
                    dia.DiagramObjects.Delete((short)i);
                    dia.DiagramObjects.Refresh();
                    break;
                }
            }


        }

        #region ShowEmbeddedElementLabel

        /// <summary>
        /// Show embedded Element Labels for:
        /// - selected nodes
        /// - all if nothing is selected
        /// </summary>
        /// <param name="rep"></param>
        [ServiceOperation("{FBEF4500-DD24-4D23-BC7F-08D70DDA2B57}", "Show Port, Pin Parameter Labels",
            "Selected Diagram Objects or all", isTextRequired: false)]
        // ReSharper disable once UnusedMember.Global
        public static void ShowEmbeddedElementsLabel(Repository rep)
        {
            UpdateEmbeddedElementStyle(rep, PortServices.LabelStyle.IsShown);
        }

        #endregion

        #region HideEmbeddedElementsLabel

        /// <summary>
        /// Hide embedded Element Labels for:
        /// - selected nodes
        /// - all if nothing is selected
        /// </summary>
        /// <param name="rep"></param>
        [ServiceOperation("{3493B9E6-F6DA-478E-A161-DD95D1D34B44}", "Hide Ports, Pins, Parameter Label",
            "Selected Diagram Objects or all", isTextRequired: false)]
        public static void HideEmbeddedElementsLabel(Repository rep)
        {
            UpdateEmbeddedElementStyle(rep, PortServices.LabelStyle.IsHidden);
        }

        #endregion

        #region HideEmbeddedElementsType

        /// <summary>
        /// Hide embedded Element Labels for:
        /// - selected nodes
        /// - all if nothing is selected
        /// </summary>
        /// <param name="rep"></param>
        [ServiceOperation("{CF59707B-35A3-4E0C-AA0D-16722DB61F7D}", "Hide Port Type",
            "Selected Diagram Objects or all", isTextRequired: false)]
        public static void HideEmbeddedElementsType(Repository rep)
        {
            UpdateEmbeddedElementStyle(rep, PortServices.LabelStyle.IsTypeHidden);
        }

        #endregion

        #region ShowEmbeddedElementsType

        /// <summary>
        /// Hide embedded Element Labels for:
        /// - selected nodes
        /// - all if nothing is selected
        /// </summary>
        /// <param name="rep"></param>
        [ServiceOperation("{CF59707B-35A3-4E0C-AA0D-16722DB61F7D}", "Show Port Type",
            "Selected Diagram Objects or all", isTextRequired: false)]
        // ReSharper disable once UnusedMember.Global
        public static void ShowEmbeddedElementsType(Repository rep)
        {
            UpdateEmbeddedElementStyle(rep, PortServices.LabelStyle.IsTypeShown);
        }

        #endregion

        #region showAllEmbeddedElementsGUI
        /// <summary>
        /// Shows all embedded elements of type: Port. A Port may have a type or interfaces.
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="embeddedElementType"></param>
        [ServiceOperation("{678AD901-1D2F-4FB0-BAAD-AEB775EE18AC}", "Show all ports for Component", "Select Class, Interface or Component", isTextRequired: false)]
        public static void ShowEmbeddedElementsGui(Repository rep, string embeddedElementType="Port Pin Parameter")
        {
            EaDiagram eaDia = new EaDiagram(rep);
            var dia = eaDia.Dia;
            if (dia == null) return;
            rep.SaveDiagram(dia.DiagramID);
            // over all selected elements
            foreach (DiagramObject diaObj in dia.SelectedObjects)
            {
                var elSource = rep.GetElementByID(diaObj.ElementID);
                // find object on Diagram
                var diaObjSource = dia.GetDiagramObjectByID(elSource.ElementID, "");
                if (diaObjSource == null) return;
                
                int pos = 0;
                foreach (Element elEmbedded in elSource.EmbeddedElements)
                {

                    if (embeddedElementType == "" | embeddedElementType.Contains(elEmbedded.Type))
                    {
                        // only ports / parameters
                        if (elEmbedded.Type == "ActivityParameter" | elEmbedded.EmbeddedElements.Count==0)
                        {
                            // visualize only Activity Parameters
                            bool newPort = HoUtil.VisualizePortForDiagramobject(rep, pos, dia, diaObjSource, elEmbedded, null);
                            if (newPort) pos = pos + 1;
                        }
                        else
                        {
                            // Port: Visualize embedded Port + embedded Interface
                            foreach (Element interf in elEmbedded.EmbeddedElements)
                            {
                                bool newPort = HoUtil.VisualizePortForDiagramobject(rep, pos, dia, diaObjSource, elEmbedded, interf);
                                if (newPort) pos = pos + 1;
                            }
                        }
                    }
                }
            }
            rep.ReloadDiagram(dia.DiagramID);
            eaDia.ReloadSelectedObjectsAndConnector();


        }
        #endregion
        public static void NavigateComposite(Repository repository)
        {
            ObjectType oType = repository.GetContextItemType();
            // find composite element of diagram
            if (oType.Equals(ObjectType.otDiagram))
            {
                Diagram d = (Diagram)repository.GetContextObject();
                string guid = HoUtil.GetElementFromCompositeDiagram(repository, d.DiagramGUID);
                if (guid != "")
                {
                    repository.ShowInProjectView(repository.GetElementByGuid(guid));
                }

            }
            // find composite diagram of element of element
            if (oType.Equals(ObjectType.otElement))
            {
                Element e = (Element)repository.GetContextObject();
                // locate text or frame
                if (LocateTextOrFrame(repository, e)) return;

                repository.ShowInProjectView(e.CompositeDiagram);
            }
        }
        public static void FindUsage(Repository rep)
        {
            ObjectType oType = rep.GetContextItemType();
            if (oType.Equals(ObjectType.otElement))
            {
                // locate text or frame
                var el = (Element)rep.GetContextObject();
                if (LocateTextOrFrame(rep, el)) return;
                rep.RunModelSearch("Element usage", el.ElementGUID, "", "");
            }
            if (oType.Equals(ObjectType.otMethod))
            {
                Method method = (Method)rep.GetContextObject();
                rep.RunModelSearch("Method usage", method.MethodGUID, "", "");
            }
            if (oType.Equals(ObjectType.otDiagram))
            {
                Diagram dia = (Diagram)rep.GetContextObject();
                rep.RunModelSearch("Diagram usage", dia.DiagramGUID, "", "");
            }
            if (oType.Equals(ObjectType.otConnector))
            {
                EA.Connector con = (EA.Connector)rep.GetContextObject();
                rep.RunModelSearch("Connector is visible in Diagrams",
                    con.ConnectorID.ToString(), "", "");
            }
        }

        public static void ShowSpecification(Repository rep)
        {
            ObjectType oType = rep.GetContextItemType();
            if (oType.Equals(ObjectType.otElement))
            {
                var el = (Element)rep.GetContextObject();
                //over all file
                foreach (EA.File f in el.Files)
                {
                    if (f.Name.Length > 2)
                    {
                        Process.Start(f.Name);
                    }
                }
            }
        }
        #region LineStyle
        #region setLineStyleLV
        [ServiceOperation("{5F5CB088-1DDD-4A00-B641-273CAC017AE5}", "Set line style LV(Lateral Vertical)", "Select Diagram, connector, nodes", isTextRequired: false)]
        #endregion
        public static void SetLineStyleLv(Repository rep)
        {
            SetLineStyle(rep, "LV");
        }
         [ServiceOperation("{9F1E7448-3B3B-4058-83AB-CBA97F24B90B}", "Set line style LH(Lateral Horizontal)", "Select Diagram, connector, nodes", isTextRequired: false)]
         public static void SetLineStyleLh(Repository rep)
         {
             SetLineStyle(rep, "LH");
         }
         [ServiceOperation("{A8199FFF-A9BA-4875-9529-45B2801F0DB3}", "Set line style TV(Tree Vertical)", "Select Diagram, connector, nodes", isTextRequired: false)]
         public static void SetLineStyleTv(Repository rep)
         {
             SetLineStyle(rep, "TV");
         }
         [ServiceOperation("{5E481745-C684-431D-BD02-AD22EE39C252}", "Set line style TH(Tree Horizontal)", "Select Diagram, connector, nodes", isTextRequired: false)]
         public static void SetLineStyleTh(Repository rep)
         {
             SetLineStyle(rep, "TH");
         }
         [ServiceOperation("{A8199FFF-A9BA-4875-9529-45B2801F0DB3}", "Set line style OS(Orthogonal Square)", "Select Diagram, connector, nodes", isTextRequired: false)]
         public static void SetLineStyleOs(Repository rep)
         {
             SetLineStyle(rep, "OS");
         }
         [ServiceOperation("{D7B75725-60B7-4C73-913F-164E6EE847D3}", "Set line style OR(Orthogonal Round)", "Select Diagram, connector, nodes", isTextRequired: false)]
         public static void SetLineStyleOr(Repository rep)
         {
             SetLineStyle(rep, "OR");
         }
         [ServiceOperation("{99F31FC7-8326-468B-B1D8-2542BBC8D4EB}", "Set line style B(Bezier)", "Select Diagram, connector, nodes", isTextRequired: false)]
         public static void SetLineStyleB(Repository rep)
         {
             SetLineStyle(rep, "B");
         }

        public static void SetLineStyle(Repository repository, string lineStyle)
        {
          EA.Connector con = null;
            Collection objCol = null;
            ObjectType oType = repository.GetContextItemType();
            Diagram diaCurrent = repository.GetCurrentDiagram();
            if (diaCurrent != null)
            {
                con = diaCurrent.SelectedConnector;
                objCol = diaCurrent.SelectedObjects;
            }
            // all connections of diagram
            if (oType.Equals(ObjectType.otDiagram))
            {
                HoUtil.SetLineStyleDiagram(repository, diaCurrent, lineStyle);
            }
            // all connections of diagram elements
            Debug.Assert(objCol != null, "objCol != null");
            if (objCol.Count >0 | con != null)
            {
                HoUtil.SetLineStyleDiagramObjectsAndConnectors(repository, diaCurrent, lineStyle);
            }
            
        }
        #endregion
        #region DisplayOperationForSelectedElement
        // display behavior or definition for selected element
        // displayMode: "Behavior" or "Method"
        public static void DisplayOperationForSelectedElement(Repository repository, DisplayMode showBehavior)
        {
            ObjectType oType = repository.GetContextItemType();
            // Method found
            if (oType.Equals(ObjectType.otMethod))
            {
                // display behavior for method
                Appl.DisplayBehaviorForOperation(repository, (Method)repository.GetContextObject());

            }
            if (oType.Equals(ObjectType.otDiagram))
            {
                // find parent element
                Diagram dia = (Diagram)repository.GetContextObject();
                if (dia.ParentID > 0)
                {
                    // find parent element
                    Element parentEl = repository.GetElementByID(dia.ParentID);
                    //
                    LocateOperationFromBehavior(repository, parentEl, showBehavior);
                }
                else
                {
                    // open diagram
                    repository.OpenDiagram(dia.DiagramID);
                }
            }


            // Connector / Message found
            if (oType.Equals(ObjectType.otConnector))
            {
                EA.Connector con = (EA.Connector)repository.GetContextObject();
                if (con.Type.Equals("StateFlow"))
                {

                    Method m = HoUtil.GetOperationFromConnector(repository, con);
                    if (m != null)
                    {
                        if (showBehavior.Equals(DisplayMode.Behavior))
                        {
                            Appl.DisplayBehaviorForOperation(repository, m);
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
                    if (opName.EndsWith(")", StringComparison.Ordinal))
                    {
                        // extract the name
                        int pos = opName.IndexOf("(", StringComparison.Ordinal);
                        opName = opName.Substring(0, pos);
                        Element el = repository.GetElementByID(con.SupplierID);
                        // find operation by name
                        foreach (Method op in el.Methods)
                        {
                            if (op.Name == opName)
                            {
                                repository.ShowInProjectView(op);
                                //Appl.DisplayBehaviorForOperation(Repository, op);
                                return;
                            }
                        }
                        if ((el.Type.Equals("Sequence") || el.Type.Equals("Object")) && el.ClassfierID > 0)
                        {
                            el = repository.GetElementByID(el.ClassifierID);
                            foreach (Method op in el.Methods)
                            {
                                if (op.Name == opName)
                                {
                                    if (showBehavior.Equals(DisplayMode.Behavior))
                                    {
                                        Appl.DisplayBehaviorForOperation(repository, op);
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
            if (oType.Equals(ObjectType.otElement))
            {
                Element el = (Element)repository.GetContextObject();
                // locate text or frame
                if (LocateTextOrFrame(repository, el)) return;

                if (el.Type.Equals("Activity") & showBehavior.Equals(DisplayMode.Behavior))
                {
                    // Open Behavior for Activity
                    HoUtil.OpenBehaviorForElement(repository, el);


                }
                if (el.Type.Equals("State"))
                {
                    // get operations
                    foreach (Method m in el.Methods)
                    {
                        // display behaviors for methods
                        Appl.DisplayBehaviorForOperation(repository, m);
                    }
                }

                if (el.Type.Equals("Action"))
                {
                    foreach (CustomProperty custproperty in el.CustomProperties)
                    {
                        if (custproperty.Name.Equals("kind") && custproperty.Value.Equals("CallOperation"))
                        {
                            ShowFromElement(repository, el, showBehavior);
                        }
                        if (custproperty.Name.Equals("kind") && custproperty.Value.Equals("CallBehavior"))
                        {
                            el = repository.GetElementByID(el.ClassfierID);
                            HoUtil.OpenBehaviorForElement(repository, el);
                        }
                    }

                }
                if (showBehavior.Equals(DisplayMode.Method) & (
                    el.Type.Equals("Activity") || el.Type.Equals("StateMachine") || el.Type.Equals("Interaction")) )
                {
                    LocateOperationFromBehavior(repository, el, showBehavior);
                }
            }
        }
        #endregion
        private static void ShowFromElement(Repository repository, Element el, DisplayMode showBehavior)
        {
            Method method = HoUtil.GetOperationFromAction(repository, el);
            if (method != null)
            {
                if (showBehavior.Equals(DisplayMode.Behavior))
                {
                    Appl.DisplayBehaviorForOperation(repository, method);
                }
                else
                {
                    repository.ShowInProjectView(method);
                }
            }
        }

        private static void LocateOperationFromBehavior(Repository repository, Element el, DisplayMode showBehavior)
        {
            Method method = HoUtil.GetOperationFromBrehavior(repository, el);
            if (method != null)
            {
                if (showBehavior.Equals(DisplayMode.Behavior))
                {
                    Appl.DisplayBehaviorForOperation(repository, method);
                }
                else
                {
                    repository.ShowInProjectView(method);
                }
            }
        }
        // ReSharper disable once UnusedMember.Local
        private static void BehaviorForOperation(Repository repository, Method method)
        {
            string behavior = method.Behavior;
            if (behavior.StartsWith("{", StringComparison.Ordinal) & behavior.EndsWith("}", StringComparison.Ordinal))
            {
                // get object according to behavior
                Element el = repository.GetElementByGuid(behavior);
                // Activity
                if (el == null) { }
                else
                {
                    if (el.Type.Equals("Activity") || el.Type.Equals("Interaction") || el.Type.Equals("StateMachine"))
                    {
                        HoUtil.OpenBehaviorForElement(repository, el);
                    }
                }
            }
        }
        #region createDiagramObjectFromContext
        //----------------------------------------------------------------------------------------
        // type:      "Action", "Activity", "CallOperation", CallBehavior", "Decision", "MergeNode","StateNode"
        // extension: "CallOperation" ,"CallBehavior": methodName
        //             "101"=StateNode, Final, 
        //             "no"= else/no Merge
        //             "comp=yes":  Activity with composite Diagram
        //----------------------------------------------------------------------------------------
        public static DiagramObject  CreateDiagramObjectFromContext(Repository rep, string name, string type,
            string extension, 
            int offsetHorizental = 0, int offsetVertical = 0, string guardString = "", Element srcEl=null)
        {
            int WidthPerCharacter = 60;
            // filter out linefeed, tab
            name = Regex.Replace(name, @"(\n|\r|\t)", "", RegexOptions.Singleline);

            if (name.Length > 255)
            {
                MessageBox.Show(type + @": '" + name + @"' has more than 255 characters.", @"Name is to long");
                return null;
            }


            string basicType = type;
            if (type == "CallOperation" || type == "CallBehavior") basicType = "Action";

            Diagram dia = rep.GetCurrentDiagram();
            if (dia == null) return null;

            rep.SaveDiagram(dia.DiagramID);
            Element elParent = null;

            // only one diagram object selected as source
            var elSource = srcEl ?? HoUtil.GetElementFromContextObject(rep);
            if (elSource == null)
            {
                // Package selected/Context Element
                if (rep.GetContextItemType() == ObjectType.otPackage)
                {
                    var pkg = (EA.Package)rep.GetContextObject();
                    elSource = rep.GetElementByGuid(pkg.PackageGUID);
                    DiagramObjectFromContext(rep, name, extension, elSource, basicType, ref elParent);
                    return null;
                }
                // Diagram selected, Context Element
                if (rep.GetContextItemType() == ObjectType.otDiagram)
                {
                    var diaContext = (EA.Diagram)rep.GetContextObject();
                    if (diaContext.ParentID > 0)
                    {
                        elSource = rep.GetElementByID(diaContext.ParentID);
                    }
                    else
                    {
                        var pkg = rep.GetPackageByID(diaContext.PackageID);
                        elSource = rep.GetElementByGuid(pkg.PackageGUID);
                    }

                    DiagramObjectFromContext(rep, name, extension, elSource, basicType, ref elParent);
                    return null;
                }
                return null;
            }


            var diaObjSource = dia.GetDiagramObjectByID(elSource.ElementID, "");

            string noValifTypes = "Note, Constraint, Boundary, Text, UMLDiagram, DiagramFrame";
            if (noValifTypes.Contains(elSource.Type)) return null;
           

            if (elSource.ParentID != 0)
            {
                dia.GetDiagramObjectByID(elSource.ParentID, "");
            }
           
            var elTarget = DiagramObjectFromContext(rep, name, extension, elSource, basicType, ref elParent);
            if (elTarget == null) return null; // not created target element
            if (diaObjSource == null) return null; // not created diagram object

            int left = diaObjSource.left + offsetHorizental;
            int right = diaObjSource.right + offsetHorizental;
            int top = diaObjSource.top + offsetVertical;
            int bottom = diaObjSource.bottom + offsetVertical;
            int length;

            if (basicType == "StateNode")
            {
                left = left - 10 + (right - left) / 2;
                right = left + 20;
                top = bottom - 20;
                bottom = top - 20;
            }
            if ((basicType == "Decision") | (basicType == "MergeNode"))
            {
                if (guardString == "no")
                {
                    if (elSource.Type == "Decision") left = left + (right -left) + 200;
                    else left = left + (right -left) + 50;
                    bottom = bottom - 5;
                }
                left = left - 15 + (right - left)/2;
                right = left + 30; 
                top = bottom - 20;
                bottom = top - 40;
            }
            if (basicType == "Action" | basicType == "Activity")
            {
                length = name.Length * WidthPerCharacter / 10;

                if (extension.ToLower() == "comp=no")
                { /* Activity ind diagram */
                    if (length < 500) length = 500;
                    left = left + ((right - left) / 2) - (length / 2);
                    right = left + length;
                    top = bottom - 20;
                    bottom = top - 200;
                    if (basicType == "Activity") bottom = top - 400;


                }
                else if (extension.ToLower() == "comp=yes")
                {
                    if (length < 220) length = 220;
                    left = left + ((right - left) / 2) - (length / 2);
                    right = left + length;
                    top = bottom - 40;
                    bottom = top - 40;
                }
                else
                {

                    if (length < 220) length = 220;
                    left = left + ((right - left) / 2) - (length / 2);
                    right = left + length;
                    top = bottom - 20;
                    bottom = top - 20;
                }

            }
            // limit values
            if (left < 5) left = 5;
            string position = "l=" + left + ";r=" + right + ";t=" + top + ";b=" + bottom + ";";
            // end note
            if ( elParent != null && elParent.Type == "Activity" && extension == "101")
            {
                DiagramObject diaObj = dia.GetDiagramObjectByID(elParent.ElementID,"");
                if (diaObj != null)
                {
                    diaObj.bottom = bottom - 40;
                    diaObj.Update();
                }
            }


            HoUtil.AddSequenceNumber(rep, dia);
            var diaObjTarget = (DiagramObject)dia.DiagramObjects.AddNew(position, "");
            diaObjTarget.ElementID = elTarget.ElementID;
            diaObjTarget.Sequence = 1;
            diaObjTarget.Update();
            HoUtil.SetSequenceNumber(rep, dia, diaObjTarget, "1");

            // position the label:
            // LBL=CX=180:  length of label
            // CY=13:       hight of label
            // OX=26:       x-position of label (relative object)
            // CY=13:       y-position of label (relative object)
            if (basicType == "Decision" & name.Length > 0)
            {
                if (name.Length > 25) length = 25 * WidthPerCharacter / 10;
                else length = name.Length * WidthPerCharacter / 10;
                // string s = "DUID=E2352ABC;LBL=CX=180:CY=13:OX=29:OY=-4:HDN=0:BLD=0:ITA=0:UND=0:CLR=-1:ALN=0:ALT=0:ROT=0;;"; 
                string s = "DUID=E2352ABC;LBL=CX=180:CY=13:OX=-"+ length+ ":OY=-4:HDN=0:BLD=0:ITA=0:UND=0:CLR=-1:ALN=0:ALT=0:ROT=0;;"; 
                HoUtil.SetDiagramObjectLabel(rep,
                    diaObjTarget.ElementID, diaObjTarget.DiagramID, diaObjTarget.InstanceID, s);
            }

            if (extension == "Comp=no")
                { /* Activity in diagram */
                    // place an init
                    int initLeft = left + ((right - left) / 2) - 10;
                    int initRight = initLeft + 20;
                    int initTop = top - 25;
                    int initBottom = initTop - 20;
                    string initPosition = "l=" + initLeft + ";r=" + initRight + ";t=" + initTop + ";b=" + initBottom + ";";
                    // set target
                    elTarget.Name = name;
                    diaObjTarget = ActivityPar.CreateInitFinalNode(rep, dia,
                        elTarget, 100, initPosition);
                    
                }

                var con = DrawConnectorBetweenElements(elSource, elTarget,"ControlFlow","");

            // set line style LV
                foreach (DiagramLink link in dia.DiagramLinks)
                {
                    if (link.ConnectorID == con.ConnectorID)
                    {
                        if (guardString != "no")
                        {
                            link.Geometry = "EDGE=3;$LLB=;LLT=;LMT=;LMB=CX=21:CY=13:OX=-20:OY=-19:HDN=0:BLD=0:ITA=0:UND=0:CLR=-1:ALN=0:DIR=0:ROT=0;LRT=;LRB=;IRHS=;ILHS=;";
                        }
                        // in case of switch case line style = LH
                        string style = "LV";
                        if ((elSource.Type == "Action" | elSource.Type == "Activity") & guardString == "no") style = "LH";
                        // Switch case: use LH LineStyle
                        if (Regex.IsMatch(elSource.Name, @"switch[\s]*\(")) style = "LH";
                        HoUtil.SetLineStyleForDiagramLink(style, link);
                        
                        break;
                    }
                }
             

                // set Guard
                if (guardString != "") {
                    if (guardString == "no" && elSource.Type != "Decision")
                    {
                       // mo GUARD
                    }
                    else
                    {
                        // GUARD
                        HoUtil.SetConnectorGuard(rep, con.ConnectorID, guardString);
                    }
                }
                else if (elSource.Type.Equals("Decision") & !elSource.Name.Trim().Equals(""))
                {
                    HoUtil.SetConnectorGuard(rep, con.ConnectorID, guardString == "no" ? "no" : "yes");
                }

                // handle subtypes of action
                if (type == "CallOperation")
                {

                    Method method = CallOperationAction.GetMethodFromMethodName(rep, extension);
                    if (method != null)
                    {
                        //CallOperationAction.CreateCallAction(rep, elTarget, method);
                        Activity.CreateCallAction(rep, elTarget, method.MethodGUID, "CallOperation");
                    }
                    
                }

                string methodName = extension;
                if (type == "CallBehavior")
                {

                    EA.Element activity = Activity.GetActivityFromMethodName(rep, methodName, verbose:true);

                    
                    if (activity != null)
                    {
                        Activity.CreateCallAction(rep, elTarget, activity.ElementGUID, "CallBehavior");

                    }
                    
                }
                


            // set selected object
            rep.SaveDiagram(dia.DiagramID);
            rep.ReloadDiagram(dia.DiagramID);
            Element elT = rep.GetElementByID(diaObjTarget.ElementID);
            dia.SelectedObjects.AddNew(diaObjTarget.ElementID.ToString(),elT.ObjectType.ToString());
            dia.SelectedObjects.Refresh();
            return diaObjTarget;
                
            
        }
        /// <summary>
        /// Create a DiagramObject beneath a source element
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="name"></param>
        /// <param name="extension"></param>
        /// <param name="elSource">The element to store the target in the same place</param>
        /// <param name="basicType"></param>
        /// <param name="elParent"></param>
        /// <returns></returns>
        private static EA.Element DiagramObjectFromContext(Repository rep, string name, string extension, Element elSource,
            string basicType, ref Element elParent)
        {
            Element elTarget;
            try
            {
                // Source is a package
                if (elSource.Type == "Package")
                {
                    var pkg = rep.GetPackageByGuid(elSource.ElementGUID);
                    elTarget = (Element)pkg.Elements.AddNew(name, basicType);
                    if (basicType == "StateNode") elTarget.Subtype = Convert.ToInt32(extension);
                    pkg.Elements.Refresh();

                }
                else
                {
                    // source is an Element
                    if (elSource.ParentID > 0)
                    {
                        elParent = rep.GetElementByID(elSource.ParentID);
                        elTarget = (Element) elParent.Elements.AddNew(name, basicType);
                        if (basicType == "StateNode") elTarget.Subtype = Convert.ToInt32(extension);
                        elParent.Elements.Refresh();
                    }
                    else
                    {
                        var pkg = rep.GetPackageByID(elSource.PackageID);
                        elTarget = (Element) pkg.Elements.AddNew(name, basicType);
                        if (basicType == "StateNode") elTarget.Subtype = Convert.ToInt32(extension);
                        pkg.Elements.Refresh();
                    }
                }

                elTarget.ParentID = elSource.ParentID;
                elTarget.Update();
                // make a Composite Element which is refined by Activity Diagram
                if (basicType == "Activity" & extension.ToLower() == "comp=yes")
                {
                    Diagram actDia = ActivityPar.CreateActivityCompositeDiagram(rep, elTarget);
                    HoUtil.SetActivityAsWithCompositeDiagram(rep, elTarget, actDia.DiagramID.ToString());
                    //elTarget.
                }
            }
            catch
            {
                return null;
            }

            return elTarget;
        }

        /// <summary>
        /// Draw a connector between two elements
        /// </summary>
        /// <param name="elSource"></param>
        /// <param name="elTarget"></param>
        /// <param name="type">Connector type</param>
        /// <param name="stereotype"></param>
        /// <returns></returns>
        private static EA.Connector DrawConnectorBetweenElements(Element elSource, Element elTarget, string type, string stereotype)
        {
            // check if connector already exists
            foreach (EA.Connector con1 in elSource.Connectors)
            {
                if (con1.SupplierID == elTarget.ElementID && con1.Stereotype == stereotype && con1.Type == type)
                {
                    return con1;
                }
            }
            // draw a Control Flow
            EA.Connector con = (EA.Connector) elSource.Connectors.AddNew("", type);
            con.SupplierID = elTarget.ElementID; 
            con.Stereotype = stereotype;
            con.Update();
            elSource.Connectors.Refresh();
            return con;
        }

        /// <summary>
        /// Draw a connector between two elements (source, target)
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="nodeSource"></param>
        /// <param name="nodeTarget"></param>
        /// <param name="type">Connector type</param>
        /// <param name="stereotype"></param>
        /// <param name="lineStyle"></param>
        /// <returns></returns>
        // ReSharper disable once UnusedMember.Local
        private static EA.Connector DrawConnectorBetweenNodes(Repository rep, 
            DiagramObject nodeSource,
            DiagramObject nodeTarget, 
            string type, 
            string stereotype="", 
            string lineStyle="D")
        {
            Element elSource = rep.GetElementByID(nodeSource.ElementID);
            Element elTarget = rep.GetElementByID(nodeTarget.ElementID);
            Diagram dia = rep.GetDiagramByID(nodeSource.DiagramID);
            EA.Connector con = DrawConnectorBetweenElements(elSource, elTarget, type, stereotype);
            con.SupplierID = elTarget.ElementID;
            con.Update();
            elSource.Connectors.Refresh();
            elTarget.Connectors.Refresh();
            dia.DiagramLinks.Refresh();
            // set line style
            foreach (DiagramLink link in dia.DiagramLinks)
            {
                if (link.ConnectorID == con.ConnectorID)
                {
                    HoUtil.SetLineStyleForDiagramLink(lineStyle, link);
                    break;
                }
            }

            return con;
        }


        #endregion
        #region insertInterface
        /// <summary>
        /// Insert Interfaces/Header files for the selected node:
        /// - It scans the source code to find used or implemented interfaces / header files.
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="dia"></param>
        /// <param name="text"></param>
        public static void InsertInterface(Repository rep, Diagram dia, string text)
        {

            bool isComponent = false;
            Package pkg = rep.GetPackageByID(dia.PackageID);
            int pos = 0;

            // only one diagram object selected as source
            if (dia.SelectedObjects.Count != 1) return;

            // save selected object
            var objSelected = (DiagramObject)dia.SelectedObjects.GetAt(0);


            rep.SaveDiagram(dia.DiagramID);
            var diaObjSource = (DiagramObject)dia.SelectedObjects.GetAt(0);
            var elSource = rep.GetElementByID(diaObjSource.ElementID);
            if (elSource.Type == "Component") isComponent = true;
            // remember selected object

            List<Element> ifList = GetInterfacesFromText(rep, pkg, text);
            foreach (Element elTarget in ifList)
            {
                if (elSource.Locked )
                {
                    MessageBox.Show($@"Source #{elSource.Name}' is locked", @"Element locked");
                    continue;
                }
                if (isComponent)
                {
                    AddNewPortToComponent(elSource, elTarget);
                    
                }
                else
                {
                    AddInterfaceToElement(rep, pos, elSource, elTarget, dia, diaObjSource);

                }
                pos = pos + 1;
            }
            // visualize ports
            if (isComponent)
            {
                dia.SelectedObjects.AddNew(diaObjSource.ElementID.ToString(), ObjectType.otElement.ToString());
                dia.SelectedObjects.Refresh();
                ShowEmbeddedElementsGui(rep);
            }
            else
            {// set linestyle
                
            }

            // reload selected object
            if (objSelected != null)
            {
                dia.SelectedObjects.AddNew(elSource.ElementID.ToString(), elSource.ObjectType.ToString());
                dia.SelectedObjects.Refresh();
            }	
            rep.ReloadDiagram(dia.DiagramID);
            dia.SelectedObjects.AddNew(elSource.ElementID.ToString(), elSource.ObjectType.ToString());
            dia.SelectedObjects.Refresh();

        }
        #endregion
        private static void AddInterfaceToElement(Repository rep, int pos, Element elSource, Element elTarget, Diagram dia, DiagramObject diaObjSource)
        {
            // check if interface already exists on diagram
            var diaObjTarget = dia.GetDiagramObjectByID(elTarget.ElementID, "");
            if (diaObjTarget == null)
            {

                int length = 250;
                //if (elTarget.Type != "Interface") length = 250;
                // calculate target position
                // int left = diaObjSource.right - 75;
                int left = diaObjSource.right ;
                int right = left + length;
                int top = diaObjSource.bottom - 25;

                top = top - 20 - pos * 70;
                var bottom = top - 50;
                string position = "l=" + left + ";r=" + right + ";t=" + top + ";b=" + bottom + ";";




                // create target diagram object
                diaObjTarget = (DiagramObject)dia.DiagramObjects.AddNew(position, "");

                diaObjTarget.ElementID = elTarget.ElementID;
                diaObjTarget.Sequence = 1;
                // supress attributes/operations
                diaObjTarget.Style = "DUID=1263D775;AttPro=0;AttPri=0;AttPub=0;AttPkg=0;AttCustom=0;OpCustom=0;PType=0;RzO=1;OpPro=0;OpPri=0;OpPub=0;OpPkg=0;";
                diaObjTarget.Update();
            }
            // connect source to target by Usage

            // make a connector/ or link if notes
            // check if connector already exists
            EA.Connector con;
            foreach (EA.Connector c in elSource.Connectors) {
                if (c.SupplierID == elTarget.ElementID &
                    ( c.Type == "Usage"  | c.Stereotype == "use" |
                      c.Type == "Realisation")                            ) return;
                    
            }


            if (elTarget.Type.Equals("Interface") )
            {
                 con = (EA.Connector)elSource.Connectors.AddNew("", "Usage");
            } else {
                con = (EA.Connector)elSource.Connectors.AddNew("", "NoteLink");
            }
            con.SupplierID = elTarget.ElementID;
            try
            {
                con.Update();
                elSource.Connectors.Refresh();
                elTarget.Connectors.Refresh();
                // set line style
                dia.DiagramLinks.Refresh();
                //rep.ReloadDiagram(dia.DiagramID);
                foreach (DiagramLink link in dia.DiagramLinks)
                {
                    if (link.ConnectorID == con.ConnectorID)
                    {
                        HoUtil.SetLineStyleForDiagramLink("LV", link);
                    }
                }
            }
            catch (Exception e)
            {
                rep.GetLastError();
                MessageBox.Show(e.ToString(), $@"Error create connector between '{elSource.Name}' and '{ elTarget.Name}' ");
            }
           

        }

        /// <summary>
        /// Add port to source element if not already connected. The type is defined by elInterface
        /// </summary>
        /// <param name="elSource">The element to add port</param>
        /// <param name="elInterface">the interface/type to use for the type</param>
        /// <param name="isRequired"></param>
        private static void AddNewPortToComponent(Element elSource, Element elInterface, bool isRequired=true)
        {
            if (elInterface.Type != "Interface") return;

            // check if port with interface already exists
            foreach (Element p in elSource.EmbeddedElements)
            {
                if (p.Name == elInterface.Name) return;
            }
            // create a port
            var port = (Element)elSource.EmbeddedElements.AddNew(elInterface.Name, "Port");
            elSource.EmbeddedElements.Refresh();
            // add interface
            string embeddedType = isRequired  ? "RequiredInterface" : "ProvidedInterface";
            var interf = (Element)port.EmbeddedElements.AddNew(elInterface.Name, embeddedType);
            // set classifier
            interf.ClassfierID = elInterface.ElementID;
            interf.Update();


          }
        /// <summary>
        /// Get required interfaces from text ('#include..'). If the Interface is missing it creates the interface.
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="pkg"></param>
        /// <param name="s"></param>
        /// <param name="addMissingInterface"></param>
        /// <returns></returns>
        private static List<Element> GetInterfacesFromText(Repository rep, Package pkg, string s, bool addMissingInterface = true)
        {
            List<Element> elList = new List<Element>();
            s = DeleteComment(s);
            // string pattern = @"#include\s*[""<]([^.]*)\.h";
            string patternPath = @"#include\s*[""<]([^"">]*)";

            Match matchPath = Regex.Match(s, patternPath, RegexOptions.Multiline);
            while (matchPath.Success)
            {
                string includePath = matchPath.Groups[1].Value;
                // get includeName
                string includeName = Regex.Match(includePath, @"([\w-]*)\.h").Groups[1].Value;


                Element el = CallOperationAction.GetElementFromName(rep, includeName, "Interface");
                if (el == null && addMissingInterface )
                {
                    // create an interface 
                    el = (Element)pkg.Elements.AddNew(includeName, "Interface");
                    el.Notes = "Interface '" + includeName + "' not available!";
                    el.Update();
                    pkg.Elements.Refresh();


                }
                elList.Add(el);
                matchPath = matchPath.NextMatch();

            }
            // make unique
            return elList
                .GroupBy(x => x.ElementID)
                .Select(g => g.First()).ToList();
        }
        /// <summary>
        /// Generate "Usage" Interface for selected Class/Interface of Diagram node
        /// - Reads the C-Code of Class/Interface
        /// - Search for "#include"
        /// -- #if ( L2C_CFG_ENABLED == L2C_MRES_CFG_TIME_TICK_WD_ENABLE_FLAG )
        /// --   #include "L2C_Mres_TtWd.h"// Usage dependency with stereotype 'L2C_MRES_CFG_TIME_TICK_WD_ENABLE_FLAG'
        /// -- #endif 
        /// - Creates/Reuse existing Interfaces 
        /// - Creates a node of the Interface to connect to
        /// - Make a Usage Connector from Class/Interface to Interface
        /// </summary>
        /// <param name="rep"></param>
        public static void GenerateUseInterfacesFromFile(Repository rep)
        {
            Diagram dia = rep.GetCurrentDiagram();
            if (dia == null) return;

            // prevent information loss
            rep.SaveDiagram(dia.DiagramID);
            HoUtil.SetDiagramStyleFitToPage(dia);// after save diagram!

            // over all selected diagram objects
            foreach (DiagramObject obj in dia.SelectedObjects)
            {

                Element elSource = rep.GetElementByID(obj.ElementID);
                obj.SetStyleEx("Notes", "300");
                obj.Update();

                // Note
                // High: top, bottom from 0 decreasing (0=top)
                // Wide: left, right from 0 increasing (0=left)
                int pos = 0;
                const int nodeWidth = 300;
                const int nodeHigh = 100;
                const int offsetRight = 100;
                const int offsetHigh = 30;

                int left = obj.right+ offsetRight;
                int topStart = obj.top;
                
                // over include files of the selected objects
                foreach (Element ifTarget in GetIncludedHeaderFilesFromCode(rep, elSource))
                {

                    int top = topStart - pos * (offsetHigh + nodeHigh);
                    pos = pos + 1;
                    // If element don't already exists, create Interface Node on Diagram
                    if (dia.GetDiagramObjectByID(ifTarget.ElementID,"")==null)
                    {
                        DiagramObject diaObjTarget = (DiagramObject)dia.DiagramObjects.AddNew($"l={left};r={left+nodeWidth};t={top};b={top-nodeHigh}", "");

                        diaObjTarget.ElementID = ifTarget.ElementID;
                        diaObjTarget.SetStyleEx("AttPro","0");
                        diaObjTarget.SetStyleEx("AttPri", "0");
                        diaObjTarget.SetStyleEx("AttPub", "0");
                        diaObjTarget.SetStyleEx("OpPro", "0");
                        diaObjTarget.SetStyleEx("OpPri", "0");
                        diaObjTarget.SetStyleEx("OpPub", "0");
                        diaObjTarget.SetStyleEx("Notes", "300");
                        diaObjTarget.Update();
                        dia.DiagramObjects.Refresh();

                    }

                    string connectionType = ifTarget.Name == elSource.Name ? @"Realisation" : "Usage";
                    // Skip dependency if already exists
                    // - Name, Connection Type
                    bool skipCreateConnector = false;
                    foreach (EA.Connector c in elSource.Connectors)
                    {
                        if (c.SupplierID == ifTarget.ElementID &&
                            c.Type.Equals(connectionType)) skipCreateConnector = true;
                    }

                    if (!skipCreateConnector)
                    {
                        // Create a "Usage" Dependency from selected Class to create Diagram node with Interface
                        EA.Connector con = (EA.Connector)elSource.Connectors.AddNew("", connectionType);
                        //con.Stereotype = "use";
                        con.SupplierID = ifTarget.ElementID;
                        con.Update();
                        elSource.Connectors.Refresh();
                        ifTarget.Connectors.Refresh();
                    }

                }
            }
            rep.ReloadDiagram(dia.DiagramID);

        }
        /// <summary>
        /// Generate "Usage" Interface for selected Class/Interface of Diagram node from text input
        /// Reads the C-Code definition in in text field
        /// - Creates/Reuse existing Interfaces 
        /// - Creates a node of the Interface to connect to
        /// - Make a Usage Connector from Class/Interface to Interface
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="codeSnippet"></param>
        public static void GenerateUseInterfacesFromInput(Repository rep, string codeSnippet)
        {
            EA.Diagram dia = rep.GetCurrentDiagram();
            if (dia == null) return;

            if (rep.GetContextItemType() != EA.ObjectType.otElement) return;
            EA.Element el = (EA.Element) rep.GetContextObject();
            EA.Package pkg = rep.GetPackageByID(el.PackageID);

            EA.DiagramObject obj = dia.GetDiagramObjectByID(el.ElementID, "");

            // prevent information loss
            rep.SaveDiagram(dia.DiagramID);
            HoUtil.SetDiagramStyleFitToPage(dia);// after save diagram!

                // Update style of selected Element (Interface/Class/..)
                Element elSource = rep.GetElementByID(obj.ElementID);
                obj.SetStyleEx("Notes", "300");
                obj.Update();

                // Note
                // High: top, bottom from 0 decreasing (0=top)
                // Wide: left, right from 0 increasing (0=left)
                int pos = 0;
                const int nodeWidth = 300;
                const int nodeHigh = 100;
                const int offsetRight = 100;
                const int offsetHigh = 30;

                int left = obj.right + offsetRight;
                int topStart = obj.top;

            // over includes of the selected objects
            foreach (Element ifTarget in GetInterfacesFromCodeSnippet(rep, pkg, codeSnippet))
            {

                int top = topStart - pos * (offsetHigh + nodeHigh);
                pos = pos + 1;
                // If element don't already exists, create Interface Node on Diagram
                if (dia.GetDiagramObjectByID(ifTarget.ElementID, "") == null)
                {
                    DiagramObject diaObjTarget =
                        (DiagramObject) dia.DiagramObjects.AddNew(
                            $"l={left};r={left + nodeWidth};t={top};b={top - nodeHigh}", "");

                    diaObjTarget.ElementID = ifTarget.ElementID;
                    diaObjTarget.SetStyleEx("AttPro", "0");
                    diaObjTarget.SetStyleEx("AttPri", "0");
                    diaObjTarget.SetStyleEx("AttPub", "0");
                    diaObjTarget.SetStyleEx("OpPro", "0");
                    diaObjTarget.SetStyleEx("OpPri", "0");
                    diaObjTarget.SetStyleEx("OpPub", "0");
                    diaObjTarget.SetStyleEx("Notes", "300");
                    diaObjTarget.Update();
                    dia.DiagramObjects.Refresh();

                }

                string connectionType = ifTarget.Name == elSource.Name ? @"Realisation" : "Usage";
                // Skip dependency if already exists
                // - Name, Connection Type
                bool skipCreateConnector = false;
                foreach (EA.Connector c in elSource.Connectors)
                {
                    if (c.SupplierID == ifTarget.ElementID &&
                        c.Type.Equals(connectionType)) skipCreateConnector = true;
                }

                if (!skipCreateConnector)
                {
                    // Create a "Usage" Dependency from selected Class to create Diagram node with Interface
                    EA.Connector con = (EA.Connector) elSource.Connectors.AddNew("", connectionType);
                    //con.Stereotype = "use";
                    con.SupplierID = ifTarget.ElementID;
                    con.Update();
                    elSource.Connectors.Refresh();
                    ifTarget.Connectors.Refresh();
                }
            }


            rep.ReloadDiagram(dia.DiagramID);

        }


        /// <summary>
        /// Insert Code/Behavior in Activity Diagram
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="text"></param>
        /// <param name="useCallBehaviorAction"></param>
        public static void InsertInActivtyDiagram(Repository rep, string text, bool useCallBehaviorAction=false)
        {
            
            // remember selected object
            Diagram dia = rep.GetCurrentDiagram();
            if (dia == null) return;
            if (dia.Type != "Activity") return;

            Element elSource = HoUtil.GetElementFromContextObject(rep);
            if (elSource == null) return;
            DiagramObject objSource = null;

            // Switch case was selected as source
            bool isSwitchCaseSelected = false;

            if (Regex.IsMatch(elSource.Name, @"switch[\s]*\("))
            {
                // don't out current line
                isSwitchCaseSelected = true;
                objSource = dia.GetDiagramObjectByID(elSource.ElementID, "");
            }

            int offsetHorizontal = 0;
            int offsetVertical = 0;
            // delete comments /* to */
            string s0 = DeleteComment(text);

            // delete casts
            // (uint8)variable
            Match match = Regex.Match(s0, @"(\([A-Za-z0-9_*& ]*\))[*&_A-Za-z]+", RegexOptions.Multiline);
            while (match.Success)
            {
                s0 = s0.Replace(match.Groups[1].Value, "");
                match = match.NextMatch();
            }



            // concatenate line with variable assignment to a multi line string:
            // 'variable = some lines;'
            // old regex:  @"[^=]*(=[^;{}]*)"
            match = Regex.Match(s0, @"\s*([^=]*)(=[^;]*)", RegexOptions.Singleline);
            while (match.Success)
            {
                if ((match.Groups[1].Value.StartsWith("if",StringComparison.Ordinal)) ||
                   (match.Groups[1].Value.StartsWith("for", StringComparison.Ordinal)) ||
                   (match.Groups[1].Value.StartsWith("while", StringComparison.Ordinal)) ||
                   (match.Groups[1].Value.EndsWith(">", StringComparison.Ordinal)) ||
                   (match.Groups[1].Value.EndsWith("<", StringComparison.Ordinal)) ||
                   (match.Groups[1].Value.EndsWith("!", StringComparison.Ordinal)) ||
                   (match.Groups[2].Value.StartsWith("==", StringComparison.Ordinal)))
                {
                    
                    match = match.NextMatch();
                    continue;
                }
                // variable assignment, add next line by replacing \r\n with ""
                string old = match.Groups[2].Value;
                if (! (match.Value.Contains("#")))
                {

                    //if (Regex.IsMatch(old, @"#[\s]*(if|elseif|else)", RegexOptions.Singleline)) continue;
                    s0 = s0.Replace(match.Groups[2].Value, Regex.Replace(old, "\r\n", ""));
                }
                match = match.NextMatch();
            }

            // concatenate lines nnnn(..);
            match = Regex.Match(s0, @"[A-Za-z0-9_]+[\s]*\([^;}{]*\)", RegexOptions.Singleline);
            while (match.Success)
            {
                if (! (match.Value.Contains("#")))
                {
                //if (Regex.IsMatch(old, @"#[\s]*(if|elseif|else)", RegexOptions.Singleline)) continue;
                    //s0 = s0.Replace(match.Groups[1].Value, Regex.Replace(old, "\r\n", ""));
                    // check if this is no if(..)
                    if (match.Value.StartsWith("if", StringComparison.Ordinal))
                    {
                    }
                    else s0 = s0.Replace(match.Value, Regex.Replace(match.Value, "\r\n", ""));
                }
                match = match.NextMatch();
            }
            // remove empty lines
            s0 = Regex.Replace(s0, @"\r\n\s*\r\n", "\r\n"); 
            
           string[] lines = Regex.Split(s0, "\r\n");

            // first line start with an "else"
            bool skipFirstLine = false;
            // case nnnnn:
            string guardString = "";
            if (lines.Length > 0) {
                string line0 = lines[0].Trim();
                if (line0.StartsWith("else", StringComparison.Ordinal) |
                    Regex.IsMatch(lines[0],"#[ ]*else") )                     
                {
                    
                    offsetHorizontal = 300;
                    guardString = "no";
                    Match matchElseIf = Regex.Match(line0, @"^else[\s]*if");
                    if (matchElseIf.Success)  {
                        offsetHorizontal = 0;
                        //lines[0] = lines[0].Replace(matchElseIf.Value, "");
                    } else {
                    skipFirstLine = true;
                    }
                }
            }
            // to navigate inside lines
            int lineNumber = -1;

            foreach (string s in lines)
            {


                lineNumber += 1;
                var s1 = s.Trim();
                s1 = Cutil.RemoveCasts(s1);

                // skip '#include'
                if (Regex.IsMatch(s1, @"^\s*#\s*include")) continue;

                // skip '# pragma'
                if (Regex.IsMatch(s1, @"^\s*#\s*pragma")) continue;

                //-------------------------------------------------------
                // case xxxxx: or default:
                if (isSwitchCaseSelected & (s1.StartsWith("case", StringComparison.Ordinal) | s1.StartsWith("default", StringComparison.Ordinal)))
                {   // set the selected element to the switch case
                    int length = dia.SelectedObjects.Count;
                    for (int i = length - 1; i >= 0; i--)
                    {
                        dia.SelectedObjects.DeleteAt((short)i, true);
                        dia.SelectedObjects.Refresh();
                    }
                    dia.SelectedObjects.Refresh();
                    
                    //foreach (EA.DiagramObject unused in dia.SelectedObjects)
                    //{
                    //    dia.SelectedObjects.DeleteAt(0, true);
                        
                    //}
                    dia.SelectedObjects.Refresh();
                    dia.SelectedObjects.AddNew(objSource.ElementID.ToString(), objSource.ObjectType.ToString());
                    dia.SelectedObjects.Refresh();
                }
                if (skipFirstLine) {
                    skipFirstLine = false;
                    // check if this was the only line
                    if (lines.Length == 1)
                    {
                        // switch case with only one line
                        if (lines[0].Contains("case"))
                        {
                            CreateActionFromText(rep, s1, offsetHorizontal, offsetVertical, guardString, useCallBehaviorAction:useCallBehaviorAction);
                        }
                    }
                    continue;
                }
                //------------------------------------------------------------------------------------------------------------------

                // check if do, while, for, #if, #else, #endif, #elseif
                if (    Regex.IsMatch(s1, @"^(if|else|else[\s]if|switch[\s]*\()") |
                        Regex.IsMatch(s1, @"#[ ]*(if|endif|else|ifdef|elseif)")  )
                {
                    CreateDecisionFromText(rep, s1, offsetHorizontal, offsetVertical, guardString);
                    offsetHorizontal = 0;
                    guardString = "";
                }
                    
                else
                {
                    if (s1.Length > 0) // Check
                    {
                        if (s1.StartsWith("case", StringComparison.Ordinal) | s1.StartsWith("default", StringComparison.Ordinal))
                        {
                            if (s1.StartsWith("case", StringComparison.Ordinal)) s1 = s1.Substring(4);
                            if (guardString == "")
                            {
                                // make space to move it to the right place
                                offsetHorizontal = 300;
                                offsetVertical = offsetVertical - 10; // room for two actions (40 = one action)
                            }
                            else { guardString = guardString + ", ";}
                            guardString = guardString + s1.Replace(":", "").Trim();
                            
                        }
                        else
                        {
                            // If break: Line before was case/default then insert "nothing to do"
                            if (s1.StartsWith("break;", StringComparison.Ordinal)  & lineNumber > 0)
                            {
                                string s2 = lines[lineNumber - 1].Trim();
                                if (s2.StartsWith("case", StringComparison.Ordinal) | s2.StartsWith("default", StringComparison.Ordinal))
                                    s1 = "nothing to do";
                                else s1 = "";
                            } 
                            if ( !( s1.Equals("")  || s1.Equals("{") || s1.Equals("}") ) )
                                    CreateActionFromText(rep, s1, offsetHorizontal, offsetVertical, guardString, useCallBehaviorAction:useCallBehaviorAction);
                            offsetHorizontal = 0;
                            guardString = "";

                        }
                       
                       
                    }
                }
            }
        }
        private static string DeleteCurleyBrackets(string s)
        {
           s = Regex.Replace(s, @"{[^{}]*}", "", RegexOptions.Multiline);
           s = Regex.Replace(s, @"{[^{}]*}", "", RegexOptions.Multiline);
           s = Regex.Replace(s, @"{[^{}]*}", "", RegexOptions.Multiline);
           s = Regex.Replace(s, @"{[^{}]*}", "", RegexOptions.Multiline);
           s = Regex.Replace(s, @"{[^{}]*}", "", RegexOptions.Multiline);
           s = Regex.Replace(s, @"{[^{}]*}", "", RegexOptions.Multiline);
           s = Regex.Replace(s, @"{[^{}]*}", "", RegexOptions.Multiline);
           s = Regex.Replace(s, @"{[^{}]*}", "", RegexOptions.Multiline);
           s = Regex.Replace(s, @"{[^{}]*}", "", RegexOptions.Multiline);
           return s;
        }
        /// <summary>
        /// Delete comments
        /// - /* */
        /// - //
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string DeleteComment(string s)
        {
            // delete comments /* to */


            // delete comments //....
            s = Regex.Replace(s, @"//[^\n]*\n", "\r\n");
            // ? for greedy behavior (find shortest matching string)
            s = Regex.Replace(s, @"/\*.*?\*/", "", RegexOptions.Singleline);
            // delete comments /*....
            s = Regex.Replace(s, @"/\*[^\n]*\n", "\r\n");
            // delete empty lines
            s = Regex.Replace(s, @"(\r\n){2,100}", "\r\n");

            // delete macros '\' at line end
            s = Regex.Replace(s, @"\\\r\n", "\r\n");

            return s;
        }
        /// <summary>
        /// Delete comment strings:
        /// - '//'
        /// - '/*'
        /// - '*/'
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private static string DeleteCommentStrings(string s)
        {
            return s.Replace("/*", "").Replace(@"*\", "").Replace(@"//", "").Trim();
        }


        #region createActionFromText
        /// <summary>
        /// Create Action from text. If a call action and a method/activity exists it creates a CallOperation- or a CallBehavior Action. If not it creates a simple Action.
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="s1"></param>
        /// <param name="offsetHorizental"></param>
        /// <param name="offsetVertical"></param>
        /// <param name="guardString"></param>
        /// <param name="removeModuleNameFromMethodName"></param>
        /// <param name="useCallBehaviorAction"></param>
        private static void CreateActionFromText(Repository rep, string s1, int offsetHorizental = 0, int offsetVertical = 0, string guardString = "", 
            bool removeModuleNameFromMethodName= false,
            bool useCallBehaviorAction=false)
        {
            // ActionType
            string actionType = useCallBehaviorAction ? @"CallBehavior" : @"CallOperation";

            // check if return
            Match matchReturn = Regex.Match(s1, @"\s*return\s*([^;]*);");
            if (matchReturn.Success)
            {
                string returnValue = "";
                if (matchReturn.Groups.Count == 2) returnValue = matchReturn.Groups[1].Value;
                CreateDiagramObjectFromContext(rep, returnValue, "StateNode", "101", 0,0, "");
            }
                // single "case"==> composite activity  
            else if (s1.Contains("case") ){
                s1 = CallOperationAction.RemoveUnwantedStringsFromText(s1);
                CreateDiagramObjectFromContext(rep, s1, "Activity", "comp=yes", offsetHorizental, offsetVertical, guardString);
            }
            else if (Regex.IsMatch(s1, @"^(for|while|do[\s]*$)"))
            {
                s1 = CallOperationAction.RemoveUnwantedStringsFromText(s1);
                CreateDiagramObjectFromContext(rep, s1, "Activity", "comp=no", offsetHorizental, offsetVertical, guardString);
            }
            else
            {
                string methodString = CallOperationAction.RemoveUnwantedStringsFromText(s1);
                string methodName = CallOperationAction.GetMethodNameFromCallString(methodString);
                // remove module name from method name (text before '_')
                if (removeModuleNameFromMethodName)
                {
                    methodString = CallOperationAction.RemoveModuleNameFromCallString(methodString);
                }

                // check if function is available
                if (methodName != "")
                {
                    if (useCallBehaviorAction)
                    {
                        // use CallBehaviourAction
                        var act = Activity.CreateInDiagramContext(rep, methodName);

                        // use CallOperation Action
                        if (act == null)
                        {
                            CreateDiagramObjectFromContext(rep, methodString, "Action", "", offsetHorizental,
                                offsetVertical, guardString);
                        }
                        else
                        {
                            CreateDiagramObjectFromContext(rep, methodString, actionType, methodName,
                                offsetHorizental, offsetVertical, guardString);
                        }
                    }
                    else
                    {
                        // use CallOperation Action
                        if (CallOperationAction.GetMethodFromMethodName(rep, methodName) == null)
                        {
                            CreateDiagramObjectFromContext(rep, methodString, "Action", "", offsetHorizental,
                                offsetVertical, guardString);
                        }
                        else
                        {
                            CreateDiagramObjectFromContext(rep, methodString, actionType, methodName,
                                offsetHorizental, offsetVertical, guardString);
                        }
                    }
                }
                else
                {   // no Method name, use simple Action (atomic)
                    CreateDiagramObjectFromContext(rep, methodString, "Action", methodName, offsetHorizental, offsetVertical, guardString);
                }
            }
            
        }

        #endregion
        #region createDecisionFromText
        public static string CreateDecisionFromText(Repository rep, string decisionName, int offsetHorizental = 0, int offsetVertical = 0, string guardString = "")
        {
            decisionName = CallOperationAction.RemoveUnwantedStringsFromText(decisionName);
            string loops = "for, while, switch";
            if (!loops.Contains(decisionName.Substring(0, 3)))
            {
                decisionName = CallOperationAction.RemoveFirstParenthesisPairFromString(decisionName);
                decisionName = CallOperationAction.AddQuestionMark(decisionName);
            }
            Match match = Regex.Match(decisionName, @"else[\s]*if");
            if (match.Success)
            {
                decisionName = decisionName.Replace(match.Value, "");
                guardString = "no";
            }
            if (decisionName.StartsWith("if", StringComparison.Ordinal)) {
                decisionName = decisionName.Substring(2).Trim();
            }
            
            if (Regex.IsMatch(decisionName, @"#[ ]*endif") ) {
                CreateDiagramObjectFromContext(rep, decisionName, "MergeNode", "", offsetHorizental, offsetVertical, guardString);
            }else
            {
                CreateDiagramObjectFromContext(rep, decisionName, "Decision", "", offsetHorizental, offsetVertical, guardString);
            }
            return decisionName;
        }
        #endregion
        #region addElementNote
        public static void AddElementNote(Repository rep)
        {
            ObjectType oType = rep.GetContextItemType();
            Element el = HoUtil.GetElementFromContextObject(rep);
            if (el != null)
            {
                Diagram dia = rep.GetCurrentDiagram();
                Package pkg = rep.GetPackageByID(el.PackageID);
                if (dia == null || pkg.IsProtected || dia.IsLocked || el.Locked ) return;

                // save diagram
                rep.SaveDiagram(dia.DiagramID);

                Element elNote;
                try
                {
                    elNote = (Element)pkg.Elements.AddNew("", "Note");
                    elNote.Update();
                    pkg.Update();
                }
                catch { return; }

                // add element to diagram
                // "l=200;r=400;t=200;b=600;"

                DiagramObject diaObj = GetDiagramObjectFromElement(el, dia);
               
                int left = diaObj.right + 50;
                int right = left + 100;
                int top = diaObj.top;
                int bottom = top - 100;

                string position = $"l={left};r={right};t={top};b={bottom};";
                DiagramObject diaObject = (DiagramObject)dia.DiagramObjects.AddNew(position, "");
                dia.Update();
                diaObject.ElementID = elNote.ElementID;
                diaObject.Sequence = 1; // put element to top
                diaObject.Update();
                pkg.Elements.Refresh();
                // make a connector
                EA.Connector con = (EA.Connector)el.Connectors.AddNew("test", "NoteLink");
                con.SupplierID = elNote.ElementID;
                con.Update();
                el.Connectors.Refresh();
                HoUtil.SetElementHasAttachedLink(rep, el, elNote);
                rep.ReloadDiagram(dia.DiagramID);
            } 
            else if (oType.Equals(ObjectType.otConnector))
            {

            }
        }
        #endregion

        /// <summary>
        /// Update Activity types (return, parameter) according to Operation
        /// </summary>
        /// <param name="rep"></param>
        public static void UpdateActivityParameter(Repository rep)
        {
            ObjectType oType = rep.GetContextItemType();
            if (oType.Equals(ObjectType.otElement))
            {

                Element el = (Element)rep.GetContextObject();
                if (el.Type.Equals("Activity"))
                {
                    // get the associated operation
                    Method m = HoUtil.GetOperationFromBrehavior(rep, el);
                    if (el.Locked) return;
                    if (m == null) return;
                    ActivityPar.UpdateParameterFromOperation(rep, el, m);// get parameters from Operation for Activity
                    Diagram dia = rep.GetCurrentDiagram();
                    DiagramObject diaObj = dia?.GetDiagramObjectByID(el.ElementID,"");
                    if (diaObj == null) return;
                    
                    int pos = 0;
                    rep.SaveDiagram(dia.DiagramID);

                    HoUtil.SetDiagramStyleFitToPage(dia);// after save diagram!
                    foreach (Element actPar in el.EmbeddedElements)
                    {
                        if (!actPar.Type.Equals("ActivityParameter")) continue;
                        HoUtil.VisualizePortForDiagramobject(rep, pos, dia, diaObj, actPar, null);
                        pos = pos + 1;
                    }
                    rep.ReloadDiagram(dia.DiagramID);
                }
                if (el.Type.Equals("Class") | el.Type.Equals("Interface"))
                {
                    UpdateActivityParameterForElement(rep, el);
                }
            }
            if (oType.Equals(ObjectType.otMethod))
            {
                Method m = (Method)rep.GetContextObject();
                Element act = Appl.GetBehaviorForOperation(rep, m);
                if (act == null) return;
                if (act.Locked) return;
                ActivityPar.UpdateParameterFromOperation(rep, act, m);// get parameters from Operation for Activity
            }
            if (oType.Equals(ObjectType.otPackage))
            {
                Package pkg = (Package)rep.GetContextObject();
                UpdateActivityParameterForPackage(rep, pkg);
            }
        }

        private static void UpdateActivityParameterForElement(Repository rep, Element el)
        {
            foreach (Method m in el.Methods)
            {
                Element act = Appl.GetBehaviorForOperation(rep, m);
                if (act == null) continue;
                if (act.Locked) continue;
                ActivityPar.UpdateParameterFromOperation(rep, act, m);// get parameters from Operation for Activity
            }
            foreach (Element elSub in el.Elements)
            {
                UpdateActivityParameterForElement(rep, elSub);
            }
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private static bool UpdateActivityParameterForPackage(Repository rep, Package pkg)
        {
            foreach (Element el in pkg.Elements)
            {
                UpdateActivityParameterForElement(rep, el);
            }
            foreach (Package pkgSub in pkg.Packages)
            {
                // update all packages
                UpdateActivityParameterForPackage(rep, pkgSub);
            }
            return true;

        }

        public static void LocateType(Repository rep)
        {
            ObjectType oType = rep.GetContextItemType();
            Element el;
            int id;
            string triggerGuid;
            // connector
            // links to trigger
            switch (oType)
            {

                case ObjectType.otConnector:
                    EA.Connector con = (EA.Connector)rep.GetContextObject();
                    triggerGuid = HoUtil.GetTrigger(rep, con.ConnectorGUID);
                    if (triggerGuid.StartsWith("{", StringComparison.Ordinal) && triggerGuid.EndsWith("}", StringComparison.Ordinal))
                    {
                        Element trigger = rep.GetElementByGuid(triggerGuid);

                        if (trigger != null) rep.ShowInProjectView(trigger);

                    }
                    break;


                case ObjectType.otMethod:
                    Method m = (Method)rep.GetContextObject();
                    if (m.ClassifierID != "")
                    {
                        id = Convert.ToInt32(m.ClassifierID);
                        // get type
                        if (id > 0)
                        {
                            el = rep.GetElementByID(id);
                            if (el != null ) rep.ShowInProjectView(el);
                        }
                    }
                    break;

                case ObjectType.otAttribute:
                    EA.Attribute attr = (EA.Attribute)rep.GetContextObject();
                    id = attr.ClassifierID;
                    // get type
                    if (id > 0)
                    {
                        el = rep.GetElementByID(attr.ClassifierID);
                        if (el != null)
                        {
                            if (el.Type.Equals("Package"))
                            {
                                Package pkg = rep.GetPackageByID(Convert.ToInt32(el.MiscData[0]));
                                rep.ShowInProjectView(pkg);
                            }
                            else
                            {
                                rep.ShowInProjectView(el);
                            }
                        }
                    }
                    break;

                // Locate Diagram (e.g. from Search Window)
                case ObjectType.otDiagram:
                    Diagram d = (Diagram)rep.GetContextObject();
                    rep.ShowInProjectView(d);
                    break;


                case ObjectType.otElement:
                    el = (Element)rep.GetContextObject();
                    if (el.ClassfierID > 0)
                    {
                        el = rep.GetElementByID(el.ClassfierID);
                        rep.ShowInProjectView(el);
                    }
                    else
                    {//MiscData(0) PDATA1,PDATA2,
                        // pdata1 GUID for parts, UmlElement
                        // object_id   for text with Hyperlink to diagram

                        // locate text or frame
                        if (LocateTextOrFrame(rep, el)) return;

                        string guid = el.MiscData[0];
                        if (guid.EndsWith("}", StringComparison.Ordinal))
                        {
                            el = rep.GetElementByGuid(guid);
                            if (el != null) rep.ShowInProjectView(el);
                        }
                        else
                        {
                            if (el.Type.Equals("Action"))
                            {
                                foreach (CustomProperty custproperty in el.CustomProperties)
                                {
                                    if (custproperty.Name.Equals("kind") && custproperty.Value.Contains("AcceptEvent"))
                                    {
                                        // get the trigger
                                        triggerGuid = HoUtil.GetTrigger(rep, el.ElementGUID);
                                        if (triggerGuid.StartsWith("{", StringComparison.Ordinal) && triggerGuid.EndsWith("}", StringComparison.Ordinal))
                                        {
                                            Element trigger = rep.GetElementByGuid(triggerGuid);
                                            if (trigger != null) rep.ShowInProjectView(trigger);
                                            break;
                                        }
                                    }


                                }
                            }
                            if (el.Type.Equals("Trigger"))
                            {
                                // get the signal
                                string signalGuid = HoUtil.GetSignal(rep, el.ElementGUID);
                                if (signalGuid.StartsWith("RefGUID={", StringComparison.Ordinal))
                                {
                                    Element signal = rep.GetElementByGuid(signalGuid.Substring(8, 38));
                                    if (signal != null) rep.ShowInProjectView(signal);
                                }
                            }

                            if (el.Type.Equals("RequiredInterface") || el.Type.Equals("ProvidedInterface"))
                            {
                                rep.ShowInProjectView(el);
                            }

                        }


                    }
                    break;

                case ObjectType.otPackage:
                    Package pkgSrc = (Package)rep.GetContextObject();
                    Package pkgTrg = HoUtil.GetModelDocumentFromPackage(rep, pkgSrc);
                    if (pkgTrg != null) rep.ShowInProjectView(pkgTrg);
                    break;
            }



        }
        /// <summary>
        /// Put Text from input field into Notes, remove comments like '//','/*', '*/'
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="text"></param>
        public static void CreateNoteFromText(Repository rep, string text)
        {
            if (rep.GetContextItemType().Equals(ObjectType.otElement))
            {
                Element el = (Element)rep.GetContextObject();
                string s0 = CallOperationAction.RemoveUnwantedStringsFromText(text.Trim(), false);
                s0 = Regex.Replace(s0, @"\/\*", "//"); // /* ==> //
                s0 = Regex.Replace(s0, @"\*\/", "");   // delete */
                el.Notes = s0;
                //el.Notes = el.Notes + "\r\n Text1 ssssssssssss fffffffffff\r\nText2 ggggggg gggggggggggggggg";

                el.Update();
            }
        }

        public static void GetVcLatestRecursive(Repository rep)
        {
            ObjectType oType = rep.GetContextItemType();
            if (oType.Equals(ObjectType.otPackage) || oType.Equals(ObjectType.otNone))
            {
                // start preparation
                int count = 0;
                int errorCount = 0;
                DateTime startTime = DateTime.Now;

                rep.CreateOutputTab("Debug");
                rep.EnsureOutputVisible("Debug");
                rep.WriteOutput("Debug", "Start GetLatestRecursive", 0);
                Package pkg = (Package)rep.GetContextObject();
                HoUtil.GetLatest(rep, pkg, true, ref count, 0, ref errorCount);
                string s = "";
                if (errorCount > 0) s = " with " + errorCount.ToString() + " errors";

                // finished
                TimeSpan span = DateTime.Now - startTime;
                rep.WriteOutput("Debug", "End GetLatestRecursive in " + span.Hours + ":" + span.Minutes + " hh:mm. " + s, 0);

            }
        }
        public static void CopyGuidSqlToClipboard(Repository rep)
        {
            string str = "";
            string str1;
            ObjectType oType = rep.GetContextItemType();
            Diagram diaCurrent = rep.GetCurrentDiagram();
            EA.Connector conCurrent = null;

            if (diaCurrent != null)
            {
                conCurrent = diaCurrent.SelectedConnector;
            }

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
                return;
            }
            if (oType.Equals(ObjectType.otElement))
            {// Element 
                var el = (Element)rep.GetContextObject();
                string pdata1 = el.MiscData[0];
                string pdata1String;
                if (pdata1.EndsWith("}", StringComparison.Ordinal))
                {
                    pdata1String = "/" + pdata1;
                }
                else
                {
                    pdata1 = "";
                    pdata1String = "";
                }
                string classifier = HoUtil.GetClassifierGuid(rep, el.ElementGUID);
                str = el.ElementGUID + ":" + classifier + pdata1String + " " + el.Name + ' ' + el.Type + "\r\n" +
                 "\r\nSelect ea_guid As CLASSGUID, object_type As CLASSTYPE,* from t_object o where ea_guid = '" + el.ElementGUID + "'";
                if (classifier != "")
                {
                    if (el.Type.Equals("ActionPin"))
                    {
                        str = str + "\r\n Typ:\r\nSelect ea_guid As CLASSGUID, 'Parameter' As CLASSTYPE,* from t_operationparams op where ea_guid = '" + classifier + "'";
                    }
                    else
                    {
                        str = str + "\r\n Typ:\r\nSelect ea_guid As CLASSGUID, object_type As CLASSTYPE,* from t_object o where ea_guid = '" + classifier + "'";
                    }
                }
                if (pdata1 != "")
                {
                    str = str + "\r\n PDATA1:  Select ea_guid As CLASSGUID, object_type As CLASSTYPE,* from t_object o where ea_guid = '" + pdata1 + "'";
                }

                // Look for diagram object
                Diagram curDia = rep.GetCurrentDiagram();
                if (curDia != null)
                {
                    foreach (DiagramObject diaObj in curDia.DiagramObjects)
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

            if (oType.Equals(ObjectType.otDiagram))
            {// Element 
                Diagram dia = (Diagram)rep.GetContextObject();
                str = dia.DiagramGUID + " " + dia.Name + ' ' + dia.Type + "\r\n" +
                       "\r\nSelect ea_guid As CLASSGUID, diagram_type As CLASSTYPE,* from t_diagram dia where ea_guid = '" + dia.DiagramGUID + "'";
            }
            if (oType.Equals(ObjectType.otPackage))
            {// Element 
                Package pkg = (Package)rep.GetContextObject();
                str = pkg.PackageGUID + " " + pkg.Name + ' ' + " Package " + "\r\n" +
                 "\r\nSelect ea_guid As CLASSGUID, 'Package' As CLASSTYPE,* from t_package pkg where ea_guid = '" + pkg.PackageGUID + "'";

            }
            if (oType.Equals(ObjectType.otAttribute))
            {// Element 
                str1 = "LEFT JOIN  t_object typAttr on (attr.Classifier = typAttr.object_id)";
                if (rep.ConnectionString.Contains(".eap"))
                {
                    str1 = "LEFT JOIN  t_object typAttr on (attr.Classifier = Format(typAttr.object_id))";

                }
                EA.Attribute attr = (EA.Attribute)rep.GetContextObject();
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
            if (oType.Equals(ObjectType.otMethod))
            {// Element 
                str1 = "LEFT JOIN t_object parTyp on (par.classifier = parTyp.object_id))";
                var str2 = "LEFT JOIN t_object opTyp on (op.classifier = opTyp.object_id)";
                if (rep.ConnectionString.Contains(".eap"))
                {
                    str1 = " LEFT JOIN t_object parTyp on (par.classifier = Format(parTyp.object_id))) ";
                    str2 = " LEFT JOIN t_object opTyp  on (op.classifier  = Format(opTyp.object_id))";
                }

                Method op = (Method)rep.GetContextObject();
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
        }
        #region createSharedMemoryFromText
        /// <summary>
        /// createSharedMemoryFromText
        /// 
        /// Create: Shared Memory as Class and interface
        ///         - Class Realize shared memory
        ///         - Class has port shared memory
        ///         Tagged values:
        ///         - StartAdress:
        ///         - EndAddress
        ///         
        /// </summary>
        /// <param name="rep">EA.Repository</param>
        /// <param name="txt">string</param>
        // #define SP_SHM_HW_MIC_START     0x40008000u
        // #define SP_SHM_HW_MIC_END       0x400083FFu
        public static void CreateSharedMemoryFromText(Repository rep, string txt) {
            ObjectType oType = rep.GetContextItemType();
            if (! oType.Equals(ObjectType.otPackage)) return;
            Package pkg = (Package)rep.GetContextObject();

            string regexShm = @"#\s*define\sSP_SHM_(.*)_(START|END)\s*(0x[0-9ABCDEF]*)";
            Match matchShm = Regex.Match(txt, regexShm, RegexOptions.Multiline);
            while (matchShm.Success)
            {
                var shm = EaElement.CreateElement(rep, pkg, matchShm.Groups[1].Value, "Class","shm");
                var ishm = EaElement.CreateElement(rep, pkg, "SHM_"+ matchShm.Groups[1].Value, "Interface", "");

                if (matchShm.Groups[2].Value == "START")
                {
                    var shmStartAddr = matchShm.Groups[3].Value;
                    // add Tagged Value "StartAddr"
                    var tagStart = TaggedValue.Add(shm, "StartAddr");
                    tagStart.Value = shmStartAddr;
                    tagStart.Update();

                }else if (matchShm.Groups[2].Value == "END"){
                    var shmEndAddr = matchShm.Groups[3].Value;
                    // add Tagged Value "StartAddr"
                    var tagEnd = TaggedValue.Add(shm, "EndAddr");
                    tagEnd.Value = shmEndAddr;
                    tagEnd.Update();
                }
                // make realize dependency from Interface to shared memory
                bool found = false;
                foreach (EA.Connector c in shm.Connectors)
                {
                    if (c.SupplierID == ishm.ElementID & c.Type == "Realisation") {
                        break;
                    }

                }
                if (found == false)
                {
                    var con = (EA.Connector)shm.Connectors.AddNew("", "Realisation");
                    con.SupplierID = ishm.ElementID;
                    try
                    {
                        con.Update();
                    }
                    catch
                    {
                        rep.GetLastError();
                    }
                    shm.Connectors.Refresh();
        
                }
                // make a port with a provided interface
                EaElement.CreatePortWithInterface(shm, ishm, "ProvidedInterface");



                matchShm = matchShm.NextMatch();

            }
        }
        #endregion
        #region createOperationsFromTextService
        [ServiceOperation("{E56C2722-605A-49BB-84FA-F3782697B6F9}", "Insert Operations in selected Class, Interface, Component", "Insert text with prototype(s)", isTextRequired: false)]
         public static void CreateOperationsFromTextService(Repository rep, string txt, bool makeDuplicateOperations=false) {
         try
            {
                Cursor.Current = Cursors.WaitCursor;
                CreateOperationsAndMacrosFromText(rep, txt, makeDuplicateOperations);
                Cursor.Current = Cursors.Default;
            }
            catch (Exception e11)
            {
                MessageBox.Show(e11.ToString() , @"Error Insert Function");
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
}
        #endregion


        #region SetMacro
        [ServiceOperation("{D92E154D-D792-4BCC-B553-9BC55747FE59}", "Set Macro as Stereotype from Clipboard (max 20 characters)","Select Element, Attribute, Operation", isTextRequired: false)]
        public static void SetMacro(Repository rep)
        {
                string txt = Clipboard.GetText().Trim();
                int lengthStereotype = 40;
                if (String.IsNullOrWhiteSpace(txt) || txt.Length > lengthStereotype || txt.Contains("\n") || txt.Contains("\t"))
                {
                    MessageBox.Show($@"- Stereotype longer than {lengthStereotype} or{Environment.NewLine}- invalid characters like Linefeed, Tabulator{Environment.NewLine}Stereotype ='{txt}'", "Invalid Stereotype");
                    return;
                }
                SetMacro(rep,txt);
               
        }
        public static void SetMacro(Repository rep, string txt)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                if (txt == "") txt = "define";
                SetMacroFromText(rep, txt);
                Cursor.Current = Cursors.Default;
            }
            catch (Exception e11)
            {
                MessageBox.Show(e11.ToString(), @"Error Create Macro from text");
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }
        #endregion

        #region DelMacros
        [ServiceOperation("{41B15F26-EA0B-454A-A8F0-2AC924A84398}", "Del Macros / Stereotype", "Select Element, Attribute, Operation", isTextRequired: false)]
        public static void DelMacro(Repository rep)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                SetMacroFromText(rep, "");
                Cursor.Current = Cursors.Default;
            }
            catch (Exception e11)
            {
                MessageBox.Show(e11.ToString(), @"Error Delete Macros/Stereotypes");
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }
        #endregion
        #region AddMacro
        [ServiceOperation("{41B15F26-EA0B-454A-A8F0-2AC924A84398}", "Add Macros / Stereotype from Clipboard (max. 20 characters)", "Select Element, Attribute, Operation", isTextRequired: false)]
        public static void AddMacro(Repository rep)
        {
                string txt = Clipboard.GetText().Trim();
                if (String.IsNullOrWhiteSpace(txt) || txt.Length > 20) return;
               AddMacro(rep,txt);
       
        }
        public static void AddMacro(Repository rep, string txt)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                AddMacroFromText(rep, txt);
                Cursor.Current = Cursors.Default;
            }
            catch (Exception e11)
            {
                MessageBox.Show(e11.ToString(), @"Error Delete Macros/Stereotypes");
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }
        #endregion

        /// <summary>
        /// Create operations and macros from text. 
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="txt"></param>
        /// <param name="makeDuplicateOperations"></param>

        private static void CreateOperationsAndMacrosFromText(Repository rep, string txt, bool makeDuplicateOperations)
        {
            Diagram dia = rep.GetCurrentDiagram();
            Element el = HoUtil.GetElementFromContextObject(rep);
            if (el == null) return;

            if (dia != null && dia.SelectedObjects.Count != 1)
            {
                dia = null;
            }

            if (dia != null) rep.SaveDiagram(dia.DiagramID);
            // delete comment
            txt = DeleteComment(txt);
            txt = DeleteCurleyBrackets(txt);

            txt = txt.Replace(";", " ");
            // delete macros
            txt = Regex.Replace(txt, @"^[\s]*#[^\n]*\n", "", RegexOptions.Multiline);

            string[] lTxt = Regex.Split(txt, @"\)[\s]*\r\n");
            for (int i = 0; i < lTxt.Length; i++)
            {
                txt = lTxt[i].Trim();
                if (!txt.Equals(""))
                {
                    if (!txt.EndsWith(")", StringComparison.Ordinal)) txt = txt + ")";
                    // Macro function
                    // #define aaaa(
                    if (Regex.IsMatch(txt, @"#\s*define\s+\w+\s*\("))
                        CreateMacroOperationFromText(rep, el, txt);
                    else
                        CreateOperationFromText(rep, el, txt, makeDuplicateOperations);

                }
            }
            if (dia != null)
            {
                rep.ReloadDiagram(dia.DiagramID);
                dia.SelectedObjects.AddNew(el.ElementID.ToString(), el.ObjectType.ToString());
                dia.SelectedObjects.Refresh();
            }



    }
        /// <summary>
        /// Create operation for Macro from text
        /// #define name(par1,par2,par3).....
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="el"></param>
        /// <param name="txt"></param>
        private static void CreateMacroOperationFromText(Repository rep, Element el, string txt)
        {
            // delete comment
            txt = DeleteComment(txt);
            // get function name 'name(par1,par2,par3)'
            string regex = @"(\w+)\s*\(([^\)]*)\)";
            Match match = Regex.Match(txt, regex);
            if (! match.Success)
            {
                MessageBox.Show(txt, $@"No macro function definition ('#define name( )') in line{Environment.NewLine}'{txt}'");
                return;
            }
            string functionName = match.Groups[1].Value;
            string parameters = match.Groups[2].Value;


            // create function if not exists, else update function
            Method m = null;
            foreach (Method m1 in el.Methods)
            {
                if (m1.Name == functionName )
                {
                    m = m1;
                    // delete all parameters
                    for (short i = (short)(m.Parameters.Count - 1); i >= 0; i--)
                    {
                        m.Parameters.Delete(i);
                        m.Parameters.Refresh();
                    }
                }
            }

            if (m == null)
            {
                m = (Method)el.Methods.AddNew(functionName, "");
                m.Pos = el.Methods.Count + 1;
                el.Methods.Refresh();
            }
            // static
            m.IsStatic = true;
            m.Visibility = "public";

            // handle 'extern' as stereotype
            m.StereotypeEx = "define";
            m.Update();

            el.Methods.Refresh();
            string[] lpar = parameters.Split(',');


            // process parameters
            int pos = 1;
            foreach (string para in lpar)
            {
                string par = para.Trim();
                if (par == "") continue;

                string name = par;
                string type = par;

 
                Parameter elPar = (Parameter)m.Parameters.AddNew(name, "");
                m.Parameters.Refresh();
                elPar.IsConst = false;
                elPar.Type = type;
                elPar.Kind = "in";
                elPar.Position = pos;
                // $@"Error creating parameter:{type} {name}" );
                try
                {
                    elPar.Update();
                }
                catch (Exception e)
                {
                    MessageBox.Show($@"{e}

{elPar.GetLastError()}", $@"Error creating parameter:'{type}' '{name}'");
                }   
                pos = pos + 1;

            }

        }

        /// <summary>
        /// Create operation from text
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="el"></param>
        /// <param name="txt"></param>
        /// <param name="makeDuplicateOperations"></param>
        private static void CreateOperationFromText(Repository rep, Element el, 
                                string txt, bool makeDuplicateOperations)
        {

            Method m = null;
            string functionName;
            string parameters = "";
            string functionType = "";
            int typeClassifier = 0;
            bool isStatic = false;
            // delete comment
            var leftover = DeleteComment(txt);



            // get function name
            // 1. void name(...)
            // 2. name(..)  // z.b. Task L2B_TASK ( L2B_SchM_Cyclic10ms );
            string regex = @"[\s]*([a-zA-Z0-9_*]*)[\s]*\(";
            Match match = Regex.Match(leftover, regex, RegexOptions.Multiline);
            if (match.Success)
            {
                functionName = match.Groups[1].Value;
                leftover = leftover.Remove(match.Groups[1].Index, match.Groups[1].Length); // delete name
            }
            else
            {
                MessageBox.Show(txt, $@"No function definition in line{Environment.NewLine}'{txt}'");
                return;
            }
            // get parameters
            regex = @"\(([^\)]*)\)";
            match = Regex.Match(leftover, regex, RegexOptions.Multiline);
            if (match.Success)
            {
                parameters = match.Groups[1].Value;
                leftover = leftover.Remove(match.Index, match.Length); // delete name
                //leftover = leftover.Replace(match.Value, "").Trim();
            }
            if (leftover.ToLower().Contains("static "))
            {
                isStatic = true;
                leftover = leftover.Replace("static ", "").Trim();
                leftover = leftover.Replace("STATIC ", "").Trim();
            }

            // get type
            string pointer = "";
            if (leftover.Contains("*"))
            {
                pointer = "*";
                leftover = leftover.Replace("*","").Trim();
            }
            leftover = leftover.Trim();

            // handle extern function
            string externAttribute = "";
            if (leftover.Contains("extern"))
            {
                externAttribute = "extern";
                leftover = leftover.Replace("extern", "").Trim();
            }



            regex = @"[a-zA-Z_0-9*]*$";
            match = Regex.Match(leftover, regex, RegexOptions.Multiline);
            if (match.Success)
            {
                functionType = match.Value + pointer;
                // get classifierID of type
                typeClassifier = HoUtil.GetTypeFromName(rep, ref functionName, ref functionType);
                if (typeClassifier == 0 & functionType.Contains("*"))
                {
                    functionType = functionType.Remove(functionName.IndexOf("*", StringComparison.Ordinal), 1);
                    functionName = "*" + functionName;
                    typeClassifier = HoUtil.GetTypeId(rep, functionType);
                    if (typeClassifier == 0 & functionType.Contains("*"))
                    {
                        functionType = functionType.Replace("*", "");
                        functionName = "*" + functionName;
                        typeClassifier = HoUtil.GetTypeId(rep, functionType);
                    }
                }
            }
            // create function if not exists, else update function
            bool isNewFunctions = true;

            // Make multiple operations with the same name
            if (! makeDuplicateOperations)
            foreach (Method m1 in el.Methods) {
                if (m1.Name == functionName && StereotypeExists(m1.StereotypeEx, externAttribute))
                {
                    isNewFunctions = false;
                    m = m1;
                    // delete all parameters
                    for (short i = (short)(m.Parameters.Count-1); i >=0; i--)
                    {
                        m.Parameters.Delete(i);
                        m.Parameters.Refresh();
                    }
                }
            }

            if (isNewFunctions)
            {
                m = (Method)el.Methods.AddNew(functionName, "");
                m.Pos = el.Methods.Count + 1;
                el.Methods.Refresh();
            }
            m.ReturnType = functionType;
            m.ClassifierID = typeClassifier.ToString();
            // static
            m.IsStatic = isStatic;
            m.Visibility = el.Type.Equals("Interface") ? "public" : "private";

            // handle 'extern' as stereotype
            m.StereotypeEx = StereotypeAdd(m.StereotypeEx, externAttribute);
            m.Update();

 
            el.Methods.Refresh();
            string[] lpar = parameters.Split(',');

            
            
            int pos = 1;
            foreach (string para in lpar)
            {
                string par = para.Trim();
                if (par == "void" | par == "") continue;
                bool isConst = false;
                if (par.Contains("const"))
                {
                    isConst = true;
                    par = par.Replace("const", "");
                }
                pointer = "";
                if (par.IndexOf("*", StringComparison.Ordinal) > -1)
                {
                    pointer = "*";
                    par = par.Remove(par.IndexOf("*", StringComparison.Ordinal),1);
                    if (par.IndexOf("*", StringComparison.Ordinal) > -1)
                    {
                        pointer = "**";
                        par = par.Remove(par.IndexOf("*", StringComparison.Ordinal),1);
                    }
                }


                // Handling parameter (only name or type+ name as a comma separated list)
                par = Regex.Replace(par.Trim(),@"[\s]+", " ");
                string[] lparElements = par.Split(' ');
                if (lparElements.Length > 2)
                {
                    MessageBox.Show(txt, @"Can't evaluate parameters, possible more than 'type name'");
                    return;

                }
                string name = lparElements[lparElements.Length - 1];
                // parameter consists of two parts: type name
                Parameter elPar = (Parameter)m.Parameters.AddNew(name, "");
                m.Parameters.Refresh();
                elPar.IsConst = isConst;
                elPar.Kind = "in";
                elPar.Position = pos;
                string type = "";
                // add type to parameter name
                if (lparElements.Length == 2)
                {
                    type = lparElements[lparElements.Length - 2] + pointer;

                    // get classifier ID
                    var classifierId = HoUtil.GetTypeFromName(rep, ref name, ref type);
                    elPar.ClassifierID = classifierId.ToString();

                }
               try
                {
                    elPar.Type = type;
                    elPar.Update();
                }
                catch (Exception e)
                {
                    MessageBox.Show($@"{e}

{elPar.GetLastError()}", $@"Error creating parameter: {type} {name}");
                }
                pos = pos + 1;

            }
        }
        #region createTypeDefStructFromTextService

        /// <summary>
        /// Creates typedef: ENUM, Struct, Union
        /// </summary>
        /// <param name="rep"></param>
        [ServiceOperation("{6784026E-1B54-47CA-898F-A49EEB8A6ECB}",
            "Create/Update typedef for struct from Clipboard, union or enum from C-text for selected Class/Interface/Component",
            "Insert text with typedef\nSelect Class to generate it beneath class\nSelect typedef to update it",
            isTextRequired: false)]
        public static void CreateTypeDefStructFromTextService(Repository rep)
        {
            string txt = Clipboard.GetText();
            if (String.IsNullOrWhiteSpace(txt)) return;
            CreateTypeDefStructFromTextService(rep, txt);
        }
        /// <summary>
        /// Create/update typedef struct, enum from code. You have to select: The type to update or the element to create a new element below.
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="txt"></param>
        public static void CreateTypeDefStructFromTextService(Repository rep, string txt)
        {
            try
            {
                
                Cursor.Current = Cursors.WaitCursor;
                rep.EnableUIUpdates = false;
                rep.BatchAppend = true;
                Diagram dia = rep.GetCurrentDiagram();
                if (dia == null) return;

                Package pkg = rep.GetPackageByID(dia.PackageID);
                if (dia.SelectedObjects.Count != 1) return;

                var el = HoUtil.GetElementFromContextObject(rep);

                // delete comment
                txt = DeleteComment(txt);

                // delete macros '#.....'
                txt = Regex.Replace(txt, @"^[\s]*#[^\n]*\n", "", RegexOptions.Multiline);

                MatchCollection matches = Regex.Matches(txt, @".*?}[\s]*[A-Za-z0-9_]*[\s]*;", RegexOptions.Singleline);
                foreach (Match match in matches)
                {
                    CreateTypeDefStructFromText(rep, dia, pkg, el,match.Value);

                }
                rep.BatchAppend = false;
                rep.EnableUIUpdates = true;
                // update Model view and open diagrams
                rep.RefreshModelView(dia.PackageID);
                rep.RefreshOpenDiagrams(true);

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
        #endregion

        /// <summary>
        /// Create a typedef struct, union, enum below a class/interface from text (used for typedefs)
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="dia"></param>
        /// <param name="pkg"></param>
        /// <param name="el"></param>
        /// <param name="txt"></param>
        /// <param name="deleteAttribute"></param>
        private static void CreateTypeDefStructFromText(Repository rep, Diagram dia, Package pkg, Element el,
            string txt, 
            bool deleteAttribute=true)
        {
            Element elTypedef = null;
           

            // delete comment
            txt = DeleteComment(txt);

            bool update = false;
            bool isStruct = false;
            bool isUnion = false;
            bool isEnum = false;
            string elType = "Class";
            string stereotype = "";

            // find start: enum|struct|union
            string regex = @"[\s]*typedef[\s]+(struct|union|enum)[\s]*([^{]*){";
            Match match = Regex.Match(txt, regex);
            if (!match.Success) return;
            switch (match.Groups[1].Value)
            {
                case "enum":
                    isEnum = true;
                    elType = "Enumeration";
                    break;
                case "struct":
                    isStruct = true;
                    stereotype = "struct";
                    break;
                case "union":
                    isUnion = true;
                    stereotype = "union";
                    break;
            }

            txt = txt.Replace(match.Value, "");

            // find name
            regex = @"}[\s]*([^;]*);";
            match = Regex.Match(txt, regex);
            if (!match.Success) return;
            string name = match.Groups[1].Value.Trim();
            if (name == "") return;
            txt = txt.Remove(match.Index, match.Length); 
            
             // check if typedef already exists
            int targetId = HoUtil.GetTypeId(rep,name);
            if (targetId != 0)
            {
                elTypedef = rep.GetElementByID(targetId);
                update = true;
                for (int i = elTypedef.Attributes.Count - 1; i > -1; i = i - 1)
                {
                    elTypedef.Attributes.DeleteAt((short)i, true);
                }
                
            }


            // create typedef
            if (update == false) 
            {
                if (el != null ) { // create class below element
                   if ("Interface Class Component".Contains(el.Type))
                    {
                        elTypedef = (Element)el.Elements.AddNew(name, elType);
                        el.Elements.Refresh();
                    }
                    else
                    {
                        MessageBox.Show($@"Can't create element '{name}' below selected Element '{el.Name}'");
                    }
               
                }
                 else // create class in package
                { 
                    elTypedef = (Element)pkg.Elements.AddNew(name, elType);
                    pkg.Elements.Refresh();

                }
            }
            if (elTypedef == null) return;
            if (isStruct|isUnion)
            {
                elTypedef.Stereotype = stereotype;
                EA.TaggedValue tag = TaggedValue.Add(elTypedef, "typedef");
                tag.Value = "true";
                tag.Update();
            }
            if (update == false)
            {
                elTypedef.Name = name;
                elTypedef.Update();
            }

            // Delete existing attributes if update
            if (deleteAttribute && update)
            {
                // ReSharper disable once PossibleNullReferenceException
                for (int i = elTypedef.Attributes.Count - 1; i > -1; i = i - 1)
                {
                    elTypedef.Attributes.DeleteAt((short)i, true);
                }
                elTypedef.Attributes.Refresh();
            }

            // add elements
            if (isStruct || isUnion) CreateClassAttributesFromText(rep, elTypedef, txt);
            if (isEnum) CreateEnumerationAttributesFromText(rep, elTypedef, txt);

            if (update)
            {
                rep.RefreshModelView(elTypedef.PackageID);
                rep.ShowInProjectView(elTypedef);
                
            }
            else
            {
                // put diagram object on diagram
                int left = 0;
                int right = left + 200;
                int top = 0;
                int bottom = top + 100;
                //int right = diaObj.right + 2 * (diaObj.right - diaObj.left);
                rep.SaveDiagram(dia.DiagramID);
                string position = "l=" + left + ";r=" + right + ";t=" + top + ";b=" + bottom + ";";
                DiagramObject diaObj = (DiagramObject)dia.DiagramObjects.AddNew(position, "");
                dia.DiagramObjects.Refresh();
                diaObj.ElementID = elTypedef.ElementID;
                diaObj.Update();
            }
            rep.ReloadDiagram(dia.DiagramID);
            dia.SelectedObjects.AddNew(elTypedef.ElementID.ToString(), elTypedef.ObjectType.ToString());
            dia.SelectedObjects.Refresh();
            
                
        }
        #region insertAttributeService

        [ServiceOperation("{BE4759E5-2E8D-454D-83F7-94AA2FF3D50A}",
            "Insert/Update Attributes in Class from Clipboard, Interface, Component",
            "Select Class, Interface or enum", isTextRequired: false)]
        public static void InsertAttributeService(Repository rep, string txt)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                CreateAttributesFromText(rep, txt);
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
        #endregion
        /// <summary>
        /// Create for the selected element from text:
        /// - Class/Interface: Attributes
        /// - Enum: Enumeration values
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="txt"></param>
        private static void CreateAttributesFromText(Repository rep, string txt)
        {

            Element el = HoUtil.GetElementFromContextObject(rep);
            if (el == null) return;

            // remember selected object
            DiagramObject objSelected = null;
            Diagram dia = rep.GetCurrentDiagram();
            if (!(dia != null && dia.SelectedObjects.Count > 0 ))
            {
                objSelected = (DiagramObject)dia.SelectedObjects.GetAt(0);
            }
            
            // update Attribute
            if (el.Type.Equals("Class") | el.Type.Equals("Interface")) CreateClassAttributesFromText(rep, el, txt);
            // update enum
            if (el.Type.Equals("Enumeration")) CreateEnumerationAttributesFromText(rep, el, txt);


            // select the previously selected element
            if (objSelected != null)
            {
                el = rep.GetElementByID(objSelected.ElementID);
                dia.SelectedObjects.AddNew(el.ElementID.ToString(), el.ObjectType.ToString());
                dia.SelectedObjects.Refresh();
            }
        }
        /// <summary>
        /// Create Class/Interface Attributes for element.
        /// No macros like #if #else are considered
        /// #define leads to 
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="el">Selected Element</param>
        /// <param name="txt">String with the attribute definitions</param>
        private static void CreateClassAttributesFromText(Repository rep, Element el, string txt)
        {
            
            
            // delete comment
            txt = DeleteComment(txt);
            txt = txt.Replace(" __far ", " ");

            // delete macro's like #if, #else, #end,..
            //txt = Regex.Replace(txt, @"^[\s]*(#[\s]*if|#[\s]*else|#[\s]*end)[^\n]*\n", "", RegexOptions.Multiline);


            // remove all \r\n
            //txt = Regex.Replace(txt, @"(\r|\n)", "");

            // get all lines
            string[] lines = Regex.Split(txt, "\r\n");
            //string[] lines = Regex.Split(txt, ";");
            string macro = "";
            string part1 = "";
            foreach (string s in lines)
            {


                string sRaw = part1 + " " + s.Trim();
                if (sRaw == "") continue;
                //sRaw = sRaw + ";";
                bool isConst = false;
                bool isStatic = false;
                string pointerValue = "";
                // remove everything behind ";"
                var indexOf = sRaw.IndexOf(";", StringComparison.Ordinal);
                if (indexOf >=0 & indexOf >= sRaw.Length) sRaw = sRaw.Substring(indexOf+1);


                // remove everything behind "//"
                sRaw = Regex.Replace(sRaw, @"//.*", "");
                // remove everything behind "/*"
                sRaw = Regex.Replace(sRaw, @"/\*.*", "");
                sRaw = sRaw.Trim();
                if (sRaw == "") continue;

                // handling macros
                if (GetIfEndifMacro(sRaw, ref macro)) continue;
                if (GetIfMacro(sRaw, ref macro)) continue;
                if (GetIfElseMacro(sRaw, ref macro)) continue;



                sRaw = CallOperationAction.RemoveCasts(sRaw);

                

                if (sRaw.Equals("")) continue;
                if (sRaw.Contains("#") && sRaw.Contains("define") )
                {
                    CreateMacroFromText(rep, el, sRaw, macro);
                    continue;
                }
               
                //-----------------------------------------------------------------------------
                // attributes
                // remove macros
                sRaw = Regex.Replace(sRaw, @"[\s]*#[^$]*", "");

                string name;
                string prefix = "";
                string defaultValue = "";
                string collectionValue = "";
                EA.Attribute a = null;
                // Attributes over more than one line
                if (sRaw.Contains(";") || sRaw.Contains("MODULE_NAME("))
                {
                   part1 = "";
                }
                else
                {
                    part1 = part1 + " " + sRaw;
                    continue;
                }

                // Module-Name
                string regexModuleName = @"(MODULE_NAME)(\([^)]*\))";
                Match match = Regex.Match(sRaw, regexModuleName);
                if (match.Success)
                {
                    name = match.Groups[1].Value;
                    defaultValue = match.Groups[2].Value;
                    a = (EA.Attribute)el.Attributes.AddNew(name + defaultValue, "");
                    a.IsConst = isConst;
                    a.Visibility = el.Type.Equals("Class") ? "Private" : "Public";

                    a.Pos = el.Attributes.Count + 1;
                    if (prefix.ToLower().Contains("static")) a.IsStatic = true;
                    a.Update();
                    continue;
                }
                
                indexOf = sRaw.IndexOf("*", StringComparison.Ordinal);
                if (indexOf > -1)
                {
                    sRaw = sRaw.Remove(indexOf, 1);
                    pointerValue = "*";
                }
                indexOf = sRaw.IndexOf("*", StringComparison.Ordinal);
                if (indexOf > -1)
                {
                    sRaw = sRaw.Remove(indexOf, 1);
                    pointerValue = "**";
                }

                if (sRaw.Contains("const")) isConst = true;
                if (sRaw.ToLower().Contains("static")) isStatic = true;


                string sCompact = sRaw.Replace("*", "");
                sCompact = sCompact.Replace("const", "");
                sCompact = sCompact.Replace("static ", "");
                sCompact = sCompact.Replace("STATIC ", "");

                string externAttribute = "";
                if (sCompact.Contains("extern "))
                {
                    sCompact = sCompact.Replace("extern ", "");
                    externAttribute = "extern";
                }

                // get name


                // get default value
                string regexDefault = @"=[\s]*([\(\)a-zA-Z0-9_ *+-{}\%]*)";
                match = Regex.Match(sCompact, regexDefault);
                if (match.Success)
                {
                    defaultValue = match.Groups[1].Value.Trim();
                    sCompact = sCompact.Remove(match.Groups[1].Index, match.Groups[1].Length); 
                    sCompact = sCompact.Replace("=", "");
                    if (!sCompact.EndsWith(";", StringComparison.Ordinal)) sCompact = sCompact + ";";
                }
                

                // get array
                //string regexArray = @"(\[[^;]*);";
                string regexArray = @"(\[[()A-Za-z0-9\]\[_ +-]*)";
                match = Regex.Match(sCompact, regexArray);
                if (match.Success)
                {
                    collectionValue = match.Groups[1].Value.Trim();
                    sCompact = sCompact.Remove(match.Groups[1].Index, match.Groups[1].Length); 
                    if (!sCompact.EndsWith(";", StringComparison.Ordinal)) sCompact = sCompact + ";";
                }

                // Format of string
                // type0 type1 type2 name:xx;
                sCompact = sCompact.Replace("  ", " ");
                sCompact = sCompact.Replace("  ", " ");
                sCompact = sCompact.Replace("  ", " ");
                string regexName = @"(.*)[\s]+([&a-zA-Z_0-9:]+)[\s]*[\[;]";
                match = Regex.Match(sCompact, regexName);
                if (match.Success)
                {
                    name = match.Groups[2].Value.Trim();
                    var type = match.Groups[1].Value.Trim();
                    sCompact = sCompact.Remove(match.Groups[2].Index, match.Groups[2].Length); // delete name
                    sCompact = sCompact.Remove(match.Groups[1].Index, match.Groups[1].Length); // delete type
                    // ReSharper disable once RedundantAssignment
                    sCompact = sCompact.Replace(";", ""); // ;

                    // add attribute if new
                    string aName = pointerValue + name;
                    aName = aName.Trim();
                    bool isNewAttribute = true;
                    foreach (EA.Attribute attr in el.Attributes)
                    {
                        if (attr.Name == aName && StereotypeExists(attr.StereotypeEx, macro) && StereotypeExists(attr.StereotypeEx, externAttribute))
                        {
                            a = attr;
                            isNewAttribute = false;
                            break;
                        }
                    }

                    if (isNewAttribute)
                    {
                        a = (EA.Attribute)el.Attributes.AddNew(aName, "");
                        if (a.Name.Length > 255)
                        {
                            MessageBox.Show(a.Name + @" has " + a.Name.Length, @"Name longer than 255");
                            continue;
                        }
                        a.Pos = el.Attributes.Count;
                        a.StereotypeEx = StereotypeAdd(a.StereotypeEx, macro);
                        a.StereotypeEx = StereotypeAdd(a.StereotypeEx, externAttribute);
                        el.Attributes.Refresh();
                    }

                    a.Type = type;
                    a.IsConst = isConst;
                    a.Default = defaultValue;
                    a.ClassifierID = HoUtil.GetTypeId(rep, type);
                    a.Visibility = el.Type.Equals("Class") ? "Private" : "Public";
                    if (!collectionValue.Equals(""))
                    {
                        a.IsCollection = true;
                        a.Container = collectionValue;
                        if (collectionValue.Length > 50)
                        {
                            MessageBox.Show($@"Collection '{collectionValue}' has {collectionValue.Length} characters.", @"Break! Collection length need to be <=50 characters");
                            continue;
                        }
                    }
                    a.IsStatic = isStatic;
                    try
                    {
                        a.Update();
                        el.Attributes.Refresh();
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.ToString(), @"Error updating attribute");
                    }
                }
                else
                {
                    MessageBox.Show($@"String:
'{s}'
Interpret:'{sCompact}'
Regex:'{regexName}'", @"Couldn't understand attribute syntax");
                }
            }

        }
        /// <summary>
        /// Get #endif macro
        /// </summary>
        /// <param name="sRaw"></param>
        /// <param name="macro"></param>
        /// <returns>True if '#endif' handled, False if no '#endif'</returns>
        private static bool GetIfEndifMacro(string sRaw, ref string macro)
        {
            if (sRaw.StartsWith(@"#endif", StringComparison.Ordinal))
            {
                macro = "";
                return true;
            }
            return false;
        }
        /// <summary>
        /// Get '#else' macro which is in fact the #if macro with the extension '#' (unequal). 
        /// </summary>
        /// <param name="sRaw">raw string</param>
        /// <param name="macro">Reference to found macro with extension '#'</param>
        /// <returns>True if '#else' handled, False if no '#else'</returns>
        private static bool GetIfElseMacro(string sRaw, ref string macro)
        {
            if (sRaw.StartsWith(@"#else", StringComparison.Ordinal))
            {
                macro = $"{macro}#";
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get '#if' macro. 
        /// #if ( L2C_CFG_ENABLED == L2C_OS_CFG_QM_T1_ENABLE_FLAG ) 
        /// gets 'L2C_OS_CFG_QM_T1_ENABLE_FLAG' 
        /// as macro
        /// </summary>
        /// <param name="sRaw">raw string</param>
        /// <param name="macro">Reference to found macro</param>
        /// <returns>True if '#if' handled, False if no '#if'</returns>
        private static bool GetIfMacro(string sRaw, ref string macro)
        {
            if (sRaw.StartsWith(@"#if", StringComparison.Ordinal))
            {
                // #if ( L2C_CFG_ENABLED == L2C_OS_CFG_QM_T1_ENABLE_FLAG )
                Regex rgx = new Regex(@"#if\s+\(\s*(\w+)\s*==\s*(\w*)");
                Match matchIf = rgx.Match(sRaw);
                if (matchIf.Success)
                {
                    macro = matchIf.Groups[2].Value;
                    return true;
                }
                else
                {
                    macro = "";
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Insert Attribute for a C-Macro line like '#define name value'
        /// It will create an Attribute with a stereotype 'Stereotype=define'
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="el"></param>
        /// <param name="sMacroDefinition">Line with macro definition</param>
        /// <param name="stereotype">The stereotype to set or 'define' as default or when blank</param>
        // ReSharper disable once UnusedParameter.Local
        private static void CreateMacroFromText(Repository rep, Element el, string sMacroDefinition, string stereotype="define") {

             string name = "";
             string value = "";
             EA.Attribute a = null;
             bool isNewAttribute = true;

             // delete spaces between parameters
            sMacroDefinition = Regex.Replace(sMacroDefinition, @",[\s]+",",");
            sMacroDefinition = Regex.Replace(sMacroDefinition, @"[\s]+,", ",");

             string regexDefine = @"#[\s]*define[\s]*([a-zA-Z0-9_(),]*)[\s]*(.*)";
                Match match = Regex.Match(sMacroDefinition, regexDefine);
                if (match.Success)
                {
                    name = match.Groups[1].Value.Trim();
                    value = match.Groups[2].Value.Trim();
                }

             if ( ! name.Equals("")) 
             {
                 value = CallOperationAction.RemoveCasts(value);
                 // don't assign an attribute two times
                 // Attribute name + stereotype="define"
                 foreach (EA.Attribute attr in el.Attributes)
                 {
                     if (attr.Name == name && StereotypeExists(attr.StereotypeEx, "define"))
                     {
                         a = attr;
                         isNewAttribute = false;
                         break;
                     }
                 }
                 if (isNewAttribute)
                 {
                     a = (EA.Attribute)el.Attributes.AddNew(name, "");
                     a.Pos = el.Attributes.Count;
                     el.Attributes.Refresh();
                 }
                 a.Default = value;
                 a.Visibility = el.Type.Equals("Interface") ? "public" : "private";
             
                 a.IsConst = true;
                 a.Stereotype = ! stereotype.Equals("") ? stereotype : "define";
                 a.ClassifierID = 0;
                 a.Type = "";
                 a.Update();
             } else {
                 MessageBox.Show(sMacroDefinition,@"Can't identify macro name");
             }

        }
        public static string CreateStereotype(Repository rep, Element el, string s) {

            string stereotype = "";
            if (s.Contains("TARGET_CPU_1")) stereotype = "SOFTWARE_VALID_FOR_CPU_1";
            if (s.Contains("TARGET_CPU_2")) stereotype = "SOFTWARE_VALID_FOR_CPU_2";
            if (stereotype == "") {
                string regexStereotype = @"if[\s]+([^]*)";
                Match match = Regex.Match(s, regexStereotype);
                if (match.Success)
                {
                    stereotype = match.Groups[1].Value.Trim();
                }
            }
            return stereotype;
        }

        // ReSharper disable once UnusedParameter.Local
        private static void CreateEnumerationAttributesFromText(Repository rep, Element el, string txt)
        {
            // delete comment
            txt = DeleteComment(txt);
            // delete casts
            txt = Cutil.RemoveCasts(txt);



            // check for (with or without comma):
            // abc = 5 ,           or
            // abc  ,
            string regexEnum = @"([a-zA-Z_0-9]+)[\s]*(=[\s]*([a-zA-Z_0-9\)\(| ]+)|,|$)";
            Match match = Regex.Match(txt, regexEnum, RegexOptions.Multiline);
            int pos = 0;
            while (match.Success)
            {
                EA.Attribute a = (EA.Attribute)el.Attributes.AddNew(match.Groups[1].Value, "");
                // with/without default value
                if (match.Groups[2].Value != ",")
                {
                    // remove leading, trailing brackets, blanks
                    a.Default = match.Groups[3].Value.Trim().TrimStart('(').TrimEnd(')').Trim();

                }
                a.Stereotype = "enum";
                a.Pos = pos;
                a.Update();
                el.Attributes.Refresh();
                pos = pos + 1;
                match = match.NextMatch();
                

            }
        }

        private static void UpdateOperationTypeForPackage(Repository rep, Package pkg)
        {
            foreach (Element el1 in pkg.Elements)
            {

                foreach (Method m in el1.Methods)
                {
                    ReconcileOperationTypes(rep, m);
                }
            }
            foreach (Package pkgSub in pkg.Packages)
            {
                UpdateOperationTypeForPackage(rep, pkgSub);
            }
        }
        /// <summary>
        /// Reconcile the type and parameter of operations/methods in selected context
        /// </summary>
        /// <param name="rep"></param>
        public static void ReconcileOperationTypesWrapper(Repository rep)
        {
            ObjectType oType = rep.GetContextItemType();
            switch (oType)
            {
                case ObjectType.otMethod:
                    ReconcileOperationTypes(rep, (Method)rep.GetContextObject());
                    break;
                case ObjectType.otElement:
                    Element el = (Element)rep.GetContextObject();
                    if (el.Type == "Activity")
                    {
                        Method m = HoUtil.GetOperationFromBrehavior(rep, el);
                        if (m == null)
                        {
                            MessageBox.Show(@"Activity hasn't an operation");
                            return;
                        }
                        ReconcileOperationTypes(rep, m);
                    }
                    else
                    {
                        foreach (Method m in el.Methods)
                        {
                            ReconcileOperationTypes(rep, m);
                        }
                    }
                    break;

                case ObjectType.otPackage:
                    Package pkg = (Package)rep.GetContextObject();
                    UpdateOperationTypeForPackage(rep, pkg);
                    break;
            }

        }

        #region ReconcileOperationTypes
        /// <summary> 
        /// Reconcile Operation types (return, parameter) to fit with existing types 
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="m"></param>
        private static void ReconcileOperationTypes(Repository rep, Method m)
        {
            // update method type
            string methodName = m.Name;
            string methodType = m.ReturnType;
            if (methodType == "") methodType = "void";
            int methodClassifierId = 0;
            if (m.ClassifierID != "")  methodClassifierId = Convert.ToInt32(m.ClassifierID);
            var typeChanged = UpdateTypeName(rep, ref methodClassifierId, ref methodName, ref methodType);
            if (typeChanged)
            {
                if (methodType == "")
                {
                    //MessageBox.Show($"Method '{m.Name}' Typ '{m.ReturnType}'",
                    //    "Warning: Method type undefined, it continues!");
                }
                else
                {
                    m.ClassifierID = methodClassifierId.ToString();
                    m.Name = methodName;
                    m.ReturnType = methodType;
                    m.Update();
                }
            }

            // update parameter
            // set parameter direction to "in"
            foreach (Parameter par in m.Parameters)
            {
                bool parameterUpdated = false;
                if (!par.Kind.Equals("in"))
                {
                    par.Kind = "in";
                    parameterUpdated = true;
                }
                string parName = par.Name;
                string parType = par.Type;
                if (parType == "") parType = "void";
                int classifierId = 0;
                if (! par.ClassifierID.Equals("")) classifierId = Convert.ToInt32(par.ClassifierID);
                typeChanged = UpdateTypeName(rep,ref classifierId , ref parName, ref parType);
                if (typeChanged)
                {
                    if (parType == "")
                    {
                        MessageBox.Show($@"Method { m.Name } Parameter '{par.Name}:{ par.Type}' ",
                            @"Parameter type undefined");
                    }
                    else
                    {
                        par.ClassifierID = classifierId.ToString();
                        par.Name = parName;
                        par.Type = parType;
                        parameterUpdated = true;
                    }
                }
                if (parameterUpdated) par.Update();

            }
        }
                    #endregion

        private static bool UpdateTypeName( Repository rep, ref int classifierId, ref string parName, ref string parType)
        {
            
            // no classifier defined
            // check if type is correct
            Element el = null;
            if (!classifierId.Equals(0))
            {
                try
                {
                    el = rep.GetElementByID(classifierId);
                    if (el.Name != parType) el = null;
                }
                catch
                {
                    // ignored
                }
            }

            if (el == null)
            {

                // get the type
                // find type from type_name
                classifierId = HoUtil.GetTypeId(rep, parType);
                // type not defined
                if (classifierId == 0)
                {
                    if (parType.EndsWith("*", StringComparison.Ordinal))
                    {
                        parType = parType.Substring(0, parType.Length - 1);
                        parName = "*" + parName;

                    }
                    if (parType.EndsWith("*", StringComparison.Ordinal))
                    {
                        parType = parType.Substring(0, parType.Length - 1);
                        parName = "*" + parName;

                    }
                    classifierId = HoUtil.GetTypeId(rep, parType);

                }

                if (classifierId != 0)
                {

                    return true;
                }
                else
                {

                    parType = "";
                    return true;
                }

            }
            else return false;
        }
        public static string GetAssemblyPath()
        {
            return Path.GetDirectoryName(
                Assembly.GetAssembly(typeof(HoService)).CodeBase);
            
        }

        
        /// <summary>
        /// Set/Change the *.xml file path for a VC(Version Controlled) package
        /// 1. Check-In the package 
        /// 2. Move the *.xml file to the new location and check it in
        /// 3. Click on Set/Change VC *.xml file
        /// 4. Enter new *.xml file path
        /// </summary>
        /// <param name="rep"></param>
        /// <returns></returns>
        public static bool SetNewXmlPath(Repository rep)
        {
            if (rep.GetContextItemType().Equals(ObjectType.otPackage))
            {
                Package pkg  = (Package)rep.GetContextObject();
                string guid = pkg.PackageGUID;

                string fileName = HoUtil.GetVccFilePath(rep, pkg);
                OpenFileDialog openFileDialogXml = new OpenFileDialog
                {
                    Filter = @"xml files (*.xml)|*.xml",
                    FileName = Path.GetFileName(fileName),
                    InitialDirectory = Path.GetDirectoryName(fileName)
                };
                if (openFileDialogXml.ShowDialog() == DialogResult.OK)
                {
                    var path = openFileDialogXml.FileName;
                    string rootPath = HoUtil.GetVccRootPath(rep, pkg);
                    // delete root path and an preceding '\'
                    string shortPath = path.Replace(rootPath, "");
                    if (shortPath.StartsWith(@"\", StringComparison.Ordinal)) shortPath = shortPath.Substring(1);
                    
                    // write to repository
                    HoUtil.SetXmlPath(rep, guid, shortPath);

                    // write to file
                    try
                    {
                        // set readonly attribute to false
                        File.SetAttributes(path, FileAttributes.Normal);


                        String strFile = HoUtil.ReadAllText(path);
                        string replace = @"value=[.0-9a-zA-Z_\\-]*\.xml";
                        string replacement = shortPath;
                        strFile = Regex.Replace(strFile, replace, replacement);
                        File.WriteAllText(path, strFile);

                        // checkout + checkin to make the change permanent
                        pkg.VersionControlCheckout();
                        pkg.VersionControlCheckin("Re- organization *.xml files");
                    }
                    catch (Exception e1)
                    {
                        MessageBox.Show(e1.ToString(), $@"Error writing '{path}'"); 
                    }

                    MessageBox.Show(path , @"Changed"); 
                }
               
            }
            return true;
        }
                    #region VcReconcile
        [ServiceOperation("{EAC9246F-96FA-40E7-885A-A572E907AF86}", "Scan XMI and reconcile", "no selection required", false)]
        public static void VcReconcile(Repository rep)
        {
                 //
                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    rep.ScanXMIAndReconcile();
                    Cursor.Current = Cursors.Default;
                }
                catch (Exception e)
                {
                    MessageBox.Show($@"{e}{Environment.NewLine}" , @"Error VC reconcile");
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                }
        }
                    #endregion
                    #region checkOutService
        [ServiceOperation("{1BF01759-DD99-4552-8B68-75F19A3C593E}", "Check out", "Select Package",false)]
        public static void CheckOutService(Repository rep)
        {

            try
            {
                Cursor.Current = Cursors.WaitCursor;
                CheckOut(rep);
                Cursor.Current = Cursors.Default;
            }
            catch (Exception e11)
            {
                MessageBox.Show(e11.ToString(), @"Error Checkout");
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

        }
                    #endregion

        private static void CheckOut(Repository rep,Package pkg=null)
        {
            if (pkg == null) pkg = rep.GetTreeSelectedPackage();
            if (pkg == null) return;

            pkg = HoUtil.GetFirstControlledPackage(rep, pkg);
            if (pkg == null) return;

            if ((Vc.EnumCheckOutStatus)pkg.VersionControlGetStatus() != Vc.EnumCheckOutStatus.CsCheckedIn)
            {
                MessageBox.Show(@"Package isn't checked in");
                return;
            }

            //
            try 
            {
                Cursor.Current = Cursors.WaitCursor;
                pkg.VersionControlCheckout("");
                Cursor.Current = Cursors.Default;
            } catch (Exception e) 
            {
                MessageBox.Show($@"{e} 

{pkg.GetLastError()}", @"Error Checkout");
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }
                    #region checkInService
        [ServiceOperation("{085C84D2-7B51-4783-8189-06E956411B94}", "Check in ", "Select package or something in package", false)]
        public static void CheckInService(Repository rep)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                CheckIn(rep, pkg: null, withGetLatest: false, comment: "code changed");
                Cursor.Current = Cursors.Default;
            }
            catch (Exception e11)
            {
                MessageBox.Show(e11.ToString(), @"Error Check-In");
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }
                    #endregion
                    #region checkInServiceWithUpdateKeyword
        [ServiceOperation("{C5BB52C6-F300-42AE-B4DC-DC97D57D8F7D}", "Check in with get latest (update VC keywords, if Tagged Values 'svnDate'/'svnRevision')", "Select package or something in package", false)]
         public static void CheckInServiceWithUpdateKeyword (Repository rep) {
         try
            {
                Cursor.Current = Cursors.WaitCursor;
                CheckIn(rep, pkg:null, withGetLatest: true, comment:"code changed");
                Cursor.Current = Cursors.Default;
            }
            catch (Exception e11)
            {
                MessageBox.Show(e11.ToString(), "Error Check-In");
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
         }
                    #endregion
                    #region checkIn
        /// <summary>
            /// Check in of a package. If there are the following package tagged values a get latest is performed to update keywords:
            /// -- svnDoc
            /// -- svnRevision
            /// </summary>
            /// <param name="rep">Repository</param>
            /// <param name="pkg">Package, default null</param>
            /// <param name="withGetLatest">false if you want to avoid a getLatest to update VC keywords
            /// Tagged Value "svnDoc" or "svnRevision" of package are true</param>
            /// <param name="comment">A checkin comment, default="0" = aks for check-in comment</param>
        private static void CheckIn(Repository rep, Package pkg=null, bool withGetLatest = false, string comment="0")
        {
                
                if (pkg == null) pkg = rep.GetTreeSelectedPackage();
                if (pkg == null) return;

                pkg = HoUtil.GetFirstControlledPackage(rep, pkg);
                if (pkg == null) return;

                if ((Vc.EnumCheckOutStatus)pkg.VersionControlGetStatus() != Vc.EnumCheckOutStatus.CsCheckedOutToThisUser)
                {
                    MessageBox.Show(@"Package isn't checked out by you");
                    return;
                }


            if (InputBox("Check-In comment", "Check-In", ref comment) == DialogResult.OK)
                {

                    //
                    try
                    {
                        Cursor.Current = Cursors.WaitCursor;
                        pkg.VersionControlCheckin(comment);
                        Cursor.Current = Cursors.Default;
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e + "\n\n" + pkg.GetLastError(), "Error Check-In");
                        return;
                    }
                    finally
                    {
                        Cursor.Current = Cursors.Default;
                       
                    }
                }
                if (withGetLatest)
                {
                    // check if GetLatest is appropriate
                    Element el = rep.GetElementByGuid(pkg.PackageGUID);
                    foreach (EA.TaggedValue t in el.TaggedValues)
                    {
                        if (t.Name == "svnDoc" | t.Name == "svnRevision")
                        {

                            pkg.VersionControlResynchPkgStatus(false);
                            if (pkg.Flags.Contains("Checkout"))
                            {
                                MessageBox.Show("Flags=" + pkg.Flags, "Package Checked out, Break!");
                                return;

                            }
                            pkg.VersionControlGetLatest(true);
                            return;
                        }
                    }
                }
            
        }
                    #endregion

        private static DialogResult InputBox(string title, string promptText, ref string value)
        {
            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = title;
            label.Text = promptText;
            textBox.Text = value;

            buttonOk.Text = @"OK";
            buttonCancel.Text = @"Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, 372, 13);
            textBox.SetBounds(12, 36, 372, 20);
            buttonOk.SetBounds(228, 72, 75, 23);
            buttonCancel.SetBounds(309, 72, 75, 23);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(396, 107);
            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
            form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult dialogResult = form.ShowDialog();
            value = textBox.Text;
            return dialogResult;
        }

        private static bool CheckTaggedValuePackage(Package pkg)
        {
            bool workForPackage = false;
            foreach (Package pkg1 in pkg.Packages)
            {
                if (pkg1.Name.Equals("Architecture") | pkg1.Name.Equals("Behavior"))
                {
                    workForPackage = true;
                    break;
                }
            }
            return workForPackage;
        }

        private static void SetDirectoryTaggedValueRecursive(Repository rep, Package pkg)
        {
            // remember GUID, because of reloading package from xmi
            string pkgGuid = pkg.PackageGUID;
            if (CheckTaggedValuePackage(pkg)) SetDirectoryTaggedValues(rep, pkg);

            pkg = rep.GetPackageByGuid(pkgGuid);
            foreach (Package pkg1 in pkg.Packages)
            {
                SetDirectoryTaggedValueRecursive(rep, pkg1);
            }
               

        }
        public static void SetTaggedValueGui(Repository rep)
        {

            try
            {
                Cursor.Current = Cursors.WaitCursor;
                ObjectType oType = rep.GetContextItemType();
                if (!oType.Equals(ObjectType.otPackage)) return;
                Package pkg = (Package)rep.GetContextObject();
                SetDirectoryTaggedValueRecursive(rep, pkg);
                Cursor.Current = Cursors.Default;
            }
            catch (Exception e11)
            {
                MessageBox.Show(e11.ToString(), @"Error set directory tagged values");
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }


        private static bool IsTaggedValuesComplete(Element el)
        {
            bool isRevision = false;
            bool isDate = false;
            foreach (EA.TaggedValue tag in el.TaggedValues) {
                if (tag.Name == "svnRevision") isRevision = true;
                if (tag.Name == "svnDate") isDate = true;
            }
            if (isRevision & isDate) return true;
            else return false;
        }

        public static void SetDirectoryTaggedValues(Repository rep, Package pkg) {
            bool withCheckIn = false;
            string guid = pkg.PackageGUID;

            Element el = rep.GetElementByGuid(guid);
            if (IsTaggedValuesComplete(el)) return;
            if (pkg.IsVersionControlled)
            {
                int state = pkg.VersionControlGetStatus();
                if (state == 4)
                {
                    MessageBox.Show("",@"Package checked out by another user, break");
                        return;
                }
                if (state == 1)// checked in
                {
                    CheckOut(rep, pkg);
                    withCheckIn = true;
                }
            }
            pkg = rep.GetPackageByGuid(guid);
            SetSvnProperty(rep, pkg);

            // set tagged values
            el = rep.GetElementByGuid(guid);
            bool createSvnDate = true;
            bool createSvnRevision = true;
            foreach (EA.TaggedValue t in el.TaggedValues)
            {
                if (t.Name == "svnDate") createSvnDate = false;
                if (t.Name == "svnRevision") createSvnRevision = false;

            }
            EA.TaggedValue tag;
            if (createSvnDate)
            {
                tag = (EA.TaggedValue)el.TaggedValues.AddNew("svnDate", "");
                tag.Value = "$Date: $";
                el.TaggedValues.Refresh();
                tag.Update();

            }
            if (createSvnRevision)
            {
                tag = (EA.TaggedValue)el.TaggedValues.AddNew("svnRevision", "");
                tag.Value = "$Revision: $";
                el.TaggedValues.Refresh();
                tag.Update();
            }


            if (pkg.IsVersionControlled)
            {
                int state = pkg.VersionControlGetStatus();
                if (state == 2 & withCheckIn)// checked out to this user
                {
                    //EaService.checkIn(rep, pkg, "");
                    CheckIn(rep,pkg, withGetLatest:true, comment:"svn tags added");
                }
             }
        }

        public static void SetSvnProperty(Repository rep, Package pkg)
        {
            // set svn properties
            if (pkg.IsVersionControlled)
            {
                svn svnHandle = new svn(rep, pkg);
                svnHandle.setProperty();
            }
        }
        public static void GotoSvnLog(Repository rep, Package pkg)
        {
            // set svn properties
            if (pkg.IsVersionControlled)
            {
                svn svnHandle = new svn(rep, pkg);
                svnHandle.gotoLog();
            }
        }
        public static void GotoSvnBrowser(Repository rep, Package pkg)
        {
            // set svn properties
            if (pkg.IsVersionControlled)
            {
                svn svnHandle = new svn(rep, pkg);
                svnHandle.gotoRepoBrowser();
            }
        }
        #region insertDiagramElement

        /// <summary>insertDiagramElement insert a diagram element 
        /// <para>type: type of the node like "Action", Activity", "MergeNode"</para>
        ///       MergeNode may have the subType "no" to draw a transition with a "no" guard.
        /// <para>subTyp: subType of the node:
        ///       StateNode: 100=ActivityInitial, 101 ActivityFinal
        /// </para>guardString  of the connector "","yes","no",..
        ///        if "yes" or "" it will locate the node under the last selected element
        /// </summary> 
        public static void InsertDiagramElement(Repository rep, string type, string subType)
        {

            Diagram dia = rep.GetCurrentDiagram();
            if (dia == null)
            {
                return;
            }
            if (dia.Type != "Activity") return;

            rep.SaveDiagram(dia.DiagramID);

            EA.Package pkg = rep.GetPackageByID(dia.PackageID);
            // Check if parent is an Element
            EA.Element elNeu;
            var parentId = dia.ParentID;
            if (parentId != 0)
            {
                // Parent is Element
                EA.Element el = rep.GetElementByID(parentId);
                elNeu = (EA.Element) el.Elements.AddNew(dia.Name, type);
                elNeu.Subtype = Int32.Parse(subType);
                el.Elements.Refresh();
                elNeu.Update();
            }
            else
            {
                // parent is a package
                pkg = rep.GetPackageByID(dia.PackageID);
                elNeu = (EA.Element)pkg.Elements.AddNew(dia.Name, type);
                elNeu.Subtype = Int32.Parse(subType);
                pkg.Elements.Refresh();
                elNeu.Update();
            }
            //pkg.Update();

            int size = 20;
            int left = 300;
            int right = left + size;
            int top = -50;
            int bottom = top-size;

            string position = "l=" + left + ";r=" + right + ";t=" + top + ";b=" + bottom + ";";
            // add diagramobject to diagram
            var dObj = (DiagramObject)dia.DiagramObjects.AddNew(position, "");
            dObj.Style = "LBL=CX=41:CY=14:OX=-10:OY=-19:HDN=0:BLD=0:ITA=0:UND=0:CLR=-1:ALN=1:ALT=0:ROT=0;";
            dObj.ElementID = elNeu.ElementID;
            dObj.Sequence = 1;
            dObj.Update();
            pkg.Elements.Refresh();
            dia.Update();
           
            rep.ReloadDiagram(dia.DiagramID);
        }
#endregion



        #region insertDiagramElementAndConnect
        /// <summary>insertDiagramElement insert a diagram element and connects it to all selected diagramobject 
        /// <para>type: type of the node like "Action", Activity", "MergeNode"</para>
        ///       MergeNode may have the subType "no" to draw a transition with a "no" guard.
        /// <para>subTyp: subType of the node:
        ///       StateNode: 100=ActivityInitial, 101 ActivityFinal
        /// </para>guardString  of the connector "","yes","no",..
        ///        if "yes" or "" it will locate the node under the last selected element
        /// </summary> 
        public static void InsertDiagramElementAndConnect(Repository rep, string type, string subType, string guardString="") 
        {
            Diagram dia = rep.GetCurrentDiagram();
            if (dia == null) return;
            if (dia.Type != "Activity") return;

            int count = dia.SelectedObjects.Count;
            if (count == 0) return;

            rep.SaveDiagram(dia.DiagramID);
            List<DiagramObject> oldCollection = new List<DiagramObject>();

            // get context element (last selected element)
            Element originalSrcEl = HoUtil.GetElementFromContextObject(rep);
            if (originalSrcEl == null) return;
            int  originalSrcId =  originalSrcEl.ElementID;

            for (int i = count - 1; i > -1; i = i - 1)
            {
                oldCollection.Add((DiagramObject)dia.SelectedObjects.GetAt((short)i));
                // keep last selected element
                //if (i > 0) dia.SelectedObjects.DeleteAt((short)i, true);
            }
            dia.GetDiagramObjectByID(originalSrcId, "");

            DiagramObject trgObj = CreateDiagramObjectFromContext(rep, "", type, subType,0,0,guardString, originalSrcEl);
            Element trgtEl = rep.GetElementByID(trgObj.ElementID);

            // if connection to more than one element make sure the new elemenet is on the deepest position
            int offset = 50;
            if (guardString == "yes") offset = 0;
            int bottom = 1000;
            int diff = trgObj.top - trgObj.bottom;


            foreach (DiagramObject diaObj in oldCollection)
                {
                    Element srcEl = rep.GetElementByID(diaObj.ElementID);
                    // don't connect two times
                    if (originalSrcId != diaObj.ElementID)
                    {
                        EA.Connector con = (EA.Connector)srcEl.Connectors.AddNew("", "ControlFlow");
                        con.SupplierID = trgObj.ElementID;
                        if (type == "MergeNode" && guardString == "no" && srcEl.Type == "Decision") con.TransitionGuard = "no";
                        con.Update();
                        srcEl.Connectors.Refresh();
                        dia.DiagramLinks.Refresh();
                        //trgtEl.Connectors.Refresh();

                        // set line style
                        string style = "LV";
                        if ((srcEl.Type == "Action" | srcEl.Type == "Activity") & guardString == "no") style = "LH";
                        var link = GetDiagramLinkForConnector(dia, con.ConnectorID);
                        if (link != null) HoUtil.SetLineStyleForDiagramLink(style, link);

                    }
                    // set new high/bottom_Position
                    var srcObj = dia.GetDiagramObjectByID(srcEl.ElementID, "");
                    if (srcObj.bottom < bottom) bottom = srcObj.bottom;

            }
            if (oldCollection.Count > 1)
            {
                // set bottom/high of target
                trgObj.top = bottom + diff - offset;
                trgObj.bottom = bottom - offset;
                trgObj.Sequence = 1;
                trgObj.Update();
                // final
                if (subType == "101" && trgtEl.ParentID > 0)
                {
                    Element parEl = rep.GetElementByID(trgtEl.ParentID);
                    if (parEl.Type == "Activity")
                    {
                        DiagramObject parObj = dia.GetDiagramObjectByID(parEl.ElementID, "");
                        if (parObj != null)
                        {
                            parObj.bottom = trgObj.bottom - 30;
                            parObj.Update();
                        }

                    }
                }
            }

            rep.ReloadDiagram(dia.DiagramID);
            dia.SelectedObjects.AddNew(trgtEl.ElementID.ToString(), trgtEl.ObjectType.ToString());
        }
        #endregion
        #region noGuard

        /// <summary>
        /// Create a [guardText] Guard connector or change connector to a [guardText] Guard for a connector / Control Flow
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="guardText"></param>
        [ServiceOperation("{F0FF506A-7124-48CC-B00B-2D8DB3047E39}", "Make a [No] guard",
            "Make a [no] guard for the connector ", false)]
        public static void NoGuard(Repository rep, string guardText)
        {
            EaDiagram curDiagram = new EaDiagram(rep);
            if (curDiagram.Dia == null) return;

            rep.SaveDiagram(curDiagram.Dia.DiagramID);
            EA.Connector con = curDiagram.SelectedConnector;

            // Connector selected
            if (con == null)
            {

                // One object selected or no diagram object selected.
                if (curDiagram.SelectedObjectsCount < 2 || !curDiagram.IsSelectedObjects) return;
                if (curDiagram.SelectedObjectsCount == 2)
                {
                    // draw connector
                    con = DrawConnectorBetweenNodes(rep, curDiagram.SelObjects[1], curDiagram.SelObjects[0],
                        "ControlFlow", stereotype: "", lineStyle: "D");
                    // set line style
                    con.TransitionGuard = guardText;
                    con.Update();
                    // the last selected element is the first in the list
                    curDiagram.SelectDiagramObject();
                }
                else
                {
                    JoinDiagramObjectsToLastSelected(rep);
                }
            }
            else
            {

                con.TransitionGuard = guardText;
                // don't change existing line style, if selected=> Link exists
                //var link = GetDiagramLinkForConnector(curDiagram.Dia, con.ConnectorID);
                //if (link != null) HoUtil.SetLineStyleForDiagramLink("LH", link);
                con.Update();
                curDiagram.SelectDiagramObject(con);
            }
        }

        #endregion
        #region joinDiagramObjectsToLastSelected
        [ServiceOperation("{6946E63E-3237-4F45-B4D8-7EE0D6580FA5}", "Join nodes to the last selected node", "Only Activity Diagram", false)]
        public static void JoinDiagramObjectsToLastSelected(Repository rep)
        {
            Diagram dia = rep.GetCurrentDiagram();
            if (dia == null) return;
            int count = dia.SelectedObjects.Count;
            if (count < 2) return;
            rep.SaveDiagram(dia.DiagramID);

            // target object/element
            Element trgEl = (Element)rep.GetContextObject();


            // The last selected element is the context element!
            // The selected elements don't reflect the sequence of selected elements
            for (int i = 0; i < count; i = i + 1)
            {
                DiagramObject srcObj = (DiagramObject)dia.SelectedObjects.GetAt((short)i);
                var srcEl = rep.GetElementByID(srcObj.ElementID);
                if (srcEl.ElementID == trgEl.ElementID) continue;
                Connector connector = GetConnectionDefault(dia);

                EA.Connector con = (EA.Connector)srcEl.Connectors.AddNew("", connector.Type);
                con.SupplierID = trgEl.ElementID;
                con.Stereotype = connector.Stereotype;
                con.Update();
                srcEl.Connectors.Refresh();
                trgEl.Connectors.Refresh();
                dia.DiagramLinks.Refresh();
                // set line style
                DiagramLink link = GetDiagramLinkForConnector(dia, con.ConnectorID);
                if (link != null) HoUtil.SetLineStyleForDiagramLink("LV", link);
               
            }
    
            rep.ReloadDiagram(dia.DiagramID);
            dia.SelectedObjects.AddNew(trgEl.ElementID.ToString(), trgEl.ObjectType.ToString());
        }
                    #endregion

        // ReSharper disable once UnusedParameter.Local
        private static Connector GetConnectionDefault(Diagram dia)
        {
            return new Connector("ControlFlow", "");

        }
                    #region splitDiagramObjectsToLastSelected
        [ServiceOperation("{521FCFEB-984B-43F0-A710-E97C29E4C8EE}", "Split last selected Diagram object from previous selected Diagram Objects", "Incoming and outgoing connections", false)]
        public static void SplitDiagramObjectsToLastSelected(Repository rep)
        {
            Diagram dia = rep.GetCurrentDiagram();
            if (dia == null) return;
            int count = dia.SelectedObjects.Count;
            if (count < 2) return;
            rep.SaveDiagram(dia.DiagramID);

            // target object/element
            ObjectType objType = rep.GetContextItemType();
            if (!(objType.Equals(ObjectType.otElement))) return;
            Element trgEl = (Element)rep.GetContextObject();

            for (int i = 0; i < count; i = i + 1)
            {
                DiagramObject srcObj = (DiagramObject)dia.SelectedObjects.GetAt((short)i);
                var srcEl = rep.GetElementByID(srcObj.ElementID);
                if (srcEl.ElementID == trgEl.ElementID) continue;
                SplitElementsByConnectorType(srcEl, trgEl, "ControlFlow");
            }

            rep.ReloadDiagram(dia.DiagramID);
            dia.SelectedObjects.AddNew(trgEl.ElementID.ToString(), trgEl.ObjectType.ToString());
        }
                    #endregion
                    #region splitAllDiagramObjectsToLastSelected
        [ServiceOperation("{CA29CB67-77EA-4BCC-B3B4-8893F6B0DAE2}", "Split last selected Diagram object from all connected Diagram Objects", "Incoming and outgoing connections", false)]
        public static void SplitAllDiagramObjectsToLastSelected(Repository rep)
        {
            Diagram dia = rep.GetCurrentDiagram();
            if (dia == null) return;
            int count = dia.SelectedObjects.Count;
            if (count == 0) return;
            rep.SaveDiagram(dia.DiagramID);

            // target object/element
            ObjectType objType = rep.GetContextItemType();
            if (!(objType.Equals(ObjectType.otElement))) return;
            Element trgEl = (Element)rep.GetContextObject();

            foreach (DiagramObject srcObj in dia.DiagramObjects)
            {
                var srcEl = rep.GetElementByID(srcObj.ElementID);
                if (srcEl.ElementID == trgEl.ElementID) continue;
                SplitElementsByConnectorType(srcEl, trgEl);
            }

            rep.ReloadDiagram(dia.DiagramID);
            dia.SelectedObjects.AddNew(trgEl.ElementID.ToString(), trgEl.ObjectType.ToString());
        }
                    #endregion
                    #region splitElementsByConnectorType
        /// <summary>
        /// Split / delete the connection of two elements
        /// </summary>
        /// <param name="srcEl">Source element of the connector</param>
        /// <param name="trgEl">Target element of the connector</param>
        /// <param name="connectorType">Connector type or default "All"</param>
        /// <param name="direction">Direction of connection ("in","out","all" or default "All"</param>
        private static void SplitElementsByConnectorType(Element srcEl, Element trgEl, string connectorType="all", string direction="all")
        {
            for (int i = srcEl.Connectors.Count - 1;i >= 0; i=i-1 ) {
                EA.Connector con = (EA.Connector)srcEl.Connectors.GetAt((short)i);
                if (con.SupplierID == trgEl.ElementID && (con.Type == connectorType | connectorType == "all" | direction == "all" | direction == "in") )
                {
                    srcEl.Connectors.DeleteAt((short)i, true);
                }
                if (con.ClientID == trgEl.ElementID && (con.Type == connectorType | connectorType == "all" | direction == "all" | direction == "out"))
                {
                    srcEl.Connectors.DeleteAt((short)i, true);
                }
            }
        }
                    #endregion
        public static void MakeNested(Repository rep)
        {
            Diagram dia = rep.GetCurrentDiagram();
            if (dia == null) return;
            int count = dia.SelectedObjects.Count;
            if (count < 2) return;

            rep.SaveDiagram(dia.DiagramID);

            // target object/element

            ObjectType objType = rep.GetContextItemType();
            if (!(objType.Equals(ObjectType.otElement))) return;

            var trgEl = (Element)rep.GetContextObject();
            if  (!(trgEl.Type.Equals("Activity"))) {
                MessageBox.Show($@"Target '{trgEl.Name}':{trgEl.Type}' isn't an Activity", @"Only move below Activity is allowed");
                return;
            }
            for (int i = 0; i < count; i = i + 1)
            {
                DiagramObject srcObj = (DiagramObject)dia.SelectedObjects.GetAt((short)i);
                var srcEl = rep.GetElementByID(srcObj.ElementID);
                if (srcEl.ElementID == trgEl.ElementID) continue;
                srcEl.ParentID = trgEl.ElementID;
                srcEl.Update();
               
            }
           
        }
        public static void DeleteInvisibleUseRealizationDependencies (Repository rep)
        {
            EA.Connector con;
            Diagram dia = rep.GetCurrentDiagram();
            if (dia == null) return;
            if (!rep.GetContextItemType().Equals(ObjectType.otElement)) return;

            // only one diagram object selected as source
            if (dia.SelectedObjects.Count != 1) return;

            rep.SaveDiagram(dia.DiagramID);
            DiagramObject diaObjSource = (DiagramObject)dia.SelectedObjects.GetAt(0);
            var elSource = rep.GetElementByID(diaObjSource.ElementID);
            var elSourceId = elSource.ElementID;
            if (! ("Interface Class".Contains(elSource.Type))) return;

            // list of all connectorIDs
            List<int> lInternalId = new List<int>();
            foreach (DiagramLink link in dia.DiagramLinks)
            {
               con = rep.GetConnectorByID(link.ConnectorID);
               if (con.ClientID != elSourceId) continue;
               if (!("Usage Realisation".Contains(con.Type))) continue;
               lInternalId.Add(con.ConnectorID);

            }


            for (int i = elSource.Connectors.Count - 1; i >=0; i = i - 1)
            {
                con = (EA.Connector)elSource.Connectors.GetAt((short)i);
                var conType = con.Type;
                if ("Usage Realisation".Contains(conType))
                {
                    // check if target is..
                    var elTarget = rep.GetElementByID(con.SupplierID);
                    if (elTarget.Type == "Interface")
                    {
                        if (lInternalId.BinarySearch(con.ConnectorID) < 0)
                        {
                            elSource.Connectors.DeleteAt((short)i, true);
                        }
                    }
                }
               
            }

            
        }
                    #region copyReleaseInfoOfModuleService
        [ServiceOperation("{1C78E1C0-AAC8-4284-8C25-2D776FF373BC}", "Copy release information to clipboard", "Select Component", false)]
        public static void CopyReleaseInfoOfModuleService(Repository rep)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                CopyReleaseInfoOfModule(rep);
                Cursor.Current = Cursors.Default;
            }
            catch (Exception e10)
            {
                MessageBox.Show(e10.ToString(), @"Error generating ports for component");
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

        }
                    #endregion

        private static void CopyReleaseInfoOfModule(Repository rep)
         {
             Diagram dia = rep.GetCurrentDiagram();
             if (dia == null) return;
             if (!rep.GetContextItemType().Equals(ObjectType.otElement)) return;
             Element elSource = (Element)rep.GetContextObject();
             if (elSource.Type != "Component") return;

             dia.GetDiagramObjectByID(elSource.ElementID, "");

             string txt = "";
             string nl = "";
             foreach (DiagramObject obj in dia.DiagramObjects)
             {
                 var elTarget = rep.GetElementByID(obj.ElementID);
                 if (!("Class Interface".Contains(elTarget.Type))) continue;
                 txt = txt + nl + AddReleaseInformation(rep, elTarget);
                 nl = "\r\n";
             }
             Clipboard.SetText(txt);
         }

        private static string AddReleaseInformation(Repository rep, Element el) {
            string txt;
            string path = HoUtil.GetGenFilePathElement(el);
            if (path == "")
            {
                MessageBox.Show(@"No file defined in property for: '" + el.Name + @"':" + el.Type);
                return "";
            }
            try
            {
                txt = HoUtil.ReadAllText(path);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), @"Error Reading file '" + el.Name + @"':" + el.Type);
                return "";
            }
            string extension = ".c";
            if (el.Type == "Interface") extension = ".h";
            string name = el.Name + extension;
            if (name.Length > 58) name = name + "   ";
            else name = name.PadRight(60);
            return name + GetReleaseInformationFromText(txt);
        }
        private static string GetReleaseInformationFromText(string txt) {
            string patternRev = @"\$Rev(ision|):[\s]*([0-9]*)";
            string patternDate = @"\$Date:[\s][^\$]*";
            string txtResult = "";
            

            Match matchPath = Regex.Match(txt, patternRev, RegexOptions.Multiline);
            if (matchPath.Success)
            {
                txtResult = matchPath.Value;
            }
            Match matchDate = Regex.Match(txt, patternDate, RegexOptions.Multiline);
            if (matchDate.Success)
            {
                txtResult = txtResult + " " + matchDate.Value;
            }
            return txtResult;
        }
        /// <summary>
        /// Generate component ports for selected component in diagram:
        /// - Generate ports for all Classes/Interfaces in diagram
        /// - They may be manually changed later
        /// </summary>
        /// <param name="rep"></param>
        #region generateComponentPortsService
        [ServiceOperation("{00602D5F-D581-4926-A31F-806F2D06691C}", "Generate ports for component", "Select Component", false)]
        public static void GenerateComponentPortsService(Repository rep)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                GenerateComponentPortsFromDiagram(rep,isRequired:false, useDependantInterface:true,
                showPorts: false);
                Cursor.Current = Cursors.Default;
            }
            catch (Exception e10)
            {
                MessageBox.Show(e10.ToString(), @"Error generating ports for component");
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

        }
                    #endregion

        /// <summary>
        /// Generate component ports from selected component in diagram:
        /// - Generate ports for all Classes/Interfaces in diagram
        /// - They may be manually changed later
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="isRequired"></param>
        /// <param name="useDependantInterface">Add ports for indirect used header files (include in header file)</param>
        /// <param name="showPorts"></param>
        private static void GenerateComponentPortsFromDiagram(Repository rep, bool isRequired=false, bool useDependantInterface=true,
            bool showPorts=true)
        {
            // Don't visualize dependent interfaces which are used in header files.

            Diagram dia = rep.GetCurrentDiagram();
            if (dia == null) return;
            if (!rep.GetContextItemType().Equals(ObjectType.otElement)) return;
            Element elSource = (Element)rep.GetContextObject();
            if (elSource.Type != "Component") return;

            dia.GetDiagramObjectByID(elSource.ElementID, "");
            rep.SaveDiagram(dia.DiagramID);

            // Generate a Port for all interfaces of the diagram
            // No private interface '_i' 
            foreach (DiagramObject obj in dia.DiagramObjects)
            {
                var elTarget = rep.GetElementByID(obj.ElementID);

                if (!("Interface".Contains(elTarget.Type))) continue;
                if (!(elTarget.Name.ToUpper().EndsWith("_I", StringComparison.Ordinal)))
                {
                    AddNewPortToComponent(elSource, elTarget, isRequired);
                }
            }
            // if generate 'required interface' also generate dependent interfaces from Code of the header files
            if (useDependantInterface)
            {
                foreach (DiagramObject obj in dia.DiagramObjects) {
                    var elTarget = rep.GetElementByID(obj.ElementID);
                    // Add all required interfaces from interfaces
                    // Interface and Class may include it (use code)
                    if ("Class Interface".Contains(elTarget.Type))
                    {
                        List<Element> lEl = GetIncludedHeaderFilesFromCode(rep, elTarget);
                        foreach (Element el in lEl)
                        {
                            if (el == null) continue;
                            if (String.IsNullOrWhiteSpace(el.Name)) continue;
                            

                            // no internal interface, ends with '_i'
                            if (!(el.Name.ToUpper().EndsWith("_I", StringComparison.Ordinal)))
                            {
                                AddNewPortToComponent(elSource, el, isRequired: true);
                            }
                        }
                    }
                }
            }
            if (showPorts)   ShowEmbeddedElementsGui(rep);

        }
        /// <summary>
        /// Get list of header files for Element code. It checks for plausibility:
        /// - Extension (.h=Interface,.c not Interface)
        /// - File name
        /// 
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="el"></param>
        /// <returns></returns>
        private static List<Element> GetIncludedHeaderFilesFromCode(Repository rep, Element el)
        {
            List<Element> lEl = new List<Element>();
            string path = HoUtil.GetGenFilePathElement(el);
            if (path == "")
            {
                MessageBox.Show(@"No file defined in property for: '" + el.Name + "':" + el.Type);
                return lEl;
            }


            // check if path plausible:
            // interface: *.h
            // class/component:*.c
            if (  (path.ToLower().EndsWith("h") && el.Type != "Interface") || (path.ToLower().EndsWith("c") && el.Type == "Interface"))
            {
                MessageBox.Show($@"File='{Path.GetFileName(path)}'
ElementName: {el.Name}
ElementType:{el.Type}", 
                    
                    $@"Code file extension for Element type '{el.Type}' probably wrong!");
                return lEl;
            }
            // Check if filename is plausible
            if (el.Name.ToLower() != Path.GetFileNameWithoutExtension(path).ToLower())
            {
                MessageBox.Show($@"File='{Path.GetFileName(path)}'
ElementName: {el.Name}
ElementType:{el.Type}",

                    $@"Code file name for Element  '{el.Name}' probably wrong!");
                return lEl;
            }

            string text;
            try
            {
                text = HoUtil.ReadAllText(path);
            }
            catch (Exception e)
            {
                Clipboard.SetText(e.ToString());
                MessageBox.Show($@"{Environment.NewLine}see Clipboard!{Environment.NewLine}{Environment.NewLine}{e}",
                    @"Error Reading file '" + el.Name + @"':" + el.Type);
                return lEl;
            }
            lEl = GetInterfacesFromText(rep, rep.GetPackageByID(el.PackageID), text, addMissingInterface:true);

            
            
            return lEl;

        }

        /// <summary>
        /// Get list of Interfaces for the code snippet. If the interface doesn't exist create one.
        /// 
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="pkg"></param>
        /// <param name="codeSnippet"></param>
        /// <returns></returns>
        private static List<Element> GetInterfacesFromCodeSnippet(Repository rep, EA.Package pkg, string codeSnippet)
        {
            List<Element> lEl = new List<Element>();
            
            lEl = GetInterfacesFromText(rep, rep.GetPackageByID(pkg.PackageID), codeSnippet, addMissingInterface: true);
            return lEl;

        }
        /// <summary>
        /// Add a Stereotype. If Stereotype already exists do nothing.
        /// </summary>
        /// <param name="oldStereotype"></param>
        /// <param name="newStereotype"></param>
        /// <returns></returns>
        private static string StereotypeAdd(string oldStereotype, string newStereotype)
        {
            if (newStereotype.Trim() == "") return oldStereotype;
                if (StereotypeExists(oldStereotype, newStereotype)) return oldStereotype;

            return  oldStereotype.Trim() == "" ? newStereotype :  $"{oldStereotype},{newStereotype}";
        }
        /// <summary>
        /// Checks whether a stereotype exists.
        /// </summary>
        /// <param name="oldStereotype"></param>
        /// <param name="newStereotype"></param>
        /// <returns></returns>
        private static bool StereotypeExists(string oldStereotype, string newStereotype)
        {
            if (newStereotype.Trim() == "") return true;
            string[] lOldStereotype = oldStereotype.Split(',');
            if (lOldStereotype.Contains(newStereotype)) return true;

            return false;
        }


        #region vCGetState
        [ServiceOperation("{597608A2-5C3F-4AE6-9B18-86C1B3C27382}", "Get and update VC state of selected package", "Select Packages", false)]
        public static void VcGetState(Repository rep)
        {
            Package pkg = rep.GetTreeSelectedPackage();
            if (pkg != null)
            {
                if (pkg.IsControlled)
                {
                    string pkgState = HoUtil.GetVCstate(rep, pkg, true);
                     DialogResult result = MessageBox.Show($@"Package is {pkgState}
Path={pkg.XMLPath} 
Flags={pkg.Flags}", @"Update package state?", MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes)  HoUtil.UpdateVc(rep, pkg);
                }
            }
            else MessageBox.Show(@"No package selected");
        }
                    #endregion
                    #region updateVcStateOfSelectedPackageRecursiveService
        [ServiceOperation("{A521EB65-3F3C-4C5D-9B82-D12FFCEC71D4}", "Update VC-State of package(recursive)", "Select Packages or model", false)]
        public static void UpdateVcStateOfSelectedPackageRecursiveService(Repository rep)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;
               
                
                Package pkg = rep.GetTreeSelectedPackage();
                UpdateVcStateRecursive(rep, pkg);
                    //pkg = rep.GetTreeSelectedPackage();
                //if (pkg != null && pkg.ParentID == 0)
                //{
                //    foreach (EA.Package p in rep.Models)
                //    {
                //        updateVcStateRecursive(rep,p);
                //    }
                //}
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
                    #endregion
                    #region updateVcStateRecursive

        private static void UpdateVcStateRecursive(Repository rep, Package pkg,bool recursive=true)
        {
            if (pkg.IsControlled) HoUtil.UpdateVc(rep, pkg);
            if (recursive)
            {
                foreach (Package p in pkg.Packages)
                {
                    if (p.IsControlled) HoUtil.UpdateVc(rep, p);
                    UpdateVcStateRecursive(rep, p);
                }
            }
        }
                    #endregion
                    #region getDiagramLinkForConnector

        private static DiagramLink GetDiagramLinkForConnector(Diagram dia, int connectorId)
        {
            foreach (DiagramLink link in dia.DiagramLinks)
            {
                if (connectorId == link.ConnectorID) return link;
            }
            return null;
        }
                    #endregion
                    #region AddFavorite
        /// <summary>
        /// Add selected item to Favorite:
        /// - element, package, diagram, attribute, operation
        /// - Favorite is stored in t_xref under the type: 'Favorite'
        /// </summary>
        /// <param name="rep"></param>
        [ServiceOperation("{7B3D065F-34AF-436E-AF96-F83DC8C3505E}", "Add selected item to Favorite",
            "Element, package, diagram, attribute, operation",
            isTextRequired: false)]
        // ReSharper disable once UnusedMember.Global
        public static void AddFavorite(Repository rep)
        {
            Favorite f = new Favorite(rep, GetGuidfromSelectedItem(rep));
            f.Save();

        }
                    #endregion
                    #region RemoveFavorite
        /// <summary>
        /// Remove selected item to Favorite:
        /// - element, package, diagram, attribute, operation
        /// - Favorite is stored in t_xref under the type: 'Favorite'
        /// </summary>
        /// <param name="rep"></param>
        [ServiceOperation("{41BFF6D9-DE73-481B-A3EC-7E158AE9BE9E}", "Remove selected item from Favorite",
            "Element, package, diagram, attribute, operation",
            isTextRequired: false)]
        public static void RemoveFavorite(Repository rep)
        {
            Favorite f = new Favorite(rep, GetGuidfromSelectedItem(rep));
            f.Delete();

        }
                    #endregion
                    #region Favorites
        /// <summary>
        /// List Favorite:
        /// - List Favorites in the search window
        /// </summary>
        /// <param name="rep"></param>
        [ServiceOperation("{756710FA-A99E-40D3-B265-518DDF1014D1}", "Search Favorites",
            "Element, package, diagram, attribute, operation",
            isTextRequired: false)]
        public static void Favorites(Repository rep)
        {
            Favorite f = new Favorite(rep);
            f.Search();

        }
                    #endregion
                    #region getGuidfromSelectedItem
        private static string GetGuidfromSelectedItem(Repository rep) {
            ObjectType objectType = rep.GetContextItemType();
            string guid = "";
            switch (objectType)
            {
                case ObjectType.otAttribute:
                    EA.Attribute a = (EA.Attribute)rep.GetContextObject();
                    guid = a.AttributeGUID;
                    break;
                case ObjectType.otMethod:
                    Method m = (Method)rep.GetContextObject();
                    guid = m.MethodGUID;
                    break;
                case ObjectType.otElement:
                    Element el = (Element)rep.GetContextObject();
                    guid = el.ElementGUID;
                    break;
                case ObjectType.otDiagram:
                    Diagram dia = (Diagram)rep.GetContextObject();
                    guid = dia.DiagramGUID;
                    break;
                case ObjectType.otPackage:
                    Package pkg = (Package)rep.GetContextObject();
                    guid = pkg.PackageGUID;
                    break;
                default:
                    MessageBox.Show(@"Nothing useful selected");
                    break;
            }
            return guid;
        }
                    #endregion
                    #region moveEmbeddedLeftGUI
        /// <summary>
        /// Move the selected ports left
        /// - If left level is crossed it locates ports to top left corner.
        /// </summary>
        /// <param name="rep"></param>
        [ServiceOperation("{28188D09-7B40-4396-8FCF-90EA901CFE12}", "Embedded Elements left", "Select embedded elements", isTextRequired: false)]
        public static void MoveEmbeddedLeftGui(Repository rep)
        {
            Diagram dia = rep.GetCurrentDiagram();
            if (dia == null) return;
            int selCount = dia.SelectedObjects.Count;
            if (selCount == 0) return;
            rep.SaveDiagram(dia.DiagramID);

            // check if port,..
            DiagramObject objPort0 = (DiagramObject)dia.SelectedObjects.GetAt(0);
            Element port = rep.GetElementByID(objPort0.ElementID);
            if (!EmbeddedElementTypes.Contains(port.Type)) return;

            // get parent of embedded element
            Element el = rep.GetElementByID(port.ParentID);
            DiagramObject obj = dia.GetDiagramObjectByID(el.ElementID, "");


            // check if left limit element is crossed
            int leftLimit = obj.left - 0;// limit cross over left 
            bool isRightLimitCrossed = false;
            foreach (DiagramObject objPort in dia.SelectedObjects)
            {
                if (objPort.left < leftLimit)
                {
                    isRightLimitCrossed = true;
                    break;
                }

            }
            // move all to left upper corner of element
            int startValueTop = obj.top - 8;
            int startValueLeft = obj.left - 8;
            int pos = 0;
            foreach (DiagramObject objPort in dia.SelectedObjects)
            {
                if (!isRightLimitCrossed)
                {
                    // move to right
                    objPort.left = objPort.left - 10;
                    objPort.Update();
                }
                else
                {
                    // move from top to down
                    objPort.top = startValueTop - pos * 20;
                    objPort.left = startValueLeft;
                    objPort.Update();
                    pos = pos + 1;
                }

            }
          
        }
                    #endregion
                    #region moveEmbeddedRightGUI
        /// <summary>
        /// Move the selected ports right
        /// - If right level is crossed it locates ports to top right corner.
        /// </summary>
        /// <param name="rep"></param>
        [ServiceOperation("{91998805-D1E6-4A3E-B9AA-8218B1C9F4AB}", "Embedded Elements right", "Select embedded elements", isTextRequired: false)]
        public static void MoveEmbeddedRightGui(Repository rep)
        {
            Diagram dia = rep.GetCurrentDiagram();
            if (dia == null) return;
            int selCount = dia.SelectedObjects.Count;
            if (selCount == 0) return;
            rep.SaveDiagram(dia.DiagramID);

            // check if port,..
            DiagramObject objPort0 = (DiagramObject)dia.SelectedObjects.GetAt(0);
            Element port = rep.GetElementByID(objPort0.ElementID);
            if (!EmbeddedElementTypes.Contains(port.Type)) return;

            // get parent of embedded element
            Element el = rep.GetElementByID(port.ParentID);
            DiagramObject obj = dia.GetDiagramObjectByID(el.ElementID, "");


            // check if right limit element is crossed
            int rightLimit = obj.right - 16;// limit cross over right 
            bool isRightLimitCrossed = false;
            foreach (DiagramObject objPort in dia.SelectedObjects)
            {
                if (objPort.left > rightLimit)
                {
                    isRightLimitCrossed = true;
                    break;
                }

            }
            // move all to left upper corner of element
            int startValueTop = obj.top - 8;
            int startValueLeft = obj.right - 8;
            int pos = 0;
            foreach (DiagramObject objPort in dia.SelectedObjects)
            {
                if (!isRightLimitCrossed)
                {
                    // move to right
                    objPort.left = objPort.left + 10;
                    objPort.Update();
                }
                else
                {
                    // move from top to down
                    objPort.top = startValueTop - pos * 20;
                    objPort.left = startValueLeft ;
                    objPort.Update();
                    pos = pos + 1;
                }

            }

        }
                    #endregion
                    #region moveEmbeddedDownGUI
        /// <summary>
        /// Move the selected ports down
        /// - If lower level is crossed it locates ports to bottom left corners.
        /// </summary>
        /// <param name="rep"></param>
        [ServiceOperation("{1F5BA798-F9AC-4F80-8004-A8E8236AF629}", "Embedded Elements down", "Select embedded elements", isTextRequired: false)]
        public static void MoveEmbeddedDownGui(Repository rep)
        {
            Diagram dia = rep.GetCurrentDiagram();
            if (dia == null) return;
            int selCount = dia.SelectedObjects.Count;
            if (selCount == 0) return;
            rep.SaveDiagram(dia.DiagramID);

            // check if port,..
            DiagramObject objPort0 = (DiagramObject)dia.SelectedObjects.GetAt(0);
            Element port = rep.GetElementByID(objPort0.ElementID);
            if (!EmbeddedElementTypes.Contains(port.Type)) return;

            // get parent of embedded element
            Element el = rep.GetElementByID(port.ParentID);
            DiagramObject obj = dia.GetDiagramObjectByID(el.ElementID, "");

            // check if lower limit element is crossed
            int lowerLimit = obj.bottom + 12;// limit cross over upper 
            bool isLowerLimitCrossed = false;
            foreach (DiagramObject objPort in dia.SelectedObjects)
            {
                if (objPort.bottom < lowerLimit)
                {
                    isLowerLimitCrossed = true;
                    break;
                }

            }
            // move all to left upper corner of element
            int startValueTop = obj.bottom + 8;
            int startValueLeft = obj.left + 8;
            int pos = 0;
            foreach (DiagramObject objPort in dia.SelectedObjects)
            {
                if (!isLowerLimitCrossed)
                {
                    // move to bottom
                    objPort.top = objPort.top - 10;
                    objPort.Update();
                }
                else
                {
                    // move from left to right
                    objPort.top = startValueTop;
                    objPort.left = startValueLeft + pos * 20;
                    objPort.Update();
                    pos = pos + 1;
                }

            }


            

        }
                    #endregion
                    #region moveEmbeddedUpGUI
        /// <summary>
        /// Move the selected ports up
        /// - If upper level is crossed it locates ports to top left corners.
        /// </summary>
        /// <param name="rep"></param>
        [ServiceOperation("{26F5F957-4CFD-4684-9417-305A1615460A}", "Embedded Elements up", "Select embedded elements", isTextRequired: false)]
        public static void MoveEmbeddedUpGui(Repository rep)
        {
            Diagram dia = rep.GetCurrentDiagram();
            if (dia == null) return;
            int selCount = dia.SelectedObjects.Count;
            if (selCount == 0) return;
            rep.SaveDiagram(dia.DiagramID);

            // check if port,..
            DiagramObject objPort0 = (DiagramObject)dia.SelectedObjects.GetAt(0);
            Element port = rep.GetElementByID(objPort0.ElementID);
            if (  ! EmbeddedElementTypes.Contains(port.Type) ) return;

            // get parent of embedded element
            Element el = rep.GetElementByID(port.ParentID);
            DiagramObject obj = dia.GetDiagramObjectByID(el.ElementID, "");
          

            // check if upper limit element is crossed
            int upLimit = obj.top - 10;// limit cross over upper 
            bool isUpperLimitCrossed = false;
            foreach (DiagramObject objPort in dia.SelectedObjects)
            {
                if (objPort.top > upLimit)
                {
                    isUpperLimitCrossed = true;
                    break;
                }

            }
            // move all to left upper corner of element
            int startValueTop = obj.top + 8;
            int startValueLeft = obj.left + 8;
            int pos = 0;
            foreach (DiagramObject objPort in dia.SelectedObjects) 
            {
                if (! isUpperLimitCrossed)
                {
                    // move to top
                    objPort.top = objPort.top + 10;
                    objPort.Update();
                }else 
                {
                    // move from left to right
                    objPort.top = startValueTop;
                    objPort.left = startValueLeft + pos * 20;
                    objPort.Update();
                    pos = pos + 1;
                }
               
            }


        }
        #endregion



        public static void VcControlRemove(Package pkg)
        {
            if (pkg.IsControlled)
            {
                pkg.VersionControlRemove();
                foreach (Package pkg1 in pkg.Packages)
                {
                    VcControlRemove(pkg1);
                }
            }
        }
        
        
        /// <summary>
        /// Move Feature (Attribute, Method) down. EA automatic ordering has to be disabled in the configuration
        /// </summary>
        /// [ServiceOperation("{F106662F-F18F-4D33-AAAA-4FC9F3246B47}", "Move Feature (Attribute, Method) down", "Select Feature (Attribute, Method)", isTextRequired: false)]
        public static void FeatureUp(Repository rep)
        {
            switch (rep.GetContextItemType())
            {
                case ObjectType.otAttribute:
                    EA.Attribute findAttribute = (EA.Attribute)rep.GetContextObject();
                    Element el = rep.GetElementByID(findAttribute.ParentID);
                    int lfdNr = 1;
                    EA.Attribute lastAttribute = null;
                    foreach (EA.Attribute a in el.Attributes)
                    {
                        a.Pos = lfdNr;
                        a.Update();
                        if (a.AttributeID == findAttribute.AttributeID)
                        {
                            if (lastAttribute != null)
                            {
                                lastAttribute.Pos = lfdNr + 1;
                                lastAttribute.Update();
                            }
                        }
                        lastAttribute = a;
                        lfdNr += 1;
                    }
                    el.Attributes.Refresh();
                    el.Update();
                    rep.ShowInProjectView(findAttribute);
                    break;
                // handle methods
                case ObjectType.otMethod:
                    Method findMethod = (Method)rep.GetContextObject();
                    Element elMethods = rep.GetElementByID(findMethod.ParentID);
                    int lfdNrMethod = 1;
                    Method lastMethod = null;
                    foreach (Method m in elMethods.Methods)
                    {
                        m.Pos = lfdNrMethod;
                        m.Update();
                        if (m.MethodID == findMethod.MethodID)
                        {
                            if (lastMethod != null)
                            {
                                lastMethod.Pos = lfdNrMethod + 1;
                                lastMethod.Update();
                            }
                        }
                        lastMethod = m;
                        lfdNrMethod += 1;
                    }
                    elMethods.Methods.Refresh();
                    elMethods.Update();
                    rep.ShowInProjectView(findMethod);
                    break;
            }
        }
        [ServiceOperation("{63618EE6-BA1D-41AD-98C4-4B53B9E19F51}", "Update CallOperation Action", "Select Action", isTextRequired: false)]
        public static bool UpdateAction(Repository rep)
        {
            switch (rep.GetContextItemType())
            {
                case ObjectType.otElement:
                    Element el = (Element)rep.GetContextObject();
                    if (el.Type != "Action") return true;



                    string methodName = CallOperationAction.GetMethodNameFromCallString(el.Name);
                    // Operation Find
                    Method m = CallOperationAction.GetMethodFromMethodName(rep, methodName, isNoExtern: true) ??
                                  CallOperationAction.GetMethodFromMethodName(rep, methodName, isNoExtern: false);
                    if (m == null) return true;

                    Diagram diaCurrent = rep.GetCurrentDiagram();
                    if (diaCurrent == null) return true;
                    var eaDia = new EaDiagram(rep);
                    rep.SaveDiagram(diaCurrent.DiagramID);

                    // try to make a callBehavior
                    hoUtils.CustomProperty customProperty = new hoUtils.CustomProperty(rep, el);
                    // no custom property available
                    if (customProperty.GetValueEx("kind") == "")
                    {
                        customProperty.Create("kind", "ActionKind","CallOperation");
                    }
                    else
                    {
                        customProperty.Update("kind", "ActionKind", "CallOperation");
                    }
                    CallOperationAction.SetClassifierId(rep, el, m.MethodGUID);
                    eaDia.ReloadSelectedObjectsAndConnector();
                    break;

            }
            return true;
        }

        /// <summary>
        /// Move Feature (Attribute, Method) down. EA automatic ordering has to be disabled in the configuration
        /// </summary>
        [ServiceOperation("{F106662F-F18F-4D33-AAAA-4FC9F3246B47}", "Move Feature (Attribute, Method) down", "Select Feature (Attribute, Method)", isTextRequired: false)]
        public static void FeatureDown(Repository rep)
        {
            switch (rep.GetContextItemType())
            {
                case ObjectType.otAttribute:
                    EA.Attribute findAttribute = (EA.Attribute)rep.GetContextObject();
                    Element el = rep.GetElementByID(findAttribute.ParentID);
                    int indexFind = -1;
                    int lfdNr = 1;
                    foreach (EA.Attribute a in el.Attributes)
                    {
                        if (a.AttributeID == findAttribute.AttributeID)
                        {
                            a.Pos = lfdNr + 1;
                            indexFind = lfdNr + 1;
                        }
                        else
                        {
                            if (indexFind == lfdNr) a.Pos = lfdNr - 1;
                            else a.Pos = lfdNr;
                        }
                        a.Update();
                        lfdNr += 1;
                    }
                    el.Attributes.Refresh();
                    el.Update();
                    rep.ShowInProjectView(findAttribute);
                    break;
                // Method
                case ObjectType.otMethod:
                    Method findMethod = (Method)rep.GetContextObject();
                    Element elMethod = rep.GetElementByID(findMethod.ParentID);
                    int indexFindMethod = -1;
                    int lfdNrMethod = 1;
                    foreach (Method m in elMethod.Methods)
                    {
                        if (m.MethodID == findMethod.MethodID)
                        {
                            m.Pos = lfdNrMethod + 1;
                            indexFindMethod = lfdNrMethod + 1;
                        }
                        else
                        {
                            if (indexFindMethod == lfdNrMethod) m.Pos = lfdNrMethod - 1;
                            else m.Pos = lfdNrMethod;
                        }
                        m.Update();
                        lfdNrMethod += 1;
                    }
                    elMethod.Methods.Refresh();
                    elMethod.Update();
                    rep.ShowInProjectView(findMethod);
                    break;
            }
        }






        #region AddElementsToDiagram

        /// <summary>
        /// Add Elements (Note, Constraint,..) to diagram and link to selected Nodes in Diagram.
        /// If nothing selected add the wanted Element to the diagram.
        /// 
        /// ConnectorLinkType contains the feature to link the note to 
        /// - "" no visualization
        /// - "Element Note", "Diagram Note", "Link Notes", "Attribute", "Operation" 
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="elementType"></param>
        /// <param name="connectorLinkType"></param>
        /// <param name="bound"></param>
        public static void AddElementsToDiagram(Repository rep,
            string elementType = "Note", string connectorLinkType = "Element Note", bool bound = true)

        {
            // handle multiple selected elements
            ObjectType objectType = rep.GetContextItemType();
            Diagram diaCurrent = rep.GetCurrentDiagram();
            if (diaCurrent == null) return;
            var eaDia = new EaDiagram(rep);
            rep.SaveDiagram(diaCurrent.DiagramID);

            switch (objectType)
            {
                case ObjectType.otDiagram:
                    AddDiagramNote(rep);
                    break;
                case ObjectType.otConnector:
                    if (!String.IsNullOrWhiteSpace(connectorLinkType)) connectorLinkType = "Link Notes";
                    AddElementWithLinkToConnector(rep, diaCurrent.SelectedConnector, elementType, bound);
                    break;
                case ObjectType.otPackage:
                case ObjectType.otElement:
                    // check for selected DiagramObjects
                    var diaCurrentSelectedObjects = diaCurrent.SelectedObjects;
                    if (diaCurrentSelectedObjects?.Count > 0)
                    {
                        foreach (DiagramObject diaObj in diaCurrentSelectedObjects)
                        {
                            AddElementWithLink(rep, diaObj, elementType, connectorLinkType);
                        }
                    }
                    break;
                case ObjectType.otMethod:
                    AddFeatureWithNoteLink(rep, (Method)rep.GetContextObject(), connectorLinkType);
                    break;
                case ObjectType.otAttribute:
                    AddFeatureWithNoteLink(rep, (EA.Attribute)rep.GetContextObject(), connectorLinkType);
                    break;
            }
            eaDia.ReloadSelectedObjectsAndConnector();
        }


        /// <summary>
        /// Add Element and optionally link to  Object from:<para/>
        /// Element, Attribute, Operation, Package
        /// 
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="diaObj"></param>
        /// <param name="elementType">Default Note</param>
        /// <param name="connectorType">Default: null</param>
        private static void AddElementWithLink(Repository rep, DiagramObject diaObj,
            string elementType = @"Note", string connectorType = "Element Link")
        {
            Element el = rep.GetElementByID(diaObj.ElementID);
            if (el != null)
            {
                Diagram dia = rep.GetCurrentDiagram();
                Package pkg = rep.GetPackageByID(el.PackageID);
                if (pkg.IsProtected || dia.IsLocked || el.Locked) return;

                Element elNewElement;
                try
                {
                    elNewElement = (Element)pkg.Elements.AddNew("", elementType);
                    elNewElement.Update();
                    pkg.Update();
                }
                catch
                {
                    return;
                }

                // add element to diagram
                // "l=200;r=400;t=200;b=600;"

                int left = diaObj.right + 50;
                int right = left + 100;
                int top = diaObj.top;
                int bottom = top - 100;

                string position = "l=" + left + ";r=" + right + ";t=" + top + ";b=" + bottom + ";";
                var diaObject = (DiagramObject)dia.DiagramObjects.AddNew(position, "");
                diaObject.ElementID = elNewElement.ElementID;
                diaObject.Sequence = 1; // put element to top
                diaObject.Update();
                pkg.Elements.Refresh();
                dia.Update();

                
                // make a connector
                EA.Connector con = (EA.Connector)el.Connectors.AddNew("", "NoteLink");
                con.SupplierID = elNewElement.ElementID;
                con.Update();
                el.Connectors.Refresh();

                // Attach Element to an Feature (Note, Attribute, Operation, ..)
                if (!String.IsNullOrWhiteSpace(connectorType))
                {
                        HoUtil.SetElementHasAttachedElementLink(rep, el, elNewElement);
                    
                }
            }

        }

        #endregion

        /// <summary>
        /// Add Attribute Link to Note (Feature Link)
        /// 
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="attr"></param>
        /// <param name="connectorLinkType"></param>
        private static void AddFeatureWithNoteLink(Repository rep, EA.Attribute attr, string connectorLinkType = "")
        {
           
            string featureType = "Attribute";
            int featureId = attr.AttributeID;
            string featureName = attr.Name;
            Element elNote = rep.GetElementByID(attr.ParentID);

            SetFeatureLink(rep, elNote, featureType, featureId, featureName, connectorLinkType);
        }

        /// <summary>
        /// Add Operation Link to Note (Feature Link)
        /// 
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="op"></param>
        /// <param name="connectorLinkType"></param>
        private static void AddFeatureWithNoteLink(Repository rep, Method op, string connectorLinkType = "")
        {

            string featureType = "Operation";
            int featureId = op.MethodID;
            string featureName = op.Name;
            Element elNote = rep.GetElementByID(op.ParentID);

            SetFeatureLink(rep, elNote, featureType, featureId, featureName, connectorLinkType);
        }

        /// <summary>
        /// Set Link Feature to Note. The link is stored inside the note object.
        /// - Attribute
        /// - Operation
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="elNote"></param>
        /// <param name="featureType"></param>
        /// <param name="featureId"></param>
        /// <param name="featureName"></param>
        /// <param name="connectorLinkType"></param>
        private static void SetFeatureLink(Repository rep, Element elNote,  string featureType,
            int featureId, string featureName, string connectorLinkType = "")
        {
            string connectorType = "NoteLink";

            if (elNote != null)
            {
                Diagram dia = rep.GetCurrentDiagram();
                Package pkg = rep.GetPackageByID(elNote.PackageID);
                if (pkg.IsProtected || dia.IsLocked || elNote.Locked) return;

                Element elNewNote;
                try
                {
                    elNewNote = (Element) pkg.Elements.AddNew("", "Note");
                    elNewNote.Update();
                    pkg.Update();
                }
                catch
                {
                    return;
                }

                // add element to diagram
                // "l=200;r=400;t=200;b=600;"
                DiagramObject diaObj = dia.GetDiagramObjectByID(elNote.ElementID, "");
                int left = diaObj.right + 50;
                int right = left + 100;
                int top = diaObj.top;
                int bottom = top - 100;

                string position = "l=" + left + ";r=" + right + ";t=" + top + ";b=" + bottom + ";";
                var diaObject = (DiagramObject) dia.DiagramObjects.AddNew(position, "");
                diaObject.ElementID = elNewNote.ElementID;
                diaObject.Sequence = 1; // put element to top
                diaObject.Update();
                pkg.Elements.Refresh();
                dia.Update();

                // connect Element to node
                if (!String.IsNullOrWhiteSpace(connectorType))
                {
                    // make a connector
                    EA.Connector con = (EA.Connector) elNote.Connectors.AddNew("", "NoteLink");
                    con.SupplierID = elNewNote.ElementID;
                    con.Update();
                    elNote.Connectors.Refresh();


                    // Lint to a feature
                    if (!String.IsNullOrWhiteSpace(connectorLinkType))
                        // set attached link to feature (Attribute/Operation)
                        HoUtil.SetElementLink(rep, elNewNote.ElementID, featureType, featureId, featureName, "Yes", 0);

                }
            }
        }

        #region AddElementWithLinkToConnector

        /// <summary>
        /// Add Element and optionally link to  Object from:<para/>
        /// Element, Attribute, Operation, Package
        /// 
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="con"></param>
        /// <param name="elementType">Default Note</param>
        /// <param name="bound"></param>
        private static void AddElementWithLinkToConnector(Repository rep, 
            EA.Connector con,
            string elementType = @"Note", 
            bool bound = true)
        {
            Diagram dia = rep.GetCurrentDiagram();
            Package pkg = rep.GetPackageByID(dia.PackageID);
            if (pkg.IsProtected || dia.IsLocked) return;

            Element elNewElement;
            try
            {
                elNewElement = (Element)pkg.Elements.AddNew("", elementType);
                elNewElement.Update();
                pkg.Update();
            }
            catch
            {
                return;
            }

            Element sourceEl = rep.GetElementByID(con.SupplierID);
            rep.GetElementByID(con.ClientID);
            DiagramObject sourceObj = dia.GetDiagramObjectByID(sourceEl.ElementID, "");

            // add element to diagram
            // "l=200;r=400;t=200;b=600;"

            int left = sourceObj.right + 50;
            int right = left + 100;
            int top = sourceObj.top;
            int bottom = top - 100;

            string position = "l=" + left + ";r=" + right + ";t=" + top + ";b=" + bottom + ";";
            var diaObject = (DiagramObject)dia.DiagramObjects.AddNew(position, "");
            dia.Update();
            diaObject.ElementID = elNewElement.ElementID;
            diaObject.Sequence = 1; // put element to top
            diaObject.Update();
            pkg.Elements.Refresh();

            // link to to connector, bout to notes text or not
            HoUtil.SetElementHasAttachedConnectorLink(rep, con, elNewElement, bound);
            elNewElement.Refresh();
            diaObject.Update();
            dia.Update();
            pkg.Elements.Refresh();
        }

        #endregion


        private static DiagramObject GetDiagramObjectFromElement(Element el, Diagram dia)
        {
            // get the position of the Element
            DiagramObject diaObj = null;
            foreach (DiagramObject dObj in dia.DiagramObjects)
            {
                if (dObj.ElementID == el.ElementID)
                {
                    diaObj = dObj;
                    break;
                }
            }
            return diaObj;
        }

        
       


    }
}
