using System.Runtime.Serialization;

namespace FRW.TR.Contrats
{
    [DataContract]
    public class Adresse
    {
        [DataMember]
        public string Rue { get; set; } = default!;

        [DataMember] 
        public string Municipalite { get; set; } = default!;

        [DataMember]
        public string MunicipaliteNomLong { get; set; } = default!;

        [DataMember]
        public string CodePostal { get; set; } = default!;
    }
}
