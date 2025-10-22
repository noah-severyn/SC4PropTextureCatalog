// Setup on page load
document.getElementById('QueryResultSummary').style.display = 'none';
document.getElementById('QueryResultTable').style.display = 'none';
ToggleThumbnailControlVisibility();



function ToggleThumbnailControlVisibility() {
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
