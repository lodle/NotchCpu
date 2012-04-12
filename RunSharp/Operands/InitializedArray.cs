/*
 * Copyright (c) 2009, Stefan Simek
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
 * OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Reflection;

namespace TriAxis.RunSharp.Operands
{
	class InitializedArray : Operand
	{
		Type type;
		Operand[] elements;

		public InitializedArray(Type type, Operand[] elements)
		{
			this.type = type;
			this.elements = elements;
		}

		internal override void EmitGet(CodeGen g)
		{
			g.EmitI4Helper(elements.Length);
			g.IL.Emit(OpCodes.Newarr, type);

			for (int i = 0; i < elements.Length; i++)
			{
				g.IL.Emit(OpCodes.Dup);
				g.EmitI4Helper(i);
				g.EmitStelemHelper(type, elements[i], false);
			}
		}
		
		public override Type Type
		{
			get
			{
				return type.MakeArrayType();
			}
		}
	}

    class StaticInitializedArray : Operand
    {
        Type type;
        int length;
        FieldBuilder fieldBuilder;

        public StaticInitializedArray(Type type, int length, StaticFieldGen staticField)
        {
            this.type = type;
            this.length = length;
            this.fieldBuilder = staticField.FieldBuilder;
        }

        internal override void EmitGet(CodeGen g)
        {
            //g.IL.Emit(OpCodes.Ldarg_0);
            g.IL.Emit(OpCodes.Ldc_I4, length);
            g.IL.Emit(OpCodes.Newarr, typeof(ushort));
            g.IL.Emit(OpCodes.Dup);
            g.IL.Emit(OpCodes.Ldtoken, fieldBuilder);
            g.IL.Emit(OpCodes.Call, typeof(RuntimeHelpers).GetMethod("InitializeArray"));
        }

        public override Type Type
        {
            get
            {
                return type.MakeArrayType();
            }
        }
    }
}
