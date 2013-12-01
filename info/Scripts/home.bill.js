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
    var viewModel = function () {

        //Properties
        this.Person = ko.observable();
        this.NonGovOrg = ko.observable();
        this.CopyText = ko.observable();

        //Methods
        this.rightClick = function (data, event) {

            var self = this;

            if (window.getSelection) {
                self.CopyText(window.getSelection().toString());
            } else if (document.selection && document.selection.type != "Control") {
                self.CopyText(document.selection.createRange().text);
            }

            console.info('Right-click Copy for: ' + this.CopyText());

            //Check for text selection
            if (self.CopyText().length != 0) {
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
        this.selectCopy = function (data, event) {
            var self = this;

            //Hide subcontext menu.
            $contextMenu.css({ display: 'none' });
            //Show tab
            $('#modCopyTabs a[href="#home"]').tab('show');
            //Show modal
            $('#modCopyTool').modal('show');

        },
        this.selectPerson = function (data, event) {
            var self = this;

            //Hide subcontext menu.
            $contextMenu.css({ display: 'none' });
            //Show tab
            $('#modCopyTabs a[href="#person"]').tab('show');
            //Show modal
            $('#modCopyTool').modal('show');

        },
        this.selectNGO = function (data, event) {
            var self = this;

            //Hide subcontext menu.
            $contextMenu.css({ display: 'none' });
            //Show tab
            $('#modCopyTabs a[href="#nongovorg"]').tab('show');
            //Show modal
            $('#modCopyTool').modal('show');

        },
        this.selectNewsletter = function (data, event) {
            var self = this;

            //Hide subcontext menu.
            $contextMenu.css({ display: 'none' });
            //Show tab
            $('#modCopyTabs a[href="#newsletter"]').tab('show');
            //Show modal
            $('#modCopyTool').modal('show');

        }
        
    };

    ko.applyBindings(new viewModel());

});
