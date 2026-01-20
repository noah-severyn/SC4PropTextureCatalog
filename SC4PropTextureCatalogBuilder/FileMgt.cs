using System;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using Force.Crc32;
using static SC4PropTextureCatalogBuilder.SC4Pac;

namespace SC4PropTextureCatalogBuilder {
    /// <summary>
    /// Helper class for moving and extracting sc4pac cache files.
    /// </summary>
    public static class FileMgt {
        /// <summary>
        /// Execute and wait for a shell command to finish
        /// </summary>
        /// <param name="executable">Executable path</param>
        /// <param name="arguments">Executable arguments</param>
        public static void ExecuteCommand(string executable, string arguments) {
            ProcessStartInfo psi = new ProcessStartInfo {
                FileName = executable,
                Arguments = arguments,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using Process? process = Process.Start(psi);
            process?.WaitForExit();
            process?.Dispose();
        }

        /// <summary>
        /// Extract a Clickteam or Java-based installer
        /// </summary>
        public static void ExtractInstaller(string installerPath) {
            string extension = Path.GetExtension(installerPath);
            if (extension == ".exe") {
                ExecuteCommand("C:\\Program Files (x86)\\SC4 Utilities\\cicdec\\cicdec.exe", $"cicdec.exe \"{installerPath}\"");
            } 
            else if (extension == ".jar") {
                var newFolder = installerPath.Replace(".jar", "");
                ExecuteCommand("C:\\Program Files\\7-Zip\\7z.exe", $"x \"{installerPath}\" -o\"{newFolder}\" -y");
            }
        }

        /// <summary>
        /// Moves each exchange asset from the sc4pac cache and movse them to the new storage location, and extracts files in the new location from zips and cicdec installers. Each item is checked first if it has already been extracted before repeating.
        /// </summary>
        /// <remarks>
        /// I previously had issues with extracting/reading/deleting the files in place with files being locked up. Plus the long path of the cache folder caused some file path errors. This approach avoids both, and also prevents both having to re-extract each time this is run and from polluting the sc4pac cache area.
        /// </remarks>
        public static void ExtractAndMoveFiles(string sc4pacCachePath, string extractLocation) {
            IEnumerable<string> folders = Directory.EnumerateDirectories(sc4pacCachePath, "*", SearchOption.AllDirectories);
            foreach (string folder in folders) {
                int startIdx = folder.LastIndexOf('\\');
                string relativePath = folder.Replace(sc4pacCachePath, "");
                string newPath = Path.Join(extractLocation, relativePath);

                //Clean up any residual extracts in the sc4pac folder from before I visited the folder within the 7zip gui
                if (folder.ContainsAny("\\ex\\", "\\extract\\", "~")) {
                    Directory.Delete(folder, true);
                    continue;
                }

                //Skip the channel json contents
                if (folder.ContainsAny("\\channel\\", "\\metadata\\")) {
                    continue;
                }

                if (!Directory.Exists(newPath)) {
                    Directory.CreateDirectory(newPath);
                }

                //Extract the main folder(s) and then their contents if there are any cicdec installers
                IEnumerable<string> exchAssets = Directory.EnumerateFiles(folder).Where(f => !f.EndsWith(".checked") && !f.EndsWith(".json"));
                foreach (string exchAsset in exchAssets) {
                    string newFolder = Path.Combine(newPath, Path.GetFileName(exchAsset));
                    if (!Directory.Exists(newFolder)) {
                        ExecuteCommand("C:\\Program Files\\7-Zip\\7z.exe", $"x \"{exchAsset}\" -o\"{newFolder}\" -y");

                        Console.WriteLine("Extract " + newFolder);
                        IEnumerable<string> installers = Directory.EnumerateFiles(newFolder).Where(f => Path.GetExtension(f) == ".exe" || Path.GetExtension(f) == ".jar");
                        foreach (string installer in installers) {
                            ExtractInstaller(installer);
                        }
                    }
                }
            }
        }



        /// <summary>
        /// Returns an id for a known exchange based on the specified path or URL.
        /// </summary>
        internal static int GetExchangeId(string pathOrUrl) {
            if (pathOrUrl.Contains("simtropolis")) {
                return 1;
            } else if (pathOrUrl.Contains("sc4evermore.com")) {
                return 2;
            } else if (pathOrUrl.Contains("toutsimcities.com")) {
                return 3;
            } else if (pathOrUrl.Contains("hide-inoki.com")) {
                return 4;
            } else if (pathOrUrl.Contains("github.com")) {
                return 5;
            }
            return 0;
        }

        /// <summary>
        /// Extracts the upload id on the exchange from the specified path or URL.
        /// </summary>
        internal static int GetAssetId(string pathOrUrl) {
            string id = string.Empty;
            pathOrUrl = pathOrUrl.Replace("/", "\\").ToLower(); //So this works with web and file path urls
            try {
                if (pathOrUrl.Contains("simtropolis")) {
                    //P:\sc4pac-cache\https\community.simtropolis.com\files\file\45-bigbus-station\%3Fdo%3Ddownload%26r%3D22305\
                    //P:\sc4pac-cache\https\community.simtropolis.com\library\maxis\\sc4\buildings\Maxis_Buildings.zip\63 Building ← No available id
                    id = pathOrUrl.Split("file\\")[1].Split("-")[0];
                } else if (pathOrUrl.Contains("sc4evermore")) {
                    //P:\sc4pac-cache\https\www.sc4evermore.com\index.php\downloads%3Ftask%3Ddownload.send%26id%3D1%3Asc4d-lex-legacy-hkabt-dependencies-pack
                    //https://www.sc4evermore.com/index.php/downloads?task=download.send&id=404:gtg-bosch-building
                    pathOrUrl = WebUtility.UrlDecode(pathOrUrl);
                    id = pathOrUrl.Split("id=")[1].Split(":")[0];
                } else if (pathOrUrl.Contains("toutsimcities")) {
                    //P:\sc4pac-cache\https\www.toutsimcities.com\downloads\start\1780\TSC\Namspopof
                    id = pathOrUrl.Split("start\\")[1].Split("\\")[0];
                } else if (pathOrUrl.Contains("hide-inoki")) {
                    //P:\sc4pac-cache\http\hide-inoki.com\bbs\archives\files\1391.zip\NekoPropSet03
                    //P:\sc4pac-cache\http\hide-inoki.com\works\sc4\has_dependencies.zip ← No available id
                    id = pathOrUrl.Split("files\\")[1].Split(".")[0];
                } else if (pathOrUrl.Contains("github")) {
                    //P:\sc4pac-cache\https\github.com\NAMTeam\Network-Addon-Mod\releases\download\49_rev1\NetworkAddonMod_Setup_Version49_rev1.zip
                    id = Crc32Kinda(pathOrUrl.Split("github.com\\")[1].Split("\\releases")[0]);
                }
            }
            catch (IndexOutOfRangeException) {
                //Include special exceptions for known assets with no id otherwise
                if (pathOrUrl.Contains("Maxis_Buildings")) {
                    id = "1"; //Full id: 1-1
                } else if (pathOrUrl.Contains("has_dependencies")) {
                    id = "1"; //Full id: 4-1
                } else {
                    id = Crc32Kinda(pathOrUrl);
                }
            }

            _ = int.TryParse(id, out int assetId);
            return assetId;
        }
        private static string Crc32Kinda(string input) {
            // A quick, abbreviated way to turn a long string into a *somewhat* unique identifier. The *actual* checksum is irrelevant and not returned.
            // The first 4 bytes of the hexadecimal checksum as a decimal number are returned.
            // This is a fallback method of getting an identifier, so the risk of collisions is low based on the quantity of items that will be hashed.
            var bytes = System.Text.Encoding.UTF8.GetBytes(input);
            uint crc = Crc32Algorithm.Compute(bytes);
            string crc_s = crc.ToString("x8");
            _ = int.TryParse(crc_s.AsSpan(0, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int result);
            return result.ToString();
        }


        internal static string HttpToCachePath(string httpUrl) {
            //From: https://www.sc4evermore.com/index.php/downloads?  task=  download.send&  id=  13:  sfbt-essentials
            //To:   https\  www.sc4evermore.com\index.php\downloads%3Ftask%3Ddownload.send%26id%3D13%3Asfbt-essentials
            //From: https://community.simtropolis.com/files/file/600-majestic-drivein-theatre/?  do=  download&  r=  23019
            //To:   https  \community.simtropolis.com\files\file\600-majestic-drivein-theatre\%3Fdo%3Ddownload%26r%3D23019
            string cleanedUrl = WebUtility.UrlEncode(httpUrl);
            cleanedUrl = cleanedUrl.Replace("%3A%2F%2F", "\\").Replace("%2F", "\\");
            return cleanedUrl;
        }

        /// <summary>
        /// Strip query params from a http url, except for SC4E urls where the id param is a critical identifying part of the url.
        /// </summary>
        internal static string CleanUrl(string httpUrl) {
            if (httpUrl.Contains("sc4evermore")) {
                return httpUrl.Replace("?task=download.send&id=", "/download/");
            } else {
                return new Uri(httpUrl).GetLeftPart(UriPartial.Path);
            }
        }
    }
}
