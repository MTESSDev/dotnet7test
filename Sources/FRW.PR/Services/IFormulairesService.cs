using FRW.TR.Contrats;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FRW.PR.Extra.Services
{
    public interface IFormulairesService
    {
        Task<string?> ObtenirContenuFichierConfiguration(int? idSystemeAutorise, string? typeFormulaire, string? version, string niveau);

        Task<string?> ObtenirContenuFichierBinding(int idSystemeAutorise, string typeFormulaire, string version = "0");

        Task<byte[]?> ObtenirContenuFichierGabarit(int idSystemeAutorise, string typeFormulaire, string nomFichier);

        Task<int> Creer(int idSystemeAutorise, string nomForm, Guid noPublicForm, string data);

        Task Maj(int nsFormulaire, bool envoyerCourriel, string typeFormulaire, string versionConfig, string data, Guid? noPublicSession, int noSystemeAutorise, IDictionary<object, object>? systeme, long? noConfirmation = null);

        Task<bool> ValiderReprise(Guid noPublicSessionClair, string noPublicSessionCrypte, string motDePasse, int nsFormulaire);

        Task<long> ObtenirNumeroConfirmation();

        Task<bool> DiffererOrchestrerProductionFormulaire(int nsFormulaire, long noConfirmation, DateTime? dateExecution, int? nbConvois);

        Task<DonneesFormulaire> ObtenirDonnees(int nsFormulaire);

        Task<RetourFusionnerDonnees[]?> ProduireDocument(int idFormulaire, string jsonData);

        Task<RetourCreerReprendreFormulaireIndividu> CreerFormulaireIndividu(int idSystemeAutorise, string typeFormulaire, string? identifiantUtilisateur, string? jsonData);

        Task<IEnumerable<RetourRechercherFormulaires>> RechercherFormulaires(int idSystemeAutorise, string identifiantUtilisateur, EntrantRechercherFormulairesSIS? etatsFormulaireRecherche);

        Task<RetourCreerReprendreFormulaireIndividu> ObtenirIdentifiantSessionFormulaire(int idSystemeAutorise, string identifiantUtilisateur, string noFormulairePublic, string? jsonData);

        Task<IEnumerable<InfoFormulaire>> ObtenirListeFormulaires(int idSystemeAutorise);

        Task SupprimerFormulaire(int idSystemeAutorise, string identifiantUtilisateur, string noFormulairePublic);

        Task DeployerSysteme(int systemeAutoriseId, byte[] contenuZip);

        Task<byte[]?> ObtenirSchema(string type);
    }
}