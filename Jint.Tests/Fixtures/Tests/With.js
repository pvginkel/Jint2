var a, x, y;
var r = 10;
with (Math) {
    a = PI * r * r;
    x = r * cos(PI);
    y = r * sin(PI / 2);
}

assert(-10, x);
assert(10, y);

var t = {
    test: function() { return this; }
};

var t2 = {
    test2: function() { return this; }
};

with (t) {
    with (t2) {
        assert(t,test());
        assert(t2,test2());
    }
}

(function () {
    var y = 3;
    var x = { y: 7 };

    with (x) {
        y = 10;
        (function () {
            y = 11;
        })();
    }

    assert(3, y);
    assert(11, x.y);
})();

(function () {
    var y = 3;
    var x = { };

    with (x) {
        y = 10;
        (function () {
            x.y = 4;
            y = 11;
        })();
    }

    assert(10, y);
    assert(11, x.y);
})();

var y = 3;
var x = { y: 7 };

with (x) {
    y = 10;
    (function () {
        y = 11;
    })();
}

assert(3, y);
assert(11, x.y);

var y = 3;
var x = {};

with (x) {
    y = 10;
    (function () {
        x.y = 4;
        y = 11;
    })();
}

assert(10, y);
assert(11, x.y);
