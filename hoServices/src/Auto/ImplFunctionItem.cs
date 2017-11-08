using System.IO;

namespace hoReverse.Services.AutoCpp
{
    public class ImplFunctionItem
    {
        public string Implementation { get; set; }
        public string Interface { get; set; }
        public string FilePath { get; set; }
        public string FilePathCallee { get; set; }
        public string FileName
        {
            get { return Path.GetFileName(FilePath); }
          
        }
        public string FileNameCallee
        {
            get { return Path.GetFileName(FilePath); }

        }
        public bool IsCalled { get; set; }
        public ImplFunctionItem(string @interface, string implementation, string filePath)
        {
            Interface = @interface;
            Implementation = implementation;
            FilePath = filePath;
            IsCalled = false;
            FilePathCallee = "";
        }

        public ImplFunctionItem(string @interface, string implementation, string filePath, bool isCalled=false)
        {
            Interface = @interface;
            Implementation = implementation;
            FilePath = filePath;
            IsCalled = isCalled;
            FilePathCallee = "";
        }

    }
}
