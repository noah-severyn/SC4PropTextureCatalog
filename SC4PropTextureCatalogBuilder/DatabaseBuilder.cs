using csDBPF;
using SQLite;

namespace SC4PropTextureCatalogBuilder {
    /// <summary>
    /// Create and operate on the Prop Texture Catalog database.
    /// </summary>
    internal partial class DatabaseBuilder {
        private readonly SQLiteConnection _db;
        private readonly HashSet<string> _sc4files;
        private readonly string _extractPath;
        /// <summary>
        /// Assets referenced in package metadata that are not found in the extract location.
        /// </summary>
        public HashSet<string> MissingAssets { get; set; }

        /// <summary>
        /// Create a new SQLite database with the necessary tables and dimensional fields.
        /// </summary>
        /// <param name="dbPath">Path to save the database file to, including the file name.</param>
        /// <param name="create">Whether to create fresh db tables or reuse existing ones.</param>
        /// <param name="extractPath">Folder path to extracted sc4pac cache files.</param>
        public DatabaseBuilder(string dbPath, bool create, string extractPath) {
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
                _db.CreateTable<FileItem>();
                Console.WriteLine("  > database created");
            }

            //Scan all files once, then continue to reference this same item throughout this class
            Console.WriteLine("  > collecting SC4 files in extract location ...");
            _sc4files = Directory.GetFiles(extractPath, "*", SearchOption.AllDirectories)
                .AsParallel()
                .Where(p => p.IsDBPF())
                .ToHashSet();
            _extractPath = extractPath;
            MissingAssets = new HashSet<string>();
        }



        /// <summary>
        /// Fill the <c>TGIs</c> table.
        /// </summary>
        /// <returns>A list of errors encountered while parsing the DBPF files</returns>
        /// <remarks>The <c>Assets</c> and <c>Files</c> tables should be populated before executing this function.</remarks>
        public List<DBPFError> FillTgiTable() {
            List<TGIItem> items = [];
            List<DBPFError> errors = [];
            foreach (string file in _sc4files) {
                var (tgisOut, errorsOut) = ExtractTGIs(file);
                errors.AddRange(errorsOut);
                _db.RunInTransaction(() => {
                    _db.InsertAll(tgisOut);
                });
            }
            return errors;
        }
        private static (List<TGIItem>, List<DBPFError>) ExtractTGIs(string file) {
            var errors = new List<DBPFError>();
            var items = new List<TGIItem>();

            //int exchangeId = FileMgt.GetExchangeId(file);
            //int assetId = FileMgt.GetAssetId(file);

            string query = $"SELECT * FROM Assets"
            var asset = _db.Table<AssetItem>();
            Console.WriteLine("  > writing " + exchangeId + "-" + assetId + " " + file);

            FileStream fs;
            try {
                fs = new FileStream(file, FileMode.Open);
            }
            catch (Exception) {

                errors.Add(new DBPFError(Path.GetFileName(file), DBPFTGI.BLANKTGI, "Opening file failed"));
                Console.WriteLine("  > could not open " + file);
                return (items, errors);
            }
            DBPFFile dbpf = new DBPFFile(fs);

            var targetEntries = dbpf.ListOfEntries.Where(e => e.MatchesAnyEntryType(DBPFTGI.FSH_BASE_OVERLAY, DBPFTGI.EXEMPLAR, DBPFTGI.COHORT, DBPFTGI.LTEXT, DBPFTGI.LUA, DBPFTGI.LUA_GEN, DBPFTGI.UI));
            var results = _db.Table<FileItem>().Where(i => i.Name == Path.GetFileName(file)).ToList();
            

            foreach (DBPFEntry entry in targetEntries) {
                //Add Base/Overlay textures (look at the least significant 4 bits and only add if it is 0, 5, or A: AND the Instance by 0b1111 (0xF) and examine the modulus result)
                if (entry.MatchesEntryType(DBPFTGI.FSH_BASE_OVERLAY) && ((entry.TGI.InstanceID & 0xF) % 5) == 0) {
                    items.Add(new TGIItem(-100, entry.TGI.ToString(), 2, null));
                }

                //Add Exemplars
                else if (entry.MatchesEntryType(DBPFTGI.EXEMPLAR)) {
                    DBPFEntryEXMP exmp = (DBPFEntryEXMP) entry;
                    try {
                        exmp.Decode();
                    }
                    catch (Exception ex) {
                        errors.Add(new DBPFError(file, exmp.TGI, ex.Message));
                        break;
                    }
                        
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
                            items.Add(new TGIItem(-100, entry.TGI.ToString(), 0, exmpName));
                            break;
                        case DBPFProperty.ExemplarType.Prop:
                            items.Add(new TGIItem(-100, entry.TGI.ToString(), 1, exmpName));
                            break;
                        case DBPFProperty.ExemplarType.FloraFauna:
                            items.Add(new TGIItem(-100, entry.TGI.ToString(), 4, exmpName));
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

                    items.Add(new TGIItem(-100, entry.TGI.ToString(), 10, exmpName));
                }

                //Add LTEXTs
                else if (entry.MatchesEntryType(DBPFTGI.LTEXT)) {
                    items.Add(new TGIItem(-100, entry.TGI.ToString(), 11, null));
                }

                //Add LUAs
                else if (entry.MatchesAnyEntryType(DBPFTGI.LUA, DBPFTGI.LUA_GEN)) {
                    items.Add(new TGIItem(-100, entry.TGI.ToString(), 12, null));
                }

                //Add UIs
                else if (entry.MatchesEntryType(DBPFTGI.UI)) {
                    items.Add(new TGIItem(-100, entry.TGI.ToString(), 13, null));
                }
            }
            return (items, errors);
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

                //Fetch all files within this asset
                var folder = Path.Combine(_extractPath, FileMgt.HttpToCachePath(asset.Url));
                
                if (!Directory.Exists(folder)) {
                    MissingAssets.Add(folder);
                    continue;
                }
                var files = _sc4files.Where(f => f.StartsWith(folder));
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
            int count = _db.ExecuteScalar<int>($"SELECT count(*) FROM Assets WHERE ExchangeId = '{exchangeId}' AND AssetId = '{assetId}'");
            return count != 0;
        }
        /// <summary>
        /// Return whether this asset exists in the <c>Packages</c> table
        /// </summary>
        /// <returns>TRUE if the asset exists; FALSE otherwise</returns>
        public bool PackageExists(int exchangeId, int assetId) {
            int count = _db.ExecuteScalar<int>($"SELECT count(*) FROM Packages WHERE ExchangeId = '{exchangeId}' AND AssetId = '{assetId}'");
            return count != 0;
        }
        /// <summary>
        /// Return whether this package exists in the <c>Packages</c> table
        /// </summary>
        /// <returns>TRUE if the package exists; FALSE otherwise</returns>
        public bool PackageExists(string package) {
            int count = _db.ExecuteScalar<int>($"SELECT count(*) FROM Packages WHERE PackageId = '{package}'");
            return count != 0;
        }
        /// <summary>
        /// Return whether this package exists in the <c>CatalogItems</c> table
        /// </summary>
        /// <returns>TRUE if the TGI exists; FALSE otherwise</returns>
        public bool TGIExists(string tgi) {
            int count = _db.ExecuteScalar<int>($"SELECT count(*) FROM CatalogItems WHERE TGI = '{tgi}'");
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
