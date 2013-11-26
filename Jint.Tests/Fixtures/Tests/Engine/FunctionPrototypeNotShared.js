var expected;

function TypeA() {
}

TypeA.prototype.f = function () {
    assert(expected, 'a');
}

function TypeB() {
}

TypeB.prototype.f = function () {
    assert(expected, 'b');
}

var a = new TypeA();
expected = 'a';
a.f();
var b = new TypeB();
expected = 'b';
b.f();
