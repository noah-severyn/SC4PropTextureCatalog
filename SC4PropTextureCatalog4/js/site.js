
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

//Theme switcher logic, based on: https://github.com/RWDevelopment/theme_switch
let isLightTheme = true;
const DEFAULT_THEME_ICON = '<svg viewBox="0 0 16 16"><path fill="currentColor" d="M8 15A7 7 0 1 0 8 1v14zm0 1A8 8 0 1 1 8 0a8 8 0 0 1 0 16z"/></svg>'
const SUN_ICON = '<svg viewBox="0 0 16 16"><path fill="currentColor" d="M8 11a3 3 0 1 1 0-6a3 3 0 0 1 0 6zm0 1a4 4 0 1 0 0-8a4 4 0 0 0 0 8zM8 0a.5.5 0 0 1 .5.5v2a.5.5 0 0 1-1 0v-2A.5.5 0 0 1 8 0zm0 13a.5.5 0 0 1 .5.5v2a.5.5 0 0 1-1 0v-2A.5.5 0 0 1 8 13zm8-5a.5.5 0 0 1-.5.5h-2a.5.5 0 0 1 0-1h2a.5.5 0 0 1 .5.5zM3 8a.5.5 0 0 1-.5.5h-2a.5.5 0 0 1 0-1h2A.5.5 0 0 1 3 8zm10.657-5.657a.5.5 0 0 1 0 .707l-1.414 1.415a.5.5 0 1 1-.707-.708l1.414-1.414a.5.5 0 0 1 .707 0zm-9.193 9.193a.5.5 0 0 1 0 .707L3.05 13.657a.5.5 0 0 1-.707-.707l1.414-1.414a.5.5 0 0 1 .707 0zm9.193 2.121a.5.5 0 0 1-.707 0l-1.414-1.414a.5.5 0 0 1 .707-.707l1.414 1.414a.5.5 0 0 1 0 .707zM4.464 4.465a.5.5 0 0 1-.707 0L2.343 3.05a.5.5 0 1 1 .707-.707l1.414 1.414a.5.5 0 0 1 0 .708z"/></svg>'
const MOON_ICON = '<svg viewBox="0 0 16 16"><g fill="currentColor"><path d="M6 .278a.768.768 0 0 1 .08.858a7.208 7.208 0 0 0-.878 3.46c0 4.021 3.278 7.277 7.318 7.277c.527 0 1.04-.055 1.533-.16a.787.787 0 0 1 .81.316a.733.733 0 0 1-.031.893A8.349 8.349 0 0 1 8.344 16C3.734 16 0 12.286 0 7.71C0 4.266 2.114 1.312 5.124.06A.752.752 0 0 1 6 .278zM4.858 1.311A7.269 7.269 0 0 0 1.025 7.71c0 4.02 3.279 7.276 7.319 7.276a7.316 7.316 0 0 0 5.205-2.162c-.337.042-.68.063-1.029.063c-4.61 0-8.343-3.714-8.343-8.29c0-1.167.242-2.278.681-3.286z"/><path d="M10.794 3.148a.217.217 0 0 1 .412 0l.387 1.162c.173.518.579.924 1.097 1.097l1.162.387a.217.217 0 0 1 0 .412l-1.162.387a1.734 1.734 0 0 0-1.097 1.097l-.387 1.162a.217.217 0 0 1-.412 0l-.387-1.162A1.734 1.734 0 0 0 9.31 6.593l-1.162-.387a.217.217 0 0 1 0-.412l1.162-.387a1.734 1.734 0 0 0 1.097-1.097l.387-1.162zM13.863.099a.145.145 0 0 1 .274 0l.258.774c.115.346.386.617.732.732l.774.258a.145.145 0 0 1 0 .274l-.774.258a1.156 1.156 0 0 0-.732.732l-.258.774a.145.145 0 0 1-.274 0l-.258-.774a1.156 1.156 0 0 0-.732-.732l-.774-.258a.145.145 0 0 1 0-.274l.774-.258c.346-.115.617-.386.732-.732L13.863.1z"/></g></svg>'

document.addEventListener('DOMContentLoaded', () => {
	const savedTheme = localStorage.getItem('prop-texture-catalog-theme');
	if (savedTheme === null) {
		document.getElementById('theme_switcher').innerHTML = DEFAULT_THEME_ICON;
		document.documentElement.setAttribute('data-theme', 'auto');
		document.getElementById('theme_switcher').setAttribute('data-tooltip', 'Auto');
	} else {
		document.documentElement.setAttribute('data-theme', savedTheme);
		document.getElementById('theme_switcher').innerHTML = isLightTheme ? SUN_ICON : MOON_ICON;
		document.getElementById('theme_switcher').setAttribute('data-tooltip', isLightTheme ? 'Light' : 'Dark');
	}
});
document.getElementById('theme_switcher').addEventListener('click', (e) => {
	e.preventDefault();
	isLightTheme = !isLightTheme;
	localStorage.setItem('prop-texture-catalog-theme', isLightTheme ? 'light' : 'dark');
	document.documentElement.setAttribute('data-theme', isLightTheme ? 'light' : 'dark');
	document.getElementById('theme_switcher').innerHTML = isLightTheme ? SUN_ICON : MOON_ICON;
	document.getElementById('theme_switcher').setAttribute('data-tooltip', isLightTheme ? 'Light' : 'Dark');
});


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
		{ name: 'Find Cleanitol Items', link: `${htmlPath}/cleanitol.html` },
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
 * @param {number} thumbnailSize - The size of the thumbnail image in pixels.
 * @param {string} category - The category of the item.
 * @param {string} [itemName] - The name of the item.
 * @returns {HTMLElement} The thumbnail image element.
 */
function CreateThumbnailImage(tgi, thumbnailSize, category, itemName = null) {
	const p = document.createElement("p");
	p.setAttribute('data-tooltip', tgi + '\n' + (itemName || ''));

	const img = document.createElement("img");
	const bucketFolder = category.toLowerCase();
	const extension = category === 'Textures' ? 'png' : 'jpg';
	img.src = `https://thumbs.sc4proptexturecatalog.net/${bucketFolder}/${tgi?.replaceAll('0x', '').replaceAll(', ', '-').toUpperCase()}.${extension}`;
	img.style.height = `${thumbnailSize}px`;
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
