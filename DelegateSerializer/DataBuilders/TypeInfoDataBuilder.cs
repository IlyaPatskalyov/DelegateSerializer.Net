using System;
using DelegateSerializer.Data;

namespace DelegateSerializer.DataBuilders
{
    public class TypeInfoDataBuilder
    {
        public TypeInfoData Build(Type type)
        {
            return new TypeInfoData
                   {
                       Name = type.FullName
                   };
        }
    }
}