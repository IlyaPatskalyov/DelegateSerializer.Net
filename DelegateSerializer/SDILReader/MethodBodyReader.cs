using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using DelegateSerializer.Helpers;

namespace DelegateSerializer.SDILReader
{
    public class MethodBodyReader
    {
        public static IEnumerable<ILInstruction> Read(MethodBase m)
        {
            var methodBody = m.GetMethodBody();
            if (methodBody == null)
                yield break;

            var il = methodBody.GetILAsByteArray();

            var genericArguments = m.ReflectedType.IsGenericType ? m.ReflectedType.GetGenericArguments() : null;
            var genericMemberArguments = m.IsGenericMethod ? m.GetGenericArguments() : null;

            int position = 0;
            while (position < il.Length)
            {
                var code = il.ReadOpCode(ref position);
                var instruction = new ILInstruction();
                instruction.Code = code;
                instruction.Offset = position - 1;
                int metadataToken = 0;
                switch (code.OperandType)
                {
                    case OperandType.InlineBrTarget:
                        instruction.Operand = il.ReadInt32(ref position) + position;
                        break;
                    case OperandType.InlineField:
                        metadataToken = il.ReadInt32(ref position);
                        instruction.Operand = m.Module.ResolveField(metadataToken, genericArguments,
                                                                    genericMemberArguments);
                        break;
                    case OperandType.InlineI:
                        instruction.Operand = il.ReadInt32(ref position);
                        break;
                    case OperandType.InlineI8:
                        instruction.Operand = il.ReadInt64(ref position);
                        break;
                    case OperandType.InlineNone:
                        instruction.Operand = null;
                        break;
                    case OperandType.InlineR:
                        instruction.Operand = il.ReadDouble(ref position);
                        break;
                    case OperandType.InlineSig:
                        metadataToken = il.ReadInt32(ref position);
                        instruction.Operand = m.Module.ResolveSignature(metadataToken);
                        break;
                    case OperandType.InlineString:
                        metadataToken = il.ReadInt32(ref position);
                        instruction.Operand = m.Module.ResolveString(metadataToken);
                        break;
                    case OperandType.InlineSwitch:
                        var length = il.ReadInt32(ref position);
                        var numArray1 = new int[length];
                        for (int index = 0; index < length; ++index)
                            numArray1[index] = il.ReadInt32(ref position);
                        var numArray2 = new int[length];
                        for (int index = 0; index < length; ++index)
                            numArray2[index] = position + numArray1[index];
                        break;
                    case OperandType.InlineMethod:
                        metadataToken = il.ReadInt32(ref position);
                        try
                        {
                            instruction.Operand = m.Module.ResolveMethod(metadataToken, genericArguments,
                                                                         genericMemberArguments);
                        }
                        catch
                        {
                            instruction.Operand = m.Module.ResolveMember(metadataToken, genericArguments,
                                                                         genericMemberArguments);
                        }
                        break;
                    case OperandType.InlineTok:
                        metadataToken = il.ReadInt32(ref position);
                        try
                        {
                            instruction.Operand = m.Module.ResolveType(metadataToken, genericArguments,
                                                                       genericMemberArguments);
                        }
                        catch
                        {
                        }
                        break;
                    case OperandType.InlineType:
                        metadataToken = il.ReadInt32(ref position);
                        instruction.Operand = m.Module.ResolveType(metadataToken,
                                                                   m.DeclaringType.GetGenericArguments(),
                                                                   genericMemberArguments);
                        break;
                    case OperandType.InlineVar:
                        instruction.Operand = il.ReadUInt16(ref position);
                        break;
                    case OperandType.ShortInlineBrTarget:
                        instruction.Operand = il.ReadSByte(ref position) + position;
                        break;
                    case OperandType.ShortInlineI:
                        instruction.Operand = il.ReadSByte(ref position);
                        break;
                    case OperandType.ShortInlineR:
                        instruction.Operand = il.ReadFloat(ref position);
                        break;
                    case OperandType.ShortInlineVar:
                        instruction.Operand = il.ReadByte(ref position);
                        break;
                    default:
                        throw new Exception("Unknown operand type.");
                }
                yield return instruction;
            }
        }
    }
}