

document.getElementById('SearchButton').addEventListener('click', () => {
    QueryData(document.getElementById('InputText').value);
});


async function QueryData(queryText) {
    await fetch('/api/search/' + queryText)
        .then(response => {
            if (!response.ok) {
                throw new Error('Network response was not ok ' + response.statusText);
            }
            return response.json();
        })
        .then(data => {
            const ul = document.getElementById('items');
            ul.textContent = '';
            data.forEach(i => {
                const li = document.createElement('li');
                li.textContent = `${i.file}  ---  ${i.name}`;
                ul.appendChild(li);
            });
        })
        .catch(console.error);
}


