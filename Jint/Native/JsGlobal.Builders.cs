using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Native
{
    partial class JsGlobal
    {
        public JsDate CreateDate(DateTime date)
        {
            return new JsDate(this, date, DateClass.Prototype);
        }

        public JsDate CreateDate(double value)
        {
            return new JsDate(this, value, DateClass.Prototype);
        }

        public JsFunction CreateFunction(string name, JsFunctionDelegate @delegate, int argumentCount, object closure)
        {
            return CreateFunction(name, @delegate, argumentCount, closure, CreateObject(FunctionClass.Prototype));
        }

        public JsFunction CreateFunction(string name, JsFunctionDelegate @delegate, int argumentCount, object closure, JsObject prototype)
        {
            return CreateFunction(name, @delegate, argumentCount, closure, prototype, false);
        }

        public JsFunction CreateFunction(string name, JsFunctionDelegate @delegate, int argumentCount, object closure, JsObject prototype, bool isClr)
        {
            return new JsFunction(this, name, @delegate, argumentCount, closure, prototype, isClr);
        }

        public JsObject CreateObject()
        {
            return CreateObject(null, ObjectClass.Prototype);
        }

        public JsObject CreateObject(JsObject prototype)
        {
            return CreateObject(null, prototype);
        }

        public JsObject CreateObject(object value, JsObject prototype)
        {
            return new JsObject(this, value, prototype);
        }

        public JsRegExp CreateRegExp(string pattern, JsRegExpOptions options)
        {
            return new JsRegExp(this, pattern, options, RegExpClass.Prototype);
        }

        public JsArray CreateArray()
        {
            return new JsArray(this, ArrayClass.Prototype);
        }

        public JsError CreateError(JsObject prototype)
        {
            return CreateError(prototype, null);
        }

        public JsError CreateError(JsObject prototype, string message)
        {
            return new JsError(this, prototype, message);
        }
    }
}
