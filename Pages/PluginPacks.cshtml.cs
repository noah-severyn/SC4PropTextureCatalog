using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SQLite;
using static SC4PropTextureCatalog.Pages.ViewPackModel;

namespace SC4PropTextureCatalog.Pages
{
    public class PluginPacksModel : PageModel {

        public PluginPackRecord PluginPack = new PluginPackRecord();
        public string? SearchID = null;
        public int PluginPackCount = 0;

        [Table("PackTableRecords")]
        public class PluginPackRecord {
            public int ID { get; set; } = 0;
            public long DECID { get; set; } = 0;
            public string HEXID { get; set; } = string.Empty;
            public string FileName { get; set; } = string.Empty;
        }

        [Table("Count")]
        public class CountRecord {
            public int Count { get; set; }
        }

        /// <summary>
        /// Creates a connection to the PluginPack database.
        /// </summary>
        /// <returns>The database connection</returns>
        private static SQLiteConnection InitialiseConnection() {
            string source = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data\\PluginPacks.db");
            SQLiteConnectionString options = new SQLiteConnectionString(source, false);
            return new SQLiteConnection(options);
        }

        /// <summary>
        /// Returns a <see cref="PackTableRecord"/> from the PackTable with the specified pack name.
        /// </summary>
        /// <param name="packname">Name of pack to return</param>
        public void GetPluginPack() {
            if (SearchID is null) {
                return;
            }

            SQLiteConnection connection = InitialiseConnection();
            StringBuilder query = new StringBuilder();

            /* SELECT DECID, HEXID, FileName from PluginPackIDs
             * WHERE DECID = SearchID OR HEXID = 'SearchID' */
            query.AppendLine("SELECT DECID, HEXID, FileName from PluginPackIDs");
            try {
                long decid = Convert.ToInt64(SearchID, 10);
                query.AppendLine($"WHERE DECID = {decid} OR ");
            }
            catch (FormatException) {
                query.AppendLine("WHERE ");
            } finally {
                query.Append($"HEXID = '{SearchID.ToUpper().Replace("0X", string.Empty)}'");
            }

            List<PluginPackRecord> queryResult = connection.Query<PluginPackRecord>(query.ToString());
            if (queryResult.Count > 0) {
                PluginPack = queryResult.First();
                if (!PluginPack.HEXID.StartsWith("0x")) {
                    PluginPack.HEXID = "0x" + PluginPack.HEXID;
                }
            } else {
                PluginPack.FileName = "#####"; //Indicate no files were returned, as opposed to uninitialized
            }
            
            connection.Close();
        }


        public void OnGet() {
            SQLiteConnection connection = InitialiseConnection();
            var queryout = connection.Query<CountRecord>("SELECT count(DECID) FROM PluginPackIDs");
            PluginPackCount = 15231;
            connection.Close();
        }
    }
}
