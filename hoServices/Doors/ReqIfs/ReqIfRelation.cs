using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using EA;
using ReqIFSharp;

namespace EaServices.Doors.ReqIfs
{
    public class ReqIfRelation
    {
        readonly ReqIF _reqIf;
        readonly Repository _rep;
        readonly FileImportSettingsItem _settings;
        private List<EA.Element> _requirements;

        public ReqIfRelation(ReqIF reqIf,  EA.Repository rep, FileImportSettingsItem settings)
        {
            _reqIf = reqIf;
            _rep = rep;
            _settings = settings;
            _requirements = new List<EA.Element>();
        }
        /// <summary>
        /// Write the EA dependencies according to ReqIF between the requirements
        /// </summary>
        public bool WriteRelations()
        {
            // Read all requirements
            string stereotypePredicate = _settings.Stereotype == "" ? "" : $" AND stereotype = '{_settings.Stereotype}' ";
            string sql = $@"select *
                    from t_object o
                    inner join t_package pkg on pkg.package_id = o.package_id 
                    where pkg.ea_guid in ( {_settings.PackageGuidCommaList} ) AND
                           o.object_type = '{_settings.ObjectType}' 
                           {stereotypePredicate}";

            EA.Collection reqCol =  _rep.GetElementSet(sql, 2);
            foreach (EA.Element req in reqCol)
            {
                _requirements.Add(req);
            }


                // All EA requirements and their target
            try
            {
                    // check not existing references
                    var notExistingTargets = (from r in _reqIf.CoreContent[0].SpecRelations
                        join eaRS in _requirements on r.Source.Identifier ?? "" equals eaRS.Multiplicity ?? ""
                        where r.Target == null
                        select $"{eaRS.Multiplicity}:{eaRS.Name}").ToArray();
                    if (notExistingTargets.Length > 0)
                    {
                        MessageBox.Show($@"{String.Join("\r\n", notExistingTargets)}",@"Target Requirements for Link not available, skip links");
                       
                    }


                    var relations = from r in _reqIf.CoreContent[0].SpecRelations
                    join eaRS in _requirements on r.Source?.Identifier??"" equals eaRS.Multiplicity??""
                    join eaRT in _requirements on r.Target?.Identifier??"" equals eaRT.Multiplicity??""
                    orderby r.Source?.Identifier??""
                    select new
                    {
                        SourceReq = eaRS,
                        TargetReq = eaRT,
                        SObjectId = r.Source.Identifier??"",
                        TObjectId = r.Target.Identifier??"",
                    };


                
                // Create the relations
                EA.Element el = null; 
                foreach (var rel in relations)
                {
                    if (el != rel.SourceReq)
                    {
                        el = rel.SourceReq;
                        if (el != null) DeleteDependencies(el,_settings.PackageGuidList);
                    }

                    EA.Connector con = (EA.Connector)el.Connectors.AddNew("", "Dependency");
                    con.Stereotype = _settings.StereotypeDependency;
                    con.ClientID = rel.TargetReq.ElementID;
                    con.SupplierID = rel.SourceReq.ElementID;
                    con.Update();

                    el.Connectors.Refresh();
                    el.Update();

                   
                }

                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show($@"Related link to requirement not available

File:
'{_settings.InputFile}'

{e}", @"Can't write ReqIF relations in EA, skip relations!");
                return false;
            }
        }

        /// <summary>
        /// Delete dependencies of element within the ReqIF Packages
        /// </summary>
        /// <param name="el"></param>
        /// <param name="packageGuidList"></param>
        private void DeleteDependencies( EA.Element el, List<ReqIfModuleAssign> packageGuidList)
        {
            for (int i = el.Connectors.Count - 1; i >= 0; i--)
            {
                var elConnector = (EA.Connector)el.Connectors.GetAt((short)i);
                var elTarget = _rep.GetElementByID(elConnector.SupplierID);
                string pkgTargetGuid = _rep.GetPackageByID(elTarget.PackageID).PackageGUID;
                // Check if guid exists
                if (packageGuidList.Exists(y => y.Guid == pkgTargetGuid))
                        el.Connectors.DeleteAt((short)i, true);
            }
            el.Refresh();
            el.Update();
        }
    }
    
}
