filterSelection("all");
document.getElementById("SearchTableByName").value = ""; 

// ===================================================================================================================
// Name: 		filterSelection
// Desc: 		Filters all elements with the 'column' class name
// Params: 		c ... string to filter by
// Called by: 	button onclick
// Comments: 	(1) code from: https://www.w3schools.com/howto/howto_js_portfolio_filter.asp
//				(2) also using: https://www.w3schools.com/howto/howto_js_active_element.asp
// ===================================================================================================================	
function filterSelection(c) {
	var x, idx;
	x = document.getElementsByClassName("column");
	updateLevel2List(c)
	if (c == "all") c = "";
	for (idx = 0; idx < x.length; idx++) {
		w3RemoveClass(x[idx], "show");
		if (x[idx].className.indexOf(c) > -1) {
			w3AddClass(x[idx], "show");
		}
	}
}

// ===================================================================================================================
// Name: 		w3AddClass
// Desc: 		Adds the specified class to all specified elements
// Params: 		element ... element to add class to
//				name ... name of class to add
// Called by: 	filterSelection()
// Comments: 	N/A
// ===================================================================================================================
function w3AddClass(element, name) {
	var idx, elementArray, nameArray;
	elementArray = element.className.split(" ");
	nameArray = name.split(" ");
	for (idx = 0; idx < nameArray.length; idx++) {
		if (elementArray.indexOf(nameArray[idx]) == -1) {
			element.className += " " + nameArray[idx];
		}
	}
}

function w3RemoveClass(element, name) {
	var i, arr1, arr2;
	arr1 = element.className.split(" ");
	arr2 = name.split(" ");
	for (i = 0; i < arr2.length; i++) {
		while (arr1.indexOf(arr2[i]) > -1) {
			arr1.splice(arr1.indexOf(arr2[i]), 1);  
		}
	}
	element.className = arr1.join(" ");
}

// Loop through primary button container and add the active class to the current/clicked button
var btnContainer = document.getElementById("btnContainer");
var btns = btnContainer.getElementsByClassName("primary");
for (var idx = 0; idx < btns.length; idx++) {
	btns[idx].addEventListener("click", function(){
		var current = document.getElementsByClassName("active");
		current[0].className = current[0].className.replace(" active", "");
		this.className += " active";}
	);
}
// Loop through secondary button container and add the active class to the current/clicked button
let btnContainers = ['_TexturesContainer','_FloraContainer','_VehiclesContainer','_BuildingsContainer','_SceneryContainer']
btnContainers.forEach((containerName) => {
	var btnContainer = document.getElementById(containerName);
	var btns = btnContainer.getElementsByClassName("secondary");
	for (var idx = 0; idx < btns.length; idx++) {
		btns[idx].addEventListener("click", function(){
			var current = document.getElementsByClassName("active2");
			current[0].className = current[0].className.replace(" active2", "");
			this.className += " active2";}
		);
	};
});

//Show/hide level 2 filters as appropriate
document.getElementById("_TexturesContainer").style.display = "none";
document.getElementById("_BuildingsContainer").style.display = "none";
document.getElementById("_FloraContainer").style.display = "none";
document.getElementById("_VehiclesContainer").style.display = "none";
document.getElementById("_SceneryContainer").style.display = "none";
function updateLevel2List(lbl) {
	document.getElementById("_TexturesContainer").style.display = "none";
	document.getElementById("_FloraContainer").style.display = "none";
	document.getElementById("_VehiclesContainer").style.display = "none";
	document.getElementById("_BuildingsContainer").style.display = "none";
	document.getElementById("_SceneryContainer").style.display = "none";
	//debugger;
	switch(lbl) {
		case "Textures":
			document.getElementById("_TexturesContainer").style.display = "block";
			document.getElementById("_FloraContainer").style.display = "none";
			document.getElementById("_VehiclesContainer").style.display = "none";
			document.getElementById("_BuildingsContainer").style.display = "none";
			document.getElementById("_SceneryContainer").style.display = "none";
			break;
		case "_Base":
			document.getElementById("_TexturesContainer").style.display = "block";
			break;
		case "_Overlay":
			document.getElementById("_TexturesContainer").style.display = "block";
			break;
		case "Vehicles":
			document.getElementById("_TexturesContainer").style.display = "none";
			document.getElementById("_FloraContainer").style.display = "none";
			document.getElementById("_VehiclesContainer").style.display = "block";
			document.getElementById("_BuildingsContainer").style.display = "none";
			document.getElementById("_SceneryContainer").style.display = "none";
			break;
		case "_Aircraft":
			document.getElementById("_VehiclesContainer").style.display = "block";
			break;
		case "_Boats":
			document.getElementById("_VehiclesContainer").style.display = "block";
			break;
		case "_Busses":
			document.getElementById("_VehiclesContainer").style.display = "block";
			break;
		case "_Cars":
			document.getElementById("_VehiclesContainer").style.display = "block";
			break;
		case "_Motorcycles":
			document.getElementById("_VehiclesContainer").style.display = "block";
			break;
		case "_TractorTrailer":
			document.getElementById("_VehiclesContainer").style.display = "block";
			break;
		case "_Trains":
			document.getElementById("_VehiclesContainer").style.display = "block";
			break;
		case "_Trucks":
			document.getElementById("_VehiclesContainer").style.display = "block";
			break;
		case "Flora":
			document.getElementById("_TexturesContainer").style.display = "none";
			document.getElementById("_FloraContainer").style.display = "block";
			document.getElementById("_VehiclesContainer").style.display = "none";
			document.getElementById("_BuildingsContainer").style.display = "none";
			document.getElementById("_SceneryContainer").style.display = "none";
			break;
		case "_Agricultural":
			document.getElementById("_FloraContainer").style.display = "block";
			break;
		case "_Bushes":
			document.getElementById("_FloraContainer").style.display = "block";
			break;
		case "_Flowers":
			document.getElementById("_FloraContainer").style.display = "block";
			break;
		case "_Grasses":
			document.getElementById("_FloraContainer").style.display = "block";
			break;
		case "_Trees":
			document.getElementById("_FloraContainer").style.display = "block";
			break;
		case "_Seasonal":
			document.getElementById("_FloraContainer").style.display = "block";
			break;
		case "Buildings":
			document.getElementById("_TexturesContainer").style.display = "none";
			document.getElementById("_FloraContainer").style.display = "none";
			document.getElementById("_VehiclesContainer").style.display = "none";
			document.getElementById("_BuildingsContainer").style.display = "block";
			document.getElementById("_SceneryContainer").style.display = "none";
			break;
		case "_Res":
			document.getElementById("_BuildingsContainer").style.display = "block";
			break;
		case "_Com":
			document.getElementById("_BuildingsContainer").style.display = "block";
			break;
		case "_Ind":
			document.getElementById("_BuildingsContainer").style.display = "block";
			break;
		case "_Civic":
			document.getElementById("_BuildingsContainer").style.display = "block";
			break;
		case "_Low":
			document.getElementById("_BuildingsContainer").style.display = "block";
			break;
		case "_Med":
			document.getElementById("_BuildingsContainer").style.display = "block";
			break;
		case "_High":
			document.getElementById("_BuildingsContainer").style.display = "block";
			break;
		case "_W2W":
			document.getElementById("_BuildingsContainer").style.display = "block";
			break;
		case "Scenery":
			document.getElementById("_TexturesContainer").style.display = "none";
			document.getElementById("_FloraContainer").style.display = "none";
			document.getElementById("_VehiclesContainer").style.display = "none";
			document.getElementById("_BuildingsContainer").style.display = "none";
			document.getElementById("_SceneryContainer").style.display = "block";
			break;
		case "_Industrial":
			document.getElementById("_SceneryContainer").style.display = "block";
			break;
		case "_Recreational":
			document.getElementById("_SceneryContainer").style.display = "block";
			break;
		case "_SmallStructures":
			document.getElementById("_SceneryContainer").style.display = "block";
			break;
		case "_Signs":
			document.getElementById("_SceneryContainer").style.display = "block";
			break;
		case "_WallsFences":
			document.getElementById("_SceneryContainer").style.display = "block";
			break;
		case "_Streetside":
			document.getElementById("_SceneryContainer").style.display = "block";
			break;
	}
}



// ===================================================================================================================
// Name: 		FilterTableName
// Desc: 		Hides all tr rows of the table that do not have a match to SearchTableByName text box
// Params: 		N/A
// Called by: 	onkeyup SearchTableByName
// Comments: 	(1) code from: https://www.w3schools.com/howto/howto_js_filter_table.asp
// ===================================================================================================================
function FilterTableName() {
	var searchBox, searchString, table, tr, td, cell;
	searchBox = document.getElementById("SearchTableByName");
	searchString = searchBox.value.toUpperCase();
	if (searchString.length < 3) { //Don't search if fewer than 3 characters
		return;
	}
	table = document.getElementById("ContentTable");
	tr = table.getElementsByTagName("tr");
	for (var rowidx = 1; rowidx < tr.length; rowidx++) { //Traverse table rows
		tr[rowidx].style.display = "none";
		td = tr[rowidx].getElementsByTagName("td");
		for (var colidx = 0; colidx < td.length; colidx++) { //Traverse table columns
			cell = tr[rowidx].getElementsByTagName("td")[colidx];
			if (cell) {
				if (cell.innerHTML.toUpperCase().indexOf(searchString) > -1) {
					tr[rowidx].style.display = "";
					break;
				}
			}
		}
	}
}



// ===================================================================================================================
// Name: 		showTab
// Desc: 		Hides/shows elements with class="tabcontent" based on button press
// Params: 		evt ... event
//				tabName ... div id to show
// Called by: 	onclick button class="tablinks"
// Comments: 	(1) code from: https://www.w3schools.com/howto/howto_js_tabs.asp
// ===================================================================================================================
function showTab(evt, tabName) {
	var i, tabcontent, tablinks;
	// Get all elements with class="tabcontent" and hide them
	tabcontent = document.getElementsByClassName("tabcontent");
	for (i = 0; i < tabcontent.length; i++) {
		tabcontent[i].style.display = "none";
	}
	// Get all elements with class="tablinks" and remove the class "activeTab"
	tablinks = document.getElementsByClassName("tablinks");
	for (i = 0; i < tablinks.length; i++) {
		tablinks[i].className = tablinks[i].className.replace(" activeTab", "");
	}
	// Show the current tab, and add an "activeTab" class to the button that opened the tab
	document.getElementById(tabName).style.display = "block";
	evt.currentTarget.className += " activeTab";
}


// ===================================================================================================================
// Name: 		checkIfChecked
// Desc: 		Hides/shows elements with class="tabcontent" based on button press
// Params: 		N/A
// Called by: 	onclick button class="tablinks"
// Comments: 	(1) code from: https://stackoverflow.com/a/9887439
// ===================================================================================================================
function checkIfChecked() {
	if (document.getElementById('HDproptoggle').checked) {
		alert("checked");
	} else {
		alert("You didn't check it! Let me check it for you.");
	}
}