using System;
using System.Linq;
using System.Windows.Forms;
using hoReverse.hoUtils.Extension;


// ReSharper disable once CheckNamespace
namespace hoReverse.hoUtils
{
    public static class Activity
    {
        /// <summary>
        /// Find Activity from method name.
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="methodName"></param>
        /// <param name="verbose"></param>
        /// <returns>Activity</returns>
        public static EA.Element GetActivityFromMethodName(EA.Repository rep, string methodName, bool verbose=true)
        {
            // object_id has to be included
            string query = $@"select object_id
                                from t_object o 
                                where o.name = '{methodName}' AND
                                      o.object_type = 'Activity'";
            EA.Collection colEl = rep.GetElementSet(query, 2);
            if (colEl.Count == 0) return null;

            if (colEl.Count == 1) return (EA.Element) colEl.GetAt(0);

            if (verbose)
            {
                // more than one activity found
                var lActivities = from i in colEl.ToEnumerable<EA.Element>()
                    orderby i.FQName
                    select $"{i.FQName}";
                var sActivities = String.Join("\r\n", lActivities);

                // Message with Path to Activity
                MessageBox.Show($"{colEl.Count} Activities found for function '{methodName}'\r\nFirst one taken. See List in Clipboard.\r\n\r\n{sActivities}",
                    "More than one Activity found!");

                // Clipboard with GUID and Path to Activity
                lActivities = from i in colEl.ToEnumerable<EA.Element>()
                    orderby i.FQName
                    select $"{i.ElementGUID}  {i.FQName}";
                sActivities = String.Join("\r\n", lActivities);
                Clipboard.SetText(sActivities);
            }

            return (EA.Element) colEl.GetAt(0);
        }

        /// <summary>
        /// Create Call Action of type "CallBehavior or "CallOperation"
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="action"></param>
        /// <param name="guid">The guid of method(CallOperation) or of Activity (CallBehavior)</param>
        /// <param name="typeCall">'CallOperation' or 'CallBehavior'</param>
        /// <returns></returns>
        public static bool CreateCallAction(EA.Repository rep, EA.Element action, string guid, string typeCall="CallOperation")
        {
            // add ClassifierGUID to target action
            string updateStr = $@"update t_object set classifier_GUID = '{guid}'
                                 where ea_guid = '{action.ElementGUID}' ";
            rep.Execute(updateStr);

            // set CallOperation
            string callBehaviorProperty = $"@PROP=@NAME=kind@ENDNAME;@TYPE=ActionKind@ENDTYPE;@VALU={typeCall}@ENDVALU;@PRMT=@ENDPRMT;@ENDPROP;";
            Guid g = Guid.NewGuid();
            string xrefid = "{" + g + "}";
            string insertIntoTXref = @"insert into t_xref 
                (XrefID,            Name,               Type,              Visibility, Namespace, Requirement, [Constraint], Behavior, Partition, Description, Client, Supplier, Link)
                VALUES('" + xrefid + "', 'CustomProperties', 'element property','Public', '','','', '',0, '" + callBehaviorProperty + "', '" + action.ElementGUID + "', null,'')";
            rep.Execute(insertIntoTXref);

         
            return true;
        }

        /// <summary>
        /// Create Activity in Diagram context. Optionally you can pass your own diagram context.
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="methodName"></param>
        /// <param name="diaContext">Context of the activity to add, the location of the passed diagram</param>
        /// <returns></returns>
        public static EA.Element CreateInDiagramContext(EA.Repository rep, string methodName, EA.Diagram diaContext=null)
        {
            EA.Element act = GetActivityFromMethodName(rep, methodName,verbose:false);
            if (act == null)
            {
                // Create Activity
                if (MessageBox.Show($"No Activity '{methodName}' exists in model.\r\n Create Activity?", "Create Activity?",
                        MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    var dia = diaContext ?? rep.GetCurrentDiagram();
                    if (dia != null)
                    {
                        if (dia.ParentID > 0)
                        {
                            EA.Element parentEl = rep.GetElementByID(dia.ParentID);
                            act = (EA.Element) parentEl.Elements.AddNew(methodName, "Activity");
                            parentEl.Elements.Refresh();
                        }
                        else
                        {
                            EA.Package pkg = rep.GetPackageByID(dia.PackageID);
                            act = (EA.Element) pkg.Elements.AddNew(methodName, "Activity");
                            pkg.Elements.Refresh();
                        }

                        act.Update();
                    }
                }
            }

            return act;
        }
    }
}
