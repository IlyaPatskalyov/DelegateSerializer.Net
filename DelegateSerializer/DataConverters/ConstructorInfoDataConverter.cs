using System.Linq;
using System.Reflection;
using DelegateSerializer.Data;

namespace DelegateSerializer.DataConverters
{
    internal class ConstructorInfoDataConverter
    {
        private readonly TypeInfoDataConverter typeInfoDataConverter;

        public ConstructorInfoDataConverter(TypeInfoDataConverter typeInfoDataConverter)
        {
            this.typeInfoDataConverter = typeInfoDataConverter;
        }

        public ConstructorInfoData Build(ConstructorInfo c)
        {
            return new ConstructorInfoData
                   {
                       DeclaringType = typeInfoDataConverter.Build(c.DeclaringType),
                       ParameterTypes = c.GetParameters()
                                         .Select(t => typeInfoDataConverter.Build(t.ParameterType))
                                         .ToArray(),
                   };
        }
    }
}