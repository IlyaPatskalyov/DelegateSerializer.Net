using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace DelegateSerializer.Data
{
    [Serializable]
    [DataContract]
    public class ExceptionHandlingClauseData
    {
        [DataMember]
        public TypeInfoData CatchType { get; set; }

        [DataMember]
        public ExceptionHandlingClauseOptions Flags { get; set; }

        [DataMember]
        public int TryOffset { get; set; }

        [DataMember]
        public int TryLength { get; set; }

        [DataMember]
        public int HandlerOffset { get; set; }

        [DataMember]
        public int HandlerLength { get; set; }
    }
}