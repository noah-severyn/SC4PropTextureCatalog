
let apiUrl = '';
const prod = false;
if (prod) {
	apiUrl = 'https://sc4proptexturecatalog-production.up.railway.app';
} else {
	apiUrl = 'http://localhost:4000';
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
		{ name: 'Home', link: '/SC4PropTextureCatalog4/Index.html' },
		{ name: 'View a Pack', link: '/SC4PropTextureCatalog4/ViewPack.html' },
		{ name: 'About', link: '/SC4PropTextureCatalog4/About.html' }
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
