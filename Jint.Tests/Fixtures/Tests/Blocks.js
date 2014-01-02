/*
{ 
    { 
        { 
            i = 1; 
        }
    }
}

assert(1, i);

// variable declared outside a function are global
{ 
    var i = 1; 
} 
{ 
    i = i + 1; 
} 

// assert(2, i);

var t = 10;

(function(){

    assert(null, t);

    t = 20;
    
    if (0) {
        var t = 10;
    }

})();

assert(10, t);

// Declarations in block are still available in glocal scope
var i =1;
if(false) {
    var prevTime = 1;
}

assert(undefined, prevTime);
*/
// Catch identifiers are scoped to the catch and can be closed over.
var e = 3;
(function () {
    e = 7;
    try {
        throw 'x';
    } catch (e) {
        assert('x', e);
        (function () {
            e = 'y';
        })();
        assert('y', e);
    }
    assert(7, e);
})();
assert(7, e);

// Catch identifiers are overruled by local declared identifiers.
var e = 3;
(function () {
    e = 7;
    try {
        throw 'x';
    } catch (e) {
        assert('x', e);
        (function () {
            var e = 'y';
        })();
        assert('x', e);
    }
    assert(7, e);
})();
assert(7, e);

// Target in for each in does not imply a declaration.
var e = 3;
(function () {
    e = 7;
    for (e in [1]) {
        assert('0', e);
    }
    assert('0', e);
})();
assert('0', e);
