using System;
using System.Runtime.Serialization;

namespace FRW.TR.Contrats.TransmissionDocuments
{
    [DataContract]
    public class EntrantTransmettre
    {
        [DataMember]
        public byte[] Fichier { get; set; } = default!;

        /// <summary>
        /// Préfix du nom du fichier OU le nom complet si l'indicateur <see cref="ForcerNomComplet"/> est true
        /// </summary>
        [DataMember]
        public string Prefix { get; set; } = default!;

        /// <summary>
        /// Permet de forcer le nom complet au lieu du préfix seulement
        /// </summary>
        [DataMember]
        public bool ForcerNomComplet { get; set; } = default!;
    }
}
