using System.Runtime.Serialization;

namespace FRW.TR.Contrats
{
    [DataContract]
    public class InfoFormulaire
    {
        [DataMember]
        public string? Id { get; set; }

        [DataMember]
        public string Version { get; set; } = default!;

        [DataMember]
        public string? TitreFrancais { get; set; }

        [DataMember]
        public string? TitreAnglais { get; set; }
    }
}