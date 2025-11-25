using SC4PropTextureCatalogBuilder;

// ===================================================================================================================================================
bool createDb = false;
ChannelOptions buildOpt = ChannelOptions.All; //Which channels to build YMAL → JSON
ChannelOptions parseOpt = ChannelOptions.All; //Which channels to parse JSON → database
// ===================================================================================================================================================
const string sc4pacCachePath = "C:\\Users\\Administrator\\AppData\\Local\\io.github.memo33\\sc4pac\\cache\\coursier";
const string extractLocation = "P:\\sc4pac-cache";
string dataPath = "C:\\source\\repos\\SC4PropTextureCatalog\\SC4PropTextureCatalogBuilder\\data";
string apiPath = "C:\\source\\repos\\SC4PropTextureCatalog\\SC4PropTextureCatalogAPI\\data\\Catalog.db";
string dbPath;
if (createDb) {
    dbPath = Path.Combine(dataPath, $"Catalog-{DateTime.Now.ToString("MM-dd-HH-mm-ss")}.db");
} else {
    dbPath = new DirectoryInfo(dataPath).GetFiles().OrderByDescending(f => f.LastWriteTime).First().FullName;
}

Dictionary<byte, string> exchanges = [];
exchanges.Add(1, Path.Combine(sc4pacCachePath, "https\\community.simtropolis.com"));
exchanges.Add(2, Path.Combine(sc4pacCachePath, "https\\www.sc4evermore.com"));
exchanges.Add(3, Path.Combine(sc4pacCachePath, "https\\www.toutsimcities.com"));
exchanges.Add(4, Path.Combine(sc4pacCachePath, "http\\hide-inoki.com"));

string basefolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "sc4pac-channels");
Dictionary<string, ChannelPaths> channels = [];
channels.Add("default", new ChannelPaths("C:\\source\\repos\\sc4pac\\src\\yaml", Path.Combine(basefolder, "default-channel\\json"), Path.Combine(dataPath, "default-data.json")));
channels.Add("simtropolis", new ChannelPaths("C:\\source\\repos\\simtropolis-channel\\src\\yaml", Path.Combine(basefolder, "simtropolis-channel\\json"), Path.Combine(dataPath, "stex-data.json")));
channels.Add("sc4evermore", new ChannelPaths("C:\\source\\repos\\sc4e-channel\\src\\yaml", Path.Combine(basefolder, "sc4evermore-channel\\json"), Path.Combine(dataPath, "sc4e-data.json")));
// ===================================================================================================================================================


FileMgt.ExtractAndMoveFiles(sc4pacCachePath, extractLocation);
Sc4pacChannel.BuildChannels(channels, buildOpt);
(var packages, var assets) = Sc4pacChannel.ParseChannelJson(channels, parseOpt);


DatabaseBuilder db = new DatabaseBuilder(dbPath, createDb);
db.BuildTGITable(extractLocation, assets);
db.FillAssetTable(assets);
//db.BuildPackageTable(packages);
db.FillPackageTable(packages);
File.Copy(dbPath, apiPath, true);
