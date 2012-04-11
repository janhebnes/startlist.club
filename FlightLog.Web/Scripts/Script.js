$(document).ready(function () {
    $('a.back').click(function () {
        parent.history.back();
        return false;
    });

    $('a.editComment').click(function (e) {
        e.preventDefault();

        var $this = $(this).closest("td");
        console.log($this);

        //$this.children("span:first").toggle();
        $this.children("span:last").toggle();

        return false;
    });


});

