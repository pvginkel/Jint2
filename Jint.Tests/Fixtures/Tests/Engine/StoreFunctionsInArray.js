// functions stored as array elements
var arr = [];
arr[0] = function (x) { return x * x; };
arr[1] = arr[0](2);
arr[2] = arr[0](arr[1]);
arr[3] = arr[0](arr[2]);

// displays 256
assert(256, arr[3]);