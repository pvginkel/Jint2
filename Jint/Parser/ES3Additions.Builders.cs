using System;
using System.Collections.Generic;
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
            private List<SyntaxNode> _functionDeclarations;

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

            public List<SyntaxNode> FunctionDeclarations
            {
                get
                {
                    if (_functionDeclarations == null)
                        _functionDeclarations = new List<SyntaxNode>();

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

            private IList<SyntaxNode> GetStatements()
            {
                if (_statements == null && _functionDeclarations == null)
                    return SyntaxNode.EmptyList;

                if (_statements == null || _functionDeclarations == null)
                    return _statements ?? _functionDeclarations;

                _functionDeclarations.AddRange(_statements);
                return _functionDeclarations;
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

            public SyntaxNode CreateFor()
            {
                if (Expression != null)
                {
                    return new ForEachInSyntax(
                        Initialization,
                        Expression,
                        Body
                    );
                }

                return new ForSyntax(
                    Initialization,
                    Test,
                    Increment,
                    Body
                );
            }
        }

        private class JsonPropertyBuilder
        {
            private readonly Dictionary<string, Assignment> _assignments = new Dictionary<string, Assignment>();

            public void AddProperty(PropertyDeclarationSyntax propertyExpression)
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
    }
}
