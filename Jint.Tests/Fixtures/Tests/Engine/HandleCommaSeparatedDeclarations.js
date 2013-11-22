var i, j = 1, k = 3 * 2;

function foo() {
    var l, m = 1, n = 3 * 2;

    assert(undefined, l);
    assert(1, m);
    assert(6, n);
}

assert(undefined, i);
assert(1, j);
assert(6, k);

foo();