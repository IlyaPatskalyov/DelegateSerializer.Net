using System.Linq;
using System.Reflection;
using DelegateSerializer.Data;

namespace DelegateSerializer.DataBuilders
{
    public class MethodInfoDataBuilder
    {
        private readonly TypeInfoDataBuilder typeInfoDataBuilder;

        public MethodInfoDataBuilder(TypeInfoDataBuilder typeInfoDataBuilder)
        {
            this.typeInfoDataBuilder = typeInfoDataBuilder;
        }

        public MethodInfoData Build(MethodInfo m)
        {
            return new MethodInfoData
                   {
                       Name = m.Name,
                       DeclaringType = typeInfoDataBuilder.Build(m.DeclaringType),
                       ParameterTypes = m.GetParameters()
                                         .Select(t => typeInfoDataBuilder.Build(t.ParameterType))
                                         .ToArray(),
                       GenericArgumentTypes = m.GetGenericArguments()
                                               .Select(t => typeInfoDataBuilder.Build(t))
                                               .ToArray(),
                   };
        }
    }
}