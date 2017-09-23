using System.Collections.Generic;
using System.IO;
using EaServices.Functions;

namespace EaServices.Files
{
    public class FileItem
    {
        //Dictionary<string, FileItem> _fns = new Dictionary<string, FileItem>();
        private readonly string _filePath;
        private readonly EA.Repository _rep;
        private EA.Element _el;

        private readonly List<FunctionItem> _providedFunctions = new List<FunctionItem>();
        private readonly List<FunctionItem> _requiredFunctions = new List<FunctionItem>();


        protected FileItem(string filePath, EA.Repository rep)
        {
            _filePath = filePath;
            _rep = rep;
            El = null;

        }

        public string FilePath => _filePath;
        public string FileName => Path.GetFileName(_filePath);
        public string Name => GetName(_filePath);
        public EA.Repository Rep => _rep;

        public List<FunctionItem> ProvidedFunctions => _providedFunctions;
        public List<FunctionItem> RequiredFunctions => _requiredFunctions;

        public EA.Element El
        {
            get { return _el; }
            set { _el = value; }
        }

        public static string GetName(string filePath)
        {
            string fileName = Path.GetFileName(filePath);
            int length = fileName.IndexOf('.');
            return fileName.Substring(0, length);
        }
        public static bool IsInterface(string filePath)
        {
            string path = filePath.ToLower();
            if (path.EndsWith(".hpp") || path.EndsWith(".h")) return true;
            return false;
        }

        public void ProvidedAdd(FunctionItem functionItem)
        {
            if (_providedFunctions.Contains(functionItem)) return;
            _providedFunctions.Add(functionItem);
        }
        public void RequiredAdd(FunctionItem functionItem)
        {
            if (_requiredFunctions.Contains(functionItem)) return;
            _requiredFunctions.Add(functionItem);
        }

    }

   


    
}
