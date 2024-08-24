using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SQLite;

namespace SC4PropTextureCatalog.Pages {
    public class AboutModel : PageModel {
        public int CountTGIs;
        public int CountPacks;
        public int CountThumbnails;
        public double ThumbnailCoverage;

        private class QueryCount {
            public int Count { get; set; }
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

        public void OnGet() {
            SQLiteConnection connection = InitialiseConnection();
            CountTGIs = connection.Query<QueryCount>("SELECT TGI FROM TGITable").Count;
            CountPacks = connection.Query<QueryCount>("SELECT PackName FROM PackTable").Count;
            CountThumbnails = Directory.EnumerateFiles("wwwroot\\img\\thumbnails").Count();
            ThumbnailCoverage = ((double) CountThumbnails) / CountTGIs;
            connection.Close();
        }
    }
}
