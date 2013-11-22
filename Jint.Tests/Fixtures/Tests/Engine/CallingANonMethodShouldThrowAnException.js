try {
    var x = { prop: 'abc'};
    x.prop();
    fail('should have thrown an Error');
}
catch (e) {
    return;
}
fail('should have caught an Error');