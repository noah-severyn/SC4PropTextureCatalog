// See https://aka.ms/new-console-template for more information
using csDBPF;
using SC4PropTextureCatalogBuilder;
using System.Linq;

// ===================================================================================================================================================
var createDb = false;
var exchangeId = 1;
// ===================================================================================================================================================
var rootFolder = "C:\\Users\\Administrator\\AppData\\Local\\io.github.memo33\\sc4pac\\cache\\coursier\\https\\community.simtropolis.com\\files\\file";
string dbPath;
if (createDb) {
    dbPath = $"C:\\source\\repos\\SC4PropTextureCatalog\\SC4PropTextureCatalogBuilder\\data\\Catalog-{DateTime.Now.ToString("MM-dd-HH-mm-ss")}.db";
} else {
    dbPath = "C:\\source\\repos\\SC4PropTextureCatalog\\SC4PropTextureCatalogBuilder\\data\\Catalog-06-01-15-12-41.db";
}

//these are items I have trouble parsing for some reason
int[] st_skips = [
    0
    , 22864
    , 31653
    , 33619
    , 35927
    , 36841
]; 

DatabaseBuilder db = new DatabaseBuilder(dbPath, createDb);

foreach (string folder in Directory.EnumerateDirectories(rootFolder)) {
    int startIdx = folder.LastIndexOf('\\');
    _ = int.TryParse(folder.AsSpan(startIdx + 1, folder.IndexOf('-', startIdx) - startIdx - 1), out int assetId);

    //Add TGIs to the database
    if (!st_skips.Contains(assetId) && !db.AssetExists(exchangeId, assetId)) {
        Console.WriteLine(assetId);
        (var tgis, var errors) = db.ParseFolder(exchangeId, assetId, folder);
        db.AddTGIs(tgis);
    }
}
