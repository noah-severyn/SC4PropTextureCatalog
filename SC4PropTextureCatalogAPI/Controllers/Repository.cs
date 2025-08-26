using SQLite;
using SC4PropTextureCatalogAPI.Models;
using System.Text;

namespace SC4PropTextureCatalogAPI.Controllers {
    public interface IItemRepository {
        
        Task<TGI> GetByIdAsync(int assetId);

        Task<List<TGI>> GetSearchResultsAsync(string search);

    }

    public class SqliteItemRepository(SQLiteAsyncConnection db) : IItemRepository {
        private readonly SQLiteAsyncConnection _db = db;



        //public Task<TGI> GetByIdAsync(int assetId) =>
        //    _db.FindAsync<TGI>(assetId).ContinueWith(t => t.Result);
        public async Task<TGI> GetByIdAsync(int assetId) {
            var results = await _db.QueryAsync<TGI>("select * from TGIs where AssetId = " + assetId);
            return results.FirstOrDefault();
            //return _db.FindAsync<TGI>(assetId).ContinueWith(t => t.Result);

        }

        public async Task<List<TGI>> GetSearchResultsAsync(string search) {
            string query = "SELECT TGIs.AssetId, TGIs.File, TGIs.TGI, TGIs.Type, TGITypes.Name TGIType, TGIs.Name FROM TGIs\n";
            query += "LEFT JOIN TGITypes ON TGIs.Type = TGITypes.Type\n";
            query += $"WHERE TGIs.AssetId LIKE '%{search}%' OR TGIs.File LIKE '%{search}%' OR TGIs.TGI LIKE '%{search}%' OR TGIs.Name LIKE '%{search}%'";

            var results = await _db.QueryAsync<TGI>(query.ToString());
            return results;
        }
            
    }
}
