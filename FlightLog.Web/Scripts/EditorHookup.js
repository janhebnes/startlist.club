/// <reference path="jquery-1.4.4.js" />
/// <reference path="jquery-ui.js" />

$(document).ready(function () {
	function getDateYymmdd(value) {
		if (value == null)
			return null;
		return $.datepicker.parseDate("yy-mm-dd", value);
	}
	$('.dateISO').each(function () {
		var minDate = getDateYymmdd($(this).data("val-rangedate-min"));
		var maxDate = getDateYymmdd($(this).data("val-rangedate-max"));
		$(this).datepicker({
			dateFormat: "yy-mm-dd",  // hard-coding dk date format, but could embed this as an attribute server-side (based on the current culture)
			firstDay: 1,
            minDate: minDate,
			maxDate: maxDate,
			dayNamesMin: ['Sø', 'Ma', 'Ti', 'On', 'To', 'Fr', 'Lø']
		});
	});
});