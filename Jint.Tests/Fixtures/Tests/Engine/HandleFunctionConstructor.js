var func = new Function('x', 'return x * x;');
var r = func(3);
assert(9, r);