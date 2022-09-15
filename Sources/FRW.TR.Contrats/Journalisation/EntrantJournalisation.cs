using FRW.TR.Contrats.Contexte;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace FRW.TR.Contrats.Journalisation
{
    [DataContract]
    public class EntrantJournalisation
    {
        /// <summary>
        /// Permet de forcer un numéro unique identifiant la journalisation.
        /// </summary>
        [DataMember]
        public long? ForcerNumeroUnique { get; set; }

        /// <summary>
        /// Identifiant de la transaction d'affaires.
        /// </summary>
        [DataMember]
        public string CodeTransaction { get; set; } = default!;

        /// <summary>
        /// Code d'option de la transaction.
        /// </summary>
        [DataMember]
        public CodeOptionTransaction CodeOptionTransaction { get; set; }

        /// <summary>
        /// Type de traitement; Valeur par défaut à "Interactif" pour l'ensemble d'ECS.
        /// </summary>
        [DataMember]
        public TypeTraitement TypeTraitement { get; set; } = TypeTraitement.I;

        /// <summary>
        /// Type de lieu d'origine; Valeur par défaut à "Poste de travail" pour l'ensemble d'ECS.
        /// </summary>
        [DataMember]
        public TypeLieuOrigine TypeLieuOrigine { get; set; } = TypeLieuOrigine.P;

        /// <summary>
        /// Type d'application; Valeur par défaut à "Fureteur Web" pour l'ensemble d'ECS.
        /// </summary>
        [DataMember]
        public TypeApplication TypeApplication { get; set; } = TypeApplication.W;

        /// <summary>
        /// Type d'intervenant; Valeur par défaut à "Client ou partenaire externe" pour l'ensemble d'ECS.
        /// </summary>
        [DataMember]
        public TypeIntervenant TypeIntervenant { get; set; } = TypeIntervenant.E;

        /// <summary>
        /// Résultat; Valeur par défaut à "Acceptée" pour l'ensemble d'ECS.
        /// </summary>
        [DataMember]
        public Resultat Resultat { get; set; } = Resultat.A;

        /// <summary>
        /// Parties variables.
        /// </summary>
        [DataMember]
        public IEnumerable<PartieVariable<string>>? PartiesVariables { get; set; }

        [DataMember]
        public string? NomUrl { get; set; }

        [DataMember]
        public string? UserAgent { get; set; }

        [DataMember]
        public string? AdresseIp { get; set; }

        [DataMember]
        public string? IdSession { get; set; }

        [DataMember]
        public string? NumeroIdentifiantDossiersCourrant { get; set; }

        public ContexteCAC? ContexteCAC { get; set; }
    }
}