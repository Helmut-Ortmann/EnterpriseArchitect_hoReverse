using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using DataModels;
using hoLinqToSql.LinqUtils;
using hoUtils.ExportImport;

namespace EaServices.Doors
{
    public class DoorsModule
    {
        string _importModuleFile;
        EA.Package _pkg;
        private EA.Package _pkgDeletedObjects;
        EA.Repository _rep;
        DataTable _dtDoorsRequirements;
        Dictionary<string, int> _dictObjectIdFromDoorsId = new Dictionary<string,int>();

        private readonly string packageNameDeletedObjects = "DeletedDoorsRequirements";

        public DoorsModule(EA.Repository rep, EA.Package pkg, string importModuleFile)
        {
            _importModuleFile = importModuleFile;
            _pkg = pkg;
            _rep = rep;

            // Read all requirements
            _dtDoorsRequirements = ExpImp.MakeDataTableFromCsvFile(_importModuleFile, ',');

            // get connection string of repository
            string connectionString = LinqUtil.GetConnectionString(_rep, out var provider);

            if (connectionString == "") return;

            // Read all existing EA Requirements of package
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

            CreatePackageDeletedObjects();
        }

        private string GetAbsoluteNumerFromDoorsId(string doorsId)
        {
            // Get only the number of the id
            Regex exDoorsAbsNumber = new Regex("[0-9]*$");
            return exDoorsAbsNumber.Match(doorsId).Value;
        }

        public void MoveDeletedRequirements()
        {
            var moveEaElements = from m in _dictObjectIdFromDoorsId
                where !_dtDoorsRequirements.Rows.Cast<DataRow>().Any(x => GetAbsoluteNumerFromDoorsId(x["Id"].ToString()) == m.Key)
                select m.Value;
            foreach (var eaId in moveEaElements)
            {
                EA.Element el = _rep.GetElementByID(eaId);
                el.PackageID = _pkgDeletedObjects.PackageID;
                el.ParentID = 0;
                el.Update();
            }
        }

        /// <summary>
        /// Create Package for deleted objects
        /// </summary>
        /// <returns>Package created Objects</returns>
        private EA.Package CreatePackageDeletedObjects()
        {
            if (_pkgDeletedObjects != null) return _pkgDeletedObjects;
            if (_pkg.Packages.Count > 0)
            {
                _pkgDeletedObjects = (EA.Package) _pkg.Packages.GetAt(0);
                return _pkgDeletedObjects;
            }

            _pkgDeletedObjects = (EA.Package)_pkg.Packages.AddNew(packageNameDeletedObjects, "");
            return _pkgDeletedObjects;

        }

        public EA.Repository Rep { get => _rep; set => _rep = value; }
        public EA.Package Pkg { get => _pkg; set => _pkg = value; }
        public string ImportModuleFile { get => _importModuleFile; set => _importModuleFile = value; }
        /// <summary>
        /// EA Element-Ids of DOORS IDs
        /// </summary>
        public Dictionary<string, int> DictObjectIdFromDoorsId { get => _dictObjectIdFromDoorsId; set => _dictObjectIdFromDoorsId = value; }
        public DataTable DtDoorsRequirements { get => _dtDoorsRequirements; set => _dtDoorsRequirements = value; }
    }
}
