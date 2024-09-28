// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


function displaySelectedImage(event, elementId, btnShow = '') {
    const selectedImage = document.getElementById(elementId);
    const btnUpload = document.getElementById(btnShow);
    const fileInput = event.target;

    if (fileInput.files && fileInput.files[0]) {
        const reader = new FileReader();

        reader.onload = function (e) {
            selectedImage.src = e.target.result;
            btnUpload.classList.remove("d-none");
        };

        reader.readAsDataURL(fileInput.files[0]);
    }
}
