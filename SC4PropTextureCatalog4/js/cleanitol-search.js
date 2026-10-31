// Setup on page load
document.getElementById('QueryResultSummary').style.display = 'none';
document.getElementById('QueryResultTable').style.display = 'none';


document.getElementById('SearchForm').addEventListener('submit', (event) => {
	event.preventDefault();
	const searchText = document.getElementById('SearchBox').value;
	let query_results = [];
	fetch(apiUrl + '/api/cleanitol?term=' + encodeURIComponent(searchText))
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

		const file = document.createElement("td");
		file.textContent = item.RemoveFileName;

		const script = document.createElement("td");
		script.textContent = item.ScriptName;

		const package = document.createElement("td");
		const packageLink = document.createElement("a");
		packageLink.href = "sc4pac:///package?pkg=" + encodeURIComponent(item.Package);
		packageLink.textContent = item.Package;
		package.appendChild(packageLink);

		tr.append(file, script, package);
		body.appendChild(tr);
	});
}
