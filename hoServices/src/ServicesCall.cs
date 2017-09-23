using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;

// ReSharper disable once CheckNamespace
namespace hoReverse.Services
{
    public class ServiceCall
    {
        // ReSharper disable once NotAccessedField.Local
        private bool _isTextRequired;

        public ServiceCall(MethodInfo method, string guid, string description, string help, bool isTextRequired)
        {
            this.Method = method;
            this.Description = description;
            this.Guid = guid;
            this.Help = help;
            _isTextRequired = isTextRequired;
        }
        
        
       
        public String Description { get; }

        public MethodInfo Method { get; }

        public String Help { get; }

        public String Guid { get; }
    }

    /// <summary>
    /// Sort ServicesCalls against column Description. Use Interface IComparable.
    /// </summary>
    public class ServicesCallDescriptionComparer : IComparer<ServiceCall>
    {
        public int Compare(ServiceCall x, ServiceCall y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return 1;
            if (y == null) return -1;
            return String.Compare(x.Description, y.Description, StringComparison.Ordinal);
        } 
    }

    /// <summary>
    /// Sort/Search ServicesCalls against column GUID. Use Interface IComparable.
    /// </summary>
    public class ServicesCallGuidComparer : IComparer<ServiceCall>
    {
        public int Compare(ServiceCall x, ServiceCall y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return 1;
            if (y == null) return -1;
            return String.Compare(x.Guid, y.Guid, StringComparison.Ordinal);
        }
    }

    /// <summary>
    /// Class to define the configurable services
    /// </summary>
    public class ServicesCallConfig
    {
        private bool _isTextRequired;
        public ServicesCallConfig(int pos, string guid, string buttonText)
        {
            Guid = guid;
            ButtonText = buttonText;
        }

        // ReSharper disable once UnusedMember.Local
        public string Invoke(EA.Repository rep, string text)
        {
            object s = null;
            if (Method != null)
            {
                try {
                    // Invoke the method itself. The string returned by the method winds up in s
                    // substitute default parameter by Type.Missing
                    if (_isTextRequired)
                    {
                        // use Type.Missing for optional parameters
                        switch (Method.GetParameters().Length)
                        {
                            case 1:
                                Method.Invoke(null, parameters: new object[] { rep, text });
                                break;
                            case 2:
                                Method.Invoke(null, new[] { rep, text, Type.Missing });
                                break;
                            case 3:
                                Method.Invoke(null, new[] { rep, text, Type.Missing, Type.Missing });
                                break;
                            default:
                                Method.Invoke(null, new[] { rep, text, Type.Missing, Type.Missing, Type.Missing });
                                break;
                        }
                    }
                    else
                    {
                        // use Type.Missing for optional parameters
                        switch (Method.GetParameters().Length)
                        {
                            case 1:
                                Method.Invoke(null, new object[] { rep });
                                break;
                            case 2:
                                Method.Invoke(null, new[] { rep, Type.Missing });
                                break;
                            case 3:
                                Method.Invoke(null, new[] { rep, Type.Missing, Type.Missing });
                                break;
                            default:
                                Method.Invoke(null, new[] { rep, Type.Missing, Type.Missing, Type.Missing });
                                break;
                        }

                    }
                } catch (Exception e)
                {
                    MessageBox.Show(e +  "\nCan't invoke " + Method.Name + "Return:'"+ Method.ReturnParameter + "' "+Method,"Error Invoking service");
                    return (string)s;
                }
            }
            return null;
        }
        public string Help { get; set; }

        public MethodInfo Method { get; set; }

        private string MethodName {
            get
            {
                if (Method == null) return "";
                else return Method.Name;
            }

        }
        public string Description { private get; set; }

        public string HelpTextLong
        {
            get
            {
                if (MethodName == "") return "";
                return "MethodName\t:\t"+ MethodName + "()\nDescription1\t:\t" + Description + "\nDescription2\t:\t" + this.Help;
            }

        }

        public bool IsTextRequired
         {
             get => _isTextRequired;
            set => _isTextRequired = value;
        }
         public string Guid { get; set; }

        public string ButtonText { get; set; }
    }
}
