using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using DelegateSerializer.Data;
using DelegateSerializer.Helpers;
using DelegateSerializer.SDILReader;

namespace DelegateSerializer
{
    public partial class DelegateSerializer
    {
        public Delegate Deserialize<TFunc>(DelegateData m)
        {
            return Deserialize(m, typeof(TFunc));
        }

        public Delegate Deserialize(DelegateData m, Type methodType)
        {
            var method = new DynamicMethod(Guid.NewGuid().ToString(), typeResolver.GetType(m.ReturnType),
                                           typeResolver.GetTypes(m.ParametersType), true);
            BuildMethod(m, method.GetILGenerator());
            return method.CreateDelegate(methodType);
        }

        public static readonly ConstructorInfo objectCtor = typeof(object).GetConstructor(Type.EmptyTypes);

        public void Deserialize2(DelegateData m)
        {
            var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(
                new AssemblyName("asm.dll"),
                AssemblyBuilderAccess.RunAndSave);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(Guid.NewGuid().ToString(), "asm.dll");
            var typeBuilder =
                moduleBuilder.DefineType(
                    string.Format("Test_ {0}", Guid.NewGuid()),
                    TypeAttributes.Public, null,
                    new Type[0]);

            BuildMethod(typeBuilder, string.Format("Method {0}", Guid.NewGuid()), m, moduleBuilder);

            ConstructorBuilder constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public,
                                                                                  CallingConventions.Standard, new Type[0]);
            var il = constructorBuilder.GetILGenerator();
            il.Emit(OpCodes.Ldarg, 0);
            il.Emit(OpCodes.Call, objectCtor);
            il.Emit(OpCodes.Ret);

            Type type = typeBuilder.CreateType();

            assemblyBuilder.Save(@"asm.dll");
        }

        public MethodBuilder BuildMethod(TypeBuilder typeBuilder, string methodName, DelegateData m, ModuleBuilder moduleBuilder = null)
        {
            var methodBuilder = typeBuilder.DefineMethod(methodName,
                                                         MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.NewSlot,
                                                         typeResolver.GetType(m.ReturnType),
                                                         typeResolver.GetTypes(m.ParametersType));
            BuildMethod(m, methodBuilder.GetILGenerator(), moduleBuilder);
            return methodBuilder;
        }

        public void BuildMethod(DelegateData m, ILGenerator il, ModuleBuilder moduleBuilder = null)
        {
            foreach (var localVariable in m.LocalVariables)
                il.DeclareLocal(typeResolver.GetType(localVariable.LocalType), localVariable.IsPinned);

            var labels = new Dictionary<int, Label>();
            foreach (var ilInstruction in m.Instructions)
                if (ilInstruction.Code.IsLabel())
                {
                    var instruction = (int) ilInstruction.Operand;
                    if (!labels.ContainsKey(instruction))
                        labels.Add(instruction, il.DefineLabel());
                }

            foreach (var ilInstruction in m.Instructions)
            {
                foreach (var clause in m.ExceptionHandlingClauses)
                    exceptionHandlingClauseDataBuilder.Emit(il, ilInstruction.Offset, clause);

                var opCodeValues = ilInstruction.Code;
                var code = opCodeValues.GetOpCode();
                if (labels.ContainsKey(ilInstruction.Offset))
                    il.MarkLabel(labels[ilInstruction.Offset]);

                var operand = ilInstruction.Operand;
                if (ilInstruction.OperandDelegateData != null)
                {
                    if (moduleBuilder == null)
                    {
                        var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(
                            new AssemblyName("LambdaSerializer_" + Guid.NewGuid()),
                            AssemblyBuilderAccess.RunAndSave);
                        moduleBuilder =
                            assemblyBuilder.DefineDynamicModule("LambdaSerializerModule_" + Guid.NewGuid());
                    }

                    var typeBuilder = moduleBuilder.DefineType(
                        string.Format("SupportInfo_{0}", Guid.NewGuid()), TypeAttributes.Public, null);
                    var methodName = "Lambda_" + Guid.NewGuid();
                    var z2 = typeBuilder.DefineMethod(methodName,
                                                      MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig,
                                                      typeResolver.GetType(ilInstruction.OperandDelegateData.ReturnType),
                                                      typeResolver.GetTypes(
                                                          ilInstruction.OperandDelegateData.ParametersType));

                    BuildMethod(ilInstruction.OperandDelegateData, z2.GetILGenerator());
                    var type = typeBuilder.CreateType();

                    var m2 = typeResolver.GetMethod(type, methodName, typeResolver.GetTypes(
                                                        ilInstruction.OperandDelegateData.ParametersType), new Type[0]);
                    il.Emit(code, m2);
                }
                else if (ilInstruction.OperandConstructor != null)
                    il.Emit(code, typeResolver.GetConstructor(ilInstruction.OperandConstructor));
                else if (ilInstruction.OperandMethod != null)
                    il.Emit(code, typeResolver.GetMethod(ilInstruction.OperandMethod));
                else if (ilInstruction.OperandType != null)
                    il.Emit(code, typeResolver.GetType(ilInstruction.OperandType));
                else if (opCodeValues.IsLabel())
                    il.Emit(code, labels[(int) ilInstruction.Operand]);
                else if (operand is sbyte)
                    il.Emit(code, (sbyte) operand);
                else if (operand is byte)
                    il.Emit(code, (byte) operand);
                else if (operand is int)
                    il.Emit(code, (int) operand);
                else if (operand is long)
                    il.Emit(code, (long) operand);
                else if (operand is string)
                    il.Emit(code, (string) operand);
                else if (operand is Type)
                {
                }
                else
                {
                    if (operand != null)
                        throw new Exception(string.Format("Unknown operand type {0} for opCode {1}", operand.GetType(), opCodeValues));
                    il.Emit(code);
                }
            }
        }
    }
}