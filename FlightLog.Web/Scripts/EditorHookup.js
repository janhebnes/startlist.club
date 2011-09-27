/// <reference path="jquery-1.4.4.js" />
/// <reference path="jquery-ui.js" />

$(document).ready(function () {
	function getDateYymmdd(value) {
		if (value == null)
			return null;
		return $.datepicker.parseDate("dd-mm-yy", value);
	}
	$('.date').each(function () {
		var minDate = getDateYymmdd($(this).data("val-rangedate-min"));
		var maxDate = getDateYymmdd($(this).data("val-rangedate-max"));
		$(this).datepicker({
			dateFormat: "dd-mm-yy",  // hard-coding dk date format, but could embed this as an attribute server-side (based on the current culture)
			minDate: minDate,
			maxDate: maxDate
		});
	});
});