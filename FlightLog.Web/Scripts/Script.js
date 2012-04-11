$(document).ready(function () {
    $('a.back').click(function () {
        parent.history.back();
        return false;
    });

    $('a.editComment').click(function (e) {
        e.preventDefault();

        var $this = $(this).closest("td");
        $this.children("span:first").hide();
        $this.children("span:last").fadeToggle("slow");

        return false;
    });

    $('a.setComment').click(function (e) {
        e.preventDefault();

        var $this = $(this).closest("form");

        $this.submit();
        return false;
    });

});

