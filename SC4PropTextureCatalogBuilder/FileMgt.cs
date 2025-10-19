using System;
using System.Diagnostics;

namespace SC4PropTextureCatalogBuilder {
    /// <summary>
    /// Helper class for moving and extracting sc4pac cache files
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

                        Console.WriteLine("Extract " + relativePath);
                        IEnumerable<string> installers = Directory.EnumerateFiles(newFolder).Where(f => Path.GetExtension(f) == ".exe" || Path.GetExtension(f) == ".jar");
                        foreach (string installer in installers) {
                            ExtractInstaller(installer);
                        }
                    }
                }
            }
        }

    }
}
