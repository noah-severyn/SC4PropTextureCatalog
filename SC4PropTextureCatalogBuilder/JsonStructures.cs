using System.Text.Json.Serialization;

namespace SC4PropTextureCatalogBuilder {
    internal partial class SC4Pac {
        /// <summary>
        /// Matches the sc4pac JSON schema for an Asset.
        /// </summary>
        public class Asset {
            [JsonPropertyName("$type")]
            public string Type { get; set; } = string.Empty;
            public string AssetId { get; set; } = string.Empty;
            public string Version { get; set; } = string.Empty;
            public string LastModified { get; set; } = string.Empty;
            public string Url { get; set; } = string.Empty;
            public List<string> RequiredBy { get; set; } = [];

            public override string ToString() {
                return $"{AssetId} ({Version})";
            }
        }

        /// <summary>
        /// Matches the sc4pac JSON schema for a Package.
        /// </summary>
        public class Package {
            [JsonPropertyName("$type")]
            public string Type { get; set; } = string.Empty;
            public string Group { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Version { get; set; } = string.Empty;
            public string Subfolder { get; set; } = string.Empty;
            public PackageInfo Info { get; set; } = new PackageInfo();
            public List<PackageAsset>? Assets { get; set; }
            public List<Variant>? Variants { get; set; }

            public override string ToString() {
                return $"{Group}:{Name} ({Version}), {Subfolder}";
            }

        }

        /// <summary>
        /// Matches the sc4pac JSON schema for the info nested within a <see cref="Package"/>.
        /// </summary>
        public class PackageInfo {
            public string Summary { get; set; } = string.Empty;
            public string Author { get; set; } = string.Empty;
            public List<string> Images { get; set; } = [];
            public string Website { get; set; } = string.Empty;
            public List<string> Websites { get; set; } = [];
        }

        /// <summary>
        /// Matches the sc4pac JSON schema for a Variant within a <see cref="Package"/>.
        /// </summary>
        public class Variant {
            public List<PackageAsset> Assets { get; set; }
        }

        /// <summary>
        /// Matches the sc4pac JSON schema for an Asset directly included within a <see cref="Package"/>.
        /// </summary>
        public class PackageAsset {
            public string AssetId { get; set; }
            public List<string>? Include { get; set; }
            public List<string>? Exclude { get; set; }
            public List<Condition>? WithConditions { get; set; }
        }

        /// <summary>
        /// Matches the sc4pac JSON schema for parts of a <see cref="PackageAsset"/> to include or exclude.
        /// </summary>
        public class Condition {
            public List<string>? Include { get; set; }
            public List<string>? Exclude { get; set; }
        }

    }
}
