f = function () { var i = 10; }
for (var i = 0; i < 3; i++) { f(); }
assert(3, i);

f = function () { i = 10; }
for (i = 0; i < 3; i++) { f(); }
assert(11, i);

f = function () { var i = 10; }
for (i = 0; i < 3; i++) { f(); }
assert(3, i);

f = function () { i = 10; }
for (var i = 0; i < 3; i++) { f(); }
assert(11, i);