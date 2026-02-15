using csDBPF;
using SQLite;

namespace SC4PropTextureCatalogBuilder {
    /// <summary>
    /// Create and operate on the Prop Texture Catalog database.
    /// </summary>
    internal partial class DatabaseBuilder {
        private readonly SQLiteConnection _db;
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
        }


        /// <summary>
        /// Fills the <c>Assets</c> table with data parsed from sc4pac JSON assets. All files inside of this asset are then added to the <c>Files</c> table.
        /// </summary>
        /// <param name="assets">List of sc4pac JSON assets</param>
        public void FillAssetAndFileTable(HashSet<string> sc4files, Dictionary<string, SC4Pac.Asset> assets) {
            Dictionary<string, AssetItem> items = [];
            Dictionary<string, FileItem> fileItems = [];
            int assetKey = 1; //SQLite auto indexes are 1-based
            foreach (var asset in assets.Values) {
                int exchId = FileMgt.GetExchangeId(asset.Url);
                string cleanedUrl = FileMgt.CleanUrl(asset.Url);
                string folder = FileMgt.HttpToCachePath(asset.Url) + "\\";

                var files = sc4files.Where(f => f.Contains(folder));
                if (!files.Any()) {
                    MissingAssets.Add(asset.AssetId); //TODO - how is this different from what is returned from `SC4Pac.ExtractFilesFromPackages`?
                    continue;
                }
                foreach (var file in files) {
                    var fileName = Path.GetFileName(file);
                    var itemKey = assetKey + "|" + fileName;
                    fileItems.TryAdd(itemKey, new FileItem(assetKey, fileName));
                }

                items.TryAdd(asset.AssetId, new AssetItem(exchId, asset.AssetId, asset.Version, asset.LastModified, cleanedUrl));
                assetKey++;
            }
            _db.RunInTransaction(() => {
                _db.InsertAll(items.Values);
            });
            _db.RunInTransaction(() => {
                _db.InsertAll(fileItems.Values);
            });
        }


        /// <summary>
        /// Fill the <c>TGIs</c> table.
        /// </summary>
        /// <remarks>The <c>Assets</c> and <c>Files</c> tables should be populated before executing this function. Any errors encountered are added to <see cref="Errors"/>.</remarks>
        public void FillTgiTable(Dictionary<string, SC4Pac.Package> packages) {
            //We do NOT simply want to list out all the files in the cache and back-calculate their asset and file ids, because the cache may have multiple obsolete revisions, resulting in a double counting of files and TGIs, in addition to potentially including files removed from the current version.
            //The TGIs table needs a FileId which we need to find. For each PkgFileItem in a package's LocalFiles, get the AssetId from the AssetName, and use the combination of the AssetId and FileName to get the FileId
            var allAssets = _db.Query<AssetItem>("SELECT * FROM Assets").ToDictionary(a => a.Name, a => a.Id);
            var allFiles = _db.Query<FileItem>("SELECT * FROM Files").ToDictionary(f => f.AssetId + "|" + f.Name, f => f.Id);


            List<TGIItem> items = [];
            int idx = 0;
            foreach (var pkg in packages) {
                Console.WriteLine($"  > writing {idx}/{packages.Count} packages : " + pkg.Key);
                foreach (var file in pkg.Value.LocalFiles) {
                    var fileName = Path.GetFileName(file.FilePath);
                    allAssets.TryGetValue(file.AssetName, out var assetId);
                    var fileFound = allFiles.TryGetValue(assetId + "|" + fileName, out var fileId);
                    if (!fileFound) {
                        Errors.Add(new DBPFError(file.FilePath, null, $"Key {assetId + "|" + fileName} not found in the 'Files' table"));
                        continue;
                    }
                    items.AddRange(ExtractTGIs(file.FilePath, fileId));
                }
                idx++;
            }

            _db.RunInTransaction(() => {
                _db.InsertAll(items);
            });
        }
        private List<TGIItem> ExtractTGIs(string file, int fileId) {
            var items = new List<TGIItem>();

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
                    items.Add(new TGIItem(fileId, entry.TGI.ToString(), 2, null));
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
                        continue;
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
                            items.Add(new TGIItem(fileId, entry.TGI.ToString(), 0, exmpName));
                            buildingCnt++;
                            break;
                        case DBPFProperty.ExemplarType.Prop:
                            items.Add(new TGIItem(fileId, entry.TGI.ToString(), 1, exmpName));
                            propCnt++;
                            break;
                        case DBPFProperty.ExemplarType.FloraFauna:
                            items.Add(new TGIItem(fileId, entry.TGI.ToString(), 4, exmpName));
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

                    items.Add(new TGIItem(fileId, entry.TGI.ToString(), 10, exmpName));
                }

                //Add LTEXTs
                else if (entry.MatchesEntryType(DBPFTGI.LTEXT)) {
                    items.Add(new TGIItem(fileId, entry.TGI.ToString(), 11, null));
                }

                //Add LUAs
                else if (entry.MatchesAnyEntryType(DBPFTGI.LUA, DBPFTGI.LUA_GEN)) {
                    items.Add(new TGIItem(fileId, entry.TGI.ToString(), 12, null));
                }

                //Add UIs
                else if (entry.MatchesEntryType(DBPFTGI.UI)) {
                    items.Add(new TGIItem(fileId, entry.TGI.ToString(), 13, null));
                }
            }

            _db.Execute($"UPDATE Files SET TextureCount = ?, PropCount = ?, FloraCount = ?, BuildingCount = ? WHERE Id = ?", textureCnt, propCnt, floraCnt, buildingCnt, fileId);
            return items;
        }


        /// <summary>
        /// Fills the <c>Package</c> table.
        /// </summary>
        /// <remarks>The <c>Assets</c> and <c>Files</c> tables should be populated before executing this function.</remarks>
        public void FillPackageTable(Dictionary<string, SC4Pac.Package> packages) {
            List<PackageItem> items = [];
            foreach (var pkg in packages.Values) {
                List<string> websites = pkg.Info.Websites ?? (pkg.Info.Website is not null ? new List<string> { pkg.Info.Website } : []);
                items.Add(new PackageItem(pkg.Group + ":" + pkg.Name, pkg.Version, pkg.Subfolder, websites, pkg.Info.Author));
            }
            _db.RunInTransaction(() => {
                _db.InsertAll(items);
            });
        }


        /// <summary>
        /// Fills the <c>PackageFile</c> table.
        /// </summary>
        /// <remarks>The <c>Assets</c>, <c>Files</c>, and <c>Packages</c> tables should be populated before executing this function. Any errors encountered are added to <see cref="Errors"/>.</remarks>
        public void FillPackageFileTable(Dictionary<string, SC4Pac.Package> packages) {
            List<PackageFileItem> items = [];
            //Must use the items from the db instead of the package or asset dictionaries because we need to get the autoincremented table ids
            var assetsByName = _db.Query<AssetItem>($"SELECT * FROM Assets").ToDictionary(a => a.Name, a => a.Id);
            var fileItems = _db.Query<FileItem>($"SELECT * FROM Files").ToDictionary(f => f.AssetId + "|" + f.Name, f => f.Id);
            var pkgsByName = _db.Query<PackageItem>($"SELECT * FROM Packages").ToDictionary(p => p.Name, p => p.Id);

            int? pkgId;
            int? assetId;
            int? fileId;
            foreach (var p in packages) {
                var pkg = p.Value;
                if (pkg.LocalFiles.Count == 0) {
                    Errors.Add(new DBPFError(string.Empty, null, "Assets for package " + p.Key + " were not found"));
                    continue;
                }
                foreach (var pkgFile in pkg.LocalFiles) {
                    assetId = assetsByName.GetValueOrDefault(pkgFile.AssetName);
                    fileId = fileItems.GetValueOrDefault(assetId + "|" + Path.GetFileName(pkgFile.FilePath));
                    pkgId = pkgsByName.GetValueOrDefault(pkgFile.PackageName);

                    if (assetId is null) {
                        Errors.Add(new DBPFError(pkgFile.FilePath, null, "Could not find in db asset " + pkgFile.AssetName));
                    } else if (fileId is null) {
                        Errors.Add(new DBPFError(pkgFile.FilePath, null, "Could not find in db asset " + pkgFile.AssetName + " with file " + pkgFile.FilePath));
                    } else if (pkgId is null) {
                        Errors.Add(new DBPFError(pkgFile.FilePath, null, "Could not find in db package " + pkgFile.PackageName));
                    } else {
                        items.Add(new PackageFileItem((int) pkgId, (int) fileId));
                    }
                }
            }
            _db.RunInTransaction(() => {
                _db.InsertAll(items);
            });
        }
    }
}
