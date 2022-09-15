using System.Runtime.Serialization;

namespace FRW.TR.Contrats
{
    [DataContract]
    public class EntrantObtenirIdentifiantSessionFormulaire
    {
        [DataMember]
        public string NoPublicFormulaire { get; set; } = default!;

        [DataMember]
        public int IdSystemeAutorise { get; set; }

        [DataMember]
        public string IdentifiantUtilisateur { get; set; } = default!;

        [DataMember]
        public string? JsonData { get; set; }
    }
}
