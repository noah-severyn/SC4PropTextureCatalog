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
        public string YamlPath = yamlPath;
        public string JsonPath = jsonPath;
        /// <summary>
        /// Local project path for the Catalog website tracker visual.
        /// </summary>
        public string TrackerPath = trackerPath;
    }

    public struct ChartData(int id, string name, string url) {
        public int Id { get; set; } = id;
        public string Name { get; set; } = name;
        public string Url { get; set; } = url;
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

        /// <summary>
        /// Dump the urls found in packages and assets for use in the Catalog progress tracker visual.
        /// </summary>
        public static void DumpUrls(List<Package> packages, List<Asset> assets, Dictionary<string, ChannelPaths> channels, ChannelOptions co) {
            List<ChartData> data = [];

            foreach (Package pkg in packages) {
                if (pkg.Info.Websites.Count > 0) {
                    foreach (string url in pkg.Info.Websites) {
                        data.Add(ParseUrl(url));
                    }
                } else {
                    data.Add(ParseUrl(pkg.Info.Website));
                }
            }

            foreach (Asset ast in assets) {
                data.Add(ParseUrl(ast.Url));
            }

            var opts = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(data, opts);
            switch (co) {
                case ChannelOptions.None:
                    return;
                case ChannelOptions.All:
                    foreach (var key in channels.Keys) {
                        File.WriteAllText(channels[key].TrackerPath, json);
                    }
                    break;
                default:
                    var name = co.ToString().ToLower();
                    File.WriteAllText(channels[name].TrackerPath, json);
                    break;
            }
        }

        private static ChartData ParseUrl(string url) {
            if (url.Contains("simtropolis")) {
                try {
                    string name = url.Split("file/")[1].Replace("/", "");
                    _ = int.TryParse(name.Split('-')[0], out int id);
                    return new ChartData(id, name, url);
                }
                catch (IndexOutOfRangeException) {
                    return new ChartData();
                }
            } else if (url.Contains("sc4evermore")) {
                return new ChartData();
            } else {
                return new ChartData();
            }
        }
    }

}
