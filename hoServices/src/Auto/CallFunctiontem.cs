using System;
using System.IO;

// ReSharper disable once CheckNamespace
namespace hoReverse.Services.AutoCpp
{
    public class CallFunctionItem
    {
        public String Function { get; set; }
        public String FileName
        {
            get { return Path.GetFileName(FilePath); }
        }

        public String FilePath { get; set; }

        public CallFunctionItem(string function, string filePath)
        {
            Function = function;
            FilePath = filePath;

        }
    }
}
