try {
    a = b;
    assert(true, false);
}
catch (e) {
}

assert('undefined', typeof foo);

try {
    var y;
    assert(false, y instanceof Foo);
    assert(true, false);
}
catch (e) {
}
