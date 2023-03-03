using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SQLite;

namespace SC4PropTextureCatalog.Pages {
    public class ViewPackModel : PageModel {

        public List<Record> ListOfPacks = new List<Record>();
        public string? SelectedPackName = null;
        public PackTableRecord SelectedPack = new PackTableRecord();

        public List<TGI_Record> TextureRecords = new List<TGI_Record>();
        public List<TGI_Record> PropRecords = new List<TGI_Record>();
        public int TextureCount = 0;
        public int PropCount = 0;

        /// <summary>
        /// An item returned as a result of a query to the Catalog database.
        /// </summary>
        [Table("Records")]
        public class Record {
            public string PackName { get; set; } = string.Empty;
        }


        [Table("PackTableRecords")]
        public class PackTableRecord {
            public string PackName { get; set; } = string.Empty;
            public string PackVersion { get; set; } = string.Empty;
            public string Hyperlink { get; set; } = string.Empty;
            public string Author { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;
        }

        [Table("TGIRecords")]
        public class TGI_Record {
            public string TGI { get; set; } = string.Empty;
            public int TGIType { get; set; }
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
        /// Fetches a list of all dependency packs to use as choices for the drop down menu.
        /// </summary>
        private void SetPackList() {
            SQLiteConnection connection = InitialiseConnection();

            /* SELECT PackName from PackTable
             * ORDER BY PackName ASC
             */
            StringBuilder query = new StringBuilder();
            query.AppendLine("SELECT PackName from PackTable");
            query.AppendLine("ORDER BY PackName ASC");

            ListOfPacks = connection.Query<Record>(query.ToString());
            connection.Close();
        }

        /// <summary>
        /// Returns a <see cref="PackTableRecord"/> from the PackTable with the specified pack name.
        /// </summary>
        /// <param name="packname">Name of pack to return</param>
        public void SetPackInfo() {
            if (SelectedPackName is null) {
                return;
            }
            SQLiteConnection connection = InitialiseConnection();
            string packname = SelectedPackName.Replace("'", "''");

            /* SELECT PackName, PackVersion, Hyperlink, Author, Type from PackTable
             * WHERE PackName = '11241036 Textures' */
            StringBuilder query = new StringBuilder();
            query.AppendLine("SELECT PackName, PackVersion, Hyperlink, Author, Type from PackTable");
            query.AppendLine($"WHERE PackName = '{packname}'");
            SelectedPack = connection.Query<PackTableRecord>(query.ToString()).First();

            /* SELECT TGITable.TGI, TGITable.TGIType from TGITable
             * LEFT JOIN PackTable ON TGITable.PackID = PackTable.PackID
             * WHERE PackName = '11241036 Textures' and TGIType = 2 */
            query.Clear();
            query.AppendLine("SELECT TGITable.TGI, TGITable.TGIType from TGITable");
            query.AppendLine("LEFT JOIN PackTable ON TGITable.PackID = PackTable.PackID");
            query.AppendLine($"WHERE PackName = '{packname}' and TGIType = 1");
            PropRecords = connection.Query<TGI_Record>(query.ToString());
            PropCount = PropRecords.Count();
            query.Clear();
            query.AppendLine("SELECT TGITable.TGI, TGITable.TGIType from TGITable");
            query.AppendLine("LEFT JOIN PackTable ON TGITable.PackID = PackTable.PackID");
            query.AppendLine($"WHERE PackName = '{packname}' and TGIType = 2");
            TextureRecords = connection.Query<TGI_Record>(query.ToString());
            TextureCount = TextureRecords.Count();            
            connection.Close();
        }


        public void OnGet() {
            SetPackList();
        }
    }
}
