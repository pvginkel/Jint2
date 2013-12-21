using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal abstract class BoundTreeVisitor
    {
        public virtual void DefaultVisit(BoundNode node)
        {
        }

        public virtual void VisitBinary(BoundBinary node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitBlock(BoundBlock node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitBody(BoundBody node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitBreak(BoundBreak node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCall(BoundCall node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCallArgument(BoundCallArgument node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCatch(BoundCatch node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitConstant(BoundConstant node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitContinue(BoundContinue node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCreateFunction(BoundCreateFunction node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitDeleteMember(BoundDeleteMember node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitDoWhile(BoundDoWhile node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitEmpty(BoundEmpty node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitExpressionBlock(BoundExpressionBlock node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitExpressionStatement(BoundExpressionStatement node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitFinally(BoundFinally node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitFor(BoundFor node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitForEachIn(BoundForEachIn node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitGetMember(BoundGetMember node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitGetVariable(BoundGetVariable node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitHasMember(BoundHasMember node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitIf(BoundIf node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitLabel(BoundLabel node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitNew(BoundNew node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitNewBuiltIn(BoundNewBuiltIn node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitRegex(BoundRegex node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitReturn(BoundReturn node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitSetAccessor(BoundSetAccessor node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitSetMember(BoundSetMember node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitSetVariable(BoundSetVariable node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitSwitch(BoundSwitch node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitSwitchCase(BoundSwitchCase node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitThrow(BoundThrow node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitTry(BoundTry node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitUnary(BoundUnary node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitWhile(BoundWhile node)
        {
            DefaultVisit(node);
        }
    }

    internal abstract class BoundTreeVisitor<T>
    {
        public virtual T DefaultVisit(BoundNode node)
        {
            return default(T);
        }

        public virtual T VisitBinary(BoundBinary node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitBlock(BoundBlock node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitBody(BoundBody node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitBreak(BoundBreak node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitCall(BoundCall node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitCallArgument(BoundCallArgument node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitCatch(BoundCatch node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitConstant(BoundConstant node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitContinue(BoundContinue node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitCreateFunction(BoundCreateFunction node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitDeleteMember(BoundDeleteMember node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitDoWhile(BoundDoWhile node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitEmpty(BoundEmpty node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitExpressionBlock(BoundExpressionBlock node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitExpressionStatement(BoundExpressionStatement node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitFinally(BoundFinally node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitFor(BoundFor node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitForEachIn(BoundForEachIn node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitGetMember(BoundGetMember node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitGetVariable(BoundGetVariable node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitHasMember(BoundHasMember node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitIf(BoundIf node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitLabel(BoundLabel node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitNew(BoundNew node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitNewBuiltIn(BoundNewBuiltIn node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitRegex(BoundRegex node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitReturn(BoundReturn node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitSetAccessor(BoundSetAccessor node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitSetMember(BoundSetMember node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitSetVariable(BoundSetVariable node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitSwitch(BoundSwitch node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitSwitchCase(BoundSwitchCase node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitThrow(BoundThrow node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitTry(BoundTry node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitUnary(BoundUnary node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitWhile(BoundWhile node)
        {
            return DefaultVisit(node);
        }
    }
}
