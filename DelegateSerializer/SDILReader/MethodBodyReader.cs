using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace DelegateSerializer.SDILReader
{
    public class MethodBodyReader
    {
        private readonly byte[] il;
        private readonly MethodBase mi;
        private List<ILInstruction> instructions;

        public MethodBodyReader(MethodBase mi)
        {
            this.mi = mi;
            if (mi.GetMethodBody() == null)
                return;
            il = mi.GetMethodBody().GetILAsByteArray();
            Type[] genericArguments = null;
            if (mi.ReflectedType.IsGenericType)
                genericArguments = mi.ReflectedType.GetGenericArguments();
            Type[] genericMemberArguments = null;
            if (mi.IsGenericMethod)
                genericMemberArguments = mi.GetGenericArguments();
            ConstructInstructions(mi.Module, genericArguments, genericMemberArguments);
        }

        public IEnumerable<ILInstruction> GetInstructions()
        {
            return instructions;
        }

        private void ConstructInstructions(Module module, Type[] genericArguments, Type[] genericMemberArguments)
        {
            int position = 0;
            instructions = new List<ILInstruction>();
            while (position < il.Length)
            {
                var instruction = new ILInstruction();
                OpCode code = OpCodes.Nop;
                ushort value = il[position++];
                if (value != 0xfe)
                {
                    code = Globals.singleByteOpCodes[value];
                }
                else
                {
                    value = il[position++];
                    code = Globals.multiByteOpCodes[value];
                    value = (ushort) (value | 0xfe00);
                }
                instruction.Code = code;
                instruction.Offset = position - 1;
                int metadataToken = 0;
                switch (code.OperandType)
                {
                    case OperandType.InlineBrTarget:
                        metadataToken = ReadInt32(il, ref position) + position;
                        instruction.Operand = metadataToken;
                        break;
                    case OperandType.InlineField:
                        metadataToken = ReadInt32(il, ref position);
                        instruction.Operand = module.ResolveField(metadataToken, genericArguments,
                                                                  genericMemberArguments);
                        break;
                    case OperandType.InlineI:
                        instruction.Operand = ReadInt32(il, ref position);
                        break;
                    case OperandType.InlineI8:
                        instruction.Operand = ReadInt64(il, ref position);
                        break;
                    case OperandType.InlineMethod:
                        metadataToken = ReadInt32(il, ref position);
                        try
                        {
                            instruction.Operand = module.ResolveMethod(metadataToken, genericArguments,
                                                                       genericMemberArguments);
                            break;
                        }
                        catch
                        {
                            instruction.Operand = module.ResolveMember(metadataToken, genericArguments,
                                                                       genericMemberArguments);
                            break;
                        }
                    case OperandType.InlineNone:
                        instruction.Operand = null;
                        break;
                    case OperandType.InlineR:
                        instruction.Operand = ReadDouble(il, ref position);
                        break;
                    case OperandType.InlineSig:
                        metadataToken = ReadInt32(il, ref position);
                        instruction.Operand = module.ResolveSignature(metadataToken);
                        break;
                    case OperandType.InlineString:
                        metadataToken = ReadInt32(il, ref position);
                        instruction.Operand = module.ResolveString(metadataToken);
                        break;
                    case OperandType.InlineSwitch:
                        int length = ReadInt32(il, ref position);
                        var numArray1 = new int[length];
                        for (int index = 0; index < length; ++index)
                            numArray1[index] = ReadInt32(il, ref position);
                        var numArray2 = new int[length];
                        for (int index = 0; index < length; ++index)
                            numArray2[index] = position + numArray1[index];
                        break;
                    case OperandType.InlineTok:
                        metadataToken = ReadInt32(il, ref position);
                        try
                        {
                            instruction.Operand = module.ResolveType(metadataToken, genericArguments,
                                                                     genericMemberArguments);
                            break;
                        }
                        catch
                        {
                            break;
                        }
                    case OperandType.InlineType:
                        metadataToken = ReadInt32(il, ref position);
                        instruction.Operand = module.ResolveType(metadataToken,
                                                                 mi.DeclaringType.GetGenericArguments(),
                                                                 mi.GetGenericArguments());
                        break;
                    case OperandType.InlineVar:
                        instruction.Operand = ReadUInt16(il, ref position);
                        break;
                    case OperandType.ShortInlineBrTarget:
                        instruction.Operand = ReadSByte(il, ref position) + position;
                        break;
                    case OperandType.ShortInlineI:
                        instruction.Operand = ReadSByte(il, ref position);
                        break;
                    case OperandType.ShortInlineR:
                        instruction.Operand = ReadSingle(il, ref position);
                        break;
                    case OperandType.ShortInlineVar:
                        instruction.Operand = ReadByte(il, ref position);
                        break;
                    default:
                        throw new Exception("Unknown operand type.");
                }
                instructions.Add(instruction);
            }
        }

        public object GetRefferencedOperand(Module module, int metadataToken)
        {
            foreach (AssemblyName assemblyRef in module.Assembly.GetReferencedAssemblies())
            {
                foreach (Module module1 in Assembly.Load(assemblyRef).GetModules())
                {
                    try
                    {
                        return module1.ResolveType(metadataToken);
                    }
                    catch
                    {
                    }
                }
            }
            return null;
        }

        public string GetBodyCode()
        {
            string str = "";
            if (instructions != null)
            {
                for (int index = 0; index < instructions.Count; ++index)
                    str = str + instructions[index].GetCode() + "\n";
            }
            return str;
        }

        private int ReadInt16(byte[] il, ref int position)
        {
            return il[position++] | il[position++] << 8;
        }

        private ushort ReadUInt16(byte[] il, ref int position)
        {
            return (ushort) (il[position++] | il[position++] << 8);
        }

        private int ReadInt32(byte[] il, ref int position)
        {
            return il[position++] | il[position++] << 8 | il[position++] << 16 | il[position++] << 24;
        }

        private ulong ReadInt64(byte[] il, ref int position)
        {
            return
                (ulong)
                (il[position++] | il[position++] << 8 | il[position++] << 16 | il[position++] << 24 | il[position++] |
                 il[position++] << 8 | il[position++] << 16 | il[position++] << 24);
        }

        private double ReadDouble(byte[] il, ref int position)
        {
            return il[position++] | il[position++] << 8 | il[position++] << 16 | il[position++] << 24 | il[position++] |
                   il[position++] << 8 | il[position++] << 16 | il[position++] << 24;
        }

        private sbyte ReadSByte(byte[] il, ref int position)
        {
            return (sbyte) il[position++];
        }

        private byte ReadByte(byte[] il, ref int position)
        {
            return il[position++];
        }

        private float ReadSingle(byte[] il, ref int position)
        {
            return il[position++] | il[position++] << 8 | il[position++] << 16 | il[position++] << 24;
        }
    }
}