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

    //Right click menu.
    var $contextMenu = $("#contextMenu");

    //Modal defaults
    $("#frmError").hide();
    $('#frmPerson').hide();

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

    //KO Model - Person
    var viewPerson = function () {

        var self = this;
        self.ID = ko.observable(null);
        self.FirstName = ko.observable();
        self.LastName = ko.observable();
        self.LegisProfile = ko.observable();
        self.WikiProfile = ko.observable();
        self.Photo = ko.observable();
        self.MaintainState = function (copy) {

            var p = {
                ID: 0,
                FirstName: self.FirstName(),
                LastName: self.LastName(),
                Copy: copy,
                LegisUrl: self.LegisProfile(),
                PhotoUrl: self.Photo(),
                WikiUrl: self.WikiProfile()
            };

            $.ajax({
                type: "POST",
                cache: false,
                url: "/API/Person",
                data: p,
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

    //KO Model - Page
    var viewModel = function () {

        //ATP spending person.
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

            //Reset form
            self.Person.FirstName('');
            self.Person.LastName('');
            self.Person.WikiProfile('');
            self.Person.LegisProfile('');
            self.Person.Photo('');

            $('.form-group')
                .children('div', '.has-error')
                .removeClass('has-error');
            $('#frmPerson').show();
            $('#frmError').hide();

            //Show tab
            //$('#modCopyTabs a[href="#person"]').tab('show');

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
            $btn.removeAttr("disabled");

            //jQuery Validate
            if ($personForm.valid()) {
                //Disable Save
                $btn.attr("disabled", "disabled");
                //Try to save.
                this.Person.MaintainState(this.CopyText);
                //Re-enable Save
                $btn.removeAttr("disabled");
            }
        }

        //Content Filter
        this.swapPeople = function () {

            var phrArr = null;
            $.ajax({
                type: "GET",
                cache: false,
                url: "/API/Phrase",
                dataType: "json"
            })
            .done(function (data) {
                console.log(data);
                phrArr = data;

                $("p").each(function () {
                    $(this).html(function () {

                        var content = $(this).html();
                        for (var i in phrArr)
                            content = content.replace(phrArr[i].Copy,
                                        "<a href='#' data='{id: " + phrArr[i].$id + " }'>" + phrArr[i].Copy + "</a>");

                        return content;

                    });
                });

            }).fail(function () {
                //alert("error");
            }).always(function () {
                //alert("complete");
            });


        }

        this.swapPeople();

    };

    ko.applyBindings(new viewModel());

});
