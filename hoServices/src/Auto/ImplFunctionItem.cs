using System.IO;

namespace hoReverse.Services.AutoCpp
{
    public class ImplFunctionItem
    {
        public string Interface { get; set; }
        public string Implementation { get; set; }
        public string FileName
        {
            get { return Path.GetFileName(FilePath); }
          
        }
        public string FileNameCallee
        {
            get { return Path.GetFileName(FilePathCallee); }

        }
        public string FilePath { get; set; }
        public string FilePathCallee { get; set; }
        public bool IsCalled { get; set; }
        public int LineStart { get; set; }
        public int LineEnd { get; set; }
        public int ColumnStart { get; set; }
        public int ColumnEnd { get; set; }

        public ImplFunctionItem(string @interface, string implementation, string filePath, string filePathCallee)
        {
            Interface = @interface;
            Implementation = implementation;
            FilePath = filePath;
            IsCalled = false;
            FilePathCallee = filePathCallee;
        }
        public ImplFunctionItem(string @interface, string implementation, string filePath)
        {
            Interface = @interface;
            Implementation = implementation;
            FilePath = filePath;
            IsCalled = false;
            FilePathCallee = "";
        }
        public ImplFunctionItem(string @interface, string implementation, string filePath, int lineStart, int columnStart, int lineEnd, int columnEnd)
        {
            Interface = @interface;
            Implementation = implementation;
            FilePath = filePath;
            IsCalled = false;
            FilePathCallee = "";
            LineStart = lineStart;
            LineEnd = lineEnd;
            ColumnStart = columnStart;
            ColumnEnd = columnEnd;

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
