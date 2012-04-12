﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    public class DataLiteralNode : CompilableNode
    {
        List<ushort> data = new List<ushort>();
        string dataLabel;

        bool compiled;

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            foreach (var child in treeNode.ChildNodes)
            {
                var token = child.FindTokenAndGetText();
                if (token[0] == '\"')
                {
                    foreach (var c in token.Substring(1, token.Length - 2))
                        data.Add((ushort)c);
                    data.Add('\0');
                }
                else if (token.StartsWith("0x"))
                {
                    data.Add(atoh(token.Substring(2)));
                }
                else
                {
                    data.Add(Convert.ToUInt16(token));
                }
            }

            if (data.Count > 1) 
                dataLabel = Scope.GetLabel() + "_DATA";
        }

        public override bool IsConstant()
        {
            return data.Count == 1;
        }

        public override ushort GetConstantValue()
        {
            return data[0];
        }

        public override void Compile(Assembly assembly, Scope scope, Register target) 
        {

            if (data.Count == 1)
            {
                assembly.Add("SET", Scope.GetRegisterLabelFirst((int)target), hex(data[0]));
            }
            else
            {
                assembly.Add("SET", Scope.GetRegisterLabelFirst((int)target), dataLabel);

                //hacky hack
                if (!compiled)
                    Scope.AddData(dataLabel, data);
            }
     

            if (target == Register.STACK) 
                scope.stackDepth += 1;

            compiled = true;
        }
    }

    
}
