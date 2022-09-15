using System.Collections.Generic;
using System.Runtime.Serialization;

namespace FRW.TR.Contrats
{
    [DataContract]
    public class EntrantRechercherFormulairesSIS
    {
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
