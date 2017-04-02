using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DelegateSerializer.Data
{
    [Serializable]
    [DataContract]
    public class DelegateData
    {
        [DataMember]
        public TypeInfoData ReturnType { get; set; }

        [DataMember]
        public TypeInfoData[] ParametersType { get; set; }

        [DataMember]
        public ExceptionHandlingClauseData[] ExceptionHandlingClauses { get; set; }

        [DataMember]
        public List<LocalVariableInfoData> LocalVariables { get; set; }

        [DataMember]
        public List<ILInstructionData> Instructions { get; set; }
    }
}