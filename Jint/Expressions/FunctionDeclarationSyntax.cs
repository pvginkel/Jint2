﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Expressions
{
    public class FunctionDeclarationSyntax : SyntaxNode, IFunctionDeclaration
    {
        public string Name { get; private set; }
        public IList<string> Parameters { get; private set; }
        public BlockSyntax Body { get; private set; }
        internal Variable Target { get; set; }

        public FunctionDeclarationSyntax(string name, IEnumerable<string> parameters, BlockSyntax body)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (parameters == null)
                throw new ArgumentNullException("parameters");
            if (body == null)
                throw new ArgumentNullException("body");

            Name = name;
            Parameters = parameters.ToReadOnly();
            Body = body;
        }

        public override SyntaxType Type
        {
            get { return SyntaxType.FunctionDeclaration; }
        }

        [DebuggerStepThrough]
        public override void Accept(ISyntaxVisitor visitor)
        {
            visitor.VisitFunctionDeclaration(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxVisitor<T> visitor)
        {
            return visitor.VisitFunctionDeclaration(this);
        }
    }
}
