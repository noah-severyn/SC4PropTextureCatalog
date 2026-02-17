
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
