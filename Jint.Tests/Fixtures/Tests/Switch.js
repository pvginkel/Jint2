function f(v) {
    var result = '';
    switch (v) {
        case 1:
            result += '1';
        case 2:
            result += '2';
            break;
        case 3:
            result += '3';
        default:
            result += 'd';
        case 4:
            result += '4';
    }
    return result;
}

assert('12', f(1));
assert('2', f(2));
assert('3d4', f(3));
assert('d4', f(5));
assert('4', f(4));

function g(v) {
    var result = '';
    switch (v) {
        case 1:
            result += '1';
        case 2:
            result += '2';
            break;
        case 3:
            result += '3';
        case 4:
            result += '4';
    }
    return result;
}


assert('12', g(1));
assert('2', g(2));
assert('34', g(3));
assert('', g(5));
assert('4', g(4));
