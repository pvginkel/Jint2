function filter(pred, arr) {
    var len = arr.length;
    var filtered = []; // shorter version of new Array();
    // iterate through every element in the array...
    for (i = 0; i < len; i = i + 1) {
        var val = arr[i];
        // if the element satisfies the predicate let it through
        if (pred(val)) {
            filtered.push(val);
        }
    }
    return filtered;
}

var someRandomNumbers = [12, 32, 1, 3, 2, 2, 234, 236, 632, 7, 8];

assert(11, someRandomNumbers.length);

function makeGreaterThanPredicate(lowerBound) {
    return function(numberToCheck) {
        return (numberToCheck > lowerBound) ? true : false;
    };
}

var greaterThan10 = makeGreaterThanPredicate(10);
var greaterThan100 = makeGreaterThanPredicate(100);

a = filter(greaterThan10, someRandomNumbers);
b = filter(greaterThan100, someRandomNumbers);

assert(5, a.length);
assert(3, b.length);

function foo() {
    var x = "right";
    function bar() {
        assert("right", x);
    }
    callFunc(bar);
}

function callFunc(f) {
    var x = "wrong";
    f();
}

foo();

/*
 * Closures should close over the correct variables.
 */

var i = 1; // This one isn't closed over because it's of the global this.

function f() {
    i++;
    var j = 2; // This one is closed over.

    var g = function () {
        i++;
        j++;
        var k = 3; // This one is closed over.

        var h = function () {
            i++;
            j++; // Closed over from f.
            k++; // Closed over from g.
        };

        h();

        assert(k, 4);
    };

    g();

    assert(j, 4);
}

f();

assert(i, 4);

/*
 * Function parameters should be captured.
 */

function x(v) {
    function g() {
        assert(v, 7);
    }

    g();
}

x(7);
