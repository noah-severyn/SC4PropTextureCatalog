
function Search(searchText) {
	let query_results = [];
	fetch('data/sample-api-data.json')
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
	console.log(query_results);

	const body = document.getElementById('QueryResultBody');
	query_results.forEach(item => {
		const tr = document.createElement("tr");

		const package = document.createElement("td");
		package.textContent = "";

		const file = document.createElement("td");
		file.textContent = item.file;

		const tgi = document.createElement("td");
		tgi.textContent = item.tgi;

		const category = document.createElement("td");
		category.textContent = item.category;

		const author = document.createElement("td");
		author.textContent = "";

		const name = document.createElement("td");
		name.textContent = item.name;

		tr.append(package, file, tgi, category, author, name);
		body.appendChild(tr);
	});
}
