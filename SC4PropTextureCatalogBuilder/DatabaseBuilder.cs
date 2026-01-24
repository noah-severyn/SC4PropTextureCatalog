using csDBPF;
using SQLite;

namespace SC4PropTextureCatalogBuilder {
    /// <summary>
    /// Create and operate on the Prop Texture Catalog database.
    /// </summary>
    internal partial class DatabaseBuilder {
        private readonly SQLiteConnection _db;
        private readonly HashSet<string> _sc4files;
        /// <summary>
        /// Assets referenced in package metadata that are not found in the extract location.
        /// </summary>
        public HashSet<string> MissingAssets { get; set; } = [];
        public List<DBPFError> Errors { get; set; } = [];

        /// <summary>
        /// Create a new SQLite database with the necessary tables and dimensional fields.
        /// </summary>
        /// <param name="dbPath">Path to save the database file to, including the file name.</param>
        /// <param name="create">Whether to create fresh db tables or reuse existing ones.</param>
        /// <param name="extractPath">Folder path to extracted sc4pac cache files.</param>
        public DatabaseBuilder(string dbPath, bool create, HashSet<string> sc4Files) {
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
                _db.Insert(new TGICategory(11, "LTEXT"));
                _db.Insert(new TGICategory(12, "Lua"));
                _db.Insert(new TGICategory(13, "UI"));
                _db.CreateTable<ExchangeItem>();
                _db.Insert(new ExchangeItem(1, "Simtropolis", "https:\\\\community.simtropolis.com"));
                _db.Insert(new ExchangeItem(2, "SC4 Evermore", "https:\\\\www.sc4evermore.com"));
                _db.Insert(new ExchangeItem(3, "ToutSimCities", "https:\\\\www.toutsimcities.com"));
                _db.Insert(new ExchangeItem(4, "Hide-Inoki", "http:\\\\hide-inoki.com"));
                _db.Insert(new ExchangeItem(5, "Github", "https:\\\\github.com"));
                _db.CreateTable<PackageItem>();
                _db.CreateTable<PackageFileItem>();
                _db.CreateTable<FileItem>();
                Console.WriteLine("  > database created");
            }

            _sc4files = sc4Files;
        }



        /// <summary>
        /// Fill the <c>TGIs</c> table.
        /// </summary>
        /// <remarks>The <c>Assets</c> and <c>Files</c> tables should be populated before executing this function. Any errors encountered are added to <see cref="Errors"/>.</remarks>
        public void FillTgiTable() {
            List<TGIItem> items = [];
            int idx = 0;
            foreach (string file in _sc4files) {
                Console.WriteLine($"  > writing {idx}/{_sc4files.Count} " + file);
                var tgisOut = ExtractTGIs(file);
                _db.RunInTransaction(() => {
                    _db.InsertAll(tgisOut);
                });
                idx++;
            }
        }
        private List<TGIItem> ExtractTGIs(string file) {
            var items = new List<TGIItem>();
            var fi = GetFile(null, Path.GetFileName(file));
            if (fi is null) {
                Errors.Add(new DBPFError(file, null, "File not found in database"));
                return [];
            }
            var ai = GetAsset(fi.AssetId);

            FileStream fs;
            try {
                fs = new FileStream(file, FileMode.Open);
            }
            catch (Exception) {
                Errors.Add(new DBPFError(Path.GetFileName(file), null, "Opening file failed"));
                Console.WriteLine("  > could not open " + file);
                return [];
            }
            DBPFFile dbpf = new DBPFFile(fs);

            var targetEntries = dbpf.ListOfEntries.Where(e => e.MatchesAnyEntryType(DBPFTGI.FSH_BASE_OVERLAY, DBPFTGI.EXEMPLAR, DBPFTGI.COHORT, DBPFTGI.LTEXT, DBPFTGI.LUA, DBPFTGI.LUA_GEN, DBPFTGI.UI));
            int textureCnt = 0;
            int propCnt = 0;
            int buildingCnt = 0;
            int floraCnt = 0;            

            foreach (DBPFEntry entry in targetEntries) {
                //Add Base/Overlay textures (look at the least significant 4 bits and only add if it is 0, 5, or A: AND the Instance by 0b1111 (0xF) and examine the modulus result)
                if (entry.MatchesEntryType(DBPFTGI.FSH_BASE_OVERLAY) && ((entry.TGI.InstanceID & 0xF) % 5) == 0) {
                    items.Add(new TGIItem(fi.Id, entry.TGI.ToString(), 2, null));
                    textureCnt++;
                }

                //Add Exemplars
                else if (entry.MatchesEntryType(DBPFTGI.EXEMPLAR)) {
                    DBPFEntryEXMP exmp = (DBPFEntryEXMP) entry;
                    try {
                        exmp.Decode();
                    }
                    catch (Exception ex) {
                        Errors.Add(new DBPFError(file, exmp.TGI, ex.Message));
                        break;
                    }
                        
                    if (exmp.ListOfProperties.Count == 0) continue;

                    DBPFProperty.ExemplarType exmpType = exmp.GetExemplarType();
                    if (exmpType == DBPFProperty.ExemplarType.LotConfiguration) {
                        continue;
                    } else if (exmpType == DBPFProperty.ExemplarType.Error) {
                        Errors.Add(new DBPFError(file, exmp.TGI, "missing property: ExemplarType"));
                        if (exmp.HasProperty("Demand Satisfied")) {
                            exmpType = DBPFProperty.ExemplarType.Building;
                        }
                    }

                    DBPFProperty prop = exmp.GetProperty("ExemplarName");
                    string exmpName;
                    if (prop is null) {
                        Errors.Add(new DBPFError(file, exmp.TGI, "missing property: ExemplarName"));
                        exmpName = "";
                    } else {
                        exmpName = exmpName = (string) prop.GetData();
                    }

                    switch (exmpType) {
                        case DBPFProperty.ExemplarType.Building:
                            items.Add(new TGIItem(fi.Id, entry.TGI.ToString(), 0, exmpName));
                            buildingCnt++;
                            break;
                        case DBPFProperty.ExemplarType.Prop:
                            items.Add(new TGIItem(fi.Id, entry.TGI.ToString(), 1, exmpName));
                            propCnt++;
                            break;
                        case DBPFProperty.ExemplarType.FloraFauna:
                            items.Add(new TGIItem(fi.Id, entry.TGI.ToString(), 4, exmpName));
                            floraCnt++;
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

                    items.Add(new TGIItem(fi.Id, entry.TGI.ToString(), 10, exmpName));
                }

                //Add LTEXTs
                else if (entry.MatchesEntryType(DBPFTGI.LTEXT)) {
                    items.Add(new TGIItem(fi.Id, entry.TGI.ToString(), 11, null));
                }

                //Add LUAs
                else if (entry.MatchesAnyEntryType(DBPFTGI.LUA, DBPFTGI.LUA_GEN)) {
                    items.Add(new TGIItem(fi.Id, entry.TGI.ToString(), 12, null));
                }

                //Add UIs
                else if (entry.MatchesEntryType(DBPFTGI.UI)) {
                    items.Add(new TGIItem(fi.Id, entry.TGI.ToString(), 13, null));
                }
            }

            _db.Execute($"UPDATE Files SET TextureCount = {textureCnt}, PropCount = {propCnt}, FloraCount = {floraCnt}, BuildingCount = {buildingCnt} WHERE Id = {fi.Id}");
            return items;
        }



        /// <summary>
        /// Fills the <c>Assets</c> table with data parsed from sc4pac JSON assets. All files inside of this asset are then added to the <c>Files</c> table.
        /// </summary>
        /// <param name="assets">List of sc4pac JSON assets</param>
        public void FillAssetAndFileTable(List<SC4Pac.Asset> assets) {
            List<AssetItem> items = [];
            List<FileItem> fileItems = [];
            int assetKey = 1;
            foreach (var asset in assets) {
                int exchId = FileMgt.GetExchangeId(asset.Url);
                string cleanedUrl = FileMgt.CleanUrl(asset.Url);
                string folder = FileMgt.HttpToCachePath(asset.Url);
                
                var files = _sc4files.Where(f => f.Contains(folder));
                if (!files.Any()) {
                    MissingAssets.Add(asset.AssetId); //TODO - how is this different from what is returned from `SC4Pac.ExtractFilesFromPackages`?
                    continue;
                }
                foreach (var file in files) {
                    fileItems.Add(new FileItem(assetKey, Path.GetFileName(file)));
                }
                _db.RunInTransaction(() => {
                    _db.InsertAll(fileItems);
                });

                items.Add(new AssetItem(exchId, asset.AssetId, asset.Version, asset.LastModified, cleanedUrl));
                assetKey++;
                fileItems.Clear();
            }
            _db.RunInTransaction(() => {
                _db.InsertAll(items);
            });
        }


        public void FillPackageTable(List<SC4Pac.Package> packages) {
            List<PackageItem> items = [];
            foreach (var pkg in packages) {
                List<string> websites = pkg.Info.Websites ?? (pkg.Info.Website is not null ? new List<string> { pkg.Info.Website } : []);
                items.Add(new PackageItem(pkg.Group + ":" + pkg.Name, pkg.Version, pkg.Subfolder, websites, pkg.Info.Author));
            }
            _db.RunInTransaction(() => {
                _db.InsertAll(items);
            });
        }


        /// <summary>
        /// Return whether this asset exists in the <c>Assets</c> table
        /// </summary>
        /// <returns>TRUE if the asset exists; FALSE otherwise</returns>
        public bool AssetExists(int exchangeId, int assetId) {
            int count = _db.ExecuteScalar<int>($"SELECT count(*) FROM Assets WHERE ExchangeId = ? AND AssetId = ?", exchangeId, assetId);
            return count != 0;
        }
        public AssetItem? GetAsset(string? name = null, string? url = null) {
            if (name is null) {
                return _db.Query<AssetItem>("SELECT * FROM Assets WHERE Url = ?", url).FirstOrDefault();
            } else {
                return _db.Query<AssetItem>("SELECT * FROM Assets WHERE Name = ?", name).FirstOrDefault();
            }
        }
        public AssetItem? GetAsset(int id) {
            return _db.Query<AssetItem>($"SELECT * FROM Assets WHERE Id = ?", id).FirstOrDefault();
        }
        public PackageItem? GetPackage(string name) {
            return _db.Query<PackageItem>($"SELECT * FROM Packages WHERE Name = ?", name).FirstOrDefault();
        }
        public PackageItem? GetPackage(int id) {
            return _db.Query<PackageItem>("SELECT * FROM Packages WHERE Id = ?", id).FirstOrDefault();
        }
        /// <summary>
        /// Return whether this asset exists in the <c>Packages</c> table
        /// </summary>
        /// <returns>TRUE if the asset exists; FALSE otherwise</returns>
        public bool PackageExists(int exchangeId, int assetId) {
            int count = _db.ExecuteScalar<int>("SELECT count(*) FROM Packages WHERE ExchangeId = ? AND AssetId = ?", exchangeId, assetId);
            return count != 0;
        }
        /// <summary>
        /// Return whether this package exists in the <c>Packages</c> table
        /// </summary>
        /// <returns>TRUE if the package exists; FALSE otherwise</returns>
        public bool PackageExists(string package) {
            int count = _db.ExecuteScalar<int>("SELECT count(*) FROM Packages WHERE PackageId = ?", package);
            return count != 0;
        }
        /// <summary>
        /// Return whether this package exists in the <c>CatalogItems</c> table
        /// </summary>
        /// <returns>TRUE if the TGI exists; FALSE otherwise</returns>
        public bool TGIExists(string tgi) {
            int count = _db.ExecuteScalar<int>("SELECT count(*) FROM CatalogItems WHERE TGI = ?", tgi);
            return count != 0;
        }

        public FileItem? GetFile(int? assetId, string name) {
            if (assetId is null) {
                return _db.Query<FileItem>("SELECT * FROM Files WHERE Name = ?", Path.GetFileName(name)).FirstOrDefault();
            } else {
                return _db.Query<FileItem>("SELECT * FROM Files WHERE Name = ? AND AssetId = ?", Path.GetFileName(name), assetId).FirstOrDefault();
            }
        }
        public FileItem? GetFile(int id) {
            return _db.Query<FileItem>("SELECT * FROM Files WHERE Id = ?", id).FirstOrDefault();
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
