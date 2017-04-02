using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DelegateSerializer.Data;
using DelegateSerializer.DataBuilders;
using DelegateSerializer.SDILReader;

namespace DelegateSerializer
{
    public partial class DelegateSerializer : IDelegateSerializer
    {
        private readonly TypeResolver typeResolver;
        private readonly TypeInfoDataBuilder typeInfoDataBuilder;
        private readonly MethodInfoDataBuilder methodInfoDataBuilder;
        private readonly ExceptionHandlingClauseDataBuilder exceptionHandlingClauseDataBuilder;
        private readonly ConstructorInfoDataBuilder constructorInfoDataBuilder;
        private readonly LocalVariableInfoDataBuilder localVariableInfoDataBuilder;

        public DelegateSerializer(TypeResolver typeResolver,
                                  TypeInfoDataBuilder typeInfoDataBuilder,
                                  MethodInfoDataBuilder methodInfoDataBuilder,
                                  ExceptionHandlingClauseDataBuilder exceptionHandlingClauseDataBuilder,
                                  ConstructorInfoDataBuilder constructorInfoDataBuilder,
                                  LocalVariableInfoDataBuilder localVariableInfoDataBuilder)
        {
            this.typeResolver = typeResolver;
            this.typeInfoDataBuilder = typeInfoDataBuilder;
            this.methodInfoDataBuilder = methodInfoDataBuilder;
            this.exceptionHandlingClauseDataBuilder = exceptionHandlingClauseDataBuilder;
            this.constructorInfoDataBuilder = constructorInfoDataBuilder;
            this.localVariableInfoDataBuilder = localVariableInfoDataBuilder;
        }

        public DelegateData Serialize(MethodInfo methodInfo)
        {
            var methodBody = methodInfo.GetMethodBody();
            if (methodBody == null)
                return null;

            var fieldsToLocals = new Dictionary<string, int>();
            var result = new DelegateData();
            result.ReturnType = typeInfoDataBuilder.Build(methodInfo.ReturnType);
            result.ParametersType = methodInfo.GetParameters()
                                              .Select(info => typeInfoDataBuilder.Build(info.ParameterType))
                                              .ToArray();
            result.ExceptionHandlingClauses = methodBody.ExceptionHandlingClauses
                                                        .Select(c => exceptionHandlingClauseDataBuilder.Build(c))
                                                        .ToArray();
            result.LocalVariables = new List<LocalVariableInfoData>();
            foreach (var localVariable in methodBody.LocalVariables)
                result.LocalVariables.Add(localVariableInfoDataBuilder.Build(localVariable));

            result.Instructions = new List<ILInstructionData>();
            foreach (var instruction in MethodBodyReader.Read(methodInfo))
            {
                var operand = instruction.Operand;
                var item = new ILInstructionData
                           {
                               Code = (OpCodeValues) (instruction.Code.Value & 0xffff),
                               Offset = instruction.Offset,
                           };
                if (!methodInfo.IsStatic)
                {
                    if (item.Code == OpCodeValues.Ldarg_0)
                        throw new Exception("Method reference to this");
                    if (item.Code == OpCodeValues.Ldarg_1)
                        item.Code = OpCodeValues.Ldarg_0;
                    else if (item.Code == OpCodeValues.Ldarg_2)
                        item.Code = OpCodeValues.Ldarg_1;
                    else if (item.Code == OpCodeValues.Ldarg_3)
                        item.Code = OpCodeValues.Ldarg_2;
                    else if (item.Code == OpCodeValues.Ldarg_S && (byte) operand == 4)
                    {
                        item.Code = OpCodeValues.Ldarg_3;
                        operand = null;
                    }
                    else if (item.Code == OpCodeValues.Ldarg_S && (byte) operand > 4)
                        operand = (byte) operand - 1;
                }
                if (item.Code == OpCodeValues.Ldftn)
                {
                    item.OperandDelegateData = Serialize((MethodInfo) instruction.Operand);
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
                            result.LocalVariables.Add(localVariableInfoDataBuilder.Build(field));
                            local = result.LocalVariables.Count - 1;
                            fieldsToLocals.Add(fieldFullName, local);
                        }


                        if (item.Code == OpCodeValues.Ldsfld)
                        {
                            if (field.FieldType.Name == "<>c")
                            {
                                item.Code = OpCodeValues.Ldnull;
                                operand = null;
                            }
                            else
                            {
                                item.Code = OpCodeValues.Ldloc;
                                operand = local;
                            }
                        }
                        else if (item.Code == OpCodeValues.Stsfld)
                        {
                            item.Code = OpCodeValues.Stloc;
                            operand = local;
                        }
                        else
                            throw new Exception("Unknown field operation");
                    }
                    else
                        throw new Exception(string.Format("Unknown field info {0}", fieldFullName));
                }
                else if (operand is ConstructorInfo)
                {
                    item.OperandConstructor = constructorInfoDataBuilder.Build((ConstructorInfo) operand);
                    operand = null;
                }
                else if (operand is MethodInfo)
                {
                    item.OperandMethod = methodInfoDataBuilder.Build((MethodInfo) operand);
                    operand = null;
                }

                item.Operand = operand;
                result.Instructions.Add(item);
            }
            return result;
        }
    }
}