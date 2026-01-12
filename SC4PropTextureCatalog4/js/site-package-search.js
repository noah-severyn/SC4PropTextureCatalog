

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
            keys: ["PackageId"],
            cache: true,
        },
        resultItem: {
            highlight: true
        },
        events: {
            input: {
                selection: (event) => {
                    const selection = event.detail.selection.value;
                    autoCompleteJS.input.value = selection.PackageId;
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



function FetchPackageTGIs(searchText) {
    let query_results = [];
    const query_text = apiUrl + '/api/package?term=' + encodeURIComponent(searchText) + '?field=';
    fetch(query_text)
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! Status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            query_results = data;
            QueryReturn2(searchText, query_results);
        });
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

function QueryReturn2(search_text, query_results) {
    let pkg = AllPackages.filter(p => p.PackageId === search_text)[0];
    document.getElementById('SelectedPackId').textContent = pkg.PackageId;

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

    // const body = document.getElementById('QueryResultBody');
    // query_results.forEach(item => {
    // 	const tr = document.createElement("tr");

    // 	const package = document.createElement("td");
    // 	package.textContent = "";

    // 	const file = document.createElement("td");
    // 	file.textContent = item.File;

    // 	const tgi = document.createElement("td");
    // 	tgi.textContent = item.TGI;

    // 	const category = document.createElement("td");
    // 	category.textContent = item.Category;

    // 	const author = document.createElement("td");
    // 	author.textContent = "";

    // 	//const thumb = document.createElement('td');
    // 	//const img = document.createElement('img');
    // 	//img.src = "img/7AB50E44-0986135E-1DA4A000.png";
    // 	//img.style.height = document.getElementById('ThumbnailSize').value;
    // 	//thumb.appendChild(img);

    // 	const name = document.createElement("td");
    // 	name.textContent = item.Name;

    // 	tr.append(package, file, tgi, category, author, name);
    // 	body.appendChild(tr);
    // });

    // UpdateFilterStates();
}
