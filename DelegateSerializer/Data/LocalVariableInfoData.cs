using System;
using System.Runtime.Serialization;

namespace DelegateSerializer.Data
{
    [Serializable]
    [DataContract]
    public class LocalVariableInfoData
    {
        [DataMember]
        public TypeInfoData LocalType { get; set; }

        [DataMember]
        public bool IsPinned { get; set; }
    }
}