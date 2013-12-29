var i = 0;
var hadException;
function f() { i++; }

assert(undefined, undefined);
assert(null, null);

// Check assignment to null.

i = 0;
hadException = false;

try {
    null = f();
} catch (e) {
    hadException = true;
}

// Assignment to null throws and does not execute the expression.

assert(true, hadException);
assert(0, i);

// Check assignment to undefined.

i = 0;
hadException = false;

try {
    undefined = f();
} catch (e) {
    hadException = true;
}

// Assignment to undefined does not throw and does evaluate the expression.

assert(false, hadException);
assert(1, i);

// Assignment to undefined returns undefined.

i = 0;
hadException = false;

var j = 1;

try {
    j = undefined = f();
} catch (e) {
    hadException = true;
}

// Assignment to undefined does not throw and does evaluate the expression.

assert(false, hadException);
assert(1, i);
assert(undefined, j);

// Assignment to this throws and does not execute the expression.

i = 0;
hadException = false;

(function () {
    try {
        this = f();
    } catch (e) {
        hadException = true;
    }
})();

// Assignment to this throws and does not execute the expression.

assert(true, hadException);
assert(0, i);
