using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using EaServices.Files;
using EaServices.Functions;
using EA;
using hoLinqToSql.LinqUtils;
using LinqToDB.DataProvider;
using Package = hoUtils.Package.Package;


// ReSharper disable once CheckNamespace
namespace hoReverse.Services.AutoCpp
{
    public partial class AutoCpp
    {
        

        private static string designRootPackageGuid = "{0DEBD6C4-F4DE-4084-881F-4E19304B2B93}";
        private static string[] processFiles =
        {
            "Sens_pClu_Posn.c",
            "Sens_pCpu_T.c"
        };
        private EA.Repository _rep;
        private readonly EA.Package _pkg;
        private readonly string _designPackagedIds;
        readonly List<string> _functionsNotFound = new List<string>();
        readonly Files _files;
        readonly Functions _functions;
        readonly Files _designFiles;
        readonly Functions _designFunctions;



        // statistics
        private int _deletedInterfaces = 0;
        private int _createdInterfaces = 0;

 
        public AutoCpp(EA.Repository rep)
        {
            Rep = rep;
            // inventory from VC Code Database
            _files = new Files(rep);
            _designFiles = new Files(rep);
        }

        public AutoCpp(EA.Repository rep, EA.Element component)
        {
            Rep = rep;
            _component = component;   
            // inventory from VC Code Database
            _files = new Files(rep);
            _designFiles = new Files(rep);
            

           
            
        }

        /// <summary>
        /// Generate the modules. It updates the modules or put it into the selected package.
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="pkg"></param>
        /// <returns></returns>
        public AutoCpp(EA.Repository rep, EA.Package pkg)
        {
            Rep = rep;
            _pkg = pkg;   
            // inventory from VC Code Database
            _files = new Files(rep);
            _designFiles = new Files(rep);
            
            if (Rep.GetPackageByGuid(designRootPackageGuid) == null)
            {
                MessageBox.Show($@"Root package of existing design isnt't valid.
GUID={designRootPackageGuid}
Change variable: 'designRootPackageGuid=...'", "Cant inventory existing design, invalid root package");

            }
            _designPackagedIds = Package.GetBranch(Rep, "", Rep.GetPackageByGuid(designRootPackageGuid).PackageID);
            
        }
        /// <summary>
        /// Inventory Interfaces
        /// </summary>
        /// <returns></returns>
        public bool InventoryInterfaces()
        {
            Files.Inventory();
            return true;

        }
        //
        public bool InventoryDesignInterfaces()
        {
            // Get all interfaces
            // 'object_id' has to be included in select
            string sql = $@"select object_id, name
            from t_object 
            where object_type = 'Interface' AND
            package_id in ( { _designPackagedIds} )
            order by 2";
            EA.Collection interfaceList = Rep.GetElementSet(sql, 2);
            foreach (EA.Element el in interfaceList)
            {
                InterfaceItem ifItem = (InterfaceItem)_designFiles.Add($"{el.Name}.h");
                ifItem.El = el;

                foreach (EA.Method m in el.Methods)
                {
                    // create function
                    FunctionItem functionItem = new FunctionItem(m.Name, m.ReturnType, m.IsStatic, new List<ParameterItem>(), ifItem, m);
                    if (_designFunctions.FunctionList.ContainsKey(m.Name))
                    {
                       var function =  _designFunctions.FunctionList[m.Name];
                        MessageBox.Show($"Interface:\t{function.Interface?.Name}\r\nModule:\t{function.Module?.Name}", 
                            $"Duplicated Function {m.Name}, skipped!");
                    }
                    else
                    {
                        _designFunctions.FunctionList.Add(m.Name, functionItem);
                    }

                    // update interface
                    ifItem.ProvidedFunctions.Add(functionItem);
                }

                
            }

            // Show deleted Interface
            var deletedInterfaces = from s in _designFiles.FileList
                where ! _files.FileList.Any(t => (s.Value.Name == t.Value.Name)         && 
                                               t.Value.GetType().Name.Equals(typeof(InterfaceItem).Name ) )
                select s.Value.El;

            var createdInterfaces = from s in _files.FileList
                                    where ! _designFiles.FileList.All(t => (s.Value.Name == t.Value.Name)             &&
                                                   t.Value.GetType().Name.Equals(typeof(InterfaceItem).Name))
                                    select s.Value.El;

            _deletedInterfaces = 0;
            string deleteMe = "DeleteMe";
            foreach (var el in deletedInterfaces)
            {
                if (!el.Name.EndsWith(deleteMe))
                {
                    el.Name = el.Name + deleteMe;
                    el.Update();
                }
                _deletedInterfaces++;
            }
            _createdInterfaces = createdInterfaces.Count();
            
            return true;
        }



        /// <summary>
        /// Generate Interfaces
        /// </summary>
        /// <returns></returns>
        public int  GenerateInterfaces()
        {
           return Files.Generate(_pkg);

        }




        /// <summary>
        /// Inventory of files to process
        /// </summary>
        /// <returns></returns>
        public bool InventoryFiles()
        {
            // inventory modules
            foreach (string fileName in processFiles)
            {
                InventoryFile(fileName);
            }
            return true;
        }
        /// <summary>
        /// Generate all
        /// </summary>
        /// <returns></returns>
        public bool Generate()
        {
            // Generate modules
            foreach (string fileName in processFiles)
            {
                GenerateFile(fileName);
            }
            Rep.RefreshModelView(_pkg.PackageID);
            return true;
        }
        /// <summary>
        /// Generate file
        /// </summary>
        /// <returns></returns>
        public bool Generate(string fileName)
        {
            return true;
        }
        /// <summary>
        /// Generate file according to list
        /// </summary>
        /// <returns></returns>
        public bool Generate(string[] fileNames)
        {
            return true;
        }

        /// <summary>
        /// Inventory a file (abc.h or abc.c) from it's content.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private bool InventoryFile(string fileName)
        {
            if (Files.FileList.ContainsKey(fileName.ToLower()))
            {
                FileItem fileItem = Files.FileList[fileName.ToLower()];
                if (fileItem is ModuleItem)
                {
                    ModuleItem moduleItem = (ModuleItem)fileItem;
                    moduleItem.InventoryAddEaModuleReference();
                    moduleItem.InventoryRequiredFunctionsFromTextFile(moduleItem.FilePath, Functions, FunctionsNotFound);


                }
            }
            else
            {
                MessageBox.Show($@"The module '{fileName}' don't exists.", "Module file don't exists.");
            }

            return true;
        }
        /// <summary>
        /// Inventory a file (abc.h or abc.c) from it's content.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private bool GenerateFile(string fileName)
        {
            if (Files.FileList.ContainsKey(fileName.ToLower()))
            {
                FileItem fileItem = Files.FileList[fileName.ToLower()];
                if (fileItem is ModuleItem)
                {
                    ModuleItem moduleItem = (ModuleItem)fileItem;
                    moduleItem.Generate(_pkg);
                }
            }
            else
            {
                MessageBox.Show($@"The module '{fileName}' don't exists.", "Module file don't exists.");
            }

            return true;
        }

        public List<string> FunctionsNotFound { get => _functionsNotFound; }
        public Functions Functions { get => _functions;  }
        public Files Files { get => _files;  }

        public int DeletedInterfaces
        {
            get { return _deletedInterfaces; }
        }

        public int CreatedInterfaces
        {
            get { return _createdInterfaces; }
        }

        /// <summary>
        /// Get the connection string of VC Code SQLite database.
        /// - Pass the root source file folder to find the right VC Code symbol database
        /// - Get the newest database
        /// - Access= ReadOnly
        /// - Returns empty string if can't find database
        /// </summary>
        private string ConnectionString => VcDbUtilities.GetConnectionString(_folderRoot);

       
        public Repository Rep
        {
            get { return _rep; }
            set { _rep = value; }
        }

        /// <summary>
        /// Generate External functions of component
        /// </summary>
        public void GenExternalFuntionsOfComponent()
        {
            // get connection string of repository
            IDataProvider provider;  // the provider to connect to database like Access, ..
            string connectionString = LinqUtil.GetConnectionString(ConnectionString, out provider);
            using (var db = new DataModels.VcSymbols.BROWSEVCDB(provider, connectionString))
            {
                // find all possible external functions

                // find root folder
                var files = from file in db.Files
                    where file.LeafName.ToLower() == $"{_component.Name.ToLower()}.c"
                    select new {file.Name, file.LeafName };

                string fileName = files.FirstOrDefault()?.Name;
                if (fileName == "")
                {
                    MessageBox.Show($"Component:\t{_component.Name}", "Can't find c-file for component");
                    return;
                }


                // Find Files
                Debug.Assert(fileName != null, nameof(fileName) + " != null");
                var f = from file in Directory.GetFiles(Path.GetDirectoryName(fileName))
                    where file.ToLower().EndsWith(".c")
                    select file;
                foreach (var file in f)
                {
                    
                }



            }
         }


        /// <summary>
        /// Get first Element from name. It's case insensitive. If it doesn't find a member it returns null
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private EA.Element GetElementFromName(string name)
        {
            // get connection string of repository
            IDataProvider provider; // the provider to connect to database like Access, ..
            string connectionString = LinqUtil.GetConnectionString(Rep, out provider);
            using (var db = new DataModels.EaDataModel(provider, connectionString))
            {
                var elGuid = (from n in db.t_object
                    where n.Name.ToLower() == name.ToLower() && n.Object_Type == "Component"
                    select n.ea_guid).FirstOrDefault();
                return elGuid != null ? Rep.GetElementByGuid(elGuid) : null;

            }
        }
       
        /// <summary>
        /// Example LINQ to SQL
        /// - All object object types
        /// - Count of object types
        /// - Percentage of object types
        /// - Total count
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        private static DataTable RunLinq2Db(IDataProvider provider, string connectionString)
        {
            //DataConnection.DefaultSettings = new hoLinq2DBSettings(provider, connectionString);
            try
            {
                {

                    using (var db = new DataModels.EaDataModel(provider, connectionString))
                    {
                        var count = db.t_object.Count();
                        var q = (from c in db.t_object.AsEnumerable()
                            group c by c.Object_Type into g
                            orderby g.Key

                            select new
                            {
                                Type = g.Key,
                                Prozent = $"{ (float)g.Count() * 100 / count:00.00}%",
                                Count = g.Count(),
                                Total = count
                            });

                        return q.ToDataTable();

                    }
                }

            }
            catch (Exception e)
            {
                MessageBox.Show($"Provider: {provider}\r\nConnection: {connectionString}\r\n\r\n{e}", "Error Linq2DB");
                return new DataTable();
            }


        }
    }
}
