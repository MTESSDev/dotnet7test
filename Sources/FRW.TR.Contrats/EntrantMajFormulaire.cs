using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace FRW.TR.Contrats
{
    [DataContract]
    public class EntrantMajFormulaire
    {
        [DataMember]
        public int NsFormulaire { get; set; }

        [DataMember]
        public Guid? NoPublicSession { get; set; }

        [DataMember]
        public int NoSystemeAutorise { get; set; }

        [DataMember]
        public string TypeFormulaire { get; set; } = default!;

        [DataMember]
        public string VersionConfig { get; set; } = default!;

        [DataMember]
        public string JsonData { get; set; } = string.Empty;

        [DataMember]
        public bool EnvoyerCourriel { get; set; }

        [DataMember]
        public IDictionary<object, object>? Systeme { get; set; }

        [DataMember]
        public long? NoConfirmation { get; set; }

    }
}
