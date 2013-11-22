function sayHi(x) {
    alert('Hi, ' + x + '!');
}

sayHi.text = 'Hello World!';
sayHi['text2'] = 'Hello World... again.';

assert('Hello World!', sayHi['text']);
assert('Hello World... again.', sayHi.text2);
