using System;
using System.Collections.Generic;

namespace Jint.Expressions
{
    public interface IGenericExpression
    {
        List<ExpressionSyntax> Generics { get; set; }
    }
}
