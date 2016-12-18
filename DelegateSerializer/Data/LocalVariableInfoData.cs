using System.Runtime.Serialization;

namespace DelegateSerializer.Data
{
    [DataContract]
    public class LocalVariableInfoData
    {
        [DataMember]
        public TypeInfoData LocalType { get; set; }

        [DataMember]
        public bool IsPinned { get; set; }
    }
}