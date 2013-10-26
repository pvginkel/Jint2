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
            private readonly BlockSyntax _block;
            private readonly List<string> _variableNames = new List<string>();
            private readonly Dictionary<ScopeBuilder, string> _aliases = new Dictionary<ScopeBuilder, string>();

            public ScopeBuilder Parent { get; private set; }
            public string Name { get; private set; }

            public ScopeBuilder(Visitor visitor, ScopeBuilder parent, BlockSyntax block)
            {
                _visitor = visitor;
                _block = block;
                Parent = parent;
                Name = _visitor.GetNextAnonymousClassName();

                const string alias = "__scope";
                _aliases.Add(this, alias);

                _block.Statements.Add(Syntax.LocalDeclarationStatement(
                    Syntax.VariableDeclaration(
                        "var",
                        new[]
                        {
                            Syntax.VariableDeclarator(
                                alias,
                                initializer: Syntax.EqualsValueClause(
                                    Syntax.ObjectCreationExpression(
                                        Name,
                                        Syntax.ArgumentList()
                                    )
                                )
                            )
                        }
                    )
                ));

                _block.Statements.Add(Syntax.ExpressionStatement(
                    Syntax.BinaryExpression(
                        BinaryOperator.Equals,
                        Syntax.ParseName("CurrentScope.CompiledScope"),
                        Syntax.ParseName(alias)
                    )
                ));
            }

            public void Build()
            {
                var klass = Syntax.ClassDeclaration(
                    modifiers: Modifiers.Private,
                    identifier: Name
                );

                _visitor._class.Members.Add(klass);

                foreach (string variableName in _variableNames)
                {
                    klass.Members.Add(Syntax.FieldDeclaration(
                        modifiers: Modifiers.Public,
                        declaration: Syntax.VariableDeclaration(
                            "JsInstance",
                            new[]
                            {
                                Syntax.VariableDeclarator(
                                    variableName,
                                    initializer: Syntax.EqualsValueClause(Syntax.ParseName("JsUndefined.Instance"))
                                )
                            }
                        )
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
                                                        builder.Name,
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
                return _variableNames.Contains(variableName, StringComparer.OrdinalIgnoreCase);
            }
        }
    }
}
