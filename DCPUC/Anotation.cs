using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;

namespace DCPUC
{
    public enum AnotationType
    {
        Barrier,
        OpenScope,
        CloseScope,
        FuncParamater,
        Memory,
        Register,
    }

    public class Anotation
    {
        public String name;
        public String sourcetext;
        public SourceSpan span;
        public AnotationType type;
        public ushort location;

        public Anotation()
        {
        }

        public Anotation(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            this.sourcetext = context.CurrentParseTree.SourceText.Substring(treeNode.Span.Location.Position, treeNode.Span.Length);
            this.span = treeNode.Span;
        }

        public Anotation(SourceSpan span, AnotationType type = AnotationType.Barrier, String name = "", ushort location = 0)
        {
            this.span = span;
            this.location = location;
            this.type = type;
            this.name = name;
        }

        public override string ToString()
        {
            if (span.Location.Line == 0)
                return null;

            var ret = String.Format(":{0} {1}-{2}", span.Location.Line+1, span.Location.Column+1, span.Location.Column + span.Length+1);

            if (sourcetext.Length > 0)
            {
                var pos = sourcetext.IndexOf("\n");
                pos--;

                if (pos < 0)
                    pos = sourcetext.Length;

                ret += String.Format(" [{0}]", sourcetext.Substring(0, pos));
            }

            return ret;
        }
    }
}
