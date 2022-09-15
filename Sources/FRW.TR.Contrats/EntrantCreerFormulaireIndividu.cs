using System.Runtime.Serialization;

namespace FRW.TR.Contrats
{
    [DataContract]
    public class EntrantCreerFormulaireIndividu
    {
        [DataMember]
        public string? IdentifiantUtilisateur { get; set; }

        [DataMember]
        public string? JsonData { get; set; }

        [DataMember]
        public int IdSystemeAutorise { get; set; }

        [DataMember]
        public string TypeFormulaire { get; set; } = default!;
    }
}
