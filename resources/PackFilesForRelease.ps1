$compress = @{
Path = "C:\Users\Administrator\OneDrive\SC4 Deps\SC4PropTextureCatalog\images","C:\Users\Administrator\OneDrive\SC4 Deps\SC4PropTextureCatalog\_searchicon.png","C:\Users\Administrator\OneDrive\SC4 Deps\SC4PropTextureCatalog\catalog.css","C:\Users\Administrator\OneDrive\SC4 Deps\SC4PropTextureCatalog\catalog.js","C:\Users\Administrator\OneDrive\SC4 Deps\SC4PropTextureCatalog\changelog.html","C:\Users\Administrator\OneDrive\SC4 Deps\SC4PropTextureCatalog\Simcity4PropTextureCatalog.html"
DestinationPath = "C:\Users\Administrator\OneDrive\SC4 Deps\SC4PropTextureCatalog\Simcity4PropTextureCatalog.zip"
CompressionLevel = "Fastest"
}
Compress-Archive @compress -Force -Verbose