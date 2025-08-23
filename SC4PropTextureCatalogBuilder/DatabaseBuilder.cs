using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using SQLite;
using System.IO;
using csDBPF;

namespace SC4PropTextureCatalogBuilder {
    /// <summary>
    /// An item in the TGI table, which tracks which TGIs are in which dependency pack. 
    /// </summary>s
    [Table("TGIs")]
    public class TGIItem(int exchId, int assetId, string file, string tgi, int type, string? exmpName) {
        /// <summary>
        /// Identifer of the exchange this item is uploaded to.
        /// </summary>
        [Column("ExchangeId")]
        public int ExchangeId { get; set; } = exchId;

        /// <summary>
        /// Identifier of this item on the exchange.
        /// </summary>
        [Column("AssetId")]
        public int AssetId { get; set; } = assetId;

        /// <summary>
        /// Name of the file within this package that contains this TGI
        /// </summary>
        [Column("File")]
        public string File { get; set; } = file;

        /// <summary>
        /// Fully qualified TGI
        /// </summary>
        [Column("TGI")]
        public string TGI { get; set; } = tgi;

        /// <summary>
        /// One of the <see cref="TgiType"/> enumerations.
        /// </summary>
        [Column("Type")]
        public int? Type { get; set; } = type;

        /// <summary>
        /// Name of this exemplar, if applicable.
        /// </summary>
        [Column("Name")]
        public string? Name { get; set; } = exmpName;

        public override string ToString() {
            return $"{TGI}: {AssetId}, {Type}, {Name}";
        }
    }

    /// <summary>
    /// An item in the Asset table. An asset referrs to a download or file, and is akin to a sc4pac asset.
    /// </summary>
    [Table("Assets")]
    public class AssetItem(int exchId, int assetId, string? version = null, string? stexUrl = null, string? sc4eUrl = null, string? pacUrl = null, string? author = null, int? primaryCat = null, int? secondaryCat = null) {
        /// <summary>
        /// Identifer of the exchange this item is uploaded to.
        /// </summary>
        [Column("ExchangeId")]
        public int ExchangeId { get; set; } = exchId;

        /// <summary>
        /// Identifier of this item on the exchange.
        /// </summary>
        [Column("AssetId")]
        public int AssetId { get; set; } = assetId;

        /// <summary>
        /// Version of this asset on the exchange.
        /// </summary>
        [Column("Version")]
        public string? Version { get; set; } = version;

        /// <summary>
        /// STEX URL, if applicable.
        /// </summary>
        [Column("StexUrl")]
        public string? StexUrl { get; set; } = stexUrl;

        /// <summary>
        /// SC4Evermore URL, if applicable.
        /// </summary>
        [Column("Sc4eUrl")]
        public string? Sc4eUrl { get; set; } = sc4eUrl;

        /// <summary>
        /// sc4pac URL, if applicable.
        /// </summary>
        [Column("PacUrl")]
        public string? PacUrl { get; set; } = pacUrl;

        /// <summary>
        /// Author of this asset pack
        /// </summary>
        [Column("Author")]
        public string? Author { get; set; } = author;

        /// <summary>
        /// Describes the contents of this asset as one or more of: Textures, Buildings, Flora, Fauna, People, Vehicles, Scenery, Helpers, Effects, Other, etc.
        /// </summary>
        [Column("PrimaryCat")]
        public int? PrimaryCat { get; set; } = primaryCat;

        /// <summary>
        /// Further categorizes each of the primary categories into subcategories
        /// </summary>
        [Column("SecondaryCat")]
        public int? SecondaryCat { get; set; } = secondaryCat;

        public override string ToString() {
            return $"Id:{ExchangeId}-{AssetId}, Version:{Version}, Author:{Author}, Primary:{PrimaryCat}, Secondary:{SecondaryCat}";
        }
    }

    /// <summary>
    /// Dimension/lookup table of TGI types (building, prop, texture, flora, cohort).
    /// </summary>
    [Table("TGITypes")]
    public class TGICategory(int type, string name) {
        [PrimaryKey]
        [Column("Type")]
        public int Type { get; set; } = type;

        [Column("Name")]
        public string Name { get; set; } = name;

        public override string ToString() {
            return $"{Type}: {Name}";
        }
    }



    /// <summary>
    /// Create and operate on the Prop Texture Catalog database.
    /// </summary>
    public class DatabaseBuilder {
        private readonly SQLiteConnection _db;

        /// <summary>
        /// Create a new SQLite database with the necessary tables and dimensional fields.
        /// </summary>
        /// <param name="dbPath">Path to save the database file to, including the file name.</param>
        /// <param name="create">Whether to create a new db structure or the structure already exists (opening a previously made db)</param>
        public DatabaseBuilder(string dbPath, bool create) {
            _db = new SQLiteConnection(dbPath);
            if (create) {
                _db.CreateTable<TGIItem>();
                _db.CreateTable<AssetItem>();
                _db.CreateTable<TGICategory>();
                _db.Insert(new TGICategory(-1, "Unknown"));
                _db.Insert(new TGICategory(0, "Building"));
                _db.Insert(new TGICategory(1, "Prop"));
                _db.Insert(new TGICategory(2, "Texture"));
                _db.Insert(new TGICategory(4, "Flora"));
                _db.Insert(new TGICategory(10, "Cohort"));
            }
        }


        /// <summary>
        /// Parse all DBPF files in a folder and return the found TGIs along with errors, if any.
        /// </summary>
        /// <param name="exchangeId">Id of the exchange this item is found on</param>
        /// <param name="folderPath">Folder path to scan</param>
        /// <returns></returns>
        public (List<TGIItem>, List<DBPFError>) ParseFolder(int exchangeId, int assetId, string folderPath) {
            string extractFolder = Path.Combine(folderPath, "ex");
            
            List<DBPFError> errors = [];
            List<TGIItem> items = [];

            
            //var extractFiles = Directory.EnumerateFiles(extractFolder, "*", SearchOption.AllDirectories);

            
            extractFiles = Directory.EnumerateFiles(extractFolder, "*", SearchOption.AllDirectories);
            var dbpfs = GetUniqueFilenamesAcrossFolders(extractFiles).FilterDBPFFiles();
            foreach (string file in dbpfs) {
                FileStream fs = WaitForFile(file, FileMode.Open);
                if (fs == null) {
                    errors.Add(new DBPFError(Path.GetFileName(file), DBPFTGI.BLANKTGI, "Opening file failed"));
                    Console.WriteLine("Could not open " + file);
                    continue;
                }
                DBPFFile dbpf = new DBPFFile(fs);

                var targetEntries = dbpf.ListOfEntries.Where(e => e.MatchesEntryType(DBPFTGI.FSH_BASE_OVERLAY) || e.MatchesEntryType(DBPFTGI.EXEMPLAR) || e.MatchesEntryType(DBPFTGI.COHORT));

                foreach (DBPFEntry entry in targetEntries) {
                    //Add Base/Overlay textures (look at the least significant 4 bits and only add if it is 0, 5, or A: AND the Instance by 0b1111 (0xF) and examine the modulus result)
                    if (entry.MatchesEntryType(DBPFTGI.FSH_BASE_OVERLAY) && ((entry.TGI.InstanceID & 0xF) % 5) == 0) {
                        items.Add(new TGIItem(exchangeId, assetId, dbpf.File.Name, entry.TGI.ToString(), 2, null));
                    }

                    //Add Exemplars
                    else if (entry.MatchesEntryType(DBPFTGI.EXEMPLAR)) {
                        DBPFEntryEXMP exmp = (DBPFEntryEXMP) entry;
                        exmp.Decode();
                        if (exmp.ListOfProperties.Count == 0) continue;

                        DBPFProperty.ExemplarType exmpType = exmp.GetExemplarType();
                        if (exmpType == DBPFProperty.ExemplarType.LotConfiguration) {
                            continue;
                        } else if (exmpType == DBPFProperty.ExemplarType.Error) {
                            errors.Add(new DBPFError(file, exmp.TGI, "missing property: ExemplarType"));
                            if (exmp.HasProperty("Demand Satisfied")) {
                                exmpType = DBPFProperty.ExemplarType.Building;
                            }
                        }

                        DBPFProperty prop = exmp.GetProperty("ExemplarName");
                        string exmpName;
                        if (prop is null) {
                            errors.Add(new DBPFError(file, exmp.TGI, "missing property: ExemplarName"));
                            exmpName = "";
                        } else {
                            exmpName = exmpName = (string) prop.GetData();
                        }

                        switch (exmpType) {
                            case DBPFProperty.ExemplarType.Building:
                                items.Add(new TGIItem(exchangeId, assetId, dbpf.File.Name, entry.TGI.ToString(), 0, exmpName));
                                break;
                            case DBPFProperty.ExemplarType.Prop:
                                items.Add(new TGIItem(exchangeId, assetId, dbpf.File.Name, entry.TGI.ToString(), 1, exmpName));
                                break;
                            case DBPFProperty.ExemplarType.FloraFauna:
                                items.Add(new TGIItem(exchangeId, assetId, dbpf.File.Name, entry.TGI.ToString(), 4, exmpName));
                                break;
                        }
                    }


                    //Add Cohorts (note the building/prop family of the cohort is always 0x10000000 less than the Cohort's Index)
                    else if (entry.MatchesEntryType(DBPFTGI.COHORT)) {
                        DBPFEntryEXMP exmp = (DBPFEntryEXMP) entry;
                        TGI family = new TGI(entry.TGI.TypeID, entry.TGI.GroupID, entry.TGI.InstanceID - 0x10000000);
                        exmp.Decode();
                        if (exmp.ListOfProperties.Count == 0) continue;
                        string exmpName;
                        var prop = exmp.GetProperty("ExemplarName");
                        if (prop == null) {
                            exmpName = "??";
                        } else {
                            exmpName = (string) prop.GetData();
                        }

                        items.Add(new TGIItem(exchangeId, assetId, dbpf.File.Name, entry.TGI.ToString(), 10, exmpName));
                    }
                }
            }
            try {
                Directory.Delete(extractFolder, true);
            }
            catch {
                Console.WriteLine("Could not delete " + extractFolder);
            }
            
            return (items, errors);
        }


        public List<string> GetUniqueFilenamesAcrossFolders(IEnumerable<string> filePaths) {
            Dictionary<string, string> uniques = [];
            foreach (string file in filePaths) {
                string fileName = Path.GetFileName(file);
                if (!uniques.ContainsKey(fileName)) {
                    uniques.Add(fileName, file);
                }
            }
            return uniques.Values.ToList();
        }

        /// <summary>
        /// Adds a series of TGIs to the database.
        /// </summary>
        /// <param name="items">List of TGIItem objects to add</param>
        public void AddTGIs(List<TGIItem> items) {
            if (items.Count == 0) {
                return; 
            }

            if (!AssetExists(items[0].ExchangeId, items[0].AssetId)) {
                _db.Insert(new AssetItem(items[0].ExchangeId, items[0].AssetId));
            }

            foreach (TGIItem item in items) {
                _db.Insert(item);
            }
        }



        /// <summary>
        /// Return whether this asset exists in the Assets table
        /// </summary>
        /// <param name="exchangeId">Id of the exchange</param>
        /// <param name="assetId">Id of the asset</param>
        /// <returns>TRUE if the asset exists; FALSE otherwise</returns>
        public bool AssetExists(int exchangeId, int assetId) {
            int count = _db.ExecuteScalar<int>($"SELECT count(*) FROM Assets WHERE ExchangeId = '{exchangeId}' AND AssetId = '{assetId}'");
            return count != 0;
        }



        /// <summary>
        /// Return whether this package exists in the TGIs table
        /// </summary>
        /// <param name="tgi">TGI to find</param>
        /// <returns>TRUE if the TGI exists; FALSE otherwise</returns>
        private bool TGIExists(string tgi) {
            int count = _db.ExecuteScalar<int>($"SELECT count(*) FROM TGIs WHERE TGI = '{tgi}'");
            return count != 0;
        }



        /// <summary>
        /// Fetches the PNG thumbnail image for this TGI.
        /// </summary>
        /// <param name="tgi">TGI to use</param>
        /// <returns>A PNG image represented as bytes</returns>
        private static byte[] GetThumbnail(string tgi) {
            string fname = tgi.Replace("0x", "").Replace(", ", "-") + ".png";
            try {
                return File.ReadAllBytes("C:\\source\\repos\\SC4PropTextureCatalog\\wwwroot\\img\\thumbnails\\" + fname);
            } catch {
                return new byte[0];
            }
        }
    }
}
