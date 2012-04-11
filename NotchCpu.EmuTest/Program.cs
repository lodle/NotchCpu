using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NotchCpu.Emulator;
using System.IO;
using System.Diagnostics;

namespace NotchCpu.EmuTest
{
    class Program : IProgram
    {
        static String _File;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(String [] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

#if !DEBUG
            var browser = new OpenFileDialog();
            browser.Filter = "bin files (*.bin, *.obj)|*.txt;*.obj|All files (*.*)|*.*" ;
            browser.InitialDirectory = Directory.GetCurrentDirectory();

            if (browser.ShowDialog() != DialogResult.OK)
                return;

            _File = browser.FileName;
#else
            _File = "helloworld.obj";
#endif

            Emu.SetProgram(new Program());
            Application.Run(Emu.GetMainForm());
        }

        ushort[] _Binary;

        public Program()
        {
            var bytes = File.ReadAllBytes(_File);

            _Binary = new ushort[bytes.Length / 2];

            for (int x = 0; x < _Binary.Length; x++)
                _Binary[x] = (ushort)((bytes[x * 2] << 8) + bytes[x * 2 + 1]);
        }

        public ushort[] GetDebugBinary()
        {
            return _Binary;
        }

        public bool PreStep(Registers reg, ushort o, ushort a, ushort b)
        {
            return true;
        }

        public bool PostStep(Registers reg, ushort ret)
        {

            return true;
        }
    }
}
