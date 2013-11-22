HelloWorld.webCallable = 'GET';
function HelloWorld() {
    return 'Hello from Javascript!';
}

assert('GET', HelloWorld.webCallable);