using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace FRW.TR.Contrats
{
    [DataContract]
    public class RetourAjouterSystemeAutorise
    {
        [DataMember]
        public int NoSequentiel { get; set; }

        [DataMember]
        public Guid GuidClePublic { get; set; }

        [DataMember]
        public string ShortguidClePublic { get; set; } = default!;
    }
}
