using csDBPF;
using SQLite;

namespace SC4PropTextureCatalogBuilder {
    /// <summary>
    /// Create and operate on the Prop Texture Catalog database.
    /// </summary>
    internal partial class DatabaseBuilder {
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
                _db.Insert(new TGICategory(11, "LTEXT"));
                _db.Insert(new TGICategory(12, "Lua"));
                _db.Insert(new TGICategory(13, "UI"));
                _db.CreateTable<Exchange>();
                _db.Insert(new Exchange(1, "Simtropolis", "https:\\\\community.simtropolis.com"));
                _db.Insert(new Exchange(2, "SC4 Evermore", "https:\\\\www.sc4evermore.com"));
                _db.Insert(new Exchange(3, "ToutSimCities", "https:\\\\www.toutsimcities.com"));
                _db.Insert(new Exchange(4, "Hide-Inoki", "http:\\\\hide-inoki.com"));
                _db.Insert(new Exchange(5, "Github", "https:\\\\github.com"));
                _db.CreateTable<PackageItem>();
            }
        }





        

        




        /// <summary>
        /// Parse all DBPF files in the extract directory and fill the <c>TGI</c> table with the TGIs in each file. Adds a new item in the <c>Assets</c> table if the TGI is part of an asset that does not yet exist in the <c>Assets</c> table.
        /// </summary>
        /// <param name="filesPath">Folder path containing extracted cache files</param>
        /// <returns>A list of any errors encountered</returns>
        public List<DBPFError> FillTgiTable(string extractPath) {
            List<DBPFError> errors = [];
            foreach (string folder in Directory.EnumerateDirectories(extractPath, "*", SearchOption.AllDirectories)) {
                var files = Directory.EnumerateFiles(folder);
                if (!files.Any(f => f.IsDBPF())) { continue; }
                    
                //if (!AssetExists(exchangeId, assetId)) {
                    errors.AddRange(ExtractTGIs(folder));
                //}
            }
            return errors;
        }

        private List<DBPFError> ExtractTGIs(string folderPath) {
            var errors = new List<DBPFError>();
            var items = new List<TGIItem>();
            var files = Directory.EnumerateFiles(folderPath, "*", SearchOption.AllDirectories);

            int exchangeId = FileMgt.GetExchangeId(folderPath);
            int assetId = FileMgt.GetAssetId(folderPath);
            Console.WriteLine("  > writing " + exchangeId + "-" + assetId + " " + folderPath);

            var dbpfFiles = files.FilterDBPFFiles().GetUniqueFilenamesAcrossFolders();
            FileStream fs;
            foreach (string file in dbpfFiles) {
                try {
                    fs = new FileStream(file, FileMode.Open);
                }
                catch (Exception) {

                    errors.Add(new DBPFError(Path.GetFileName(file), DBPFTGI.BLANKTGI, "Opening file failed"));
                    Console.WriteLine("  > could not open " + file);
                    continue;
                }
                DBPFFile dbpf = new DBPFFile(fs);

                var targetEntries = dbpf.ListOfEntries.Where(e => e.MatchesAnyEntryType(DBPFTGI.FSH_BASE_OVERLAY, DBPFTGI.EXEMPLAR, DBPFTGI.COHORT, DBPFTGI.LTEXT, DBPFTGI.LUA, DBPFTGI.LUA_GEN, DBPFTGI.UI));

                foreach (DBPFEntry entry in targetEntries) {
                    //Add Base/Overlay textures (look at the least significant 4 bits and only add if it is 0, 5, or A: AND the Instance by 0b1111 (0xF) and examine the modulus result)
                    if (entry.MatchesEntryType(DBPFTGI.FSH_BASE_OVERLAY) && ((entry.TGI.InstanceID & 0xF) % 5) == 0) {
                        items.Add(new TGIItem(exchangeId, assetId, dbpf.File.Name, entry.TGI.ToString(), 2, null));
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

                    //Add LTEXTs
                    else if (entry.MatchesEntryType(DBPFTGI.LTEXT)) {
                        items.Add(new TGIItem(exchangeId, assetId, dbpf.File.Name, entry.TGI.ToString(), 11, null));
                    }

                    //Add LUAs
                    else if (entry.MatchesAnyEntryType(DBPFTGI.LUA, DBPFTGI.LUA_GEN)) {
                        items.Add(new TGIItem(exchangeId, assetId, dbpf.File.Name, entry.TGI.ToString(), 12, null));
                    }

                    //Add UIs
                    else if (entry.MatchesEntryType(DBPFTGI.UI)) {
                        items.Add(new TGIItem(exchangeId, assetId, dbpf.File.Name, entry.TGI.ToString(), 13, null));
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

            //foreach (CatalogItem item in items) {
            //    _db.Insert(item);
            //}
            _db.RunInTransaction(() => {
                _db.InsertAll(items);
            });
            return errors;
        }




        /// <summary>
        /// Updates the <c>Assets</c> table with data parsed from sc4pac JSON assets. Adds a new item in the <c>Packages</c>
        /// </summary>
        /// <param name="assets">List of sc4pac JSON assets</param>
        public void FillAssetTable(List<SC4Pac.Asset> assets) {
            foreach (var asset in assets) {
                int exchId = FileMgt.GetExchangeId(asset.Url);
                int assetId = FileMgt.GetAssetId(asset.Url);

                string cleanedUrl;
                 //Strip query params from the Url, except from SC4E where the id is a critical parameter
                if (asset.Url.Contains("sc4evermore")) {
                    cleanedUrl = asset.Url.Replace("?task=download.send&id=", "/download/");
                } else {
                    cleanedUrl = new Uri(asset.Url).GetLeftPart(UriPartial.Path);
                }
                _db.Execute($"UPDATE Assets SET Version = \"{asset.Version}\", LastModified = \"{asset.LastModified}\", Url = \"{cleanedUrl}\" WHERE ExchangeId = {exchId} AND AssetId = {assetId}");

                foreach (var pkgId in asset.RequiredBy) {
                    if (!PackageExists(pkgId)) {
                        _db.Insert(new PackageItem(exchId, assetId, pkgId));
                    }
                }
            }
        }


        public void FillPackageTable(List<SC4Pac.Package> packages) {
            foreach (var pkg in packages) {
                //int exchId = GetExchangeId(asset.Url);
                //int assetId = GetAssetId(asset.Url);
                (var exchId, var assetId) = _db.Query<(int, int)>("SELECT ExchangeId, AssetId FROM Packages WHERE PackageId = ?", pkg.Group + ":" + pkg.Name).FirstOrDefault();
                
                //string cleanedUrl = new Uri(asset.Url).GetLeftPart(UriPartial.Path); //Strip query params from the Url
                _db.Execute($"UPDATE Packages SET Version = \"{pkg.Version}\", Websites = \"{String.Join(";", pkg.Info.Websites)}\", Author = \"{pkg.Info.Author}\" WHERE ExchangeId = {exchId} AND AssetId = {assetId}");
            }
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
