﻿// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(function () {
    $("#emojibtn").click(function () {
        $("#emojis-container").toggleClass("d-none");
    });
})

$('#show-create-room-modal').on('click', function () {
    $('create-room-modal').modal('show');
});