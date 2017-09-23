using System.Collections.Generic;
using EaServices.Files;
using EA;

namespace EaServices.Functions
{

    /// <summary>
    /// A function:
    /// - Published by one Interface
    /// - Implemented by one Module
    /// - Operation is part of the one Interface
    /// </summary>
    public class FunctionItem
    {
        // The Interface it's defined in
        private InterfaceItem _interface;
        // The Module the interface is defined in 
        private ModuleItem _module;
        private string _name;
        // Function defined in an Interface
        private EA.Method _op;

        private string _returnType;
        private bool _isStatic;
        private List<ParameterItem> _parameterList;



        public FunctionItem(string name, string returnType, bool isStatic, List<ParameterItem> par)
        {
            FunctionItemInit(name, returnType, isStatic, par, null, null);
        }
        public FunctionItem(string name, string returnType, bool isStatic, List<ParameterItem> par, InterfaceItem interfaceItem, EA.Method method)
        {
            FunctionItemInit(name, returnType, isStatic, par, interfaceItem, method);
        }
        private void FunctionItemInit(string name, string returnType, bool isStatic, List<ParameterItem> par, InterfaceItem interfaceItem, EA.Method method)
        {
            _name = name;
            _parameterList = par;
            _returnType = returnType;
            _isStatic = isStatic;

            _interface = interfaceItem;
            _module = null;
            _op = method;
        }



        public InterfaceItem Interface { get => _interface; set => _interface = value; }
        public ModuleItem Module { get => _module; set => _module = value; }

        public Method Op { get => _op; set => _op = value;}
        public string Name { get => _name; set => _name = value; }
        public List<ParameterItem> ParameterList { get => _parameterList; }

        public string ReturnType => _returnType;
        public bool IsStatic => _isStatic;
    }

    public class ParameterItem
    {
        public string Name { get; }
        public string Type { get; }
        public bool IsConst { get; }
        public int Position { get;  }

        public ParameterItem(int position, string name, string type, bool isConst)
        {
            Name = name;
            Type = type;
            IsConst = isConst;
            Position = position;

        }
    }
}
