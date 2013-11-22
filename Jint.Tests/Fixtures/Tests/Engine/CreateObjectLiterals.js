var myDog = {
    'name': 'Spot',
    'bark': function () { return 'Woof!'; },
    'displayFullName': function () {
        return this.name + ' The Alpha Dog';
    },
    'chaseMrPostman': function () {
        // implementation beyond the scope of this article 
    }
};
assert('Spot The Alpha Dog', myDog.displayFullName());
assert('Woof!', myDog.bark()); // Woof!