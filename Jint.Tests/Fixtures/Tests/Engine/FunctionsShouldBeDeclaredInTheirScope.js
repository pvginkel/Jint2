function foo() {
    function bar() {
    }

    bar();
}

var bar = 1;
foo();
assert(1, bar);
