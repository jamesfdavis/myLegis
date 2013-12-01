/// <reference path="jquery-1.9.1.js" />
/// <reference path="jquery.validate.js" />
/// <reference path="jquery.validate.unobtrusive.js" />
/// <reference path="knockout-3.0.0.debug.js" />
/// <reference path="modernizr-2.5.3.js" />

$(document).ready(function () {

    // Handler for .ready() called.
    $('.commentArea > div:odd').addClass('bubbledRight');
    $('.commentArea > div:even').addClass('bubbledLeft');

    //Right click menu.
    var $contextMenu = $("#contextMenu");

    //Knockout Modal
    var viewModel = {
        CopyText: ko.observable(),
        rightClick: function (data, event) {

            var self = this;

            if (window.getSelection) {
                self.CopyText = window.getSelection().toString();
            } else if (document.selection && document.selection.type != "Control") {
                self.CopyText = document.selection.createRange().text;
            }

            console.info('running right click!');

            //Check for text selection
            if (self.CopyText.length != 0) {
                //Show menu
                $contextMenu.css({
                    display: 'block',
                    'z-index': 3,
                    left: event.pageX,
                    top: event.pageY
                });
            } else {
                //Hide menu
                $contextMenu.css({ display: 'none' });
            }
        },
        selectCopy: function (data, event) {
            var self = this;

            //Workaround
            $('#modCopy').html(function () { return self.CopyText });

            //Hide subcontext menu.
            $contextMenu.css({ display: 'none' });

            //Show modal
            $('#myModal').modal('show');

        }
    };

    ko.applyBindings(viewModel);

});
