

document.getElementById('PackageForm').addEventListener('submit', () => {
    event.preventDefault();
    FetchPackageTGIs(document.getElementById('PackageSelection').value);
});


/**
 * List of all packages in the database matching the specified package query term, or all packages if the query term is blank.
 */
let AllPackages = [];

Setup();


/**
 * Fetch collection of all packages and populate the dropdown
 */
async function Setup() {
    await FetchPackages('');

    const autoCompleteJS = new autoComplete({
        selector: "#PackageSelection",
        placeHolder: "Search by package name",
        data: {
            src: async () => {
                return AllPackages;
            },
            keys: ["Package"],
            cache: true,
        },
        resultItem: {
            highlight: true
        },
        events: {
            input: {
                selection: (event) => {
                    const selection = event.detail.selection.value;
                    autoCompleteJS.input.value = selection.Package;
                }
            }
        }
    });
};



async function FetchPackages(searchText) {
    await fetch(apiUrl + '/api/package?term=' + encodeURIComponent(searchText))
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! Status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            AllPackages = data;
        });
}



function FetchPackageTGIs(packageName) {
    let query_results = [];
    const query_text = apiUrl + '/api/search?term=' + encodeURIComponent(packageName) + '&field=package';
    fetch(query_text)
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! Status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            query_results = data;
            FillPackageHeader(packageName, query_results);
            document.getElementById('PackageDetails').replaceChildren();
            AddTable('Texture', query_results);
            AddTable('Prop', query_results);
            AddTable('Flora', query_results);
        });

    function FillPackageHeader(search_text) {
        let pkg = AllPackages.filter(p => p.Package === search_text)[0];

        const packageLink = document.createElement("a");
        packageLink.href = "sc4pac:///package?pkg=" + encodeURIComponent(pkg.Package);
        packageLink.textContent = pkg.Package;
        document.getElementById('SelectedPackId').replaceChildren(packageLink);

        document.getElementById('SelectedPackUrls').innerHTML = '';
        pkg.Websites.split(';').forEach(url => {
            const tagDiv = document.createElement('div');
            tagDiv.classList.add('tag');
            const link = document.createElement('a');
            link.href = url;
            link.textContent = GetExchangeAbbreviation(url);
            tagDiv.appendChild(link);
            document.getElementById('SelectedPackUrls').appendChild(tagDiv);
        });

        document.getElementById('SelectedPackVersion').textContent = pkg.Version;
        document.getElementById('SelectedPackAuthor').textContent = pkg.Author;
        document.getElementById('SelectedPackSubfolder').textContent = pkg.Subfolder;
        document.getElementById('SelectedPackTextureCount').textContent = pkg.Textures;
        document.getElementById('SelectedPackPropCount').textContent = pkg.Props;
        document.getElementById('SelectedPackFloraCount').textContent = pkg.Flora;
        document.getElementById('SelectedPackModelCount').textContent = pkg.Buildings;
    }

    function AddTable(category, allData) {
        const detailsArea = document.getElementById('PackageDetails');
        const data = allData.filter(item => item.Category === category);
        if (data.length === 0) {
            return;
        }
        const details = document.createElement('details');
        const summary = document.createElement('summary');
        //const h4 = document.createElement('h4');
        summary.textContent = category + 's (' + data.length + ')';
        //summary.appendChild(h4);
        details.appendChild(summary);

        const table = document.createElement('table');
        data.forEach(item => {
            const tr = document.createElement("tr");
            const tgi = document.createElement("td");
            tgi.textContent = item.TGI;
            const name = document.createElement("td");
            name.textContent = item.ExemplarName;
            tr.appendChild(tgi);
            tr.appendChild(name);
            table.appendChild(tr);
        });
        details.appendChild(table);
        detailsArea.appendChild(details);

            // <!-- <tbody>
            //     @{
            //     if (Model.TextureCount > 0) {
            //     for (int row = 0; row < Math.Ceiling(((double) Model.TextureCount) / 10); row++) { <tr>
            //         @{
            //         for (int col = 0; col < 12; col++) { try { <td>
            //             <img src="~/img/thumbnails/@(Model.TextureRecords[row*10 + col].TGI.Replace(" 0x", "" ).Replace(", ", " -")).png" height="64px" loading="lazy" />
            //             <p>@(Model.TextureRecords[row * 10 + col].TGI.Substring(Model.TextureRecords[row * 10 + col].TGI.Length - 8))</p>
            //             </td>
            //             }
            //             catch (ArgumentOutOfRangeException) { } //Account for last row where there will be < 12 columns } } </tr>
            //                 }
            //                 }
            //                 }
            // </tbody> -->
    }
}

const exchanges = {
    'simtropolis': 'STEX',
    'sc4evermore': 'SC4E',
    'toutsimcities': 'TSC',
    'capitalsimcity': 'CSC',
    'hide-inoki': 'HaS',
    'github': 'GitHub'
};
function GetExchangeAbbreviation(url) {
    for (const key in exchanges) {
        if (url.includes(key)) {
            return exchanges[key];
        }
    }
    return 'Other';
}
