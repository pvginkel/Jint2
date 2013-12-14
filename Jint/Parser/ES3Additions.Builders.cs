using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Jint.Expressions;

namespace Jint.Parser
{
    partial class ES3Parser
    {
        private class BlockBuilder
        {
            private List<SyntaxNode> _statements;
            private List<FunctionSyntax> _functionDeclarations;

            public VariableCollection DeclaredVariables { get; private set; }

            public List<SyntaxNode> Statements
            {
                get
                {
                    if (_statements == null)
                        _statements = new List<SyntaxNode>();

                    return _statements;
                }
            }

            public List<FunctionSyntax> FunctionDeclarations
            {
                get
                {
                    if (_functionDeclarations == null)
                        _functionDeclarations = new List<FunctionSyntax>();

                    return _functionDeclarations;
                }
            }

            public BlockBuilder()
            {
                DeclaredVariables = new VariableCollection();
            }

            public BlockSyntax CreateBlock()
            {
                return new BlockSyntax(GetStatements(), DeclaredVariables);
            }

            private IEnumerable<SyntaxNode> GetStatements()
            {
                if (_functionDeclarations != null)
                {
                    foreach (var functionDeclaration in _functionDeclarations)
                    {
                        yield return functionDeclaration;
                    }
                }

                if (_statements != null)
                {
                    foreach (var statement in _statements)
                    {
                        yield return statement;
                    }
                }
            }

            public ProgramSyntax CreateProgram()
            {
                return new ProgramSyntax(GetStatements(), DeclaredVariables);
            }
        }

        private class ForBuilder
        {
            public SyntaxNode Body { get; set; }

            public SyntaxNode Initialization { get; set; }

            public ExpressionSyntax Expression { get; set; }

            public ExpressionSyntax Test { get; set; }

            public ExpressionSyntax Increment { get; set; }

            public SyntaxNode CreateFor(ES3Parser parser, SourceLocation location)
            {
                if (Expression != null)
                {
                    string identifier;
                    Variable target;

                    if (Initialization is IdentifierSyntax)
                    {
                        var identifierSyntax = (IdentifierSyntax)Initialization;
                        identifier = identifierSyntax.Name;
                        target = identifierSyntax.Target;
                    }
                    else
                    {
                        var variableDeclaration = (VariableDeclarationSyntax)Initialization;

                        Debug.Assert(variableDeclaration.Declarations.Count == 1);

                        var declaration = variableDeclaration.Declarations[0];

                        identifier = declaration.Identifier;
                        target = declaration.Target;

                        Debug.Assert(declaration.Expression == null);
                    }

                    return new ForEachInSyntax(
                        identifier,
                        target ?? parser._currentBody.DeclaredVariables.AddOrGet(identifier, true),
                        Expression,
                        Body,
                        location
                    );
                }

                return new ForSyntax(
                    Initialization,
                    Test,
                    Increment,
                    Body,
                    location
                );
            }
        }

        private class JsonPropertyBuilder
        {
            private readonly Dictionary<string, Assignment> _assignments = new Dictionary<string, Assignment>();

            public void AddProperty(PropertyDeclaration propertyExpression)
            {
                string name = propertyExpression.Name;
                var mode = propertyExpression.Mode;

                if (name == null)
                {
                    name = mode.ToString().ToLower();
                    mode = PropertyExpressionType.Data;
                }

                Assignment declaration;
                if (_assignments.TryGetValue(name, out declaration))
                {
                    if (
                        (declaration.Mode == PropertyExpressionType.Data) !=
                        (mode == PropertyExpressionType.Data)
                    )
                        throw new JintException("A property cannot be both an accessor and data");
                }
                else
                {
                    declaration = new Assignment
                    {
                        Mode = mode,
                        Expression = propertyExpression.Expression
                    };

                    _assignments.Add(name, declaration);
                }

                switch (mode)
                {
                    case PropertyExpressionType.Get:
                        declaration.GetExpression = propertyExpression.Expression;
                        declaration.Expression = null;
                        break;

                    case PropertyExpressionType.Set:
                        declaration.SetExpression = propertyExpression.Expression;
                        declaration.Expression = null;
                        break;
                }
            }

            public IEnumerable<JsonProperty> GetProperties()
            {
                foreach (var assignment in _assignments)
                {
                    if (assignment.Value.Mode == PropertyExpressionType.Data)
                    {
                        yield return new JsonDataProperty(
                            assignment.Key,
                            assignment.Value.Expression
                        );
                    }
                    else
                    {
                        yield return new JsonAccessorProperty(
                            assignment.Key,
                            assignment.Value.GetExpression,
                            assignment.Value.SetExpression
                        );
                    }
                }
            }

            private class Assignment
            {
                public PropertyExpressionType Mode { get; set; }
                public ExpressionSyntax Expression { get; set; }
                public ExpressionSyntax GetExpression { get; set; }
                public ExpressionSyntax SetExpression { get; set; }
            }
        }

        private class PropertyDeclaration
        {
            public string Name { get; private set; }
            public ExpressionSyntax Expression { get; private set; }
            public PropertyExpressionType Mode { get; private set; }

            public PropertyDeclaration(string name, ExpressionSyntax expression, PropertyExpressionType mode)
            {
                Name = name;
                Expression = expression;
                Mode = mode;
            }
        }
    }
}
