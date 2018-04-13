using System.Collections.Generic;
using System.Linq;
using DataModels;
using hoLinqToSql.LinqUtils;
using LinqToDB.DataProvider;

namespace EaServices.Doors
{
    public class DoorsModule
    {
        string _importModuleFile;
        EA.Package _pkg;
        EA.Repository _rep;
        Dictionary<string, int> _dictObjectIdFromDoorsId = new Dictionary<string,int>();

        public DoorsModule(EA.Repository rep, EA.Package pkg, string importModuleFile)
        {
            _importModuleFile = importModuleFile;
            _pkg = pkg;
            _rep = rep;

            // get connection string of repository
            IDataProvider provider;  // the provider to connect to database like Access, ..
            string connectionString = LinqUtil.GetConnectionString(_rep, out provider);

            if (connectionString == "") return;
            using (var db = new EaDataModel(provider, connectionString))
            {
                _dictObjectIdFromDoorsId = (from r in db.q_object
                    where r.Object_Type == "Requirement" && r.Package_ID == _pkg.PackageID
                    select new
                    {
                       Name = r.Multiplicity,
                        Value = r.Object_ID

                    }).ToDictionary(x=>x.Name, x=>x.Value);
            }
        }

        public EA.Repository Rep { get => _rep; set => _rep = value; }
        public EA.Package Pkg { get => _pkg; set => _pkg = value; }
        public string ImportModuleFile { get => _importModuleFile; set => _importModuleFile = value; }
        public Dictionary<string, int> DictObjectIdFromDoorsId { get => _dictObjectIdFromDoorsId; set => _dictObjectIdFromDoorsId = value; }
    }
}
