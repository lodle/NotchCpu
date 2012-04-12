using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    public class InlineASMNode : CompilableNode
    {
        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            this.AsString = treeNode.ChildNodes[1].FindTokenAndGetText();

            anotation = new Anotation(context, treeNode);
        }

        public override void Compile(Assembly assembly, Scope scope, Register target)
        {
            var lines = AsString.Split(new String[2]{"\n", "\r"}, StringSplitOptions.RemoveEmptyEntries);
            assembly.Barrier();

            int aid = assembly.PushAnotation(anotation);

            foreach (var str in lines)
            {
                assembly.Add(new Instruction(str + " ;", "", ""));
                anotation = null;
            }

            assembly.PopAnotation(aid);
        }
    }

    
}
