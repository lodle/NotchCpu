using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.IO;
using Irony.Parsing;

namespace NotchCpu.CompilerTasks
{
    public class NotchCpuCompilerTask : Task
    {
        /// <summary>
        /// List of C# source files that should be compiled into the assembly
        /// </summary>
        [Required()]
        public string[] SourceFiles { get; set; }

        /// <summary>
        /// Output Assembly (including extension)
        /// </summary>
        [Required()]
        public string OutputAssembly { get; set; }

        /// <summary>
        /// This should be set to $(MSBuildProjectDirectory)
        /// </summary>
        public string ProjectPath { get; set; }

        public bool WriteToConsole { get; set; }

        public NotchCpuCompilerTask()
        {
        }

        public override bool Execute()
        {
            var parser = new Irony.Parsing.Parser(new CSharpGrammar());
            ParseTree tree = parser.Parse(File.ReadAllText(SourceFiles[0]), SourceFiles[0]);

            if (tree.HasErrors())
            {
                foreach (var msg in tree.ParserMessages)
                    LogError(msg.Level.ToString(), "", "", SourceFiles[0], msg.Location.Line, msg.Location.Column, msg.Location.Line, msg.Location.Column + 10, msg.Message);

                return false;
            }

            var root = tree.Root.AstNode as ClassNode;
            var assembly = new DCPUC.Assembly();
            var scope = new DCPUC.Scope();

            try
            {
                root.CompileMain(assembly, scope, DCPUC.Register.DISCARD);
                assembly.Add("BRK", "", "", "Non-standard");

                //foreach (var pendingFunction in scope.pendingFunctions)
                //    pendingFunction.CompileFunction(assembly);

                foreach (var dataItem in DCPUC.Scope.dataElements)
                {
                    assembly.Add(":" + dataItem.Item1, "", "");
                    var datString = "";

                    foreach (var item in dataItem.Item2)
                    {
                        datString += DCPUC.CompilableNode.hex(item);
                        datString += ", ";
                    }

                    assembly.Add("DAT", datString.Substring(0, datString.Length - 2), "");
                }
            }
            catch (DCPUC.CompileError c)
            {
                LogError(c.Message);
                return false;
            }

            var outFile = OutputAssembly.Replace(".exe", ".dcpu");

            using (var fh = File.CreateText(outFile))
            {
                foreach (var str in assembly.instructions)
                {
                    if (str.Ignore)
                        continue;

                    var anno = str.anotation.ToString();

                    if (!String.IsNullOrEmpty(anno))
                    {
                        fh.WriteLine(";");
                        fh.WriteLine(";\t" + SourceFiles[0] + anno);
                        fh.WriteLine(";");
                    }

                    fh.WriteLine(str.ToString());
                }
            }

            LogMessage("Out file location: " + outFile);
            return true;
        }

        void LogMessage(string message)
        {
            if (WriteToConsole)
                Console.WriteLine(message);
            else
                Log.LogMessage(message);
        }

        void LogError(string message)
        {
            if (WriteToConsole)
                Console.WriteLine(message);
            else
                Log.LogError(message);
        }

        void LogError(string subcategory, string errorCode, string helpKeyword, string file, int lineNumber, int columnNumber, int endLineNumber, int endColumnNumber, string message, params object[] messageArgs)
        {
            if (WriteToConsole)
                Console.WriteLine("ERR: " + message);
            else
                LogError(subcategory, errorCode, helpKeyword, file, lineNumber, columnNumber, endLineNumber, endColumnNumber, message, messageArgs);
        }
      
    }
}
