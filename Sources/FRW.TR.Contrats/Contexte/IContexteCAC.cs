using System;
using System.Collections.Generic;
namespace FRW.TR.Contrats.Contexte
{
    public interface IContexteCAC
    {
        string CodeSysteme { get; set; }
        string CodeUtilisateur { get; set; }
        string CodeUtilisateurComplet { get; set; }
        string CodeUtilisateurCompletUPN { get; set; }
        string Domaine { get; set; }
        string DomaineUPN { get; set; }
        string Phase { get; set; }
        string Environnement { get; set; }
        string Palier { get; set; }
        DateTime DateProduction { get; }
        DateTime DateHeureProduction { get; }
        string DateReference { get; set; }
        string Poste { get; set; }
        bool EstProduction { get; set; }
        Dictionary<string, string>? SurchargesVariablesProfil { get; set; }

        /// <summary>
        /// Convertis le contexte interne en vrai contexte pour les appels externes, entres autres.
        /// </summary>
        /// <returns>CAC.GestionContexte.Contexte</returns>
        CAC.GestionContexte.Contexte ToContexte();

        /// <summary>
        /// Convertis le contexte interne en vrai contexte pour les appels aux services du Central.
        /// </summary>
        /// <returns>CAC.GestionContexte.Contexte</returns>
        CAC.GestionContexte.Contexte ToContextePourAppelCentral();
    }
}
