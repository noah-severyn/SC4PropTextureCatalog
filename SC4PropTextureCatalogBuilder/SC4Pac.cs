using System.Text.Json;

namespace SC4PropTextureCatalogBuilder {
    public enum ChannelOptions {
        None,
        All,
        Default,
        Simtropolis,
        SC4Evermore
    }

    public struct ChannelPaths(string yamlPath, string jsonPath, string trackerPath) {
    public struct ChannelPaths(string yamlPath, string jsonPath) {
        public string YamlPath = yamlPath;
        public string JsonPath = jsonPath;
    }

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
            string channelFolder;

            switch (options) {
                case ChannelOptions.None:
                    return ([], []);
                case ChannelOptions.All:
                    foreach (var key in channels.Keys) {
                        Console.WriteLine("  > parsing " + channels[key].YamlPath);
                        channelFolder = Path.Combine(channels[key].JsonPath, "metadata");
                        if (!Directory.Exists(channelFolder)) {
                            Directory.CreateDirectory(channelFolder);
                        }
                        paths.AddRange(Directory.EnumerateFiles(channelFolder, "*", SearchOption.AllDirectories).Where(path => path.EndsWith("latest\\pkg.json") && !path.Contains("_ext")));
                    }
                    break;
                default:
                    var name = options.ToString().ToLower();
                    Console.WriteLine("  > parsing " + channels[name].YamlPath);
                    channelFolder = Path.Combine(channels[name].JsonPath, "metadata");
                    if (!Directory.Exists(channelFolder)) {
                        Directory.CreateDirectory(channelFolder);
                    }
                    paths.AddRange(Directory.EnumerateFiles(channelFolder, "*", SearchOption.AllDirectories).Where(path => path.EndsWith("latest\\pkg.json") && !path.Contains("_ext")));
                    break;
            }


            foreach (string path in paths) {
                json = File.ReadAllText(path);
                if (path.Contains("sc4pacAsset")) {
                    assets.Add(JsonSerializer.Deserialize<Asset>(json, opt));
                } else {
                   packages.Add(JsonSerializer.Deserialize<Package>(json, opt));
                } 
            }

            return (packages, assets);
        }
            }
        }

    }

}
