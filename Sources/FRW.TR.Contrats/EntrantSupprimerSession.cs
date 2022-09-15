using System;
using System.Runtime.Serialization;

namespace FRW.TR.Contrats
{
    [DataContract]
    public class EntrantSupprimerSession
    {
        [DataMember]
        public Guid NumeroSession { get; set; }

        [DataMember]
        public string? CodeNatureSession { get; set; }
    }
}
