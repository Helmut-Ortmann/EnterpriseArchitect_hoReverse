using System;
using hoUtil;

namespace Appls
{
    class Appl
    {
        private static bool CreateDefaultElementsForActivity(EA.Repository rep, EA.Diagram dia, EA.Element act)
        {

            // check if init and final node are available
            bool init = false;
            bool final = false;
            foreach (EA.Element node in act.Elements)
            {
                if (node.Type == "StateNode" & node.Subtype == 100) init = true;
                if (node.Type == "StateNode" & node.Subtype == 101) final = true;
            }
            if (!init)
            {
                EA.Element initNode = (EA.Element)act.Elements.AddNew("", "StateNode");
                initNode.Subtype = 100;
                initNode.Update();
                if (dia != null)           {
                    EA.DiagramObject initDiaNode = (EA.DiagramObject)dia.DiagramObjects.AddNew("l=295;r=315;t=125;b=135;", "");
                    initDiaNode.ElementID = initNode.ElementID;
                    initDiaNode.Update();
                }

            }
            if (!final)
            {
                EA.Element finalNode = (EA.Element)act.Elements.AddNew("", "StateNode");
                finalNode.Subtype = 101;
                finalNode.Update();
                if (dia != null)           {
                    EA.DiagramObject finalDiaNode = (EA.DiagramObject)dia.DiagramObjects.AddNew("l=285;r=305;t=745;b=765;", "");
                finalDiaNode.ElementID = finalNode.ElementID;
                finalDiaNode.Update();
                }
            }
            act.Elements.Refresh();
            dia.DiagramObjects.Refresh();
            dia.Update();
            rep.ReloadDiagram(dia.DiagramID);

            return true;
        }
        //-------------------------------------------------------------------------------------------------
        // get Parameter from operation
        // visualize them on diagram / activity
        //-------------------------------------------------------------------------------------------------
        private static bool GetParameterFromOperation (EA.Repository rep,    EA.Element act, EA.Method m)
        {
            if (m == null) return false;
            if (act.Locked) return false;
            if (!act.Type.Equals("Activity")) return false;

            EA.Element parTrgt = null;

            // If diagram is active visualize on diagram
            EA.Diagram curDia = rep.GetCurrentDiagram();
                            
            // delete all old "Activity Paramaeter"
            if (act.EmbeddedElements.Count > 0)
            {
                for (short i = (short)(act.EmbeddedElements.Count - 1); i >= 0; --i)
                {
                    EA.Element embeddedEl = (EA.Element)act.EmbeddedElements.GetAt(i);
                    if (embeddedEl.Type.Equals("ActivityParameter"))
                    {
                        act.EmbeddedElements.Delete(i);
                    }
                }
                act.EmbeddedElements.Refresh();
                act.Update();
            }
                            
            // return code
            if (!m.ReturnType.Equals("void"))
            {
                // create an Parameter for Activity (in fact an element with properties)
                parTrgt = (EA.Element)act.EmbeddedElements.AddNew("Return", "Parameter");
                parTrgt.Alias = "return:" + m.ReturnType;
                if (m.ClassifierID != "") parTrgt.ClassifierID = Convert.ToInt32(m.ClassifierID);
                parTrgt.Update();
                act.EmbeddedElements.Refresh();


                if (curDia != null)
                {
                    // "l=120;r=145;t=785;b=805", "");
                    EA.DiagramObject diaObj = (EA.DiagramObject)curDia.DiagramObjects.AddNew("l=140;t=780;", "");
                    diaObj.ElementID = parTrgt.ElementID;
                    diaObj.Update();
                    curDia.DiagramObjects.Refresh();
                    rep.ReloadDiagram(curDia.DiagramID);
                }

            }

            // over all parameters
            int top = 300;
            int left = 45;
            foreach (EA.Parameter parSrc in m.Parameters)
            {
                // create an Parameter for Activity (in fact an element with properties)
                parTrgt = (EA.Element)act.EmbeddedElements.AddNew(parSrc.Position + "_" + parSrc.Name, "Parameter");
                // get classifier/type


                if ( parSrc.ClassifierID != "")  parTrgt.ClassifierID = Convert.ToInt32(parSrc.ClassifierID);
                parTrgt.Notes = parSrc.Notes;
                parTrgt.Alias = "par_" + parSrc.Position + ":" + parSrc.Type;
                parTrgt.Update();

                // If diagram is active visualize on diagram
                if (curDia != null)
                {
                    // "l=120;r=145;t=785;b=805", "");
                    EA.DiagramObject diaObj = (EA.DiagramObject)curDia.DiagramObjects.AddNew($"l={left};t={top};", "");
                    diaObj.ElementID = parTrgt.ElementID;
                    diaObj.Update();
                    curDia.DiagramObjects.Refresh();
                    rep.ReloadDiagram(curDia.DiagramID);
                    top = top + 50;
                }
            }
            act.EmbeddedElements.Refresh();

        return true;
        }
        public static bool CreateInteractionForOperation(EA.Repository rep, EA.Method m)
        {
            // get class
            EA.Element elClass = rep.GetElementByID(m.ParentID);
            EA.Package pkgSrc = rep.GetPackageByID(elClass.PackageID);

            // create a package with the name of the operation
            EA.Package pkgTrg = (EA.Package)pkgSrc.Packages.AddNew(m.Name, "");
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
            Util.SetBehaviorForOperation(rep, m, seq);

            // Set show behavior
            Util.SetShowBehaviorInDiagram(rep, m);

            

            Util.SetFrameLinksToDiagram(rep, frm, seqDia); // link Overview frame to diagram
            frm.Update();
            //rep.ReloadDiagram(actDia.DiagramID);


            return true;
        }


        public static bool CreateActivityForOperation(EA.Repository rep, EA.Method m)
        {
            // get class
            EA.Element elClass = rep.GetElementByID(m.ParentID);
            EA.Package pkgSrc = rep.GetPackageByID(elClass.PackageID);

            // create a package with the name of the operation
            EA.Package pkgTrg = (EA.Package)pkgSrc.Packages.AddNew(m.Name, "");
            pkgTrg.Update();
            pkgSrc.Packages.Refresh();

            // create Class Activity Diagram in target package
            EA.Diagram pkgActDia = (EA.Diagram)pkgTrg.Diagrams.AddNew("Operation:" + m.Name + " Content", "Activity");
            pkgActDia.Update();
            pkgTrg.Diagrams.Refresh();

            // add frame in Activity diagram
            EA.DiagramObject frmObj = (EA.DiagramObject)pkgActDia.DiagramObjects.AddNew("l=100;r=400;t=25;b=50", "");
            EA.Element frm = (EA.Element)pkgTrg.Elements.AddNew(m.Name, "UMLDiagram");
            frm.Update();
            frmObj.ElementID = frm.ElementID;
            //frmObj.Style = "fontsz=200;pitch=34;DUID=265D32D5;font=Arial Narrow;bold=0;italic=0;ul=0;charset=0;";
            frmObj.Update();
            pkgTrg.Elements.Refresh();
            pkgActDia.DiagramObjects.Refresh();


            // create activity with the name of the operation
            EA.Element act = (EA.Element)pkgTrg.Elements.AddNew(m.Name, "Activity");
            act.Notes = "Generated from Operation:\r\n" + m.Visibility + " " + m.Name + ":" + m.ReturnType + ";\r\nDetails see Operation definition!!";
            act.Update();
            pkgTrg.Elements.Refresh();

            // create activity diagram beneath Activity
            EA.Diagram actDia = (EA.Diagram)act.Diagrams.AddNew(m.Name, "Activity");
            actDia.Update();
            act.Diagrams.Refresh();




            // put the activity on the diagram
            EA.DiagramObject actObj = (EA.DiagramObject)actDia.DiagramObjects.AddNew("l=50;r=600;t=100;b=800", "");
            actObj.ElementID = act.ElementID;
            actObj.Update();
            actDia.DiagramObjects.Refresh();

            // add default nodes (init/final)
            CreateDefaultElementsForActivity(rep, actDia, act);

            // Add Heading to diagram
            EA.DiagramObject noteObj = (EA.DiagramObject)actDia.DiagramObjects.AddNew("l=40;r=700;t=25;b=50", "");
            EA.Element note = (EA.Element)pkgTrg.Elements.AddNew("Text", "Text");

            note.Notes = m.Visibility + " " + elClass.Name + "_" + m.Name + ":" + m.ReturnType;
            note.Update();
            noteObj.ElementID = note.ElementID;
            noteObj.Style = "fontsz=200;pitch=34;DUID=265D32D5;font=Arial Narrow;bold=0;italic=0;ul=0;charset=0;";
            noteObj.Update();
            pkgTrg.Elements.Refresh();
            actDia.DiagramObjects.Refresh();


            // Link Operation to activity
            Util.SetBehaviorForOperation(rep, m, act);

            // Set show behavior
            Util.SetShowBehaviorInDiagram(rep, m);

            // add parameters to activity
            GetParameterFromOperation(rep, act, m);

            Util.SetFrameLinksToDiagram(rep, frm, actDia); // link Overview frame to diagram
            frm.Update();
            //rep.ReloadDiagram(actDia.DiagramID);
        

            return true;
        }
    }
}
