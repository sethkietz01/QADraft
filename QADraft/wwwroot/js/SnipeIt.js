// Auto set the date picker to 2 weeks from the current date
document.addEventListener("DOMContentLoaded", (event) => {
    console.log("DOM fully loaded and parsed");
    const today = new Date();
    var due_date = new Date(); // Get 2 weeks from current date
    due_date.setDate(today.getDate() + 2 * 7)
    const date_picker = document.getElementById("due-date");
    date_picker.valueAsDate = due_date;
});

function changeTab(event, tab_name) {
    var i, tabcontent, tablinks;
    tabcontent = document.getElementsByClassName("tabcontent");
    for (i = 0; i < tabcontent.length; i++) {
        tabcontent[i].style.display = "none";
    }

    tablinks = document.getElementsByClassName("tablinks");
    for (i = 0; i < tablinks.length; i++) {
        tablinks[i].className = tablinks[i].className.replace(" active", "");
    }

    document.getElementById(tab_name).style.display = "block";
    event.currentTarget.className += " active";
}