using System;
using System.Runtime.Serialization;

namespace FRW.TR.Contrats
{
    [DataContract]
    public class EntrantValiderReprise
    {
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string? NoPublicSessionCrypte { get; set; }

        [DataMember]
        public Guid NoPublicSessionClair { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string? MotDePasse { get; set; }

        [DataMember]
        public int NsFormulaire { get; set; }
    }
}
