// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function ShowHideSizeRange() {
	const bgGroup = document.getElementById("BackgroundGroup");
	const sizeElem = document.getElementById('ThumbnailSize');
	if (document.getElementById("ThumbnailToggle").checked) {
		bgGroup.style.display = "block";
		sizeElem.style.display = "block";
	} else {
		bgGroup.style.display = "none";
		sizeElem.style.display = "none";
	}
}

function ToggleThumbnails() {

}

function ChangeThumbnailSize() {
	const imgs = document.getElementsByTagName('img');
	const height = document.getElementById('ThumbnailSize').value;
	for (var elem of imgs) {
		elem.style.height = height + 'px';
	}
}

function ToggleDarkThumbnailBg() {
	const imgs = document.getElementsByTagName("img");
	if (document.getElementById("BackgroundToggle").checked) {
		for (var elem of imgs) {
			elem.classList.add("DarkThumbnail");
		}
	} else {
		for (var elem of imgs) {
			elem.classList.remove("DarkThumbnail");
		}
	}
}