// Setup on page load
document.getElementById('QueryResultSummary').style.display = 'none';
document.getElementById('QueryResultTable').style.display = 'none';
ToggleThumbnailControlVisibility();



function ToggleThumbnailControlVisibility() {
	const bgGroup = document.getElementById("BackgroundGroup");
	const sizeElem = document.getElementById('ThumbnailSize');
	// if (document.getElementById("ThumbnailToggle").checked) {
	// 	bgGroup.style.display = "block";
	// 	sizeElem.style.display = "block";
	// } else {
	// 	bgGroup.style.display = "none";
	// 	sizeElem.style.display = "none";
	// }
}
