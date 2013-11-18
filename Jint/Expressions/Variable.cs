﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Expressions
{
    internal class Variable
    {
        public static readonly Variable This = new Variable(VariableType.This);

        public string Name { get; private set; }
        public int Index { get; private set; }
        public VariableType Type { get; set; }
        public ClosedOverVariable ClosureField { get; set; }
        public WithScope WithScope { get; set; }
        public Variable FallbackVariable { get; set; }

        public Variable(string name, int index)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            Name = name;
            Index = index;
        }

        public Variable(Variable fallbackVariable, WithScope withScope)
        {
            if (fallbackVariable == null)
                throw new ArgumentNullException("fallbackVariable");
            if (withScope == null)
                throw new ArgumentNullException("withScope");

            FallbackVariable = fallbackVariable;
            WithScope = withScope;
            Type = VariableType.WithScope;
        }

        private Variable(VariableType type)
        {
            Type = type;
        }

        public override string ToString()
        {
            return Name + " [" + Type + (ClosureField != null ? "*" : "") + "]";
        }
    }
}
