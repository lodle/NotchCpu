using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DCPUC;

namespace NotchCpu.CompilerTasks
{
    public class ClassNode : CompilableNode
    {
        String _ClassName = "";

        public ClassNode()
        {
        }

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            _ClassName = treeNode.ChildNodes[0].ChildNodes[1].FindTokenAndGetText();

            base.Init(context, treeNode);
            foreach (var f in treeNode.ChildNodes)
                AddChild("Class Declaration", f);
        }

        public override void Compile(Assembly assembly, Scope scope, Register target)
        {
            Class.Push(_ClassName);

            foreach (var child in ChildNodes)
            {
                assembly.Barrier();
                (child as CompilableNode).Compile(assembly, scope, Register.DISCARD);
            }

            Class.Pop();
        }

        internal void CompileMain(Assembly assembly, Scope scope, Register register)
        {
            var mainFunction = Class.MainFunction;

            if (mainFunction == null)
                throw new Exception("Failed to find main function");

            {
                var temp = new Assembly();
                Class.Push(mainFunction.ClassDecl.AsString);
                mainFunction.CompileFunction(temp);
                Class.Pop();

                Class.CompileUsedFunctions(temp);
            }

            //Hack hack: Do this twice to avoid missing functions
            {
                Class.Push(mainFunction.ClassDecl.AsString);
                mainFunction.CompileFunction(assembly);
                Class.Pop();

                Class.CompileUsedFunctions(assembly);
            }
        }
    }
}
