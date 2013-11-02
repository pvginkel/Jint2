﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Permissions;
using System.Text;
using Jint.Expressions;
using System.Collections;
using Jint.Native;
using System.Security;

namespace Jint.Backend.Interpreted
{

    internal class Visitor : ISyntaxVisitor, IJintVisitor
    {
        private readonly IJintBackend _backend;

        struct ResultInfo
        {
            public JsDictionaryObject BaseObject;
            public JsInstance Result;
        }

        private ITypeResolver _typeResolver;

        public IGlobal Global { get; private set; }
        public JsScope GlobalScope { get; private set; }

        protected Stack<JsScope> Scopes = new Stack<JsScope>();

        private bool _exit;

        public SyntaxNode CurrentStatement { get; set; }

        public JsInstance Returned { get; private set; }
        public bool AllowClr { get; set; }
        public PermissionSet PermissionSet { get; set; }

        private StringBuilder _typeFullName;
        private string _lastIdentifier = String.Empty;

        private ResultInfo _lastResult;

        public JsDictionaryObject CallTarget
        {
            get
            {
                return _lastResult.BaseObject;
            }
        }
        public JsInstance Result
        {
            get
            {
                return _lastResult.Result;
            }
            set
            {
                _lastResult.Result = value;
                _lastResult.BaseObject = null;
            }
        }

        public void SetResult(JsInstance value, JsDictionaryObject baseObject)
        {
            _lastResult.Result = value;
            _lastResult.BaseObject = baseObject;
        }

        public Visitor(Options options, IJintBackend backend)
        {
            _backend = backend;
            _typeResolver = CachedTypeResolver.Default;

            Global = new JsGlobal(_backend, options);
            GlobalScope = new JsScope(Global as JsObject);
            PermissionSet = new PermissionSet(PermissionState.None);

            EnterScope(GlobalScope);
        }

        public Visitor(IGlobal globalObject, JsScope scope)
        {
            if (globalObject == null)
                throw new ArgumentNullException("globalObject");
            if (scope == null)
                throw new ArgumentNullException("scope");

            _typeResolver = CachedTypeResolver.Default;

            Global = globalObject;
            GlobalScope = scope.Global;

            EnterScope(scope);
        }

        public JsScope CurrentScope
        {
            get { return Scopes.Peek(); }
        }

        protected void EnterScope(JsDictionaryObject scope)
        {
            Scopes.Push(new JsScope(CurrentScope, scope));
        }

        protected void EnterScope(JsScope scope)
        {
            Scopes.Push(scope);
        }

        protected void ExitScope()
        {
            Scopes.Pop();
        }

        public void VisitProgram(ProgramSyntax program)
        {
            // initialize local variables, in case the visitor is used multiple times by the same engine
            _typeFullName = null;
            _exit = false;
            _lastIdentifier = String.Empty;

            foreach (var statement in program.Statements)
            {
                CurrentStatement = statement;

                Result = null;
                statement.Accept(this);

                if (_exit)
                {
                    _exit = false;
                    return;
                }
            }
        }

        public void VisitAssignment(AssignmentSyntax statement)
        {
            switch (statement.AssignmentOperator)
            {
                case AssignmentOperator.Assign: statement.Right.Accept(this);
                    break;
                case AssignmentOperator.Multiply: new BinaryExpressionSyntax(BinaryExpressionType.Times, statement.Left, statement.Right).Accept(this);
                    break;
                case AssignmentOperator.Divide: new BinaryExpressionSyntax(BinaryExpressionType.Div, statement.Left, statement.Right).Accept(this);
                    break;
                case AssignmentOperator.Modulo: new BinaryExpressionSyntax(BinaryExpressionType.Modulo, statement.Left, statement.Right).Accept(this);
                    break;
                case AssignmentOperator.Add: new BinaryExpressionSyntax(BinaryExpressionType.Plus, statement.Left, statement.Right).Accept(this);
                    break;
                case AssignmentOperator.Substract: new BinaryExpressionSyntax(BinaryExpressionType.Minus, statement.Left, statement.Right).Accept(this);
                    break;
                case AssignmentOperator.ShiftLeft: new BinaryExpressionSyntax(BinaryExpressionType.LeftShift, statement.Left, statement.Right).Accept(this);
                    break;
                case AssignmentOperator.ShiftRight: new BinaryExpressionSyntax(BinaryExpressionType.RightShift, statement.Left, statement.Right).Accept(this);
                    break;
                case AssignmentOperator.UnsignedRightShift: new BinaryExpressionSyntax(BinaryExpressionType.UnsignedRightShift, statement.Left, statement.Right).Accept(this);
                    break;
                case AssignmentOperator.And: new BinaryExpressionSyntax(BinaryExpressionType.BitwiseAnd, statement.Left, statement.Right).Accept(this);
                    break;
                case AssignmentOperator.Or: new BinaryExpressionSyntax(BinaryExpressionType.BitwiseOr, statement.Left, statement.Right).Accept(this);
                    break;
                case AssignmentOperator.XOr: new BinaryExpressionSyntax(BinaryExpressionType.BitwiseXOr, statement.Left, statement.Right).Accept(this);
                    break;
                default: throw new NotSupportedException();
            }

            JsInstance right = Result;

            MemberAccessSyntax left = statement.Left as MemberAccessSyntax;
            if (left == null)
            {
                left = new MemberAccessSyntax(statement.Left, null);
            }

            Assign(left, right);

            Result = right;
        }

        public void Assign(MemberAccessSyntax left, JsInstance value)
        {
            string propertyName;
            Descriptor d = null;

            if (!(left.Member is IAssignable))
            {
                throw new JintException("The left member of an assignment must be a member");
            }

            EnsureIdentifierIsDefined(value);

            JsDictionaryObject baseObject;

            if (left.Previous != null)
            {
                // if this a property
                left.Previous.Accept(this);
                baseObject = Result as JsDictionaryObject;

                if (baseObject == null)
                    throw new JintException("Attempt to assign to an undefined variable.");
            }
            else
            {
                baseObject = CurrentScope;
                // this a variable
                propertyName = ((IdentifierSyntax)left.Member).Text;

                CurrentScope.TryGetDescriptor(propertyName, out d);
            }

            // now baseObject contains an object or a scope against which to resolve left.Member

            if (left.Member is IdentifierSyntax)
            {
                propertyName = ((IdentifierSyntax)left.Member).Text;

                // Assigning function Name
                //if (value.Class == JsInstance.CLASS_FUNCTION)
                //    ((JsFunction)value).Name = propertyName;

                Result = baseObject[propertyName] = value;
            }
            else
            {
                IndexerSyntax indexer = left.Member as IndexerSyntax;

                // calculate index expression
                indexer.Expression.Accept(this);

                if (baseObject is JsObject)
                {
                    JsObject target = baseObject as JsObject;
                    if (target.Indexer != null)
                    {
                        target.Indexer.Set(target, Result, value);
                        Result = value;
                        return;
                    }
                }

                // Assigning function Name
                //if (value.Class == JsInstance.CLASS_FUNCTION)
                //    ((JsFunction)value).Name = Result.Value.ToString();
                Result = baseObject[Result] = value;
            }
        }

        public void VisitCommaOperator(CommaOperatorSyntax statement)
        {
            foreach (var s in statement.Expressions)
            {
                s.Accept(this);

                if (StopStatementFlow())
                {
                    return;
                }
            }
        }

        public void VisitBlock(BlockSyntax statement)
        {
            SyntaxNode oldStatement = CurrentStatement;
            foreach (var s in statement.Statements)
            {
                CurrentStatement = s;

                Result = null;
                _typeFullName = null;

                s.Accept(this);

                if (StopStatementFlow())
                {
                    return;
                }
            }
            CurrentStatement = oldStatement;
        }

        private ContinueSyntax _continueStatement;

        public void VisitContinue(ContinueSyntax statement)
        {
            _continueStatement = statement;
        }

        private BreakSyntax _breakStatement;

        public void VisitBreak(BreakSyntax statement)
        {
            _breakStatement = statement;
        }

        public void VisitDoWhile(DoWhileSyntax statement)
        {
            do
            {
                statement.Body.Accept(this);

                ResetContinueIfPresent(statement.Label);

                if (StopStatementFlow())
                {
                    if (_breakStatement != null && statement.Label == _breakStatement.Label)
                    {
                        _breakStatement = null;
                    }

                    return;
                }

                statement.Test.Accept(this);

                EnsureIdentifierIsDefined(Result);

            } while (Result.ToBoolean());
        }

        public void VisitEmpty(EmptySyntax statement)
        {
        }

        [DebuggerStepThrough]
        public void VisitExpressionStatement(ExpressionStatementSyntax statement)
        {
            statement.Expression.Accept(this);
        }

        public void VisitForEachIn(ForEachInSyntax statement)
        {
            string identifier;

            if (statement.Initialization is VariableDeclarationSyntax)
                identifier = ((VariableDeclarationSyntax)statement.Initialization).Identifier;
            else if (statement.Initialization is IdentifierSyntax)
                identifier = ((IdentifierSyntax)statement.Initialization).Text;
            else
                throw new NotSupportedException("Only variable declaration are allowed in a for in loop");

            statement.Expression.Accept(this);

            var dictionary = Result as JsDictionaryObject;

            if (Result.Value is IEnumerable)
            {
                foreach (object value in (IEnumerable)Result.Value)
                {
                    CurrentScope[identifier] = Global.WrapClr(value);

                    statement.Body.Accept(this);

                    ResetContinueIfPresent(statement.Label);

                    if (StopStatementFlow())
                    {
                        if (_breakStatement != null && statement.Label == _breakStatement.Label)
                        {
                            _breakStatement = null;
                        }

                        return;
                    }

                    ResetContinueIfPresent(statement.Label);
                }
            }
            else if (dictionary != null)
            {
                List<string> keys = new List<string>(dictionary.GetKeys());

                // Uses a for loop as it might be changed by the inner statements
                for (int i = 0; i < keys.Count; i++)
                {
                    string value = keys[i];

                    CurrentScope[identifier] = Global.StringClass.New(value);

                    statement.Body.Accept(this);

                    ResetContinueIfPresent(statement.Label);

                    if (StopStatementFlow())
                    {
                        if (_breakStatement != null && statement.Label == _breakStatement.Label)
                        {
                            _breakStatement = null;
                        }

                        return;
                    }

                    ResetContinueIfPresent(statement.Label);
                }
            }
            else
            {
                throw new InvalidOperationException("The property can't be enumerated");
            }
        }

        public void VisitWith(WithSyntax statement)
        {
            statement.Expression.Accept(this);

            if (!(Result is JsDictionaryObject))
            {
                throw new JsException(Global.StringClass.New("Invalid expression in 'with' statement"));
            }

            EnterScope((JsDictionaryObject)Result);

            try
            {
                statement.Body.Accept(this);
            }
            finally
            {
                ExitScope();
            }
        }

        public void VisitFor(ForSyntax syntax)
        {
            if (syntax.Initialization != null)
                syntax.Initialization.Accept(this);

            if (syntax.Test != null)
                syntax.Test.Accept(this);
            else
                Result = Global.BooleanClass.New(true);

            EnsureIdentifierIsDefined(Result);

            while (Result.ToBoolean())
            {
                syntax.Body.Accept(this);

                ResetContinueIfPresent(syntax.Label);

                if (StopStatementFlow())
                {
                    if (_breakStatement != null && syntax.Label == _breakStatement.Label)
                    {
                        _breakStatement = null;
                    }

                    return;
                }

                // Goes back in the scopes so that the variables are accessible after the statement
                if (syntax.Increment != null)
                    syntax.Increment.Accept(this);

                if (syntax.Test != null)
                    syntax.Test.Accept(this);
                else
                    Result = Global.BooleanClass.New(true);

            }
        }

        public JsFunction CreateFunction(IFunctionDeclaration functionDeclaration)
        {
            JsFunction f = Global.FunctionClass.New();

            var statementsWithDefaultReturn = new BlockSyntax();

            // injects a default return statement at the end of each function
            statementsWithDefaultReturn.Statements.AddLast(functionDeclaration.Body);
            statementsWithDefaultReturn.Statements.AddLast(new ReturnSyntax(new IdentifierSyntax("undefined")));
            f.Statement = statementsWithDefaultReturn;

            f.Name = functionDeclaration.Name;
            f.Scope = CurrentScope; // copy current scope hierarchy

            f.Arguments = functionDeclaration.Parameters;
            if (HasOption(Options.Strict))
            {
                foreach (string arg in f.Arguments)
                {
                    if (arg == "eval" || arg == "arguments")
                        throw new JsException(Global.StringClass.New("The parameters do not respect strict mode"));
                }
            }

            return f;
        }

        public void VisitFunctionDeclaration(FunctionDeclarationSyntax statement)
        {
            JsFunction f = CreateFunction(statement);
            CurrentScope.DefineOwnProperty(statement.Name, f);
        }

        public void VisitIf(IfSyntax statement)
        {
            statement.Test.Accept(this);

            EnsureIdentifierIsDefined(Result);

            if (Result.ToBoolean())
            {
                statement.Then.Accept(this);
            }
            else
            {
                if (statement.Else != null)
                {
                    statement.Else.Accept(this);
                }
            }
        }

        public void VisitReturn(ReturnSyntax statement)
        {
            if (statement.Expression != null)
            {
                statement.Expression.Accept(this);
                Return(Result);
            }

            _exit = true;
        }

        public JsInstance Return(JsInstance instance)
        {
            Returned = instance;
            return Returned;
        }

        public void VisitSwitch(SwitchSyntax statement)
        {
            CurrentStatement = statement.Expression;

            bool found = false;
            if (statement.Cases != null)
            {
                foreach (var clause in statement.Cases)
                {
                    CurrentStatement = clause.Expression;

                    if (found)
                    {
                        // jumping from one case to the next one
                        clause.Body.Accept(this);
                        if (_exit)
                            break;
                    }
                    else
                    {
                        new BinaryExpressionSyntax(BinaryExpressionType.Equal, (ExpressionSyntax)statement.Expression, clause.Expression).Accept(this);
                        if (Result.ToBoolean())
                        {
                            clause.Body.Accept(this);
                            found = true;
                            if (_exit)
                                break;
                        }
                    }

                    if (_breakStatement != null)
                    {
                        _breakStatement = null;
                        break;
                    }
                }
            }

            if (!found && statement.Default != null)
            {
                statement.Default.Accept(this);

                // handle break statements in default case by clearing it
                if (_breakStatement != null)
                {
                    _breakStatement = null;
                }
            }
        }

        public void VisitThrow(ThrowSyntax statement)
        {
            Result = JsUndefined.Instance;

            if (statement.Expression != null)
            {
                statement.Expression.Accept(this);
            }

            throw new JsException(Result);
        }

        public void VisitTry(TrySyntax statement)
        {
            try
            {
                statement.Body.Accept(this);
            }
            catch (Exception e)
            {
                // there might be no catch statement defined
                if (statement.Catch != null)
                {
                    JsException jsException = e as JsException;

                    if (jsException == null)
                        jsException = new JsException(Global.ErrorClass.New(e.Message));

                    // handle thrown exception assignment to a local variable: catch(e)
                    if (statement.Catch.Identifier != null)
                    {
                        // if catch is called, Result contains the thrown value
                        Assign(new MemberAccessSyntax(new PropertySyntax(statement.Catch.Identifier), null), jsException.Value);
                    }

                    statement.Catch.Body.Accept(this);
                }
                else
                {
                    throw;
                }
            }
            finally
            {

                if (statement.Finally != null)
                {
                    JsObject catchScope = new JsObject();
                    statement.Finally.Body.Accept(this);
                }
            }

        }

        public void VisitVariableDeclaration(VariableDeclarationSyntax statement)
        {
            Result = JsUndefined.Instance;

            // if the right expression is not defined, declare the variable as undefined
            if (statement.Expression != null)
            {
                statement.Expression.Accept(this);
                if (statement.Global)
                {
                    throw new InvalidOperationException("Cant declare a global variable");
                    // todo: where is it from? 
                }
                else
                {
                    if (!CurrentScope.HasOwnProperty(statement.Identifier))
                        CurrentScope.DefineOwnProperty(statement.Identifier, Result);
                    else
                        CurrentScope[statement.Identifier] = Result;
                }
            }
            else
            {
                // a var declaration should not affect existing one
                if (!CurrentScope.HasOwnProperty(statement.Identifier))
                    CurrentScope.DefineOwnProperty(statement.Identifier, JsUndefined.Instance);
            }



        }

        public void VisitWhile(WhileSyntax statement)
        {
            statement.Test.Accept(this);

            EnsureIdentifierIsDefined(Result);

            while (Result.ToBoolean())
            {
                statement.Body.Accept(this);

                ResetContinueIfPresent(statement.Label);

                if (StopStatementFlow())
                {
                    if (_breakStatement != null && statement.Label == _breakStatement.Label)
                    {
                        _breakStatement = null;
                    }

                    return;
                }

                statement.Test.Accept(this);
            }
        }

        public void VisitNew(NewSyntax expression)
        {

            Result = null;

            expression.Expression.Accept(this);

            if (AllowClr && Result == JsUndefined.Instance && _typeFullName != null && _typeFullName.Length > 0 && expression.Generics.Count > 0)
            {
                string typeName = _typeFullName.ToString();
                _typeFullName = new StringBuilder();

                var genericParameters = new Type[expression.Generics.Count];

                try
                {
                    int i = 0;
                    foreach (ExpressionSyntax generic in expression.Generics)
                    {
                        generic.Accept(this);
                        genericParameters[i] = Global.Marshaller.MarshalJsValue<Type>(Result);
                        i++;
                    }
                }
                catch (Exception e)
                {
                    throw new JintException("A type parameter is required", e);
                }

                typeName += "`" + genericParameters.Length;
                Result = Global.Marshaller.MarshalClrValue<Type>(_typeResolver.ResolveType(typeName).MakeGenericType(genericParameters));
            }

            if (Result != null && Result is JsFunction)
            {
                JsFunction function = (JsFunction)Result;

                // Process parameters
                JsInstance[] parameters = new JsInstance[expression.Arguments.Count];

                for (int i = 0; i < expression.Arguments.Count; i++)
                {
                    expression.Arguments[i].Accept(this);
                    parameters[i] = Result;
                }

                Result = function.Construct(parameters, null, this);

                return;
            }
            else
                throw new JsException(Global.ErrorClass.New("Function expected."));
        }

        public void VisitTernary(TernarySyntax expression)
        {
            Result = null;

            // Evaluates the left expression and saves the value
            expression.Test.Accept(this);
            var left = Result;

            Result = null;

            EnsureIdentifierIsDefined(left);

            if (left.ToBoolean())
            {
                // Evaluates the middle expression
                expression.Then.Accept(this);
            }
            else
            {
                // Evaluates the right expression
                expression.Else.Accept(this);
            }
        }

        public JsBoolean Compare(JsInstance x, JsInstance y)
        {
            if (x.IsClr && y.IsClr)
            {
                return Global.BooleanClass.New(x.Value.Equals(y.Value));
            }

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
            { // if both are Objects but then only one is Clrs
                if (x == JsUndefined.Instance)
                {
                    return Global.BooleanClass.True;
                }
                else if (x == JsNull.Instance)
                {
                    return Global.BooleanClass.True;
                }
                else if (x.Type == JsInstance.TypeNumber)
                {
                    if (x.ToNumber() == double.NaN)
                    {
                        return Global.BooleanClass.False;
                    }
                    else if (y.ToNumber() == double.NaN)
                    {
                        return Global.BooleanClass.False;
                    }
                    else if (x.ToNumber() == y.ToNumber())
                    {
                        return Global.BooleanClass.True;
                    }
                    else
                    {
                        return Global.BooleanClass.False;
                    }
                }
                else if (x.Type == JsInstance.TypeString)
                {
                    return Global.BooleanClass.New(x.ToString() == y.ToString());
                }
                else if (x.Type == JsInstance.TypeBoolean)
                {
                    return Global.BooleanClass.New(x.ToBoolean() == y.ToBoolean());
                }
                else if (x.Type == JsInstance.TypeObject)
                {
                    return Global.BooleanClass.New(x == y);
                }
                else
                {
                    return Global.BooleanClass.New(x.Value.Equals(y.Value));
                }
            }
            else if (x == JsNull.Instance && y == JsUndefined.Instance)
            {
                return Global.BooleanClass.True;
            }
            else if (x == JsUndefined.Instance && y == JsNull.Instance)
            {
                return Global.BooleanClass.True;
            }
            else if (x.Type == JsInstance.TypeNumber && y.Type == JsInstance.TypeString)
            {
                return Global.BooleanClass.New(x.ToNumber() == y.ToNumber());
            }
            else if (x.Type == JsInstance.TypeString && y.Type == JsInstance.TypeNumber)
            {
                return Global.BooleanClass.New(x.ToNumber() == y.ToNumber());
            }
            else if (x.Type == JsInstance.TypeBoolean || y.Type == JsInstance.TypeBoolean)
            {
                return Global.BooleanClass.New(x.ToNumber() == y.ToNumber());
            }
            else if (y.Type == JsInstance.TypeObject && (x.Type == JsInstance.TypeString || x.Type == JsInstance.TypeNumber))
            {
                return Compare(x, y.ToPrimitive(Global));
            }
            else if (x.Type == JsInstance.TypeObject && (y.Type == JsInstance.TypeString || y.Type == JsInstance.TypeNumber))
            {
                return Compare(x.ToPrimitive(Global), y);
            }
            else
            {
                return Global.BooleanClass.False;
            }
        }

        public bool CompareTo(JsInstance x, JsInstance y, out int result)
        {
            result = 0;

            if (x.IsClr && y.IsClr)
            {
                IComparable xcmp = x.Value as IComparable;

                if (xcmp == null || y.Value == null || xcmp.GetType() != y.Value.GetType())
                    return false;
                result = xcmp.CompareTo(y.Value);
            }
            else
            {

                Double xnum = x.ToNumber();
                Double ynum = y.ToNumber();

                if (Double.IsNaN(xnum) || Double.IsNaN(ynum))
                    return false;

                if (xnum < ynum)
                    result = -1;
                else if (xnum == ynum)
                    result = 0;
                else
                    result = 1;
            }
            return true;
        }


        public void VisitBinaryExpression(BinaryExpressionSyntax expression)
        {
            // Evaluates the left expression and saves the value
            expression.Left.Accept(this);

            EnsureIdentifierIsDefined(Result);

            JsInstance left = Result;

            //prevents execution of the right hand side if false
            if (expression.Type == BinaryExpressionType.And && !left.ToBoolean())
            {
                Result = left;
                return;
            }

            //prevents execution of the right hand side if true
            if (expression.Type == BinaryExpressionType.Or && left.ToBoolean())
            {
                Result = left;
                return;
            }

            // Evaluates the right expression and saves the value
            expression.Right.Accept(this);

            EnsureIdentifierIsDefined(Result);

            JsInstance right = Result;
            int cmpResult;

            switch (expression.Type)
            {
                case BinaryExpressionType.And:

                    if (left.ToBoolean())
                    {
                        Result = right;
                    }
                    else
                    {
                        Result = Global.BooleanClass.False;
                    }

                    break;

                case BinaryExpressionType.Or:
                    if (left.ToBoolean())
                    {
                        Result = left;
                    }
                    else
                    {
                        Result = right;
                    }

                    break;

                case BinaryExpressionType.Div:
                    var rightNumber = right.ToNumber();
                    var leftNumber = left.ToNumber();

                    if (right == Global.NumberClass["NEGATIVE_INFINITY"] || right == Global.NumberClass["POSITIVE_INFINITY"])
                    {
                        Result = Global.NumberClass.New(0);
                    }
                    else if (rightNumber == 0)
                    {
                        Result = leftNumber > 0 ? Global.NumberClass["POSITIVE_INFINITY"] : Global.NumberClass["NEGATIVE_INFINITY"];
                    }
                    else
                    {
                        Result = Global.NumberClass.New(leftNumber / rightNumber);
                    }
                    break;

                case BinaryExpressionType.Equal:
                    Result = Compare(left, right);

                    break;

                case BinaryExpressionType.Greater:
                    Result = CompareTo(left, right, out cmpResult) && cmpResult > 0 ? Global.BooleanClass.True : Global.BooleanClass.False;
                    break;

                case BinaryExpressionType.GreaterOrEqual:
                    Result = CompareTo(left, right, out cmpResult) && cmpResult >= 0 ? Global.BooleanClass.True : Global.BooleanClass.False;
                    break;

                case BinaryExpressionType.Lesser:
                    Result = CompareTo(left, right, out cmpResult) && cmpResult < 0 ? Global.BooleanClass.True : Global.BooleanClass.False;
                    break;

                case BinaryExpressionType.LesserOrEqual:
                    Result = CompareTo(left, right, out cmpResult) && cmpResult <= 0 ? Global.BooleanClass.True : Global.BooleanClass.False;
                    break;

                case BinaryExpressionType.Minus:
                    Result = Global.NumberClass.New(left.ToNumber() - right.ToNumber());
                    break;

                case BinaryExpressionType.Modulo:
                    if (right == Global.NumberClass["NEGATIVE_INFINITY"] || right == Global.NumberClass["POSITIVE_INFINITY"])
                    {
                        Result = Global.NumberClass["POSITIVE_INFINITY"];
                    }
                    else if (right.ToNumber() == 0)
                    {
                        Result = Global.NumberClass["NaN"];
                    }
                    else
                    {
                        Result = Global.NumberClass.New(left.ToNumber() % right.ToNumber());
                    }
                    break;

                case BinaryExpressionType.NotEqual:

                    Result = Global.BooleanClass.New(!Compare(left, right).ToBoolean());

                    break;

                case BinaryExpressionType.Plus:
                    {
                        JsInstance lprim = left.ToPrimitive(Global);
                        JsInstance rprim = right.ToPrimitive(Global);

                        if (lprim.Class == JsInstance.ClassString || rprim.Class == JsInstance.ClassString)
                            Result = Global.StringClass.New(String.Concat(lprim.ToString(), rprim.ToString()));
                        else
                            Result = Global.NumberClass.New(lprim.ToNumber() + rprim.ToNumber());
                    }
                    break;

                case BinaryExpressionType.Times:
                    Result = Global.NumberClass.New(left.ToNumber() * right.ToNumber());
                    break;

                case BinaryExpressionType.Pow:
                    Result = Global.NumberClass.New(Math.Pow(left.ToNumber(), right.ToNumber()));
                    break;

                case BinaryExpressionType.BitwiseAnd:
                    if (left == JsUndefined.Instance || right == JsUndefined.Instance)
                        Result = Global.NumberClass.New(0);
                    else
                        Result = Global.NumberClass.New(Convert.ToInt64(left.ToNumber()) & Convert.ToInt64(right.ToNumber()));
                    break;

                case BinaryExpressionType.BitwiseOr:
                    if (left == JsUndefined.Instance)
                    {
                        if (right == JsUndefined.Instance)
                            Result = Global.NumberClass.New(1);
                        else
                            Result = Global.NumberClass.New(Convert.ToInt64(right.ToNumber()));
                    }
                    else if (right == JsUndefined.Instance)
                        Result = Global.NumberClass.New(Convert.ToInt64(left.ToNumber()));
                    else
                        Result = Global.NumberClass.New(Convert.ToInt64(left.ToNumber()) | Convert.ToInt64(right.ToNumber()));
                    break;

                case BinaryExpressionType.BitwiseXOr:
                    if (left == JsUndefined.Instance)
                    {
                        if (right == JsUndefined.Instance)
                            Result = Global.NumberClass.New(1);
                        else
                            Result = Global.NumberClass.New(Convert.ToInt64(right.ToNumber()));
                    }
                    else if (right == JsUndefined.Instance)
                        Result = Global.NumberClass.New(Convert.ToInt64(left.ToNumber()));
                    else
                        Result = Global.NumberClass.New(Convert.ToInt64(left.ToNumber()) ^ Convert.ToInt64(right.ToNumber()));
                    break;

                case BinaryExpressionType.Same:
                    Result = JsInstance.StrictlyEquals(Global, left, right);

                    break;

                case BinaryExpressionType.NotSame:
                    new BinaryExpressionSyntax(BinaryExpressionType.Same, expression.Left, expression.Right).Accept(this);
                    Result = Global.BooleanClass.New(!Result.ToBoolean());
                    break;

                case BinaryExpressionType.LeftShift:
                    if (left == JsUndefined.Instance)
                        Result = Global.NumberClass.New(0);
                    else if (right == JsUndefined.Instance)
                        Result = Global.NumberClass.New(Convert.ToInt64(left.ToNumber()));
                    else
                        Result = Global.NumberClass.New(Convert.ToInt64(left.ToNumber()) << Convert.ToUInt16(right.ToNumber()));
                    break;

                case BinaryExpressionType.RightShift:
                    if (left == JsUndefined.Instance)
                        Result = Global.NumberClass.New(0);
                    else if (right == JsUndefined.Instance)
                        Result = Global.NumberClass.New(Convert.ToInt64(left.ToNumber()));
                    else
                        Result = Global.NumberClass.New(Convert.ToInt64(left.ToNumber()) >> Convert.ToUInt16(right.ToNumber()));
                    break;

                case BinaryExpressionType.UnsignedRightShift:
                    if (left == JsUndefined.Instance)
                        Result = Global.NumberClass.New(0);
                    else if (right == JsUndefined.Instance)
                        Result = Global.NumberClass.New(Convert.ToInt64(left.ToNumber()));
                    else
                        Result = Global.NumberClass.New(Convert.ToInt64(left.ToNumber()) >> Convert.ToUInt16(right.ToNumber()));
                    break;

                case BinaryExpressionType.InstanceOf:
                    {
                        var func = right as JsFunction;
                        var obj = left as JsObject;
                        if (func == null)
                            throw new JsException(Global.TypeErrorClass.New("Right argument should be a function: " + expression.Right.ToString()));
                        if (obj == null)
                            throw new JsException(Global.TypeErrorClass.New("Left argument should be an object: " + expression.Left.ToString()));

                        Result = Global.BooleanClass.New(func.HasInstance(obj));
                    }
                    break;

                case BinaryExpressionType.In:
                    if (right is ILiteral)
                    {
                        throw new JsException(Global.ErrorClass.New("Cannot apply 'in' operator to the specified member."));
                    }
                    else
                    {
                        Result = Global.BooleanClass.New(((JsDictionaryObject)right).HasProperty(left));
                    }

                    break;

                default:
                    throw new NotSupportedException("Unkown binary operator");
            }
        }

        public void VisitUnaryExpression(UnaryExpressionSyntax expression)
        {
            MemberAccessSyntax member;

            switch (expression.Type)
            {
                case UnaryExpressionType.TypeOf:

                    expression.Operand.Accept(this);

                    if (Result == null)
                        Result = Global.StringClass.New(JsUndefined.Instance.Type);
                    else if (Result is JsNull)
                        Result = Global.StringClass.New(JsInstance.TypeObject);
                    else if (Result is JsFunction)
                        Result = Global.StringClass.New(JsInstance.TypeofFunction);
                    else
                        Result = Global.StringClass.New(Result.Type);

                    break;

                case UnaryExpressionType.Not:
                    expression.Operand.Accept(this);
                    EnsureIdentifierIsDefined(Result);
                    Result = Global.BooleanClass.New(!Result.ToBoolean());
                    break;

                case UnaryExpressionType.Negate:
                    expression.Operand.Accept(this);
                    EnsureIdentifierIsDefined(Result);
                    Result = Global.NumberClass.New(-Result.ToNumber());
                    break;

                case UnaryExpressionType.Positive:
                    expression.Operand.Accept(this);
                    EnsureIdentifierIsDefined(Result);
                    Result = Global.NumberClass.New(+Result.ToNumber());
                    break;

                case UnaryExpressionType.PostfixPlusPlus:

                    expression.Operand.Accept(this);
                    EnsureIdentifierIsDefined(Result);
                    JsInstance value = Result;

                    member = expression.Operand as MemberAccessSyntax ?? new MemberAccessSyntax(expression.Operand, null);

                    Assign(member, Global.NumberClass.New(value.ToNumber() + 1));

                    Result = value;

                    break;

                case UnaryExpressionType.PostfixMinusMinus:

                    expression.Operand.Accept(this);
                    EnsureIdentifierIsDefined(Result);
                    value = Result;

                    member = expression.Operand as MemberAccessSyntax ?? new MemberAccessSyntax(expression.Operand, null);

                    Assign(member, Global.NumberClass.New(value.ToNumber() - 1));

                    Result = value;

                    break;

                case UnaryExpressionType.PrefixPlusPlus:

                    expression.Operand.Accept(this);
                    EnsureIdentifierIsDefined(Result);
                    value = Global.NumberClass.New(Result.ToNumber() + 1);

                    member = expression.Operand as MemberAccessSyntax ?? new MemberAccessSyntax(expression.Operand, null);
                    Assign(member, value);

                    break;

                case UnaryExpressionType.PrefixMinusMinus:

                    expression.Operand.Accept(this);
                    EnsureIdentifierIsDefined(Result);
                    value = Global.NumberClass.New(Result.ToNumber() - 1);

                    member = expression.Operand as MemberAccessSyntax ?? new MemberAccessSyntax(expression.Operand, null);
                    Assign(member, value);

                    break;

                case UnaryExpressionType.Delete:

                    member = expression.Operand as MemberAccessSyntax;
                    if (member == null)
                        throw new NotImplementedException("delete");
                    member.Previous.Accept(this);
                    EnsureIdentifierIsDefined(Result);
                    value = Result;
                    string propertyName = null;
                    if (member.Member is PropertySyntax)
                        propertyName = ((PropertySyntax)member.Member).Text;
                    if (member.Member is IndexerSyntax)
                    {
                        ((IndexerSyntax)member.Member).Expression.Accept(this);
                        propertyName = Result.ToString();
                    }
                    if (string.IsNullOrEmpty(propertyName))
                        throw new JsException(Global.TypeErrorClass.New());
                    try
                    {
                        ((JsDictionaryObject)value).Delete(propertyName);
                    }
                    catch (JintException)
                    {
                        throw new JsException(Global.TypeErrorClass.New());
                    }
                    Result = value;
                    break;

                case UnaryExpressionType.Void:

                    expression.Operand.Accept(this);
                    Result = JsUndefined.Instance;
                    break;

                case UnaryExpressionType.Inv:

                    expression.Operand.Accept(this);
                    EnsureIdentifierIsDefined(Result);
                    Result = Global.NumberClass.New(0 - Result.ToNumber() - 1);
                    break;

            }
        }

        public void VisitValue(ValueSyntax expression)
        {
            switch (expression.TypeCode)
            {
                case TypeCode.Boolean: Result = Global.BooleanClass.New((bool)expression.Value); break;
                case TypeCode.Int32:
                case TypeCode.Single:
                case TypeCode.Double: Result = Global.NumberClass.New(Convert.ToDouble(expression.Value)); break;
                case TypeCode.String: Result = Global.StringClass.New((string)expression.Value); break;
                default: Result = expression.Value as JsInstance;
                    break;
            }
        }

        public void VisitFunction(FunctionSyntax fe)
        {
            Result = CreateFunction(fe);
        }

        public void VisitClrIdentifier(ClrIdentifierSyntax expression)
        {
            // fallback for an unsupported expression
            throw new NotImplementedException();
        }

        public void VisitMemberAccess(MemberAccessSyntax expression)
        {
            if (expression.Previous != null)
            {
                // the previous part is an property, it will set a callTarget
                expression.Previous.Accept(this);
            }

            expression.Member.Accept(this);

            // Try to evaluate a CLR type
            if (AllowClr && Result == JsUndefined.Instance && _typeFullName != null && _typeFullName.Length > 0)
            {
                EnsureClrAllowed();

                Type type = _typeResolver.ResolveType(_typeFullName.ToString());

                if (type != null)
                {
                    Result = Global.WrapClr(type);
                    _typeFullName = new StringBuilder();
                }
            }
        }

        public void EnsureIdentifierIsDefined(object value)
        {
            if (value == null)
            {
                throw new JsException(Global.ReferenceErrorClass.New(_lastIdentifier + " is not defined"));
            }
        }

        public void VisitIndexer(IndexerSyntax indexer)
        {
            EnsureIdentifierIsDefined(Result);

            JsObject target = (JsObject)Result;

            indexer.Expression.Accept(this);

            if (target.IsClr)
                EnsureClrAllowed();

            if (target.Class == JsInstance.ClassString)
            {
                try
                {
                    SetResult(Global.StringClass.New(target.ToString()[Convert.ToInt32(Result.ToNumber())].ToString()), target);
                    return;
                }
                catch
                {
                    // if an error occured, try to access the index as a member
                }
            }

            if (target.Indexer != null)
                SetResult(target.Indexer.Get(target, Result), target);
            else
                SetResult(target[Result], target);
        }

        public void VisitMethodCall(MethodCallSyntax methodCall)
        {
            var that = CallTarget;
            var target = Result;

            if (target == JsUndefined.Instance || Result == null)
            {
                if (String.IsNullOrEmpty(_lastIdentifier))
                {
                    throw new JsException(Global.TypeErrorClass.New("Method isn't defined"));
                }
            }

            Type[] genericParameters = null;

            if (AllowClr && methodCall.Generics.Count > 0)
            {
                genericParameters = new Type[methodCall.Generics.Count];

                try
                {
                    var i = 0;
                    foreach (var generic in methodCall.Generics)
                    {
                        generic.Accept(this);
                        genericParameters[i] = Global.Marshaller.MarshalJsValue<Type>(Result);
                        i++;
                    }
                }
                catch (Exception e)
                {
                    throw new JintException("A type parameter is required", e);
                }
            }

            #region Evaluates parameters
            var parameters = new JsInstance[methodCall.Arguments.Count];

            if (methodCall.Arguments.Count > 0)
            {

                for (int j = 0; j < methodCall.Arguments.Count; j++)
                {
                    methodCall.Arguments[j].Accept(this);
                    parameters[j] = Result;
                }

            }
            #endregion

            var function = target as JsFunction;
            if (function != null)
            {
                Returned = JsUndefined.Instance;

                var original = new JsInstance[parameters.Length];
                parameters.CopyTo(original, 0);

                ExecuteFunction(function, that, parameters, genericParameters);

                for (var i = 0; i < original.Length; i++)
                    if (original[i] != parameters[i])
                    {
                        if (methodCall.Arguments[i] is MemberAccessSyntax && ((MemberAccessSyntax)methodCall.Arguments[i]).Member is IAssignable)
                        {
                            Assign((MemberAccessSyntax)methodCall.Arguments[i], parameters[i]);
                        }
                        else if (methodCall.Arguments[i] is IdentifierSyntax)
                        {
                            Assign(new MemberAccessSyntax(methodCall.Arguments[i], null), parameters[i]);
                        }
                    }

                Result = Returned;
                Returned = JsUndefined.Instance;
            }
            else
            {
                throw new JsException(Global.ErrorClass.New("Function expected."));
            }

        }

        public void ExecuteFunction(JsFunction function, JsDictionaryObject that, JsInstance[] parameters)
        {
            ExecuteFunction(function, that, parameters, null);
        }

        public void ExecuteFunction(JsFunction function, JsDictionaryObject that, JsInstance[] parameters, Type[] genericParameters)
        {
            if (function == null)
            {
                return;
            }

            // ecma chapter 10.
            // TODO: move creation of the activation object to the JsFunction
            // create new argument object and instantinate arguments into it
            JsArguments args = new JsArguments(Global, function, parameters);

            // create new activation object and copy instantinated arguments to it
            // Activation should be before the function.Scope hierarchy
            JsScope functionScope = new JsScope(function.Scope ?? GlobalScope);

            for (int i = 0; i < function.Arguments.Count; i++)
                if (i < parameters.Length)
                    functionScope.DefineOwnProperty(
                        new LinkedDescriptor(
                            functionScope,
                            function.Arguments[i],
                            args.GetDescriptor(i.ToString()),
                            args
                        )
                    );
                else
                    functionScope.DefineOwnProperty(
                        new ValueDescriptor(
                            functionScope,
                            function.Arguments[i],
                            JsUndefined.Instance
                        )
                    );

            // define arguments variable
            if (HasOption(Options.Strict))
                functionScope.DefineOwnProperty(JsScope.Arguments, args);
            else
                args.DefineOwnProperty(JsScope.Arguments, args);

            // set this variable
            if (that != null)
                functionScope.DefineOwnProperty(JsScope.This, that);
            else
                functionScope.DefineOwnProperty(JsScope.This, that = Global as JsObject);

            // enter activation object
            EnterScope(functionScope);

            try
            {
                if (AllowClr)
                {
                    PermissionSet.PermitOnly();
                }

                if (AllowClr && genericParameters != null && genericParameters.Length > 0)
                {
                    Result = function.Execute(this, that, parameters, genericParameters);
                }
                else
                {
                    Result = function.Execute(this, that, parameters);
                }

                // Resets the return flag
                if (_exit)
                {
                    _exit = false;
                }
            }
            finally
            {
                // return to previous execution state
                ExitScope();

                if (AllowClr)
                {
                    CodeAccessPermission.RevertPermitOnly();
                }
            }
        }

        private bool HasOption(Options options)
        {
            return Global.HasOption(options);
        }


        public void VisitProperty(PropertySyntax expression)
        {
            // save base of current expression
            var callTarget = Result as JsDictionaryObject;

            // this check is disabled becouse it prevents Clr names to resolve
            //if ((callTarget) == null || callTarget == JsUndefined.Instance || callTarget == JsNull.Instance)
            //{
            //    throw new JsException( Global.TypeErrorClass.New( String.Format("An object is required: {0} while resolving property {1}", lastIdentifier, expression.Text) ) );
            //}

            Result = null;

            string propertyName = _lastIdentifier = expression.Text;

            JsInstance result = null;

            if (callTarget != null && callTarget.TryGetProperty(propertyName, out result))
            {
                SetResult(result, callTarget);
                return;
            }

            if (Result == null && _typeFullName != null && _typeFullName.Length > 0)
            {
                _typeFullName.Append('.').Append(propertyName);
            }

            SetResult(JsUndefined.Instance, callTarget);
        }

        public void VisitPropertyDeclaration(PropertyDeclarationSyntax expression)
        {
            // previous result was the object in which we need to define a property
            var target = Result as JsDictionaryObject;

            switch (expression.Mode)
            {
                case PropertyExpressionType.Data:
                    expression.Expression.Accept(this);
                    target.DefineOwnProperty(new ValueDescriptor(target, expression.Name, Result));
                    break;
                case PropertyExpressionType.Get:
                case PropertyExpressionType.Set:
                    JsFunction get = null, set = null;
                    if (expression.GetExpression != null)
                    {
                        expression.GetExpression.Accept(this);
                        get = (JsFunction)Result;
                    }
                    if (expression.SetExpression != null)
                    {
                        expression.SetExpression.Accept(this);
                        set = (JsFunction)Result;
                    }
                    target.DefineOwnProperty(new PropertyDescriptor(Global, target, expression.Name) { GetFunction = get, SetFunction = set, Enumerable = true });
                    break;
                default:
                    break;
            }
        }

        public void VisitIdentifier(IdentifierSyntax expression)
        {
            Result = null;

            string propertyName = _lastIdentifier = expression.Text;

            Descriptor result = null;
            if (CurrentScope.TryGetDescriptor(propertyName, out result))
            {
                if (!result.IsReference)
                    Result = result.Get(CurrentScope);
                else
                {
                    LinkedDescriptor r = result as LinkedDescriptor;
                    SetResult(r.Get(CurrentScope), r.TargetObject);
                }

                if (Result != null)
                    return;
            }

            if (propertyName == "null")
            {
                Result = JsNull.Instance;
            }

            if (propertyName == "undefined")
            {
                Result = JsUndefined.Instance;
            }

            // Try to record full path in case it's a type
            if (Result == null)
            {
                if (_typeFullName == null)
                {
                    _typeFullName = new StringBuilder();
                }

                _typeFullName.Append(propertyName);
            }
        }

        private void EnsureClrAllowed()
        {
            if (!AllowClr)
            {
                throw new SecurityException("Use of Clr is not allowed");
            }
        }

        public void VisitJsonExpression(JsonExpressionSyntax json)
        {
            JsObject instance = Global.ObjectClass.New();

            foreach (var item in json.Values)
            {
                Result = instance;
                item.Value.Accept(this);
            }

            Result = instance;
        }

        /// <summary>
        /// Called by a loop to stop the "continue" keyword escalation
        /// </summary>
        protected void ResetContinueIfPresent(string label)
        {
            if (_continueStatement != null && _continueStatement.Label == label)
            {
                _continueStatement = null;
            }
        }

        protected bool StopStatementFlow()
        {
            return _exit ||
            _breakStatement != null ||
            _continueStatement != null;
        }

        public void VisitArrayDeclaration(ArrayDeclarationSyntax expression)
        {
            var array = Global.ArrayClass.New();

            // Process parameters
            JsInstance[] parameters = new JsInstance[expression.Parameters.Count];

            for (int i = 0; i < expression.Parameters.Count; i++)
            {
                expression.Parameters[i].Accept(this);
                array[i.ToString()] = Result;
            }

            Result = array;
        }

        public void VisitRegexp(RegexpSyntax expression)
        {
            Result = Global.RegExpClass.New(expression.Regexp, expression.Options.Contains("g"), expression.Options.Contains("i"), expression.Options.Contains("m"));
        }


        #region IDeserializationCallback Members

        public void OnDeserialization(object sender)
        {
            /*
            methodInvoker = new CachedMethodInvoker(this);
            propertyGetter = new CachedReflectionPropertyGetter(methodInvoker);
            constructorInvoker = new CachedConstructorInvoker(methodInvoker);
            
            fieldGetter = new CachedReflectionFieldGetter(methodInvoker);
            */
            _typeResolver = new CachedTypeResolver();
        }

        #endregion

    }
}
