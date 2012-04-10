using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DCPUC;
using System.Diagnostics;

namespace NotchCpu.CompilerTasks
{
    public class FunctionDeclarationNodeCSharp : FunctionDeclarationNode
    {
        public String Protection { get; protected set; }
        public String RetType { get; protected set; }
        public bool IsStatic { get; protected set; }
        public bool IsVoid { get { return RetType == "void"; } }
        public bool IsMain { get; protected set; }

        public ClassDeclarationNode ClassDecl { get; set; }

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            Class.RegFunction(this);

            InitCompilableNode(context, treeNode);
            AddChild("Block", treeNode.ChildNodes[5]);

            var parameters = treeNode.ChildNodes[4].ChildNodes;

            for (int i = 0; i < parameters.Count; ++i)
            {
                var variable = new VariableEx();

                variable.scope = localScope;
                variable.name = parameters[i].ChildNodes[1].FindTokenAndGetText();
                variable.Type = parameters[i].ChildNodes[0].FindTokenAndGetText();

                localScope.variables.Add(variable);

                if (i < 3)
                {
                    variable.location = (Register)i;
                    localScope.UseRegister(i);
                }
                else
                {
                    variable.location = Register.STACK;
                    variable.stackOffset = localScope.stackDepth;
                    localScope.stackDepth += 1;
                }

                parameterCount += 1;
            }

            Protection = treeNode.ChildNodes[0].FindTokenAndGetText();
            IsStatic = treeNode.ChildNodes[1].FindTokenAndGetText() == "static";
            RetType = treeNode.ChildNodes[2].FindTokenAndGetText();

            this.AsString = treeNode.ChildNodes[3].FindTokenAndGetText();
            label = Scope.GetLabel() + "_{0}_" + AsString;
            localScope.activeFunction = this;

            if (AsString == "Main" && IsStatic)
            {
                Class.SetMain(this);
                IsMain = true;
                label = label.Split('_').First() + "_MAIN";
            }
        }

        public override void CompileFunction(Assembly assembly)
        {
            label = String.Format(label, Class.GetLabel()).ToUpper();
            base.CompileFunction(assembly);
        }
    }
}
