using System;
using EA;
using hoReverse.hoUtils.Resources;

// ReSharper disable once CheckNamespace
namespace hoReverse.hoUtils.Favorites
{
    public class Favorite
    {
        private readonly Repository _rep;
        private readonly string _xrefGuid = "";
        private readonly string _clientGuid = "";

        #region Constructor
        public  Favorite(Repository rep, string clientGuid) {
            _rep = rep;
            _xrefGuid = Guid.NewGuid().ToString();
            _clientGuid = clientGuid;
        }
        public Favorite(Repository rep)
        {
            _rep = rep;
            
        }
        #endregion
        // ReSharper disable once UnusedMember.Global
        public static bool InstallSearches(Repository rep)
        {
            rep.AddDefinedSearches(Strings.SearchFavorite);
            return true;
        }
        #region save
        // ReSharper disable once UnusedMethodReturnValue.Global
        public bool Save()
        {

            Delete();
            // insert 
            string sql = @"insert into t_xref " + @"        (XrefID, Type, Client) " +
                         $@" VALUES ( '{_xrefGuid}','Favorite', '{_clientGuid}') ";
            _rep.Execute(sql);
            
            return true;
        }
        #endregion
        #region delete
        // ReSharper disable once UnusedMethodReturnValue.Global
        public bool Delete()
        {
            // delete all old on
            string sql = @"delete from t_xref " + $@"where Client = '{_clientGuid}'";
            _rep.Execute(sql);
            return true;
        }
        #endregion
        #region search
        public void Search()
        {
            _rep.RunModelSearch(Strings.SearchFavoriteName, "", "","");
        }
        #endregion
    }
}
