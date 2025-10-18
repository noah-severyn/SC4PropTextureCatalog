let stex_data = [];
const stex_max_id = 37061;
fetch('data/stex-data.json')
  .then(response => {
    if (!response.ok) {
      throw new Error(`HTTP error! Status: ${response.status}`);
    }
    return response.json();
  })
  .then(data => {
    stex_data = data;
    const progress = document.getElementById('stex-progress');
    progress.setAttribute('value', data.length);
    progress.setAttribute('max', stex_max_id);
    document.getElementById('stex-progress-label').innerText = data.length + "/" + stex_max_id + ' (' + (data.length/stex_max_id*100).toLocaleString(undefined, {maximumFractionDigits: 2}) + '%) of uploads are indexed'
  });

// stex_data = stex_data.slice(0, 3000);

const chart = document.getElementById('activity-chart');
const chunkSize = 250;
let rendered = 0;
let fragments = 0;
function renderChunk() {
    const fragment = document.createDocumentFragment();

    for (let i = 1; i < chunkSize && rendered < stex_max_id; i++, rendered++) {
        const square = document.createElement('button');
        const item = stex_data.find((i) => i.id === rendered + 1);
        if (item !== undefined) {
            square.style.backgroundColor = '#507a99';
            square.setAttribute('data-tooltip', item.name);
            square.onclick = "location.href='https://community.simtropolis.com/files/file/" + item.name + "';";
        } 
        else {
            square.setAttribute('data-tooltip', rendered + 1);
        }
        square.className = 'square';
        square.title = `Id ${rendered + 1}`;
        fragment.appendChild(square);
    }

    chart.appendChild(fragment);
    fragments++;
            console.log('rendering ' + fragments)

    if (rendered < stex_max_id) {
        if ('requestIdleCallback' in window) {
            requestIdleCallback(renderChunk);
        } else {
            setTimeout(renderChunk, 0);
        }
    }
}


renderChunk();