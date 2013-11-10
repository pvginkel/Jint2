using System;
using System.Collections.Generic;
using System.Text;
using Jint.Expressions;
using Jint.Delegates;
using Jint.Marshal;

namespace Jint.Native
{
    [Serializable]
    public class JsFunctionConstructor : JsConstructor
    {

        public JsFunctionConstructor(IGlobal global, JsObject prototype)
            : base(global, prototype)
        {
            Name = "Function";
            DefineOwnProperty(PrototypeName, prototype, PropertyAttributes.DontEnum | PropertyAttributes.DontDelete | PropertyAttributes.ReadOnly);
        }

        public override void InitPrototype(IGlobal global)
        {
            var prototype = PrototypeProperty;

            // ((JsFunction)prototype).Scope = global.ObjectClass.Scope;

            prototype.DefineOwnProperty("constructor", this, PropertyAttributes.DontEnum);

            prototype.DefineOwnProperty(CallName, new JsCallFunction(this), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty(ApplyName, new JsApplyFunction(this), PropertyAttributes.DontEnum);

            prototype.DefineOwnProperty("toString", New<JsDictionaryObject>(ToString2), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("toLocaleString", New<JsDictionaryObject>(ToString2), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty(new PropertyDescriptor<JsObject>(global, prototype, "length", GetLengthImpl, SetLengthImpl));
        }



        public JsInstance GetLengthImpl(JsDictionaryObject target)
        {
            return Global.NumberClass.New(target.Length);
        }

        public JsInstance SetLengthImpl(JsInstance target, JsInstance[] parameters)
        {
            int length = (int)parameters[0].ToNumber();

            if (length < 0 || double.IsNaN(length) || double.IsInfinity(length))
            {
                throw new JsException(Global.RangeErrorClass.New("invalid length"));
            }

            JsDictionaryObject obj = (JsDictionaryObject)target;
            obj.Length = length;

            return parameters[0];
        }

        public JsInstance GetLength(JsDictionaryObject target)
        {
            return Global.NumberClass.New(target.Length);
        }

        public JsFunction New()
        {
            JsFunction function = new JsFunction(PrototypeProperty);
            function.PrototypeProperty = Global.ObjectClass.New(function);
            return function;
        }

        public JsFunction New<T>(Func<T, JsInstance> impl) where T : JsInstance
        {
            JsFunction function = new ClrImplDefinition<T>(impl, PrototypeProperty);
            function.PrototypeProperty = Global.ObjectClass.New(function);
            //function.Scope = new JsScope(PrototypeProperty);
            return function;
        }
        public JsFunction New<T>(Func<T, JsInstance> impl, int length) where T : JsInstance
        {
            JsFunction function = new ClrImplDefinition<T>(impl, length, PrototypeProperty);
            function.PrototypeProperty = Global.ObjectClass.New(function);
            //function.Scope = new JsScope(PrototypeProperty);
            return function;
        }

        public JsFunction New<T>(Func<T, JsInstance[], JsInstance> impl) where T : JsInstance
        {
            JsFunction function = new ClrImplDefinition<T>(impl, PrototypeProperty);
            function.PrototypeProperty = Global.ObjectClass.New(function);
            //function.Scope = new JsScope(PrototypeProperty);
            return function;
        }
        public JsFunction New<T>(Func<T, JsInstance[], JsInstance> impl, int length) where T : JsInstance
        {
            JsFunction function = new ClrImplDefinition<T>(impl, length, PrototypeProperty);
            function.PrototypeProperty = Global.ObjectClass.New(function);
            //function.Scope = new JsScope(PrototypeProperty);
            return function;
        }

        public JsFunction New(Delegate d)
        {
            if (d == null)
                throw new ArgumentNullException();
            //JsFunction function = new ClrFunction(d, PrototypeProperty);

            JsMethodImpl impl = Global.Marshaller.WrapMethod(d.GetType().GetMethod("Invoke"), false);
            JsObject wrapper = new JsObject(d, JsNull.Instance);

            JsFunction function = New<JsInstance>((that, args) => impl(Global, wrapper, args));
            function.PrototypeProperty = Global.ObjectClass.New(function);

            //function.Scope = new JsScope(PrototypeProperty);
            return function;
        }

        public override JsFunctionResult Execute(IGlobal visitor, JsDictionaryObject that, JsInstance[] parameters, Type[] genericArguments)
        {
            var result = Construct(parameters, null, null);
            return new JsFunctionResult(result, result);
        }

        public override JsObject Construct(JsInstance[] parameters, Type[] genericArgs, IGlobal global)
        {
            JsFunction instance = New();

            instance.Arguments = new List<string>();

            for (int i = 0; i < parameters.Length - 1; i++)
            {
                string arg = parameters[i].ToString();

                foreach (string a in arg.Split(','))
                {
                    instance.Arguments.Add(a.Trim());
                }
            }

            if (parameters.Length >= 1)
            {
                ProgramSyntax p = JintEngine.Compile(parameters[parameters.Length - 1].Value.ToString());
                instance.Statement = new BlockSyntax(p);
            }

            return instance;
        }

        public JsInstance ToString2(JsDictionaryObject target, JsInstance[] parameters)
        {
            return Global.StringClass.New(target.ToSource());
        }
    }
}
