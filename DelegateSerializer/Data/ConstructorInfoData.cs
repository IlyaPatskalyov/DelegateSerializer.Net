using System;
using System.Runtime.Serialization;

namespace DelegateSerializer.Data
{
    [Serializable]
    [DataContract]
    public class ConstructorInfoData
    {
        [DataMember]
        public TypeInfoData DeclaringType { get; set; }

        [DataMember]
        public TypeInfoData[] ParameterTypes { get; set; }
    }
}