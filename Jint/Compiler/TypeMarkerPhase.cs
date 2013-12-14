using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jint.Expressions;
using ValueType = Jint.Expressions.ValueType;

namespace Jint.Compiler
{
    internal class TypeMarkerPhase : SyntaxVisitor
    {
        public override void VisitProgram(ProgramSyntax syntax)
        {
            foreach (var variable in syntax.DeclaredVariables)
            {
                if (variable.Type == VariableType.Global)
                {
#if TRACE_TYPEMARKING
                    Trace.WriteLine(String.Format("{0} -> Unknown: Initializing global", variable.Name));
#endif

                    variable.ValueType = ValueType.Unknown;
                }
            }

            base.VisitProgram(syntax);
        }

        public override void VisitFunction(FunctionSyntax syntax)
        {
            if (syntax.Target != null)
                MarkAssign(syntax.Target, ValueType.Object);

            foreach (var variable in syntax.Body.DeclaredVariables)
            {
                if (variable.Type == VariableType.Parameter)
                {
#if TRACE_TYPEMARKING
                    Trace.WriteLine(String.Format("{0} -> Unknown: Initializing function parameter", variable.Name));
#endif

                    variable.ValueType = ValueType.Unknown;
                }
            }

            base.VisitFunction(syntax);
        }

        public override void VisitAssignment(AssignmentSyntax syntax)
        {
            var identifier = syntax.Left as IdentifierSyntax;

            if (identifier != null)
            {
                if (syntax.Operation != AssignmentOperator.Assign)
                    MarkRead(identifier.Target);

                MarkAssign(identifier.Target, syntax.ValueType);
            }

            base.VisitAssignment(syntax);
        }

        public override void VisitVariableDeclaration(VariableDeclarationSyntax syntax)
        {
            foreach (var declaration in syntax.Declarations)
            {
                if (declaration.Expression != null)
                    MarkAssign(declaration.Target, declaration.Expression.ValueType);
            }

            base.VisitVariableDeclaration(syntax);
        }

        public override void VisitTry(TrySyntax syntax)
        {
            if (syntax.Catch != null)
                MarkAssign(syntax.Catch.Target, ValueType.Object);

            base.VisitTry(syntax);
        }

        public override void VisitIdentifier(IdentifierSyntax syntax)
        {
            MarkRead(syntax.Target);

            base.VisitIdentifier(syntax);
        }

        private void MarkRead(Variable variable)
        {
            // Mark variables that may be read before they are written to.

            if (variable.ValueType == ValueType.Unset)
            {
#if TRACE_TYPEMARKING
                Trace.WriteLine(String.Format("{0} -> Unknown: Read before write", variable.Name));
#endif

                variable.ValueType = ValueType.Unknown;
            }
        }

        private void MarkAssign(Variable variable, ValueType valueType)
        {
            if (variable.ValueType == ValueType.Unset)
            {
#if TRACE_TYPEMARKING
                Trace.WriteLine(String.Format("{0} -> {1}", variable.Name, valueType));
#endif
                variable.ValueType = valueType;
            }
            else if (variable.ValueType != valueType && variable.ValueType != ValueType.Unknown)
            {
#if TRACE_TYPEMARKING
                Trace.WriteLine(String.Format("{0} -> Unknown: Cannot assign {1} to {2}", variable.Name, valueType, variable.ValueType));
#endif
                variable.ValueType = ValueType.Unknown;
            }
        }
    }
}
