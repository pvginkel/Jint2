using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Native
{
    partial class JsGlobal
    {
        private partial class Scope : JsObject
        {
            public Scope(JsGlobal global)
                : base(global, null, global.ObjectClass)
            {
                DefineProperty(this, "null", JsNull.Instance, PropertyAttributes.DontEnum);
                DefineProperty(this, "Function", global.FunctionClass, PropertyAttributes.DontEnum);
                DefineProperty(this, "Object", global.ObjectClass, PropertyAttributes.DontEnum);
                DefineProperty(this, "Array", global.ArrayClass, PropertyAttributes.DontEnum);
                DefineProperty(this, "Boolean", global.BooleanClass, PropertyAttributes.DontEnum);
                DefineProperty(this, "Date", global.DateClass, PropertyAttributes.DontEnum);
                DefineProperty(this, "Error", global.ErrorClass, PropertyAttributes.DontEnum);
                DefineProperty(this, "EvalError", global.EvalErrorClass, PropertyAttributes.DontEnum);
                DefineProperty(this, "RangeError", global.RangeErrorClass, PropertyAttributes.DontEnum);
                DefineProperty(this, "ReferenceError", global.ReferenceErrorClass, PropertyAttributes.DontEnum);
                DefineProperty(this, "SyntaxError", global.SyntaxErrorClass, PropertyAttributes.DontEnum);
                DefineProperty(this, "TypeError", global.TypeErrorClass, PropertyAttributes.DontEnum);
                DefineProperty(this, "URIError", global.URIErrorClass, PropertyAttributes.DontEnum);
                DefineProperty(this, "Number", global.NumberClass, PropertyAttributes.DontEnum);
                DefineProperty(this, "RegExp", global.RegExpClass, PropertyAttributes.DontEnum);
                DefineProperty(this, "String", global.StringClass, PropertyAttributes.DontEnum);
                DefineProperty(this, "Math", global.MathClass, PropertyAttributes.DontEnum);
                DefineProperty(this, "NaN", global.NumberClass["NaN"], PropertyAttributes.DontEnum); // 15.1.1.1
                DefineProperty(this, "Infinity", global.NumberClass["POSITIVE_INFINITY"], PropertyAttributes.DontEnum); // 15.1.1.2
                DefineProperty(this, "undefined", JsUndefined.Instance, PropertyAttributes.DontEnum); // 15.1.1.3
                DefineProperty(this, JsNames.This, this, PropertyAttributes.DontEnum);
                global.DefineFunction(this, "ToBoolean", Functions.ToBoolean, 1, PropertyAttributes.DontEnum);
                global.DefineFunction(this, "ToByte", Functions.ToByte, 1, PropertyAttributes.DontEnum);
                global.DefineFunction(this, "ToChar", Functions.ToChar, 1, PropertyAttributes.DontEnum);
                global.DefineFunction(this, "ToDateTime", Functions.ToDateTime, 1, PropertyAttributes.DontEnum);
                global.DefineFunction(this, "ToDecimal", Functions.ToDecimal, 1, PropertyAttributes.DontEnum);
                global.DefineFunction(this, "ToDouble", Functions.ToDouble, 1, PropertyAttributes.DontEnum);
                global.DefineFunction(this, "ToInt16", Functions.ToInt16, 1, PropertyAttributes.DontEnum);
                global.DefineFunction(this, "ToInt32", Functions.ToInt32, 1, PropertyAttributes.DontEnum);
                global.DefineFunction(this, "ToInt64", Functions.ToInt64, 1, PropertyAttributes.DontEnum);
                global.DefineFunction(this, "ToSByte", Functions.ToSByte, 1, PropertyAttributes.DontEnum);
                global.DefineFunction(this, "ToSingle", Functions.ToSingle, 1, PropertyAttributes.DontEnum);
                global.DefineFunction(this, "ToString", Functions.ToString, 1, PropertyAttributes.DontEnum);
                global.DefineFunction(this, "ToUInt16", Functions.ToUInt16, 1, PropertyAttributes.DontEnum);
                global.DefineFunction(this, "ToUInt32", Functions.ToUInt32, 1, PropertyAttributes.DontEnum);
                global.DefineFunction(this, "ToUInt64", Functions.ToUInt64, 1, PropertyAttributes.DontEnum);
                global.DefineFunction(this, "eval", Functions.Eval, 1, PropertyAttributes.DontEnum); // 15.1.2.1
                global.DefineFunction(this, "parseInt", Functions.ParseInt, 1, PropertyAttributes.DontEnum); // 15.1.2.2
                global.DefineFunction(this, "parseFloat", Functions.ParseFloat, 1, PropertyAttributes.DontEnum); // 15.1.2.3
                global.DefineFunction(this, "isNaN", Functions.IsNaN, 1, PropertyAttributes.DontEnum);
                global.DefineFunction(this, "isFinite", Functions.IsFinite, 1, PropertyAttributes.DontEnum);
                global.DefineFunction(this, "decodeURI", Functions.DecodeURI, 1, PropertyAttributes.DontEnum);
                global.DefineFunction(this, "encodeURI", Functions.EncodeURI, 1, PropertyAttributes.DontEnum);
                global.DefineFunction(this, "decodeURIComponent", Functions.DecodeURIComponent, 1, PropertyAttributes.DontEnum);
                global.DefineFunction(this, "encodeURIComponent", Functions.EncodeURIComponent, 1, PropertyAttributes.DontEnum);
            }

            public override string Class
            {
                get { return ClassGlobal; }
            }

            public override JsInstance this[string index]
            {
                get
                {
                    var descriptor = GetDescriptor(index);
                    if (descriptor != null)
                        return descriptor.Get(this);

                    // If we're the global scope, perform special handling on JsUndefined.

                    return Global.Backend.ResolveUndefined(index, null);
                }
                set
                {
                    base[index] = value;
                }
            }
        }
    }
}
