using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using YamlDotNet;
using YamlDotNet.Core.Tokens;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SC4PropTextureCatalogBuilder {
    public enum ChannelOptions {
        None = -1,
        All = 0,
        Default,
        Simtropolis,
        SC4Evermore
    }

    public struct ChannelPaths(string yamlPath, string jsonPath) {
        public string YamlPath = yamlPath;
        public string JsonPath = jsonPath;
    }

    /// <summary>
    /// A representation of the metadata created for a sc4pac channel.
    /// </summary>
    internal static class Sc4pacChannel {


        /// <summary>
        /// Build sc4pac channel(s) converting the YAML files to JSON.
        /// </summary>
        /// <param name="channels"></param>
        /// <param name="options">The channel(s) to build</param>
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
                    Build(channels[name].YamlPath, channels[name].YamlPath);
                    break;
            }
        }
        /// <summary>
        /// Build a sc4pac channel.
        /// </summary>
        /// <param name="yamlPath">Input directory with YAML files</param>
        /// <param name="outputPath">Output directory for JSON files</param>
        private static void Build(string yamlPath, string outputPath) {
            FileMgt.ExecuteCommand("cmd.exe", $"/C sc4pac channel build --output \"{outputPath.Replace("\\", "/")}\" \"{yamlPath.Replace("\\", "/")}\"");
        }


        /// <summary>
        /// Parse the JSON files created from a sc4pac <c>channel build</c> operation.
        /// </summary>
        /// <param name="channelFolder">Folder containing the files to parse</param>
        /// <returns>A list of packages found</returns>
        internal static List<JsonPackage> ParseChannelJson(Dictionary<string, ChannelPaths> channels, ChannelOptions options) {
            string json;
            List<JsonPackage> packages = [];
            var opt = new JsonSerializerOptions {
                PropertyNameCaseInsensitive = true
            };

            List<string> paths = [];
            string channelFolder;

            switch (options) {
                case ChannelOptions.None:
                    return [];
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
                packages.Add(JsonSerializer.Deserialize<JsonPackage>(json, opt));
            }

            return packages;
        }

        internal static List<YamlAsset> ParseChannelYaml(Dictionary<string, ChannelPaths> channels, ChannelOptions options) {
            string yaml;
            List<YamlAsset> assets = [];
            var ds = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).IgnoreUnmatchedProperties().Build();

            List<string> paths = [];

            switch (options) {
                case ChannelOptions.None:
                    return [];
                case ChannelOptions.All:
                    foreach (var key in channels.Keys) {
                        paths.AddRange(Directory.EnumerateFiles(channels[key].YamlPath, "*.yaml", SearchOption.AllDirectories));
                    }
                    break;
                default:
                    var name = options.ToString().ToLower();
                    paths.AddRange(Directory.EnumerateFiles(channels[name].YamlPath, "*.yaml", SearchOption.AllDirectories));
                    break;
            }


            foreach (string path in paths) {
                yaml = File.ReadAllText(path);
                var parser = new YamlDotNet.Core.Parser(new StringReader(yaml));
                while (parser.MoveNext()) {
                    try {
                        var doc = ds.Deserialize<YamlAsset>(yaml);
                        if (doc != null) {
                            assets.Add(doc);
                        }
                    }
                    catch (Exception ex) {
                        Console.WriteLine($"Skipping invalid document: {ex.Message}");
                    }
                }
            }

            return assets;
        }

    }

    public class YamlAsset {
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
        public List<string> Websites { get; set; } = [];
    }
}
