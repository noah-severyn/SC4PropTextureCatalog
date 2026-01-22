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
            document.getElementById('PackageCount').textContent = data[0].Packages;
            document.getElementById('AssetCount').textContent = data[0].Assets;
            document.getElementById('FileCount').textContent = data[0].Files;
            document.getElementById('TgiCount').textContent = data[0].TGIs;
            console.log(data);
        });
}