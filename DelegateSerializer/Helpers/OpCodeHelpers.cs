using System;
using System.Reflection.Emit;
using DelegateSerializer.SDILReader;

namespace DelegateSerializer.Helpers
{
    public static class OpCodeHelpers
    {
        private const int MultiBytePrefix = 0xfe00;
        private static readonly OpCode[] multiByteOpCodes;
        private static readonly OpCode[] singleByteOpCodes;

        static OpCodeHelpers()
        {
            singleByteOpCodes = new OpCode[256];
            multiByteOpCodes = new OpCode[256];
            foreach (var fieldInfo in typeof(OpCodes).GetFields())
                if (fieldInfo.FieldType == typeof(OpCode))
                {
                    var opCode = (OpCode) fieldInfo.GetValue(null);
                    var num = (ushort) opCode.Value;
                    if (num < 0x100)
                        singleByteOpCodes[num] = opCode;
                    else
                    {
                        if ((num & 0xff00) != MultiBytePrefix)
                            throw new Exception("Invalid OpCode.");
                        multiByteOpCodes[num & byte.MaxValue] = opCode;
                    }
                }
        }

        public static OpCode GetOpCode(this OpCodeValues code)
        {
            var value = (ushort) code;
            if (((uint) code & MultiBytePrefix) == MultiBytePrefix)
                return multiByteOpCodes[value & 0xff];
            return singleByteOpCodes[value];
        }

        public static OpCode ReadOpCode(this byte[] il, ref int position)
        {
            var code = OpCodes.Nop;
            ushort value = il[position++];
            if (value != 0xfe)
                code = singleByteOpCodes[value];
            else
            {
                value = il[position++];
                code = multiByteOpCodes[value];
            }
            return code;
        }

        public static bool IsLabel(this OpCodeValues opCodeValue)
        {
            return opCodeValue == OpCodeValues.Br_S ||
                   opCodeValue == OpCodeValues.Br ||
                   opCodeValue == OpCodeValues.Brfalse ||
                   opCodeValue == OpCodeValues.Brfalse_S ||
                   opCodeValue == OpCodeValues.Brtrue ||
                   opCodeValue == OpCodeValues.Brtrue_S ||
                   opCodeValue == OpCodeValues.Leave ||
                   opCodeValue == OpCodeValues.Leave_S;
        }
    }
}