using System;
using System.Diagnostics;
using System.Windows.Forms;
using hoUtil;

namespace MksUtil
{
    class Mks
    {
        private readonly string _vcPath;
        private readonly EA.Package _pkg;
        private readonly EA.Repository _rep;

        // constructor
        public Mks(EA.Repository rep, EA.Package pkg)  {
            _pkg = pkg;
            _rep = rep;
            _vcPath = "";
            if (pkg.IsControlled)
            {

                _vcPath = Util.GetFilePath(rep, pkg);
            }

            
        }
        
        public bool GetNewest()
        {
            // check nested packages
            foreach (EA.Package nestedPkg in _pkg.Packages)
            {
                Mks mks = new Mks(_rep, nestedPkg);
                mks.GetNewest();
            }
            if (_pkg.IsControlled)
            {
                // 
                _rep.ShowInProjectView(_pkg);
                try
                {
                    // preference head revision
                    Mks mks = new Mks(_rep, _pkg);
                    mks.Checkout();

                    // load package
                    _rep.CreateOutputTab("Debug");
                    _rep.EnsureOutputVisible("Debug");
                    _rep.WriteOutput("Debug", _pkg.Name + " " + _pkg.Notes, 0);

                    //MessageBox.Show(_pkg.Name + " " + _pkg.Packages.Count.ToString() + " " + _pkg.PackageGUID, "CountBefore");
                    EA.Project prj = _rep.GetProjectInterface();
                    prj.LoadControlledPackage(_pkg.PackageGUID);


                    _rep.WriteOutput("Debug", _pkg.Name + " " + _pkg.Notes, 0);
                    //MessageBox.Show(_pkg.Name + " " + _pkg.Packages.Count.ToString() + " " + _pkg.PackageGUID, "CountAfter");


                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString(), "Error");
                }
            }
            


            return true;
        }

        public string ViewHistory()
        {
            if (_vcPath == null) return "";
            return this.Cmd("viewhistory");
        }

        public string Checkout()
        {
            if (_vcPath == null) return "";
            string txt = this.Cmd("co --batch --lock --forceConfirm=yes");
            //string txt = this.cmd("co --batch --nolock --unlock");
            return txt;
        }

        public string UndoCheckout()
        {
            if (_vcPath == null) return "";
            string txt = this.Cmd("unlock --action=remove --revision :head");
            return txt;
        }

        private string Cmd(string cmd)
        {
            string returnString = "";
            if (_vcPath == null) return returnString;
            ProcessStartInfo psi = new ProcessStartInfo(@"si");
            psi.Arguments = cmd + " \"" + _vcPath + "\"";  // wrap file name in " to avoid problems with blank in name
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.UseShellExecute = false;
            Process p;
            try
            {
                p = Process.Start(psi);
                var output = p.StandardOutput;
                var standardError = p.StandardError;
                //outputError = p.StandardError;
                p.WaitForExit(10000);
                if (p.HasExited)
                {
                    if (p.ExitCode != 0)
                    {
                        MessageBox.Show("ErrorCode:"+p.ExitCode + "\r\n" + standardError.ReadToEnd(),"mks");
                       return "Error";
                    }
                    return output.ReadToEnd();

                }
                else
                {
                    MessageBox.Show("Error: Timeout","mks");
                    return "Error: Timeout";
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e +
                    "\r\n\r\nCommand:" + psi + " " + psi.Arguments, "Error mks");
            }


            return returnString;
        }
    }
}
