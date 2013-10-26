using System;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using System.Text;
using Jint.Native;

namespace Jint.Backend.Compiled
{
    public abstract class JintProgram
    {
        private readonly IJintBackend _backend;

        private ResultInfo _lastResult;
        private StringBuilder _typeFullName;
        private string _lastIdentifier;

        public IGlobal Global { get; private set; }
        public JsScope GlobalScope { get; private set;  }
        public JsScope CurrentScope { get; private set; }
        public PermissionSet PermissionSet { get; private set; }

        protected JintProgram(IJintBackend backend, Options options)
        {
            if (backend == null)
                throw new ArgumentNullException("backend");

            _backend = backend;
            Global = backend.Global;
            GlobalScope = backend.GlobalScope;
            PermissionSet = new PermissionSet(PermissionState.None);

            var global = (JsObject)Global;

            global["ToBoolean"] = Global.FunctionClass.New(new Func<object, Boolean>(Convert.ToBoolean));
            global["ToByte"] = Global.FunctionClass.New(new Func<object, Byte>(Convert.ToByte));
            global["ToChar"] = Global.FunctionClass.New(new Func<object, Char>(Convert.ToChar));
            global["ToDateTime"] = Global.FunctionClass.New(new Func<object, DateTime>(Convert.ToDateTime));
            global["ToDecimal"] = Global.FunctionClass.New(new Func<object, Decimal>(Convert.ToDecimal));
            global["ToDouble"] = Global.FunctionClass.New(new Func<object, Double>(Convert.ToDouble));
            global["ToInt16"] = Global.FunctionClass.New(new Func<object, Int16>(Convert.ToInt16));
            global["ToInt32"] = Global.FunctionClass.New(new Func<object, Int32>(Convert.ToInt32));
            global["ToInt64"] = Global.FunctionClass.New(new Func<object, Int64>(Convert.ToInt64));
            global["ToSByte"] = Global.FunctionClass.New(new Func<object, SByte>(Convert.ToSByte));
            global["ToSingle"] = Global.FunctionClass.New(new Func<object, Single>(Convert.ToSingle));
            global["ToString"] = Global.FunctionClass.New(new Func<object, String>(Convert.ToString));
            global["ToUInt16"] = Global.FunctionClass.New(new Func<object, UInt16>(Convert.ToUInt16));
            global["ToUInt32"] = Global.FunctionClass.New(new Func<object, UInt32>(Convert.ToUInt32));
            global["ToUInt64"] = Global.FunctionClass.New(new Func<object, UInt64>(Convert.ToUInt64));

            EnterScope(GlobalScope);
        }

        public abstract JsInstance Main();

        protected JsInstance GetByIdentifier(string propertyName)
        {
            Descriptor result;
            if (CurrentScope.TryGetDescriptor(propertyName, out result))
            {
                if (!result.IsReference)
                    return result.Get(CurrentScope);

                var linkedDescriptor = (LinkedDescriptor)result;

                SetResult(linkedDescriptor.Get(CurrentScope), linkedDescriptor.TargetObject);

                if (_lastResult.Result != null)
                    return _lastResult.Result;
            }

            if (propertyName == "null")
                return JsNull.Instance;

            if (propertyName == "undefined")
                return JsUndefined.Instance;

            if (_typeFullName == null)
                _typeFullName = new StringBuilder();

            _typeFullName.Append(propertyName);

            return null;
        }

        protected JsInstance GetByIndexer(JsInstance operand, JsInstance indexer)
        {
            var target = (JsObject)operand;

            //EnsureIdentifierIsDefined(Result);

            if (target.IsClr)
                EnsureClrAllowed();

            if (target.Class == JsInstance.ClassString)
            {
                try
                {
                    SetResult(Global.StringClass.New(target.ToString()[(int)indexer.ToNumber()].ToString()), target);

                    return _lastResult.Result;
                }
                catch
                {
                    // if an error occured, try to access the index as a member
                }
            }

            if (target.Indexer != null)
                SetResult(target.Indexer.Get(target, indexer), target);
            else
                SetResult(target[indexer], target);

            return _lastResult.Result;
        }

        private void EnsureClrAllowed()
        {
            //throw new NotImplementedException();
        }

        private void SetResult(JsInstance value, JsDictionaryObject baseObject)
        {
            _lastResult.Result = value;
            _lastResult.BaseObject = baseObject;
        }

        protected void AssignVariable(string identifier, JsInstance expression)
        {
            var scope = CurrentScope;

            // if the right expression is not defined, declare the variable as undefined
            if (expression != null)
            {
                if (!scope.HasOwnProperty(identifier))
                    scope.DefineOwnProperty(identifier, expression);
                else
                    scope[identifier] = expression;
            }
            else
            {
                // a var declaration should not affect existing one
                if (!scope.HasOwnProperty(identifier))
                    scope.DefineOwnProperty(identifier, JsUndefined.Instance);
            }
        }

        protected JsFunction CreateFunction(string name, CompiledFunctionDelegate function, string[] parameters)
        {
            return new CompiledFunction(function, Global.FunctionClass.PrototypeProperty)
            {
                Name = name,
                Scope = CurrentScope,
                Arguments = new List<string>(parameters ?? new string[0])
            };
        }

        protected JsInstance ExecuteFunction(JsDictionaryObject that, JsInstance target, JsInstance[] parameters)
        {
            if (target == JsUndefined.Instance || target == null)
            {
                if (String.IsNullOrEmpty(_lastIdentifier))
                    throw new JsException(Global.TypeErrorClass.New("Method isn't defined"));
            }

            Type[] genericParameters = null;

            //if (AllowClr && methodCall.Generics.Count > 0)
            //{
            //    genericParameters = new Type[methodCall.Generics.Count];

            //    try
            //    {
            //        var i = 0;
            //        foreach (var generic in methodCall.Generics)
            //        {
            //            generic.Accept(this);
            //            genericParameters[i] = Global.Marshaller.MarshalJsValue<Type>(Result);
            //            i++;
            //        }
            //    }
            //    catch (Exception e)
            //    {
            //        throw new JintException("A type parameter is required", e);
            //    }
            //}

            var function = target as JsFunction;
            if (function == null)
                throw new JsException(Global.ErrorClass.New("Function expected."));

            var original = new JsInstance[parameters.Length];

            parameters.CopyTo(original, 0);

            var returned = _backend.ExecuteFunction(function, that, parameters, genericParameters);

            for (var i = 0; i < original.Length; i++)
            {
                if (original[i] != parameters[i])
                {
                    throw new NotImplementedException();
                    //if (methodCall.Arguments[i] is MemberExpression && ((MemberExpression)methodCall.Arguments[i]).Member is IAssignable)
                    //    Assign((MemberExpression)methodCall.Arguments[i], parameters[i]);
                    //else if (methodCall.Arguments[i] is Identifier)
                    //    Assign(new MemberExpression(methodCall.Arguments[i], null), parameters[i]);
                }
            }

            return returned;
        }

        public JsScope EnterScope(JsDictionaryObject scope)
        {
            var previous = CurrentScope;
            CurrentScope = new JsScope(CurrentScope, scope);
            return previous;
        }

        public JsScope EnterScope(JsScope scope)
        {
            var previous = CurrentScope;
            CurrentScope = scope;
            return previous;
        }

        public void ExitScope(JsScope previousScope)
        {
            CurrentScope = previousScope;
        }

        protected JsInstance Div(JsInstance left, JsInstance right)
        {
            var rightNumber = right.ToNumber();
            var leftNumber = left.ToNumber();

            if (right == Global.NumberClass["NEGATIVE_INFINITY"] || right == Global.NumberClass["POSITIVE_INFINITY"])
                return Global.NumberClass.New(0);
            if (rightNumber == 0)
                return leftNumber > 0 ? Global.NumberClass["POSITIVE_INFINITY"] : Global.NumberClass["NEGATIVE_INFINITY"];
            return Global.NumberClass.New(leftNumber / rightNumber);
        }

        protected JsInstance Modulo(JsInstance left, JsInstance right)
        {
            if (right == Global.NumberClass["NEGATIVE_INFINITY"] || right == Global.NumberClass["POSITIVE_INFINITY"])
                return Global.NumberClass["POSITIVE_INFINITY"];
            if (right.ToNumber() == 0)
                return Global.NumberClass["NaN"];
            return Global.NumberClass.New(left.ToNumber() % right.ToNumber());
        }

        protected JsInstance Plus(JsInstance left, JsInstance right)
        {
            JsInstance lprim = left.ToPrimitive(Global);
            JsInstance rprim = right.ToPrimitive(Global);

            if (lprim.Class == JsInstance.ClassString || rprim.Class == JsInstance.ClassString)
                return Global.StringClass.New(String.Concat(lprim.ToString(), rprim.ToString()));
            return Global.NumberClass.New(lprim.ToNumber() + rprim.ToNumber());
        }

        protected JsInstance BitwiseAnd(JsInstance left, JsInstance right)
        {
            if (left == JsUndefined.Instance || right == JsUndefined.Instance)
                return Global.NumberClass.New(0);
            return Global.NumberClass.New(Convert.ToInt64(left.ToNumber()) & Convert.ToInt64(right.ToNumber()));
        }

        protected JsInstance BitwiseOr(JsInstance left, JsInstance right)
        {
            if (left == JsUndefined.Instance)
            {
                if (right == JsUndefined.Instance)
                    return Global.NumberClass.New(1);
                return Global.NumberClass.New(Convert.ToInt64(right.ToNumber()));
            }
            if (right == JsUndefined.Instance)
                return Global.NumberClass.New(Convert.ToInt64(left.ToNumber()));
            return Global.NumberClass.New(Convert.ToInt64(left.ToNumber()) | Convert.ToInt64(right.ToNumber()));
        }

        protected JsInstance BitwiseXOr(JsInstance left, JsInstance right)
        {
            if (left == JsUndefined.Instance)
            {
                if (right == JsUndefined.Instance)
                    return Global.NumberClass.New(1);
                return Global.NumberClass.New(Convert.ToInt64(right.ToNumber()));
            }
            if (right == JsUndefined.Instance)
                return Global.NumberClass.New(Convert.ToInt64(left.ToNumber()));
            return Global.NumberClass.New(Convert.ToInt64(left.ToNumber()) ^ Convert.ToInt64(right.ToNumber()));
        }

        protected JsInstance LeftShift(JsInstance left, JsInstance right)
        {
            if (left == JsUndefined.Instance)
                return Global.NumberClass.New(0);
            if (right == JsUndefined.Instance)
                return Global.NumberClass.New(Convert.ToInt64(left.ToNumber()));
            return Global.NumberClass.New(Convert.ToInt64(left.ToNumber()) << Convert.ToUInt16(right.ToNumber()));
        }

        protected JsInstance RightShift(JsInstance left, JsInstance right)
        {
            if (left == JsUndefined.Instance)
                return Global.NumberClass.New(0);
            if (right == JsUndefined.Instance)
                return Global.NumberClass.New(Convert.ToInt64(left.ToNumber()));
            return Global.NumberClass.New(Convert.ToInt64(left.ToNumber()) >> Convert.ToUInt16(right.ToNumber()));
        }

        protected JsInstance UnsignedRightShift(JsInstance left, JsInstance right)
        {
            if (left == JsUndefined.Instance)
                return Global.NumberClass.New(0);
            if (right == JsUndefined.Instance)
                return Global.NumberClass.New(Convert.ToInt64(left.ToNumber()));
            return Global.NumberClass.New(Convert.ToInt64(left.ToNumber()) >> Convert.ToUInt16(right.ToNumber()));
        }

        protected JsInstance InstanceOf(JsInstance left, JsInstance right)
        {
            var func = right as JsFunction;
            var obj = left as JsObject;
            if (func == null)
                throw new JsException(Global.TypeErrorClass.New("Right argument should be a function: " + right));
            if (obj == null)
                throw new JsException(Global.TypeErrorClass.New("Left argument should be an object: " + left));
            return Global.BooleanClass.New(func.HasInstance(obj));
        }

        protected JsInstance In(JsInstance left, JsInstance right)
        {
            if (right is ILiteral)
                throw new JsException(Global.ErrorClass.New("Cannot apply 'in' operator to the specified member."));
            return Global.BooleanClass.New(((JsDictionaryObject)right).HasProperty(left));
        }

        protected JsInstance Compare(JsInstance x, JsInstance y, CompareMode mode)
        {
            int compare = 0;

            if (x.IsClr && y.IsClr)
            {
                IComparable xcmp = x.Value as IComparable;

                if (xcmp == null || y.Value == null || xcmp.GetType() != y.Value.GetType())
                    return Global.BooleanClass.False;
                compare = xcmp.CompareTo(y.Value);
            }
            else
            {

                Double xnum = x.ToNumber();
                Double ynum = y.ToNumber();

                if (Double.IsNaN(xnum) || Double.IsNaN(ynum))
                    return Global.BooleanClass.False;

                if (xnum < ynum)
                    compare = -1;
                else if (xnum == ynum)
                    compare = 0;
                else
                    compare = 1;
            }

            bool result;

            switch (mode)
            {
                case CompareMode.Greater: result = compare > 0; break;
                case CompareMode.GreaterOrEqual: result = compare >= 0; break;
                case CompareMode.Lesser: result = compare < 0; break;
                case CompareMode.LesserOrEqual: result = compare <= 0; break;
                default: throw new ArgumentOutOfRangeException("mode");
            }

            return result ? Global.BooleanClass.True : Global.BooleanClass.False;
        }

        protected JsInstance CompareEquals(JsInstance x, JsInstance y)
        {
            if (x.IsClr && y.IsClr)
                return Global.BooleanClass.New(x.Value.Equals(y.Value));

            // if one of the arguments is a native js object, we should
            // apply an ecma compare rules
            /* if (x.IsClr)
            {
                return Compare(x.ToPrimitive(Global), y);
            }

            if (y.IsClr)
            {
                return Compare(x, y.ToPrimitive(Global));
            } */

            if (x.Type == y.Type)
            {
                // if both are Objects but then only one is Clrs
                if (x == JsUndefined.Instance)
                    return Global.BooleanClass.True;
                if (x == JsNull.Instance)
                    return Global.BooleanClass.True;

                switch (x.Type)
                {
                    case JsInstance.TypeNumber:
                        if (x.ToNumber() == double.NaN)
                            return Global.BooleanClass.False;
                        if (y.ToNumber() == double.NaN)
                            return Global.BooleanClass.False;
                        if (x.ToNumber() == y.ToNumber())
                            return Global.BooleanClass.True;
                        return Global.BooleanClass.False;
                    case JsInstance.TypeString:
                        return Global.BooleanClass.New(x.ToString() == y.ToString());
                    case JsInstance.TypeBoolean:
                        return Global.BooleanClass.New(x.ToBoolean() == y.ToBoolean());
                    case JsInstance.TypeObject:
                        return Global.BooleanClass.New(x == y);
                    default:
                        return Global.BooleanClass.New(x.Value.Equals(y.Value));
                }
            }

            if (x == JsNull.Instance && y == JsUndefined.Instance)
                return Global.BooleanClass.True;
            if (x == JsUndefined.Instance && y == JsNull.Instance)
                return Global.BooleanClass.True;
            if (x.Type == JsInstance.TypeNumber && y.Type == JsInstance.TypeString)
                return Global.BooleanClass.New(x.ToNumber() == y.ToNumber());
            if (x.Type == JsInstance.TypeString && y.Type == JsInstance.TypeNumber)
                return Global.BooleanClass.New(x.ToNumber() == y.ToNumber());
            if (x.Type == JsInstance.TypeBoolean || y.Type == JsInstance.TypeBoolean)
                return Global.BooleanClass.New(x.ToNumber() == y.ToNumber());
            if (y.Type == JsInstance.TypeObject && (x.Type == JsInstance.TypeString || x.Type == JsInstance.TypeNumber))
                return CompareEquals(x, y.ToPrimitive(Global));
            if (x.Type == JsInstance.TypeObject && (y.Type == JsInstance.TypeString || y.Type == JsInstance.TypeNumber))
                return CompareEquals(x.ToPrimitive(Global), y);
            return Global.BooleanClass.False;
        }

        protected JsInstance TypeOf(JsInstance operand)
        {
            if (operand == null)
                return Global.StringClass.New(JsUndefined.Instance.Type);
            if (operand is JsNull)
                return Global.StringClass.New(JsInstance.TypeObject);
            if (operand is JsFunction)
                return Global.StringClass.New(JsInstance.TypeofFunction);
            return Global.StringClass.New(operand.Type);
        }

        protected JsInstance PrefixIncrementIdentifier(string identifier, JsInstance value, int offset)
        {
            value = Global.NumberClass.New(value.ToNumber() + offset);

            AssignIdentifier(identifier, value);

            return value;
        }

        protected JsInstance PrefixIncrementIdentifier(ref JsInstance identifier, int offset)
        {
            identifier = Global.NumberClass.New(identifier.ToNumber() + offset);

            return identifier;
        }

        protected JsInstance PrefixIncrementMember(JsInstance baseObject, string identifier, JsInstance value, int offset)
        {
            value = Global.NumberClass.New(value.ToNumber() + offset);

            AssignMember(baseObject, identifier, value);

            return value;
        }

        protected JsInstance PrefixIncrementIndexer(JsInstance baseObject, JsInstance indexer, JsInstance value, int offset)
        {
            value = Global.NumberClass.New(value.ToNumber() + offset);

            AssignIndexer(baseObject, indexer, value);

            return value;
        }

        protected JsInstance PostfixIncrementIdentifier(string identifier, JsInstance value, int offset)
        {
            AssignIdentifier(identifier, Global.NumberClass.New(value.ToNumber() + offset));

            return value;
        }

        protected JsInstance PostfixIncrementIdentifier(ref JsInstance identifier, int offset)
        {
            var value = identifier;

            identifier = Global.NumberClass.New(value.ToNumber() + offset);

            return value;
        }

        protected JsInstance PostfixIncrementMember(JsInstance baseObject, string identifier, JsInstance value, int offset)
        {
            AssignMember(baseObject, identifier, Global.NumberClass.New(value.ToNumber() + offset));

            return value;
        }

        protected JsInstance PostfixIncrementIndexer(JsInstance baseObject, JsInstance indexer, JsInstance value, int offset)
        {
            AssignIndexer(baseObject, indexer, Global.NumberClass.New(value.ToNumber() + offset));

            return value;
        }

        protected JsInstance AssignIdentifier(string identifier, JsInstance value)
        {
            //Descriptor descriptor;
            //CurrentScope.TryGetDescriptor(identifier, out descriptor);

            // Assigning function Name
            //if (value.Class == JsInstance.CLASS_FUNCTION)
            //    ((JsFunction)value).Name = propertyName;

            return CurrentScope[identifier] = value;
        }

        protected JsInstance AssignMember(JsInstance baseObject, string identifier, JsInstance value)
        {
            if (baseObject == null)
                throw new JintException("Attempt to assign to an undefined variable.");

            // now baseObject contains an object or a scope against which to resolve left.Member

            // Assigning function Name
            //if (value.Class == JsInstance.CLASS_FUNCTION)
            //    ((JsFunction)value).Name = propertyName;

            return ((JsDictionaryObject)baseObject)[identifier] = value;
        }

        protected JsInstance AssignIndexer(JsInstance baseObject, JsInstance indexer, JsInstance value)
        {
            if (baseObject == null)
                throw new JintException("Attempt to assign to an undefined variable.");

            // now baseObject contains an object or a scope against which to resolve left.Member

            if (baseObject is JsObject)
            {
                var target = (JsObject)baseObject;

                if (target.Indexer != null)
                {
                    target.Indexer.Set(target, indexer, value);
                    return value;
                }
            }

            // Assigning function Name
            //if (value.Class == JsInstance.CLASS_FUNCTION)
            //    ((JsFunction)value).Name = Result.Value.ToString();

            return ((JsDictionaryObject)baseObject)[indexer] = value;
        }

        protected JsInstance GetByProperty(JsInstance operand, string propertyName)
        {
            // save base of current expression
            var callTarget = operand as JsDictionaryObject;

            // this check is disabled becouse it prevents Clr names to resolve
            //if ((callTarget) == null || callTarget == JsUndefined.Instance || callTarget == JsNull.Instance)
            //{
            //    throw new JsException( Global.TypeErrorClass.New( String.Format("An object is required: {0} while resolving property {1}", lastIdentifier, expression.Text) ) );
            //}

            _lastResult.Result = null;

            _lastIdentifier = propertyName;

            JsInstance result;

            if (callTarget != null && callTarget.TryGetProperty(propertyName, out result))
            {
                SetResult(result, callTarget);

                return _lastResult.Result;
            }

            if (_lastResult.Result == null && _typeFullName != null && _typeFullName.Length > 0)
                _typeFullName.Append('.').Append(propertyName);

            SetResult(JsUndefined.Instance, callTarget);

            return _lastResult.Result;
        }

        protected JsScope GetScope(int parent)
        {
            var scope = CurrentScope;

            for (int i = 0; i < parent; i++)
            {
                scope = scope.Outer;
            }

            return scope;
        }

        private struct ResultInfo
        {
            public JsDictionaryObject BaseObject;
            public JsInstance Result;
        }

        protected enum CompareMode
        {
            Greater,
            GreaterOrEqual,
            Lesser,
            LesserOrEqual
        }
    }
}
