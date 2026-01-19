using SQLite;

namespace SC4PropTextureCatalogBuilder {
    internal partial class DatabaseBuilder {
        /// <summary>
        /// An item in the TGI table, which tracks which TGIs are in which dependency pack. 
        /// </summary>s
        [Table("TGIs")]
        public class TGIItem(int fileId, string tgi, int category, string? exmpName) {
            /// <summary>
            /// Reference to the <see cref="FileItem.Id"/> that contains this item.
            /// </summary>
            public int FileId { get; set; } = fileId;

            public string TGI { get; set; } = tgi;

            /// <summary>
            /// One of the <see cref="TGICategory"/> enumerations.
            /// </summary>
            public int Category { get; set; } = category;

            /// <summary>
            /// Item name, if applicable. Typically an exemplar name.
            /// </summary>
            public string? Name { get; set; } = exmpName;

            public override string ToString() {
                return $"{FileId}: {TGI}, {Category}, {Name}";
            }
        }

        /// <summary>
        /// An item in the Asset table. An asset referrs to a download or file, and is akin to a sc4pac asset.
        /// </summary>
        [Table("Packages")]
        public class PackageItem(string packageName, string version, string subfolder, List<string>? websites = null, string? author = null, List<string>? files = null, int? primaryCat = null, int? secondaryCat = null) {
            [PrimaryKey]
            [AutoIncrement]
            public int Id { get; set; }

            /// <summary>
            /// sc4pac package identifier in the format <c>group:name</c>.
            /// </summary>
            [NotNull]
            public string Name { get; set; } = packageName;

            public string Version { get; set; } = version;

            public string Subfolder { get; set; } = subfolder;

            ///// <summary>
            ///// List of files this package contains
            ///// </summary>
            //public List<string> Files { get; set; } = files ?? [];

            /// <summary>
            /// Semicolon separated list of one or more urls
            /// </summary>
            public string Websites { get; set; } = string.Join(';', websites ?? []);

            public string? Author { get; set; } = author;


            ///// <summary>
            ///// Describes the contents of this asset as one or more of: Textures, Buildings, Flora, Fauna, People, Vehicles, Scenery, Helpers, Effects, Other, etc.
            ///// </summary>
            //[Column("PrimaryCat")]
            //public int? PrimaryCats { get; set; } = primaryCat;

            ///// <summary>
            ///// Further categorizes each of the primary categories into subcategories
            ///// </summary>
            //[Column("SecondaryCat")]
            //public int? SecondaryCats { get; set; } = secondaryCat;

            public override string ToString() {
                return $"Id:{Id}, PkgId:{Name}, Version:{Version}, Subfolder:{Subfolder}, Author:{Author}";
            }
        }

        /// <summary>
        /// Represents a file included in an sc4pac asset, used to link Packages to TGIs
        /// </summary>
        [Table("Files")]
        public class FileItem(int assetId, string fileName) {
            /// <summary>
            /// File primary key. Autoincremented.
            /// </summary>
            [PrimaryKey]
            [AutoIncrement]
            public int Id { get; set; }
            /// <summary>
            /// Reference to the <see cref="AssetItem.AssetId"/> that contains this item.
            /// </summary>
            [NotNull]
            public int AssetId { get; set; } = assetId;
            /// <summary>
            /// Filename of this item.
            /// </summary>
            [NotNull]
            public string Name { get; set; } = fileName;

            public int TextureCount { get; set; }
            public int PropCount { get; set; }
            public int FloraCount { get; set; }
            public int BuildingCount { get; set; }
        }

        /// <summary>
        /// An item in the Asset table. An asset referrs to a download or file, and is akin to a sc4pac asset.
        /// </summary>
        [Table("Assets")]
        public class AssetItem(int exchangeId, string name, string version, string lastModified, string url) {
            /// <summary>
            /// Asset primary key. Autoincremented.
            /// </summary>
            [PrimaryKey]
            [AutoIncrement]
            public int Id { get; set; }

            /// <summary>
            /// Reference to the <see cref="ExchangeItem.Id"/> where this item is uploaded to.
            /// </summary>
            [NotNull]
            public int ExchangeId { get; set; } = exchangeId;

            [NotNull]
            public string Name { get; set; } = name;

            public string Version { get; set; } = version;

            public string LastModified { get; set; } = lastModified;

            public string Url { get; set; } = url;
        }


        /// <summary>
        /// Dimension table of TGI types (building, prop, texture, flora, cohort, etc.). These values are nominally based off Rep 0 of the LotConfigPropertyLotObject property, with a few extra values added in for the purposes of tracking in this database.
        /// </summary>
        [Table("TGICategories")]
        public class TGICategory(int id, string name) {
            [PrimaryKey]
            public int Id { get; set; } = id;

            [Column("Name")]
            public string Category { get; set; } = name;

            public override string ToString() {
                return $"{Id}: {Category}";
            }
        }

        /// <summary>
        /// Dimension table with information about each exchange.
        /// </summary>
        [Table("Exchanges")]
        public class ExchangeItem(int id, string name, string url) {
            [PrimaryKey]
            public int Id { get; set; } = id;

            public string Name { get; set; } = name;

            public string Url { get; set; } = url;

            public override string ToString() {
                return $"{Id}: {Name}";
            }
        }
    }
}
