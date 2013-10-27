using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpSyntax;
using Jint.Expressions;
using Jint.Native;

namespace Jint.Backend.Compiled
{
    internal partial class Visitor : IStatementVisitor, IJintVisitor
    {
        private readonly Options _options;
        private readonly CompiledBackend _backend;
        private ClassDeclarationSyntax _class;
        private BlockSyntax _main;
        private BlockSyntax _block;
        private SyntaxNode _result;
        private Expression _callTarget;
        private string _lastIdentifier;
        private int _nextAnonymousVariableId = 1;
        private int _nextAnonymousFunctionId = 1;
        private int _nextAnonymousClassId = 1;
        private ScopeBuilder _scopeBuilder;
        private ScopeBuilder _globalScopeBuilder;

        public JsInstance Result
        {
            get { throw new InvalidOperationException(); }
            set { throw new InvalidOperationException(); }
        }

        public JsDictionaryObject CallTarget
        {
            get { throw new InvalidOperationException(); }
        }

        public IGlobal Global
        {
            get { return _backend.Global; }
        }

        public JsInstance Returned
        {
            get { throw new InvalidOperationException(); }
        }

        public Visitor(Options options, CompiledBackend backend)
        {
            if (backend == null)
                throw new ArgumentNullException("backend");

            _options = options;
            _backend = backend;
        }

        public void Reset()
        {
            _class = Syntax.ClassDeclaration(
                identifier: "Program",
                modifiers: Modifiers.Public,
                baseList: Syntax.BaseList("JintProgram")
            );

            _block = _main = Syntax.Block();

            _globalScopeBuilder = _scopeBuilder = new ScopeBuilder(this, null, _block);

            _class.Members.Add(Syntax.ConstructorDeclaration(
                identifier: "Program",
                parameterList: Syntax.ParameterList(
                    Syntax.Parameter(
                        identifier: "backend",
                        type: "IJintBackend"
                    ),
                    Syntax.Parameter(
                        identifier: "options",
                        type: "Options"
                    )   
                ),
                initializer: Syntax.ConstructorInitializer(
                    ThisOrBase.Base,
                    Syntax.ArgumentList(
                        Syntax.Argument(Syntax.IdentifierName("backend")),
                        Syntax.Argument(Syntax.IdentifierName("options"))
                    )
                ),
                body: Syntax.Block(),
                modifiers: Modifiers.Public
            ));

            _class.Members.Add(Syntax.MethodDeclaration(
                returnType: "JsInstance",
                identifier: "Main",
                parameterList: Syntax.ParameterList(),
                body: _main,
                modifiers: Modifiers.Public | Modifiers.Override
            ));
        }

        public void Close()
        {
            _scopeBuilder.Build();
        }

        public ClassDeclarationSyntax GetClassDeclaration()
        {
            // Make sure we return something.

            _main.Statements.Add(Syntax.ReturnStatement(Syntax.IdentifierName("null")));

            return _class;
        }

        public JsInstance Return(JsInstance result)
        {
            _backend.Returned = result;
            return result;
        }

        public void ExecuteFunction(JsFunction function, JsDictionaryObject @this, JsInstance[] parameters)
        {
            throw new NotImplementedException();
        }

        public void Visit(Program expression)
        {
            _lastIdentifier = null;

            foreach (string variableName in expression.DeclaredVariables)
            {
                _scopeBuilder.EnsureVariable(variableName);
            }

            foreach (var statement in expression.Statements)
            {
                statement.Accept(this);

                _block.Statements.Add(MakeStatement(_result));
            }
        }

        private StatementSyntax MakeStatement(SyntaxNode result)
        {
            var statement = result as StatementSyntax;

            if (statement != null)
                return statement;

            return Syntax.ExpressionStatement((ExpressionSyntax)result);
        }

        private string GetNextAnonymousVariableName()
        {
            return "__anonymousLocalVariable" + _nextAnonymousVariableId++;
        }

        private string GetNextAnonymousFunctionName()
        {
            return "__AnonymousFunction" + _nextAnonymousFunctionId++;
        }

        private string GetNextAnonymousClassName()
        {
            return "__AnonymousClass" + _nextAnonymousClassId++;
        }

        public void Visit(AssignmentExpression statement)
        {
            if (statement.AssignmentOperator == AssignmentOperator.Assign)
            {
                statement.Right.Accept(this);
            }
            else
            {
                BinaryExpressionType op;

                switch (statement.AssignmentOperator)
                {
                    case AssignmentOperator.Multiply: op = BinaryExpressionType.Times; break;
                    case AssignmentOperator.Divide: op = BinaryExpressionType.Div; break;
                    case AssignmentOperator.Modulo: op = BinaryExpressionType.Modulo; break;
                    case AssignmentOperator.Add: op = BinaryExpressionType.Plus; break;
                    case AssignmentOperator.Substract: op = BinaryExpressionType.Minus; break;
                    case AssignmentOperator.ShiftLeft: op = BinaryExpressionType.LeftShift; break;
                    case AssignmentOperator.ShiftRight: op = BinaryExpressionType.RightShift; break;
                    case AssignmentOperator.UnsignedRightShift: op = BinaryExpressionType.UnsignedRightShift; break;
                    case AssignmentOperator.And: op = BinaryExpressionType.BitwiseAnd; break;
                    case AssignmentOperator.Or: op = BinaryExpressionType.BitwiseOr; break;
                    case AssignmentOperator.XOr: op = BinaryExpressionType.BitwiseXOr; break;
                    default: throw new InvalidOperationException();
                }

                var binaryExpression = new BinaryExpression(op, statement.Left, statement.Right);

                binaryExpression.Accept(this);
            }

            var right = _result;
            var left = statement.Left as MemberExpression ?? new MemberExpression(statement.Left, null);

            if (left.Previous == null)
            {
                string memberName = SanitizeName(((Identifier)left.Member).Text);
                string alias = _scopeBuilder.FindAndCreateAlias(memberName);

                // If we're assigning a variable that isn't known in any scope,
                // it's for the global scope.

                if (alias == null)
                {
                    _globalScopeBuilder.EnsureVariable(memberName);
                    alias = _scopeBuilder.FindAndCreateAlias(memberName);
                }

                _result = Syntax.BinaryExpression(
                    BinaryOperator.Equals,
                    Syntax.MemberAccessExpression(
                        Syntax.ParseName(alias),
                        memberName
                    ),
                    (ExpressionSyntax)right
                );
            }
            else
            {
                left.Previous.Accept(this);

                var baseObject = _result;

                if (left.Member is Identifier)
                {
                    _result = Syntax.InvocationExpression(
                        Syntax.ParseName("AssignMember"),
                        Syntax.ArgumentList(
                            Syntax.Argument((ExpressionSyntax)baseObject),
                            Syntax.Argument(Syntax.LiteralExpression(((Identifier)left.Member).Text)),
                            Syntax.Argument((ExpressionSyntax)right)
                        )
                    );
                }
                else
                {
                    left.Previous.Accept(this);

                    ((Indexer)left.Member).Index.Accept(this);

                    _result = Syntax.InvocationExpression(
                        Syntax.ParseName("AssignIndexer"),
                        Syntax.ArgumentList(
                            Syntax.Argument((ExpressionSyntax)baseObject),
                            Syntax.Argument((ExpressionSyntax)_result),
                            Syntax.Argument((ExpressionSyntax)right)
                        )
                    );
                }
            }
        }

        private string SanitizeName(string name)
        {
            var sb = new StringBuilder();

            foreach (char c in name)
            {
                if (c == '$')
                    sb.Append("__DOLLAR__");
                else
                    sb.Append(c);
            }

            return sb.ToString();
        }

        public void Visit(BlockStatement expression)
        {
            var block = Syntax.Block();

            foreach (string variableName in expression.DeclaredVariables)
            {
                _scopeBuilder.EnsureVariable(variableName);
            }

            foreach (var statement in expression.Statements)
            {
                statement.Accept(this);

                block.Statements.Add(MakeStatement(_result));
            }

            _result = block;
        }

        public void Visit(BreakStatement expression)
        {
            _result = Syntax.BreakStatement();
        }

        public void Visit(ContinueStatement expression)
        {
            throw new NotImplementedException();
        }

        public void Visit(DoWhileStatement expression)
        {
            throw new NotImplementedException();
        }

        public void Visit(EmptyStatement expression)
        {
            _result = Syntax.EmptyStatement();
        }

        public void Visit(ExpressionStatement expression)
        {
            expression.Expression.Accept(this);
        }

        public void Visit(ForEachInStatement expression)
        {
            throw new NotImplementedException();
        }

        public void Visit(ForStatement statement)
        {
            SyntaxNode condition = null;
            SyntaxNode increment = null;
            SyntaxNode body = null;

            var block = Syntax.Block();

            if (statement.InitialisationStatement != null)
            {
                statement.InitialisationStatement.Accept(this);
                block.Statements.Add(MakeStatement(_result));
            }

            if (statement.ConditionExpression != null)
            {
                statement.ConditionExpression.Accept(this);
                condition = _result;
            }

            if (statement.IncrementExpression != null)
            {
                statement.IncrementExpression.Accept(this);
                increment = _result;
            }

            if (statement.Statement != null)
            {
                statement.Statement.Accept(this);
                body = _result;
            }

            block.Statements.Add(Syntax.ForStatement(
                condition: Syntax.InvocationExpression(
                    Syntax.MemberAccessExpression(
                        (ExpressionSyntax)condition,
                        "ToBoolean"
                    ),
                    Syntax.ArgumentList()
                ),
                incrementors: new[] { (ExpressionSyntax)increment },
                statement: MakeStatement(body)
            ));

            _result = block;

            //EnsureIdentifierIsDefined(Result);

            //while (Result.ToBoolean())
            //{
            //    statement.Statement.Accept(this);

            //    ResetContinueIfPresent(statement.Label);

            //    if (StopStatementFlow())
            //    {
            //        if (_breakStatement != null && statement.Label == _breakStatement.Label)
            //        {
            //            _breakStatement = null;
            //        }

            //        return;
            //    }

            //    // Goes back in the scopes so that the variables are accessible after the statement
            //    if (statement.IncrementExpression != null)
            //        statement.IncrementExpression.Accept(this);

            //    if (statement.ConditionExpression != null)
            //        statement.ConditionExpression.Accept(this);
            //    else
            //        Result = Global.BooleanClass.New(true);

            //}
        }

        public void Visit(FunctionDeclarationStatement statement)
        {
            var functionName = DeclareFunction(statement);

            if ((_options & Options.Strict) != 0)
            {
                foreach (string arg in statement.Parameters)
                {
                    if (arg == "eval" || arg == "arguments")
                        throw new JsException(Global.StringClass.New("The parameters do not respect strict mode"));
                }
            }

            ArgumentSyntax parameters;

            if (statement.Parameters.Count == 0)
            {
                parameters = Syntax.Argument(Syntax.LiteralExpression());
            }
            else
            {
                parameters = Syntax.Argument(Syntax.ImplicitArrayCreationExpression(
                    Syntax.InitializerExpression(statement.Parameters.Select(Syntax.LiteralExpression))
                ));
            }

            string memberName = SanitizeName(statement.Name);

            _scopeBuilder.EnsureVariable(memberName);
            string alias = _scopeBuilder.FindAndCreateAlias(memberName);

            _result = Syntax.ExpressionStatement(
                Syntax.BinaryExpression(
                    BinaryOperator.Equals,
                    Syntax.MemberAccessExpression(
                        Syntax.ParseName(alias),
                        memberName
                    ),
                    Syntax.InvocationExpression(
                        Syntax.IdentifierName("CreateFunction"),
                        Syntax.ArgumentList(
                            Syntax.Argument(Syntax.LiteralExpression(statement.Name)),
                            Syntax.Argument(Syntax.IdentifierName(functionName)),
                            parameters
                        )
                    )
                )
            );
        }

        private string DeclareFunction(FunctionDeclarationStatement expression)
        {
            string functionName = GetNextAnonymousFunctionName();

            var block = Syntax.Block();

            _scopeBuilder = new ScopeBuilder(this, _scopeBuilder, block);

            // Assign the function parameters.

            for (int i = 0; i < expression.Parameters.Count; i++)
            {
                string parameter = SanitizeName(expression.Parameters[i]);

                _scopeBuilder.EnsureVariable(parameter);
                string alias = _scopeBuilder.FindAndCreateAlias(parameter);

                block.Statements.Add(Syntax.ExpressionStatement(
                    Syntax.BinaryExpression(
                        BinaryOperator.Equals,
                        Syntax.MemberAccessExpression(
                            Syntax.ParseName(alias),
                            parameter
                        ),
                        Syntax.ConditionalExpression(
                            Syntax.BinaryExpression(
                                BinaryOperator.GreaterThan,
                                Syntax.MemberAccessExpression(
                                    Syntax.ParseName("arguments"),
                                    "Length"
                                ),
                                Syntax.LiteralExpression(i)
                            ),
                            Syntax.ElementAccessExpression(
                                Syntax.ParseName("arguments"),
                                Syntax.BracketedArgumentList(
                                    Syntax.Argument(Syntax.LiteralExpression(i))
                                )
                            ),
                            Syntax.ParseName("JsUndefined.Instance")
                        )
                    )
                ));
            }

            _class.Members.Add(Syntax.MethodDeclaration(
                returnType: "JsInstance",
                identifier: functionName,
                parameterList: Syntax.ParameterList(
                    Syntax.Parameter(
                        type: "JsDictionaryObject",
                        identifier: "that"
                    ),
                    Syntax.Parameter(
                        type: Syntax.IdentifierName("JsInstance[]"),
                        identifier: "arguments"
                    )
                ),
                body: block,
                modifiers: Modifiers.Private
            ));

            expression.Statement.Accept(this);

            block.Statements.Add(MakeStatement(_result));

            // Make sure we return something.

            block.Statements.Add(Syntax.ReturnStatement(
                Syntax.IdentifierName("JsUndefined.Instance")
            ));

            _scopeBuilder.Build();
            _scopeBuilder = _scopeBuilder.Parent;

            return functionName;
        }

        public void Visit(IfStatement statement)
        {
            statement.Expression.Accept(this);
            var expression = _result;

            statement.Then.Accept(this);
            var @then = _result;

            SyntaxNode @else = null;
            if (statement.Else != null)
            {
                statement.Else.Accept(this);
                @else = _result;
            }

            _result = Syntax.IfStatement(
                Syntax.InvocationExpression(
                    Syntax.MemberAccessExpression(
                        (ExpressionSyntax)expression,
                        "ToBoolean"
                    ),
                    Syntax.ArgumentList()
                ),
                MakeStatement(then),
                @else != null
                ? Syntax.ElseClause(MakeStatement(@else))
                : null
            );
        }

        public void Visit(ReturnStatement statement)
        {
            if (statement.Expression != null)
            {
                statement.Expression.Accept(this);

                _result = Syntax.ReturnStatement((ExpressionSyntax)_result);
            }
            else
            {
                _result = Syntax.ReturnStatement(Syntax.IdentifierName("null"));
            }
        }

        public void Visit(SwitchStatement expression)
        {
            throw new NotImplementedException();
        }

        public void Visit(WithStatement expression)
        {
            throw new NotImplementedException();
        }

        public void Visit(ThrowStatement expression)
        {
            throw new NotImplementedException();
        }

        public void Visit(TryStatement expression)
        {
            throw new NotImplementedException();
        }

        public void Visit(VariableDeclarationStatement statement)
        {
            _result = null;

            if (statement.Expression != null)
            {
                statement.Expression.Accept(this);

                if (statement.Global)
                    throw new InvalidOperationException("Can't declare a global variable");
            }

            string identifier = SanitizeName(statement.Identifier);
            _scopeBuilder.EnsureVariable(identifier);

            string alias = _scopeBuilder.FindAndCreateAlias(identifier);

            _result = Syntax.ExpressionStatement(
                Syntax.BinaryExpression(
                    BinaryOperator.Equals,
                    Syntax.MemberAccessExpression(
                        Syntax.ParseName(alias),
                        identifier
                    ),
                    _result != null ? (ExpressionSyntax)_result : Syntax.ParseName("JsUndefined.Instance")
                )
            );
        }

        public void Visit(WhileStatement statement)
        {
            statement.Condition.Accept(this);
            var condition = _result;

            statement.Statement.Accept(this);
            var body = _result;

            _result = Syntax.WhileStatement(
                Syntax.InvocationExpression(
                    Syntax.MemberAccessExpression(
                        (ExpressionSyntax)condition,
                        "ToBoolean"
                    ),
                    Syntax.ArgumentList()
                ),
                MakeStatement(body)
            );
        }

        public void Visit(ArrayDeclaration expression)
        {
            throw new NotImplementedException();
        }

        public void Visit(CommaOperatorStatement expression)
        {
            throw new NotImplementedException();
        }

        public void Visit(FunctionExpression expression)
        {
            throw new NotImplementedException();
        }

        public void Visit(MemberExpression expression)
        {
            if (expression.Previous == null)
            {
                // Get by identifier is implemented in the visitor.

                expression.Member.Accept(this);
                return;
            }

            var previousCallTarget = _callTarget;

            _callTarget = null;

            var nestedMemberExpression = expression.Previous as MemberExpression;

            if (
                nestedMemberExpression != null && (
                    nestedMemberExpression.Member is PropertyExpression ||
                    nestedMemberExpression.Member is Identifier ||
                    nestedMemberExpression.Member is Indexer
                )
            )
                _callTarget = nestedMemberExpression.Previous;

            expression.Previous.Accept(this);

            expression.Member.Accept(this);

            _callTarget = previousCallTarget;

            //// Try to evaluate a CLR type
            //if (AllowClr && Result == JsUndefined.Instance && _typeFullName != null && _typeFullName.Length > 0)
            //{
            //    EnsureClrAllowed();

            //    Type type = _typeResolver.ResolveType(_typeFullName.ToString());

            //    if (type != null)
            //    {
            //        Result = Global.WrapClr(type);
            //        _typeFullName = new StringBuilder();
            //    }
            //}
        }

        public void Visit(MethodCall methodCall)
        {
            var target = _result;

            SyntaxNode that = null;

            if (_callTarget != null)
            {
                _callTarget.Accept(this);
                that = _result;
            }

            var arguments = new List<ExpressionSyntax>();

            foreach (var argument in methodCall.Arguments)
            {
                argument.Accept(this);

                arguments.Add((ExpressionSyntax)_result);
            }

            _result = Syntax.InvocationExpression(
                Syntax.IdentifierName("ExecuteFunction"),
                Syntax.ArgumentList(
                    Syntax.Argument(that != null ? (ExpressionSyntax)that : Syntax.IdentifierName("null")),
                    Syntax.Argument((ExpressionSyntax)target),
                    Syntax.Argument(Syntax.ArrayCreationExpression(
                        "JsInstance[]",
                        Syntax.InitializerExpression(
                            arguments
                        )
                    ))
                )
            );
        }

        public void Visit(Indexer expression)
        {
            var baseObject = _result;

            expression.Index.Accept(this);
            var indexer = _result;

            _result = Syntax.InvocationExpression(
                Syntax.ParseName("GetByIndexer"),
                Syntax.ArgumentList(
                    Syntax.Argument((ExpressionSyntax)baseObject),
                    Syntax.Argument((ExpressionSyntax)indexer)
                )
            );
        }

        public void Visit(PropertyExpression expression)
        {
            var baseObject = _result;

            _result = Syntax.InvocationExpression(
                Syntax.ParseName("GetByProperty"),
                Syntax.ArgumentList(
                    Syntax.Argument((ExpressionSyntax)baseObject),
                    Syntax.Argument(Syntax.LiteralExpression(expression.Text))
                )
            );
        }

        public void Visit(PropertyDeclarationExpression expression)
        {
            throw new NotImplementedException();
        }

        public void Visit(Identifier expression)
        {
            string propertyName = _lastIdentifier = SanitizeName(expression.Text);
            string alias = _scopeBuilder.FindAndCreateAlias(propertyName);

            if (alias != null)
            {
                _result = Syntax.MemberAccessExpression(
                    Syntax.ParseName(alias),
                    propertyName
                );
            }
            else
            {
                // TODO: Normalize fallback to global scope.

                _result = Syntax.InvocationExpression(
                    Syntax.IdentifierName("GetByIdentifier"),
                    Syntax.ArgumentList(
                        Syntax.Argument(Syntax.LiteralExpression(propertyName))
                    )
                );
            }
        }

        public void Visit(JsonExpression expression)
        {
            throw new NotImplementedException();
        }

        public void Visit(NewExpression expression)
        {
            throw new NotImplementedException();
        }

        public void Visit(BinaryExpression expression)
        {
            // Evaluates the left expression and saves the value
            expression.LeftExpression.Accept(this);

            EnsureIdentifierIsDefined(_result);

            var left = _result;

            // Evaluates the left expression for the condition and saves the value
            // TODO: When switching to MSIL, left must be stored in a temp so we
            // don't calculate it twice.
            expression.LeftExpression.Accept(this);

            EnsureIdentifierIsDefined(_result);

            var condition = _result;

            // Evaluates the right expression and saves the value
            expression.RightExpression.Accept(this);

            EnsureIdentifierIsDefined(_result);

            var right = _result;

            BinaryOperator op;

            switch (expression.Type)
            {
                case BinaryExpressionType.And:
                    _result = Syntax.ConditionalExpression(
                        Syntax.InvocationExpression(
                            Syntax.MemberAccessExpression((ExpressionSyntax)condition, "ToBoolean"),
                            Syntax.ArgumentList()
                        ),
                        Syntax.CastExpression("JsInstance", (ExpressionSyntax)right),
                        (ExpressionSyntax)left
                    );
                    break;

                case BinaryExpressionType.Or:
                    _result = Syntax.ConditionalExpression(
                        Syntax.InvocationExpression(
                            Syntax.MemberAccessExpression((ExpressionSyntax)condition, "ToBoolean"),
                            Syntax.ArgumentList()
                        ),
                        Syntax.CastExpression("JsInstance", (ExpressionSyntax)left),
                        (ExpressionSyntax)right
                    );
                    break;

                case BinaryExpressionType.Div:
                case BinaryExpressionType.Modulo:
                case BinaryExpressionType.Plus:
                case BinaryExpressionType.BitwiseAnd:
                case BinaryExpressionType.BitwiseOr:
                case BinaryExpressionType.BitwiseXOr:
                case BinaryExpressionType.LeftShift:
                case BinaryExpressionType.RightShift:
                case BinaryExpressionType.UnsignedRightShift:
                case BinaryExpressionType.InstanceOf:
                case BinaryExpressionType.In:
                    _result = Syntax.InvocationExpression(
                        Syntax.IdentifierName(expression.Type.ToString()),
                        Syntax.ArgumentList(
                            Syntax.Argument((ExpressionSyntax)left),
                            Syntax.Argument((ExpressionSyntax)right)
                        )
                    );
                    break;

                case BinaryExpressionType.Times:
                case BinaryExpressionType.Minus:
                    switch (expression.Type)
                    {
                        case BinaryExpressionType.Times: op = BinaryOperator.Asterisk; break;
                        case BinaryExpressionType.Minus: op = BinaryOperator.Minus; break;
                        default: throw new InvalidOperationException();
                    }

                    _result = Syntax.InvocationExpression(
                        Syntax.ParseName("Global.NumberClass.New"),
                        Syntax.ArgumentList(
                            Syntax.Argument(
                                Syntax.BinaryExpression(
                                    op,
                                    Syntax.InvocationExpression(
                                        Syntax.MemberAccessExpression((ExpressionSyntax)left, "ToNumber"),
                                        Syntax.ArgumentList()
                                    ),
                                    Syntax.InvocationExpression(
                                        Syntax.MemberAccessExpression((ExpressionSyntax)right, "ToNumber"),
                                        Syntax.ArgumentList()
                                    )
                                )
                            )
                        )
                    );
                    break;

                case BinaryExpressionType.Pow:
                    _result = Syntax.InvocationExpression(
                        Syntax.ParseName("Global.NumberClass.New"),
                        Syntax.ArgumentList(
                            Syntax.Argument(
                                Syntax.InvocationExpression(
                                    Syntax.ParseName("Math.Pow"),
                                    Syntax.ArgumentList(
                                        Syntax.Argument(
                                            Syntax.InvocationExpression(
                                                Syntax.MemberAccessExpression((ExpressionSyntax)left, "ToNumber"),
                                                Syntax.ArgumentList()
                                            )
                                        ),
                                        Syntax.Argument(
                                            Syntax.InvocationExpression(
                                                Syntax.MemberAccessExpression((ExpressionSyntax)right, "ToNumber"),
                                                Syntax.ArgumentList()
                                            )
                                        )
                                    )
                                )
                            )
                        )
                    );
                    break;

                case BinaryExpressionType.Equal:
                    _result = CompileEquals(left, right);
                    break;

                case BinaryExpressionType.NotEqual:
                    var equals = CompileEquals(left, right);
                    _result = Syntax.InvocationExpression(
                        Syntax.ParseName("Global.BooleanClass.New"),
                        Syntax.ArgumentList(
                            Syntax.Argument(
                                Syntax.PrefixUnaryExpression(
                                    PrefixUnaryOperator.Exclamation,
                                    Syntax.InvocationExpression(
                                        Syntax.MemberAccessExpression(
                                            equals,
                                            "ToBoolean"
                                        ),
                                        Syntax.ArgumentList()
                                    )
                                )
                            )
                        )
                    );
                    break;

                case BinaryExpressionType.Greater:
                case BinaryExpressionType.GreaterOrEqual:
                case BinaryExpressionType.Lesser:
                case BinaryExpressionType.LesserOrEqual:
                    _result = Syntax.InvocationExpression(
                        Syntax.ParseName("Compare"),
                        Syntax.ArgumentList(
                            Syntax.Argument((ExpressionSyntax)left),
                            Syntax.Argument((ExpressionSyntax)right),
                            Syntax.Argument(Syntax.ParseName("CompareMode." + expression.Type))
                        )
                    );
                    break;

                case BinaryExpressionType.Same:
                    _result = CompileSame(left, right);
                    break;

                case BinaryExpressionType.NotSame:
                    var same = CompileSame(left, right);

                    _result = Syntax.InvocationExpression(
                        Syntax.ParseName("Global.BooleanClass.New"),
                        Syntax.ArgumentList(
                            Syntax.Argument(
                                Syntax.PrefixUnaryExpression(
                                    PrefixUnaryOperator.Exclamation,
                                    Syntax.InvocationExpression(
                                        Syntax.MemberAccessExpression(
                                            same,
                                            "ToBoolean"
                                        ),
                                        Syntax.ArgumentList()
                                    )
                                )
                            )
                        )
                    );
                    break;

                default:
                    throw new NotSupportedException("Unknown binary operator");
            }
        }

        private static InvocationExpressionSyntax CompileSame(SyntaxNode left, SyntaxNode right)
        {
            return Syntax.InvocationExpression(
                Syntax.ParseName("JsInstance.StrictlyEquals"),
                Syntax.ArgumentList(
                    Syntax.Argument(Syntax.ParseName("Global")),
                    Syntax.Argument((ExpressionSyntax)left),
                    Syntax.Argument((ExpressionSyntax)right)
                )
            );
        }

        private static InvocationExpressionSyntax CompileEquals(SyntaxNode left, SyntaxNode right)
        {
            return Syntax.InvocationExpression(
                Syntax.ParseName("CompareEquals"),
                Syntax.ArgumentList(
                    Syntax.Argument((ExpressionSyntax)left),
                    Syntax.Argument((ExpressionSyntax)right)
                )
            );
        }

        public void Visit(TernaryExpression expression)
        {
            throw new NotImplementedException();
        }

        public void Visit(UnaryExpression expression)
        {
            expression.Expression.Accept(this);
            var operand = _result;

            MemberExpression member;
            PrefixUnaryOperator op;

            switch (expression.Type)
            {
                case UnaryExpressionType.TypeOf:
                    _result = Syntax.InvocationExpression(
                        Syntax.ParseName(expression.Type.ToString()),
                        Syntax.ArgumentList(
                            Syntax.Argument((ExpressionSyntax)operand)
                        )
                    );
                    break;

                case UnaryExpressionType.Not:
                    switch (expression.Type)
                    {
                        case UnaryExpressionType.Not: op = PrefixUnaryOperator.Exclamation; break;
                        default: throw new InvalidOperationException();
                    }

                    _result = Syntax.InvocationExpression(
                        Syntax.ParseName("Global.BooleanClass.New"),
                        Syntax.ArgumentList(
                            Syntax.Argument(
                                Syntax.PrefixUnaryExpression(
                                    op,
                                    Syntax.InvocationExpression(
                                        Syntax.MemberAccessExpression(
                                            (ExpressionSyntax)operand,
                                            "ToBoolean"
                                        ),
                                        Syntax.ArgumentList()
                                    )
                                )
                            )
                        )
                    );
                    break;

                case UnaryExpressionType.Positive:
                case UnaryExpressionType.Negate:
                    switch (expression.Type)
                    {
                        case UnaryExpressionType.Positive: op = PrefixUnaryOperator.Plus; break;
                        case UnaryExpressionType.Negate: op = PrefixUnaryOperator.Minus; break;
                        default: throw new InvalidOperationException();
                    }

                    _result = Syntax.InvocationExpression(
                        Syntax.ParseName("Global.NumberClass.New"),
                        Syntax.ArgumentList(
                            Syntax.Argument(
                                Syntax.PrefixUnaryExpression(
                                    op,
                                    Syntax.InvocationExpression(
                                        Syntax.MemberAccessExpression((ExpressionSyntax)operand, "ToNumber"),
                                        Syntax.ArgumentList()
                                    )
                                )
                            )
                        )
                    );
                    break;

                case UnaryExpressionType.PostfixPlusPlus:
                case UnaryExpressionType.PostfixMinusMinus:
                case UnaryExpressionType.PrefixPlusPlus:
                case UnaryExpressionType.PrefixMinusMinus:
                    int offset =
                        expression.Type == UnaryExpressionType.PrefixPlusPlus || expression.Type == UnaryExpressionType.PostfixPlusPlus
                        ? 1
                        : -1;

                    string type =
                        expression.Type == UnaryExpressionType.PrefixMinusMinus || expression.Type == UnaryExpressionType.PrefixPlusPlus
                        ? "Prefix"
                        : "Postfix";

                    member =
                        expression.Expression as MemberExpression ??
                        new MemberExpression(expression.Expression, null);

                    member.Accept(this);

                    operand = _result;

                    if (member.Previous == null)
                    {
                        string memberName = SanitizeName(((Identifier)member.Member).Text);

                        _scopeBuilder.EnsureVariable(memberName);
                        string alias = _scopeBuilder.FindAndCreateAlias(memberName);

                        // If we're assigning a variable that isn't known in any scope,
                        // it's for the global scope.

                        if (alias == null)
                        {
                            _globalScopeBuilder.EnsureVariable(memberName);
                            alias = _scopeBuilder.FindAndCreateAlias(memberName);
                        }

                        var argument = Syntax.Argument((ExpressionSyntax)operand);

                        argument.Modifier = ParameterModifier.Ref;

                        _result = Syntax.InvocationExpression(
                            Syntax.ParseName(type + "IncrementIdentifier"),
                            Syntax.ArgumentList(
                                argument,
                                Syntax.Argument(Syntax.LiteralExpression(offset))
                            )
                        );
                    }
                    else
                    {
                        member.Previous.Accept(this);

                        var baseObject = _result;

                        if (member.Member is Identifier)
                        {
                            _result = Syntax.InvocationExpression(
                                Syntax.ParseName(type + "IncrementMember"),
                                Syntax.ArgumentList(
                                    Syntax.Argument((ExpressionSyntax)baseObject),
                                    Syntax.Argument(Syntax.LiteralExpression(((Identifier)member.Member).Text)),
                                    Syntax.Argument((ExpressionSyntax)operand),
                                    Syntax.Argument(Syntax.LiteralExpression(offset))
                                )
                            );
                        }
                        else
                        {
                            member.Member.Accept(this);

                            _result = Syntax.InvocationExpression(
                                Syntax.ParseName(type + "IncrementIndexer"),
                                Syntax.ArgumentList(
                                    Syntax.Argument((ExpressionSyntax)baseObject),
                                    Syntax.Argument((ExpressionSyntax)_result),
                                    Syntax.Argument((ExpressionSyntax)operand),
                                    Syntax.Argument(Syntax.LiteralExpression(offset))
                                )
                            );
                        }
                    }
                    break;

                case UnaryExpressionType.Delete:
                    throw new NotImplementedException();

                    //member = expression.Expression as MemberExpression;
                    //if (member == null)
                    //    throw new InvalidOperationException("Delete is not implemented");
                    //member.Previous.Accept(this);
                    //EnsureIdentifierIsDefined(Result);
                    //value = Result;
                    //string propertyName = null;
                    //if (member.Member is PropertyExpression)
                    //    propertyName = ((PropertyExpression)member.Member).Text;
                    //if (member.Member is Indexer)
                    //{
                    //    ((Indexer)member.Member).Index.Accept(this);
                    //    propertyName = Result.ToString();
                    //}
                    //if (string.IsNullOrEmpty(propertyName))
                    //    throw new JsException(Global.TypeErrorClass.New());
                    //try
                    //{
                    //    ((JsDictionaryObject)value).Delete(propertyName);
                    //}
                    //catch (JintException)
                    //{
                    //    throw new JsException(Global.TypeErrorClass.New());
                    //}
                    //Result = value;
                    //break;

                case UnaryExpressionType.Void:
                    expression.Expression.Accept(this);

                    _result = Syntax.InvocationExpression(
                        Syntax.ParseName("Void"),
                        Syntax.ArgumentList(
                            Syntax.Argument((ExpressionSyntax)_result)
                        )
                    );
                    break;

                case UnaryExpressionType.Inv:
                    _result = Syntax.InvocationExpression(
                        Syntax.ParseName("Global.NumberClass.New"),
                        Syntax.ArgumentList(
                            Syntax.Argument(
                                Syntax.BinaryExpression(
                                    BinaryOperator.Minus,
                                    Syntax.BinaryExpression(
                                        BinaryOperator.Minus,
                                        Syntax.LiteralExpression(0),
                                        Syntax.InvocationExpression(
                                            Syntax.MemberAccessExpression((ExpressionSyntax)operand, "ToNumber"),
                                            Syntax.ArgumentList()
                                        )
                                    ),
                                    Syntax.LiteralExpression(1)
                                )
                            )
                        )
                    );
                    break;

            }
        }

        public void Visit(ValueExpression expression)
        {
            switch (expression.TypeCode)
            {
                case TypeCode.Boolean:
                    _result = Syntax.InvocationExpression(
                        Syntax.IdentifierName("Global.BooleanClass.New"),
                        Syntax.ArgumentList(
                            Syntax.Argument(Syntax.LiteralExpression((bool)expression.Value))
                        )
                    );
                    break;

                case TypeCode.Int32:
                case TypeCode.Single:
                case TypeCode.Double:
                    _result = Syntax.InvocationExpression(
                        Syntax.IdentifierName("Global.NumberClass.New"),
                        Syntax.ArgumentList(
                            Syntax.Argument(
                                Syntax.InvocationExpression(
                                    Syntax.IdentifierName("Convert.ToDouble"),
                                    Syntax.ArgumentList(
                                        Syntax.Argument(Syntax.LiteralExpression(expression.Value))
                                    )
                                )
                            )
                        )
                    );
                    break;

                case TypeCode.String:
                    _result = Syntax.InvocationExpression(
                        Syntax.IdentifierName("Global.StringClass.New"),
                        Syntax.ArgumentList(
                            Syntax.Argument(Syntax.LiteralExpression((string)expression.Value))
                        )
                    );
                    break;

                default:
                    //Result = expression.Value as JsInstance;
                    throw new NotImplementedException();
            }
        }

        public void Visit(RegexpExpression expression)
        {
            throw new NotImplementedException();
        }

        public void Visit(Statement expression)
        {
            throw new NotImplementedException();
        }

        private void EnsureIdentifierIsDefined(object value)
        {
            if (value == null)
                throw new JsException(Global.ReferenceErrorClass.New(_lastIdentifier + " is not defined"));
        }
    }
}
