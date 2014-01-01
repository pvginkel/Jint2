using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal class BoundTreeRewriter : BoundTreeVisitor<BoundNode>
    {
        public virtual BoundNode Visit(BoundNode node)
        {
            if (node != null)
                return node.Accept(this);

            return null;
        }

        private ReadOnlyArray<T> VisitList<T>(ReadOnlyArray<T> items)
            where T : BoundNode
        {
            var result = new ReadOnlyArray<T>.Builder(items.Count);

            bool changed = false;

            foreach (var item in items)
            {
                var updated = Visit(item);
                if (updated != item)
                    changed = true;

                result.Add((T)updated);
            }

            if (changed)
                return result.ToReadOnly();

            return items;
        }

        public override BoundNode VisitBinary(BoundBinary node)
        {
            return node.Update(
                node.Operation,
                (BoundExpression)Visit(node.Left),
                (BoundExpression)Visit(node.Right)
            );
        }

        public override BoundNode VisitBlock(BoundBlock node)
        {
            return node.Update(
                node.Temporaries,
                VisitList(node.Nodes),
                node.Location
            );
        }

        public override BoundNode VisitBody(BoundBody node)
        {
            return node.Update(
                (BoundBlock)Visit(node.Body),
                node.Closure,
                node.ScopedClosure,
                node.Arguments,
                node.Locals,
                node.MappedArguments,
                node.Flags,
                node.TypeManager
            );
        }

        public override BoundNode VisitBreak(BoundBreak node)
        {
            return node.Update(
                node.Target,
                node.Location
            );
        }

        public override BoundNode VisitCall(BoundCall node)
        {
            return node.Update(
                (BoundExpression)Visit(node.Target),
                (BoundExpression)Visit(node.Method),
                VisitList(node.Arguments),
                VisitList(node.Generics)
            );
        }

        public override BoundNode VisitCallArgument(BoundCallArgument node)
        {
            return node.Update(
                (BoundExpression)Visit(node.Expression),
                node.IsRef
            );
        }

        public override BoundNode VisitCatch(BoundCatch node)
        {
            return node.Update(
                node.Target,
                (BoundBlock)Visit(node.Body)
            );
        }

        public override BoundNode VisitConstant(BoundConstant node)
        {
            return node.Update(
                node.Value
            );
        }

        public override BoundNode VisitContinue(BoundContinue node)
        {
            return node.Update(
                node.Target,
                node.Location
            );
        }

        public override BoundNode VisitCreateFunction(BoundCreateFunction node)
        {
            return node.Update(
                node.Function
            );
        }

        public override BoundNode VisitDeleteMember(BoundDeleteMember node)
        {
            return node.Update(
                (BoundExpression)Visit(node.Expression),
                (BoundExpression)Visit(node.Index)
            );
        }

        public override BoundNode VisitDoWhile(BoundDoWhile node)
        {
            return node.Update(
                (BoundExpression)Visit(node.Test),
                (BoundBlock)Visit(node.Body),
                node.Location
            );
        }

        public override BoundNode VisitEmpty(BoundEmpty node)
        {
            return node.Update(
                node.Location
            );
        }

        public override BoundNode VisitExpressionBlock(BoundExpressionBlock node)
        {
            return node.Update(
                node.Result,
                (BoundBlock)Visit(node.Body)
            );
        }

        public override BoundNode VisitExpressionStatement(BoundExpressionStatement node)
        {
            return node.Update(
                (BoundExpression)Visit(node.Expression),
                node.Location
            );
        }

        public override BoundNode VisitFinally(BoundFinally node)
        {
            return node.Update(
                (BoundBlock)Visit(node.Body)
            );
        }

        public override BoundNode VisitFor(BoundFor node)
        {
            return node.Update(
                (BoundBlock)Visit(node.Initialization),
                (BoundExpression)Visit(node.Test),
                (BoundBlock)Visit(node.Increment),
                (BoundBlock)Visit(node.Body),
                node.Location
            );
        }

        public override BoundNode VisitForEachIn(BoundForEachIn node)
        {
            return node.Update(
                node.Target,
                (BoundExpression)Visit(node.Expression),
                (BoundBlock)Visit(node.Body),
                node.Location
            );
        }

        public override BoundNode VisitGetMember(BoundGetMember node)
        {
            return node.Update(
                (BoundExpression)Visit(node.Expression),
                (BoundExpression)Visit(node.Index)
            );
        }

        public override BoundNode VisitGetVariable(BoundGetVariable node)
        {
            return node.Update(
                node.Variable
            );
        }

        public override BoundNode VisitHasMember(BoundHasMember node)
        {
            return node.Update(
                (BoundExpression)Visit(node.Expression),
                node.Index
            );
        }

        public override BoundNode VisitIf(BoundIf node)
        {
            return node.Update(
                (BoundExpression)Visit(node.Test),
                (BoundBlock)Visit(node.Then),
                (BoundBlock)Visit(node.Else),
                node.Location
            );
        }

        public override BoundNode VisitLabel(BoundLabel node)
        {
            return node.Update(
                node.Label,
                (BoundStatement)Visit(node.Statement),
                node.Location
            );
        }

        public override BoundNode VisitNew(BoundNew node)
        {
            return node.Update(
                (BoundExpression)Visit(node.Expression),
                VisitList(node.Arguments),
                VisitList(node.Generics)
            );
        }

        public override BoundNode VisitNewBuiltIn(BoundNewBuiltIn node)
        {
            return node.Update(
                node.NewBuiltInType
            );
        }

        public override BoundNode VisitRegex(BoundRegEx node)
        {
            return node.Update(
                node.Regex,
                node.Options
            );
        }

        public override BoundNode VisitReturn(BoundReturn node)
        {
            return node.Update(
                (BoundExpression)Visit(node.Expression),
                node.Location
            );
        }

        public override BoundNode VisitSetAccessor(BoundSetAccessor node)
        {
            return node.Update(
                (BoundExpression)Visit(node.Expression),
                node.Name,
                (BoundExpression)Visit(node.GetFunction),
                (BoundExpression)Visit(node.SetFunction),
                node.Location
            );
        }

        public override BoundNode VisitSetMember(BoundSetMember node)
        {
            return node.Update(
                (BoundExpression)Visit(node.Expression),
                (BoundExpression)Visit(node.Index),
                (BoundExpression)Visit(node.Value),
                node.Location
            );
        }

        public override BoundNode VisitSetVariable(BoundSetVariable node)
        {
            return node.Update(
                node.Variable,
                (BoundExpression)Visit(node.Value),
                node.Location
            );
        }

        public override BoundNode VisitSwitch(BoundSwitch node)
        {
            return node.Update(
                node.Temporary,
                VisitList(node.Cases),
                node.Location
            );
        }

        public override BoundNode VisitSwitchCase(BoundSwitchCase node)
        {
            return node.Update(
                (BoundExpression)Visit(node.Expression),
                (BoundBlock)Visit(node.Body),
                node.Location
            );
        }

        public override BoundNode VisitThrow(BoundThrow node)
        {
            return node.Update(
                (BoundExpression)Visit(node.Expression),
                node.Location
            );
        }

        public override BoundNode VisitTry(BoundTry node)
        {
            return node.Update(
                (BoundBlock)Visit(node.Try),
                (BoundCatch)Visit(node.Catch),
                (BoundFinally)Visit(node.Finally),
                node.Location
            );
        }

        public override BoundNode VisitUnary(BoundUnary node)
        {
            return node.Update(
                node.Operation,
                (BoundExpression)Visit(node.Operand)
            );
        }

        public override BoundNode VisitWhile(BoundWhile node)
        {
            return node.Update(
                (BoundExpression)Visit(node.Test),
                (BoundBlock)Visit(node.Body),
                node.Location
            );
        }
    }
}
