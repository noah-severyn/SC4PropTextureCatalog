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

// document.getElementById('ThumbnailToggle').addEventListener('click', () => {
// 	ToggleThumbnailControlVisibility();
// });

// document.getElementById('BackgroundToggle').addEventListener('click', () => {
// 	const imgs = document.getElementsByTagName("img");
// 	if (document.getElementById("BackgroundToggle").checked) {
// 		for (var elem of imgs) {
// 			elem.classList.add("DarkThumbnail");
// 		}
// 	} else {
// 		for (var elem of imgs) {
// 			elem.classList.remove("DarkThumbnail");
// 		}
// 	}
// });

// const sizes = document.getElementById('ThumbnailSize').getElementsByTagName('input');
// Array.from(sizes).forEach(radio => {
// 	radio.addEventListener('change', () => {
// 		const imgs = document.getElementsByTagName('img');
// 		const height = radio.value;
// 		for (var elem of imgs) {
// 			elem.style.height = height;
// 		}
// 	});
// });
