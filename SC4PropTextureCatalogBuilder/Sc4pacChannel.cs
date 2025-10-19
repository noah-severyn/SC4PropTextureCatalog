using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SC4PropTextureCatalogBuilder {
    public enum ChannelOptions {
        None = -1,
        All = 0,
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
    internal static class Sc4pacChannel {


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
            FileMgt.ExecuteCommand("cmd.exe", $"/C sc4pac channel build --output \"{outputPath.Replace("\\", "/")}\" \"{yamlPath.Replace("\\", "/")}\"");
        }


        /// <summary>
        /// Parse the channel JSON files to a list of packages and assets.
        /// </summary>
        /// <returns>A list of <see cref="JsonPackage"/> and a list of <see cref="JsonAsset"/> found</returns>
        internal static (List<JsonPackage>, List<JsonAsset>) ParseChannelJson(Dictionary<string, ChannelPaths> channels, ChannelOptions options) {
            string json;
            List<JsonPackage> packages = [];
            List<JsonAsset> assets = [];
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
                        channelFolder = Path.Combine(channels[key].JsonPath, "metadata");
                        paths.AddRange(Directory.EnumerateFiles(channelFolder, "*", SearchOption.AllDirectories).Where(path => path.EndsWith("latest\\pkg.json")));
                    }
                    break;
                default:
                    var name = options.ToString().ToLower();
                    channelFolder = Path.Combine(channels[name].JsonPath, "metadata");
                    paths.AddRange(Directory.EnumerateFiles(channelFolder, "*", SearchOption.AllDirectories).Where(path => path.EndsWith("latest\\pkg.json")));
                    break;
            }


            foreach (string path in paths) {
                json = File.ReadAllText(path);
                if (path.Contains("sc4pacAsset")) {
                    assets.Add(JsonSerializer.Deserialize<JsonAsset>(json, opt));
                } else {
                    packages.Add(JsonSerializer.Deserialize<JsonPackage>(json, opt));
                } 
            }

            DumpUrls(packages, assets, channels, options);
            return (packages, assets);
        }

        /// <summary>
        /// Dump the urls found in packages and assets for use in the Catalog progress tracker visual.
        /// </summary>
        private static void DumpUrls(List<JsonPackage> packages, List<JsonAsset> assets, Dictionary<string, ChannelPaths> channels, ChannelOptions co) {
            List<ChartData> data = [];

            foreach (JsonPackage pkg in packages) {
                if (pkg.Info.Websites.Count > 0) {
                    foreach (string url in pkg.Info.Websites) {
                        data.Add(ParseUrl(url));
                    }
                } else {
                    data.Add(ParseUrl(pkg.Info.Website));
                }
            }

            foreach (JsonAsset ast in assets) {
                data.Add(ParseUrl(ast.Url));
            }

            var json = JsonSerializer.Serialize(data);
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
                string name = url.Split("file/")[1].Replace("/", "");
                int.TryParse(name.Split('-')[0], out int id);
                return new ChartData(id, name, url);
            } else if (url.Contains("sc4evermore")) {
                return new ChartData();
            } else {
                return new ChartData();
            }
        }
    }

    public class JsonAsset {
        [JsonPropertyName("$type")]
        public string Type { get; set; } = string.Empty;
        public string AssetId { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string LastModified { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }

    public class JsonPackage {
        [JsonPropertyName("$type")]
        public string Type { get; set; } = string.Empty;
        public string Group { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Subfolder { get; set; } = string.Empty;
        public JsonPackageInfo Info { get; set; } = new JsonPackageInfo();

    }

    public class JsonPackageInfo {
        public string Summary { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public List<string> Images { get; set; } = [];
        public string Website { get; set; } = string.Empty;
        public List<string> Websites { get; set; } = [];
    }
}
