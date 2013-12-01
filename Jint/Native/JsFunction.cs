using System;
using System.Collections.Generic;
using System.Text;
using Jint.Runtime;

namespace Jint.Native
{
    [Serializable]
    public class JsFunction : JsObject
    {
        private readonly bool _isClr;
        public JsFunctionDelegate Delegate { get; private set; }
        public string Name { get; private set; }
        public object Closure { get; private set; }

        internal JsFunction(JsGlobal global, string name, JsFunctionDelegate @delegate, int argumentCount, object closure, JsObject prototype, bool isClr)
            : base(global, null, prototype)
        {
            _isClr = isClr;
            if (@delegate == null)
                throw new ArgumentNullException("delegate");

            Name = name;
            Closure = closure;
            Delegate = @delegate;
            Length = argumentCount;
        }

        public override bool IsClr
        {
            get { return _isClr; }
        }

        public override string Class
        {
            get { return JsNames.ClassFunction; }
        }

        // 15.3.5.3
        public bool HasInstance(JsObject instance)
        {
            return
                instance != null &&
                Prototype.IsPrototypeOf(instance);
        }

        // 13.2.2
        public JsInstance Construct(JintRuntime runtime, JsInstance[] arguments)
        {
            // TODO: Change this when we flatten the hierarchy further.

            JsObject @this;

            switch (Name)
            {
                case "Array": @this = Global.CreateArray(); break;
                case "Date": @this = Global.CreateDate(0); break;
                case "Error": @this = Global.CreateError((JsObject)this["prototype"]); break;
                case "Function": @this = null; break;
                case "RegExp": @this = null; break;
                default: @this = Global.CreateObject((JsObject)this["prototype"]); break;
            }

            var boxedThis = @this;

            var result = Delegate(runtime, boxedThis, this, Closure, arguments, null);

            if (result is JsObject)
                return result;

            return boxedThis;
        }

        public JsInstance Execute(JintRuntime runtime, JsInstance @this, JsInstance[] arguments, JsInstance[] genericArguments)
        {
            return Delegate(runtime, @this, this, Closure, arguments, genericArguments);
        }
    }
}
