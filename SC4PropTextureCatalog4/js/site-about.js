document.addEventListener("DOMContentLoaded", function () {
  FetchStats();
});

async function FetchStats() {
    let textureTgis = 0;
    let propTgis = 0;
    let floraTgis = 0;
    let buildingTgis = 0;
    await fetch(apiUrl + '/api/dbstats')
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! Status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            textureTgis = data[0].Textures;
            propTgis = data[0].Props;
            floraTgis = data[0].Flora;
            buildingTgis = data[0].Buildings;

            document.getElementById('PackageCnt').textContent = data[0].Packages;
            document.getElementById('AssetCnt').textContent = data[0].Assets;
            document.getElementById('FileCnt').textContent = data[0].Files;
            document.getElementById('TgiCnt').textContent = data[0].TGIs;
            document.getElementById('TextureCnt').textContent = textureTgis;
            document.getElementById('PropCnt').textContent = propTgis;
            document.getElementById('FloraCnt').textContent = floraTgis;
            document.getElementById('BuildingCnt').textContent = buildingTgis;
            document.getElementById('ModelCnt').textContent = data[0].Models;
        });
    
    await fetch(apiUrl + '/api/thumbnailstats')
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! Status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            var textures = data[0].TextureCount;
            var props = data[0].PropCount;
            var flora = data[0].FloraCount;
            var buildings = data[0].BuildingCount;
            const total = textures + props + flora + buildings;

            document.getElementById('TextureProg').value = textures / textureTgis;
            document.getElementById('PropProg').value = props / propTgis;
            document.getElementById('FloraProg').value = flora / floraTgis;
            document.getElementById('BuildingProg').value = buildings / buildingTgis;

            document.getElementById('TexturePct').textContent = GeneratePctLabel(textures, textureTgis);
            document.getElementById('PropPct').textContent = GeneratePctLabel(props, propTgis);
            document.getElementById('FloraPct').textContent = GeneratePctLabel(flora, floraTgis);
            document.getElementById('BuildingPct').textContent = GeneratePctLabel(buildings, buildingTgis);
        });
}

function GeneratePctLabel(count, total)   {
    if (total === 0) {
        return "0% (0/0)";
    }
    return `Thumbnails: ${(count / total * 100).toFixed(2)}% (${count}/${total})`;
}
