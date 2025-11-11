
function SendQuery(searchText) {
	let query_results = [];
	fetch('https://sc4proptexturecatalog-production.up.railway.app/api/search?term=$' + encodeURIComponent(searchText))
	//fetch('test/sample-api-data.json')
		.then(response => {
			if (!response.ok) {
				throw new Error(`HTTP error! Status: ${response.status}`);
			}
			return response.json();
		})
		.then(data => {
			query_results = data;
			QueryReturn(searchText, query_results);
		});
}



function QueryReturn(search_text, query_results) {
	document.getElementById('QueryResultSummary').style.display = 'block';
	document.getElementById('QueryResultTable').style.display = 'block';
	document.getElementById('QuerySearchText').textContent = search_text;
	document.getElementById('QueryResultCount').textContent = query_results.length;
	document.getElementById('QueryFilterCount').textContent = query_results.length;

	const body = document.getElementById('QueryResultBody');
	query_results.forEach(item => {
		const tr = document.createElement("tr");

		const package = document.createElement("td");
		package.textContent = "";

		const file = document.createElement("td");
		file.textContent = item.File;

		const tgi = document.createElement("td");
		tgi.textContent = item.TGI;

		const category = document.createElement("td");
		category.textContent = item.Category;

		const author = document.createElement("td");
		author.textContent = "";

		//const thumb = document.createElement('td');
		//const img = document.createElement('img');
		//img.src = "img/7AB50E44-0986135E-1DA4A000.png";
		//img.style.height = document.getElementById('ThumbnailSize').value;
		//thumb.appendChild(img);

		const name = document.createElement("td");
		name.textContent = item.Name;

		tr.append(package, file, tgi, category, author, name);
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