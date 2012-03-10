$(document).ready(function () {
    $('a.back').click(function () {
        parent.history.back();
        return false;
    });
});