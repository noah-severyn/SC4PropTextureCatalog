using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
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
            ProcessStartInfo psi = new ProcessStartInfo {
                FileName = "cmd.exe",
                Arguments = $"/C sc4pac channel build --output \"{outputPath.Replace("\\", "/")}\" \"{yamlPath.Replace("\\", "/")}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using Process? process = Process.Start(psi);
            process?.WaitForExit();
            process?.Dispose();
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

    }


    internal class JsonPackage {
        [JsonPropertyName("$type")]
        public string Type { get; set; } = string.Empty;
        public string Group { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Subfolder { get; set; } = string.Empty;
        public JsonPackageInfo Info { get; set; } = new JsonPackageInfo();

    }

    internal class JsonPackageInfo {
        public string Summary { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public List<string> Images { get; set; } = [];
        public List<string> Websites { get; set; } = [];
    }
}
