using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DCPUC;

namespace NotchCpu.CompilerTasks
{
    class Class
    {
        static Stack<String> _Stack = new Stack<String>();

        public static FunctionDeclarationNodeCSharp MainFunction { get; set; }

        internal static String GetLabel()
        {
            if (_Stack.Count() == 0)
                return "global";

            return _Stack.Peek();
        }

        internal static void Push(string name)
        {
            _Stack.Push(name);
        }

        internal static void Pop()
        {
            _Stack.Pop();
        }

        static List<FunctionDeclarationNodeCSharp> _FunctionList = new List<FunctionDeclarationNodeCSharp>();
        static Dictionary<String, ClassDeclarationNode> _ClassLookup = new Dictionary<string, ClassDeclarationNode>();

        internal static void RegFunction(FunctionDeclarationNodeCSharp functionDeclarationNodeCSharp)
        {
            _FunctionList.Add(functionDeclarationNodeCSharp);
        }

        internal static void RegClass(String name, ClassDeclarationNode c)
        {
            foreach (var funct in _FunctionList)
            {
                c.ChildNodes.Add(funct);
                funct.ClassDecl = c;
            }

            _ClassLookup.Add(name, c);
            _FunctionList.Clear();
        }

        internal static Irony.Interpreter.Ast.AstNode FindClass(string p)
        {
            if (_ClassLookup.ContainsKey(p))
                return _ClassLookup[p];

            return null;
        }

        internal static void SetMain(FunctionDeclarationNodeCSharp mainFunction)
        {
            if (MainFunction != null)
                throw new Exception("Can only have one static function called main");

            MainFunction = mainFunction;
        }

        internal static void Clear()
        {
            MainFunction = null;
            _FunctionList.Clear();
            _ClassLookup.Clear();
            _Stack.Clear();
        }

        internal static void CompileUsedFunctions(Assembly assembly)
        {
            foreach (var c in _ClassLookup)
            {
                Class.Push(c.Key);

                foreach (var f in c.Value.ChildNodes)
                {
                    if (f is FunctionDeclarationNode == false)
                        continue;

                    var func = f as FunctionDeclarationNode;

                    if (func == MainFunction)
                        continue;

                    if (func.references > 0)
                        func.CompileFunction(assembly);
                }

                Class.Pop();
            }
        }
    }
}
