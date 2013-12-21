using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jint.Expressions;

namespace Jint.Bound
{
    internal class BoundFunction
    {
        public string Name { get; private set; }
        public ReadOnlyArray<string> Parameters { get; private set; }
        public BoundBody Body { get; private set; }

        public BoundFunction(string name, ReadOnlyArray<string> parameters, BoundBody body)
        {
            if (parameters == null)
                throw new ArgumentNullException("parameters");
            if (body == null)
                throw new ArgumentNullException("body");

            Name = name;
            Parameters = parameters;
            Body = body;
        }

        public BoundFunction Update(string name, ReadOnlyArray<string> parameters, BoundBody body)
        {
            if (
                name == Name &&
                parameters == Parameters &&
                body == Body
            )
                return this;

            return new BoundFunction(name, parameters, body);
        }
    }
}
