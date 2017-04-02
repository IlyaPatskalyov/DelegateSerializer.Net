using System;
using System.Runtime.Serialization;

namespace DelegateSerializer.Data
{
    [Serializable]
    [DataContract]
    public class MethodInfoData
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public TypeInfoData DeclaringType { get; set; }

        [DataMember]
        public TypeInfoData[] ParameterTypes { get; set; }

        [DataMember]
        public TypeInfoData[] GenericArgumentTypes { get; set; }
    }
}