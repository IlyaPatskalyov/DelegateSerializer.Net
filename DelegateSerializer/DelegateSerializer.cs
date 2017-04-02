using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DelegateSerializer.Data;
using DelegateSerializer.DataConverters;
using DelegateSerializer.Exceptions;
using DelegateSerializer.ILReader;

namespace DelegateSerializer
{
    public sealed partial class DelegateSerializer : IDelegateSerializer
    {
        private readonly TypeResolver typeResolver;
        private readonly TypeInfoDataConverter typeInfoDataConverter;
        private readonly MethodInfoDataConverter methodInfoDataConverter;
        private readonly ExceptionHandlingClauseDataConverter exceptionHandlingClauseDataConverter;
        private readonly ConstructorInfoDataConverter constructorInfoDataConverter;
        private readonly LocalVariableInfoDataConverter localVariableInfoDataConverter;

        public static DelegateSerializer Create()
        {
            var typeResolver = new TypeResolver();
            var typeInfoDataBuilder = new TypeInfoDataConverter();
            return new DelegateSerializer(typeResolver,
                                          typeInfoDataBuilder,
                                          new MethodInfoDataConverter(typeInfoDataBuilder),
                                          new ExceptionHandlingClauseDataConverter(typeInfoDataBuilder, typeResolver),
                                          new ConstructorInfoDataConverter(typeInfoDataBuilder),
                                          new LocalVariableInfoDataConverter(typeInfoDataBuilder, typeResolver));
        }

        internal DelegateSerializer(TypeResolver typeResolver,
                                    TypeInfoDataConverter typeInfoDataConverter,
                                    MethodInfoDataConverter methodInfoDataConverter,
                                    ExceptionHandlingClauseDataConverter exceptionHandlingClauseDataConverter,
                                    ConstructorInfoDataConverter constructorInfoDataConverter,
                                    LocalVariableInfoDataConverter localVariableInfoDataConverter)
        {
            this.typeResolver = typeResolver;
            this.typeInfoDataConverter = typeInfoDataConverter;
            this.methodInfoDataConverter = methodInfoDataConverter;
            this.exceptionHandlingClauseDataConverter = exceptionHandlingClauseDataConverter;
            this.constructorInfoDataConverter = constructorInfoDataConverter;
            this.localVariableInfoDataConverter = localVariableInfoDataConverter;
        }

        public DelegateData Serialize(MethodInfo methodInfo)
        {
            var methodBody = methodInfo.GetMethodBody();
            if (methodBody == null)
                return null;

            var fieldsToLocals = new Dictionary<string, int>();
            var result = new DelegateData();
            result.ReturnType = typeInfoDataConverter.Build(methodInfo.ReturnType);
            result.ParametersType = methodInfo.GetParameters()
                                              .Select(info => typeInfoDataConverter.Build(info.ParameterType))
                                              .ToArray();
            result.ExceptionHandlingClauses = methodBody.ExceptionHandlingClauses
                                                        .Select(c => exceptionHandlingClauseDataConverter.Build(c))
                                                        .ToArray();
            result.LocalVariables = new List<LocalVariableInfoData>();
            foreach (var localVariable in methodBody.LocalVariables)
                result.LocalVariables.Add(localVariableInfoDataConverter.Build(localVariable));

            result.Instructions = new List<ILInstructionData>();
            foreach (var instruction in MethodReader.Read(methodInfo))
            {
                //Console.WriteLine(instruction);
                var operand = instruction.Operand;
                var code = (OpCodeValues) (instruction.Code.Value & 0xffff);

                var il = new ILInstructionData();

                if (!methodInfo.IsStatic)
                {
                    if (code == OpCodeValues.Ldarg_0)
                        throw new DelegateSerializationException("Method reference to this");
                    if (code == OpCodeValues.Ldarg_1)
                        code = OpCodeValues.Ldarg_0;
                    else if (code == OpCodeValues.Ldarg_2)
                        code = OpCodeValues.Ldarg_1;
                    else if (code == OpCodeValues.Ldarg_3)
                        code = OpCodeValues.Ldarg_2;
                    else if (code == OpCodeValues.Ldarg_S && (byte) operand == 4)
                    {
                        code = OpCodeValues.Ldarg_3;
                        operand = null;
                    }
                    else if (code == OpCodeValues.Ldarg_S && (byte) operand > 4)
                        operand = (byte) operand - 1;
                }
                if (code == OpCodeValues.Ldftn)
                {
                    il.OperandDelegateData = Serialize((MethodInfo) instruction.Operand);
                    operand = null;
                }
                else if (operand is FieldInfo)
                {
                    var field = (FieldInfo) operand;
                    var fieldFullName = field.DeclaringType.FullName + "_" + field.Name;
                    if (field.Name.StartsWith("CS$<>9__CachedAnonymousMethodDelegate") || field.Name.StartsWith("<>9"))
                    {
                        int local;
                        if (!fieldsToLocals.TryGetValue(fieldFullName, out local) && field.FieldType.Name != "<>c")
                        {
                            result.LocalVariables.Add(localVariableInfoDataConverter.Build(field));
                            local = result.LocalVariables.Count - 1;
                            fieldsToLocals.Add(fieldFullName, local);
                        }


                        if (code == OpCodeValues.Ldsfld)
                        {
                            if (field.FieldType.Name == "<>c")
                            {
                                code = OpCodeValues.Ldnull;
                                operand = null;
                            }
                            else
                            {
                                code = OpCodeValues.Ldloc;
                                operand = local;
                            }
                        }
                        else if (code == OpCodeValues.Stsfld)
                        {
                            code = OpCodeValues.Stloc;
                            operand = local;
                        }
                        else
                            throw new DelegateSerializationException("Unknown field operation");
                    }
                    else
                        throw new DelegateSerializationException(string.Format("Unknown field info {0}", fieldFullName));
                }
                else if (operand is ConstructorInfo)
                {
                    il.OperandConstructor = constructorInfoDataConverter.Build((ConstructorInfo) operand);
                    operand = null;
                }
                else if (operand is MethodInfo)
                {
                    il.OperandMethod = methodInfoDataConverter.Build((MethodInfo) operand);
                    operand = null;
                }

                il.Code = (uint) code;
                il.Offset = instruction.Offset;
                il.Operand = operand;
                result.Instructions.Add(il);
            }
            return result;
        }
    }
}