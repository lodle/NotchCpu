﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;

namespace NotchCpu.Emulator
{
    public class Emu
    {
        static MainUi _MainUi = new MainUi();
        static IProgram _Program = null;

        const long _TicksPerInstruction = 100 * 1000 / 60;
        

        public static void SetProgram(IProgram program)
        {
            _Program = program;
        }

        public static Form GetMainForm()
        {
            return _MainUi;
        }

        public static void Run(Registers reg)
        {
            new Emu(_Program, reg).RunProgram();
        }

        Registers _Reg;
        ushort [] _Binary;
        Stopwatch _Stopwatch = new Stopwatch();

        Random _Rand = new Random();

        public Emu(IProgram prog, Registers reg)
        {
            _Reg = reg;
            _Binary = _Program.GetDebugBinary();
        }

        public void RunProgram()
        {
            Array.Copy(_Binary, _Reg.Ram, _Binary.Length);

            ushort val;
            while (Step(out val))
            {
            }
        }

        private bool Step(out ushort value, bool ignore = false)
        {
            var r = _Reg.Ram[_Reg.PC];

            ushort o = (ushort)(r & 0xF);
            ushort a = (ushort)((r >> 4) & 0x3F);
            ushort b = (ushort)((r >> 10) & 0x3F);

            value = 0;

            if (!ignore && !_Program.PreStep(_Reg, o, a, b))
                return false;

            RunOpCode(o, a, b, out value, ignore);

            _Reg.PC++;

            return ignore || _Program.PostStep(_Reg, value);
        }

        public void RunOpCode(ushort opCode, ushort a, ushort b, out ushort aOut, bool ignore)
        {
            System.GC.Collect();

            aOut = 0;
            var op = GetOpCode(opCode);
            long cost = GetOpCodeCost(op);

            _Stopwatch.Restart();

            if (op == OpCode.NB_OP)
            {
                var ra = _Reg.Get(b);
                PerformAdvancedOperation(GetOpCode(a), ra);
            }
            else
            {
                bool skip = false;

                var ra = _Reg.Get(a);
                var rb = _Reg.Get(b);

                if (!ignore)
                {
                    var res = PerformOperation(op, ra, rb, out skip);

                    if (skip)
                    {
                        cost++;
                        ushort temp;
                        _Reg.PC++;
                        Step(out temp, true);
                        _Reg.PC--;
                    }
                    else if (res != -1)
                    {
                        ra.Value = (ushort)res;
                        aOut = (ushort)res;
                        _Reg.O = (ushort)(res >> 16);
                    }
                }
            }

            cost *= _TicksPerInstruction;

            if (_Stopwatch.ElapsedTicks > cost)
            {
                _MainUi.Log("Instruction {0} took longer than {1}", op, cost);
            }
            else
            {
                while (_Stopwatch.IsRunning && (_Stopwatch.ElapsedTicks < cost))
                {
                    //chew some cycles
                    double t1 = _Rand.NextDouble() * _Rand.NextDouble();
                    double t2 = t1 % 5;
                    t1 = t1 % t2;
                }
            }

            _Stopwatch.Stop();
        }


        private void PerformAdvancedOperation(OpCode opCode, MemLoc a)
        {
            switch (opCode)
            {
               case OpCode.JSR_OP:

                    ushort res;
                    Step(out res);

                    _Reg.Ram[--_Reg.SP] = _Reg.PC;
                    _Reg.PC = res;
                    break;
            }
        }

        private int PerformOperation(OpCode opCode, MemLoc a, MemLoc b, out bool skip)
        {
            skip = false;

            switch (opCode)
            {
                case OpCode.ADD_OP:
                    return (ushort)(a.Value + b.Value);

                case OpCode.AND_OP:
                    return (ushort)(a.Value & b.Value);

                case OpCode.BOR_OP:
                    return (ushort)(a.Value | b.Value);

                case OpCode.DIV_OP:
                    return (b.Value != 0) ? (ushort)(a.Value / b.Value) : (ushort)0;

                case OpCode.MOD_OP:
                    return (b.Value != 0) ? (ushort)(a.Value % b.Value) : (ushort)0;

                case OpCode.MUL_OP:
                    return (ushort)(a.Value * b.Value);

                case OpCode.SET_OP:
                    return a.Value = b.Value;

                case OpCode.SHL_OP:
                    return (ushort)(a.Value << b.Value);

                case OpCode.SHR_OP:
                    return (ushort)(a.Value >> b.Value);

                case OpCode.SUB_OP:
                    return (ushort)(a.Value - b.Value);

                case OpCode.XOR_OP:
                    return (ushort)(a.Value ^ b.Value);

                case OpCode.IFB_OP:
                    skip = (a.Value & b.Value) == 0;
                    return -1;

                case OpCode.IFE_OP:
                    skip = (a.Value != b.Value);
                    return -1;

                case OpCode.IFG_OP:
                    skip = (a.Value <= b.Value);
                    return -1;

                case OpCode.IFN_OP:
                    skip = (a.Value == b.Value);
                    return -1;

                case OpCode.NB_OP:
                case OpCode.JSR_OP:
                    return -1;
            }

            throw new Exception("Invalid op code for operation");
        }

        private OpCode GetOpCode(ushort opCode)
        {
            return (OpCode)opCode;
        }

        private int GetOpCodeCost(OpCode opCode)
        {
            switch (opCode)
            {
                case OpCode.AND_OP:
                case OpCode.BOR_OP:
                case OpCode.SET_OP:
                case OpCode.XOR_OP:
                    return 1;

                case OpCode.ADD_OP:
                case OpCode.SUB_OP:
                case OpCode.MUL_OP:
                case OpCode.SHL_OP:
                case OpCode.SHR_OP:
                    return 2;

                case OpCode.DIV_OP:
                case OpCode.MOD_OP:
                    return 3;

                case OpCode.IFB_OP:
                case OpCode.IFE_OP:
                case OpCode.IFG_OP:
                case OpCode.IFN_OP:
                    return 2;

                case OpCode.JSR_OP:
                    return 2;                    
            }

            return 0;
        }
    }
}
