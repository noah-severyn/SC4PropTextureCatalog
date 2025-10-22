const STEX_MAX_ID = 37061;
const CHUNK_SIZE = 250;
const CHUNK_COUNT = Math.ceil(STEX_MAX_ID / CHUNK_SIZE);

// Create list of STEX ids for non-sc4pac files
let non_sc4pac_data = [];
const fileIds = ["003", "010", "012", "013", "015", "016", "017", "019", "020", "021", "022", "023", "024", "026", "027", "029", "030", "031", "038", "041", "042", "043", 115, 116, 117, 119, 121, 150, 156];
const non_sc4pac_files = fileIds.map(id => `data/exclusions/stex-cat-${id}.json`);
Promise.all(
	non_sc4pac_files.map(file =>
		fetch(file)
			.then(response => {
				if (!response.ok) {
					throw new Error(`HTTP error! Status: ${response.status}`);
				}
				return response.json();
			})
	)
).then(results => {
	results.forEach(data => {
		non_sc4pac_data = non_sc4pac_data.concat(data);
	});
});


let all_data = [];
fetch('data/stex-data.json')
	.then(response => {
		if (!response.ok) {
			throw new Error(`HTTP error! Status: ${response.status}`);
		}
		return response.json();
	})
	.then(data => {
		all_data = all_data.concat(data);
		const progress = document.getElementById('stex-progress');
		const sc4FilesCount = STEX_MAX_ID - non_sc4pac_files.length
		
		progress.setAttribute('value', all_data.length);
		progress.setAttribute('max', sc4FilesCount);
		document.getElementById('stex-progress-label').innerText = all_data.length + "/" + sc4FilesCount + ' (' + (all_data.length / sc4FilesCount * 100).toLocaleString(undefined, { maximumFractionDigits: 2 }) + '%) of uploads are indexed'
	});

fetch('data/default-data.json')
	.then(response => {
		if (!response.ok) {
			throw new Error(`HTTP error! Status: ${response.status}`);
		}
		return response.json();
	})
	.then(data => {
		all_data = all_data.concat(data);
		const progress = document.getElementById('stex-progress');
		progress.setAttribute('value', all_data.length);
		progress.setAttribute('max', STEX_MAX_ID);
		document.getElementById('stex-progress-label').innerText = all_data.length + "/" + (STEX_MAX_ID - non_sc4pac_files.length) + ' (' + (all_data.length / (STEX_MAX_ID - non_sc4pac_files.length) * 100).toLocaleString(undefined, { maximumFractionDigits: 2 }) + '%) of uploads are indexed'
	});



const chart = document.getElementById('activity-chart');

// Create placeholders for N chunks
for (let i = 0; i < CHUNK_COUNT; i++) {
	const placeholder = document.createElement('div');
	placeholder.className = 'chunk-placeholder';
	placeholder.setAttribute('data-tooltip', i);
	placeholder.dataset.index = i;
	placeholder.style.minHeight = '15px';  // visual space so user can scroll
	//placeholder.textContent = i;
	chart.appendChild(placeholder);
}

// Function that builds/appends the real chunk content
function loadChunk(index, placeholder) {
	// Build content in a DocumentFragment to minimize reflows
	const frag = document.createDocumentFragment();

	for (var idx = 1; idx <= CHUNK_SIZE; idx++) {
		const square = document.createElement('button');
		const itemId = CHUNK_SIZE * index + idx;
		const item = all_data.find((i) => i.id === itemId);
		if (non_sc4pac_data.some(i => i.id === itemId)) {
			square.style.backgroundColor = '#fafafa';
			square.classList.add("nohover");
		}
		else if	(item !== undefined) {
			square.style.backgroundColor = '#507a99';
			square.setAttribute('data-tooltip', item.name);
			square.setAttribute('onclick', "window.open('" + item.url + "', '_blank')");
		}
		else {
			square.setAttribute('data-tooltip', itemId);
		}
		square.classList.add("square");
		square.title = itemId;
		frag.appendChild(square);
	}

	// Replace placeholder with actual content
	placeholder.replaceWith(frag);
}

// Observer callback: schedule load when placeholder enters viewport
const onIntersect = (entries, obs) => {
	for (const entry of entries) {
		if (!entry.isIntersecting) continue;

		const placeholder = entry.target;
		const index = Number(placeholder.dataset.index);

		// Stop observing this placeholder
		obs.unobserve(placeholder);

		// Prefer requestIdleCallback to do DOM work when browser is idle
		if ('requestIdleCallback' in window) {
			requestIdleCallback(
				() => loadChunk(index, placeholder),
				{ timeout: 2000 } // fallback to run within 2s if idle never occurs
			);
		} else {
			// Fallback: use setTimeout to yield to the event loop
			setTimeout(() => loadChunk(index, placeholder), 0);
		}
	}
};

// Create the IntersectionObserver
const observer = new IntersectionObserver(onIntersect, {
	root: null,           // viewport
	rootMargin: '200px',  // start loading a bit before it hits the bottom
	threshold: 0.01
});

// Start observing placeholders
document.querySelectorAll('.chunk-placeholder').forEach(el => observer.observe(el));










//const chart = document.getElementById('activity-chart');
//const chunkSize = 250;
//let rendered = 0;
//let fragments = 0;
//function renderChunk() {
//    const fragment = document.createDocumentFragment();

//    for (let i = 1; i < chunkSize && rendered < stex_max_id; i++, rendered++) {
//        const square = document.createElement('button');
//        const item = stex_data.find((i) => i.id === rendered + 1);
//        if (item !== undefined) {
//            square.style.backgroundColor = '#507a99';
//            square.setAttribute('data-tooltip', item.name);
//            square.onclick = "location.href='https://community.simtropolis.com/files/file/" + item.name + "';";
//        }
//        else {
//            square.setAttribute('data-tooltip', rendered + 1);
//        }
//        square.className = 'square';
//        square.title = `Id ${rendered + 1}`;
//        fragment.appendChild(square);
//    }

//    chart.appendChild(fragment);
//    fragments++;
//            console.log('rendering ' + fragments)

//    if (rendered < stex_max_id) {
//        if ('requestIdleCallback' in window) {
//            requestIdleCallback(renderChunk);
//        } else {
//            setTimeout(renderChunk, 0);
//        }
//    }
//}


//renderChunk();