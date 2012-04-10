using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DCPUC;

namespace NotchCpu.CompilerTasks
{
    public class FunctionNode : CompilableNode
    {
        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            foreach (var f in treeNode.ChildNodes)
                AddChild("Function Declaration", f);
        }

        public override void Compile(Assembly assembly, Scope scope, Register target)
        {
            foreach (var child in ChildNodes)
            {
                assembly.Barrier();
                (child as CompilableNode).Compile(assembly, scope, Register.DISCARD);
            }


        }

    }
}
