using System;
using System.Reflection;
using DelegateSerializer.Data;

namespace DelegateSerializer
{
    public interface IDelegateSerializer
    {
        DelegateData Serialize(MethodInfo methodInfo);
        Delegate Deserialize<TFunc>(DelegateData m);
        Delegate Deserialize(DelegateData m, Type methodType);
    }
}