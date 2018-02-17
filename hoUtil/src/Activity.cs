using System;

namespace hoReverse.hoUtils
{
    public static class Activity
    {
        /// <summary>
        /// Find method from method name
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="methodName"></param>
        /// <returns>Activity</returns>
        public static EA.Element GetActivityFromMethodName(EA.Repository rep, string methodName)
        {
            // object_id has to be included
            string query = $@"select object_id
                                from t_object o 
                                where o.name = '{methodName}' AND
                                      o.object_type = 'Activity'";
            EA.Collection colEl = rep.GetElementSet(query, 2);
            if (colEl.Count == 0) return null;

            if (colEl.Count == 1) return (EA.Element)colEl.GetAt(0);

            // more than one activity found

            return (EA.Element)colEl.GetAt(0);
        }

        /// <summary>
        /// Create Call Action of type "CallBehavior" or "CallOperation"
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
            string CallBehaviorProperty = "@PROP=@NAME=kind@ENDNAME;@TYPE=ActionKind@ENDTYPE;@VALU=CallBehavior@ENDVALU;@PRMT=@ENDPRMT;@ENDPROP;";
            Guid g = Guid.NewGuid();
            string xrefid = "{" + g + "}";
            string insertIntoTXref = @"insert into t_xref 
                (XrefID,            Name,               Type,              Visibility, Namespace, Requirement, [Constraint], Behavior, Partition, Description, Client, Supplier, Link)
                VALUES('" + xrefid + "', 'CustomProperties', 'element property','Public', '','','', '',0, '" + CallBehaviorProperty + "', '" + action.ElementGUID + "', null,'')";
            rep.Execute(insertIntoTXref);

         
            return true;
        }
    }
}
