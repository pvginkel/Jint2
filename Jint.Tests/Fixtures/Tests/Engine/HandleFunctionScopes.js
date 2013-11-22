var success = false;
$ = {};

(function () {

    function a(x) {
        success = x;
    }

    $.b = function () {
        a(true);
    }

}());

$.b();