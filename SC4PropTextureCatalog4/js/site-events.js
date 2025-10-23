document.getElementById('SearchForm').addEventListener('submit', () => {
	event.preventDefault();
	SendQuery(document.getElementById('SearchBox').value);
});

const categories = document.getElementById('Categories').getElementsByTagName('input');
Array.from(categories).forEach(chk => {
	chk.addEventListener('change', () => {
		const category = chk.parentElement.textContent;
		const status = chk.checked;
		let cnt = Filter(category, status);
		document.getElementById('QueryFilterCount').textContent = cnt;
	});
});

document.getElementById('ThumbnailToggle').addEventListener('click', () => {
	ToggleThumbnailControlVisibility();
});
document.getElementById('BackgroundToggle').addEventListener('click', () => {
	ToggleDarkThumbnailBg();
});
document.getElementById('ThumbnailSize').addEventListener('click', () => {
	ChangeThumbnailSize();
});
