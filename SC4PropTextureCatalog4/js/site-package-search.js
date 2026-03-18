

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
            AddTable('Textures', query_results);
            AddTable('Props', query_results);
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
        const detailsContainer = document.getElementById('PackageDetails');
        const bucketFolder = category.toLowerCase();
        const dbCategory = category.replace(/s$/, '');

        const data = allData.filter(item => item.Category === dbCategory);
        if (data.length === 0) {
            return;
        }
        const details = document.createElement('details');
        const summary = document.createElement('summary');
        summary.textContent = category + ' (' + data.length + ')';
        details.appendChild(summary);

        if (category === 'Textures') {
            const msg = document.createElement('p');
            msg.textContent = "Tip: If a thumbnail is difficult to see, try switching the site theme to light or dark mode.";
            details.appendChild(msg);
        }
        
        const flexDiv = document.createElement('div');
        flexDiv.id = category;
        flexDiv.classList.add('thumbnail-grid');

        data.forEach(item => {
            //Pico tooltips only work with inline elements, so we have to wrap the img in a p. Dumb.
            const p = document.createElement("p");
            p.setAttribute('data-tooltip', item.TGI + '\n' + item.ExemplarName);

            const img = document.createElement("img");
            img.src = `https://thumbs.sc4proptexturecatalog.net/${bucketFolder}/${item.TGI.replaceAll('0x', '').replaceAll(', ', '-').toUpperCase()}.png`;
            img.style.height = '96px';
            img.style.cursor = 'help';
            img.loading = 'lazy';
            img.classList.add('thumbnail');
            img.addEventListener('click', () => {
                navigator.clipboard.writeText(item.TGI).then(() => {
                    const original = p.getAttribute('data-tooltip');
                    p.setAttribute('data-tooltip', 'TGI copied!');
                    setTimeout(() => p.setAttribute('data-tooltip', original), 1500);
                });
            });

            p.appendChild(img);
            flexDiv.appendChild(p);
        });
        details.appendChild(flexDiv);
        detailsContainer.appendChild(details);
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
