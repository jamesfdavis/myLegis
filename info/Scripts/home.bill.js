/// <reference path="jquery-1.9.1.js" />
/// <reference path="jquery.validate.js" />
/// <reference path="jquery.validate.unobtrusive.js" />
/// <reference path="knockout-3.0.0.debug.js" />
/// <reference path="modernizr-2.5.3.js" />
/// <reference path="regex.js" />

$(document).ready(function () {

    //Style for the minutes bubbles.
    $('.commentArea > div:odd').addClass('bubbledRight');
    $('.commentArea > div:even').addClass('bubbledLeft');

    //Setup MetaData defaults.
    $.metadata.setType("attr", "data");

    //Right click menu.
    var $contextMenu = $("#contextMenu");

    //Modal defaults
    $("#frmError").hide();
    //Reset forms.
    $('#modCopyTabs a').click(function (e) {
        e.preventDefault();

        //Reset error message.
        $("#frmError").hide();
        //Reset control validation.
        $('.form-group')
                .children('div', '.has-error')
                .removeClass('has-error');
        $('#frmError').hide();

    })

    //Person Form form Validation
    var $valPerson = $("#frmPerson").validate({
        rules: {
            inpFirstName: {
                required: true
            },
            inpLastName: {
                required: true
            },
            inpLegisProfile: {
                url: true,
                required: false
            },
            inpLegisProfile: {
                url: true,
                required: false
            },
            inpPhoto: {
                url: true,
                required: false
            },
            inpWikiProfile: {
                url: true,
                required: false
            }
        },
        errorClass: 'has-error',
        invalidHandler: formErrors,
        errorPlacement: function () {
            return false;
        },
        highlight: function (element, errorClass, validClass) {
            $(element).parent().addClass(errorClass);
        },
        unhighlight: function (element, errorClass, validClass) {
            $(element).parent().removeClass(errorClass);
        }
    });

    //Generic function for handling all Modal errors.
    function formErrors(event, validator) {
        // 'this' refers to the form
        var errors = validator.numberOfInvalids();
        if (errors) {
            var message = errors == 1
        ? 'You missed 1 field. It has been highlighted'
        : 'You missed ' + errors + ' fields. They have been highlighted';
            $("#frmError").html(message);
            $("#frmError").show();
        } else {
            $("#frmError").hide();
        }
    }

    function serverErrors(data) {
        if (errors) {
            var message = errors == 1
        ? 'You missed 1 field. It has been highlighted'
        : 'You missed ' + errors + ' fields. They have been highlighted';
            $("#frmError").html(message);
            $("#frmError").show();
        } else {
            $("#frmError").hide();
        }
    }

    //KO Model - Person
    var viewPerson = function () {

        var self = this;
        self.ID = ko.observable(0);
        self.FirstName = ko.observable();
        self.LastName = ko.observable();
        self.LegisProfile = ko.observable();
        self.WikiProfile = ko.observable();
        self.Photo = ko.observable();
        self.MaintainState = function (copy) {

            if (self.ID() == 0) {
                //Try to create the Person
                $.ajax({
                    type: "POST",
                    cache: false,
                    data: {
                        ID: self.ID(),
                        FirstName: self.FirstName(),
                        LastName: self.LastName(),
                        Copy: copy,
                        LegisUrl: self.LegisProfile(),
                        PhotoUrl: self.Photo(),
                        WikiUrl: self.WikiProfile()
                    },
                    url: "/API/Person",
                    dataType: "json"
                })
                .done(function (data) {
                    //Set the Person ID
                    self.ID(data.ID);
                }).fail(function () {
                    //alert("error");
                }).always(function () {
                    //alert("complete");
                });
            } else {
                //Try to update the Person.
                console.warn('TODO - Update Person');
            }
        }
    }

    //KO Model - Page
    var viewModel = function () {

        //Homeosapien.
        this.Person = new viewPerson();

        //pending development
        this.NonGovOrg = ko.observable();
        this.CopyText = ko.observable();

        //Basic Methods
        this.rightClick = function (data, event) {

            var self = this;

            if (window.getSelection) {
                self.CopyText(window.getSelection().toString());
            } else if (document.selection && document.selection.type != "Control") {
                self.CopyText(document.selection.createRange().text);
            }

            console.info('Right-click Copy: (' + this.CopyText() + ')');

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
        this.selectSearch = function (data, event) {
            var self = this;

            //Hide subcontext menu.
            $contextMenu.css({ display: 'none' });
            //Show tab
            $('#modCopyTabs a[href="#tabCopyHome"]').tab('show');

            //Show modal
            $('#modCopyTool').modal('show');

        },
        this.selectPerson = function (data, event) {
            var self = this;

            //Hide subcontext menu.
            $contextMenu.css({ display: 'none' });

            //Reset Person data
            self.Person.FirstName('');
            self.Person.LastName('');
            self.Person.WikiProfile('');
            self.Person.LegisProfile('');
            self.Person.Photo('');
            //Reset control validation.
            $('.form-group')
                .children('div', '.has-error')
                .removeClass('has-error');
            $('#frmError').hide();

            //Show tab
            $('#modCopyTabs a[href="#tabPerson"]').tab('show');

            //Show modal, and focus.
            $('#modCopyTool')
                .on('shown.bs.modal',
                    function (e) {
                        $('#inpFirstName').focus();
                    })
                .modal('show');

        },
        this.selectNGO = function (data, event) {
            var self = this;

            //Hide subcontext menu.
            $contextMenu.css({ display: 'none' });
            //Show tab
            $('#modCopyTabs a[href="#tabNGO"]').tab('show');
            //Show modal
            $('#modCopyTool').modal('show');

        },
        this.selectNewsletter = function (data, event) {
            var self = this;

            //Hide subcontext menu.
            $contextMenu.css({ display: 'none' });
            //Show tab
            $('#modCopyTabs a[href="#tabNewsletter"]').tab('show');
            //Show modal
            $('#modCopyTool').modal('show');

        },
        this.savePerson = function (data, event) {

            var self = this;
            var $btn = $(event.target);

            var $personForm = $("#frmPerson");
            $personForm.validate();

            //Enable Save
            $btn.removeAttr("disabled");

            //jQuery Validate
            if ($personForm.valid()) {
                //Disable Save
                $btn.attr("disabled", "disabled");
                //Try to save.
                self.Person.MaintainState(this.CopyText);
                //Re-enable Save
                $btn.removeAttr("disabled");
            }
        }

        //Filter Methods
        this.swapPeople = function () {

            var self = this;

            $.ajax({
                type: "GET",
                cache: false,
                url: "/API/Phrase",
                dataType: "json"
            })
            .done(function (data) {
                $("p").each(function () {
                    $(this).html(function () {
                        //Replace the keywords with links.
                        var content = $(this).html();
                        for (var i in data)
                            content = content.replace(data[i].Copy,
                                        "<a class='phrase' href='#' data='{id: " + data[i].$id + " }'>" + data[i].Copy + "</a>");

                        return content;

                    });
                });

                //Click event for Phrase links.
                $('a.phrase').on('click', function (event) {
                    event.preventDefault();

                    var $data = $(this).metadata();
                    //Fire up modal window.
                    self.loadPhrase($data.id);

                });


            }).fail(function () {
                //alert("error");
            }).always(function () {
                //alert("complete");
            });


        },

        //Load Phrase Modal window.
        this.loadPhrase = function (id) {

            //Show Modal window

            //Get the dataset from the server

            //Map KO Model to Appropriate View (Person, NGO)

        }

        this.swapPeople();

    };

    ko.applyBindings(new viewModel());

});
