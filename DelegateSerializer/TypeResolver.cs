using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DelegateSerializer
{
    internal class TypeResolver
    {
        private HashSet<Assembly> assemblies;

        public TypeResolver(IEnumerable<Assembly> customAssemblies = null)
        {
            assemblies = new HashSet<Assembly>(customAssemblies ?? new Assembly[0])
                         {
                             typeof (Enumerable).Assembly,
                             Assembly.GetExecutingAssembly()
                         };
        }

        public Type GetType(string typeName, IEnumerable<Type> genericArgumentTypes)
        {
            return GetType(typeName).MakeGenericType(genericArgumentTypes.ToArray());
        }

        public Type GetType(string typeName)
        {
            Type type;
            if (string.IsNullOrEmpty(typeName))
                throw new ArgumentNullException("typeName");
            if (typeName.EndsWith("[]"))
                return GetType(typeName.Substring(0, typeName.Length - 2)).MakeArrayType();

            foreach (Assembly assembly in assemblies)
            {
                type = assembly.GetType(typeName);
                if (type != null)
                    return type;
            }

            type = Type.GetType(typeName, false, true);
            if (type != null)
                return type;

            throw new ArgumentException("Could not find a matching type", typeName);
        }

        public MemberInfo GetField(Type declaringType, string fieldName)
        {
            return declaringType.GetProperty(fieldName);
        }

        public MemberInfo GetProperty(Type declaringType, string propertyName)
        {
            return declaringType.GetProperty(propertyName);
        }

        public ConstructorInfo GetConstructor(Type declaringType, Type[] parameterTypes)
        {
            return declaringType.GetConstructor(parameterTypes);
        }

        public MethodInfo GetMethod(Type declaringType, string name, Type[] parameterTypes, Type[] genArgTypes)
        {
            IEnumerable<MethodInfo> methods = from mi in declaringType.GetMethods()
                                              where mi.Name == name
                                              select mi;
            foreach (MethodInfo method in methods)
            {
                try
                {
                    MethodInfo realMethod = method;
                    if (method.IsGenericMethod)
                    {
                        realMethod = method.MakeGenericMethod(genArgTypes);
                    }
                    IEnumerable<Type> methodParameterTypes = realMethod.GetParameters().Select(p => p.ParameterType);
                    if (MatchPiecewise(parameterTypes, methodParameterTypes))
                    {
                        return realMethod;
                    }
                }
                catch (ArgumentException)
                {
                }
            }
            return null;
        }

        private bool MatchPiecewise<T>(IEnumerable<T> first, IEnumerable<T> second)
        {
            T[] firstArray = first.ToArray();
            T[] secondArray = second.ToArray();
            if (firstArray.Length != secondArray.Length)
                return false;
            for (int i = 0; i < firstArray.Length; i++)
                if (!firstArray[i].Equals(secondArray[i]))
                    return false;
            return true;
        }
    }
}