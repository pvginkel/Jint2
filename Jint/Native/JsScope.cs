using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Native
{
    /// <summary>
    /// Scope. Uses Prototype inheritance to store scopes hierarchy.
    /// </summary>
    /// <remarks>
    /// Tries to add new properties to the global scope.
    /// </remarks>
    [Serializable]
    public class JsScope : JsDictionaryObject
    {
        private Descriptor _thisDescriptor;
        private Descriptor _argumentsDescriptor;
        private readonly JsScope _globalScope;
        private readonly JsDictionaryObject _bag;

        public const string This = "this";
        public const string Arguments = "arguments";

        public JsScope Outer { get; private set; }
        public object CompiledScope { get; set; }

        /// <summary>
        /// Creates a new Global scope
        /// </summary>
        public JsScope()
            : base(JsNull.Instance)
        {
            _globalScope = null;
        }

        /// <summary>
        /// Creates a new scope inside the specified scope
        /// </summary>
        /// <param name="outer">Scope inside which the new scope should be created</param>
        public JsScope(JsScope outer)
            : base(outer)
        {
            if (outer == null)
                throw new ArgumentNullException("outer");

            Outer = outer;
            _globalScope = outer.Global;
        }

        public JsScope(JsScope outer, JsDictionaryObject bag)
            : base(outer)
        {
            if (outer == null)
                throw new ArgumentNullException("outer");
            if (bag == null)
                throw new ArgumentNullException("bag");

            Outer = outer;
            _globalScope = outer.Global;
            _bag = bag;
        }

        public JsScope(JsDictionaryObject bag)
            : base(JsNull.Instance)
        {
            _bag = bag;
        }

        public override string Class
        {
            get { return ClassScope; }
        }

        public override JsType Type
        {
            get { return JsType.Object; }
        }

        public JsScope Global
        {
            get { return _globalScope ?? this; }
        }

        public override JsInstance this[string index]
        {
            get
            {
                if (index == This && _thisDescriptor != null)
                    return _thisDescriptor.Get(this);
                if (index == Arguments && _argumentsDescriptor != null)
                    return _argumentsDescriptor.Get(this);

                var descriptor = GetDescriptor(index);
                if (descriptor != null)
                    return descriptor.Get(this);

                // If we're the global scope, perform special handling on JsUndefined.

                if (_globalScope == null)
                    return ((JsGlobal)Global._bag).Backend.ResolveUndefined(index, null);

                throw new JsException(((JsGlobal)Global._bag).ReferenceErrorClass.New());
            }
            set
            {
                if (index == This)
                {
                    if (_thisDescriptor != null)
                        _thisDescriptor.Set(this, value);
                    else
                    {
                        DefineOwnProperty(_thisDescriptor = new ValueDescriptor(this, index, value));
                    }
                }
                else if (index == Arguments)
                {
                    if (_argumentsDescriptor != null)
                        _argumentsDescriptor.Set(this, value);
                    else
                    {
                        DefineOwnProperty(_argumentsDescriptor = new ValueDescriptor(this, index, value));
                    }
                }
                else
                {
                    Descriptor d = GetDescriptor(index);
                    if (d != null)
                    {
                        d.Set(this, value);
                    }
                    else if (_globalScope != null)
                    {
                        // TODO: move to Execution visitor
                        // define missing property in the global scope
                        _globalScope.DefineOwnProperty(index, value);
                    }
                    else
                    {
                        // this scope is a global scope
                        DefineOwnProperty(index, value);
                    }
                }
            }
        }

        /// <summary>
        /// Overriden. Returns a property descriptor.
        /// </summary>
        /// <remarks>
        /// Tries to resolve proeprty in the following order:
        /// 
        /// 1. OwnProperty for the current scope
        /// 2. Any property from the bag (if specified).
        /// 3. A property from scopes hierarchy.
        /// 
        /// A proeprty from the bag will be added as a link to the current scope.
        /// </remarks>
        /// <param name="index">Property name.</param>
        /// <returns>Descriptor</returns>
        public override Descriptor GetDescriptor(string index)
        {
            Descriptor own, d;
            if ((own = base.GetDescriptor(index)) != null && own.Owner == this)
                return own;

            if (_bag != null && (d = _bag.GetDescriptor(index)) != null)
            {
                Descriptor link = new LinkedDescriptor(this, d.Name, d, _bag);
                DefineOwnProperty(link);
                return link;
            }

            return own;
        }

        public override IEnumerable<string> GetKeys()
        {
            if (_bag != null)
            {
                foreach (var key in _bag.GetKeys())
                    if (BaseGetDescriptor(key) == null)
                        yield return key;
            }
            foreach (var key in BaseGetKeys())
                yield return key;
        }

        private Descriptor BaseGetDescriptor(string key)
        {
            return base.GetDescriptor(key);
        }

        private IEnumerable<string> BaseGetKeys()
        {
            return base.GetKeys();
        }

        public override IEnumerable<JsInstance> GetValues()
        {
            foreach (var key in GetKeys())
                yield return this[key];
        }

        public override bool IsClr
        {
            get
            {
                return _bag != null ? _bag.IsClr : false;
            }
        }

        public override object Value
        {
            get
            {
                return _bag != null ? _bag.Value : null;
            }
            set
            {
                if (_bag != null)
                    _bag.Value = value;
            }
        }

    }
}
