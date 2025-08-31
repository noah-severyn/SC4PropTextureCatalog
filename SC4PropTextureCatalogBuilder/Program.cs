using csDBPF;
using SC4PropTextureCatalogBuilder;
using System.Diagnostics;
using System.Linq;

// ===================================================================================================================================================
var createDb = false;
byte exchangeId = 1;
ChannelOptions buildOpt = ChannelOptions.None; //Which channels to build YMAL → JSON
ChannelOptions parseOpt = ChannelOptions.Default; //Which channels to parse JSON → database
// ===================================================================================================================================================
const string sc4pacRoot = "C:\\Users\\Administrator\\AppData\\Local\\io.github.memo33\\sc4pac\\cache\\coursier\\";
const string extractRoot = "P:\\sc4pac-cache";

Dictionary<byte, string> exchanges = [];
exchanges.Add(1, sc4pacRoot + "https\\community.simtropolis.com");
exchanges.Add(2, sc4pacRoot + "https\\www.sc4evermore.com");
exchanges.Add(3, sc4pacRoot + "https\\www.toutsimcities.com");
exchanges.Add(4, sc4pacRoot + "http\\hide-inoki.com");

Dictionary<string, ChannelPaths> channels = [];
channels.Add("default", new ChannelPaths("C:\\source\\repos\\sc4pac\\src\\yaml", "C:\\Users\\Administrator\\sc4pac-default-channel\\json"));
channels.Add("simtropolis", new ChannelPaths("C:\\source\\repos\\simtropolis-channel\\src\\yaml", "C:\\Users\\Administrator\\sc4pac-simtropolis-channel\\json"));
// ===================================================================================================================================================

//Extract each exchange asset from all zips and cicdec installers first and move to the backup location. Checks if the item has already been extracted before repeating.
//ExtractAndMoveFiles(exchanges[exchangeId]);

//Optionally build the channels and then parse their JSON metadata
//Sc4pacChannel.BuildChannels(channels, buildOpt);
var packages = Sc4pacChannel.ParseChannelJson(channels, parseOpt);


//Parse each item from the backup location and populate the database
//BuildDatabase(createDb, exchangeId);


static void ExtractAndMoveFiles(string sc4pacCachePath) {
    IEnumerable<string> folders = Directory.EnumerateDirectories(sc4pacCachePath, "*", SearchOption.AllDirectories);
    foreach (string folder in folders) {
        int startIdx = folder.LastIndexOf('\\');
        string relativePath = folder.Replace(sc4pacRoot, "");
        string newPath = Path.Combine(extractRoot, relativePath);

        //Clean up any residual extracts in the sc4pac folder from before I visited the folder within the 7zip gui
        if (folder.ContainsAny("\\ex\\", "\\extract\\", "~")) {
            Directory.Delete(folder, true);
            continue;
        }

        if (!Directory.Exists(newPath)) {
            Directory.CreateDirectory(newPath);
        }

        //Extract the main folder(s) and then their contents if there are any cicdec installers
        IEnumerable<string> exchAssets = Directory.EnumerateFiles(folder).Where(f => !f.EndsWith(".checked"));
        foreach (string exchAsset in exchAssets) {
            string newAsset = Path.Combine(newPath, Path.GetFileName(exchAsset));
            if (!Directory.Exists(newAsset)) {
                ExtractZipFile(exchAsset, newAsset);
                Console.WriteLine("Extract " + relativePath);
                IEnumerable<string> installers = Directory.EnumerateFiles(newAsset).Where(f => Path.GetExtension(f) == ".exe");
                foreach (string installer in installers) {
                    ExtractInstaller(installer);
                }
            }
        }
    }
}

static void BuildDatabase(bool createDb, byte exchangeId) {
    string dbPath;
    if (createDb) {
        dbPath = $"C:\\source\\repos\\SC4PropTextureCatalog\\SC4PropTextureCatalogBuilder\\data\\Catalog-{DateTime.Now.ToString("MM-dd-HH-mm-ss")}.db";
    } else {
        dbPath = new DirectoryInfo("C:\\source\\repos\\SC4PropTextureCatalog\\SC4PropTextureCatalogBuilder\\data").GetFiles().OrderByDescending(f => f.LastWriteTime).First().FullName;
    }

    DatabaseBuilder db = new DatabaseBuilder(dbPath, createDb);

    var errors = new List<DBPFError>();
    foreach (string folder in Directory.EnumerateDirectories("P:\\sc4pac-cache\\https\\community.simtropolis.com\\files\\file")) {
        int startIdx = folder.LastIndexOf('\\');
        _ = int.TryParse(folder.AsSpan(startIdx + 1, folder.IndexOf('-', startIdx) - startIdx - 1), out int assetId);

        if (!db.AssetExists(exchangeId, assetId)) {
            Console.WriteLine(assetId);
            errors = db.ParseFolder(folder, exchangeId, assetId);
        }
    }
    Console.WriteLine(errors.Count);
}

/// <summary>
/// Extract a 7zip archive file
/// </summary>
/// <param name="archivePath">File to extract</param>
/// <param name="toFolder">Output folder</param>
static void ExtractZipFile(string archivePath, string toFolder) {
    ProcessStartInfo psi = new ProcessStartInfo {
        FileName = "C:\\Program Files\\7-Zip\\7z.exe",
        Arguments = $"x \"{archivePath}\" -o\"{toFolder}\" -y",
        RedirectStandardOutput = true,
        UseShellExecute = false,
        CreateNoWindow = true
    };

    using Process? process = Process.Start(psi);
    process?.WaitForExit();
    process?.Dispose();
}

/// <summary>
/// Extract a Clickteam installer with cicdec
/// </summary>
/// <param name="archivePath">File to extract</param>
/// <remarks>By default, cicdec extracts to a new subfolder with the same name as the file. This is preferred, because it eliminates the risk of multiple extractions producing files that would overwrite each other (multiple installers contain file(s) with the same name). If this is the case, the cicdec cli requires requires user input for how to proceed. No commandline args are provided to skip this input. However, extracting to a subfolder *may* cause a hidden path too long error and cicdec will hang. Prefer extracting to a subfolder to reduce file name collision risks, and extract to the root folder as a fallback if the path is too long</remarks>
static void ExtractInstaller(string archivePath) {
    ProcessStartInfo psi = new ProcessStartInfo {
        FileName = "C:\\Program Files (x86)\\SC4 Utilities\\cicdec\\cicdec.exe",
        Arguments = $"cicdec.exe \"{archivePath}\"",
        RedirectStandardOutput = true,
        UseShellExecute = false,
        CreateNoWindow = true
    };

    using Process? process = Process.Start(psi);
    process?.WaitForExit();
    process?.Dispose();
}