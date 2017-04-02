using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using DelegateSerializer.Helpers;

namespace DelegateSerializer.ILReader
{
    internal class MethodReader
    {
        public static IEnumerable<ILInstruction> Read(MethodBase m)
        {
            var methodBody = m.GetMethodBody();
            if (methodBody == null)
                yield break;

            var il = methodBody.GetILAsByteArray();
            int position = 0;
            while (position < il.Length)
            {
                var code = il.ReadOpCode(ref position);
                var offset = position - 1;

                var operand = ReadOperand(code, il, ref position);
                var token = operand as MetadataToken;
                if (token != null)
                    operand = token.Resolve(m.Module);

                yield return new ILInstruction(code, operand, offset);
            }
        }

        private static object ReadOperand(OpCode code, byte[] il, ref int position)
        {
            switch (code.OperandType)
            {
                case OperandType.InlineI:
                    return il.ReadInt32(ref position);
                case OperandType.InlineI8:
                    return il.ReadInt64(ref position);
                case OperandType.InlineNone:
                    return null;
                case OperandType.InlineR:
                    return il.ReadDouble(ref position);
                case OperandType.InlineSwitch:
                    var length = il.ReadInt32(ref position);
                    var numArray1 = new int[length];
                    for (int index = 0; index < length; ++index)
                        numArray1[index] = il.ReadInt32(ref position);
                    var numArray2 = new int[length];
                    for (int index = 0; index < length; ++index)
                        numArray2[index] = position + numArray1[index];
                    break;
                case OperandType.InlineString:
                case OperandType.InlineSig:
                case OperandType.InlineField:
                case OperandType.InlineMethod:
                case OperandType.InlineType:
                case OperandType.InlineTok:
                    return new MetadataToken((uint) il.ReadInt32(ref position));
                case OperandType.InlineVar:
                    return il.ReadUInt16(ref position);
                case OperandType.InlineBrTarget:
                    return il.ReadInt32(ref position) + position;
                case OperandType.ShortInlineBrTarget:
                    return il.ReadSByte(ref position) + position;
                case OperandType.ShortInlineI:
                    return il.ReadSByte(ref position);
                case OperandType.ShortInlineR:
                    return il.ReadFloat(ref position);
                case OperandType.ShortInlineVar:
                    return il.ReadByte(ref position);
            }
            throw new NotSupportedException("Unknown operand type.");
        }
    }
}