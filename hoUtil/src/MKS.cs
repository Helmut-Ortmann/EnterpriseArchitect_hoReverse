﻿using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using EA;

namespace hoReverse.hoUtils.MksUtil
{
    class Mks
    {
        private string _vcPath;
        private Package _pkg;
        private Repository _rep;

        // constructor
        public Mks(Repository rep, Package pkg)  {
            _pkg = pkg;
            _rep = rep;
            _vcPath = "";
            if (pkg.IsControlled)
            {

                _vcPath = HoUtil.GetVccFilePath(rep, pkg);
            }

            
        }
        
        public bool getNewest()
        {
            // check nested packages
            foreach (Package nestedPkg in _pkg.Packages)
            {
                Mks mks = new Mks(_rep, nestedPkg);
                mks.getNewest();
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
                    Project prj = _rep.GetProjectInterface();
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
            return cmd("viewhistory");
        }

        public string Checkout()
        {
            if (_vcPath == null) return "";
            string txt = cmd("co --batch --lock --forceConfirm=yes");
            //string txt = this.cmd("co --batch --nolock --unlock");
            return txt;
        }

        public string UndoCheckout()
        {
            if (_vcPath == null) return "";
            string txt = cmd("unlock --action=remove --revision :head");
            return txt;
        }

        private string cmd(string cmd)
        {
            string returnString = "";
            if (_vcPath == null) return returnString;
            ProcessStartInfo psi = new ProcessStartInfo(@"si");
            psi.Arguments = cmd + " \"" + _vcPath + "\"";  // wrap file name in " to avoid problems with blank in name
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.UseShellExecute = false;
            StreamReader output;
            StreamReader standardError;
            Process p;
            try
            {
                p = Process.Start(psi);
                output = p.StandardOutput;
                standardError = p.StandardError;
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
                MessageBox.Show("Error: Timeout","mks");
                return "Error: Timeout";
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
