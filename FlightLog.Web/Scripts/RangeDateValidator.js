/// <reference path="jquery-1.4.4.js" />
/// <reference path="jquery.validate.js" />
/// <reference path="jquery-ui.js" />
(function ($) {
    // The validator function
    $.validator.addMethod('rangeDate', function (value, element, param) {
        if (!value) {
            return true; // not testing 'is required' here!
        }
        try {
            var dateValue = $.datepicker.parseDate("dd-mm-yy", value); // hard-coding dk date format, but could embed this as an attribute server-side (based on the current culture)
        }
        catch (e) {
            return false;
        }
        return param.min <= dateValue && dateValue <= param.max;
    });

    // The adapter to support ASP.NET MVC unobtrusive validation
    $.validator.unobtrusive.adapters.add('rangedate', ['min', 'max'], function (options) {
        var params = {
            min: $.datepicker.parseDate("dd-mm-yy", options.params.min),
            max: $.datepicker.parseDate("dd-mm-yy", options.params.max)
        };

        options.rules['rangeDate'] = params;
        if (options.message) {
            options.messages['rangeDate'] = options.message;
        }
    });
} (jQuery));