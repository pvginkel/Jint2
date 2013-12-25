using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal class BoundTreeWalker : BoundTreeVisitor
    {
        [DebuggerStepThrough]
        public virtual void Visit(BoundNode node)
        {
            if (node != null)
                node.Accept(this);
        }

        [DebuggerStepThrough]
        public virtual void VisitList<T>(ReadOnlyArray<T> items)
            where T : BoundNode
        {
            foreach (var item in items)
            {
                Visit(item);
            }
        }

        [DebuggerStepThrough]
        public override void VisitBinary(BoundBinary node)
        {
            Visit(node.Left);
            Visit(node.Right);
        }

        [DebuggerStepThrough]
        public override void VisitBlock(BoundBlock node)
        {
            VisitList(node.Nodes);
        }

        [DebuggerStepThrough]
        public override void VisitBody(BoundBody node)
        {
            Visit(node.Body);
        }

        [DebuggerStepThrough]
        public override void VisitBreak(BoundBreak node)
        {
        }

        [DebuggerStepThrough]
        public override void VisitCall(BoundCall node)
        {
            Visit(node.Target);
            Visit(node.Method);
            VisitList(node.Arguments);
            VisitList(node.Generics);
        }

        [DebuggerStepThrough]
        public override void VisitCallArgument(BoundCallArgument node)
        {
            Visit(node.Expression);
        }

        [DebuggerStepThrough]
        public override void VisitCatch(BoundCatch node)
        {
            Visit(node.Body);
        }

        [DebuggerStepThrough]
        public override void VisitConstant(BoundConstant node)
        {
        }

        [DebuggerStepThrough]
        public override void VisitContinue(BoundContinue node)
        {
        }

        [DebuggerStepThrough]
        public override void VisitCreateFunction(BoundCreateFunction node)
        {
        }

        [DebuggerStepThrough]
        public override void VisitDeleteMember(BoundDeleteMember node)
        {
            Visit(node.Expression);
            Visit(node.Index);
        }

        [DebuggerStepThrough]
        public override void VisitDoWhile(BoundDoWhile node)
        {
            Visit(node.Test);
            Visit(node.Body);
        }

        [DebuggerStepThrough]
        public override void VisitEmpty(BoundEmpty node)
        {
        }

        [DebuggerStepThrough]
        public override void VisitExpressionBlock(BoundExpressionBlock node)
        {
            Visit(node.Body);
        }

        [DebuggerStepThrough]
        public override void VisitExpressionStatement(BoundExpressionStatement node)
        {
            Visit(node.Expression);
        }

        [DebuggerStepThrough]
        public override void VisitFinally(BoundFinally node)
        {
            Visit(node.Body);
        }

        [DebuggerStepThrough]
        public override void VisitFor(BoundFor node)
        {
            Visit(node.Initialization);
            Visit(node.Test);
            Visit(node.Increment);
            Visit(node.Body);
        }

        [DebuggerStepThrough]
        public override void VisitForEachIn(BoundForEachIn node)
        {
            Visit(node.Expression);
            Visit(node.Body);
        }

        [DebuggerStepThrough]
        public override void VisitGetMember(BoundGetMember node)
        {
            Visit(node.Expression);
            Visit(node.Index);
        }

        [DebuggerStepThrough]
        public override void VisitGetVariable(BoundGetVariable node)
        {
        }

        [DebuggerStepThrough]
        public override void VisitHasMember(BoundHasMember node)
        {
            Visit(node.Expression);
        }

        [DebuggerStepThrough]
        public override void VisitIf(BoundIf node)
        {
            Visit(node.Test);
            Visit(node.Then);
            Visit(node.Else);
        }

        [DebuggerStepThrough]
        public override void VisitLabel(BoundLabel node)
        {
            Visit(node.Statement);
        }

        [DebuggerStepThrough]
        public override void VisitNew(BoundNew node)
        {
            Visit(node.Expression);
            VisitList(node.Arguments);
            VisitList(node.Generics);
        }

        [DebuggerStepThrough]
        public override void VisitNewBuiltIn(BoundNewBuiltIn node)
        {
        }

        [DebuggerStepThrough]
        public override void VisitRegex(BoundRegEx node)
        {
        }

        [DebuggerStepThrough]
        public override void VisitReturn(BoundReturn node)
        {
            Visit(node.Expression);
        }

        [DebuggerStepThrough]
        public override void VisitSetAccessor(BoundSetAccessor node)
        {
            Visit(node.Expression);
            Visit(node.GetFunction);
            Visit(node.SetFunction);
        }

        [DebuggerStepThrough]
        public override void VisitSetMember(BoundSetMember node)
        {
            Visit(node.Expression);
            Visit(node.Index);
            Visit(node.Value);
        }

        [DebuggerStepThrough]
        public override void VisitSetVariable(BoundSetVariable node)
        {
            Visit(node.Value);
        }

        [DebuggerStepThrough]
        public override void VisitSwitch(BoundSwitch node)
        {
            VisitList(node.Cases);
        }

        [DebuggerStepThrough]
        public override void VisitSwitchCase(BoundSwitchCase node)
        {
            Visit(node.Expression);
            Visit(node.Body);
        }

        [DebuggerStepThrough]
        public override void VisitThrow(BoundThrow node)
        {
            Visit(node.Expression);
        }

        [DebuggerStepThrough]
        public override void VisitTry(BoundTry node)
        {
            Visit(node.Try);
            Visit(node.Catch);
            Visit(node.Finally);
        }

        [DebuggerStepThrough]
        public override void VisitUnary(BoundUnary node)
        {
            Visit(node.Operand);
        }

        [DebuggerStepThrough]
        public override void VisitWhile(BoundWhile node)
        {
            Visit(node.Test);
            Visit(node.Body);
        }
    }
}
