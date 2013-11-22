// assign an anonymous function to a variable
var greet = function (x) {
    return 'Hello, ' + x;
};

assert('Hello, MSDN readers', greet('MSDN readers'));

// passing a function as an argument to another
function square(x) {
    return x * x;
}
function operateOn(num, func) {
    return func(num);
}
// displays 256
assert(256, operateOn(16, square));

// functions as return values
function makeIncrementer() {
    return function (x) { return x + 1; };
}
var inc = makeIncrementer();
// displays 8
assert(8, inc(7));