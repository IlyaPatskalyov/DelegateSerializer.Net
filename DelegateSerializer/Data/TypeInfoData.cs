using System;
using System.Runtime.Serialization;

namespace DelegateSerializer.Data
{
    [Serializable]
    [DataContract]
    public class TypeInfoData
    {
        [DataMember]
        public string Name { get; set; } 
    }
}