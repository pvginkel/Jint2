using System;
namespace Jint.Native {
    public interface IGlobal {
        bool HasOption(Options options);

        JsArrayConstructor ArrayClass { get; }
        JsBooleanConstructor BooleanClass { get; }
        JsDateConstructor DateClass { get; }
        JsErrorConstructor ErrorClass { get; }
        JsErrorConstructor EvalErrorClass { get; }
        JsFunctionConstructor FunctionClass { get; }
        JsInstance IsNaN(JsInstance[] arguments);
        JsMathConstructor MathClass { get; }
        JsNumberConstructor NumberClass { get; }
        JsObjectConstructor ObjectClass { get; }
        JsInstance ParseFloat(Jint.Native.JsInstance[] arguments);
        JsInstance ParseInt(Jint.Native.JsInstance[] arguments);
        JsErrorConstructor RangeErrorClass { get; }
        JsErrorConstructor ReferenceErrorClass { get; }
        JsRegExpConstructor RegExpClass { get; }
        JsStringConstructor StringClass { get; }
        JsErrorConstructor SyntaxErrorClass { get; }
        JsErrorConstructor TypeErrorClass { get; }
        JsErrorConstructor URIErrorClass { get; }
        JsObject Wrap(object value);
        JsObject WrapClr(object value);

        JsInstance NaN { get; }

        IJintBackend Backend { get; }
        Marshaller Marshaller { get; }
    }
}
