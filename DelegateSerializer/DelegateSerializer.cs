using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DelegateSerializer.Data;
using DelegateSerializer.SDILReader;

namespace DelegateSerializer
{
    public partial class DelegateSerializer : IDelegateSerializer
    {
        private readonly TypeResolver typeResolver;

        public DelegateSerializer(TypeResolver typeResolver)
        {
            this.typeResolver = typeResolver;
        }

        public DelegateData Serialize(MethodInfo methodInfo)
        {
            var methodBody = methodInfo.GetMethodBody();
            if (methodBody == null)
                return null;
            var instructions = new MethodBodyReader(methodInfo).GetInstructions();
            if (instructions == null)
                return null;

            var fieldsToLocals = new Dictionary<string, int>();
            var result = new DelegateData();
            result.ReturnType = BuildTypeInfoData(methodInfo.ReturnType);
            result.ParametersType =
                methodInfo.GetParameters().Select(info => BuildTypeInfoData(info.ParameterType)).ToArray();
            result.ExceptionHandlingClauses =
                methodBody.ExceptionHandlingClauses.Select(c => new ExceptionHandlingClauseData
                                                                {
                                                                    Flags = c.Flags,
                                                                    CatchType =
                                                                        c.Flags == ExceptionHandlingClauseOptions.Clause
                                                                            ? BuildTypeInfoData(c.CatchType)
                                                                            : null,
                                                                    TryOffset = c.TryOffset,
                                                                    TryLength = c.TryLength,
                                                                    HandlerOffset = c.HandlerOffset,
                                                                    HandlerLength = c.HandlerLength
                                                                })
                          .ToArray();
            result.LocalVariables = new List<LocalVariableInfoData>();
            foreach (var localVariable in methodBody.LocalVariables)
                result.LocalVariables.Add(new LocalVariableInfoData
                                  {
                                      LocalType = BuildTypeInfoData(localVariable.LocalType),
                                      IsPinned = localVariable.IsPinned
                                  });
            result.Instructions = new List<ILInstructionData>();
            foreach (var instruction in instructions)
            {
                var code = (OpCodeValues)(instruction.Code.Value & 0xffff);
                var item = new ILInstructionData
                           {
                               Offset = instruction.Offset,
                           };
                var operand = instruction.Operand;
                if (code == OpCodeValues.Ldftn)
                {
                    item.OperandDelegateData = Serialize((MethodInfo) instruction.Operand);
                    operand = null;
                }
                else if (operand is FieldInfo)
                {
                    var field = (FieldInfo) operand;
                    var fieldFullName = field.DeclaringType.FullName + "_" + field.Name;
                    
                    if (field.Name.StartsWith("CS$<>9__CachedAnonymousMethodDelegate"))
                    {
                        
                        int local;
                        if (!fieldsToLocals.TryGetValue(fieldFullName, out local))
                        {
                            result.LocalVariables.Add(new LocalVariableInfoData
                            {
                                LocalType = BuildTypeInfoData(field.FieldType)
                            });
                            local = result.LocalVariables.Count - 1;
                            fieldsToLocals.Add(fieldFullName, local);
                        }
                        
                        
                        if (code == OpCodeValues.Ldsfld)
                        {
                            code = OpCodeValues.Ldloc;
                            operand = local;
                        }
                        else if (code == OpCodeValues.Stsfld)
                        {
                            code = OpCodeValues.Stloc;
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
                    var m = (ConstructorInfo)operand;
                    item.OperandConstructor = new ConstructorInfoData
                                              {
                                                  DeclaringType = BuildTypeInfoData(m.DeclaringType),
                                                  ParameterTypes =
                                                      m.GetParameters()
                                                       .Select(t => BuildTypeInfoData(t.ParameterType))
                                                       .ToArray(),
                                              };
                    operand = null;
                }
                else if (operand is MethodInfo)
                {
                    var m = (MethodInfo) operand;
                    item.OperandMethod = new MethodInfoData
                                             {
                                                 Name = m.Name,
                                                 DeclaringType = BuildTypeInfoData(m.DeclaringType),
                                                 ParameterTypes =
                                                     m.GetParameters()
                                                      .Select(t => BuildTypeInfoData(t.ParameterType))
                                                      .ToArray(),
                                                 GenericArgumentTypes =
                                                     m.GetGenericArguments().Select(BuildTypeInfoData).ToArray(),
                                             };
                    operand = null;
                }
                item.Code = code;
                item.Operand = operand;
                result.Instructions.Add(item);
            }
            return result;
        }


        private static TypeInfoData BuildTypeInfoData(Type type)
        {
            return new TypeInfoData
                   {
                       Name = type.FullName
                   };
        }
    }
}