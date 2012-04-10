using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NotchCpu.Emulator
{
    public interface IProgram
    {
        ushort[] GetDebugBinary();

        bool PreStep(Registers reg, ushort o, ushort a, ushort b);
        bool PostStep(Registers reg, ushort ret);
    }
}
