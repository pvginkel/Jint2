// functions as object properties
var obj = { 'toString': function () { return 'This is an object.'; } };
// calls obj.toString()
assert('This is an object.', obj.toString());