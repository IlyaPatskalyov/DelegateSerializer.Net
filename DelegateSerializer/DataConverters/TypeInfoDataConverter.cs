using System;
using DelegateSerializer.Data;

namespace DelegateSerializer.DataConverters
{
    internal class TypeInfoDataConverter
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