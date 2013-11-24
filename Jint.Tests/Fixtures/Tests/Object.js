// 15.2.2

var a = new Object();

assert(true, Object.hasOwnProperty.call({ foo: 3 }, 'foo'));
assert(false, Object.hasOwnProperty.call({ foo: 3 }, 'dummy'));

function foo() {

}

(function ($foo) { $foo.bar = function () { }; })(foo)

assert('function', typeof foo.bar);

function A() {
};

A.prototype.fld = "a";

function B() {
};

B.prototype = new A();
B.prototype.constructor = B;

var inst = new B();
var instA = new A();

assert("a", inst.fld, 'Field of instance must go to prototype');

A.prototype.fld = "b";

assert("b", inst.fld, 'Change in prototype must cascade to instance');

assert("b", instA.fld, 'Change in prototype must cascade to all instances');

assert("b", B.prototype.fld, 'Chained prototypes must show base field');

B.prototype.fld = "c";

assert("c", inst.fld, 'Change in chained prototype must show in instances');

assert("b", A.prototype.fld, 'Change in chained prototype must not cascade to base prototype');
assert("b", instA.fld, 'Change in chainged prototype must not cascade to instances of base prototype');

instA.fld = "x";

assert("b", A.prototype.fld, 'Change in instance must not cascade to prototype');
assert("x", instA.fld, 'Change in instance must become visible');
assert("c", inst.fld, 'Change in instance must not cascade to other instances');

var a = {};
assert(undefined, a.foo, 'Undefined member should return undefined');
assert(undefined, a['foo'], 'Undefined index should return undefined');

baz = function () {
    function bar(b, c, d) {
        this.base = b, this.properties = c || [], d && (this[d] = true);
        return this;
    }
    assert(true, bar({ "value": "number" }, null, null).properties != null, 'Assignment to this in function should become visible');
}

baz();

