using System.Text.Json;
using csDBPF;
using SC4PropTextureCatalogBuilder;

bool createDb = PromptYesNo("Create new database?");

// ===================================================================================================================================================
const string sc4pacCachePath = "C:\\Users\\Administrator\\AppData\\Local\\io.github.memo33\\sc4pac\\cache\\coursier";
const string extractLocation = "P:\\sc4pac-cache";
string dataPath = "C:\\source\\repos\\SC4PropTextureCatalog\\SC4PropTextureCatalogBuilder\\data";
string apiPath = "C:\\source\\repos\\SC4PropTextureCatalog\\SC4PropTextureCatalogAPI\\data\\Catalog.db";
string dbPath;
if (createDb) {
    dbPath = Path.Combine(dataPath, $"Catalog-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.db");
} else {
    dbPath = new DirectoryInfo(dataPath).GetFiles().OrderByDescending(f => f.LastWriteTime).First().FullName;
}

string basefolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "sc4pac-channels");
Dictionary<string, ChannelPaths> channels = [];
channels.Add("default", new ChannelPaths("C:\\source\\repos\\sc4pac\\src\\yaml", Path.Combine(basefolder, "default-channel\\json"), Path.Combine(dataPath, "default-data.json")));
channels.Add("simtropolis", new ChannelPaths("C:\\source\\repos\\simtropolis-channel\\src\\yaml", Path.Combine(basefolder, "simtropolis-channel\\json"), Path.Combine(dataPath, "stex-data.json")));
channels.Add("sc4evermore", new ChannelPaths("C:\\source\\repos\\sc4e-channel\\src\\yaml", Path.Combine(basefolder, "sc4evermore-channel\\json"), Path.Combine(dataPath, "sc4e-data.json")));
// ===================================================================================================================================================

if (PromptYesNo("Extract and move files?")) {
    FileMgt.ExtractAndMoveFiles(sc4pacCachePath, extractLocation);
}
if (PromptYesNo("Build channels?")) {
    ChannelOptions buildOpt = PromptChannelOption("Which channel(s) do you want to build? (YAML → JSON)");
    SC4Pac.BuildChannels(channels, buildOpt);
}
ChannelOptions parseOpt = PromptChannelOption("Which channel(s) do you want to parse? (JSON → DB Objects");
(var packages, var assets) = SC4Pac.ParseChannelJson(channels, parseOpt);
//Sc4pacChannel.DumpUrls(packages, assets, channels, parseOpt);

List<DBPFError> errors = [];
DatabaseBuilder db = new DatabaseBuilder(dbPath, createDb);
if (PromptYesNo("Fill TGI table?")) {
    errors = db.FillTgiTable(extractLocation);
    string json = JsonSerializer.Serialize(errors);
    File.WriteAllText(Path.Combine(dataPath, $"Errors-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.json"), json);
}
if (PromptYesNo("Fill Asset table?")) {
    db.FillAssetTable(assets);
}
if (PromptYesNo("Fill Package table?")) {
    db.FillPackageTable(packages);
}
if (PromptYesNo("Copy database to API path?")) {
    File.Copy(dbPath, apiPath, true);
}


static bool PromptYesNo(string message) {
    Console.Write($"{message} (y/n): ");
    var response = Console.ReadLine()?.Trim().ToLower();
    return response == "y" || response == "yes";
}
static ChannelOptions PromptChannelOption(string message) {
    Console.Write($"{message} (0=None, 1=All, 2=Default, 3=ST, 4=SC4E): ");
    var response = Console.ReadLine()?.Trim().ToLower();
    _ = int.TryParse(response, out int value);
    return (ChannelOptions) value;
}