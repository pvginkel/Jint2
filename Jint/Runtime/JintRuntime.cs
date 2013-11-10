using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security;
using System.Text;
using Jint.Backend.Dlr;
using Jint.Expressions;
using Jint.Native;

namespace Jint.Runtime
{
    public class JintRuntime
    {
        internal const string GlobalName = "Global";
        internal const string GlobalScopeName = "GlobalScope";

        private readonly Options _options;
        private readonly bool _allowClr;
        private readonly PermissionSet _permissionSet;
        private readonly bool _isStrict;

        public JsScope GlobalScope { get; private set; }
        public JsGlobal Global { get; private set; }

        public JintRuntime(IJintBackend backend, Options options, bool allowClr, PermissionSet permissionSet)
        {
            if (backend == null)
                throw new ArgumentNullException("backend");

            _options = options;
            _allowClr = allowClr;
            _permissionSet = permissionSet;
            _isStrict = _options.HasFlag(Options.Strict);

            var global = new JsGlobal(backend, options);

            Global = global;
            GlobalScope = new JsScope(global);

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
        }

        public JsFunction CreateFunction(string name, DlrFunctionDelegate function, object closure, string[] parameters)
        {
            var result = new DlrFunction(function, Global.FunctionClass.PrototypeProperty, closure, this)
            {
                Name = name,
                Arguments = new List<string>(parameters ?? new string[0])
            };

            result.PrototypeProperty = Global.ObjectClass.New(function);

            return result;
        }

        public JsInstance ExecuteFunction(JsInstance that, JsInstance target, JsInstance[] parameters)
        {
            Type[] genericParameters = null;

            //if (_allowClr && methodCall.Generics.Count > 0)
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

            if (parameters == null)
                parameters = new JsInstance[0];

            var original = new JsInstance[parameters.Length];

            parameters.CopyTo(original, 0);

            var returned = ExecuteFunctionCore(function, (JsObject)that, parameters, genericParameters);

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

            return returned.Result;
        }

        public JsFunctionResult ExecuteFunctionCore(JsFunction function, JsDictionaryObject that, JsInstance[] parameters, Type[] genericParameters)
        {
/*
            if (function == null)
                return null;

            // ecma chapter 10.
            // TODO: move creation of the activation object to the JsFunction
            // create new argument object and instantinate arguments into it
            var args = new JsArguments(Global, function, parameters);

            // create new activation object and copy instantinated arguments to it
            // Activation should be before the function.Scope hierarchy
            var functionScope = new JsScope(function.Scope ?? GlobalScope);

            for (int i = 0; i < function.Arguments.Count; i++)
            {
                if (i < parameters.Length)
                {
                    functionScope.DefineOwnProperty(
                        new LinkedDescriptor(
                            functionScope,
                            function.Arguments[i],
                            args.GetDescriptor(i.ToString()),
                            args
                        )
                    );
                }
                else
                {
                    functionScope.DefineOwnProperty(
                        new ValueDescriptor(
                            functionScope,
                            function.Arguments[i],
                            JsUndefined.Instance
                        )
                    );
                }
            }

            // define arguments variable
            if (_isStrict)
                functionScope.DefineOwnProperty(JsScope.Arguments, args);
            else
                args.DefineOwnProperty(JsScope.Arguments, args);

            if (that == null)
                that = Global;

            functionScope.DefineOwnProperty(JsScope.This, that);

            try
            {
                if (_allowClr)
                    _permissionSet.PermitOnly();

                //var previousScope = _program.EnterScope(functionScope);

                try
                {
                    if (!_allowClr || (genericParameters != null && genericParameters.Length == 0))
                        genericParameters = null;

                    var result = function.Execute(Global, that, parameters);

                    return result.Result;
                }
                finally
                {
                    //_program.ExitScope(previousScope);
                }
            }
            finally
            {
                if (_allowClr)
                    CodeAccessPermission.RevertPermitOnly();
            }
*/
            if (function == null)
                throw new ArgumentNullException("function");

            try
            {
                if (_allowClr)
                    _permissionSet.PermitOnly();

                if (!_allowClr)
                    genericParameters = null;

                return function.Execute(Global, that ?? Global, parameters ?? JsInstance.Empty, genericParameters);
            }
            finally
            {
                if (_allowClr)
                    CodeAccessPermission.RevertPermitOnly();
            }
        }

        public static bool Compare(IGlobal global, JsInstance left, JsInstance right, ExpressionType expressionType)
        {
            if (
                expressionType == ExpressionType.Equal ||
                expressionType == ExpressionType.NotEqual
            )
                return CompareEquality(global, left, right, expressionType);
            else
                return CompareRange(global, left, right, expressionType);
        }

        private static bool CompareRange(IGlobal global, JsInstance left, JsInstance right, ExpressionType operation)
        {
            double result;

            if (left.IsClr && right.IsClr)
            {
                var comparer = left.Value as IComparable;

                if (comparer == null || right.Value == null || comparer.GetType() != right.Value.GetType())
                    return false;

                result = comparer.CompareTo(right.Value);
            }
            else
            {

                double leftNumber = left.ToNumber();
                double rightNumber = right.ToNumber();

                if (Double.IsNaN(leftNumber) || Double.IsNaN(rightNumber))
                    return false;

                if (leftNumber < rightNumber)
                    result = -1;
                else if (leftNumber > rightNumber)
                    result = 1;
                else
                    result = 0;
            }

            switch (operation)
            {
                case ExpressionType.GreaterThan: return result > 0;
                case ExpressionType.GreaterThanOrEqual: return result >= 0;
                case ExpressionType.LessThan: return result < 0;
                case ExpressionType.LessThanOrEqual: return result <= 0;
                default: throw new InvalidOperationException();
            }
        }

        private static bool CompareEquality(IGlobal global, JsInstance left, JsInstance right, ExpressionType expressionType)
        {
            bool result;

            if (left.IsClr && right.IsClr)
            {
                result = left.Value.Equals(right.Value);
            }
            else if (left.Type == right.Type)
            {
                // if both are Objects but then only one is Clrs
                if (left == JsUndefined.Instance)
                {
                    result = true;
                }
                else if (left == JsNull.Instance)
                {
                    result = true;
                }
                else if (left.Type == JsInstance.TypeNumber)
                {
                    if (left.ToNumber() == double.NaN)
                    {
                        result = false;
                    }
                    else if (right.ToNumber() == double.NaN)
                    {
                        result = false;
                    }
                    else if (left.ToNumber() == right.ToNumber())
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                }
                else if (left.Type == JsInstance.TypeString)
                {
                    result = left.ToString() == right.ToString();
                }
                else if (left.Type == JsInstance.TypeBoolean)
                {
                    result = left.ToBoolean() == right.ToBoolean();
                }
                else if (left.Type == JsInstance.TypeObject)
                {
                    result = left == right;
                }
                else
                {
                    result = left.Value.Equals(right.Value);
                }
            }
            else if (left == JsNull.Instance && right == JsUndefined.Instance)
            {
                result = true;
            }
            else if (left == JsUndefined.Instance && right == JsNull.Instance)
            {
                result = true;
            }
            else if (left.Type == JsInstance.TypeNumber && right.Type == JsInstance.TypeString)
            {
                result = left.ToNumber() == right.ToNumber();
            }
            else if (left.Type == JsInstance.TypeString && right.Type == JsInstance.TypeNumber)
            {
                result = left.ToNumber() == right.ToNumber();
            }
            else if (left.Type == JsInstance.TypeBoolean || right.Type == JsInstance.TypeBoolean)
            {
                result = left.ToNumber() == right.ToNumber();
            }
            else if (right.Type == JsInstance.TypeObject && (left.Type == JsInstance.TypeString || left.Type == JsInstance.TypeNumber))
            {
                return Compare(global, left, right.ToPrimitive(global), expressionType);
            }
            else if (left.Type == JsInstance.TypeObject && (right.Type == JsInstance.TypeString || right.Type == JsInstance.TypeNumber))
            {
                return Compare(global, left.ToPrimitive(global), right, expressionType);
            }
            else
            {
                result = false;
            }

            switch (expressionType)
            {
                case ExpressionType.Equal:
                    return result;

                case ExpressionType.NotEqual:
                    return !result;

                default:
                    throw new InvalidOperationException();
            }
        }

        public JsInstance BinaryOperation(JsInstance left, JsInstance right, BinaryExpressionType operation)
        {
            switch (operation)
            {
                case BinaryExpressionType.LeftShift:
                    if (left == JsUndefined.Instance)
                        return Global.NumberClass.New(0);
                    if (right == JsUndefined.Instance)
                        return Global.NumberClass.New(Convert.ToInt64(left.ToNumber()));
                    return Global.NumberClass.New(Convert.ToInt64(left.ToNumber()) << Convert.ToUInt16(right.ToNumber()));

                case BinaryExpressionType.RightShift:
                    if (left == JsUndefined.Instance)
                        return Global.NumberClass.New(0);
                    if (right == JsUndefined.Instance)
                        return Global.NumberClass.New(Convert.ToInt64(left.ToNumber()));
                    return Global.NumberClass.New(Convert.ToInt64(left.ToNumber()) >> Convert.ToUInt16(right.ToNumber()));

                case BinaryExpressionType.UnsignedRightShift:
                    if (left == JsUndefined.Instance)
                        return Global.NumberClass.New(0);
                    if (right == JsUndefined.Instance)
                        return Global.NumberClass.New(Convert.ToInt64(left.ToNumber()));
                    return Global.NumberClass.New(Convert.ToInt64(left.ToNumber()) >> Convert.ToUInt16(right.ToNumber()));

                case BinaryExpressionType.Same:
                    return JsInstance.StrictlyEquals(Global, left, right);

                case BinaryExpressionType.NotSame:
                    var result = JsInstance.StrictlyEquals(Global, left, right);
                    return Global.BooleanClass.New(!result.ToBoolean());

                case BinaryExpressionType.In:
                    if (right is ILiteral)
                        throw new JsException(Global.ErrorClass.New("Cannot apply 'in' operator to the specified member."));

                    return Global.BooleanClass.New(((JsDictionaryObject)right).HasProperty(left));

                case BinaryExpressionType.InstanceOf:
                    var function = right as JsFunction;
                    var obj = left as JsObject;

                    if (function == null)
                        throw new JsException(Global.TypeErrorClass.New("Right argument should be a function"));
                    if (obj == null)
                        throw new JsException(Global.TypeErrorClass.New("Left argument should be an object"));

                    return Global.BooleanClass.New(function.HasInstance(obj));

                case BinaryExpressionType.Plus:
                    var leftPrimitive = left.ToPrimitive(Global);
                    var rightPrimitive = right.ToPrimitive(Global);

                    if (leftPrimitive is JsString || rightPrimitive is JsString)
                        return Global.StringClass.New(String.Concat(leftPrimitive.ToString(), rightPrimitive.ToString()));

                    return Global.NumberClass.New(leftPrimitive.ToNumber() + rightPrimitive.ToNumber());

                case BinaryExpressionType.Div:
                    var rightNumber = right.ToNumber();
                    var leftNumber = left.ToNumber();

                    if (right == Global.NumberClass["NEGATIVE_INFINITY"] || right == Global.NumberClass["POSITIVE_INFINITY"])
                        return Global.NumberClass.New(0);

                    if (rightNumber == 0)
                        return leftNumber > 0 ? Global.NumberClass["POSITIVE_INFINITY"] : Global.NumberClass["NEGATIVE_INFINITY"];

                    return Global.NumberClass.New(leftNumber / rightNumber);

                default:
                    throw new NotImplementedException();
            }
        }

        public JsInstance UnaryOperation(JsInstance operand, UnaryExpressionType operation)
        {
            switch (operation)
            {
                case UnaryExpressionType.Inv:
                    return Global.NumberClass.New(0 - operand.ToNumber() - 1);

                case UnaryExpressionType.TypeOf:
                    if (operand == null)
                        return Global.StringClass.New(JsUndefined.Instance.Type);
                    if (operand is JsNull)
                        return Global.StringClass.New(JsInstance.TypeObject);
                    if (operand is JsFunction)
                        return Global.StringClass.New(JsInstance.TypeofFunction);
                    return Global.StringClass.New(operand.Type);

                default:
                    throw new NotImplementedException();
            }
        }

        public IEnumerable<JsInstance> GetForEachKeys(JsInstance obj)
        {
            if (obj == null)
                yield break;

            var values = obj.Value as IEnumerable;

            if (values != null)
            {
                foreach (object value in values)
                {
                    yield return Global.WrapClr(value);
                }
            }
            else
            {
                foreach (string key in new List<string>(((JsDictionaryObject)obj).GetKeys()))
                {
                    yield return Global.StringClass.New(key);
                }
            }
        }

        public JsInstance WrapException(Exception exception)
        {
            if (exception == null)
                throw new ArgumentNullException("exception");

            var jsException =
                exception as JsException ??
                new JsException(Global.ErrorClass.New(exception.Message));

            return jsException.Value;
        }

        public JsInstance New(JsInstance target, JsInstance[] arguments, JsInstance[] generics)
        {
            //if (AllowClr && function  == JsUndefined.Instance && typeFullname != null && typeFullname.Length > 0 && expression.Generics.Count > 0)
            //{
            //    string typeName = typeFullname.ToString();
            //    typeFullname = new StringBuilder();

            //    var genericParameters = new Type[expression.Generics.Count];

            //    try
            //    {
            //        int i = 0;
            //        foreach (Expression generic in expression.Generics)
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

            //    typeName += "`" + genericParameters.Length;
            //    Result = Global.Marshaller.MarshalClrValue<Type>(typeResolver.ResolveType(typeName).MakeGenericType(genericParameters));
            //}

            var function = target as JsFunction;
            if (function == null)
                throw new JsException(Global.ErrorClass.New("Function expected."));

            return function.Construct(arguments, null, Global);
        }
    }
}
