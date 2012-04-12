using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUC
{
    public class Instruction
    {
        public string ins;
        public string a;
        public string b;
        public string comment;

        public Anotation anotation = new Anotation();
 
        public Instruction()
        {
        }

        public Instruction(string ins, string a, string b, string comment = null)
        {
            this.ins = ins;
            this.a = a;
            this.b = b;
            this.comment = comment;
        }

        public override string ToString()
        {
            if (String.IsNullOrEmpty(a)) return ins;
            else if (String.IsNullOrEmpty(b)) return ins + " " + a;
            else return ins + " " + a + (a != "DAT" ? ", " : " ") + b;
            //if (ins[0] == ':' || ins == "BRK") return ins;// + (String.IsNullOrEmpty(comment) ? "" : (" ;" + comment));
            //else if (ins == "JSR") return ins + " " + a;// + (String.IsNullOrEmpty(comment) ? "" : (" ;" + comment));
            //else return ins + " " + a + ", " + b;// +(String.IsNullOrEmpty(comment) ? "" : (" ;" + comment));
        }

        public bool Ignore
        {
            get
            {
                return ins == null;
            }
        }
    }

    public class Assembly
    {
        public List<Instruction> instructions = new List<Instruction>();
        public int _barrier = 0;

        Stack<Tuple<int, Anotation>> anotations = new Stack<Tuple<int,Anotation>>();
        int lastAnotateId = 0;

        public int PushAnotation(Anotation anotiation)
        {
            int id = lastAnotateId++;
            anotations.Push(new Tuple<int, Anotation>(id, anotiation));
            return id;
        }

        public void PopAnotation(int id)
        {
            while (anotations.Count() > 0 && anotations.Peek().Item1 > id)
                anotations.Pop();
        }

        public void Add(string ins, string a, string b, string comment = null)
        {
            Add(new Instruction(ins, a, b, comment));
        }

        public void Add(Instruction instruction)
        {
            if (anotations.Count() > 0)
            {
                var a = anotations.Pop();
                instruction.anotation = a.Item2;
            }

            instructions.Add(instruction);
        }

        public void Optimise()
        {
            if (instructions.Count() == 0)
                return;

            var last = instructions.Count() - 1;

            List<List<Instruction>> parts = new List<List<Instruction>>();

            parts.Add(new List<Instruction>());

            foreach (var i in instructions)
            {
                if (i.anotation.type == AnotationType.Barrier)
                {
                    parts.Add(new List<Instruction>());
                }
                else
                {
                    i.anotation = new Anotation();
                    parts.Last().Add(i);
                }
            }

            for (int x=0; x<parts.Count(); ++x)
            {
                var list = parts[x];
                Optimise(ref list);
                parts[x] = list;
            }

            instructions.Clear();

            foreach (var list in parts)
                instructions.AddRange(list);
        }

        public void Barrier() 
        {
            instructions.Add(new Instruction());
        }

        private static void Optimise(ref List<Instruction> instructions)
        {
            var pos = 0;

            while (pos+1 < instructions.Count())
            {
                var insA = instructions[pos];
                var insB = instructions[pos+1];

                //SET A, POP
                //SET PUSH, A
                if (insA.ins == "SET" && insB.ins == "SET" && insA.b == "POP" && insB.a == "PUSH" && insB.b == insA.a)
                {
                    //remove both
                    instructions.Remove(insA);
                    instructions.Remove(insB);
                }
                //SET A, !POP
                //SET !PUSH, A
                else if (insA.ins == "SET" && insB.ins == "SET" && insA.a == insB.b && insA.b != "POP" && insB.a != "PUSH")
                {
                    insA.a = insB.a;
                    instructions.Remove(insB);
                }
                //SET PUSH, A
                //SET A, POP
                else if (insA.ins == "SET" && insB.ins == "SET" && insA.b == insB.a && insA.a == "PUSH" && insB.b == "pop")
                {
                    instructions.Remove(insA);
                    instructions.Remove(insB);
                }
                //SET PUSH, A
                //SET A, PEEK
                else if (insA.ins == "SET" && insB.ins == "SET" && insA.b == insB.a && insA.a == "PUSH" && insB.b == "PEEK")
                {
                    instructions.Remove(insB);
                }
                //SET A, ?             -> IFN|IFE|IFG ?, A
                //IFN|IFE|IFG ?, A
                else if (insA.ins == "SET" && (insB.ins == "IFN" || insB.ins == "IFE" || insB.ins == "IFG")
                    && insA.a == insB.b)
                {
                    insA.ins = insB.ins;
                    insA.a = insB.a;
                    instructions.Remove(insB);
                }
                else
                {
                    pos++;
                }
            }
        }
    }
}
