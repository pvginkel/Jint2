var a = false;
assert(false, a);
test();
assert(true, a);

function test() {
    a = true;
}