using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using DelegateSerializer.Data;
using DelegateSerializer.Exceptions;
using DelegateSerializer.Helpers;
using OpCodeValues = DelegateSerializer.ILReader.OpCodeValues;

namespace DelegateSerializer
{
    public sealed partial class DelegateSerializer
    {
        private const string AssemblyFileName = "asm.dll";

        public TFunc Deserialize<TFunc>(DelegateData m) where TFunc : class
        {
            var method = new DynamicMethod(Guid.NewGuid().ToString(), typeResolver.GetType(m.ReturnType),
                                           typeResolver.GetTypes(m.ParametersType), true);
            AssemblyBuilder assemblyBuilder = null;
            ModuleBuilder moduleBuilder = null;
            BuildMethod(m, method.GetILGenerator(), ref assemblyBuilder, ref moduleBuilder);
            return method.CreateDelegate(typeof(TFunc)) as TFunc;
        }

        internal TFunc DeserializeForDebug<TFunc>(DelegateData delegateData) where TFunc : class
        {
            AssemblyBuilder assemblyBuilder = null;
            ModuleBuilder moduleBuilder = null;
            var method = BuildAssembly(delegateData, ref assemblyBuilder, ref moduleBuilder, saveAssembly: true);

            assemblyBuilder.Save("asm.dll");
            return method.CreateDelegate(typeof(TFunc)) as TFunc;
        }

        private void BuildMethod(DelegateData m, ILGenerator il, ref
                                     AssemblyBuilder assemblyBuilder, ref ModuleBuilder moduleBuilder)
        {
            foreach (var localVariable in m.LocalVariables)
                localVariableInfoDataConverter.Emit(il, localVariable);

            var labels = new Dictionary<int, Label>();
            foreach (var ilInstruction in m.Instructions)
                if (((OpCodeValues) ilInstruction.Code).IsLabel())
                {
                    var instruction = (int) ilInstruction.Operand;
                    if (!labels.ContainsKey(instruction))
                        labels.Add(instruction, il.DefineLabel());
                }

            foreach (var ilInstruction in m.Instructions)
            {
                foreach (var clause in m.ExceptionHandlingClauses)
                    exceptionHandlingClauseDataConverter.Emit(il, ilInstruction.Offset, clause);

                var opCodeValues = (OpCodeValues) (ilInstruction.Code);
                var code = opCodeValues.GetOpCode();
                if (labels.ContainsKey(ilInstruction.Offset))
                    il.MarkLabel(labels[ilInstruction.Offset]);

                var operand = ilInstruction.Operand;
                if (ilInstruction.OperandDelegateData != null)
                    il.Emit(code, BuildAssembly(ilInstruction.OperandDelegateData, ref assemblyBuilder, ref moduleBuilder));
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
                else if (!(operand is Type))
                {
                    if (operand != null)
                        throw new DelegateDeserializationException(
                            string.Format("Unknown operand type {0} for opCode {1}", operand.GetType(), opCodeValues));
                    il.Emit(code);
                }
            }
        }

        private MethodInfo BuildAssembly(DelegateData delegateData,
                                         ref AssemblyBuilder assemblyBuilder,
                                         ref ModuleBuilder moduleBuilder,
                                         bool saveAssembly = false)
        {
            var assemblyName = string.Format("DelegateSerializer_{0}", Guid.NewGuid());
            var typeName = string.Format("InternalDelegate_{0}", Guid.NewGuid());

            if (assemblyBuilder == null)
            {
                if (saveAssembly)
                    assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(AssemblyFileName),
                                                                                    AssemblyBuilderAccess.RunAndSave);

                else
                {
                    assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(
                        new AssemblyName(assemblyName),
                        AssemblyBuilderAccess.RunAndSave);
                }
            }

            if (moduleBuilder == null)
            {
                if (saveAssembly)
                    moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName, AssemblyFileName);
                else
                    moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName);
            }

            var typeBuilder = moduleBuilder.DefineType(typeName, TypeAttributes.Public, null);
            var methodName = "Delegate";
            var methodBuilder = typeBuilder.DefineMethod(methodName,
                                                         MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig,
                                                         typeResolver.GetType(delegateData.ReturnType),
                                                         typeResolver.GetTypes(delegateData.ParametersType));

            BuildMethod(delegateData, methodBuilder.GetILGenerator(), ref assemblyBuilder, ref moduleBuilder);
            var type = typeBuilder.CreateType();

            var method = typeResolver.GetMethod(type, methodName, typeResolver.GetTypes(delegateData.ParametersType), new Type[0]);
            return method;
        }
    }
}