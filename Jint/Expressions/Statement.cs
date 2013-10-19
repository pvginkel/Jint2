using System;
using Jint.Debugger;

namespace Jint.Expressions {
    [Serializable]
    public abstract class Statement {
        public string Label { get; set; }

        public abstract void Accept(IStatementVisitor visitor);

        public SourceCodeDescriptor Source { get; set; }

        protected Statement() {
            Label = String.Empty;
        }
    }
}
