var called = false;
function dontcallme() {
    called = true;
}

assert(true, true || dontcallme());
assert(false, called);

assert(false, false && dontcallme());
assert(false, called);