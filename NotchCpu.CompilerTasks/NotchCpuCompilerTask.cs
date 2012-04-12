using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.IO;
using Irony.Parsing;
using System.Reflection;

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

        public bool ShowSourceInAsm { get; set; }

        public NotchCpuCompilerTask()
        {
            ShowSourceInAsm = true;
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
                DoCompile(root, assembly, scope);
                return true;
            }
            catch (DCPUC.CompileError c)
            {
                LogError(c.Message);
                return false;
            }
        }

        private void DoCompile(ClassNode root, DCPUC.Assembly assembly, DCPUC.Scope scope)
        {
            ParseSource(root, assembly, scope);

            var outAsm = OutputAssembly.Replace(".exe", ".dcpu");
            var outBin = OutputAssembly.Replace(".exe", ".bin");

            Dictionary<ushort, List<DCPUC.Anotation>> ano = new Dictionary<ushort,List<DCPUC.Anotation>>();

            String[] asm = GenerateAsm(assembly, ref ano);
            SaveAsm(asm, outAsm);

            byte[] bin = CDCPU16Assemble.Assemble(asm, ref ano);
            SaveBin(bin, outBin);

            //Swap them for cli as it stores them backwards
            for (int x = 0; x < bin.Length; x+=2)
            {
                byte a = bin[x];
                bin[x] = bin[x + 1];
                bin[x + 1] = a;
            }

            GenerateCli(root, bin, ano);

            LogMessage("Asm file location: " + outAsm);
            LogMessage("Bin file location: " + outBin);
        }

        private void GenerateCli(ClassNode root, byte[] bin, Dictionary<ushort, List<DCPUC.Anotation>> ano)
        {
            root.CompileCli(OutputAssembly, bin, ano);
        }

        private String[] GenerateAsm(DCPUC.Assembly assembly, ref Dictionary<ushort, List<DCPUC.Anotation>> ano)
        {
            List<String> ret = new List<string>();

            foreach (var str in assembly.instructions)
            {
                if (str.Ignore)
                    continue;

                if (ShowSourceInAsm && str.anotation != null)
                {
                    var anno = str.anotation.ToString();

                    if (!String.IsNullOrEmpty(anno))
                    {
                        ret.Add(";");
                        ret.Add(";\t" + SourceFiles[0] + anno);
                        ret.Add(";");
                    }
                }

                ret.Add(str.ToString());
            }

            return ret.ToArray();
        }

        private static void ParseSource(ClassNode root, DCPUC.Assembly assembly, DCPUC.Scope scope)
        {
            root.CompileMain(assembly, scope, DCPUC.Register.DISCARD);
            assembly.Add("SUB", "PC", "1", "Non-standard");

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

        private void SaveAsm(String[] asm, string outAsmFile)
        {
            File.WriteAllLines(outAsmFile, asm);
        }

        public void SaveBin(byte[] bin, string outBinFile)
        {
            MemoryStream outfile = new MemoryStream();

            foreach (byte b in bin)
                outfile.WriteByte(b);

            File.WriteAllBytes(outBinFile, outfile.ToArray());
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
