using csDBPF;
using SC4PropTextureCatalogBuilder;
using System.Diagnostics;
using System.Linq;

// ===================================================================================================================================================
var createDb = false;
byte exchangeId = 1;
ChannelOptions buildOpt = ChannelOptions.None; //Which channels to build YMAL → JSON
ChannelOptions parseOpt = ChannelOptions.Simtropolis; //Which channels to parse JSON → database
// ===================================================================================================================================================
const string sc4pacCachePath = "C:\\Users\\Administrator\\AppData\\Local\\io.github.memo33\\sc4pac\\cache\\coursier";
const string extractLocation = "P:\\sc4pac-cache";
string dbPath = "C:\\source\\repos\\SC4PropTextureCatalog\\SC4PropTextureCatalogBuilder\\data";
if (createDb) {
    dbPath = Path.Combine(dbPath, $"Catalog-{DateTime.Now.ToString("MM-dd-HH-mm-ss")}.db");
} else {
    dbPath = new DirectoryInfo(dbPath).GetFiles().OrderByDescending(f => f.LastWriteTime).First().FullName;
}

Dictionary<byte, string> exchanges = [];
exchanges.Add(1, Path.Combine(sc4pacCachePath, "https\\community.simtropolis.com"));
exchanges.Add(2, Path.Combine(sc4pacCachePath, "https\\www.sc4evermore.com"));
exchanges.Add(3, Path.Combine(sc4pacCachePath, "https\\www.toutsimcities.com"));
exchanges.Add(4, Path.Combine(sc4pacCachePath, "http\\hide-inoki.com"));

Dictionary<string, ChannelPaths> channels = [];
channels.Add("default", new ChannelPaths("C:\\source\\repos\\sc4pac\\src\\yaml", "C:\\Users\\Administrator\\sc4pac-default-channel\\json", string.Empty));
channels.Add("simtropolis", new ChannelPaths("C:\\source\\repos\\simtropolis-channel\\src\\yaml", "C:\\Users\\Administrator\\sc4pac-simtropolis-channel\\json", "C:\\source\\repos\\SC4PropTextureCatalog\\SC4PropTextureCatalog4\\data\\stex-data.json"));
channels.Add("sc4evermore", new ChannelPaths("C:\\source\\repos\\sc4e-channel\\src\\yaml", "C:\\Users\\Administrator\\sc4pac-sc4evermore-channel\\json", "C:\\source\\repos\\SC4PropTextureCatalog\\SC4PropTextureCatalog4\\data\\sc4e-data.json"));
// ===================================================================================================================================================

//Extract each exchange asset from all zips and cicdec installers first and move to the backup location. Checks if the item has already been extracted before repeating.
FileMgt.ExtractAndMoveFiles(sc4pacCachePath, extractLocation);

//Optionally build the channels and then parse their JSON metadata
Sc4pacChannel.BuildChannels(channels, buildOpt);
(var packages, var assets) = Sc4pacChannel.ParseChannelJson(channels, parseOpt);


//Parse each item from the backup location and populate the database
DatabaseBuilder db = new DatabaseBuilder(dbPath, createDb);
//db.BuildTGITable(parseOpt);
//db.BuildPackageTable(packages);

