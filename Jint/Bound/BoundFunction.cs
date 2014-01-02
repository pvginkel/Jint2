using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jint.Ast;

namespace Jint.Bound
{
    internal class BoundFunction
    {
        public string Name { get; private set; }
        public ReadOnlyArray<string> Parameters { get; private set; }
        public BoundBody Body { get; private set; }
        public SourceLocation Location { get; private set; }

        public BoundFunction(string name, ReadOnlyArray<string> parameters, BoundBody body, SourceLocation location)
        {
            if (parameters == null)
                throw new ArgumentNullException("parameters");
            if (body == null)
                throw new ArgumentNullException("body");
            if (location == null)
                throw new ArgumentNullException("location");

            Name = name;
            Parameters = parameters;
            Body = body;
            Location = location;
        }

        public BoundFunction Update(string name, ReadOnlyArray<string> parameters, BoundBody body, SourceLocation location)
        {
            if (
                name == Name &&
                parameters == Parameters &&
                body == Body &&
                location == Location
            )
                return this;

            return new BoundFunction(name, parameters, body, location);
        }
    }
}
