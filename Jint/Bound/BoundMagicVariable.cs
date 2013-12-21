using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal class BoundMagicVariable : IBoundReadable
    {
        public static readonly BoundMagicVariable This = new BoundMagicVariable(BoundMagicVariableType.This);
        public static readonly BoundMagicVariable Global = new BoundMagicVariable(BoundMagicVariableType.Global);
        //public static readonly BoundMagicVariable Arguments = new BoundMagicVariable(BoundMagicVariableType.Arguments);
        //public static readonly BoundMagicVariable Null = new BoundMagicVariable(BoundMagicVariableType.Null);
        //public static readonly BoundMagicVariable Undefined = new BoundMagicVariable(BoundMagicVariableType.Undefined);

        public BoundMagicVariableType VariableType { get; private set; }

        private BoundMagicVariable(BoundMagicVariableType variableType)
        {
            VariableType = variableType;
        }

        public override string ToString()
        {
            return "Magic(" + VariableType + ")";
        }
    }
}
