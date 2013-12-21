using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Jint.Support;

namespace Jint.Bound
{
    internal class BoundTreePrettyPrintVisitor : BoundTreeWalker
    {
        private readonly TextWriter _writer;
        private int _indent;

        private void WriteIndent()
        {
            if (_indent > 0)
                _writer.Write(new string(' ', _indent * 4));
        }

        private void WriteLine(string value)
        {
            _writer.WriteLine(value);
        }

        private void WriteLine(string format, params object[] args)
        {
            _writer.WriteLine(format, args);
        }

        private void Write(string value)
        {
            _writer.Write(value);
        }

        private void Write(string format, params object[] args)
        {
            _writer.Write(format, args);
        }

        public BoundTreePrettyPrintVisitor(TextWriter writer)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");

            _writer = writer;
        }

        private void DefaultBefore(BoundNode node)
        {
            WriteLine("{0} (", node.NodeType);

            _indent++;

            WriteIndent();
        }

        private void DefaultAfter()
        {
            WriteLine(")");

            _indent--;

            WriteIndent();
        }

        public override void VisitList<T>(ReadOnlyArray<T> node)
        {
            WriteLine("{");

            _indent++;

            foreach (var item in node)
            {
                WriteIndent();

                Visit(item);

                WriteLine("");
            }

            _indent--;

            WriteIndent();

            Write("}");
        }

        public override void VisitBinary(BoundBinary node)
        {
            Visit(node.Left);

            Write(" " + GetOperation(node.Operation) + " ");

            Visit(node.Right);
        }

        public override void VisitBlock(BoundBlock node)
        {
            if (node.Temporaries.Count > 0)
                Write("<" + String.Join(", ", node.Temporaries) + "> ");

            base.VisitBlock(node);
        }

        public override void VisitBody(BoundBody node)
        {
            var sb = new StringBuilder();

            foreach (var argument in node.Arguments)
            {
                if (sb.Length > 0)
                    sb.Append(", ");
                sb.Append(argument);
            }

            foreach (var local in node.Locals)
            {
                if (sb.Length > 0)
                    sb.Append(", ");
                sb.Append(local);
            }

            WriteLine("<" + sb + ">");

            base.VisitBody(node);
        }

        public override void VisitBreak(BoundBreak node)
        {
            Write("Break");
            if (node.Target != null)
            {
                Write("(Target = ");
                Write(node.Target);
                Write(")");
            }
        }

        public override void VisitCall(BoundCall node)
        {
            Write("Call(Target = ");
            Visit(node.Target);
            Write(", Method = ");
            Visit(node.Method);
            bool hadOne = false;
            if (node.Generics.Count > 0)
            {
                Write("{");
                foreach (var generic in node.Generics)
                {
                    if (hadOne)
                        Write(", ");
                    else
                        hadOne = true;
                    Visit(generic);
                }
            }
            Write(")(");
            hadOne = false;
            foreach (var argument in node.Arguments)
            {
                if (hadOne)
                    Write(", ");
                else
                    hadOne = true;
                Visit(argument);
            }
            Write(")");
        }

        public override void VisitCallArgument(BoundCallArgument node)
        {
            if (node.IsRef)
                Write("Ref ");
            Visit(node.Expression);
        }

        public override void VisitCatch(BoundCatch node)
        {
            DefaultBefore(node);
            base.VisitCatch(node);
            DefaultAfter();
        }

        public override void VisitConstant(BoundConstant node)
        {
            var stringValue = node.Value as string;

            if (stringValue != null)
            {
                Write("\"");
                Write(JintEngine.EscapeStringLiteral(stringValue));
                Write("\"");
            }
            else
            {
                Write(node.Value.ToString());
            }
        }

        public override void VisitContinue(BoundContinue node)
        {
            Write("Continue");
            if (node.Target != null)
            {
                Write("(Target = ");
                Write(node.Target);
                Write(")");
            }
        }

        public override void VisitCreateFunction(BoundCreateFunction node)
        {
            Write("CreateFunction(" + node.Function.Name + ")");
        }

        public override void VisitDeleteMember(BoundDeleteMember node)
        {
            DefaultBefore(node);
            base.VisitDeleteMember(node);
            DefaultAfter();
        }

        public override void VisitDoWhile(BoundDoWhile node)
        {
            Write("Do ");
            Visit(node.Body);
            Write(" While (");
            Visit(node.Test);
            Write(")");
        }

        public override void VisitEmpty(BoundEmpty node)
        {
            Write("Empty()");
        }

        public override void VisitExpressionBlock(BoundExpressionBlock node)
        {
            Write("ValueOf(" + node.Result + ") ");
            base.VisitExpressionBlock(node);
        }

        public override void VisitFinally(BoundFinally node)
        {
            DefaultBefore(node);
            base.VisitFinally(node);
            DefaultAfter();
        }

        public override void VisitFor(BoundFor node)
        {
            Write("For (");
            Visit(node.Initialization);
            Write(", ");
            Visit(node.Test);
            Write(", ");
            Visit(node.Increment);
            Write(") ");
            Visit(node.Body);
        }

        public override void VisitForEachIn(BoundForEachIn node)
        {
            DefaultBefore(node);
            base.VisitForEachIn(node);
            DefaultAfter();
        }

        public override void VisitGetMember(BoundGetMember node)
        {
            Visit(node.Expression);
            Write("[");
            Visit(node.Index);
            Write("]");
        }

        public override void VisitGetVariable(BoundGetVariable node)
        {
            Write(node.Variable.ToString());
        }

        public override void VisitHasMember(BoundHasMember node)
        {
            DefaultBefore(node);
            base.VisitHasMember(node);
            DefaultAfter();
        }

        public override void VisitIf(BoundIf node)
        {
            Write("If (");
            Visit(node.Test);
            Write(") ");
            Visit(node.Then);
            if (node.Else != null)
            {
                Write(" Else ");
                Visit(node.Else);
            }
        }

        public override void VisitNew(BoundNew node)
        {
            DefaultBefore(node);
            base.VisitNew(node);
            DefaultAfter();
        }

        public override void VisitNewBuiltIn(BoundNewBuiltIn node)
        {
            DefaultBefore(node);
            base.VisitNewBuiltIn(node);
            DefaultAfter();
        }

        public override void VisitRegex(BoundRegex node)
        {
            DefaultBefore(node);
            base.VisitRegex(node);
            DefaultAfter();
        }

        public override void VisitReturn(BoundReturn node)
        {
            Write("Return");
            if (node.Expression != null)
            {
                Write(" ");
                Visit(node.Expression);
            }
        }

        public override void VisitSetAccessor(BoundSetAccessor node)
        {
            DefaultBefore(node);
            base.VisitSetAccessor(node);
            DefaultAfter();
        }

        public override void VisitSetMember(BoundSetMember node)
        {
            Visit(node.Expression);
            Write("[");
            Visit(node.Index);
            Write("] = ");
            Visit(node.Value);
        }

        public override void VisitSetVariable(BoundSetVariable node)
        {
            Write(node.Variable.ToString());
            Write(" = ");
            Visit(node.Value);
        }

        public override void VisitSwitch(BoundSwitch node)
        {
            WriteLine("Switch <{0}> (", node.Temporary);

            _indent++;

            WriteIndent();

            base.VisitSwitch(node);

            DefaultAfter();
        }

        public override void VisitSwitchCase(BoundSwitchCase node)
        {
            DefaultBefore(node);
            base.VisitSwitchCase(node);
            DefaultAfter();
        }

        public override void VisitThrow(BoundThrow node)
        {
            DefaultBefore(node);
            base.VisitThrow(node);
            DefaultAfter();
        }

        public override void VisitTry(BoundTry node)
        {
            DefaultBefore(node);
            base.VisitTry(node);
            DefaultAfter();
        }

        public override void VisitUnary(BoundUnary node)
        {
            Write(GetOperation(node.Operation));
            Write(" ");
            Visit(node.Operand);
        }

        public override void VisitWhile(BoundWhile node)
        {
            Write("While (");
            Visit(node.Test);
            Write(") ");
            Visit(node.Body);
        }

        private string GetOperation(BoundExpressionType type)
        {
            switch (type)
            {
                case BoundExpressionType.Add: return "+";
                case BoundExpressionType.BitwiseAnd: return "&";
                case BoundExpressionType.BitwiseExclusiveOr: return "^";
                case BoundExpressionType.BitwiseNot: return "~";
                case BoundExpressionType.BitwiseOr: return "|";
                case BoundExpressionType.Divide: return "/";
                case BoundExpressionType.Equal: return "==";
                case BoundExpressionType.GreaterThan: return ">";
                case BoundExpressionType.GreaterThanOrEqual: return ">=";
                case BoundExpressionType.In: return "in";
                case BoundExpressionType.InstanceOf: return "instanceof";
                case BoundExpressionType.LeftShift: return "<<";
                case BoundExpressionType.LessThan: return "<";
                case BoundExpressionType.LessThanOrEqual: return "<=";
                case BoundExpressionType.Modulo: return "%";
                case BoundExpressionType.Multiply: return "*";
                case BoundExpressionType.Negate: return "-";
                case BoundExpressionType.Not: return "!";
                case BoundExpressionType.NotEqual: return "!=";
                case BoundExpressionType.NotSame: return "!==";
                case BoundExpressionType.RightShift: return ">>";
                case BoundExpressionType.Same: return "===";
                case BoundExpressionType.Subtract: return "-";
                case BoundExpressionType.TypeOf: return "typeof";
                case BoundExpressionType.UnaryPlus: return "+";
                case BoundExpressionType.UnsignedRightShift: return ">>>";
                case BoundExpressionType.Void: return "void";
                default: throw new ArgumentOutOfRangeException("type");
            }
        }
    }
}
