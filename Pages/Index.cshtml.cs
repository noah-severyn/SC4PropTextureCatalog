using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SQLite;

namespace SC4PropTextureCatalog.Pages {
    public class IndexModel : PageModel {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger) {
            _logger = logger;
        }

        /// <summary>
        /// An item returned as a result of a query to the Catalog database.
        /// </summary>
        [Table("records")]
        public class Record {
            public string PackName { get; set; } = string.Empty;
            public string PackVersion { get; set; } = string.Empty;
            public string Hyperlink { get; set; } = string.Empty;
            public string TGI { get; set; } = string.Empty;
            public string TGIName { get; set; } = string.Empty;
            public string Author { get; set; } = string.Empty;
            public string ExemplarName { get; set; } = string.Empty;
        }

        /// <summary>
        /// Creates a connection to the Catalog database.
        /// </summary>
        /// <returns>The database connection</returns>
        public SQLiteConnection InitialiseConnection() {
            string source = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data\\Catalog.db");
            SQLiteConnectionString options = new SQLiteConnectionString(source, false);
            return new SQLiteConnection(options);
        }

        /// <summary>
        /// Returns a list of each item where a value in any column matches the search text.
        /// </summary>
        /// <param name="connection">SQLiteConnection to use</param>
        /// <param name="searchtext">Text to search for</param>
        /// <returns>A list of matching records</returns>
        public List<Record> GetRecords(SQLiteConnection connection,string? searchtext) {
            if (searchtext is null || searchtext.Length < 3) {
                return new List<Record>();
            }

            /* SELECT PackTable.PackName, TGITable.TGI,TGITypes.TGIName ,PackTable.Author, TGITable.ExemplarName FROM TGITable
             * LEFT JOIN PackTable ON TGITable.PackID = PackTable.PackID
             * LEFT JOIN TGITypes ON TGITable.TGIType = TGITypes.TGIType
             * WHERE PackName LIKE '%fire%' OR TGI LIKE '%fire%' OR Author LIKE '%fire%' OR ExemplarName LIKE '%fire%' 
             */
            StringBuilder query = new StringBuilder();
            query.AppendLine("SELECT PackTable.PackName, PackTable.Hyperlink, TGITable.TGI, TGITable.TGIType, TGITypes.TGIName, PackTable.Author, TGITable.ExemplarName FROM TGITable");
            query.AppendLine("LEFT JOIN PackTable ON TGITable.PackID = PackTable.PackID");
            query.AppendLine("LEFT JOIN TGITypes ON TGITable.TGIType = TGITypes.TGIType");
            query.AppendLine($"WHERE PackName LIKE '%{searchtext}%' OR TGI LIKE '%{searchtext}%' OR Author LIKE '%{searchtext}%' OR ExemplarName LIKE '%{searchtext}%'");
            return connection.Query<Record>(query.ToString());
        }

        /// <summary>
        /// Closes the database connection.
        /// </summary>
        /// <param name="connection">Connection to close</param>
        public void CloseConnection(SQLiteConnection connection) {
            connection.Close();
        }

        public void OnGet() {

        }
    }
}