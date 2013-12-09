/// <reference path="jquery-1.9.1.js" />
/// <reference path="jquery.validate.js" />
/// <reference path="jquery.validate.unobtrusive.js" />
/// <reference path="knockout-3.0.0.debug.js" />
/// <reference path="modernizr-2.5.3.js" />

$(document).ready(function () {

    //Style for the minutes bubbles.
    $('.commentArea > div:odd').addClass('bubbledRight');
    $('.commentArea > div:even').addClass('bubbledLeft');

    //Modal dialog form validation
    $("#frmPerson").validate({
        rules: {
            inpFirstName: {
                required: true
            }
        },
        messages: {
            inpFirstName: {
                required : "Required Field"
            }
        }
    });

    //Right click menu.
    var $contextMenu = $("#contextMenu");

    //Person Model
    var viewPerson = function () {

        var self = this;
        self.ID = ko.observable(null);
        self.FirstName = ko.observable();
        self.LastName = ko.observable();
        self.LegisProfile = ko.observable();
        self.WikiProfile = ko.observable();
        self.Photo = ko.observable();
        self.MaintainState = function (copy) {

            $.ajax({
                type: "POST",
                cache: false,
                url: "/api/Person",
                data: {
                    ID: "",
                    FirstName: self.FirstName(),
                    LastName: self.LastName(),
                    Copy: copy,
                    LegisUrl: self.LegisProfile(),
                    PhotoUrl: self.Photo(),
                    WikiUrl: self.WikiProfile()
                },
                dataType: "json"
            })
            .done(function () {
                //alert("success");
            }).fail(function () {
                //alert("error");
            }).always(function () {
                //alert("complete");
            });

            //Save back to the server.
            console.warn('TODO Save');

        }

    }

    //Knockout Page Model
    var viewModel = function () {

        //Properties
        this.Person = new viewPerson();
        //pending
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

            console.info('Right-click Copy:(' + this.CopyText() + ')');

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
            //$('#modCopyTabs a[href="#person"]').tab('show');
            //Show modal
            $('#modCopyTool').modal('show');

        },
        this.selectNGO = function (data, event) {
            var self = this;

            //Hide subcontext menu.
            $contextMenu.css({ display: 'none' });
            //Show tab
            //$('#modCopyTabs a[href="#nongovorg"]').tab('show');
            //Show modal
            $('#modCopyTool').modal('show');

        },
        this.selectNewsletter = function (data, event) {
            var self = this;

            //Hide subcontext menu.
            $contextMenu.css({ display: 'none' });
            //Show tab
            //$('#modCopyTabs a[href="#newsletter"]').tab('show');
            //Show modal
            $('#modCopyTool').modal('show');

        },
        this.saveModal = function (data, event) {

            var self = this;
            var $btn = $(event.target);

            var $personForm = $("#frmPerson");
            $personForm.validate();

            //Enable Save
            $btn.attr("disabled", "");

            //jQuery Validate
            if (!$personForm.valid()) {

                //Disable Save
                $btn.attr("disabled", "disabled");
                this.Person.MaintainState(this.CopyText);
                $btn.removeAttr("disabled");

            }
        }
    };

    ko.applyBindings(new viewModel());

});
