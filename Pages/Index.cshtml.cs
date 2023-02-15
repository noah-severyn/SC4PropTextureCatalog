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
            public string Title { get; set; } = string.Empty;
            public string TGI { get; set; } = string.Empty;
            public string Author { get; set; } = string.Empty;
            public string ExmpName { get; set; } = string.Empty;
        }

        /// <summary>
        /// Creates a connection to the Catalog database.
        /// </summary>
        /// <returns>The database connection</returns>
        public SQLiteConnection InitialiseConnection() {
            string source = "C:\\source\\repos\\SC4PropTextureCatalog\\SC4PropTextureCatalog\\Data\\Catalog.db";
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

            StringBuilder query = new StringBuilder();
            query.AppendLine("SELECT PackTable.Title, TGITable.TGI, PackTable.Author, TGITable.ExmpName FROM TGITable");
            query.AppendLine("LEFT JOIN PackTable ON TGITable.PackID = PackTable.PackID");
            query.AppendLine($"WHERE Title LIKE '%{searchtext}%' OR TGI LIKE '%{searchtext}%' OR Author LIKE '%{searchtext}%' OR ExmpName LIKE '%{searchtext}%'");

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