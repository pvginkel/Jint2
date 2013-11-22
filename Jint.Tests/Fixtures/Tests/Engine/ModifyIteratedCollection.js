var values = [0, 1, 2];

for (var v in values) {
    values[v] = v * v;
}

assert(0, values[0]);
assert(1, values[1]);
assert(4, values[2]);
