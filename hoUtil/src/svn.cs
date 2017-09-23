using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using EA;

namespace hoReverse.hoUtils.svnUtil
{
    public class svn
    {
        private string _vcPath;
        private Package _pkg;
        private Repository _rep;

        // constructor
        public svn(Repository rep, Package pkg)  {
            _pkg = pkg;
            _rep = rep;
            _vcPath = "";
            if (pkg.IsControlled)
            {

                _vcPath = HoUtil.GetVccFilePath(rep, pkg);
            }

            
        }

        public bool setProperty()
        {
            cmd("svn","propset svn:keywords \"Date Author Rev Id Header\" ");
            return true;
        }
        public void gotoLog()
        {
            string repUrl = getRepositoryPath();
            cmd("tortoiseProc", "/command:log /findtype:2 /path:",repUrl);
        }
        public string getLockingUser()
        {
            string repUrl = getRepositoryPath();
            string s = cmd("svn", "info " + repUrl);
            Match match = Regex.Match(s, @"Lock Owner:[\s]*([A-Za-z]*)");
            return match.Groups[1].Value;
        }
        public void gotoRepoBrowser()
        {
            string repUrl = getRepositoryPath();
            cmd("tortoiseProc", "/command:repobrowser /path:", repUrl);
        }
        private string getRepositoryPath()
        {
            string url = "";
            string s = cmd("svn", "info");
            string[] lines = Regex.Split(s, "\r\n");
            url = lines[3].Substring(5);
            
            return url;
        }

        //proc: svn, tortoiseProc
        public string cmd(string proc, string cmd, string url="")
        {
            string returnString = "";
            
            ProcessStartInfo psi = new ProcessStartInfo(proc);
            string path = _vcPath;
            if (!url.Equals("")) path = url;
            if (path == null) return returnString;
            string space = "";
            if (proc.ToUpper().Equals("SVN")) space = " "; 

            psi.Arguments = cmd + space + "\"" + path + "\"";  // wrap file name in " to avoid problems with blank in name
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.CreateNoWindow = true;
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
                p.WaitForExit(20000);
                if (p.HasExited)
                {
                    if (p.ExitCode != 0)
                    {
                        MessageBox.Show("ErrorCode:"+p.ExitCode + "\r\n" + standardError.ReadToEnd(),"svn");
                       return "Error";
                    }
                    return output.ReadToEnd();

                }
                MessageBox.Show("Error: Timeout","svn");
                return "Error: Timeout";
            }
            catch (Exception e)
            {
                MessageBox.Show(e +
                    "\r\n\r\nCommand:" + psi + " " + psi.Arguments, "Error svn");
            }


            return returnString;
        }
    }
}
