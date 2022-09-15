using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace FRW.TR.Contrats
{
    [DataContract]
    public class EntrantCreerSession
    {
        [DataMember]
        public int NsFormulaire { get; set; }

        [DataMember]
        public string CodeNatureSession { get; set; } = default!;
    }
}
