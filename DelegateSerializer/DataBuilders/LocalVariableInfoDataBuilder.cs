using System.Reflection;
using DelegateSerializer.Data;

namespace DelegateSerializer.DataBuilders
{
    public class LocalVariableInfoDataBuilder
    {
        private readonly TypeInfoDataBuilder typeInfoDataBuilder;

        public LocalVariableInfoDataBuilder(TypeInfoDataBuilder typeInfoDataBuilder)
        {
            this.typeInfoDataBuilder = typeInfoDataBuilder;
        }

        public LocalVariableInfoData Build(LocalVariableInfo localVariable)
        {
            return new LocalVariableInfoData
                   {
                       LocalType = typeInfoDataBuilder.Build(localVariable.LocalType),
                       IsPinned = localVariable.IsPinned
                   };
        }

        public LocalVariableInfoData Build(FieldInfo f)
        {
            return new LocalVariableInfoData
                   {
                       LocalType = typeInfoDataBuilder.Build(f.FieldType)
                   };
        }
    }
}