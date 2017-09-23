using System.Collections.Generic;
using System.IO;

namespace EaServices.Files
{
    public class Files
    {
        private readonly Dictionary<string, FileItem> _fileList = new Dictionary<string, FileItem>();
        private readonly EA.Repository _rep;
        public Files(EA.Repository rep) {_rep = rep;}

        public Dictionary<string, FileItem> FileList => _fileList;

        /// <summary>
        /// Add a fileItem of type Module or Interface and return the current Item. 
        /// The item may or may not exists.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public FileItem Add(string filePath)
        {
            string fileName = Path.GetFileName(filePath);
            FileItem fileItem;
            if (FileList.TryGetValue(fileName.ToLower(), out fileItem)) return fileItem;

            if (FileItem.IsInterface(filePath)) FileList.Add(fileName.ToLower(), new InterfaceItem(filePath,_rep));
            else FileList.Add(fileName.ToLower(), new ModuleItem(filePath,_rep));

            return FileList[fileName.ToLower()];
        }




        public FileItem GetItemFromPath(string filePath)
        {
            FileItem result;
            if (FileList.TryGetValue(Path.GetFileName(filePath), out result)) return result;
            return null;
        }
        public FileItem GetItemFromFileName(string fileName)
        {
            FileItem result;
            if (FileList.TryGetValue(fileName, out result)) return result;
            return null;
        }

        public string GetFileNameFromPath(string filePath)
        {
            return Path.GetFileName(filePath);
        }


        /// <summary>
        /// Inventory the files:
        /// - Get existing EA interface
        /// - Get provided EA function
        /// </summary>
        /// <returns></returns>
        public bool Inventory()
        {
            foreach (var file in FileList)
            {
                InterfaceItem interfaceItem = file.Value as InterfaceItem;
                interfaceItem?.Inventory();


            }
            return true;
        }
        /// <summary>
        /// Generate EA Interfaces:
        /// - Update all 
        /// - Get existing EA interface
        /// - Get provided EA function
        /// </summary>
        /// <returns>Newly created Methods</returns>
        public int Generate(EA.Package pkg)
        {
            int newMethods = 0;
            foreach (var file in FileList)
            {
                InterfaceItem interfaceItem = file.Value as InterfaceItem;
                interfaceItem?.Generate(pkg);
                if (interfaceItem != null) newMethods = newMethods + interfaceItem.Generate(pkg);
            }
            _rep.RefreshModelView(pkg.PackageID);
            ;
            return newMethods;
        }



    }

   
   
}
