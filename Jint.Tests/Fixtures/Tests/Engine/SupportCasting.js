var value = Number(3);
assert('number', typeof value);
value = String(value); // casting
assert('string', typeof value);
assert('3', value);
