assert(
    'bye',
    eval('\
        var i = 0; \
        if (i > 0) { \
            "hi"; \
        } else { \
            while (true) { \
                "bye"; \
                break; \
            } \
        } \
    '),
    'Return last result in if/while/break'
);

function f() { return 'hi'; }

assert(
    'hi',
    eval('\
        f(); \
    '),
    'Return result from last function call'
);

/*
// TODO: This one is tricky because in the bound tree, we don't know whether an
// assignment is because of a variable declaration.
assert(
    undefined,
    eval('\
        var i = 0; \
    '),
    'Variable declaration does not return anything'
);
*/

assert(
    1,
    eval('\
        var i = 0; \
        i = 1; \
    '),
    'Return value of last assignment'
);

assert(
    9,
    eval('\
        var i = 0; \
        while (i < 10) { \
            i++; \
        } \
    '),
    'Return pre-increment result in while loop.'
);
