try {
    if(abc) {
    }
    fail('should have thrown an Error');
}
catch (e) {
    return;
}
fail('should have caught an Error');

try {
    do{
    } while(abc);

    fail('should have thrown an Error');
}
catch (e) {
    return;
}
fail('should have caught an Error');
