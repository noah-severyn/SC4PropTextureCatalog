using SC4PropTextureCatalogBuilder;
using System.Diagnostics;
using System.Linq;

// ===================================================================================================================================================
var createDb = true;
byte exchangeId = 1;
// ===================================================================================================================================================
const string sc4pacRoot = "C:\\Users\\Administrator\\AppData\\Local\\io.github.memo33\\sc4pac\\cache\\coursier\\";
const string extractRoot = "P:\\sc4pac-cache";

Dictionary<byte, string> exchanges = [];
exchanges.Add(1, sc4pacRoot + "https\\community.simtropolis.com");
exchanges.Add(2, sc4pacRoot + "https\\www.sc4evermore.com");
exchanges.Add(3, sc4pacRoot + "https\\www.toutsimcities.com");
exchanges.Add(4, sc4pacRoot + "http\\hide-inoki.com");

//Extract each exchange asset from all zips and cicdec installers first. Don't repeati this every time the catalog is generated
IEnumerable<string> folders = Directory.EnumerateDirectories(exchanges[exchangeId], "*", SearchOption.AllDirectories);
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




string dbPath;
if (createDb) {
    dbPath = $"C:\\source\\repos\\SC4PropTextureCatalog\\SC4PropTextureCatalogBuilder\\data\\Catalog-{DateTime.Now.ToString("MM-dd-HH-mm-ss")}.db";
} else {
    dbPath = "C:\\source\\repos\\SC4PropTextureCatalog\\SC4PropTextureCatalogBuilder\\data\\Catalog-06-01-15-12-41.db";
    dbPath = "C:\\source\\repos\\SC4PropTextureCatalog\\SC4PropTextureCatalogBuilder\\data\\Catalog-06-01-17-25-17.db";
}

DatabaseBuilder db = new DatabaseBuilder(dbPath, createDb);

foreach (string folder in Directory.EnumerateDirectories(rootFolder)) {
foreach (string folder in Directory.EnumerateDirectories(exchanges[exchangeId])) {
    int startIdx = folder.LastIndexOf('\\');
    _ = int.TryParse(folder.AsSpan(startIdx + 1, folder.IndexOf('-', startIdx) - startIdx - 1), out int assetId);

    //Add TGIs to the database
    if (!db.AssetExists(exchangeId, assetId)) {
        Console.WriteLine(assetId);
        (var tgis, var errors) = db.ParseFolder(exchangeId, assetId, folder);
        db.AddTGIs(tgis);
    }
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