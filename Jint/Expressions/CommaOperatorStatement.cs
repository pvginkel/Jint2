using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Expressions {
    [Serializable]
    public class CommaOperatorStatement : Expression {
        class StatementInfo {
            public int Index { get; private set; }
            public Statement Statement { get; private set; }

            public StatementInfo(int i, Statement s) {
                Index = i;
                Statement = s;
            }
        }

        public List<Statement> Statements { get; set; }

        public CommaOperatorStatement() {
            Statements = new List<Statement>();
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override void Accept(IStatementVisitor visitor) {
            visitor.Visit(this);
        }

    }
}
