using SQLite;

namespace SC4PropTextureCatalogAPI.Models {
    [Table("TGIs")]
    public class TGI {

        [NotNull]
        public int ExchangeId { get; set; }
        [NotNull]
        public int AssetId { get; set; }
        [NotNull]
        public string File { get; set; } = string.Empty;
        [NotNull]
        public string TGIString { get; set; } = string.Empty;
        public int TGIType { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    //[Table("CatalogRecord")]
    //public class CatalogRecord {

    //    [NotNull]
    //    public int ExchangeId { get; set; }
    //    [NotNull]
    //    public int AssetId { get; set; }
    //    [NotNull]
    //    public string File { get; set; } = string.Empty;
    //    [NotNull]
    //    public string TGIString { get; set; } = string.Empty;
    //    public int TGIType { get; set; }
    //    public string Name { get; set; } = string.Empty;
    //}
}
