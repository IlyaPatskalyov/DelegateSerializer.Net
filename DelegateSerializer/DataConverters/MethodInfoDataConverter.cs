using System.Linq;
using System.Reflection;
using DelegateSerializer.Data;

namespace DelegateSerializer.DataConverters
{
    internal class MethodInfoDataConverter
    {
        private readonly TypeInfoDataConverter typeInfoDataConverter;

        public MethodInfoDataConverter(TypeInfoDataConverter typeInfoDataConverter)
        {
            this.typeInfoDataConverter = typeInfoDataConverter;
        }

        public MethodInfoData Build(MethodInfo m)
        {
            return new MethodInfoData
                   {
                       Name = m.Name,
                       DeclaringType = typeInfoDataConverter.Build(m.DeclaringType),
                       ParameterTypes = m.GetParameters()
                                         .Select(t => typeInfoDataConverter.Build(t.ParameterType))
                                         .ToArray(),
                       GenericArgumentTypes = m.GetGenericArguments()
                                               .Select(t => typeInfoDataConverter.Build(t))
                                               .ToArray(),
                   };
        }
    }
}