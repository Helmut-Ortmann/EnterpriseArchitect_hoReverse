using System;
using System.Windows.Forms;
using EA;
using hoReverse.hoUtils.Parameter;

// ReSharper disable once CheckNamespace
namespace hoReverse.hoUtils.ActivityParameter
{
    //--------------------------------------------------------------------------------------------
    // Create / Update Activity Parameter from Operation
    //--------------------------------------------------------------------------------------------

    public static class ActivityPar
    {
        //------------------------------------------------------------------------------
        // Create default Elements for Activity
        //------------------------------------------------------------------------------
        //
        // init
        // final
        private static readonly bool ActivityIsSimple = true; // Create Activity from function in the simple form

        // ReSharper disable once UnusedMethodReturnValue.Local
        private static bool CreateDefaultElementsForActivity(EA.Repository rep, 
                                EA.Diagram dia, EA.Element act)
        {
            // create init node
            CreateInitFinalNode(rep, dia, act, 100, "l=350;r=370;t=70;b=90;");
            act.Elements.Refresh();
            dia.DiagramObjects.Refresh();
            dia.Update();
            rep.ReloadDiagram(dia.DiagramID);

            return true;
        }

        // subtype=100 init node
        // subtype=101 final node

        public static EA.DiagramObject CreateInitFinalNode(EA.Repository rep, EA.Diagram dia, EA.Element act, 
                                      int subType, string position)
        {
            EA.Element initNode = (EA.Element)act.Elements.AddNew("", "StateNode");
            initNode.Subtype = subType;
            initNode.Update();
            if (dia != null)
            {
                HoUtil.AddSequenceNumber(rep, dia);
                EA.DiagramObject initDiaNode = (EA.DiagramObject)dia.DiagramObjects.AddNew(position,"");
                initDiaNode.ElementID = initNode.ElementID;
                initDiaNode.Sequence = 1;
                initDiaNode.Update();
                HoUtil.SetSequenceNumber(rep, dia, initDiaNode, "1");
                return initDiaNode;
            }
            return null;
        }
        /// <summary>
        ///  Create/Update an Activity Diagram for the operation
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="m"></param>
        /// <param name="treePos">If a new package is created the new tree position to show the package in the correct order</param>
        /// <returns></returns>
        public static bool CreateActivityForOperation(EA.Repository rep, EA.Method m, int treePos=100)
        {
            // get class
            EA.Element elClass = rep.GetElementByID(m.ParentID);
            EA.Package pkgSrc = rep.GetPackageByID(elClass.PackageID);

            // Check if update behavior, behavior exists
            string behaviorGuid = m.Behavior;

            // Check if update Activity or Create a new one
            if (behaviorGuid.StartsWith("{") && behaviorGuid.EndsWith("}") )
            {
                //behaviorGuid = behaviorGuid.Substring(1, behaviorGuid.Length-2);
                EA.Element actForUpdate = rep.GetElementByGuid(behaviorGuid);
                if (actForUpdate == null)
                {
                    var res = MessageBox.Show($@"Can't update Activity for Operation, invalid link to Activity found

Behavior GUID =GUID of expected Activity:
{behaviorGuid}

Unable to find Activity for this GUID",
                        @"Invalid link to Activity found, Create a new one?", 
                        MessageBoxButtons.OKCancel);
                    if (res != DialogResult.OK) return false;
                    CreateActivityFromOperation(rep, m, treePos, pkgSrc, elClass);

                }
                else
                    UpdateParameterFromOperation(rep, actForUpdate, m); // update parameters from Operation for Activity
                return true;
            }
            CreateActivityFromOperation(rep, m, treePos, pkgSrc, elClass);
            return true;
        }

        /// <summary>
        /// Create Activity for operation
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="m"></param>
        /// <param name="treePos"></param>
        /// <param name="pkgSrc"></param>
        /// <param name="elClass"></param>
        private static void CreateActivityFromOperation(Repository rep, Method m, int treePos, Package pkgSrc, Element elClass)
        {
            // create a package with the name of the operation
            Package pkgTrg = (Package) pkgSrc.Packages.AddNew(m.Name, "");
            pkgTrg.TreePos = treePos;
            pkgTrg.Update();
            pkgSrc.Packages.Refresh();

            EA.Element frm = null; // frame beneath package
            if (ActivityIsSimple == false)
            {
                // create Class Activity Diagram in target package
                EA.Diagram pkgActDia = (EA.Diagram) pkgTrg.Diagrams.AddNew("Operation:" + m.Name + " Content", "Activity");
                pkgActDia.Update();
                pkgTrg.Diagrams.Refresh();
                // prevent information loss
                HoUtil.SetDiagramStyleFitToPage(pkgActDia); // after save diagram!

                // add frame in Activity diagram
                EA.DiagramObject frmObj = (EA.DiagramObject) pkgActDia.DiagramObjects.AddNew("l=100;r=400;t=25;b=50", "");
                frm = (EA.Element) pkgTrg.Elements.AddNew(m.Name, "UMLDiagram");
                frm.Update();
                frmObj.ElementID = frm.ElementID;
                //frmObj.Style = "fontsz=200;pitch=34;DUID=265D32D5;font=Arial Narrow;bold=0;italic=0;ul=0;charset=0;";
                frmObj.Update();
                pkgTrg.Elements.Refresh();
                pkgActDia.DiagramObjects.Refresh();
            }

            // create activity with the name of the operation
            EA.Element act = (EA.Element) pkgTrg.Elements.AddNew(m.Name, "Activity");
            if (ActivityIsSimple == false)
            {
                act.Notes = "Generated from Operation:\r\n" + m.Visibility + " " + m.Name + ":" + m.ReturnType +
                            ";\r\nDetails see Operation definition!!";
            }

            act.StereotypeEx = m.StereotypeEx;
            act.Update();
            pkgTrg.Elements.Refresh();

            // create activity diagram beneath Activity
            EA.Diagram actDia = (EA.Diagram) act.Diagrams.AddNew(m.Name, "Activity");
            // update diagram properties
            actDia.ShowDetails = 0; // hide details
            // scale page to din A4

            if (actDia.StyleEx.Length > 0)
            {
                actDia.StyleEx = actDia.StyleEx.Replace("HideQuals=0", "HideQuals=1"); // hide qualifier
            }
            else
            {
                actDia.StyleEx = "HideQuals=1;";
            }

            // Hide Qualifier
            if (actDia.ExtendedStyle.Length > 0)
            {
                actDia.ExtendedStyle = actDia.ExtendedStyle.Replace("ScalePI=0", "ScalePI=1");
            }
            else
            {
                actDia.ExtendedStyle = "ScalePI=1;";
            }

            actDia.Update();
            act.Diagrams.Refresh();


            // put the activity on the diagram
            HoUtil.AddSequenceNumber(rep, actDia);
            EA.DiagramObject actObj = (EA.DiagramObject) actDia.DiagramObjects.AddNew("l=30;r=780;t=30;b=1120", "");
            actObj.ElementID = act.ElementID;
            actObj.Sequence = 1;
            actObj.Update();
            HoUtil.SetSequenceNumber(rep, actDia, actObj, "1");
            actDia.DiagramObjects.Refresh();

            // add default nodes (init/final)
            CreateDefaultElementsForActivity(rep, actDia, act);

            if (ActivityIsSimple == false)
            {
                // Add Heading to diagram
                HoUtil.AddSequenceNumber(rep, actDia);
                EA.DiagramObject noteObj = (EA.DiagramObject) actDia.DiagramObjects.AddNew("l=40;r=700;t=25;b=50", "");
                EA.Element note = (EA.Element) pkgTrg.Elements.AddNew("Text", "Text");

                note.Notes = m.Visibility + " " + elClass.Name + "_" + m.Name + ":" + m.ReturnType;
                note.Update();
                noteObj.ElementID = note.ElementID;
                noteObj.Style = "fontsz=200;pitch=34;DUID=265D32D5;font=Arial Narrow;bold=0;italic=0;ul=0;charset=0;";
                noteObj.Sequence = 1;
                noteObj.Update();
                HoUtil.SetSequenceNumber(rep, actDia, noteObj, "1");
            }

            pkgTrg.Elements.Refresh();
            actDia.DiagramObjects.Refresh();


            // Link Operation to activity
            HoUtil.SetBehaviorForOperation(rep, m, act);

            // Set show behavior
            HoUtil.SetShowBehaviorInDiagram(rep, m);

            // add parameters to activity
            UpdateParameterFromOperation(rep, act, m);
            int pos = 0;
            foreach (EA.Element actPar in act.EmbeddedElements)
            {
                if (!actPar.Type.Equals("ActivityParameter")) continue;
                HoUtil.VisualizePortForDiagramobject(rep, pos, actDia, actObj, actPar, null);
                pos = pos + 1;
            }

            if (ActivityIsSimple == false)
            {
                // link Overview frame to diagram
                HoUtil.SetFrameLinksToDiagram(rep, frm, actDia);
                frm.Update();
            }

            // select operation
            rep.ShowInProjectView(m);
        }

        /// <summary>
        /// Create an Activity diagram beneath selected Activity.
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="act"></param>
        /// <returns></returns>

        public static Diagram CreateActivityCompositeDiagram(Repository rep, EA.Element act) {
            // create activity diagram beneath Activity
            EA.Diagram actDia = (Diagram)act.Diagrams.AddNew(act.Name, "Activity");
            // update diagram properties
            actDia.ShowDetails = 0; // hide details
            // scale page to din A4
            
            if (actDia.StyleEx.Length > 0)
            {
                actDia.StyleEx = actDia.StyleEx.Replace("HideQuals=0", "HideQuals=1"); // hide qualifier
            }
            else
            {
                actDia.StyleEx = "HideQuals=1;";
            }
            // Hide Qualifier
            if (actDia.ExtendedStyle.Length > 0)
            { actDia.ExtendedStyle = actDia.ExtendedStyle.Replace("ScalePI=0", "ScalePI=1");  }
            else 
            {  
                actDia.ExtendedStyle = "ScalePI=1;";
            }
            actDia.Update();
            act.Diagrams.Refresh();

            // put the activity on the diagram
            HoUtil.AddSequenceNumber(rep, actDia);
            EA.DiagramObject actObj = (EA.DiagramObject)actDia.DiagramObjects.AddNew("l=30;r=780;t=30;b=1120", "");
            actObj.ElementID = act.ElementID;
            actObj.Update();
            actDia.DiagramObjects.Refresh();

            // add default nodes (init/final)
            CreateDefaultElementsForActivity(rep, actDia, act);
            act.Elements.Refresh();
            actDia.DiagramObjects.Refresh();
            return actDia;
        }

       
        //-------------------------------------------------------------------------------------------------
        // get Parameter from operation
        // visualize them on diagram / activity
        //-------------------------------------------------------------------------------------------------
        public static bool UpdateParameterFromOperation(Repository rep, EA.Element act, Method m)
        {
            if (m == null) return false;
            if (act.Locked) return false;
            if (!act.Type.Equals("Activity")) return false;

            EA.Element parTrgt = null;


            ///////////////////////////////////////////////////////////////////////////////////
            // return code
            string parName = "Return";
            int methodReturnTypId;

            // is type defined?
            if ((m.ClassifierID != "0") & (m.ClassifierID != ""))
            {
                methodReturnTypId = Convert.ToInt32(m.ClassifierID);
            }
            
            // type is only defined as text
            else
            {
                methodReturnTypId = Convert.ToInt32(HoUtil.GetTypeId(rep, m.ReturnType));
            }

            bool withActivityReturnParameter = false;
            if (withActivityReturnParameter)
            {
                parTrgt.ClassifierID = methodReturnTypId;
                // create an return Parameter for Activity (in fact an element with properties)
                parTrgt = HoUtil.GetParameterFromActivity(rep, null, act, true);
                if (parTrgt == null)
                {
                    parTrgt = (EA.Element)act.EmbeddedElements.AddNew(parName, "Parameter");
                }
                else { parTrgt.Name = parName; }


                parTrgt.Alias = "return:" + m.ReturnType;
                parTrgt.ClassifierID = parTrgt.ClassifierID;

                parTrgt.Update();
                // update properties for return value
                Param par = new Param(rep, parTrgt);
                par.setParameterProperties("direction", "out");
                par.save();
                par = null;
            }
            // returnType for activity
            act.ClassfierID = methodReturnTypId;
            act.Name = m.Name;

            // use stereotype of operation as stereotype for activity
            act.StereotypeEx = m.StereotypeEx;
            act.Update();
            act.EmbeddedElements.Refresh();

            // over all parameters
            string guids = "";
            // It looks as if parSrc.Position isn't reliable
            int pos = 0;
            foreach (EA.Parameter parSrc in m.Parameters)
            {
                // create an Parameter for Activity (in fact an element with properties)
                // - New if the parameter don't exists
                // - Update if the parameter exists
                // -- Update according to the parameter position

                //string direction = " [" + parSrc.Kind + "]";
                string direction = "";
                string prefixTyp = "";
                if (parSrc.IsConst) prefixTyp = " const";
                var postfixName = "";
                if (parSrc.Kind.Contains("out")) postfixName = "*";
                //parName = parSrc.Position + ":" + parSrc.Name + postfixName + prefixTyp + direction;
                parName = $"{pos}:{parSrc.Name}{postfixName}{prefixTyp}{direction}";

                // check if parameter already exists (last parameter = false)
                parTrgt = HoUtil.GetParameterFromActivity(rep, parSrc, act);



                // parameter doesn't exists
                if (parTrgt == null)
                {
                    parTrgt = (EA.Element)act.EmbeddedElements.AddNew(parName, "Parameter");
                }
                else
                {
                    parTrgt.Name = parName;
                }
                guids = guids + parTrgt.ElementGUID;

                // is type defined?
                if ((parSrc.ClassifierID != "0") & (parSrc.ClassifierID != ""))
                {
                    parTrgt.ClassifierID = Convert.ToInt32(parSrc.ClassifierID);
                }
                // type is only defined as text
                else
                {   // try to find classifier
                    parTrgt.ClassifierID = Convert.ToInt32(HoUtil.GetTypeId(rep, parSrc.Type));
                    // use type in name (no classifier found)
                    if (parTrgt.ClassifierID == 0) parTrgt.Name = parTrgt.Name + ":" + parSrc.Type;
                }

                parTrgt.Notes = parSrc.Notes;
                parTrgt.Alias = $"par_{pos}:{parSrc.Type}";
                //parTrgt.Alias = $"par_{parSrc.Position}:{parSrc.Type}";

                // update properties for parameter
                Param par = new Param(rep, parTrgt);
                par.setParameterProperties("direction", parSrc.Kind);
                if (parSrc.IsConst)  par.setParameterProperties("constant", "true");
                par.save();
                parTrgt.Update();
                pos += 1;



            }
            act.EmbeddedElements.Refresh();
            // delete all unused parameter
            for (short i = (short)(act.EmbeddedElements.Count - 1); i >= 0; --i)
            {
                EA.Element embeddedEl = (EA.Element)act.EmbeddedElements.GetAt(i);
                if (embeddedEl.Type.Equals("ActivityParameter"))
                {
                    if (! (guids.Contains(embeddedEl.ElementGUID) )) {
                        act.EmbeddedElements.Delete(i);
                        }
                }
            }
            act.EmbeddedElements.Refresh();

            return true;
        }
        public static void VisualizePortForDiagramobject(int pos, EA.Diagram dia, EA.DiagramObject diaObjSource, EA.Element port, EA.Element interf)
        {
            // check if port already exists
            foreach (EA.DiagramObject diaObj in dia.DiagramObjects)
            {
                if (diaObj.ElementID == port.ElementID) return;
            }

            // visualize ports
            int length = 6;
            // calculate target position
            int left = diaObjSource.right - length / 2;
            int right = left + length;
            int top = diaObjSource.top;

            top = top - 10 - pos * 10;
            var bottom = top - length;
            string position = $"l={left};r={right};t={top};b={bottom};";
            EA.DiagramObject diaObject = (EA.DiagramObject)dia.DiagramObjects.AddNew(position, "");
            dia.Update();
            if (port.Type.Equals("Port"))
            {
                // not showing label
                diaObject.Style = "LBL=CX=97:CY=13:OX=0:OY=0:HDN=1:BLD=0:ITA=0:UND=0:CLR=-1:ALN=0:ALT=0:ROT=0;";
            }
            else
            {

                // not showing label
                diaObject.Style = "LBL=CX=97:CY=13:OX=39:OY=3:HDN=0:BLD=0:ITA=0:UND=0:CLR=-1:ALN=0:ALT=0:ROT=0;";
            }
            diaObject.ElementID = port.ElementID;


            diaObject.Update();

            if (interf == null) return;

            // visualize interface
            EA.DiagramObject diaObject2 = (EA.DiagramObject)dia.DiagramObjects.AddNew(position, "");
            dia.Update();
            diaObject.Style = "LBL=CX=69:CY=13:OX=-69:OY=0:HDN=0:BLD=0:ITA=0:UND=0:CLR=-1:ALN=0:ALT=0:ROT=0;";
            diaObject2.ElementID = interf.ElementID;
            diaObject2.Update();

        }
    }
}
