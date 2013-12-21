using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal static class TypeMarkerPhase
    {
        public static void Perform(BoundProgram node)
        {
            Perform(node.Body);
        }

        private static void Perform(BoundBody body)
        {
            new Marker(body.TypeManager).Visit(body);
        }

        private class Marker : BoundTreeWalker
        {
            private readonly TypeResolver _typeResolver = new TypeResolver();
            private readonly BoundTypeManager _typeManager;
            private readonly List<Block> _blocks = new List<Block>();
            private readonly Dictionary<BoundStatement, string> _labels = new Dictionary<BoundStatement, string>();
            private readonly BoundTypeManager.MarkerBlock _rootBlock;
            private BoundTypeManager.Marker _marker;

            public Marker(BoundTypeManager typeManager)
            {
                _typeManager = typeManager;
                _rootBlock = _typeManager.CreateTypeMarker().RootBlock;
                _blocks.Add(new Block(null, _rootBlock));
                _marker = _rootBlock.Marker;
            }

            private void MarkRead(IBoundReadable variable)
            {
                var hasType = variable as IHasBoundType;
                if (hasType != null)
                    _marker.MarkRead(hasType.Type);
            }

            private void MarkWrite(IBoundWritable variable, BoundValueType type)
            {
                var hasType = variable as IHasBoundType;
                if (hasType != null)
                    _marker.MarkWrite(hasType.Type, type);
            }

            public override void VisitSetVariable(BoundSetVariable node)
            {
                MarkWrite(node.Variable, node.Value.Accept(_typeResolver));

                base.VisitSetVariable(node);
            }

            public override void VisitGetVariable(BoundGetVariable node)
            {
                MarkRead(node.Variable);

                base.VisitGetVariable(node);
            }

            public override void VisitForEachIn(BoundForEachIn node)
            {
                MarkWrite(node.Target, BoundValueType.Object);

                base.VisitForEachIn(node);
            }

            public override void VisitCatch(BoundCatch node)
            {
                MarkWrite(node.Target, BoundValueType.Unknown);

                base.VisitCatch(node);
            }

            public override void VisitCreateFunction(BoundCreateFunction node)
            {
                base.VisitCreateFunction(node);
            }

            public override void VisitExpressionBlock(BoundExpressionBlock node)
            {
                MarkRead(node.Result);

                base.VisitExpressionBlock(node);
            }

            public override void VisitLabel(BoundLabel node)
            {
                _labels.Add(node.Statement, node.Label);

                base.VisitLabel(node);
            }

            private class Block
            {
                public string Name { get; private set; }
                public BoundTypeManager.MarkerBlock Marker { get; private set; }

                public Block(string name, BoundTypeManager.MarkerBlock marker)
                {
                    Name = name;
                    Marker = marker;
                }
            }
        }

        private class TypeResolver : BoundTreeVisitor<BoundValueType>
        {
            private readonly Dictionary<BoundNode, BoundValueType> _resolved = new Dictionary<BoundNode, BoundValueType>();

            public override BoundValueType DefaultVisit(BoundNode node)
            {
                BoundValueType result;
                if (!_resolved.TryGetValue(node, out result))
                {
                    result = base.DefaultVisit(node);
                    _resolved.Add(node, result);
                }

                return result;
            }
        }
    }
}
