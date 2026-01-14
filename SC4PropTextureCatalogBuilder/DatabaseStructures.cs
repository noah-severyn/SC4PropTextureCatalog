using SQLite;

namespace SC4PropTextureCatalogBuilder {
    internal partial class DatabaseBuilder {
        /// <summary>
        /// An item in the TGI table, which tracks which TGIs are in which dependency pack. 
        /// </summary>s
        [Table("TGIs")]
        public class TGIItem(int exchId, int assetId, string file, string tgi, int type, string? exmpName) {
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

        /// <summary>
        /// An item in the Asset table. An asset referrs to a download or file, and is akin to a sc4pac asset.
        /// </summary>
        [Table("Packages")]
        public class PackageItem(int exchId, int assetId, string packageId, string? version = null, List<string>? websites = null, string? author = null, int? primaryCat = null, int? secondaryCat = null) {
            [NotNull]
            public int ExchangeId { get; set; } = exchId;

            [NotNull]
            public int AssetId { get; set; } = assetId;

            /// <summary>
            /// sc4pac package identifier in the format <c>group:name</c>.
            /// </summary>
            [NotNull]
            public string? PackageId { get; set; } = packageId;

            public string? Version { get; set; } = version;

            /// <summary>
            /// Semicolon separated list of one or more urls
            /// </summary>
            public string Websites { get; set; } = string.Join(';', websites ?? []);

            public string? Author { get; set; } = author;


            public int TextureCount { get; set; }
            public int PropCount { get; set; }
            public int FloraCount { get; set; }
            public int BuildingCount { get; set; }



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
                return $"Id:{ExchangeId}-{AssetId}, PackageId:{PackageId}, Version:{Version}, Author:{Author}";
            }
        }

        /// <summary>
        /// An item in the Asset table. An asset referrs to a download or file, and is akin to a sc4pac asset.
        /// </summary>
        [Table("Assets")]
        public class AssetItem(int exchId, int assetId) {
            [NotNull]
            public int Exchange { get; set; } = exchId;
            [NotNull]
            public int ExchangeId2 { get; set; } = assetId;
            public string? AssetId { get; set; }
            public string? Version { get; set; }
            public string? LastModified { get; set; }
            public string? Url { get; set; }
        }


        /// <summary>
        /// Dimension table of TGI types (building, prop, texture, flora, cohort, etc.). These values are nominally based off Rep 0 of the LotConfigPropertyLotObject property, with a few extra values added in for the purposes of tracking in this database.
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
        /// Dimension table with information about each exchange.
        /// </summary>
        [Table("Exchanges")]
        public class Exchange(int id, string name, string url) {
            [PrimaryKey]
            [Column("ExchangeId")]
            public int ExchangeId { get; set; } = id;

            [Column("Name")]
            public string Name { get; set; } = name;

            [Column("Url")]
            public string Url { get; set; } = url;

            public override string ToString() {
                return $"{ExchangeId}: {Name}";
            }
        }
    }
}
