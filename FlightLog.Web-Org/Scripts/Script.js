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

        // async post - requires e.g. backbone.jd for response handling.
        //        $.post($this.attr('action'), $this.serialize(), function (data) {
        //            $this.replaceWith(data);
        //        }, 'html');

        $this.submit();
        return false;
    });

});

