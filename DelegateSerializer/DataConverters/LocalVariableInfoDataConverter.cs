using System.Reflection;
using System.Reflection.Emit;
using DelegateSerializer.Data;

namespace DelegateSerializer.DataConverters
{
    internal class LocalVariableInfoDataConverter
    {
        private readonly TypeInfoDataConverter typeInfoDataConverter;
        private readonly TypeResolver typeResolver;

        public LocalVariableInfoDataConverter(TypeInfoDataConverter typeInfoDataConverter,
                                            TypeResolver typeResolver)
        {
            this.typeInfoDataConverter = typeInfoDataConverter;
            this.typeResolver = typeResolver;
        }

        public LocalVariableInfoData Build(LocalVariableInfo localVariable)
        {
            return new LocalVariableInfoData
                   {
                       LocalType = typeInfoDataConverter.Build(localVariable.LocalType),
                       IsPinned = localVariable.IsPinned
                   };
        }

        public LocalVariableInfoData Build(FieldInfo f)
        {
            return new LocalVariableInfoData
                   {
                       LocalType = typeInfoDataConverter.Build(f.FieldType)
                   };
        }

        public void Emit(ILGenerator il, LocalVariableInfoData localVariable)
        {
            il.DeclareLocal(typeResolver.GetType(localVariable.LocalType), localVariable.IsPinned);
        }
    }
}