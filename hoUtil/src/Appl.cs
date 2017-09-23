using System.Collections.Generic;
using System.Windows.Forms;
using EA;
using hoReverse.hoUtils.SQL;


// ReSharper disable once CheckNamespace
namespace hoReverse.hoUtils.Appls
{
    public class Appl
    {
        

        /// <summary>
        /// Get the Behavior for an Operation. The behavior is stored in:
        /// .Behavior '{ea_guid}' of classifier which is the behavior
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="method"></param>
        /// <returns>Behavior</returns>
        public static EA.Element GetBehaviorForOperation(Repository repository, Method method)
        {
            EA.Element returnValue = null;
            string behavior = method.Behavior;
            if (behavior.StartsWith("{") & behavior.EndsWith("}"))
            {
                // get object according to behavior
                EA.Element el = repository.GetElementByGuid(behavior);
                // Behavior found
                if (el != null) returnValue = el;
                else behavior = "";
            }
            // try to establish link to behavior.
            if (behavior == "")
            {
                string colName = "guid_activity";
                string query =
                    $@"select ea_guid As [{colName}] from t_object where name = '{method.Name}' and Object_Type = 'Activity'";

                UtilSql sql = new UtilSql(repository);
                List<string> lActivityGuid = sql.GetListOfStringFromSql(query, colName);
                if (lActivityGuid.Count == 1)
                {
                    // get Behavior
                    behavior = lActivityGuid[0];
                    returnValue = repository.GetElementByGuid(behavior);
                    if (returnValue == null)
                    {
                        MessageBox.Show(
                            $"Operation: '{method.Name}' has no possible Activity, can't establish a link from Operation to Activity",
                            "Can't link Activity to operation");
                    }
                    else
                    {
                        method.Behavior = behavior;
                        method.Update();
                    }

                }
                else
                {
                    MessageBox.Show($@"Operation: '{method.Name}' has {lActivityGuid.Count} possible Activities, can't establish a link from Operation to Activity.
This can be because of macros and multiple implementations of the operetion.",
                                                "Can't link Activity to operation, stereotype?");
                }

            }
            return returnValue;
        }

        public static void DisplayBehaviorForOperation(Repository repository, Method method)
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
                        HoUtil.OpenBehaviorForElement(repository, el);
                    }
                }
            }
        }
        
        public static bool CreateInteractionForOperation(EA.Repository rep, EA.Method m)
        {
            // get class
            EA.Element elClass = rep.GetElementByID(m.ParentID);
            Package pkgSrc = rep.GetPackageByID(elClass.PackageID);

            // create a package with the name of the operation
            Package pkgTrg = (Package)pkgSrc.Packages.AddNew(m.Name, "");
            pkgTrg.Update();
            pkgSrc.Packages.Refresh();

            // create Class Sequence Diagram in target package
            EA.Diagram pkgSeqDia = (EA.Diagram)pkgTrg.Diagrams.AddNew("Operation:" + m.Name + " Content", "Sequence");
            pkgSeqDia.Update();
            pkgTrg.Diagrams.Refresh();

            // add frame in Sequence diagram
            EA.DiagramObject frmObj = (EA.DiagramObject)pkgSeqDia.DiagramObjects.AddNew("l=100;r=400;t=25;b=50", "");
            EA.Element frm = (EA.Element)pkgTrg.Elements.AddNew(m.Name, "UMLDiagram");
            frm.Update();
            frmObj.ElementID = frm.ElementID;
            //frmObj.Style = "fontsz=200;pitch=34;DUID=265D32D5;font=Arial Narrow;bold=0;italic=0;ul=0;charset=0;";
            frmObj.Update();
            pkgTrg.Elements.Refresh();
            pkgSeqDia.DiagramObjects.Refresh();


            // create Interaction with the name of the operation
            EA.Element seq = (EA.Element)pkgTrg.Elements.AddNew(m.Name, "Interaction");
            seq.Notes = "Generated from Operation:\r\n" + m.Visibility + " " + m.Name + ":" + m.ReturnType + ";\r\nDetails see Operation definition!!";
            seq.Update();
            pkgTrg.Elements.Refresh();

            // create sequence diagram beneath Interaction
            EA.Diagram seqDia = (EA.Diagram)seq.Diagrams.AddNew(m.Name, "Sequence");
            seqDia.Update();
            seq.Diagrams.Refresh();

            // create instance from class beneath Interaction
            EA.Element obj = (EA.Element)seq.Elements.AddNew("", "Object");
            seq.Elements.Refresh();
            obj.ClassfierID = elClass.ElementID;
            obj.Update();

            // add node object to Sequence Diagram  
            EA.DiagramObject node = (EA.DiagramObject)seqDia.DiagramObjects.AddNew("l=100;r=180;t=50;b=70", "");
            node.ElementID = obj.ElementID;
            node.Update();


            // Add Heading to diagram
            EA.DiagramObject noteObj = (EA.DiagramObject)seqDia.DiagramObjects.AddNew("l=40;r=700;t=10;b=25", "");
            EA.Element note = (EA.Element)pkgTrg.Elements.AddNew("Text", "Text");

            note.Notes = m.Visibility + " " + elClass.Name + "_" + m.Name + ":" + m.ReturnType;
            note.Update();
            noteObj.ElementID = note.ElementID;
            noteObj.Style = "fontsz=200;pitch=34;DUID=265D32D5;font=Arial Narrow;bold=0;italic=0;ul=0;charset=0;";
            noteObj.Update();
            pkgTrg.Elements.Refresh();
            seqDia.DiagramObjects.Refresh();


            // Link Operation to activity
            HoUtil.SetBehaviorForOperation(rep, m, seq);

            // Set show behavior
            HoUtil.SetShowBehaviorInDiagram(rep, m);

            

            HoUtil.SetFrameLinksToDiagram(rep, frm, seqDia); // link Overview frame to diagram
            frm.Update();
            //rep.ReloadDiagram(actDia.DiagramID);


            return true;
        }

        //------------------------------------------------------------------------------
        // Create default Elements for Statemachine
        //------------------------------------------------------------------------------
        //
        // init
        // state 'State1'
        // final
        // transition from init to 'State1'

        public static bool CreateDefaultElementsForStateDiagram(Repository rep, EA.Diagram dia, EA.Element stateChart)
        {

            // check if init and final node are available
            bool init = false;
            bool final = false;
            foreach (EA.Element node in stateChart.Elements)
            {
                if (node.Type == "StateNode" & node.Subtype == 100) init = true;
                if (node.Type == "StateNode" & node.Subtype == 101) final = true;
            }
            EA.Element initNode = null;
            if (!init)
            {
                initNode = (EA.Element) stateChart.Elements.AddNew("", "StateNode");
                initNode.Subtype = 3;
                initNode.ParentID = stateChart.ElementID;
                initNode.Update();
                if (dia != null)
                {
                    HoUtil.AddSequenceNumber(rep, dia);
                    EA.DiagramObject initDiaNode =
                        (EA.DiagramObject) dia.DiagramObjects.AddNew("l=295;r=315;t=125;b=135;", "");
                    initDiaNode.Sequence = 1;
                    initDiaNode.ElementID = initNode.ElementID;
                    initDiaNode.Update();
                    HoUtil.SetSequenceNumber(rep, dia, initDiaNode, "1");
                }

            }
            EA.Element finalNode = null;
            // create final node
            if (!final)
            {
                finalNode = (EA.Element) stateChart.Elements.AddNew("", "StateNode");
                finalNode.Subtype = 4;
                finalNode.ParentID = stateChart.ElementID;
                finalNode.Update();
                if (dia != null)
                {
                    HoUtil.AddSequenceNumber(rep, dia);
                    EA.DiagramObject finalDiaNode =
                        (EA.DiagramObject) dia.DiagramObjects.AddNew("l=285;r=305;t=745;b=765;", "");
                    finalDiaNode.Sequence = 1;
                    finalDiaNode.ElementID = finalNode.ElementID;
                    finalDiaNode.Update();
                    HoUtil.SetSequenceNumber(rep, dia, finalDiaNode, "1");
                }
            }
            // create state node
            EA.Element stateNode = (EA.Element) stateChart.Elements.AddNew("", "State");
            stateNode.Subtype = 0; // state
            stateNode.Name = "State1";
            stateNode.ParentID = stateChart.ElementID;
            stateNode.Update();
            if (dia != null)
            {
                HoUtil.AddSequenceNumber(rep, dia);
                string pos = "l=300;r=400;t=-400;b=-470";
                EA.DiagramObject stateDiaNode = (EA.DiagramObject) dia.DiagramObjects.AddNew(pos, "");
                stateDiaNode.Sequence = 1;
                stateDiaNode.ElementID = stateNode.ElementID;
                stateDiaNode.Update();
                HoUtil.SetSequenceNumber(rep, dia, stateDiaNode, "1");

                // draw a transition
                EA.Connector con = (EA.Connector) finalNode.Connectors.AddNew("", "StateFlow");
                con.SupplierID = stateNode.ElementID;
                con.ClientID = initNode.ElementID;
                con.Update();
                finalNode.Connectors.Refresh();
                stateChart.Elements.Refresh();
                dia.DiagramObjects.Refresh();
                dia.Update();
                rep.ReloadDiagram(dia.DiagramID);
        }

        return true;
    }
        //-----------------------------------------------------------------------------------------
        // Create StateMachine for Operation
        //----------------------------------------------------------------------------------
        public static bool CreateStateMachineForOperation(EA.Repository rep, EA.Method m)
        {
            // get class
            EA.Element elClass = rep.GetElementByID(m.ParentID);
            EA.Package pkgSrc = rep.GetPackageByID(elClass.PackageID);

            // create a package with the name of the operation
            EA.Package pkgTrg = (Package)pkgSrc.Packages.AddNew(m.Name, "");
            pkgTrg.Update();
            pkgSrc.Packages.Refresh();

            // create Class StateMachine Diagram in target package
            EA.Diagram pkgSeqDia = (EA.Diagram)pkgTrg.Diagrams.AddNew("Operation:" + m.Name + " Content", "Statechart");
            pkgSeqDia.Update();
            pkgTrg.Diagrams.Refresh();

            // add frame in StateMachine diagram
            EA.DiagramObject frmObj = (EA.DiagramObject)pkgSeqDia.DiagramObjects.AddNew("l=100;r=400;t=25;b=50", "");
            EA.Element frm = (EA.Element)pkgTrg.Elements.AddNew(m.Name, "UMLDiagram");
            frm.Update();
            frmObj.ElementID = frm.ElementID;
            //frmObj.Style = "fontsz=200;pitch=34;DUID=265D32D5;font=Arial Narrow;bold=0;italic=0;ul=0;charset=0;";
            frmObj.Update();
            pkgTrg.Elements.Refresh();
            pkgSeqDia.DiagramObjects.Refresh();


            // create StateMachine with the name of the operation
            EA.Element stateMachine = (EA.Element)pkgTrg.Elements.AddNew(m.Name, "StateMachine");
            stateMachine.Notes = "Generated from Operation:\r\n" + m.Visibility + " " + m.Name + ":" + m.ReturnType + ";\r\nDetails see Operation definition!!";
            stateMachine.Update();
            pkgTrg.Elements.Refresh();

            // create Statechart diagram beneath State Machine
            EA.Diagram chartDia = (EA.Diagram)stateMachine.Diagrams.AddNew(m.Name, "Statechart");
            chartDia.Update();
            stateMachine.Diagrams.Refresh();

            // put the state machine on the diagram
            EA.DiagramObject chartObj = (EA.DiagramObject)chartDia.DiagramObjects.AddNew("l=50;r=600;t=100;b=800", "");
            chartObj.ElementID = stateMachine.ElementID;
            chartObj.Update();
            chartDia.DiagramObjects.Refresh();

            // add default nodes (init/final)
            CreateDefaultElementsForStateDiagram(rep, chartDia, stateMachine);

            // Add Heading to diagram
            EA.DiagramObject noteObj = (EA.DiagramObject)chartDia.DiagramObjects.AddNew("l=40;r=700;t=10;b=25", "");
            EA.Element note = (EA.Element)pkgTrg.Elements.AddNew("Text", "Text");

            note.Notes = m.Visibility + " " + elClass.Name + "_" + m.Name + ":" + m.ReturnType;
            note.Update();
            noteObj.ElementID = note.ElementID;
            noteObj.Style = "fontsz=200;pitch=34;DUID=265D32D5;font=Arial Narrow;bold=0;italic=0;ul=0;charset=0;";
            noteObj.Update();
            pkgTrg.Elements.Refresh();
            chartDia.DiagramObjects.Refresh();


            // Link Operation to StateMachine
            HoUtil.SetBehaviorForOperation(rep, m, stateMachine);

            // Set show behavior
            HoUtil.SetShowBehaviorInDiagram(rep, m);



            HoUtil.SetFrameLinksToDiagram(rep, frm, chartDia); // link Overview frame to diagram
            frm.Update();
            //rep.ReloadDiagram(actDia.DiagramID);


            return true;
        }
   
    }
}
