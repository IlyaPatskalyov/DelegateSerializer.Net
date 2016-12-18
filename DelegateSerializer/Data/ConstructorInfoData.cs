using System.Runtime.Serialization;

namespace DelegateSerializer.Data
{
    [DataContract]
    public class ConstructorInfoData
    {
        [DataMember]
        public TypeInfoData DeclaringType { get; set; }

        [DataMember]
        public TypeInfoData[] ParameterTypes { get; set; }
    }
}