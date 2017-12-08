using System.IO;

// ReSharper disable once CheckNamespace
namespace hoReverse.Services.AutoCpp
{
    public class ImplFunctionItem
    {
        public string Interface { get; set; }
        public string Implementation { get; set; }
        public string FileName => Path.GetFileName(FilePath);
        public string FileNameCallee => Path.GetFileName(FilePathCallee);
        public string FilePath { get; set; }
        public string FilePathCallee { get; set; }
        public bool IsCalled => FilePathCallee.Trim() != "";

        // true = is defined by macro
        // false = isn't defined by macro
        public bool Macro { get; set; }
        public int LineStart { get; set; }
        public int LineEnd { get; set; }
        public int ColumnStart { get; set; }
        public int ColumnEnd { get; set; }

        public ImplFunctionItem(string @interface, string implementation, string filePath, string filePathCallee)
        {
            Interface = @interface;
            Implementation = implementation;
            FilePath = filePath;
            FilePathCallee = filePathCallee;
        }
        public ImplFunctionItem(string @interface, string implementation, string filePath)
        {
            Interface = @interface;
            Implementation = implementation;
            FilePath = filePath;
            FilePathCallee = "";
        }
        public ImplFunctionItem(string @interface, string implementation, string filePath, int lineStart, int columnStart, int lineEnd, int columnEnd)
        {
            Interface = @interface;
            Implementation = implementation;
            FilePath = filePath;
            FilePathCallee = "";
            LineStart = lineStart;
            LineEnd = lineEnd;
            ColumnStart = columnStart;
            ColumnEnd = columnEnd;

        }
        public ImplFunctionItem(string @interface, string implementation, string filePath, int lineStart, int columnStart, int lineEnd, int columnEnd, bool macro)
        {
            Interface = @interface;
            Implementation = implementation;
            FilePath = filePath;
            FilePathCallee = "";
            LineStart = lineStart;
            LineEnd = lineEnd;
            ColumnStart = columnStart;
            ColumnEnd = columnEnd;
            Macro = macro;

        }

        
    }
}
