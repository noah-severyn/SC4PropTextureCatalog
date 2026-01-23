using System;
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
    /// Helps relate which files are included inside a package.
    /// </summary>
    /// <remarks>We cannot assume any content is in the database at this point, so the package and asset Names are used instead of Ids.
    public struct PkgFileItem(string pkgName, string assetName, string filePath) {
        public string PackageName = pkgName;
        public string AssetName = assetName;
        public string FilePath = filePath;
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
                    var asset = JsonSerializer.Deserialize<Asset>(json, opt) ?? new Asset();
                    assets.Add(asset);
                } else {
                    var pkg = JsonSerializer.Deserialize<Package>(json, opt) ?? new Package();
                    packages.Add(pkg);
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

        public static List<string> ExtractFilesFromJson(string extractFolder, ref List<Package> packages, List<Asset> assets) {
            Console.WriteLine("  > extracting files from json ...");
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

                List<PkgFileItem> result = [];
                foreach (var pkgAsset in pkgAssets) {
                    var asset = assets.Find(a => a.AssetId == pkgAsset.AssetId);
                    
                    if (asset is not null) {
                        var files = ResolveAssetFiles(asset, extractFolder, pkgAsset.Include ?? [], pkgAsset.Exclude ?? []);
                        List<PkgFileItem> pfis = files.Select(f => new PkgFileItem(pkg.Group + ":" + pkg.Name, asset.AssetId, Path.GetFileName(f))).ToList();
                        result.AddRange(pfis);
                    } else {
                        missingAssets.Add(asset.Url);
                    }
                }
                pkg.LocalFiles = result;
            }
            return missingAssets.ToList();
        }

        private static List<string> ResolveAssetFiles(Asset asset, string baseFolder, List<string> includeRules, List<string> excludeRules) {
            var folder = FileMgt.HttpToCachePath(baseFolder, asset.Url);
            if (!Directory.Exists(folder)) {
                return [];
            }
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
