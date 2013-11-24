using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Native
{
    [Serializable]
    public class JsFunction : JsObject
    {
        public static string CallName = "call";
        public static string ApplyName = "apply";
        public static string ConstructorName = "constructor";
        public static string PrototypeName = "prototype";

        public string Name { get; set; }
        public List<string> Arguments { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="global"></param>
        public JsFunction(JsGlobal global)
            : this(global, global.FunctionClass.Prototype)
        {
        }

        /// <summary>
        /// Init new function object with a specified prototype
        /// </summary>
        /// <param name="prototype">prototype for this object</param>
        public JsFunction(JsGlobal global, JsObject prototype)
            : base(global, prototype)
        {
            Arguments = new List<string>();
        }

        public override int Length
        {
            get { return Arguments.Count; }
            set { }
        }

        //15.3.5.3
        public virtual bool HasInstance(JsObject instance)
        {
            return
                instance != null &&
                !IsNull(instance) &&
                Prototype.IsPrototypeOf(instance);
        }

        //13.2.2
        public virtual JsObject Construct(JsInstance[] parameters, Type[] generics)
        {
            var instance = new JsObject(Global, (JsObject)this["prototype"]);

            var result = Global.Backend.ExecuteFunction(this, instance, parameters, generics);

            var obj = result.Result as JsObject;
            if (obj != null)
                return obj;

            obj = result.This as JsObject;
            if (obj != null)
                return obj;

            return instance;
        }

        public override object Value
        {
            get { return null; }
            set { }
        }

        public JsFunctionResult Execute(JsInstance that, JsInstance[] parameters)
        {
            return Execute(that, parameters, null);
        }

        public virtual JsFunctionResult Execute(JsInstance that, JsInstance[] parameters, Type[] genericArguments)
        {
            if (genericArguments != null)
                throw new JintException("This method can't be called as a generic");

            throw new InvalidOperationException();
        }

        public override string Class
        {
            get { return ClassFunction; }
        }

        public override string ToSource()
        {
            return String.Format("function {0} ( {1} ) {{ {2} }}", Name, String.Join(", ", Arguments.ToArray()), GetBody());
        }

        public virtual string GetBody()
        {
            return "/* js code */";
        }

        public override bool IsClr
        {
            get { return false; }
        }
    }
}
