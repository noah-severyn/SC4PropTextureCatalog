using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SQLite;

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
            public string PackName { get; set; } = string.Empty;
            public string PackVersion { get; set; } = string.Empty;
            public string Hyperlink { get; set; } = string.Empty;
            public string TGI { get; set; } = string.Empty;
            public string TGIName { get; set; } = string.Empty;
            public string Author { get; set; } = string.Empty;
            public string ExemplarName { get; set; } = string.Empty;
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

            /* SELECT PackTable.PackName, TGITable.TGI,TGITypes.TGIName ,PackTable.Author, TGITable.ExemplarName FROM TGITable
             * LEFT JOIN PackTable ON TGITable.PackID = PackTable.PackID
             * LEFT JOIN TGITypes ON TGITable.TGIType = TGITypes.TGIType
             * WHERE PackName LIKE '%fire%' OR TGI LIKE '%fire%' OR Author LIKE '%fire%' OR ExemplarName LIKE '%fire%' */
            StringBuilder query = new StringBuilder();
            query.AppendLine("SELECT PackTable.PackName, PackTable.Hyperlink, TGITable.TGI, TGITable.TGIType, TGITypes.TGIName, PackTable.Author, TGITable.ExemplarName FROM TGITable");
            query.AppendLine("LEFT JOIN PackTable ON TGITable.PackID = PackTable.PackID");
            query.AppendLine("LEFT JOIN TGITypes ON TGITable.TGIType = TGITypes.TGIType");
            query.AppendLine($"WHERE PackName LIKE '%{search}%' OR TGI LIKE '%{search}%' OR Author LIKE '%{search}%' OR ExemplarName LIKE '%{search}%'");
            ListOfRecords =  connection.Query<CatalogRecord>(query.ToString());
            connection.Close();
        }

        public void OnGet() {
            SQLiteConnection connection = InitialiseConnection();
            int countTGIs = connection.Query<QueryCount>("SELECT TGI FROM TGITable").Count;
            int countThumbs = Directory.EnumerateFiles("wwwroot\\img\\thumbnails").Count();
            ThumbnailCoverage = ((double) countThumbs) / countTGIs;
            connection.Close();
        }
        
    }
}