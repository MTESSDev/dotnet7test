using System.Collections.Generic;
using System.Runtime.Serialization;

namespace FRW.TR.Contrats
{
    [DataContract]
    public class EntrantRechercherFormulaires
    {
        [DataMember]
        public int? NsFormulaire { get; set; }

        [DataMember]
        public int? IdSystemeAutorise { get; set; }

        [DataMember]
        public string? IdentifiantUtilisateur { get; set; }

        [DataMember]
        public List<string>? EtatsFormulaireRecherche { get; set; }

        [DataMember]
        public string? CodeTypeFormulaire { get; set; }

        [DataMember]
        public int? NoConfirmation { get; set; }

        [DataMember]
        public string? NoPublicFormulaire { get; set; }

    }
}
