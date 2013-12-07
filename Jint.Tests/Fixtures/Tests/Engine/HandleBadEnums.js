assert('Name', Jint.Tests.Fixtures.FooEnum.Name.toString());
assert('function GetType ( ) { [native code] }', Jint.Tests.Fixtures.FooEnum.GetType.toString());
assert('IsEnum', Jint.Tests.Fixtures.FooEnum.IsEnum.toString());
assert('System', Jint.Tests.Fixtures.FooEnum.System.toString());

// still can access hidden Type properties
assert('FooEnum', Jint.Tests.Fixtures.FooEnum.get_Name());