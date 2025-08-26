using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SQLite;
using System.IO;

namespace SC4PropTextureCatalog.Pages {
    public class IndexModel : PageModel {

        public string? SearchText;
        public bool ShowThumbs;
        public int ThumbSize;
        public List<CatalogRecord> ListOfRecords = new List<CatalogRecord>();
        public double ThumbnailCoverage;

        /// <summary>
        /// An item returned as a result of a query to the Catalog database.
        /// </summary>
        [Table("Records")]
        public class CatalogRecord {
            public string AssetId { get; set; } = string.Empty;
            //public string AssetVersion { get; set; } = string.Empty;
            //public string AssetLink { get; set; } = string.Empty;
            public string TGI { get; set; } = string.Empty;
            public string File { get; set; } = string.Empty;
            public string TGIType { get; set; } = string.Empty;
            //public string Author { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            /// <summary>
            /// Image path if it exists; Empty otherwise
            /// </summary>
            public string ImgSrc { get; set; } = string.Empty; 
        }

        private class QueryCount {
            public int Count { get; set; }
        }

        /// <summary>
        /// Creates a connection to the Catalog database.
        /// </summary>
        /// <returns>The database connection</returns>
        private static SQLiteConnection InitialiseConnection() {
            string source = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data\\Catalog.db");
            SQLiteConnectionString options = new SQLiteConnectionString(source, false);
            return new SQLiteConnection(options);
        }

        /// <summary>
        /// Fetches a list of each item where a value in any column matches the search text.
        /// </summary>
        public void SetRecords() {
            string? search = SearchText;
            if (search is null || search.Length < 3) {
                ListOfRecords = new List<CatalogRecord>();
            }
            SQLiteConnection connection = InitialiseConnection();
            search = search.Replace("'", "''");

            StringBuilder query = new StringBuilder();
            query.AppendLine("SELECT TGIs.AssetId, TGIs.File, TGIs.TGI, TGIs.Type, TGITypes.Name TGIType, TGIs.Name FROM TGIs");
            //query.AppendLine("LEFT JOIN Assets ON TGIs.AssetId = Assets.AssetId");
            query.AppendLine("LEFT JOIN TGITypes ON TGIs.Type = TGITypes.Type");
            query.AppendLine($"WHERE TGIs.AssetId LIKE '%{search}%' OR TGIs.File LIKE '%{search}%' OR TGIs.TGI LIKE '%{search}%' OR TGIs.Name LIKE '%{search}%'");
            ListOfRecords =  connection.Query<CatalogRecord>(query.ToString());
            connection.Close();

            //~/img/thumbnails/@(item.TGI.Replace("0x", "").Replace(", ", "-")).png
            foreach (var item in ListOfRecords) {
                string newPath = "~/img/thumbnails/" + item.TGI.Replace("0x", "").Replace(", ", "-") + ".png";
                if (System.IO.File.Exists(newPath)) {
                    item.ImgSrc = newPath;
                }
            }
        }

        public void OnGet() {
            SQLiteConnection connection = InitialiseConnection();
            int countTGIs = connection.Query<QueryCount>("SELECT TGI FROM TGIs").Count;
            int countThumbs = Directory.EnumerateFiles("wwwroot\\img\\thumbnails").Count();
            ThumbnailCoverage = ((double) countThumbs) / countTGIs;
            connection.Close();
        }
        
    }
}