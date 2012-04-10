using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DCPUC;
using System.Diagnostics;

namespace NotchCpu.CompilerTasks
{
    public class ClassDeclarationNode : CompilableNode
    {
        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            AsString = treeNode.ChildNodes[1].FindTokenAndGetText(); 
            Class.RegClass(AsString, this);
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
