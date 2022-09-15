using System;
using System.Runtime.Serialization;

namespace FRW.TR.Contrats
{
    [DataContract]
    public class Erreur
    {
        /// <summary>
        /// Numéro fonctionnel de l'erreur, si disponible
        /// </summary>
        [DataMember]
        public string No { get; set; } = default!;
        /// <summary>
        /// Contient le message de l'erreur
        /// </summary>
        [DataMember]
        public string Message { get; set; } = default!;
        /// <summary>
        /// Permet d'indiquer sur quoi l'erreur doit ou devrait s'afficher
        /// </summary>
        [DataMember]
        public string Cible { get; set; } = default!;
    }
}
