using System;
using System.Reflection;
using DelegateSerializer.Data;

namespace DelegateSerializer
{
    public interface IDelegateSerializer
    {
        DelegateData Serialize(MethodInfo methodInfo);
        TFunc Deserialize<TFunc>(DelegateData m) where TFunc : class;
    }
}