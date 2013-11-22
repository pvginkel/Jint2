var box = new Jint.Tests.Fixtures.Box();
box.SetSize(ToInt32(100), ToInt32(100));
assert(100, Number(box.Width));
assert(100, Number(box.Height));