using System;
using System.Runtime.Serialization;

namespace FRW.TR.Contrats
{
    [DataContract]
    public class SystemeAutorise
    {
        /// <summary>
        /// Numéro séquentiel de l'autre systeme
        /// </summary>
        [DataMember]
        public int ID { get; set; }

        /// <summary>
        /// Numéro séquentiel de publication de l'autre systeme
        /// </summary>
        [DataMember]
        public Guid NumeroPublique { get; set; }

        /// <summary>
        /// Nom autre systeme
        /// </summary>
        [DataMember]
        public string? Nom { get; set; }

        /// <summary>
        /// Date de debut du systeme
        /// </summary>        
        [DataMember]
        public DateTime DateDebutAutorisation { get; set; }

        /// <summary>
        /// Code du systeme
        /// </summary>
        [DataMember]
        public string? Code { get; set; }

        /// <summary>
        /// Date de fin du systeme
        /// </summary>
        [DataMember]
        public DateTime? DateFinAutorisation { get; set; }
    }
}
