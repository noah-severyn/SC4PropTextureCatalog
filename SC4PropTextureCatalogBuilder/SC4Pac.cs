using System.Text.Json;
using System.Text.RegularExpressions;
using csDBPF;

namespace SC4PropTextureCatalogBuilder {
    public enum ChannelOptions {
        None,
        All,
        Default,
        Simtropolis,
        SC4Evermore
    }

    public struct ChannelPaths(string yamlPath, string jsonPath) {
        public string YamlPath = yamlPath;
        public string JsonPath = jsonPath;
    }

    /// <summary>
    /// Helps relate a collection of files on disk to a database item via the <see cref="ExchangeId"/> and <see cref="AssetId"/>.
    /// </summary>
    public struct CacheAsset(string path) {
        public int ExchangeId = FileMgt.GetExchangeId(path);
        public int AssetId = FileMgt.GetAssetId(path);
        public string FilePath = path;
    }

    /// <summary>
    /// A representation of the metadata created for a sc4pac channel.
    /// </summary>
    internal static partial class SC4Pac {


        /// <summary>
        /// Build sc4pac channel(s) with the sc4pac <c>channel build</c> command, converting the YAML metadata files to JSON for easier parsing.
        /// </summary> 
        /// .0 
        internal static void BuildChannels(Dictionary<string, ChannelPaths> channels, ChannelOptions options) {
            switch (options) {
                case ChannelOptions.None:
                    return;
                case ChannelOptions.All:
                    foreach (var key in channels.Keys) {
                        Build(channels[key].YamlPath, channels[key].JsonPath);
                    }
                    break;
                default:
                    var name = options.ToString().ToLower();
                    Build(channels[name].YamlPath, channels[name].JsonPath);
                    break;
            }
        }
        private static void Build(string yamlPath, string outputPath) {
            Console.WriteLine("  > building " + outputPath);
            FileMgt.ExecuteCommand("cmd.exe", $"/C sc4pac channel build --output \"{outputPath.Replace("\\", "/")}\" \"{yamlPath.Replace("\\", "/")}\"");
        }


        /// <summary>
        /// Parse the channel JSON files to a list of packages and assets.
        /// </summary>
        /// <returns>A list of <see cref="Package"/> and a list of <see cref="Asset"/> found</returns>
        internal static (List<Package>, List<Asset>) ParseChannelJson(Dictionary<string, ChannelPaths> channels, ChannelOptions options) {
            string json;
            List<Package> packages = [];
            List<Asset> assets = [];
            var opt = new JsonSerializerOptions {
                PropertyNameCaseInsensitive = true
            };

            List<string> paths = [];
            switch (options) {
                case ChannelOptions.None:
                    return ([], []);
                case ChannelOptions.All:
                    foreach (var key in channels.Keys) {
                        paths.AddRange(Parse(channels[key].YamlPath, channels[key].JsonPath));
                    }
                    break;
                default:
                    var name = options.ToString().ToLower();
                    paths.AddRange(Parse(channels[name].YamlPath, channels[name].JsonPath));
                    break;
            }

            foreach (string path in paths) {
                json = File.ReadAllText(path);
                if (path.Contains("sc4pacAsset")) {
                    var asset = JsonSerializer.Deserialize<Asset>(json, opt);
                    //asset.LocalFilePath = path;
                    assets.Add(asset);
                } else {
                    packages.Add(JsonSerializer.Deserialize<Package>(json, opt));
                }
            }

            return (packages, assets);
        }
        private static IEnumerable<string> Parse(string yamlPath, string jsonPath) {
            Console.WriteLine("  > parsing " + yamlPath);
            string channelFolder = Path.Combine(jsonPath, "metadata");
            if (!Directory.Exists(channelFolder)) {
                Directory.CreateDirectory(channelFolder);
            }
            return Directory.EnumerateFiles(channelFolder, "*", SearchOption.AllDirectories).Where(path => path.EndsWith("latest\\pkg.json") && !path.Contains("_ext"));
        }

        public static void ExtractFilesFromJson(string extractFolder, ref List<Package> packages, List<Asset> assets) {
            //var allPackageAssets =  packages
            //    .SelectMany(pkg =>
            //        //Extract the list of PackageAsset(s) out of the package
            //        (pkg.Assets ?? Enumerable.Empty<PackageAsset>())
            //        //Append to that list the PackageAsset(s) found nested inside a variant
            //        .Concat(
            //            (pkg.Variants ?? Enumerable.Empty<Variant>())
            //                .SelectMany(v => v.Assets ?? Enumerable.Empty<PackageAsset>())
            //        )
            //    )
            //    .ToList();

            HashSet<string> missingAssets = new HashSet<string>();
            foreach (var pkg in packages) {
                //Extract the list of PackageAsset(s) out of the package
                var pkgAssets =
                    (pkg.Assets ?? Enumerable.Empty<PackageAsset>())
                    //Append to that list the PackageAsset(s) found nested inside a variant
                    .Concat(
                        (pkg.Variants ?? Enumerable.Empty<Variant>())
                            .SelectMany(v => v.Assets ?? Enumerable.Empty<PackageAsset>())
                    );

                List<string> result = [];
                foreach (var asset in pkgAssets) {
                    var url = assets.Find(a => a.AssetId == asset.AssetId).Url;
                    var folder = Path.Combine(extractFolder, FileMgt.HttpToCachePath(url));
                    if (Directory.Exists(folder)) {
                        result.AddRange(ResolveAssetFiles(folder, asset.Include ?? [], asset.Exclude ?? []));
                    } else {
                        missingAssets.Add(folder);
                    }
                }
                pkg.LocalFiles = result;
            }

        }

        private static List<string> ResolveAssetFiles(string folder, List<string> includeRules, List<string> excludeRules) {
            var files = Directory.GetFiles(folder, "*", SearchOption.AllDirectories)
                .Select(p => Path.GetRelativePath(folder, p))
                .Where(p => p.IsDBPF())
                .ToList();
            bool hasInclude = includeRules.Count > 0;
            bool hasExclude = excludeRules.Count > 0;

            var includeRegexes = hasInclude ? includeRules.Select(rule => BuildRegex(rule)).ToList() : [];
            var includeMatches = hasInclude ? files.Where(file => includeRegexes.Any(rgx => rgx.IsMatch(file))).ToHashSet() : [];
            var excludeRegexes = hasExclude ? excludeRules.Select(rule => BuildRegex(rule)).ToList() : [];
            var excludeMatches = hasExclude ? files.Where(file => excludeRegexes.Any(rgx => rgx.IsMatch(file))).ToHashSet() : [];

            IEnumerable<string> result;
            if (hasInclude) {
                result = includeMatches;
                if (hasExclude) {
                    //Remove excluded files, but only if they were not also included.
                    //If there are variants that include/exclude each other's files, they will appear in both lists. We want to include these files regardless, as they are included in the package in some capacity.
                    result = result.Where(file => !excludeMatches.Contains(file) || includeMatches.Contains(file));
                }
            } else if (hasExclude) {
                result = files.Where(file => !excludeMatches.Contains(file));
            } else {
                result = files;
            }
            return result.ToList();
        }

        private static Regex BuildRegex(string rule) {
            //Most rules start with / or \, except they represent relative file paths, not regex anchors → remove them
            //We need to handle situations where sometimes the regexes are literal file names, and other times they are active regex sequences
            rule = rule.TrimStart('/', '\\').TrimEnd('/', '\\');
            Regex rgx;
            try {
                rgx = new Regex(rule, RegexOptions.IgnoreCase);
                return rgx;
            }
            catch (Exception) {
                rgx = new Regex(Regex.Escape(rule), RegexOptions.IgnoreCase);
                return rgx;
            }
        }
    }

}
