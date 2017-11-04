using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DataModels.VcSymbols;
using EaServices.Files;
using EaServices.Functions;
using hoUtils.Package;
using hoLinqToSql.LinqUtils;
using LinqToDB.DataProvider;
using hoReverse.Services;
using hoReverse.Services.AutoCpp.Analyze;
using File = System.IO.File;


// ReSharper disable once CheckNamespace
namespace hoReverse.Services.AutoCpp
{
    public class AutoCpp
    {
        // VSCode SQLite Database for symbols to ease finding
        // the folder name is a hash to the folder, it only changes when the folder name changes.
        // Access by: 'Data Source=".."' // patch of the db
        // - ADODB
        // - LINQ (linq2db)
        // d:\hoData\Projects\00Current\ZF\Work\Source\
        private static readonly string dataSource =
                @"c:\Users\helmu_000\AppData\Roaming\Code\User\workspaceStorage\54bce7b4d8587e2ef489a9d5cc784ca4\ms-vscode.cpptools\.BROWSE.VC.DB";
        // private static readonly string dataSource = @"c:\Users\helmu_000\AppData\Roaming\Code\User\workspaceStorage\aa695e4b2b69e4df2595f987547a5da3\ms-vscode.cpptools\.BROWSE.VC.DB";
        // private static string dataSource = @"c:\Users\uidr5387\AppData\Roaming\Code\User\workspaceStorage\26045e663446b5f8d692303182313101\ms-vscode.cpptools\.BROWSE.VC.DB";

        private static string designRootPackageGuid = "{0DEBD6C4-F4DE-4084-881F-4E19304B2B93}";
        private static string[] processFiles =
        {
            "Sens_pClu_Posn.c",
            "Sens_pCpu_T.c"
        };
        private readonly EA.Repository _rep;
        private readonly EA.Package _pkg;
        private readonly EA.Element _component;
        private readonly string _designPackagedIds;
        readonly List<string> _functionsNotFound = new List<string>();
        readonly Files _files;
        readonly Functions _functions;
        readonly Files _designFiles;
        readonly Functions _designFunctions;


        string _connectionString = "";

        // statistics
        private int _deletedInterfaces = 0;
        private int _createdInterfaces = 0;

        public AutoCpp(EA.Repository rep)
        {
            _rep = rep;
            // inventory from VC Code Database
            _files = new Files(rep);
            _designFiles = new Files(rep);
            _functions = new Functions(dataSource, Files, rep);
            _designFunctions = new Functions(_designFiles, rep);



        }

        public AutoCpp(EA.Repository rep, EA.Element component)
        {
            _rep = rep;
            _component = component;   
            // inventory from VC Code Database
            _files = new Files(rep);
            _designFiles = new Files(rep);
            _functions = new Functions(dataSource, Files, rep);
            _designFunctions = new Functions(_designFiles, rep);

           
            
        }

        /// <summary>
        /// Generate the modules. It updates the modules or put it into the selected package.
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="pkg"></param>
        /// <returns></returns>
        public AutoCpp(EA.Repository rep, EA.Package pkg)
        {
            _rep = rep;
            _pkg = pkg;   
            // inventory from VC Code Database
            _files = new Files(rep);
            _designFiles = new Files(rep);
            _functions = new Functions(dataSource, Files,rep);
            _designFunctions = new Functions(_designFiles, rep);

            if (_rep.GetPackageByGuid(designRootPackageGuid) == null)
            {
                MessageBox.Show($@"Root package of existing design isnt't valid.
GUID={designRootPackageGuid}
Change variable: 'designRootPackageGuid=...'", "Cant inventory existing design, invalid root package");

            }
            _designPackagedIds = Package.GetBranch(_rep, "", _rep.GetPackageByGuid(designRootPackageGuid).PackageID);
            
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
            EA.Collection interfaceList = _rep.GetElementSet(sql, 2);
            foreach (EA.Element el in interfaceList)
            {
                string name = el.Name;
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
            _rep.RefreshModelView(_pkg.PackageID);
            return true;
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

        public string ConnectionString
        {
            get
            {
                _connectionString = "Data Source=" + dataSource;
                    return _connectionString; }
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
        /// Show all external functions for this Component/Class
        /// </summary>
        /// <param name="el"></param>
        /// <returns></returns>
        public bool ShowExternalFunctions(EA.Element el)
        {
            // get connection string of repository
            IDataProvider provider; // the provider to connect to database like Access, ..
            string connectionString = LinqUtil.GetConnectionString(ConnectionString, out provider);
            using (var db = new BROWSEVCDB(provider, connectionString))
            {
                string fileNameOfClass = (from f in db.Files
                    where f.LeafName.ToLower() == $"{el.Name.ToLower()}.c" || f.LeafName.ToLower() == $"{el.Name.ToLower()}.h" ||
                          f.LeafName.ToLower() == $"{el.Name.ToLower()}.cpp" || f.LeafName.ToLower() == $"{el.Name.ToLower()}.hpp"
                                          select f.Name).FirstOrDefault();
                fileNameOfClass = Path.GetDirectoryName(fileNameOfClass);
                if (fileNameOfClass == null)
                {
                    MessageBox.Show("Checked file extensions (*.c,*.h,*.hpp,*.cpp)", $"Cant't find source for '{el.Name}', Break!!");
                    return false;
                }

                // Component and Module implementation file names beneath folder
                IQueryable<string> fileNamesOfClassTree = from f in db.Files
                    where f.Name.StartsWith(fileNameOfClass) && f.LeafName.ToLower().EndsWith(".c")
                    select f.LeafName;

                // all possible external functions for component recursive
                var functions = (from function in db.CodeItems
                    join f in db.Files on function.FileId equals f.Id
                    where function.Kind == 22 && f.Name.StartsWith(fileNameOfClass) && f.LeafName.ToLower().EndsWith(".c")
                    orderby function.Name
                    select new { FName = function.Name, RX = new Regex($@"\b{function.Name}\s*\(") }).ToList();

                // over all files except Class/Component Tree (files not part of component/class/subfolder)
                var fileNames = from f in db.Files
                    where !fileNamesOfClassTree.Any(x => x == f.LeafName) &&
                          f.LeafName.ToLower().EndsWith(".c")
                    select f.Name;

                
                // get FunctioName, c-File name
                var lFunctions = new List<Tuple<string, string>>();
                foreach (var f in fileNames)
                {
                    string t = File.ReadAllText(f);
                    t = hoService.DeleteComment(t);
                    foreach (var f1 in functions)
                    {
                        if (f1.RX.IsMatch(t))
                        {
                            string fileName = Path.GetFileName(f);
                            lFunctions.Add(new Tuple<string, string>(f1.FName, fileName ));
                      
                        }
                    }
                }
                // Sort: Function, FileName
                var outputList = (from f in lFunctions
                    orderby f.Item1, f.Item2
                    select new {Function = f.Item1, File = f.Item2}).Distinct();

                DataTable dt = outputList.ToDataTable();

                // Output Function, FileNme/GUID
                string delimiter = Environment.NewLine;
                string lExternalFunction = $"GUID={el.ElementGUID}{delimiter}FQ={el.FQName}{delimiter}";
                string lExternalFunctionDialog = lExternalFunction;
                foreach (var row in outputList)
                {
                    string fileName = row.File;
                    string functionName = row.Function;
                    EA.Element elComponent = GetElementFromName(Path.GetFileNameWithoutExtension(fileName));
                    string guid = elComponent != null ? elComponent.ElementGUID : "";

                    lExternalFunction = $"{lExternalFunction}{delimiter}{functionName.PadRight(50)}\t{fileName}/{guid}";


                    lExternalFunctionDialog = functionName.Length > 20
                        ? $"{lExternalFunctionDialog}{delimiter}{functionName.PadRight(50)}\t{fileName}"
                        : $"{lExternalFunctionDialog}{delimiter}{functionName.PadRight(50)}\t\t{fileName}";
                }
  

                Clipboard.SetText(lExternalFunction);
                FrmComponentFunctions frm = new FrmComponentFunctions(el, dt);
                //MessageBox.Show($"hoReverse copied to clipboard:\r\n\r\n{lExternalFunctionDialog}", $"External functions of {el.Name}");
                return true;

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
            string connectionString = LinqUtil.GetConnectionString(_rep, out provider);
            using (var db = new DataModels.EaDataModel(provider, connectionString))
            {
                var elGuid = (from n in db.t_object
                    where n.Name.ToLower() == name.ToLower() && n.Object_Type == "Component"
                    select n.ea_guid).FirstOrDefault();
                return elGuid != null ? _rep.GetElementByGuid(elGuid) : null;

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
