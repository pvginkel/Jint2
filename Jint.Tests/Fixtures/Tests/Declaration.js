// Undeclared global variable must throw.

try {
    print(k);
    fail('Access of undeclared variable should throw ReferenceError');
}
catch (e) {
    if (!(e instanceof ReferenceError))
        throw e;
}

// Undeclared local variable must throw.

(function () {
    try {
        print(l);
        fail('Access of undeclared variable should throw ReferenceError');
    }
    catch (e) {
        if (!(e instanceof ReferenceError))
            throw e;
    }
})();

// Global variable must exist before declaration.

assert(true, i === undefined, 'Global variable must exist before being declared');

var i = 1;

// Local variable must exist before declaration.

(function () {
    assert(true, j === undefined, 'Local variable must exist before being declared');

    var j = 1;
})();
