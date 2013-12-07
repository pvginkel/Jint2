using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Native
{
    partial class JsGlobal
    {
        public JsObject CreateDate(DateTime date)
        {
            return CreateDate(JsConvert.ToNumber(date));
        }

        public JsObject CreateDate(double value)
        {
            return (JsObject)DateClass.Construct(
                _runtime,
                new[] { JsBox.CreateNumber(value) }
            );
        }

        public JsObject CreateFunction(string name, JsFunction @delegate, int argumentCount, object closure)
        {
            // The CreateObject here is because of 13.2; this is "Result(9)":
            //   9. Create a new object as would be constructed by the expression new Object(). 
            return CreateFunction(name, @delegate, argumentCount, closure, CreateObject(FunctionClass.Prototype));
        }

        public JsObject CreateFunction(string name, JsFunction @delegate, int argumentCount, object closure, JsObject prototype)
        {
            return CreateFunction(name, @delegate, argumentCount, closure, prototype, false);
        }

        public JsObject CreateFunction(string name, JsFunction @delegate, int argumentCount, object closure, JsObject prototype, bool isClr)
        {
            // Prototype is set to the created object from the CreateFunction
            // above; prototype here is "Result(9)"
            //   11. Set the prototype property of F to Result(9). This property is given attributes as specified in section 15.3.5.2. 
            var result = CreateObject(null, prototype, new JsDelegate(name, @delegate, argumentCount, closure));

            result.SetClass(JsNames.ClassFunction);
            result.SetIsClr(isClr);
            
            // Constructor on the prototype links back to the result of CreateFunction:
            //   10. Set the constructor property of Result(9) to F. This property is given attributes { DontEnum }. 
            DefineProperty(prototype, "constructor", JsBox.CreateObject(result), PropertyAttributes.DontEnum);


            return result;
        }

        public JsObject CreateObject()
        {
            return CreateObject(ObjectClass.Prototype);
        }

        public JsObject CreateObject(JsObject prototype)
        {
            return CreateObject(null, prototype, null);
        }

        public JsObject CreateObject(object value, JsObject prototype)
        {
            return CreateObject(value, prototype, null);
        }

        private JsObject CreateObject(object value, JsObject prototype, JsDelegate @delegate)
        {
            return new JsObject(this, value, prototype, @delegate);
        }

        public JsObject CreateError(JsObject constructor)
        {
            return CreateError(constructor, null);
        }

        public JsObject CreateError(JsObject constructor, string message)
        {
            return (JsObject)constructor.Construct(
                _runtime,
                new[] { JsString.Box(message) }
            );
        }

        public JsObject CreateRegExp(string pattern)
        {
            return CreateRegExp(pattern, null);
        }

        public JsObject CreateRegExp(string pattern, string options)
        {
            return (JsObject)RegExpClass.Construct(
                _runtime,
                new[]
                {
                    JsString.Box(pattern),
                    JsString.Box(options)
                }
            );
        }

        public JsObject CreateArray()
        {
            return (JsObject)ArrayClass.Construct(
                _runtime,
                JsBox.EmptyArray
            );
        }
    }
}
