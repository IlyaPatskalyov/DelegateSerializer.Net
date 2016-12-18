using System;
using System.Reflection;
using System.Reflection.Emit;

namespace DelegateSerializer.SDILReader
{
    public static class Globals
    {
        public static OpCode[] multiByteOpCodes;
        public static OpCode[] singleByteOpCodes;
        public static Module[] modules;

        static Globals()
        {
            LoadOpCodes();
        }

        public static OpCode GetOpCode(OpCodeValues code)
        {
            var value = (ushort) code;
            if (((uint)code & 0xfe00) == 0xfe00)
            {
                return multiByteOpCodes[value & 0xff];
            }
            return singleByteOpCodes[value];
        }

        private static void LoadOpCodes()
        {
            singleByteOpCodes = new OpCode[256];
            multiByteOpCodes = new OpCode[256];
            foreach (FieldInfo fieldInfo in typeof (OpCodes).GetFields())
            {
                if (fieldInfo.FieldType == typeof (OpCode))
                {
                    var opCode = (OpCode) fieldInfo.GetValue(null);
                    var num = (ushort) opCode.Value;
                    if (num < 256)
                    {
                        singleByteOpCodes[num] = opCode;
                    }
                    else
                    {
                        if ((num & 65280) != 65024)
                            throw new Exception("Invalid OpCode.");
                        multiByteOpCodes[num & byte.MaxValue] = opCode;
                    }
                }
            }
        }

        public static string ProcessSpecialTypes(string typeName)
        {
            string str = typeName;
            switch (typeName)
            {
                case "System.string":
                case "System.String":
                case "String":
                    str = "string";
                    break;
                case "System.Int32":
                case "Int":
                case "Int32":
                    str = "int";
                    break;
            }
            return str;
        }
    }
}
