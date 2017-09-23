using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using EaServices.Functions;
using hoReverse.hoUtils;
using hoReverse.Services;

namespace EaServices.Files
{
    /// <summary>
    /// Handle a FileItem
    /// </summary>
    public class ModuleItem : FileItem
    {


        public ModuleItem(string filePath, EA.Repository rep) : base(filePath, rep)
        {
        }


        /// <summary>
        /// Inventory a Module. It adds the EA Module reference if exists.
        /// </summary>
        public void Inventory()
        {
            // get module: Component, Stereotype: 'LE Modul'
            string sql = $@"select object_id 
                              from t_object 
                              where name = '{Name}'             AND
                                    object_type = 'Component'   AND
                                    Stereotype = 'LE Modul'";
            EA.Collection col = Rep.GetElementSet(sql, 2);
            if (col.Count > 1)
            {
                MessageBox.Show("More than one 'LE Module' available.", "Can't inventory Module {Name}");
                return;
            }
            if (col.Count == 0) El = null;
            else El = (EA.Element)col.GetAt(0);
        }
        /// <summary>
        /// Generate an EA-Module into the defined package. It adds the EA Module reference if exists.
        /// - Component with Stereotype 'LE Modul'
        /// - <tbd>Tagged values are missing(ASIL level)</tbd>
        /// </summary>
        public void Generate(EA.Package pkg)
        {
            if (El == null)
            {

                // Create Module
                El = (EA.Element)pkg.Elements.AddNew(Name, "Component");
                El.Stereotype = @"LE Modul";
                El.Update();
                pkg.Elements.Refresh();



            }
            //-------------------------------------------------------
            // Create Ports
            var portNames = new List<string>();
            foreach (var functionItem in RequiredFunctions)
            {
                var interfaceName = functionItem.Interface.Name;
                if (!portNames.Contains(interfaceName)) portNames.Add(interfaceName);
            }
            foreach (var functionItem in ProvidedFunctions)
            {
                var interfaceName = functionItem.Interface.Name;
                if (!portNames.Contains(interfaceName)) portNames.Add(interfaceName);
            }

            var existingPorts = EaElement.GetEmbeddedElements(El);


            // Create new ports
            var newPorts = from s in portNames
                where existingPorts.All(i => s != i.Name)
                select s;
            // Delete not used ports
            var deletePorts = from s in existingPorts
                where portNames.All(i => s.Name != i)
                orderby s.Pos descending
                select s;

            // delete not needed ports
            foreach (var deletePort in deletePorts)
            {
                El.EmbeddedElements.Delete(deletePort.Pos);
            }
            El.EmbeddedElements.Refresh();


            // create new ports
            foreach (var portName in newPorts)
            {
                var newPort = (EA.Element)El.EmbeddedElements.AddNew(portName, "Port");
                newPort.Name = portName;
            }
        }


        /// <summary>
        /// Inventory required functions from file
        /// </summary>
        public void InventoryRequiredFunctionsFromTextFile(string filePath, Functions.Functions functions, List<string> functionsNotFound)
        {
            string t = hoReverse.hoUtils.HoUtil.ReadAllText(filePath);
            if (t == "") return;
            string s = hoService.DeleteComment(t);

            Regex rgx = new Regex(@"(\w+)\(", RegexOptions.Multiline);
            Match match = rgx.Match(s);
            while (match.Success)
            {
                string functionCode = match.Groups[1].Value;
                FunctionItem functionItem;
                if (functions.FunctionList.TryGetValue(functionCode, out functionItem))
                {
                    // function found
                    RequiredAdd(functionItem);
                }
                else
                {
                    // function not found
                    if (!functionsNotFound.Contains(functionCode))
                    {
                        functionsNotFound.Add($@"{functionCode}");
                    }
                    //MessageBox.Show($"Function: '{functionCode}' not found in file\r\n'{filePath}'", "Function not find");
                }
                match = match.NextMatch();

            }
            return;
        }
    }
}
