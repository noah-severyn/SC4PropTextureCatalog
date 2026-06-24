// Setup on page load
document.getElementById('QueryResultSummary').style.display = 'none';
document.getElementById('QueryResultTable').style.display = 'none';


document.getElementById('ThumbnailSize').addEventListener('input', () => {
	const sizeElem = document.getElementById('ThumbnailSize');
	const thumbCol = document.getElementById('QueryResultTable').getElementsByClassName('thumbnail-col');
	if (sizeElem.value == "0") {
		Array.from(thumbCol).forEach(cell => cell.style.display = 'none');
	} else {
		Array.from(thumbCol).forEach(cell => cell.style.display = '');
	}
	const images = document.getElementById('QueryResultBody').getElementsByTagName('img');
	Array.from(images).forEach(img => {
		if (sizeElem.value == "0") {
			img.style.display = 'none';
		} else {
			img.style.display = '';
			img.style.height = sizeElem.value + 'px';
		}
	});
});

document.getElementById('SearchForm').addEventListener('submit', (event) => {
	event.preventDefault();
	const searchText = document.getElementById('SearchBox').value;
	let query_results = [];
	fetch(apiUrl + '/api/search?term=' + encodeURIComponent(searchText))
		.then(response => {
			if (!response.ok) {
				throw new Error(`HTTP error! Status: ${response.status}`);
			}
			return response.json();
		})
		.then(data => {
			query_results = data;
			QueryReturn(searchText, query_results);
			if(document.getElementById('ThumbnailSize').value == "0") {
				document.querySelectorAll('.thumbnail-col').forEach(el => el.style.display = 'none');
			};
		});
	
});

const categories = document.getElementById('Categories').getElementsByTagName('input');
Array.from(categories).forEach(chk => {
	chk.addEventListener('change', () => {
		const category = chk.parentElement.textContent;
		const status = chk.checked;
		let cnt = Filter(category, status);
		document.getElementById('QueryFilterCount').textContent = cnt;
	});
});


/**
 * Update the main table with the query results.
 * @param {string} search_text Query search text
 * @param {Array} query_results Results of the query
 */
function QueryReturn(search_text, query_results) {
	document.getElementById('QueryResultSummary').style.display = 'block';
	document.getElementById('QueryResultTable').style.display = 'block';
	document.getElementById('QuerySearchText').textContent = search_text;
	document.getElementById('QueryResultCount').textContent = query_results.length;
	document.getElementById('QueryFilterCount').textContent = query_results.length;

	const body = document.getElementById('QueryResultBody');
	body.replaceChildren();
	query_results.forEach(item => {
		const tr = document.createElement("tr");

		const package = document.createElement("td");
		const packageLink = document.createElement("a");
		packageLink.href = "sc4pac:///package?pkg=" + encodeURIComponent(item.Package);
		packageLink.textContent = item.Package;
		package.appendChild(packageLink);

		const file = document.createElement("td");
		file.textContent = item.FileName;

		const tgi = document.createElement("td");
		tgi.textContent = item.TGI;

		const category = document.createElement("td");
		category.textContent = item.Category;

		const author = document.createElement("td");
		author.textContent = item.Author;

		const thumb = document.createElement('td');
		thumb.classList.add('thumbnail-col');
		const p = CreateThumbnailImage(item.TGI, document.getElementById('ThumbnailSize').value, item.Category + 's', item.ExemplarName);
		thumb.appendChild(p);

		const name = document.createElement("td");
		name.textContent = item.ExemplarName ?? "null";
		if (item.ExemplarName === null) {
			name.style.fontStyle = "oblique";
		}
		
		tr.append(package, file, tgi, category, author, thumb, name);
		body.appendChild(tr);
	});

	UpdateFilterStates();
}

/**
 * Show or hide rows in the table matching the selected category
 * @param {string} category TGI category to update
 * @param {bool} checked New state for the category. true = visible; false = hidden.
 * @returns {number} Count of rows remaining after the filter
 */
function Filter(category, checked) {
	const rows = document.getElementById('QueryResultBody').childNodes;
	const matchCategory = category.slice(0, category.length - 1)
	rows.forEach(row => {
		const rowCat = row.childNodes[3].textContent;
		if (rowCat === matchCategory || (matchCategory === 'Other' && ['Cohort', 'LTEXT', 'Lua', 'UI'].includes(rowCat))) {
			if (checked) {
				row.removeAttribute('style');
			} else {
				row.style.display = 'none';
			}
		}
	});

	return Array.from(rows).filter(row => !row.hasAttribute('style')).length;
}

function UpdateFilterStates() {
	const categories = document.getElementById('Categories').getElementsByTagName('input');
	Array.from(categories).forEach(chk => {
		const category = chk.parentElement.textContent;
		const status = chk.checked;
		let cnt = Filter(category, status);
		document.getElementById('QueryFilterCount').textContent = cnt;
	});
}