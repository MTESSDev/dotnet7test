using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace FRW.TR.Contrats
{
    [DataContract]
    public class EntrantCreerFormulaire
    {
        [DataMember]
        public string TypeFormulaire { get; set; } = string.Empty;

        [DataMember]
        public string? JsonData { get; set; }

        [DataMember]
        public Guid? NoPublicForm { get; set; } = null;

        [DataMember]
        public int IdSystemeAutorise { get; set; }

        [DataMember]
        public string? IdentifiantUtilisateur { get; set; } = default!;
    }
}
