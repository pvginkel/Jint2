using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal static class TypeMarkerPhase
    {
        public static void Perform(BoundProgram node)
        {
            Perform(node.Body, false);
        }

        private static void Perform(BoundBody body, bool isFunction)
        {
            new Marker(body.TypeManager, isFunction).Visit(body);
        }

        private class Marker : BoundTreeWalker
        {
            private readonly bool _isFunction;
            private readonly TypeResolver _typeResolver = new TypeResolver();
            private readonly BoundTypeManager.TypeMarker _marker;

            public Marker(BoundTypeManager typeManager, bool isFunction)
            {
                _isFunction = isFunction;
                _marker = typeManager.CreateTypeMarker();
            }

            public override void VisitBody(BoundBody node)
            {
                // Mark the arguments local as object.

                if (_isFunction)
                {
                    var argumentsLocal = node.Locals.SingleOrDefault(p => p.Name == "arguments");
                    if (argumentsLocal != null)
                        MarkWrite(argumentsLocal, BoundValueType.Object);
                }

                base.VisitBody(node);
            }

            private void MarkWrite(IBoundWritable variable, BoundValueType type)
            {
                var hasType = variable as BoundVariable;
                if (hasType != null)
                    _marker.MarkWrite(hasType.Type, type);
            }

            public override void VisitSetVariable(BoundSetVariable node)
            {
                base.VisitSetVariable(node);

                MarkWrite(node.Variable, node.Value.Accept(_typeResolver));
            }

            public override void VisitForEachIn(BoundForEachIn node)
            {
                MarkWrite(node.Target, BoundValueType.String);

                base.VisitForEachIn(node);
            }

            public override void VisitCatch(BoundCatch node)
            {
                MarkWrite(node.Target, BoundValueType.Unknown);

                base.VisitCatch(node);
            }

            public override void VisitCreateFunction(BoundCreateFunction node)
            {
                // It's the responsibility of the phase to correctly process
                // functions.

                Perform(node.Function.Body, true);
            }
        }

        private class TypeResolver : BoundTreeVisitor<BoundValueType>
        {
            public override BoundValueType DefaultVisit(BoundNode node)
            {
                throw new InvalidOperationException("Cannot get type of specified node");
            }

            public override BoundValueType VisitNewBuiltIn(BoundNewBuiltIn node)
            {
                return BoundValueType.Object;
            }

            public override BoundValueType VisitBinary(BoundBinary node)
            {
                var left = node.Left.Accept(this);
                var right = node.Right.Accept(this);

                switch (node.Operation)
                {
                    case BoundExpressionType.Add:
                        if (left == BoundValueType.String || right == BoundValueType.String)
                            return BoundValueType.String;

                        return BoundValueType.Number;

                    case BoundExpressionType.BitwiseAnd:
                    case BoundExpressionType.BitwiseExclusiveOr:
                    case BoundExpressionType.BitwiseOr:
                    case BoundExpressionType.Divide:
                    case BoundExpressionType.LeftShift:
                    case BoundExpressionType.RightShift:
                    case BoundExpressionType.UnsignedRightShift:
                    case BoundExpressionType.Modulo:
                    case BoundExpressionType.Multiply:
                    case BoundExpressionType.Subtract:
                        return BoundValueType.Number;

                    case BoundExpressionType.Equal:
                    case BoundExpressionType.NotEqual:
                    case BoundExpressionType.Same:
                    case BoundExpressionType.NotSame:
                    case BoundExpressionType.LessThan:
                    case BoundExpressionType.LessThanOrEqual:
                    case BoundExpressionType.GreaterThan:
                    case BoundExpressionType.GreaterThanOrEqual:
                    case BoundExpressionType.In:
                    case BoundExpressionType.InstanceOf:
                        return BoundValueType.Boolean;

                    default:
                        throw new InvalidOperationException();
                }
            }

            public override BoundValueType VisitGetVariable(BoundGetVariable node)
            {
                return node.Variable.ValueType;
            }

            public override BoundValueType VisitGetMember(BoundGetMember node)
            {
                return BoundValueType.Unknown;
            }

            public override BoundValueType VisitCreateFunction(BoundCreateFunction node)
            {
                return BoundValueType.Object;
            }

            public override BoundValueType VisitCall(BoundCall node)
            {
                return BoundValueType.Unknown;
            }

            public override BoundValueType VisitNew(BoundNew node)
            {
                return BoundValueType.Object;
            }

            public override BoundValueType VisitRegex(BoundRegex node)
            {
                return BoundValueType.Object;
            }

            public override BoundValueType VisitUnary(BoundUnary node)
            {
                switch (node.Operation)
                {
                    case BoundExpressionType.BitwiseNot:
                    case BoundExpressionType.Negate:
                    case BoundExpressionType.UnaryPlus:
                        return BoundValueType.Number;

                    case BoundExpressionType.Not:
                        return BoundValueType.Boolean;

                    case BoundExpressionType.TypeOf:
                        return BoundValueType.String;

                    case BoundExpressionType.Void:
                        return BoundValueType.Unknown;

                    default:
                        throw new ArgumentOutOfRangeException("operand");
                }
            }

            public override BoundValueType VisitConstant(BoundConstant node)
            {
                if (node.Value is string)
                    return BoundValueType.String;
                if (node.Value is double)
                    return BoundValueType.Number;

                Debug.Assert(node.Value is bool);

                return BoundValueType.Boolean;
            }

            public override BoundValueType VisitDeleteMember(BoundDeleteMember node)
            {
                return BoundValueType.Boolean;
            }

            public override BoundValueType VisitExpressionBlock(BoundExpressionBlock node)
            {
                return node.Result.ValueType;
            }

            public override BoundValueType VisitHasMember(BoundHasMember node)
            {
                return BoundValueType.Boolean;
            }
        }
    }
}
