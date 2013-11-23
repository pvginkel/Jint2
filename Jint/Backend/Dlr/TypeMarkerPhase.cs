using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jint.Expressions;
using ValueType = Jint.Expressions.ValueType;

namespace Jint.Backend.Dlr
{
    internal class TypeMarkerPhase : SyntaxVisitor
    {
        public override void VisitProgram(ProgramSyntax syntax)
        {
            foreach (var variable in syntax.DeclaredVariables)
            {
                if (variable.Type == VariableType.Global)
                    variable.ValueType = ValueType.Unknown;
            }

            base.VisitProgram(syntax);
        }

        public override void VisitFunctionDeclaration(FunctionDeclarationSyntax syntax)
        {
            MarkAssign(syntax.Target, ValueType.Unknown);

            foreach (var variable in syntax.Body.DeclaredVariables)
            {
                if (variable.Type == VariableType.Parameter)
                    variable.ValueType = ValueType.Unknown;
            }

            base.VisitFunctionDeclaration(syntax);
        }

        public override void VisitFunction(FunctionSyntax syntax)
        {
            if (syntax.Target != null)
                MarkAssign(syntax.Target, ValueType.Unknown);

            foreach (var variable in syntax.Body.DeclaredVariables)
            {
                if (variable.Type == VariableType.Parameter)
                    variable.ValueType = ValueType.Unknown;
            }

            base.VisitFunction(syntax);
        }

        public override void VisitAssignment(AssignmentSyntax syntax)
        {
            var identifier = syntax.Left as IdentifierSyntax;

            if (identifier != null)
                MarkAssign(identifier.Target, syntax.Right.ValueType);

            base.VisitAssignment(syntax);
        }

        public override void VisitVariableDeclaration(VariableDeclarationSyntax syntax)
        {
            if (syntax.Expression != null)
                MarkAssign(syntax.Target, syntax.Expression.ValueType);

            base.VisitVariableDeclaration(syntax);
        }

        public override void VisitIndexer(IndexerSyntax syntax)
        {
            // TODO: This can be optimized. If we only do a get index,
            // we can go to the prototype manually.

            var identifier = syntax.Expression as IdentifierSyntax;
            if (identifier != null)
                MarkAssign(identifier.Target, ValueType.Unknown);

            base.VisitIndexer(syntax);
        }

        public override void VisitProperty(PropertySyntax syntax)
        {
            // TODO: This can be optimized. If we only do a get property,
            // we can go to the prototype manually.

            var identifier = syntax.Expression as IdentifierSyntax;
            if (identifier != null)
                MarkAssign(identifier.Target, ValueType.Unknown);

            base.VisitProperty(syntax);
        }

        public override void VisitTry(TrySyntax syntax)
        {
            if (syntax.Catch != null)
                MarkAssign(syntax.Catch.Target, ValueType.Unknown);

            base.VisitTry(syntax);
        }

        public override void VisitIdentifier(IdentifierSyntax syntax)
        {
            // If we get a read before we get a write, we set it to JsInstance
            // because that's the only way we can make it undefined.

            if (syntax.Target.ValueType == ValueType.Unset)
                syntax.Target.ValueType = ValueType.Unknown;

            base.VisitIdentifier(syntax);
        }

        private void MarkAssign(Variable variable, ValueType valueType)
        {
            if (variable.ValueType == ValueType.Unset)
                variable.ValueType = valueType;
            else if (variable.ValueType != valueType)
                variable.ValueType = ValueType.Unknown;
        }
    }
}
