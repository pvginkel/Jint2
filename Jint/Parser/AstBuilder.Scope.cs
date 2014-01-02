using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jint.Ast;
using Jint.Native;
using System.Globalization;

namespace Jint.Parser
{
    partial class AstBuilder
    {
        private class Scope
        {
#if DEBUG
            private const string WithPrefix = "__with";
#else
            private const string WithPrefix = "<>with";
#endif

            private readonly BodyType _bodyType;
            private List<SyntaxNode> _statements;
            private List<FunctionSyntax> _declaredFunctions;
            private bool _hadStrict;
            private readonly Scope _rootScope;
            private BuilderWithScope _withScope;
            private CatchScope _catchScope;
            private int _nextWithScopeIndex = 1;
            private readonly HashSet<string> _declaredIdentifiers = new HashSet<string>();
            private readonly List<UnresolvedIdentifier> _identifiers = new List<UnresolvedIdentifier>();
            private readonly List<ResolvedIdentifier> _resolvedIdentifiers = new List<ResolvedIdentifier>();
            private readonly HashSet<ResolvedIdentifier> _closedOverIdentifiers = new HashSet<ResolvedIdentifier>();
            private readonly ReadOnlyArray<string> _parameters;

            public Scope Parent { get; private set; }

            private bool IsStrict
            {
                get { return _hadStrict || (Parent != null && Parent.IsStrict); }
            }

            public Scope(BodyType bodyType, Scope parent, ReadOnlyArray<string> parameters)
            {
                _bodyType = bodyType;
                Parent = parent;
                _parameters = parameters;
                _rootScope = parent != null ? parent._rootScope : this;
            }

            public void AddStatement(SyntaxNode node)
            {
                if (_statements == null)
                {
                    _statements = new List<SyntaxNode>();

                    var expressionStatement = node as ExpressionStatementSyntax;
                    if (expressionStatement != null)
                    {
                        var value = expressionStatement.Expression as ValueSyntax;
                        if (value != null)
                            _hadStrict = value.Value is string && (string)value.Value == "use strict";
                    }
                }

                _statements.Add(node);
            }

            public void AddDeclaredFunctions(FunctionSyntax function)
            {
                if (_declaredFunctions == null)
                    _declaredFunctions = new List<FunctionSyntax>();

                _declaredFunctions.Add(function);
            }

            public BodySyntax BuildBody()
            {
                // Check for strict mode.

                if (
                    IsStrict &&
                    (
                        _declaredIdentifiers.Contains(JsNames.Eval) ||
                        (_bodyType == BodyType.Function && _declaredIdentifiers.Contains(JsNames.Arguments))
                    )
                )
                    throw new JsException(JsErrorType.SyntaxError, "Assignment to eval or arguments is not allowed in strict mode");

                var identifiers = CommitIdentifiers();
                var closure = BuildClosure();

                return new BodySyntax(
                    _bodyType,
                    GetStatements(),
                    identifiers,
                    IsStrict,
                    closure
                );
            }

            private ReadOnlyArray<IIdentifier> CommitIdentifiers()
            {
                var identifiers = new Dictionary<string, ResolvedIdentifier>();

                // Create the parameters.

                if (_parameters != null)
                {
                    for (int i = _parameters.Count - 1; i >= 0; i--)
                    {
                        if (!identifiers.ContainsKey(_parameters[i]))
                        {
                            identifiers.Add(
                                _parameters[i],
                                new ResolvedIdentifier(
                                    _parameters[i],
                                    i,
                                    IdentifierType.Parameter,
                                    true
                                )
                            );
                        }
                    }
                }

                // Add the declared identifiers.

                foreach (string identifier in _declaredIdentifiers)
                {
                    if (!identifiers.ContainsKey(identifier))
                    {
                        identifiers.Add(
                            identifier,
                            new ResolvedIdentifier(
                                identifier,
                                null,
                                _bodyType == BodyType.Program ? IdentifierType.Global : IdentifierType.Local,
                                true
                            )
                        );
                    }
                }

                // Resolve all identifiers.

                foreach (var identifier in _identifiers)
                {
                    ResolvedIdentifier resolved = null;

                    // First check whether we have a catch variable in scope.

                    if (identifier.CatchScope != null && identifier.CatchScope.Scope == this)
                    {
                        var catchScope = identifier.CatchScope;

                        while (catchScope != null && catchScope.Scope == this)
                        {
                            if (catchScope.Identifier.Name == identifier.Name)
                            {
                                resolved = catchScope.Identifier;
                                break;
                            }

                            catchScope = catchScope.Parent;
                        }

                        if (resolved != null)
                        {
                            identifier.Identifier.ResolvedIdentifier = resolved;

                            if (identifier.Scope != this)
                                _closedOverIdentifiers.Add(resolved);

                            continue;
                        }

                        // We need to change the catch scope of the unresolved
                        // identifier to strip the catch scopes that belong to
                        // us. This way resolving in the parent scope resolves
                        // to a declared variable or to the catch variable of
                        // that scope.

                        identifier.CatchScope = catchScope;
                    }

                    if (identifiers.TryGetValue(identifier.Name, out resolved))
                    {
                        // We have this identifier.

                        identifier.Identifier.ResolvedIdentifier = resolved;

                        // If the identifier does not belong to this scope,
                        // it's being closed over.

                        if (identifier.Scope != this && resolved.Type != IdentifierType.Global)
                            _closedOverIdentifiers.Add(resolved);
                    }
                    else if (Parent == null)
                    {
                        // It's an undeclared global identifier.

                        resolved = new ResolvedIdentifier(
                            identifier.Name,
                            null,
                            IdentifierType.Global,
                            false
                        );

                        identifiers.Add(identifier.Name, resolved);
                        identifier.Identifier.ResolvedIdentifier = resolved;
                    }
                    else
                    {
                        // Else, push it to the parent and let that figure it out.

                        Parent._identifiers.Add(identifier);
                    }
                }

                var builder = new ReadOnlyArray<IIdentifier>.Builder();

                builder.AddRange(identifiers.Select(p => p.Value));
                builder.AddRange(_resolvedIdentifiers);

                return builder.ToReadOnly();
            }

            private Closure BuildClosure()
            {
                if (_closedOverIdentifiers.Count <= 0)
                    return null;

                // Build the closure.

                var closure = new Closure(_closedOverIdentifiers.OrderBy(p => p.Name).Select(p => p.Name).ToReadOnlyArray());

                foreach (var identifier in _closedOverIdentifiers)
                {
                    identifier.Closure = closure;
                }

                return closure;
            }

            private ReadOnlyArray<SyntaxNode> GetStatements()
            {
                var builder = new ReadOnlyArray<SyntaxNode>.Builder();

                if (_declaredFunctions != null)
                    builder.AddRange(_declaredFunctions);
                if (_statements != null)
                    builder.AddRange(_statements);

                return builder.ToReadOnly();
            }

            public IIdentifier CreateIdentifier(string name)
            {
                switch (name)
                {
                    case JsNames.This: return ResolvedIdentifier.This;
                    case JsNames.Null: return ResolvedIdentifier.Null;
                    case JsNames.Undefined: return ResolvedIdentifier.Undefined;
                    case JsNames.Arguments:
                        if (_bodyType == BodyType.Function)
                            return ResolvedIdentifier.Arguments;
                        break;
                }

                var result = new Identifier();

                _identifiers.Add(new UnresolvedIdentifier(name, this, result, FindCatchScope()));

                var withScope = FindWithScope(true);

                if (withScope != null)
                    return new ScopedIdentifier(withScope.WithScope, result);

                return result;
            }

            public void DeclareIdentifier(string name)
            {
                _declaredIdentifiers.Add(name);
            }

            private BuilderWithScope FindWithScope(bool usage)
            {
                // With scopes link over functions. Find any current with scope in
                // any body.

                var scope = this;

                while (scope != null)
                {
                    var withScope = scope._withScope;

                    if (withScope != null)
                    {
                        if (usage && scope != this)
                            CloseOverWithScope(withScope);

                        return withScope;
                    }

                    scope = scope.Parent;
                }

                return null;
            }

            private void CloseOverWithScope(BuilderWithScope withScope)
            {
                while (withScope != null)
                {
                    withScope.Scope._closedOverIdentifiers.Add(
                        (ResolvedIdentifier)withScope.WithScope.Identifier
                    );

                    withScope = withScope.Parent;
                }
            }

            public void EnterWith()
            {
                var identifier = new ResolvedIdentifier(
                    WithPrefix + (_nextWithScopeIndex++).ToString(CultureInfo.InvariantCulture),
                    null,
                    IdentifierType.Local,
                    true
                );

                _resolvedIdentifiers.Add(identifier);

                _withScope = new BuilderWithScope(
                    FindWithScope(false),
                    identifier,
                    this
                );
            }

            public WithSyntax ExitWith(ExpressionSyntax expression, SyntaxNode body, SourceLocation location)
            {
                var result = new WithSyntax(expression, _withScope.WithScope.Identifier, body, location);

                _withScope = _withScope.Parent;

                return result;
            }

            public void EnterCatch(string name)
            {
                var identifier = new ResolvedIdentifier(
                    name,
                    null,
                    IdentifierType.Local,
                    true
                );

                _resolvedIdentifiers.Add(identifier);

                _catchScope = new CatchScope(identifier, this, FindCatchScope());
            }

            private CatchScope FindCatchScope()
            {
                var scope = this;

                while (scope != null)
                {
                    if (scope._catchScope != null)
                        return scope._catchScope;

                    scope = scope.Parent;
                }

                return null;
            }

            public CatchClause ExitCatch(SyntaxNode statement)
            {
                var identifier = _catchScope.Identifier;

                _catchScope = _catchScope.Parent;

                return new CatchClause(identifier, statement);
            }

            private class BuilderWithScope
            {
                public WithScope WithScope { get; private set; }
                public BuilderWithScope Parent { get; private set; }
                public Scope Scope { get; private set; }

                public BuilderWithScope(BuilderWithScope parent, IIdentifier identifier, Scope scope)
                {
                    Parent = parent;
                    WithScope = new WithScope(parent != null ? parent.WithScope : null, identifier);
                    Scope = scope;
                }
            }

            private class UnresolvedIdentifier
            {
                public string Name { get; private set; }
                public Scope Scope { get; private set; }
                public Identifier Identifier { get; private set; }
                public CatchScope CatchScope { get; set; }

                public UnresolvedIdentifier(string name, Scope scope, Identifier identifier, CatchScope catchScope)
                {
                    Name = name;
                    Scope = scope;
                    Identifier = identifier;
                    CatchScope = catchScope;
                }

                public override string ToString()
                {
                    return String.Format(
                        "Name={0}, Scope={{{1}}}, CatchScope={{{2}}}",
                        Name,
                        Scope,
                        CatchScope
                    );
                }
            }

            private class CatchScope
            {
                public ResolvedIdentifier Identifier { get; private set; }
                public Scope Scope { get; private set; }
                public CatchScope Parent { get; private set; }

                public CatchScope(ResolvedIdentifier identifier, Scope scope, CatchScope parent)
                {
                    Identifier = identifier;
                    Scope = scope;
                    Parent = parent;
                }
            }
        }
    }
}
