using SQLite;
using SC4PropTextureCatalogAPI.Models;
using System.Text;

namespace SC4PropTextureCatalogAPI.Controllers {
    public interface IItemRepository {
        
        Task<CatalogItems> GetByIdAsync(int assetId);

        Task<List<CatalogItems>> GetSearchResultsAsync(string search);

        Task<List<CatalogItems>> GetByInstanceAsync(string instance);

    }

    public class SqliteItemRepository(SQLiteAsyncConnection db) : IItemRepository {
        private readonly SQLiteAsyncConnection _db = db;


        public async Task<CatalogItems> GetByIdAsync(int assetId) {
            var results = await _db.QueryAsync<CatalogItems>("select * from CatalogItems where AssetId = " + assetId);
            return results.FirstOrDefault();
        }

        public async Task<List<CatalogItems>> GetSearchResultsAsync(string search) {
            string query = "SELECT CatalogItems.AssetId, CatalogItems.File, CatalogItems.TGI, CatalogItems.Type, TGITypes.Name TGIType, CatalogItems.Name FROM CatalogItems\n";
            query += "LEFT JOIN TGITypes ON CatalogItems.Type = TGITypes.Type\n";
            query += $"WHERE CatalogItems.AssetId LIKE '%{search}%' OR CatalogItems.File LIKE '%{search}%' OR CatalogItems.TGI LIKE '%{search}%' OR CatalogItems.Name LIKE '%{search}%'";

            var results = await _db.QueryAsync<CatalogItems>(query.ToString());
            return results;
        }

        public async Task<List<CatalogItems>> GetByInstanceAsync(string instance) {
            string query = "SELECT CatalogItems.AssetId, CatalogItems.File, substr(CatalogItems.TGI, -8) Instance, CatalogItems.TGI, CatalogItems.Type, TGITypes.Name TGIType, CatalogItems.Name FROM CatalogItems\n";
            query += "LEFT JOIN TGITypes ON CatalogItems.Type = TGITypes.Type\n";
            query += $"WHERE Instance LIKE '%{instance}%'";
            var results = await _db.QueryAsync<CatalogItems>(query.ToString());
            return results;
        }

    }
}
