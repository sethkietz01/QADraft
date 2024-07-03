// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


// Test for color customization
var userBasedButtonColors = {
    "6": "purple" // the idea is, for user with ID = 6, color is purple
};

// return the color of the user
function getUserColor(int id) {
    return userBasedButtonColors[id];
}

// get all buttons with class .buttons and update the background-color
function changeButtonColors() {
    // Get all elements with the specified class name
    let buttons = document.querySelectorAll(".buttons");

    // test id
    var id = 6;

    // Get the new color
    var newButtonColor = getUserColor(id);

    // Iterate through each button and change its background color
    buttons.forEach(function (button) {
        button.style.backgroundColor = newButtonColor;
    });
}

