document.addEventListener("DOMContentLoaded", function () {
  FetchTableStats();
});

async function FetchTableStats() {
    await fetch(apiUrl + '/api/dbstats')
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! Status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            document.getElementById('PackageCnt').textContent = data[0].Packages;
            document.getElementById('AssetCnt').textContent = data[0].Assets;
            document.getElementById('FileCnt').textContent = data[0].Files;
            document.getElementById('TgiCnt').textContent = data[0].TGIs;
            document.getElementById('TextureCnt').textContent = data[0].Textures;
            document.getElementById('PropCnt').textContent = data[0].Props;
            document.getElementById('FloraCnt').textContent = data[0].Flora;
            document.getElementById('BuildingCnt').textContent = data[0].Buildings;

            var total = data[0].Textures + data[0].Props + data[0].Flora + data[0].Buildings;
            var thumbnails = 0;
            document.getElementById('ThumbnailCoveragePct').textContent = `${thumbnails / total * 100}% (${thumbnails}/${total})`;
            document.getElementById('ThumbnailCoverage').value = thumbnails / total;
        });
}