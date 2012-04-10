using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NotchCpu.Emulator
{
    public class Register
    {
        Registers _Reg;
        RegisterCodes _Code;
        ushort _Offset;

        internal Register(Registers reg, ushort offset)
        {
            _Reg = reg;
            _Offset = offset;
        }

        internal Register(Registers reg, RegisterCodes code, ushort offset) : this(reg, offset)
        {
            _Code = code;
        }

        internal Register(Registers reg, int code, ushort offset)
            : this(reg, offset)
        {
            _Code = (RegisterCodes)code;
        }

        public ushort Value
        {
            get
            {
                switch (_Code)
                {
                    case RegisterCodes.A:
                    case RegisterCodes.B:
                    case RegisterCodes.C:
                    case RegisterCodes.X:
                    case RegisterCodes.Y:
                    case RegisterCodes.Z:
                    case RegisterCodes.I:
                    case RegisterCodes.J:
                        return _Reg.Reg[_Offset];

                    case RegisterCodes.SP:
                        return _Reg.SP;

                    case RegisterCodes.PC:
                        return _Reg.PC;

                    case RegisterCodes.O:
                        return _Reg.O;

                    case RegisterCodes.Literal:
                        return _Offset;

                    default:
                        return _Reg.RAM[_Offset];
                }
            }
            set
            {
                switch (_Code)
                {
                    case RegisterCodes.A:
                    case RegisterCodes.B:
                    case RegisterCodes.C:
                    case RegisterCodes.X:
                    case RegisterCodes.Y:
                    case RegisterCodes.Z:
                    case RegisterCodes.I:
                    case RegisterCodes.J:
                        _Reg.Reg[_Offset] = value;
                        break;

                    case RegisterCodes.Literal:
                        break;

                    case RegisterCodes.SP:
                        _Reg.SP = value;
                        break;

                    case RegisterCodes.PC:
                        _Reg.PC = (ushort)(value - 1); //take 1 as we increment after the cur instruction
                        break;

                    case RegisterCodes.O:
                        _Reg.O = value;
                        break;

                    default:
                        _Reg.RAM[_Offset] = value;
                        break;
                }
            }
        }
    }

    public class Registers
    {
        public ushort SP;
        public ushort PC;
        public ushort O;

        public ushort[] Reg = new ushort[8];
        public ushort[] RAM = new ushort[65536];

        public Register Get(ushort code)
        {
            int r = (ushort)(((int)code) & 0x7);

            switch ((RegisterCodes)code)
            {
                case RegisterCodes.A:
                case RegisterCodes.B:
                case RegisterCodes.C:
                case RegisterCodes.X:
                case RegisterCodes.Y:
                case RegisterCodes.Z:
                case RegisterCodes.I:
                case RegisterCodes.J:
                    return new Register(this, code, (ushort)code);

                case RegisterCodes.A_Mem:
                case RegisterCodes.B_Mem:
                case RegisterCodes.C_Mem:
                case RegisterCodes.X_Mem:
                case RegisterCodes.Y_Mem:
                case RegisterCodes.Z_Mem:
                case RegisterCodes.I_Mem:
                case RegisterCodes.J_Mem:
                    return new Register(this, code, (ushort)Reg[r]);

                case RegisterCodes.A_NextWord:
                case RegisterCodes.B_NextWord:
                case RegisterCodes.C_NextWord:
                case RegisterCodes.X_NextWord:
                case RegisterCodes.Y_NextWord:
                case RegisterCodes.Z_NextWord:
                case RegisterCodes.I_NextWord:
                case RegisterCodes.J_NextWord:
                    return new Register(this, code, (ushort)(Reg[r] + RAM[++PC]));

                case RegisterCodes.POP:
                    return new Register(this, code, (ushort)SP++);
                case RegisterCodes.PEEK:
                    return new Register(this, code, (ushort)SP);
                case RegisterCodes.PUSH:
                    return new Register(this, code, (ushort)--SP);
                case RegisterCodes.SP:
                    return new Register(this, code, (ushort)SP);
                case RegisterCodes.PC:
                    return new Register(this, code, (ushort)PC);
                case RegisterCodes.O:
                    return new Register(this, code, (ushort)O);
                case RegisterCodes.NextWord_Literal_Mem:
                    return new Register(this, code, (ushort)RAM[++PC]);
                case RegisterCodes.NextWord_Literal_Value:
                    return new Register(this, code, ++PC);

                default:
                    return new Register(this, RegisterCodes.Literal, (ushort)(code - 0x20));
            }

            throw new Exception("Invalid Op Code");
        }
    }

    public enum RegisterCodes : ushort
    {
        // Basic register code, used to read value from register
        // ie SET A, X
        A = 0x00,
        B = 0x01,
        C = 0x02,
        X = 0x03,
        Y = 0x04,
        Z = 0x05,
        I = 0x06,
        J = 0x07,

        // References memory location of register value
        // ie SET A, [X] 
        A_Mem = 0x08,
        B_Mem = 0x09,
        C_Mem = 0x0A,
        X_Mem = 0x0B,
        Y_Mem = 0x0C,
        Z_Mem = 0x0D,
        I_Mem = 0x0E,
        J_Mem = 0x0F,

        // References memory location with modifier
        // ie SET A [X+2]
        A_NextWord = 0x10,
        B_NextWord = 0x11,
        C_NextWord = 0x12,
        X_NextWord = 0x13,
        Y_NextWord = 0x14,
        Z_NextWord = 0x15,
        I_NextWord = 0x16,
        J_NextWord = 0x17,

        POP = 0x18,     // [SP++]
        PEEK = 0x19,    // [SP]
        PUSH = 0x1A,    // [--SP]

        SP = 0x1B,      // Stack pointer
        PC = 0x1C,      // Program Counter
        O = 0x1D,       // Overflow Register

        NextWord_Literal_Mem = 0x1E,    // IE for "SET A, [0x1000]" B register will be 0x1E and we assume the next word (PC++)'s value is to reference a memory location
        NextWord_Literal_Value = 0x1F,   // Same as above but its a literal value, not literal value to a memory location

        Literal = 0x20,

        // if Literal value is < 0x1F we can encode it in 0x20-0x3F and skip the next word requirement. 
        // this is really handy for simple register initialization and incrementation, as we can encode it in as 
        // little as 1 word!
    }
}
