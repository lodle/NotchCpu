using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;
using Irony.Parsing;

namespace DCPUC
{
    public class CompilableNode : AstNode
    {
        protected Anotation anotation;

        public virtual void Compile(Assembly assembly, Scope scope, Register target) { throw new NotImplementedException(); }
        public virtual bool IsConstant() { return false; }
        public virtual ushort GetConstantValue() { return 0; }

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            anotation = new Anotation(context, treeNode);
        }

        private static string hexDigits = "0123456789ABCDEF";
        public static String htoa(int x)
        {
            var s = "";
            while (x > 0)
            {
                s = hexDigits[x % 16] + s;
                x /= 16;
            }
            while (s.Length < 4) s = '0' + s;
            return s;
        }
        public static ushort atoh(string s)
        {
            ushort h = 0;
            s = s.ToUpper();
            for (int i = 0; i < s.Length; ++i)
            {
                h <<= 4;
                ushort d = (ushort)hexDigits.IndexOf(s[i]);
                h += d;
            }
            return h;
        }

        public static String hex(int x) { return "0x" + htoa(x); }
        public static String hex(string x) { return "0x" + htoa(Convert.ToInt16(x)); }

        public static Scope BeginBlock(Assembly assembly, Scope scope, SourceSpan span)
        {
            var aid = assembly.PushAnotation(new Anotation(span));
            assembly.Add(new Instruction());
            assembly.PopAnotation(aid);

            return scope.Push(new Scope());
        }

        public static void EndBlock(Assembly assembly, Scope scope, SourceSpan span)
        {
            var aid = assembly.PushAnotation(new Anotation(span));

            if (scope.stackDepth - scope.parentDepth > 0)
                assembly.Add(new Instruction("ADD", "SP", hex(scope.stackDepth - scope.parentDepth), "End block"));
            else
                assembly.Add(new Instruction());

            assembly.PopAnotation(aid);
        }

        public void InsertLibrary(List<string> library)
        {
            for (int i = 0; i < library.Count; ++i)
            {
                if (library[i].StartsWith(";DCPUC FUNCTION"))
                {
                    //Parse function..
                    var funcHeader = library[i].Split(' ');
                    List<String> funcCode = new List<string>();
                    while (i < library.Count && !library[i].StartsWith(";DCPUC ENDFUNCTION"))
                    {
                        funcCode.Add(library[i]);
                        ++i;
                    }
                    if (i < library.Count)
                    {
                        funcCode.Add(library[i]);
                        ++i;
                    }
                    var funcNode = new LibraryFunctionNode();
                    funcNode.AsString = funcHeader[2];
                    funcNode.label = funcHeader[3];
                    funcNode.parameterCount = Convert.ToInt32(funcHeader[4]);
                    funcNode.code = funcCode;
                    ChildNodes.Add(funcNode);
                }
            }
        }
    }
}
