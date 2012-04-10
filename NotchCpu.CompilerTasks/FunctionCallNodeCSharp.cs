using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DCPUC;
using Irony.Interpreter.Ast;
using Irony.Parsing;

namespace NotchCpu.CompilerTasks
{
    class FunctionCallNodeCSharp : DCPUC.FunctionCallNode
    {
        FunctionCallNodeCSharp _RealFunctionCall;

        protected override FunctionDeclarationNode findFunction(AstNode node, string name)
        {
            String [] parts = name.Split('.');

            FunctionDeclarationNode ret = null;

            if (parts.Count() == 1)
                ret = base.findFunction(node, name);
            else
                ret = base.findFunction(Class.FindClass(parts[0]), parts[1]);

            if (ret != null)
                ret.label = String.Format(ret.label, (ret as FunctionDeclarationNodeCSharp).ClassDecl.AsString).ToUpper();

            return ret;
        }


        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);

            if (treeNode.ChildNodes[1].AstNode is FunctionCallNodeCSharp)
            {
                _RealFunctionCall = treeNode.ChildNodes[1].AstNode as FunctionCallNodeCSharp;
                _RealFunctionCall.AsString = treeNode.ChildNodes[0].FindTokenAndGetText() + "." + _RealFunctionCall.AsString;
            }
        }

        public override void Compile(Assembly assembly, Scope scope, Register target)
        {
            if (_RealFunctionCall != null)
                _RealFunctionCall.Compile(assembly, scope, target);
            else
                base.Compile(assembly, scope, target);
        }
    }
}
