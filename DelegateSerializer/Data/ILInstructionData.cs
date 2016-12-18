using System.Runtime.Serialization;
using DelegateSerializer.SDILReader;

namespace DelegateSerializer.Data
{
    [DataContract]
    public class ILInstructionData
    {
        [DataMember]
        public OpCodeValues Code { get; set; }

        [DataMember]
        public int Offset { get; set; }

        [DataMember]
        public object Operand { get; set; }

        [DataMember]
        public MethodInfoData OperandMethod { get; set; }

        [DataMember]
        public TypeInfoData OperandType { get; set; }

        [DataMember]
        public DelegateData OperandDelegateData { get; set; }

        [DataMember]
        public ConstructorInfoData OperandConstructor { get; set; }
    }
}