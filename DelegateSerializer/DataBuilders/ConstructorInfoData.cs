using System.Linq;
using System.Reflection;
using DelegateSerializer.Data;

namespace DelegateSerializer.DataBuilders
{
    public class ConstructorInfoDataBuilder
    {
        private readonly TypeInfoDataBuilder typeInfoDataBuilder;

        public ConstructorInfoDataBuilder(TypeInfoDataBuilder typeInfoDataBuilder)
        {
            this.typeInfoDataBuilder = typeInfoDataBuilder;
        }

        public ConstructorInfoData Build(ConstructorInfo c)
        {
            return new ConstructorInfoData
                   {
                       DeclaringType = typeInfoDataBuilder.Build(c.DeclaringType),
                       ParameterTypes = c.GetParameters()
                                         .Select(t => typeInfoDataBuilder.Build(t.ParameterType))
                                         .ToArray(),
                   };
        }
    }
}