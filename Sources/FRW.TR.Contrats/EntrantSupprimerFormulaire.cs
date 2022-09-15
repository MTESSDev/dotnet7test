using System.Runtime.Serialization;

namespace FRW.TR.Contrats
{
    [DataContract]
    public class EntrantSupprimerFormulaire
    {
        [DataMember]
        public string NoPublicFormulaire { get; set; } = default!;

        [DataMember]
        public int IdSystemeAutorise { get; set; }

        [DataMember]
        public string IdentifiantUtilisateur { get; set; } = default!;
    }
}
