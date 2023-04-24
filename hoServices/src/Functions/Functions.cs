using System;
using System.Collections.Generic;
using System.Linq;
using EaServices.Files;
using hoReverse.hoUtils.DB;

namespace EaServices.Functions
{
    /// <summary>
    /// Inventory all Functions (Interface+Implementation) from VC-Code C++ DB
    /// </summary>
    public class Functions
    {
        private readonly Dictionary<string, FunctionItem> _functionList = new Dictionary<string, FunctionItem>();
        private readonly Files.Files _files;
        private readonly EA.Repository _rep;


        public Functions(Files.Files files, EA.Repository rep)
        {
            _rep = rep;
            _files = files;
        }

        // Read all Functions as Interface and as Module. It captures:
        // - Name, ReturnValue, Parameter, File the function is defined (implemented/interface)
        //
        // It only captures one function for a 'leaf_name'  (ignore multiple header files with the same content)
        readonly string _sql = $@"
select distinct i.name, f.name, i.type, i1.name, i1.type, i1.param_number 
     from (code_items i inner join files f      on (i.file_id = f.id ))
                        left join code_items i1 on (i1.parent_id=i.id and i1.kind = 10)

     where     i.kind = 22                                           AND NOT
               Instr(lower(leaf_name), 'mockup')                     AND NOT
               Instr(lower(leaf_name), 'stub')                       AND NOT
               lower(f.name) LIKE 'c:\program%'
      group by f.leaf_name, i.name,i1.param_number 
      order by f.leaf_name, i.name, i1.param_number";


        /// <summary>
        /// Capture all Functions from VC Code DB
        /// </summary>
        /// <param name="dataSource"></param>
        /// <param name="files"></param>
        /// <param name="rep"></param>
        public Functions(string dataSource, Files.Files files, EA.Repository rep)
        {
            _rep = rep;
            _files = files;
            // Check data Source
           
            SQLite sqlLite = new SQLite(dataSource);
            var reader = sqlLite.ExecuteSql(_sql);
            if (reader == null) return;
            Dictionary<string, ParameterItem> parList = new Dictionary<string, ParameterItem>();
            string filePath="";
            string returnValue="";
            string oldFunctionName = "";
            bool isStatic = false;

            //
            // 0: File
            // 1: FunctionName
            // 2: ReturnValue
            // 3: ParameterName
            // 4: ParameterType
            // 5: ParameterNumber
            while (reader.Read())
            {
                string functionName=reader[0].ToString();

                // new function
                if (oldFunctionName != functionName)
                {
                    if (oldFunctionName != "")
                    {
                        AddFunction(filePath, oldFunctionName, returnValue, false, parameterDictionaryToList(parList));
                        parList = new Dictionary<string, ParameterItem>(); 
                    }
                }
                // Function
                oldFunctionName = functionName;
                filePath = reader[1].ToString();

                // Add parameter
                returnValue = reader[2].ToString();
                isStatic = false;
                if (returnValue.StartsWith("static"))
                {
                    isStatic = true;
                    returnValue = returnValue.Substring(6).Trim();
                }


                string parName = reader[3].ToString();
                if (parName.Trim() != "")
                {
                    string parType = reader[4].ToString().Replace("\u0001","").Trim();
                    bool isConst = false;
                    if (parType.StartsWith("const"))
                    {
                        isConst = true;
                        parType = parType.Substring(6).Trim();
                    }
                    int index = 1;
                    while ( parList.ContainsKey(parName))
                    {
                        parName = $"{parName}_{index}";
                        index = index + 1;
                    }
                    int parPos = Int32.Parse(reader[5].ToString());
                    parList.Add(parName, new ParameterItem(parPos, parName, parType, isConst));
                }
            }
            AddFunction(filePath, oldFunctionName, returnValue, isStatic, parameterDictionaryToList(parList));
            sqlLite.EndSql();
            
        }
        /// <summary>
        /// ConvertRtfToXhtml Dictionary to simple list
        /// </summary>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        private List<ParameterItem> parameterDictionaryToList(Dictionary<string, ParameterItem> dictionary)
        {
            List<ParameterItem> list = dictionary
                .Select(e => new ParameterItem(e.Value.Position, e.Value.Name, e.Value.Type, e.Value.IsConst))
                .ToList();
            return list;
        }

        public Dictionary<string, FunctionItem> FunctionList { get => _functionList; }

        /// <summary>
        /// Add Function with parameters
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="functionName"></param>
        /// <param name="returnValue"></param>
        /// <param name="parList"></param>
        private void AddFunction(string filePath, string functionName, string returnValue, bool isStatic, List<ParameterItem> parList)
        {
            if (functionName == "") return;
            // add File if not exists
            FileItem fileItem = _files.Add(filePath);


            // only unique names, Implementation and Interface is possible
            if (!_functionList.ContainsKey(functionName))
            {
                _functionList.Add(functionName, new FunctionItem(functionName, returnValue, isStatic, parList));
            }
            // update Interface + Module/Implementation
            if (fileItem is InterfaceItem)
            {
                _functionList[functionName].Interface = (InterfaceItem)fileItem;
                fileItem.ProvidedFunctions.Add(_functionList[functionName]);
            }
            else
            {
                _functionList[functionName].Module = (ModuleItem)fileItem;
                fileItem.ProvidedFunctions.Add(_functionList[functionName]);
            }

        }

    }
}
