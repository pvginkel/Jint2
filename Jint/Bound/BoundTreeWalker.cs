using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jint.Support;

namespace Jint.Bound
{
    internal class BoundTreeWalker : BoundTreeVisitor
    {
        public virtual void Visit(BoundNode node)
        {
            if (node != null)
                node.Accept(this);
        }

        public virtual void VisitList<T>(ReadOnlyArray<T> items)
            where T : BoundNode
        {
            foreach (var item in items)
            {
                Visit(item);
            }
        }

        public override void VisitBinary(BoundBinary node)
        {
            Visit(node.Left);
            Visit(node.Right);
        }

        public override void VisitBlock(BoundBlock node)
        {
            VisitList(node.Nodes);
        }

        public override void VisitBody(BoundBody node)
        {
            Visit(node.Body);
        }

        public override void VisitBreak(BoundBreak node)
        {
        }

        public override void VisitCall(BoundCall node)
        {
            Visit(node.Target);
            Visit(node.Method);
            VisitList(node.Arguments);
            VisitList(node.Generics);
        }

        public override void VisitCallArgument(BoundCallArgument node)
        {
            Visit(node.Expression);
        }

        public override void VisitCatch(BoundCatch node)
        {
            Visit(node.Body);
        }

        public override void VisitConstant(BoundConstant node)
        {
        }

        public override void VisitContinue(BoundContinue node)
        {
        }

        public override void VisitCreateFunction(BoundCreateFunction node)
        {
        }

        public override void VisitDeleteMember(BoundDeleteMember node)
        {
            Visit(node.Expression);
            Visit(node.Index);
        }

        public override void VisitDoWhile(BoundDoWhile node)
        {
            Visit(node.Test);
            Visit(node.Body);
        }

        public override void VisitEmpty(BoundEmpty node)
        {
        }

        public override void VisitExpressionBlock(BoundExpressionBlock node)
        {
            Visit(node.Body);
        }

        public override void VisitExpressionStatement(BoundExpressionStatement node)
        {
            Visit(node.Expression);
        }

        public override void VisitFinally(BoundFinally node)
        {
            Visit(node.Body);
        }

        public override void VisitFor(BoundFor node)
        {
            Visit(node.Initialization);
            Visit(node.Test);
            Visit(node.Increment);
            Visit(node.Body);
        }

        public override void VisitForEachIn(BoundForEachIn node)
        {
            Visit(node.Expression);
            Visit(node.Body);
        }

        public override void VisitGetMember(BoundGetMember node)
        {
            Visit(node.Expression);
            Visit(node.Index);
        }

        public override void VisitGetVariable(BoundGetVariable node)
        {
        }

        public override void VisitHasMember(BoundHasMember node)
        {
            Visit(node.Expression);
            Visit(node.Index);
        }

        public override void VisitIf(BoundIf node)
        {
            Visit(node.Test);
            Visit(node.Then);
            Visit(node.Else);
        }

        public override void VisitLabel(BoundLabel node)
        {
            Visit(node.Statement);
        }

        public override void VisitNew(BoundNew node)
        {
            Visit(node.Expression);
            VisitList(node.Arguments);
            VisitList(node.Generics);
        }

        public override void VisitNewBuiltIn(BoundNewBuiltIn node)
        {
        }

        public override void VisitRegex(BoundRegex node)
        {
        }

        public override void VisitReturn(BoundReturn node)
        {
            Visit(node.Expression);
        }

        public override void VisitSetAccessor(BoundSetAccessor node)
        {
            Visit(node.Expression);
            Visit(node.GetFunction);
            Visit(node.SetFunction);
        }

        public override void VisitSetMember(BoundSetMember node)
        {
            Visit(node.Expression);
            Visit(node.Index);
            Visit(node.Value);
        }

        public override void VisitSetVariable(BoundSetVariable node)
        {
            Visit(node.Value);
        }

        public override void VisitSwitch(BoundSwitch node)
        {
            VisitList(node.Cases);
        }

        public override void VisitSwitchCase(BoundSwitchCase node)
        {
            Visit(node.Expression);
            Visit(node.Body);
        }

        public override void VisitThrow(BoundThrow node)
        {
            Visit(node.Expression);
        }

        public override void VisitTry(BoundTry node)
        {
            Visit(node.Try);
            Visit(node.Catch);
            Visit(node.Finally);
        }

        public override void VisitUnary(BoundUnary node)
        {
            Visit(node.Operand);
        }

        public override void VisitWhile(BoundWhile node)
        {
            Visit(node.Test);
            Visit(node.Body);
        }
    }
}
