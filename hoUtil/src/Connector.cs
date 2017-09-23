using System;
using System.Collections.Generic;
using System.ComponentModel;

// ReSharper disable once CheckNamespace
namespace hoReverse.Connectors {
    public class Connector
    {
        private readonly bool _isDefault;
        private readonly bool _isEnabled;


        public Connector(string type, string stereotype, string lineStyle="LV", bool isDefault=false, bool isEnabled = true)
        {
            Type = type;
            Stereotype = stereotype;
            _isDefault = isDefault;
            _isEnabled = isEnabled;
            LineStyle = lineStyle;
        }
        public static List<string> GetLineStyle()
        {
            return new List<String> {
        "LV","LH","TV","TH","B","OS","OR","D"};
        }
        public string LineStyle { get; }

        public string Type { get; }

        public int Pos { get; set; }

        public string Stereotype { get; }

        public bool IsDefault
        {
            get => _isDefault;
            set => IsDefault = value;
        }
        public bool IsEnabled
        {
            get => _isEnabled;
            set => IsEnabled = value;
        }
    }
    public class DiagramConnector : BindingList<Connector>
    {
        public DiagramConnector(string diagramType)
        {
                DiagramType = diagramType;
            }
        public string DiagramType { get; set; }
    }
    public class LogicalConnectors : DiagramConnector
    {

        public LogicalConnectors() : base("Logical") 
        {
           
        }
        public List<String> GetConnectorTypes()
        {
            return new List<String> {
                "Abstraction", "Association", "Aggregate", "Assembly", "Compose","Delegate", "Dependency","Generalize", "InformationFlow",  "Nesting", "NoteLink", "Realisation", "Usage"
            };
        }
        public List<String> GetStandardStereotypes()
        {
            return new List<String> {
                "trace", "trace1", "trace2"
            };
        }
       
    }
    public class ActivityConnectors : DiagramConnector
    {

        public ActivityConnectors()
            : base("Activity")
        {

        }
        public List<String> GetConnectorTypes()
        {
            return new List<String> {
                "DataFlow", "ControlFlow"
            };
        }
        public List<String> GetStandardStereotypes()
        {
            return new List<String>();
        }

    }

}
