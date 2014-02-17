/// <reference path="jquery-1.9.1.js" />
/// <reference path="jquery.validate.js" />
/// <reference path="jquery.validate.unobtrusive.js" />
/// <reference path="knockout-3.0.0.debug.js" />
/// <reference path="modernizr-2.5.3.js" />
$(document).ready(function () {

    var viewModel = function () {
        var self = this;

        self.List = ko.observableArray([]);
        self.Empty = ko.observable(false);
        self.Loaded = ko.observable(false);
        self.Fetching = ko.observable(false);

        //Get Watch list
        self.GetList = function () {
            var url = '/API/Watch/';
            self.Fetching(true);
            $.get(url, null, function (data) {
          
                console.log(data);
                //Each item in the array
                for (var i = 0; i < data.length; i++) {
                    var row = data[i];
                    row.Title = htmlDecode(row.Title);
                    self.List.push(row);
                }

                if (self.List().length != 0)
                    self.Empty(false);
                else
                    self.Empty(true);

                self.Loaded(true);
                self.Fetching(false);

            }, 'json');
        };

        //Page Load
        self.GetList();
    };

    ko.applyBindings(new viewModel());

    function htmlEncode(value) {
        //create a in-memory div, set it's inner text(which jQuery automatically encodes)
        //then grab the encoded contents back out.  The div never exists on the page.
        return $('<div/>').text(value).html();
    }

    function htmlDecode(value) {
        return $('<div/>').html(value).text();
    }

});
