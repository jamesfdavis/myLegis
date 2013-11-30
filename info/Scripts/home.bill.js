/// <reference path="jquery-1.9.1.js" />
/// <reference path="jquery.validate.js" />
/// <reference path="jquery.validate.unobtrusive.js" />
/// <reference path="knockout-3.0.0.debug.js" />
/// <reference path="modernizr-2.5.3.js" />

$(document).ready(function () {

    // Handler for .ready() called.
    $('.commentArea > div:odd').addClass('bubbledRight');
    $('.commentArea > div:even').addClass('bubbledLeft');

    $('#myText').click(function (e) {
        e.preventDefault();
        viewModel.myFuntion();
    });

    var $contextMenu = $("#contextMenu");

    //Knockout Modal
    var viewModel = {
        personName: 'Bob',
        selectedValue: 123,
        ContextMenuClick: function (data, event) {

            $contextMenu.css({
                display: 'block',
                'z-index': 3,
                left: event.pageX,
                top: event.pageY
            });

        },
        getHighlight: function () {
            var text = "";
            if (window.getSelection) {
                text = window.getSelection().toString();
            } else if (document.selection && document.selection.type != "Control") {
                text = document.selection.createRange().text;
            }
            return text;
        }

    };

    ko.applyBindings(viewModel);

});
