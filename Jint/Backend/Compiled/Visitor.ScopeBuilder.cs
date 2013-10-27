using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpSyntax;

namespace Jint.Backend.Compiled
{
    partial class Visitor
    {
        private class ScopeBuilder
        {
            private readonly Visitor _visitor;
            private BlockSyntax _block;
            private readonly bool _isGlobal;
            private readonly List<string> _variableNames = new List<string>();
            private readonly Dictionary<ScopeBuilder, string> _aliases = new Dictionary<ScopeBuilder, string>();
            private readonly string _name;

            public ScopeBuilder Parent { get; private set; }

            public ScopeBuilder(Visitor visitor, ScopeBuilder parent, bool isGlobal)
            {
                _visitor = visitor;
                _isGlobal = isGlobal;
                Parent = parent;
                _name = _visitor.GetNextAnonymousClassName();

                const string alias = "__scope";
                _aliases.Add(this, alias);
            }

            public void Build()
            {
                var klass = Syntax.ClassDeclaration(
                    modifiers: Modifiers.Private,
                    identifier: _name
                );

                _visitor._class.Members.Add(klass);

                foreach (string variableName in _variableNames)
                {
                    EqualsValueClauseSyntax initializer = null;

                    if (!_isGlobal)
                        initializer = Syntax.EqualsValueClause(Syntax.ParseName("JsUndefined.Instance"));

                    klass.Members.Add(Syntax.FieldDeclaration(
                        modifiers: Modifiers.Public,
                        declaration: Syntax.VariableDeclaration(
                            "JsInstance",
                            new[]
                            {
                                Syntax.VariableDeclarator(
                                    variableName,
                                    initializer: initializer
                                )
                            }
                        )
                    ));
                }

                // We need some extra support from the global scope because it
                // needs to synchronize the fields with the GlobalScope scope.

                if (_isGlobal)
                {
                    var body = Syntax.Block();

                    foreach (string variableName in _variableNames)
                    {
                        body.Statements.Add(Syntax.ExpressionStatement(
                            Syntax.BinaryExpression(
                                BinaryOperator.Equals,
                                Syntax.MemberAccessExpression(
                                    Syntax.ParseName("this"),
                                    variableName
                                ),
                                Syntax.ElementAccessExpression(
                                    Syntax.ParseName("scope"),
                                    Syntax.BracketedArgumentList(
                                        Syntax.Argument(Syntax.LiteralExpression(variableName))
                                    )
                                )
                            )
                        ));
                    }

                    klass.Members.Add(Syntax.MethodDeclaration(
                        modifiers: Modifiers.Public,
                        returnType: "void",
                        identifier: "__LoadScope",
                        parameterList: Syntax.ParameterList(
                            Syntax.Parameter(
                                type: "JsScope",
                                identifier: "scope"
                            )
                        ),
                        body: body
                    ));

                    body = Syntax.Block();

                    foreach (string variableName in _variableNames)
                    {
                        body.Statements.Add(Syntax.ExpressionStatement(
                            Syntax.BinaryExpression(
                                BinaryOperator.Equals,
                                Syntax.ElementAccessExpression(
                                    Syntax.ParseName("scope"),
                                    Syntax.BracketedArgumentList(
                                        Syntax.Argument(Syntax.LiteralExpression(variableName))
                                    )
                                ),
                                Syntax.MemberAccessExpression(
                                    Syntax.ParseName("this"),
                                    variableName
                                )
                            )
                        ));
                    }

                    klass.Members.Add(Syntax.MethodDeclaration(
                        modifiers: Modifiers.Public,
                        returnType: "void",
                        identifier: "__SaveScope",
                        parameterList: Syntax.ParameterList(
                            Syntax.Parameter(
                                type: "JsScope",
                                identifier: "scope"
                            )
                        ),
                        body: body
                    ));
                }
            }

            public string FindAndCreateAlias(string variableName)
            {
                int level = 0;
                var builder = this;

                while (builder != null)
                {
                    if (builder.HasVariable(variableName))
                    {
                        string alias;
                        if (!_aliases.TryGetValue(builder, out alias))
                        {
                            alias = "__scope" + level;
                            _aliases.Add(builder, alias);

                            _block.Statements.Add(
                                Syntax.LocalDeclarationStatement(
                                    Syntax.VariableDeclaration(
                                        "var",
                                        new[]
                                        {
                                            Syntax.VariableDeclarator(
                                                alias,
                                                initializer: Syntax.EqualsValueClause(
                                                    Syntax.CastExpression(
                                                        builder._name,
                                                        Syntax.MemberAccessExpression(
                                                            Syntax.InvocationExpression(
                                                                Syntax.ParseName("GetScope"),
                                                                Syntax.ArgumentList(
                                                                    Syntax.Argument(Syntax.LiteralExpression(level))
                                                                )
                                                            ),
                                                            "CompiledScope"
                                                        )
                                                    )
                                                )
                                            )
                                        }
                                    )
                                )
                            );
                        }

                        return alias;
                    }

                    level++;
                    builder = builder.Parent;
                }

                return null;
            }

            public void EnsureVariable(string variableName)
            {
                if (!HasVariable(variableName))
                    _variableNames.Add(variableName);
            }

            private bool HasVariable(string variableName)
            {
                return _variableNames.Contains(variableName);
            }

            public BlockSyntax InitializeBody(BlockSyntax body)
            {
                _block = body;

                string alias = _aliases[this];

                body.Statements.Add(Syntax.LocalDeclarationStatement(
                    Syntax.VariableDeclaration(
                        "var",
                        new[]
                        {
                            Syntax.VariableDeclarator(
                                alias,
                                initializer: Syntax.EqualsValueClause(
                                    Syntax.ObjectCreationExpression(
                                        _name,
                                        Syntax.ArgumentList()
                                    )
                                )
                            )
                        }
                    )
                ));

                body.Statements.Add(Syntax.ExpressionStatement(
                    Syntax.BinaryExpression(
                        BinaryOperator.Equals,
                        Syntax.ParseName("CurrentScope.CompiledScope"),
                        Syntax.ParseName(alias)
                    )
                ));

                if (!_isGlobal)
                    return body;

                body.Statements.Add(Syntax.ExpressionStatement(
                    Syntax.InvocationExpression(
                        Syntax.MemberAccessExpression(
                            Syntax.ParseName(alias),
                            "__LoadScope"
                        ),
                        Syntax.ArgumentList(
                            Syntax.Argument(Syntax.ParseName("CurrentScope"))
                        )
                    )
                ));

                var result = Syntax.Block();

                _block = result;

                body.Statements.Add(Syntax.TryStatement(
                    block: result,
                    @finally: Syntax.FinallyClause(
                        Syntax.Block(
                            Syntax.ExpressionStatement(
                                Syntax.InvocationExpression(
                                    Syntax.MemberAccessExpression(
                                        Syntax.ParseName(alias),
                                        "__SaveScope"
                                    ),
                                    Syntax.ArgumentList(
                                        Syntax.Argument(Syntax.ParseName("CurrentScope"))
                                    )
                                )
                            )
                        )
                    )
                ));

                return result;
            }
        }
    }
}
