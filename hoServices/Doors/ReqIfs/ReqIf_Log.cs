namespace EaServices.Doors.ReqIfs
{
    /// <summary>
    /// Structure to log activities
    /// </summary>
    public class ReqIfLog
    {
        public string File { set; get; }
        public string PkgName { set; get; }
        public string PkgGuid { set; get; }
        public string ModuleId { set; get; }
        public string Comment { set; get; }

        /// <summary>
        /// A ReqIfLog Entry
        /// </summary>
        /// <param name="reqIfFile"></param>
        /// <param name="pkgName"></param>
        /// <param name="pkgGuid"></param>
        /// <param name="moduleId"></param>
        /// <param name="comment"></param>
        public ReqIfLog(string reqIfFile, string pkgName, string pkgGuid, string moduleId, string comment)
        {
            File = reqIfFile;
            PkgName = pkgName;
            PkgGuid = pkgGuid;
            ModuleId = moduleId;
            Comment = comment;
        }

    }
}
