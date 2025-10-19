using System;
using csDBPF;
using SQLite;

namespace SC4PropTextureCatalogBuilder {
    /// <summary>
    /// An item in the TGI table, which tracks which TGIs are in which dependency pack. 
    /// </summary>s
    [Table("CatalogItems")]
    public class CatalogItem(int exchId, int assetId, string file, string tgi, int type, string? exmpName) {
        [Column("ExchangeId")]
        public int ExchangeId { get; set; } = exchId;

        [Column("AssetId")]
        public int AssetId { get; set; } = assetId;

        [Column("File")]
        public string File { get; set; } = file;

        [Column("TGI")]
        public string TGI { get; set; } = tgi;

        /// <summary>
        /// One of the <see cref="TGICategory"/> enumerations.
        /// </summary>
        [Column("Category")]
        public int? Category { get; set; } = type;

        /// <summary>
        /// Item name, if applicable. Typically an exemplar name.
        /// </summary>
        [Column("Name")]
        public string? Name { get; set; } = exmpName;

        public override string ToString() {
            return $"{ExchangeId}-{AssetId} {TGI}: {AssetId}, {Category}, {Name}";
        }
    }

    ///// <summary>
    ///// An item in the Asset table. An asset referrs to a download or file, and is akin to a sc4pac asset.
    ///// </summary>
    //[Table("Packages")]
    //public class PackageItem(int exchId, int assetId, string? group = null, string? name = null, string? version = null, List<string>? websites = null, string? author = null, int? primaryCat = null, int? secondaryCat = null) {
    //    [Column("ExchangeId")]
    //    [NotNull]
    //    public int ExchangeId { get; set; } = exchId;
        
    //    [Column("AssetId")]
    //    [NotNull]
    //    public int AssetId { get; set; } = assetId;

    //    /// <summary>
    //    /// sc4pac package identifier in the format <c>group:name</c>.
    //    /// </summary>
    //    [Column("Package")]
    //    public string?  Package { get; set; } = $"{group}:{name}";

    //    [Column("Version")]
    //    public string? Version { get; set; } = version;

    //    /// <summary>
    //    /// Semicolon separated list of one or more urls
    //    /// </summary>
    //    [Column("Websites")]
    //    public string Websites { get; set; } = string.Join(';', websites ?? []);

    //    [Column("Author")]
    //    public string? Author { get; set; } = author;

    //    /// <summary>
    //    /// Describes the contents of this asset as one or more of: Textures, Buildings, Flora, Fauna, People, Vehicles, Scenery, Helpers, Effects, Other, etc.
    //    /// </summary>
    //    [Column("PrimaryCat")]
    //    public int? PrimaryCats { get; set; } = primaryCat;

    //    /// <summary>
    //    /// Further categorizes each of the primary categories into subcategories
    //    /// </summary>
    //    [Column("SecondaryCat")]
    //    public int? SecondaryCats { get; set; } = secondaryCat;

    //    public override string ToString() {
    //        return $"Id:{ExchangeId}-{AssetId}, Package:{Package}, Version:{Version}, Author:{Author}, Primary:{PrimaryCats}, Secondary:{SecondaryCats}";
    //    }
    //}

    /// <summary>
    /// An item in the Asset table. An asset referrs to a download or file, and is akin to a sc4pac asset.
    /// </summary>
    [Table("Assets")]
    public class AssetItem(int exchId, int assetId) {
        [NotNull]
        public int ExchangeId { get; set; } = exchId;
        [NotNull]
        public int AssetId { get; set; } = assetId;
        public string Version { get; set; } = string.Empty;
        public string LastModified { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }


    /// <summary>
    /// Dimension/lookup table of TGI types (building, prop, texture, flora, cohort, etc.). These values are nominally based off Rep 0 of the LotConfigPropertyLotObject property, with a few extra values added in for the purposes of tracking in this database.
    /// </summary>
    [Table("TGICategories")]
    public class TGICategory(int type, string name) {
        [PrimaryKey]
        [Column("Category")]
        public int Category { get; set; } = type;

        [Column("Name")]
        public string Name { get; set; } = name;

        public override string ToString() {
            return $"{Category}: {Name}";
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
                _db.CreateTable<CatalogItem>();
                _db.CreateTable<AssetItem>();
                _db.CreateTable<TGICategory>();
                _db.Insert(new TGICategory(-1, "Unknown"));
                _db.Insert(new TGICategory(0, "Building"));
                _db.Insert(new TGICategory(1, "Prop"));
                _db.Insert(new TGICategory(2, "Texture"));
                _db.Insert(new TGICategory(4, "Flora"));
                _db.Insert(new TGICategory(10, "Cohort"));
                _db.Insert(new TGICategory(11, "LTEXT"));
                _db.Insert(new TGICategory(12, "Lua"));
                _db.Insert(new TGICategory(13, "UI"));
            }
        }


        //public void BuildPackageTable(List<JsonPackage> packages) {
        //    List<PackageItem> items = [];
        //    int exchangeId;
        //    foreach (var pkg in packages) {
        //        exchangeId = GetExchangeId(pkg.Info.Websites[0]);
        //        items.Add(new PackageItem(exchangeId, 0, pkg.Group, pkg.Name, pkg.Version, pkg.Info.Websites, pkg.Info.Author));
        //    }

        //    if (items.Count == 0) {
        //        return;
        //    }
        //    foreach (PackageItem item in items) {
        //        _db.Insert(item);
        //    }
        //}



        /// <summary>
        /// Build the TGI table from all extracted files, skipping any assets that already exist.
        /// </summary>
        /// <param name="filesPath"></param>
        public List<DBPFError> BuildTGITable(string filesPath) {
            List<DBPFError> errors = [];
            foreach (string folder in Directory.EnumerateDirectories(filesPath, "*", SearchOption.AllDirectories)) {
                var files = Directory.EnumerateFiles(folder);
                if (!files.Any(f => f.IsDBPF())) { continue; }
                    
                
                int exchangeId = GetExchangeId(folder);
                int assetId = GetAssetId(folder);

                if (!AssetExists(exchangeId, assetId)) {
                    Console.WriteLine(exchangeId + "-" + assetId);
                    errors = ParseFolder(folder, exchangeId, assetId);
                }
            }
            return errors;
        }

        private static int GetAssetId(string pathOrUrl) {
            string id = string.Empty;
            try {
                if (pathOrUrl.Contains("simtropolis")) {
                    //P:\sc4pac-cache\https\community.simtropolis.com\files\file\45-bigbus-station\%3Fdo%3Ddownload%26r%3D22305\
                    //P:\sc4pac-cache\https\community.simtropolis.com\library\maxis\\sc4\buildings\Maxis_Buildings.zip\63 Building ← No available id
                    id = pathOrUrl.Split("file\\")[1].Split("-")[0];
                } else if (pathOrUrl.Contains("sc4evermore")) {
                    //P:\sc4pac-cache\https\www.sc4evermore.com\index.php\downloads%3Ftask%3Ddownload.send%26id%3D1%3Asc4d-lex-legacy-hkabt-dependencies-pack
                    id = pathOrUrl.Split("id%3D")[1].Split("%3A")[0];
                } else if (pathOrUrl.Contains("toutsimcities")) {
                    //P:\sc4pac-cache\https\www.toutsimcities.com\downloads\start\1780\TSC\Namspopof
                    id = pathOrUrl.Split("start\\")[1].Split("\\")[0];
                } else if (pathOrUrl.Contains("hide-inoki")) {
                    //P:\sc4pac-cache\http\hide-inoki.com\bbs\archives\files\1391.zip\NekoPropSet03
                    //P:\sc4pac-cache\http\hide-inoki.com\works\sc4\has_dependencies.zip ← No available id
                    id = pathOrUrl.Split("files\\")[1].Split(".")[0];
                }
            }
            catch (IndexOutOfRangeException) {
                id = "0";
            }
            
            _ = int.TryParse(id, out int assetId);
            return assetId;
        }

        private static int GetExchangeId(string pathOrUrl) {
            if (pathOrUrl.Contains("simtropolis")) {
                return 1;
            } else if (pathOrUrl.Contains("sc4evermore")) {
                return 2;
            } else if (pathOrUrl.Contains("toutsimcities")) {
                return 3;
            } else if (pathOrUrl.Contains("hide-inoki")) {
                return 4;
            }
            return 0;
        }

        /// <summary>
        /// Parse all DBPF files in a folder and add found TGIs to the database.
        /// </summary>
        /// <param name="folderPath">Folder path to scan</param>
        /// <param name="exchangeId">Id of the exchange this item is found on</param>
        /// <param name="assetId">Id of the asset</param>
        /// <returns>A list of any errors encountered</returns>
        private List<DBPFError> ParseFolder(string folderPath, int exchangeId, int assetId) {
            var errors = new List<DBPFError>();
            var items = new List<CatalogItem>();
            var files = Directory.EnumerateFiles(folderPath, "*", SearchOption.AllDirectories);

            var dbpfFiles = files.FilterDBPFFiles().GetUniqueFilenamesAcrossFolders();
            FileStream fs;
            foreach (string file in dbpfFiles) {
                try {
                    fs = new FileStream(file, FileMode.Open);
                }
                catch (Exception) {

                    errors.Add(new DBPFError(Path.GetFileName(file), DBPFTGI.BLANKTGI, "Opening file failed"));
                    Console.WriteLine("Could not open " + file);
                    continue;
                }
                DBPFFile dbpf = new DBPFFile(fs);

                var targetEntries = dbpf.ListOfEntries.Where(e => e.MatchesAnyEntryType(DBPFTGI.FSH_BASE_OVERLAY, DBPFTGI.EXEMPLAR, DBPFTGI.COHORT, DBPFTGI.LTEXT, DBPFTGI.LUA, DBPFTGI.LUA_GEN, DBPFTGI.UI));

                foreach (DBPFEntry entry in targetEntries) {
                    //Add Base/Overlay textures (look at the least significant 4 bits and only add if it is 0, 5, or A: AND the Instance by 0b1111 (0xF) and examine the modulus result)
                    if (entry.MatchesEntryType(DBPFTGI.FSH_BASE_OVERLAY) && ((entry.TGI.InstanceID & 0xF) % 5) == 0) {
                        items.Add(new CatalogItem(exchangeId, assetId, dbpf.File.Name, entry.TGI.ToString(), 2, null));
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
                                items.Add(new CatalogItem(exchangeId, assetId, dbpf.File.Name, entry.TGI.ToString(), 0, exmpName));
                                break;
                            case DBPFProperty.ExemplarType.Prop:
                                items.Add(new CatalogItem(exchangeId, assetId, dbpf.File.Name, entry.TGI.ToString(), 1, exmpName));
                                break;
                            case DBPFProperty.ExemplarType.FloraFauna:
                                items.Add(new CatalogItem(exchangeId, assetId, dbpf.File.Name, entry.TGI.ToString(), 4, exmpName));
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

                        items.Add(new CatalogItem(exchangeId, assetId, dbpf.File.Name, entry.TGI.ToString(), 10, exmpName));
                    }

                    //Add LTEXTs
                    else if (entry.MatchesEntryType(DBPFTGI.LTEXT)) {
                        items.Add(new CatalogItem(exchangeId, assetId, dbpf.File.Name, entry.TGI.ToString(), 11, null));
                    }

                    //Add LUAs
                    else if (entry.MatchesAnyEntryType(DBPFTGI.LUA, DBPFTGI.LUA_GEN)) {
                        items.Add(new CatalogItem(exchangeId, assetId, dbpf.File.Name, entry.TGI.ToString(), 12, null));
                    }

                    //Add UIs
                    else if (entry.MatchesEntryType(DBPFTGI.UI)) {
                        items.Add(new CatalogItem(exchangeId, assetId, dbpf.File.Name, entry.TGI.ToString(), 13, null));
                    }
                }
            }

            if (items.Count == 0) {
                return errors;
            }

            if (!AssetExists(items[0].ExchangeId, items[0].AssetId)) {
                //At this time, we do not know the group, name or version of this asset as these come from the sc4pac JSON data
                _db.Insert(new AssetItem(items[0].ExchangeId, items[0].AssetId));
            }

            foreach (CatalogItem item in items) {
                _db.Insert(item);
            }
            return errors;
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
        public bool TGIExists(string tgi) {
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
