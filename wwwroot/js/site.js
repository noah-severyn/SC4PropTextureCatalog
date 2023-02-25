// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function ShowHideSizeRange() {
	var thumbSizeGroup = document.getElementById("ThumbSizeGroup");
	if (showThumbCheckbox.checked) {
		thumbSizeGroup.style.display = "block";
	} else {
		thumbSizeGroup.style.display = "none";
	}
}

function ToggleDarkThumbnailBg() {
	var imgs = document.getElementsByTagName("img");
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