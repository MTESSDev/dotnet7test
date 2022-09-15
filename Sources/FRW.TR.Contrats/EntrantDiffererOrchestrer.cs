using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace FRW.TR.Contrats
{
    [DataContract]
    public class EntrantDiffererOrchestrer
    {
        [DataMember]
        public DateTime? DateExecution { get; set; }
    }
}
