
let apiUrl = '';
let htmlPath = '';
// Automatically pull from the production API if this code is running from the deployed site.
const prod = window.location.hostname.includes("github.io") || window.location.hostname.includes("sc4proptexturecatalog.net");
if (prod) {
	apiUrl = 'https://sc4proptexturecatalog-production.up.railway.app';
	htmlPath = '';
} else {
	apiUrl = 'http://localhost:4000';
	htmlPath = '/SC4PropTextureCatalog4';
}
AddHeader();

/**
 * Add the site header and navigation
 */
function AddHeader() {
	const header = document.createElement('header');
	const nav = document.createElement('nav');
	nav.className = "container";
	const ul = document.createElement('ul');
	
	const title = document.createElement('li');
	title.className = "logo";
	title.textContent = "SC4PropTextureCatalog";
	ul.appendChild(title);
	
	const pages = [
		{ name: 'Home', link: `${htmlPath}/index.html` },
		{ name: 'View a Pack', link: `${htmlPath}/view-pack.html` },
		{ name: 'Plugin Pack Ids', link: `${htmlPath}/plugin-pack-ids.html` },
		{ name: 'API Docs ⮥', link: `${apiUrl}/docs/` },
		{ name: 'About', link: `${htmlPath}/about.html` }
	];
	pages.forEach(page => {
		const li = document.createElement('li');
		const a = document.createElement('a');
		a.href = page.link;
		a.textContent = page.name;
		li.appendChild(a);
		ul.appendChild(li);
	});

	const theme = document.createElement('li');
	const themeLink = document.createElement('a');
	themeLink.href = "#";
	themeLink.id = "theme_switcher";
	theme.appendChild(themeLink);
	ul.appendChild(theme);

	nav.appendChild(ul);
	header.appendChild(nav);
	document.body.insertBefore(header, document.body.firstChild);
}

/**
 * Create a thumbnail image element for the specified TGI and category, with a tooltip showing the TGI and exemplar name.
 * Clicking the thumbnail copies the TGI to the clipboard and shows a "TGI copied!" tooltip.
 * Pico tooltips only work with inline elements, so the img element is wrapped in a p element. Dumb.
 * @param {string} tgi - The TGI of the item.
 * @param {string} category - The category of the item.
 * @returns {HTMLElement} The thumbnail image element.
 */
function CreateThumbnailImage(tgi, category, itemName = null) {
    const p = document.createElement("p");
    p.setAttribute('data-tooltip', tgi + '\n' + (itemName || ''));

    const img = document.createElement("img");
    const bucketFolder = category.toLowerCase();
    const extension = category === 'Textures' ? 'png' : 'jpg';
    img.src = `https://thumbs.sc4proptexturecatalog.net/${bucketFolder}/${tgi?.replaceAll('0x', '').replaceAll(', ', '-').toUpperCase()}.${extension}`;
    img.style.height = '96px';
    img.style.cursor = 'help';
	img.style.border = "1px solid var(--pico-form-element-border-color)";
    img.loading = 'lazy';
    img.classList.add('thumbnail');
    img.addEventListener('click', () => {
        navigator.clipboard.writeText(tgi).then(() => {
            const original = p.getAttribute('data-tooltip');
            p.setAttribute('data-tooltip', 'TGI copied!');
            setTimeout(() => p.setAttribute('data-tooltip', original), 1500);
        });
    });
    p.appendChild(img);
    return p;
}
