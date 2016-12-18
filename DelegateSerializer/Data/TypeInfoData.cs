using System.Runtime.Serialization;

namespace DelegateSerializer.Data
{
    [DataContract]
    public class TypeInfoData
    {
        [DataMember]
        public string Name { get; set; } 
    }
}