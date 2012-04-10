using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NotchCpu.CompilerTasks;

namespace Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            NotchCpuCompilerTask compiler = new NotchCpuCompilerTask();

            compiler.SourceFiles = new String[] { "NotchTest.cs" };
            compiler.OutputAssembly = "Out.exe";
            compiler.ProjectPath = Directory.GetCurrentDirectory();

            compiler.WriteToConsole = true;

            compiler.Execute();

        }
    }
}
