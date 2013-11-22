function doSwitch(input) {
    var result = 0;
    switch (input) {
        case 'a':
        case 'b':
            result = 2;
            break;
        case 'c':
            result = 3;
            break;
        case 'd':
            result = 4;
            break;
        default:
            break;
    }
    return result;
}

assert(2, doSwitch('a'));
assert(0, doSwitch('z'));
assert(2, doSwitch('b'));
assert(3, doSwitch('c'));
assert(4, doSwitch('d'));
