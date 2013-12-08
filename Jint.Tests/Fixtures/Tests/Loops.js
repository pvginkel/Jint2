var obj = {prop1: 5, prop2: 13, prop3: 8};

var result = "";
for (var i in obj) {
  result += i;
}
assert("prop1prop2prop3", result);
