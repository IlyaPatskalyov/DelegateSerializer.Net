using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

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
            var sb = new StringBuilder();
            sb.AppendFormat("{0} : {1}", offset.ToString("x").PadLeft(4, '0'), code);
            if (operand != null)
            {
                switch (code.OperandType)
                {
                    case OperandType.InlineBrTarget:
                    case OperandType.ShortInlineBrTarget:
                        sb.Append(" ");
                        sb.Append(((int) operand).ToString().PadLeft(4, '0'));
                        break;
                    case OperandType.InlineField:
                        var fieldInfo = (FieldInfo) operand;
                        sb.Append(" ");
                        sb.AppendFormat("{0} {1}::{2}", fieldInfo.FieldType, fieldInfo.ReflectedType, fieldInfo.Name);
                        break;
                    case OperandType.InlineType:
                        sb.Append(" ");
                        sb.Append(operand);
                        break;
                    case OperandType.InlineI:
                    case OperandType.InlineI8:
                    case OperandType.InlineR:
                    case OperandType.ShortInlineVar:
                    case OperandType.ShortInlineI:
                    case OperandType.ShortInlineR:
                        sb.Append(operand);
                        break;
                    case OperandType.InlineMethod:
                        try
                        {
                            var methodInfo = (MethodInfo) operand;
                            sb.Append(" ");
                            if (!methodInfo.IsStatic)
                                sb.Append("instance ");
                            sb.AppendFormat("{0} {1}::{2}", methodInfo.ReturnType, methodInfo.ReflectedType, methodInfo.Name);
                            break;
                        }
                        catch
                        {
                            try
                            {
                                var constructorInfo = (ConstructorInfo) operand;
                                sb.Append(" ");
                                if (!constructorInfo.IsStatic)
                                    sb.Append("instance ");
                                sb.AppendFormat("void {0}::{1}", constructorInfo.ReflectedType, constructorInfo.Name);
                                break;
                            }
                            catch
                            {
                                break;
                            }
                        }
                    case OperandType.InlineString:
                        sb.AppendFormat(" \"{0}\"", operand);
                        break;
                    case OperandType.InlineTok:
                        if (!(operand is Type))
                            sb.Append("not supported");
                        else
                            sb.Append(((Type) operand).FullName);
                        break;
                    default:
                        sb.Append("not supported");
                        break;
                }
            }
            return sb.ToString();
        }

    }
}
