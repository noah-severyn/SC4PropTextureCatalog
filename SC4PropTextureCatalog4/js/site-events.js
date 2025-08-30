document.getElementById('SearchBox').addEventListener('click', () => {
	Search(document.getElementById('SearchBox').value);
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
