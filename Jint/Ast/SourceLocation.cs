using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Ast
{
    internal class SourceLocation
    {
        public static readonly SourceLocation Missing = new SourceLocation(-1, -1, -1, -1, -1, -1, null);

        public int StartOffset { get; private set; }
        public int StartLine { get; private set; }
        public int StartColumn { get; private set; }
        public int EndOffset { get; private set; }
        public int EndLine { get; private set; }
        public int EndColumn { get; private set; }
        public string SourceCode { get; private set; }

        public SourceLocation(int startOffset, int startLine, int startColumn, int endOffset, int endLine, int endColumn, string sourceCode)
        {
            StartOffset = startOffset;
            StartLine = startLine;
            StartColumn = startColumn;
            EndOffset = endOffset;
            EndLine = endLine;
            EndColumn = endColumn;
            SourceCode = sourceCode;
        }

        public string GetSourceCode()
        {
            if (SourceCode == null)
                return null;

            return SourceCode.Substring(StartOffset, EndOffset - StartOffset);
        }

        public override string ToString()
        {
            return String.Format(
                "{0}:{1} ({2}) - {3}:{4} ({5})",
                StartLine,
                StartColumn,
                StartOffset,
                EndLine,
                EndColumn,
                EndOffset
            );
        }
    }
}
