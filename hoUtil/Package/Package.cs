using EA;

namespace hoUtils.Package
{
    public static class Package
    {
        /// <summary>
        /// Get list of package ids as comma separated list
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="branch">The current Branch as comma separated list</param>
        /// <param name="pkgId">The current PackageID to add recursively to Branch</param>
        /// <returns></returns>
        public static string GetBranch(Repository rep, string branch, int pkgId)
        {
            if (pkgId > 0)
            {
                // add current package id
                if (branch == "") branch = pkgId.ToString();
                else branch = branch + ", " + pkgId;

                EA.Package pkg = rep.GetPackageByID(pkgId);
                foreach (EA.Package p in pkg.Packages)
                {
                    int newPkgId = p.PackageID;
                    string s = newPkgId.ToString();

                    branch = GetBranch(rep, branch, newPkgId);
                }
            }
            return branch;
        }
    }
}
