function foo() {
    var i;
    for (i = 2; i < 3; i++);
}

function bar() {
    var i = 1;
    foo();

    assert(1, i);
}

bar();