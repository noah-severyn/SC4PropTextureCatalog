using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using SQLite;
using System.IO;
using csDBPF;

namespace SC4PropTextureCatalog {
    /// <summary>
    /// An item in the TGITable, which tracks which TGIs are in which dependency pack. 
    /// </summary>s
    [Table("TGITable")]
    public class TGIItem {
        [PrimaryKey, AutoIncrement]
        [Column("TGIID")]
        public int ID { get; set; }

        [Column("PackID")]
        public int PackID { get; set; }

        [Column("TGI")]
        public string TGI { get; set; }

        [Column("TGIType")]
        public int? TGIType { get; set; }

        [Column("ExemplarName")]
        public string ExemplarName { get; set; }

        //[Column("Thumbnail")]
        //public byte[] Thumbnail { get; set; }

        public override string ToString() {
            return $"{ID}: {PackID}, {TGI}, {TGIType}, {ExemplarName}";
        }
    }

    /// <summary>
    /// An item in the PackTable, which tracks information about each dependency pack (aka download or file or path).
    /// </summary>
    [Table("PackTable")]
    public class PackItem {
        [PrimaryKey, AutoIncrement]
        [Column("PackID")]
        public int PackID { get; set; }

        [Column("PackName")]
        public string PackName { get; set; }

        [Column("PackVersion")]
        public string PackVersion { get; set; }

        [Column("Hyperlink")]
        public string Hyperlink { get; set; }

        [Column("Author")]
        public string Author { get; set; }

        [Column("Type")]
        public string Type { get; set; }

        [Column("PrimaryCat")]
        public string PrimaryCat { get; set; }

        [Column("SecondaryCat")]
        public string SecondaryCat { get; set; }

        public override string ToString() {
            return $"{PackID}: {PackName}, (v{PackVersion}) by {Author}. Type:{Type}, Primary:{PrimaryCat}, Secondary:{SecondaryCat}";
        }
    }

    /// <summary>
    /// Dimension 
    /// </summary>
    [Table("TGITypes")]
    public class TGICategory {
        [PrimaryKey]
        [Column("TGIType")]
        public int TGIType { get; set; }

        [Column("TGIName")]
        public string TGIName { get; set; }

        public override string ToString() {
            return $"{TGIType}: {TGIName}";
        }
    }



    /// <summary>
    /// Create and operate on the Prop Texture Catalog database.
    /// </summary>
    public class DatabaseBuilder {
        private readonly SQLiteConnection _db;
        private readonly List<FileInfo> _files;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbpath">File path to save the database as</param>
        /// <param name="dbpfFiles">List of file paths to iterate through</param>
        public DatabaseBuilder(string dbpath, List<FileInfo> dbpfFiles) {
            _files = dbpfFiles;
            dbpath = Path.Combine(dbpath, "Catalog.db");
            if (File.Exists(dbpath)) {
                File.Delete(dbpath);
            }

            //Open a new StreamWriter to create a file then immediately close - writing is handled by the SQLite functions
            File.CreateText(dbpath).Dispose();

            //Initialize database tables and schema
            _db = new SQLiteConnection(dbpath);
            _db.CreateTable<TGIItem>();
            _db.CreateTable<PackItem>();
            _db.CreateTable<TGICategory>();
            _db.Insert(new TGICategory { TGIType = 0, TGIName = "Building" });
            _db.Insert(new TGICategory { TGIType = 1, TGIName = "Prop" });
            _db.Insert(new TGICategory { TGIType = 2, TGIName = "Texture" });
            _db.Insert(new TGICategory { TGIType = 4, TGIName = "Flora" });
            _db.Insert(new TGICategory { TGIType = 10, TGIName = "Cohort" });
        }



        /// <summary>
        /// Add a TGI item with associated information to the database.
        /// </summary>
        /// <param name="packName">Name of dependency pack the TGI is contained in</param>
        /// <param name="tgi">String representation of the TGI in the format 0x00000000, 0x00000000, 0x00000000 </param>
        /// <param name="tgitype">Type of TGI: Building=0, Prop=1, Texture=2, Flora=4, Cohort=10</param>
        /// <param name="exmpName">Name of the exemplar; null if the TGI is a texture</param>
        /// <remarks>The path in TGITable is stored as a reference to the full path in PathTable. This dramatically reduces file size as the long path string only needs to be stored once.</remarks>
        private void AddTGI(string packName, string tgi, int? tgitype, string exmpName) {
            //check if we already have a matching PackID, create new PackItem if not, otherwise use found PackID
            int packID = GetPackID(packName);
            if (packID <= 0) {

                PackItem newPack = new PackItem {
                    PackName = packName
                };
                _db.Insert(newPack);
                packID = newPack.PackID;
            }

            //once we know our PackID then add the new TGI with that PackID
            TGIItem newTGI = new TGIItem {
                PackID = packID,
                TGI = tgi,
                TGIType = tgitype,
                ExemplarName = exmpName
                //Thumbnail = GetThumbnail(tgi)
            };
            _db.Insert(newTGI);
        }



        /// <summary>
        /// Lookup and return the PathID for the provided PathName.
        /// </summary>
        /// <param name="packName">Pack name (file name) to lookup</param>
        /// <returns>PathID if name is found; -1 if item is not found or if multiple matches were found</returns>
        /// <remarks>PathItems table should always have unique items, so we might have an issue if the return is more than one item.</remarks>
        private int GetPackID(string packName) {
            string searchName = packName.Replace("'", "''");
            List<PackItem> result = _db.Query<PackItem>($"SELECT * FROM PackTable WHERE PackName = '{searchName}'");

            if (result.Count == 1) {
                return result[0].PackID;
            } else {
                return -1;
            }
        }



        private bool DoesTGIExist(string tgi) {
            List<TGIItem> result = _db.Query<TGIItem>($"SELECT * FROM TGITable WHERE TGI = '{tgi}'");
            return result.Count == 0;
        }



        /// <summary>
        /// Fetches the PNG thumbnail image for this TGI.
        /// </summary>
        /// <param name="tgi">TGI to use</param>
        /// <returns>A PNG image represented as bytes</returns>
        private byte[] GetThumbnail(string tgi) {
            string fname = tgi.Replace("0x", "").Replace(", ", "-") + ".png";
            try {
                return File.ReadAllBytes("C:\\source\\repos\\SC4PropTextureCatalog\\wwwroot\\img\\thumbnails\\" + fname);
            } catch {
                return new byte[0];
            }
            
        }



        /// <summary>
        /// Creates a SQLite DB of props and lot textures from the list of files.
        /// </summary>
        public void BuildDB() {
            StringBuilder log = new StringBuilder("FileName,Type,Group,Instance,TGIType,TGISubtype,Message");

            for (int idx = 0; idx < _files.Count; idx++) {
                FileInfo file = _files[idx];

                //Debug.WriteLine(file.Name);
                DBPFFile dbpf = new DBPFFile(file);
                log.AppendLine(dbpf.GetIssueLog());

                _db.Insert(new PackItem { 
                    PackID = idx, 
                    PackName = file.Name
                });
                
                foreach (DBPFEntry entry in dbpf.GetEntries()) {
                    string filename = Path.GetFileNameWithoutExtension(file.Name);


                    //Add Base/Overlay textures (look at the least significant 4 bits and only add if it is 0, 5, or A: AND the Instance by 0b1111 (0xF) and examine the modulus result)
                    if (entry.MatchesEntryType(DBPFTGI.FSH_BASE_OVERLAY) && ((entry.TGI.InstanceID & 0xF) % 5) == 0) {
                        AddTGI(filename, entry.TGI.ToString().Substring(0, 34), 2, null);
                    }


                    //Add Exemplars
                    else if (entry.MatchesEntryType(DBPFTGI.EXEMPLAR)) {
                        DBPFEntryEXMP exmp = (DBPFEntryEXMP) entry;
                        entry.Decode();
                        if (exmp.ListOfProperties.Count == 0) continue;

                        DBPFProperty.ExemplarType exmpType = exmp.GetExemplarType();
                        if (exmpType == DBPFProperty.ExemplarType.UnknownType) {
                            log.AppendLine($"{filename}, {exmp.TGI.ToString().Replace(" ", "")},missing property: ExemplarType");
                            exmpType = 0;
                        }

                        DBPFProperty prop = exmp.GetProperty("ExemplarName");
                        string exmpName;
                        if (prop is null) {
                            log.AppendLine($"{filename}, {exmp.TGI.ToString().Replace(" ", "")},missing property: ExemplarName");
                            exmpName = "";
                        } else {
                            exmpName = exmpName = (string) prop.GetData();
                        }

                        switch (exmpType) {
                            case DBPFProperty.ExemplarType.UnknownType:
                                AddTGI(filename, exmp.TGI.ToString().Substring(0, 34), null, exmpName);
                                break;
                            case DBPFProperty.ExemplarType.Building:
                                AddTGI(filename, exmp.TGI.ToString().Substring(0, 34), 0, exmpName);
                                break;
                            case DBPFProperty.ExemplarType.Prop:
                                AddTGI(filename, exmp.TGI.ToString().Substring(0, 34), 1, exmpName);
                                break;
                            case DBPFProperty.ExemplarType.FloraFauna:
                                AddTGI(filename, exmp.TGI.ToString().Substring(0, 34), 4, exmpName);
                                break;
                            default:
                                break;
                        }
                    }


                    //Add Cohorts (note the building/prop family of the cohort is always 0x10000000 less than the Cohort's Index)
                    else if (entry.MatchesEntryType(DBPFTGI.COHORT)) {
                        DBPFEntryEXMP exmp = (DBPFEntryEXMP) entry;
                        TGI family = new TGI(entry.TGI.TypeID, entry.TGI.GroupID, entry.TGI.InstanceID - 0x10000000);
                        entry.Decode();
                        if (exmp.ListOfProperties.Count == 0) continue;

                        string exmpName = (string) exmp.GetProperty("ExemplarName").GetData();
                        AddTGI(filename, family.ToString().Substring(0, 34), 10, exmpName);
                    }
                }
            }

            File.WriteAllText("C:\\source\\repos\\SC4PropTextureCatalog\\Data\\log.csv", log.ToString());
        }
    }
}
