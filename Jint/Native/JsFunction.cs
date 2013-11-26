using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public int ArgumentCount { get; set; }
        public object Closure { get; private set; }

        internal JsFunction(JsGlobal global, string name, JsFunctionDelegate @delegate, int argumentCount, object closure, JsObject prototype, bool isClr)
            : base(global, null, prototype)
        {
            _isClr = isClr;
            if (@delegate == null)
                throw new ArgumentNullException("delegate");

            Name = name;
            ArgumentCount = argumentCount;
            Closure = closure;
            Delegate = @delegate;
        }

        public override int Length
        {
            get { return ArgumentCount; }
            set { }
        }

        // 15.3.5.3
        public bool HasInstance(JsObject instance)
        {
            return
                instance != null &&
                !IsNull(instance) &&
                Prototype.IsPrototypeOf(instance);
        }

        // 13.2.2
        public JsObject Construct(JintRuntime runtime, JsInstance[] arguments)
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

            var result = Delegate(runtime, @this, this, Closure, arguments, null);

            return result as JsObject ?? @this;
        }

        public JsInstance Execute(JintRuntime runtime, JsInstance @this, JsInstance[] arguments, JsInstance[] genericArguments)
        {
            return Delegate(runtime, @this, this, Closure, arguments, genericArguments);
        }

        public override string Class
        {
            get { return ClassFunction; }
        }

        public override string ToSource()
        {
            return String.Format("function {0} () {{ /* js code */ }}", Name);
        }

        public override bool IsClr
        {
            get { return _isClr; }
        }
    }
}
