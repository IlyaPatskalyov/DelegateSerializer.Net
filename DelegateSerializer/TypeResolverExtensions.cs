using System;
using System.Linq;
using System.Reflection;
using DelegateSerializer.Data;

namespace DelegateSerializer
{
    internal static class TypeResolverExtensions
    {
        public static MethodInfo GetMethod(this TypeResolver typeResolver, MethodInfoData methodInfoData)
        {
            if (methodInfoData != null)
                return typeResolver.GetMethod(typeResolver.GetType(methodInfoData.DeclaringType),
                    methodInfoData.Name, typeResolver.GetTypes(methodInfoData.ParameterTypes),
                    typeResolver.GetTypes(methodInfoData.GenericArgumentTypes));
            return null;
        }

        public static ConstructorInfo GetConstructor(this TypeResolver typeResolver, ConstructorInfoData constructorInfoData)
        {
            if (constructorInfoData != null)
                return typeResolver.GetConstructor(typeResolver.GetType(constructorInfoData.DeclaringType),
                     typeResolver.GetTypes(constructorInfoData.ParameterTypes));
            return null;
        }


        public static Type GetType(this TypeResolver typeResolver, TypeInfoData typeInfoData)
        {
            if (typeInfoData != null && typeInfoData.Name != null)
                return typeResolver.GetType(typeInfoData.Name);
            return null;
        }

        public static Type[] GetTypes(this TypeResolver typeResolver, TypeInfoData[] typeInfoDatas)
        {
            if (typeInfoDatas != null)
                return typeInfoDatas.Select(t => typeResolver.GetType(t.Name)).ToArray();
            return null;
        }
    }
}