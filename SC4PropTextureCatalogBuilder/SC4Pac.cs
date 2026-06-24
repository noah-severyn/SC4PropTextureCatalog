using System;
using System.Collections.Concurrent;
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
        /// <returns>A dictionary of <see cref="Package"/>s keyed by the sc4pac package id, and a dictionary of <see cref="Asset"/>s, keyed by the sc4pac asset id.</returns>
        internal static (Dictionary<string, Package>, Dictionary<string, Asset>) ParseChannelJson(Dictionary<string, ChannelPaths> channels, ChannelOptions options) {
            ConcurrentDictionary<string, Package> packages = new();
            ConcurrentDictionary<string, Asset> assets = new();
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

            Console.WriteLine("  > Parsing YAML files ...");
            Parallel.ForEach(paths, path => {
                using var stream = File.OpenRead(path);
                if (path.Contains("sc4pacAsset")) {
                    var asset = JsonSerializer.Deserialize<Asset>(stream, opt) ?? new Asset();
                    _ = assets.TryAdd(asset.AssetId, asset);
                } else {
                    var pkg = JsonSerializer.Deserialize<Package>(stream, opt) ?? new Package();
                    _ = packages.TryAdd(pkg.Group + ":" + pkg.Name, pkg);
                }
            });

            return (new Dictionary<string, Package>(packages), new Dictionary<string, Asset>(assets));
        }
        private static IEnumerable<string> Parse(string yamlPath, string jsonPath) {
            Console.WriteLine("  > parsing " + yamlPath);
            string channelFolder = Path.Combine(jsonPath, "metadata");
            if (!Directory.Exists(channelFolder)) {
                Directory.CreateDirectory(channelFolder);
            }
            return Directory.EnumerateFiles(channelFolder, "*", SearchOption.AllDirectories).Where(path => path.EndsWith("latest\\pkg.json") && !path.Contains("_ext"));
        }


        /// <summary>
        /// List all SC4 files in the extract location once to avoid repetitive expensive <c>Directory.GetFiles</c> calls.
        /// </summary>
        internal static HashSet<string> ListCacheFiles(string extractFolder) {
            Console.WriteLine("  > fetching all SC4 files in extract location ...");
            return Directory.EnumerateFiles(extractFolder, "*", SearchOption.AllDirectories)
                .AsParallel()
                .Where(p => p.IsDBPF())
                .ToHashSet();
        }

        /// <summary>
        /// Iterate each <see cref="Package"/> and examine tne the <c>include</c> and <c>exclude</c> properties of each <see cref="Asset"/> to determine list of referenced cache files. Save this information to <see cref="Package.LocalFiles"/>.
        /// </summary>
        /// <returns>A list of <see cref="Asset.AssetId"/>s which are referenced in a package but missing from the cache.</returns>
        public static List<string> ExtractFilesFromPackages(HashSet<string> sc4Files, ref Dictionary<string, Package> packages, Dictionary<string, Asset> assets) {
            Console.WriteLine("  > extracting referenced files from sc4pac packages ...");
            
            HashSet<string> missingAssets = new HashSet<string>();
            foreach (var pkg in packages.Values) {
                //Extract the list of PackageAsset(s) out of the package
                var pkgAssets =
                    (pkg.Assets ?? Enumerable.Empty<PackageAsset>())
                    //Append to that list the PackageAsset(s) found nested inside a variant
                    .Concat(
                        (pkg.Variants ?? Enumerable.Empty<Variant>())
                            .SelectMany(v => v.Assets ?? Enumerable.Empty<PackageAsset>())
                    );

                HashSet<PkgFileItem> result = []; //Use a Hashset to enforce unique files in case a duplicate files is added to an asset
                string pkgName = pkg.Group + ":" + pkg.Name;
                foreach (var pkgAsset in pkgAssets) {
                    if (assets.TryGetValue(pkgAsset.AssetId, out var asset)) {
                        var files = ResolveAssetFiles(sc4Files, asset, pkgAsset.Include ?? [], pkgAsset.Exclude ?? []);
                        foreach (var file in files) {
                            result.Add(new PkgFileItem(pkgName, asset.AssetId, file));
                        }
                    } else {
                        missingAssets.Add(pkgAsset.AssetId);
                    }
                }
                pkg.LocalFiles = result.ToList();
            }
            return missingAssets.ToList();
        }

        private static List<string> ResolveAssetFiles(HashSet<string> sc4Files, Asset asset, List<string> includeRules, List<string> excludeRules) {
            var folderPart = FileMgt.HttpToCachePath(asset.Url);
            var assetFiles = sc4Files.Where(f => f.Contains(folderPart)).ToList();
            
            bool hasInclude = includeRules.Count > 0;
            bool hasExclude = excludeRules.Count > 0;

            if (!hasInclude && !hasExclude) {
                return assetFiles;
            }

            var includeRegexes = hasInclude ? includeRules.Select(rule => BuildRegex(rule)).ToList() : null;
            var excludeRegexes = hasExclude ? excludeRules.Select(rule => BuildRegex(rule)).ToList() : null;

            List<string> result;
            if (hasInclude && hasExclude) {
                var includeMatches = new HashSet<string>();
                var excludeMatches = new HashSet<string>();
                
                foreach (var file in assetFiles) {
                    bool isIncluded = includeRegexes!.Any(rgx => rgx.IsMatch(file));
                    bool isExcluded = excludeRegexes!.Any(rgx => rgx.IsMatch(file));
                    
                    if (isIncluded) {
                        includeMatches.Add(file);
                    }
                    if (isExcluded) {
                        excludeMatches.Add(file);
                    }
                }

                //Remove excluded files, but only if they were not also included.
                //If there are variants that include/exclude each other's files, they will appear in both lists. We want to include these files regardless, as they are included in the package in some capacity.
                result = includeMatches.Where(file => !excludeMatches.Contains(file) || includeMatches.Contains(file))
                    .ToList();
            } 
            else if (hasInclude) {
                result = assetFiles.Where(file => includeRegexes!.Any(rgx => rgx.IsMatch(file)))
                    .ToList();
            } 
            else {
                result = assetFiles.Where(file => !excludeRegexes!.Any(rgx => rgx.IsMatch(file)))
                    .ToList();
            }
            
            return result;
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
