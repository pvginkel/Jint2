// Without strict.

var hadException = false;

try {
    (function () {
        delete Object.prototype;
    })();
} catch (e) {
    hadException = true;
}

assert(false, hadException);

// With strict.

try {
    (function () {
        'use strict';
        delete Object.prototype;
    })();
} catch (e) {
    hadException = true;
}

assert(true, hadException);
