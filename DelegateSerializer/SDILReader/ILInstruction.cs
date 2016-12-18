using System;
using System.Reflection;
using System.Reflection.Emit;

namespace DelegateSerializer.SDILReader
{
    public class ILInstruction
    {
        private OpCode code;
        private object operand;
        private int offset;

        public OpCode Code
        {
            get { return code; }
            set { code = value; }
        }

        public object Operand
        {
            get { return operand; }
            set { operand = value; }
        }

        public byte[] OperandData { get; set; }

        public int Offset
        {
            get { return offset; }
            set { offset = value; }
        }

        public string GetCode()
        {
            string str = string.Concat(new object[4]
                                       {
                                           "",
                                           GetExpandedOffset(offset),
                                           " : ",
                                           code
                                       });
            if (operand != null)
            {
                switch (code.OperandType)
                {
                    case OperandType.InlineBrTarget:
                    case OperandType.ShortInlineBrTarget:
                        str = str + " " + GetExpandedOffset((int) operand);
                        break;
                    case OperandType.InlineField:
                        var fieldInfo = (FieldInfo) operand;
                        str = str + " " + Globals.ProcessSpecialTypes(fieldInfo.FieldType.ToString()) + " " +
                              Globals.ProcessSpecialTypes(fieldInfo.ReflectedType.ToString()) + "::" + fieldInfo.Name;
                        break;
                    case OperandType.InlineI:
                    case OperandType.InlineI8:
                    case OperandType.InlineR:
                    case OperandType.ShortInlineI:
                    case OperandType.ShortInlineR:
                        str = str + operand;
                        break;
                    case OperandType.InlineMethod:
                        try
                        {
                            var methodInfo = (MethodInfo) operand;
                            str = str + " ";
                            if (!methodInfo.IsStatic)
                                str = str + "instance ";
                            str = str + Globals.ProcessSpecialTypes(methodInfo.ReturnType.ToString()) + " " +
                                  Globals.ProcessSpecialTypes(methodInfo.ReflectedType.ToString()) + "::" +
                                  methodInfo.Name + "()";
                            break;
                        }
                        catch
                        {
                            try
                            {
                                var constructorInfo = (ConstructorInfo) operand;
                                str = str + " ";
                                if (!constructorInfo.IsStatic)
                                    str = str + "instance ";
                                str = str + "void " +
                                      Globals.ProcessSpecialTypes(constructorInfo.ReflectedType.ToString()) + "::" +
                                      constructorInfo.Name + "()";
                                break;
                            }
                            catch
                            {
                                break;
                            }
                        }
                    case OperandType.InlineString:
                        str = operand.ToString() != "\r\n" ? str + " \"" + operand + "\"" : str + " \"\\r\\n\"";
                        break;
                    case OperandType.InlineTok:
                        str = !(operand is Type) ? str + "not supported" : str + ((Type) operand).FullName;
                        break;
                    case OperandType.InlineType:
                        str = str + " " + Globals.ProcessSpecialTypes(operand.ToString());
                        break;
                    case OperandType.ShortInlineVar:
                        str = str + operand;
                        break;
                    default:
                        str = str + "not supported";
                        break;
                }
            }
            return str;
        }

        private string GetExpandedOffset(long offset)
        {
            string str = offset.ToString();
            int num = 0;
            while (str.Length < 4)
            {
                str = "0" + str;
                ++num;
            }
            return str;
        }
    }
}
