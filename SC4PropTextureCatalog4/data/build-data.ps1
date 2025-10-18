# ======================
# Simtropolis STEX Items
# ======================
function Convert-Sc4pacStexCacheToJson {
    param([string]$FolderPath)

    $names = Get-ChildItem -Path $path | Select-Object -ExpandProperty Name
    $json = $names | ForEach-Object {  
        #'{"Id":' + [int]($_ -split '-')[0] + ', "Name":"' + $_ + '"},'
        [PSCustomObject]@{
            id = [int]($_ -split '-')[0]
            name = $_
        }
    }
    $json = $json | ConvertTo-Json
}

$path = "C:\Users\Administrator\AppData\Local\io.github.memo33\sc4pac\cache\coursier\https\community.simtropolis.com\files\file"
Convert-Sc4pacFolderCacheToJson -FolderPath $path
$jsonPath = "C:\source\repos\SC4PropTextureCatalog\SC4PropTextureCatalog4\data\stex-data.json"
Set-Content -Path $jsonPath -Value $json

# ======================
# SC4Evermore SC4E Items
# ======================
function Convert-Sc4pacSc4eCacheToJson {
    param([string]$FolderPath)

    $names = Get-ChildItem -Path $path | Select-Object -ExpandProperty Name
    $json = $names | ForEach-Object {  
        #'{"Id":' + [int]($_ -split '-')[0] + ', "Name":"' + $_ + '"},'
        [PSCustomObject]@{
            id = [int]($_ -split '-')[0]
            name = $_
        }
    }
    $json = $json | ConvertTo-Json
}

$path = "C:\Users\Administrator\AppData\Local\io.github.memo33\sc4pac\cache\coursier\https\www.sc4evermore.com\index.php"
Convert-Sc4pacSc4eCacheToJson -FolderPath $path
$jsonPath = "C:\source\repos\SC4PropTextureCatalog\SC4PropTextureCatalog4\data\sc4e-data.json"
Set-Content -Path $jsonPath -Value $json
