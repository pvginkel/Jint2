using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal abstract class BoundTreeVisitor
    {
        [DebuggerStepThrough]
        public virtual void DefaultVisit(BoundNode node)
        {
        }

        [DebuggerStepThrough]
        public virtual void VisitBinary(BoundBinary node)
        {
            DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual void VisitBlock(BoundBlock node)
        {
            DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual void VisitBody(BoundBody node)
        {
            DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual void VisitBreak(BoundBreak node)
        {
            DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual void VisitCall(BoundCall node)
        {
            DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual void VisitCallArgument(BoundCallArgument node)
        {
            DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual void VisitCatch(BoundCatch node)
        {
            DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual void VisitConstant(BoundConstant node)
        {
            DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual void VisitContinue(BoundContinue node)
        {
            DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual void VisitCreateFunction(BoundCreateFunction node)
        {
            DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual void VisitDeleteMember(BoundDeleteMember node)
        {
            DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual void VisitDoWhile(BoundDoWhile node)
        {
            DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual void VisitEmpty(BoundEmpty node)
        {
            DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual void VisitExpressionBlock(BoundExpressionBlock node)
        {
            DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual void VisitExpressionStatement(BoundExpressionStatement node)
        {
            DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual void VisitFinally(BoundFinally node)
        {
            DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual void VisitFor(BoundFor node)
        {
            DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual void VisitForEachIn(BoundForEachIn node)
        {
            DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual void VisitGetMember(BoundGetMember node)
        {
            DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual void VisitGetVariable(BoundGetVariable node)
        {
            DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual void VisitHasMember(BoundHasMember node)
        {
            DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual void VisitIf(BoundIf node)
        {
            DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual void VisitLabel(BoundLabel node)
        {
            DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual void VisitNew(BoundNew node)
        {
            DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual void VisitNewBuiltIn(BoundNewBuiltIn node)
        {
            DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual void VisitRegex(BoundRegEx node)
        {
            DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual void VisitReturn(BoundReturn node)
        {
            DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual void VisitSetAccessor(BoundSetAccessor node)
        {
            DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual void VisitSetMember(BoundSetMember node)
        {
            DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual void VisitSetVariable(BoundSetVariable node)
        {
            DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual void VisitSwitch(BoundSwitch node)
        {
            DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual void VisitSwitchCase(BoundSwitchCase node)
        {
            DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual void VisitThrow(BoundThrow node)
        {
            DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual void VisitTry(BoundTry node)
        {
            DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual void VisitUnary(BoundUnary node)
        {
            DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual void VisitWhile(BoundWhile node)
        {
            DefaultVisit(node);
        }
    }

    internal abstract class BoundTreeVisitor<T>
    {
        [DebuggerStepThrough]
        public virtual T DefaultVisit(BoundNode node)
        {
            return default(T);
        }

        [DebuggerStepThrough]
        public virtual T VisitBinary(BoundBinary node)
        {
            return DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual T VisitBlock(BoundBlock node)
        {
            return DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual T VisitBody(BoundBody node)
        {
            return DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual T VisitBreak(BoundBreak node)
        {
            return DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual T VisitCall(BoundCall node)
        {
            return DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual T VisitCallArgument(BoundCallArgument node)
        {
            return DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual T VisitCatch(BoundCatch node)
        {
            return DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual T VisitConstant(BoundConstant node)
        {
            return DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual T VisitContinue(BoundContinue node)
        {
            return DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual T VisitCreateFunction(BoundCreateFunction node)
        {
            return DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual T VisitDeleteMember(BoundDeleteMember node)
        {
            return DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual T VisitDoWhile(BoundDoWhile node)
        {
            return DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual T VisitEmpty(BoundEmpty node)
        {
            return DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual T VisitExpressionBlock(BoundExpressionBlock node)
        {
            return DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual T VisitExpressionStatement(BoundExpressionStatement node)
        {
            return DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual T VisitFinally(BoundFinally node)
        {
            return DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual T VisitFor(BoundFor node)
        {
            return DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual T VisitForEachIn(BoundForEachIn node)
        {
            return DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual T VisitGetMember(BoundGetMember node)
        {
            return DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual T VisitGetVariable(BoundGetVariable node)
        {
            return DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual T VisitHasMember(BoundHasMember node)
        {
            return DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual T VisitIf(BoundIf node)
        {
            return DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual T VisitLabel(BoundLabel node)
        {
            return DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual T VisitNew(BoundNew node)
        {
            return DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual T VisitNewBuiltIn(BoundNewBuiltIn node)
        {
            return DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual T VisitRegex(BoundRegEx node)
        {
            return DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual T VisitReturn(BoundReturn node)
        {
            return DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual T VisitSetAccessor(BoundSetAccessor node)
        {
            return DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual T VisitSetMember(BoundSetMember node)
        {
            return DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual T VisitSetVariable(BoundSetVariable node)
        {
            return DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual T VisitSwitch(BoundSwitch node)
        {
            return DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual T VisitSwitchCase(BoundSwitchCase node)
        {
            return DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual T VisitThrow(BoundThrow node)
        {
            return DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual T VisitTry(BoundTry node)
        {
            return DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual T VisitUnary(BoundUnary node)
        {
            return DefaultVisit(node);
        }

        [DebuggerStepThrough]
        public virtual T VisitWhile(BoundWhile node)
        {
            return DefaultVisit(node);
        }
    }
}
