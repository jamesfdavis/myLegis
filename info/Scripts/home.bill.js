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

    //Generic function for handling all Modal errors.
    function serverErrors(data) {
        var message = 'Error on server-side!';
        $("#frmError").html(message + ' (' + data.statusText + ' - code: ' + data.status + ')');
        $("#frmError").show();
    }

    //KO Model - Watch
    var viewWatch = function () {
        var self = this;

        //Basic properties.
        self.Watched = ko.observable(false);
        self.Bill = ko.observable('');

        self.Update = function () {
            if (self.Watched() == false) {
                self.Watched(true);
            } else { self.Watched(false); }
        }
    }

    //KO Model - Person
    var viewPerson = function () {

        var self = this;
        //Basic Properties
        self.ID = ko.observable(0);
        self.Copy = ko.observable('');
        self.Search = ko.observable('');
        self.Leg_Id = ko.observable();
        self.FullName = ko.observable();

        //Save Data
        self.MaintainState = function (data, event) {

            //Grab Selected Copy
            self.Copy($('#personText').val());

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
                self.Save();
                //Re-enable Save
                $btn.removeAttr("disabled");
            }

        }
        self.Save = function () {

            //Try to create the Person
            $.ajax({
                type: "POST",
                cache: false,
                data: {
                    ID: self.ID(),
                    FirstName: self.FirstName(),
                    LastName: self.LastName(),
                    Copy: self.Copy(),
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
                //Show modal
                $('#modCopyTool').modal('hide');

                //TODO - Show person profile dialog.

            }).fail(function (data, status) {
                serverErrors(data);
                //alert("error");
            }).always(function () {
                //alert("complete");
            });

        }
    }

    //KO Model - Page
    var viewModel = function () {

        //OpenStates.org Key
        var OSKey = '0b6a5cec96214c22b41cd8c4ba605d85';

        //Homeosapien and watchfulness.
        this.Person = new viewPerson();
        this.Watch = new viewWatch();

        //pending development
        this.NonGovOrg = ko.observable(),
        this.CopyText = ko.observable(),

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
        this.selectPerson = function (data, event) {
            var self = this;

            //Hide subcontext menu.
            $contextMenu.css({ display: 'none' });

            //Reset Person data
            self.Person.FullName('');
            self.Person.Search('');
            self.Person.Leg_Id('');



            //    var $records = $('#json-records'),
            //    myRecords = JSON.parse($records.text());

            //            $('#my-final-table').dynatable({
            //                dataset: {
            //                    records: myRecords
            //                }
            //            });

            //Selected Copy value to all the sub-objects!
            //self.Person.Copy(self.CopyText());

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
                        $('#inpSearch').focus();
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

        this.searchLegislators = function (data, event) {

            var self = this;

            console.log('Searching for: ' + self.Search());

            //OpenStates.org API - Legislators by last name.
            var url = 'http://openstates.org/api/v1//legislators/?state=ak&active=true&last_name=' + self.Search() + '&apikey=' + OSKey;

            // Function that renders the list items from our records
            function ulWriter(rowIndex, record, columns, cellWriter) {

                var legis = '';

                legis += '<div class="col-md-4">';
                legis += '    <div>';
                legis += '        <div>';
                legis += '            <img src="' + record.photo_url + '" alt="Legislator"  class="img-thumbnail" />';
                legis += '        </div>';
                legis += '    </div>';
                legis += '</div>';
                legis += '<div class="col-md-8">';
                legis += '    <div class="caption">';
                legis += '        <h3 style="margin-top : 3px;">';
                legis += '            ' + record.full_name + '</h3>';
                legis += '        <p>';
                legis += '            Party: ' + record.party + '</p>';
                legis += '        <p>';
                legis += '            District: ' + record.district + '</p>';
                legis += '        <p>';
                legis += '            Level: ' + record.level + '</p>';
                legis += '        <p>';
                legis += '            <a href="' + record.url + '" target="_blank" class="btn btn-default">';
                legis += '                Public Profile</a>';
                legis += '        </p>';
                legis += '    </div>';
                legis += '</div>';

                return legis;

            }

            // Function that creates our records from the DOM when the page is loaded
            function ulReader(index, li, record) {
                var $li = $(li),
                $caption = $li.find('.caption');
                record.thumbnail = $li.find('.thumbnail-image').html();
                record.caption = $caption.html();
                record.label = $caption.find('h3').text();
                record.description = $caption.find('p').text();
                record.color = $li.data('color');
            }

            var $dynatable = $('#my-legislator-results').dynatable({
                dataset: {
                    perPageDefault: 1,
                    page: 0,
                    records: null
                },
                table: {
                    bodyRowSelector: 'div'
                },
                features: {
                    search: false,
                    perPageSelect: false,
                    recordCount: false
                },
                writers: {
                    _rowWriter: ulWriter
                },
                readers: {
                    _rowReader: ulReader
                }
            }).data('dynatable');



            $.get(url, null,
            function (data) {

                console.log(data);

                //Update table
                $dynatable.settings.dataset.originalRecords = data;
                $dynatable.settings.dataset.page = 1;
                $dynatable.process();

                if ($dynatable.settings.dataset.queryRecordCount == 0) {
                    $dynatable.hide();
                } else {
                    $dynatable.show();
                }

            }, 'jsonp');

        },
        //Phrase Methods
        this.anchorPhrases = function () {

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
                        console.log(data);
                        for (var i in data)
                            content = content.replace(data[i].Copy,
                                        "<a class='phrase' href='#' data='{id: " + data[i].ID + " }'>" + data[i].Copy + "</a>");

                        return content;

                    });
                });

                //Wire up new Anchor tags
                self.anchorClick();

            }).fail(function () {
                //alert("error");
            }).always(function () {
                //alert("complete");
            });

        },
        this.anchorClick = function () {

            var self = this;
            //Click event for Phrase links.
            $('a.phrase').on('click', function (event) {
                event.preventDefault();

                var $data = $(this).metadata();
                //Fire up modal window.
                self.loadPhrase($data.id);

            });

        },

        //Load Phrase Modal window.
        this.loadPhrase = function (id) {

            console.log('Loading phrase id: ' + id);

            $.ajax({
                type: "GET",
                cache: false,
                url: "/API/Phrase/" + id,
                dataType: "json"
            }).done(function (data) {
                //alert(data);
                console.log(data);

            }).fail(function () {
                //alert("error");
            }).always(function () {
                //alert("complete");
            });
            //Show Modal window


            //Get the dataset from the server

            //Map KO Model to Appropriate View (Person, NGO)

        }

        this.anchorPhrases();

    };

    ko.applyBindings(new viewModel());

});
