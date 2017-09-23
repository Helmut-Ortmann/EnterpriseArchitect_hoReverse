using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Windows.Forms;
using EaServices.Functions;

namespace EaServices.Files
{
    public class InterfaceItem : FileItem
    {


        public InterfaceItem(string filePath, EA.Repository rep) : base(filePath, rep)
        {
        }

        /// <summary>
        /// Inventory a Module. It adds the EA Interface reference to the Interface Element if exists.
        /// </summary>
        public void Inventory()
        {
            // get interface: Interface
            string sql = $@"select object_id 
                              from t_object 
                              where name = '{Name}'             AND
                                    object_type = 'Interface' ";
            // Get Element list from sql=2
            EA.Collection col = Rep.GetElementSet(sql, 2);
            if (col.Count > 1)
            {
                MessageBox.Show("More than one 'Interface' available.", "Can't inventory Interface {Name}");
                return;
            }
            if (col.Count == 0) El = null;
            else
            {  // Interface existing
                El = (EA.Element)col.GetAt(0);
                List<MethodItem> methods = new List<MethodItem>();
                foreach (EA.Method m in El.Methods)
                {
                    methods.Add(new MethodItem(m, m.Name));
                }
                // update existing methods
                var existingMethods = from provided in ProvidedFunctions
                                      join ea in methods on provided.Name equals ea.Name
                                      select new { ea.Method, provided};
                foreach (var f in existingMethods)
                {
                    f.provided.Op = f.Method;
                }


            }
        }

        /// <summary>
        /// Generate an EA-Interface into the defined package. It adds the EA Interface reference if exists.
        /// </summary>
        public int Generate(EA.Package pkg)
        {
            int newMethods = 0;
            if (El == null)
            {
                // Create Interface
                El = (EA.Element)pkg.Elements.AddNew(Name, "Interface");
                El.Update();
            }
            // Generate operations for Interface
            foreach (FunctionItem functionItem in ProvidedFunctions)
            {
                EA.Method m = functionItem.Op;
                bool isUpdate = true;
                if (m == null)
                {
                    isUpdate = false;
                    // new function
                    m = (EA.Method) El.Methods.AddNew(functionItem.Name, "Operation");
                    newMethods++;
                }
                m.ReturnType = functionItem.ReturnType;
                m.Update();
                if (isUpdate) DeleteParameter(m);
                foreach (ParameterItem par in functionItem.ParameterList)
                    {
                        string parName = par.Name;
                        EA.Parameter p = (EA.Parameter) m.Parameters.AddNew(parName, "Parameter");
                        p.Position = par.Position;
                        p.Type = par.Type;
                        p.IsConst = par.IsConst;
                        p.Update();
                        m.Parameters.Refresh();
                        functionItem.Op = m;
                }
                m.Parameters.Refresh();
                
                

                functionItem.Op = m;
            }
            El.Methods.Refresh();
            return newMethods;

        }

        private void DeleteParameter(EA.Method m)
        {
            for (short i = (short)(m.Parameters.Count - 1); i >= 0; i--)
            {
                m.Parameters.Delete(i);
                m.Parameters.Refresh();
            }
        }
    }

    public class MethodItem
    {
        public EA.Method Method { get; }
        public string Name { get; }

        public MethodItem(EA.Method method, string name)
        {
            Method = method;
            Name = name;
        }


    } 
}
