using FRW.TR.Contrats;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace FRW.PR.Extra.Services
{
    /// <summary>
    /// Expose les services Formulaire
    /// </summary>
    public class FormulairesService : IFormulairesService
    {
        private readonly IDorsale _dorsale;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _config;

        private const string _serviceFormulaires = "FRW.SV.GestionFormulaires";
        private const string _apiPathForm = "/api/v1/Formulaires";
        private const string _apiPathGabarit = "/api/v1/Gabarit";

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="dorsale"></param>
        /// <param name="config"></param>
        /// <param name="memoryCache"></param>
        public FormulairesService(IDorsale dorsale, IConfiguration config, IMemoryCache memoryCache)
        {
            _dorsale = dorsale;
            _config = config;
            _cache = memoryCache;
        }

        /// <summary>
        /// Obtenir la configuration
        /// </summary>
        /// <param name="idSystemeAutorise"></param>
        /// <param name="typeFormulaire"></param>
        /// <param name="niveau"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public async Task<string?> ObtenirContenuFichierConfiguration(int? idSystemeAutorise, string? typeFormulaire, string? version, string niveau)
        {
            idSystemeAutorise ??= 0;
            typeFormulaire ??= "default";

            var config = await _cache.GetOrCreateAsync($"obtenirconfig-{idSystemeAutorise}-{typeFormulaire}-{version}-{niveau}", entry =>
                        {
                            int secondesExpiration = _config.GetValue<int>("FRW:DureeCacheGeneral");

                            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(secondesExpiration);
                            return _dorsale.Recevoir<string?>(_serviceFormulaires, $"{_apiPathForm}/ObtenirConfiguration/form/{idSystemeAutorise}/{typeFormulaire ?? default}/{version ?? "0"}/{niveau}");
                        });

            return config;
        }

        /// <summary>
        /// Obtenir les binding
        /// </summary>
        /// <param name="idSystemeAutorise"></param>
        /// <param name="typeFormulaire"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public async Task<string?> ObtenirContenuFichierBinding(int idSystemeAutorise, string typeFormulaire, string version = "0")
        {
            return await _dorsale.Recevoir<string?>(_serviceFormulaires, $"{_apiPathForm}/ObtenirConfiguration/bind/{idSystemeAutorise}/{typeFormulaire}/{version}/FORMULAIRE");
        }

        /// <summary>
        /// Obtenir un fichier gabarit
        /// </summary>
        /// <param name="idSystemeAutorise"></param>
        /// <param name="typeFormulaire"></param>
        /// <param name="nomFichier"></param>
        /// <returns></returns>
        public async Task<byte[]?> ObtenirContenuFichierGabarit(int idSystemeAutorise, string typeFormulaire, string nomFichier)
        {
            return await _dorsale.Recevoir<byte[]?>(_serviceFormulaires, $"{_apiPathForm}/ObtenirGabarit/{idSystemeAutorise}/{typeFormulaire}/{nomFichier}?extension=.pdf");
        }

        /// <summary>
        /// Créer un formulaire
        /// </summary>
        /// <param name="idSystemeAutorise"></param>
        /// <param name="nomForm"></param>
        /// <param name="noPublicForm"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<int> Creer(int idSystemeAutorise, string nomForm, Guid noPublicForm, string data)
        {
            var a = await _dorsale.EnvoyerRecevoir<EntrantCreerFormulaire, int>(HttpMethod.Post,
                new EntrantCreerFormulaire
                {
                    NoPublicForm = noPublicForm,
                    TypeFormulaire = nomForm,
                    IdSystemeAutorise = idSystemeAutorise,
                    JsonData = data
                },
                _serviceFormulaires, $"{_apiPathForm}/Creer");

            return a;
        }

        /// <summary>
        /// Mettre à jour un formulaire
        /// </summary>
        /// <param name="nsFormulaire"></param>
        /// <param name="envoyerCourriel"></param>
        /// <param name="typeFormulaire"></param>
        /// <param name="versionConfig"></param>
        /// <param name="data"></param>
        /// <param name="noPublicSession"></param>
        /// <param name="noSystemeAutorise"></param>
        /// <param name="systeme"></param>
        /// <param name="noConfirmation"></param>
        /// <returns></returns>
        public async Task Maj(int nsFormulaire, bool envoyerCourriel, string typeFormulaire, string versionConfig, string data, Guid? noPublicSession, int noSystemeAutorise, IDictionary<object, object>? systeme, long? noConfirmation = null)
        {
            string langue = CultureInfoExtensions.LangueUtilisateur;

            await _dorsale.EnvoyerRecevoir<EntrantMajFormulaire, int>(HttpMethod.Post,
                new EntrantMajFormulaire
                {
                    NsFormulaire = nsFormulaire,
                    EnvoyerCourriel = envoyerCourriel,
                    JsonData = data,
                    Systeme = systeme,
                    TypeFormulaire = typeFormulaire,
                    VersionConfig = versionConfig,
                    NoSystemeAutorise = (short)noSystemeAutorise,
                    NoPublicSession = noPublicSession,
                    NoConfirmation = noConfirmation
                }, _serviceFormulaires, $"{_apiPathForm}/Maj?langue={langue}");
        }

        /// <summary>
        /// Valider la reprise d'un formulaire
        /// </summary>
        /// <param name="noPublicSessionClair"></param>
        /// <param name="noPublicSessionCrypte"></param>
        /// <param name="motDePasse"></param>
        /// <param name="nsFormulaire"></param>
        /// <returns></returns>
        public async Task<bool> ValiderReprise(Guid noPublicSessionClair, string noPublicSessionCrypte, string motDePasse, int nsFormulaire)
        {
            var entrant = new EntrantValiderReprise
            {
                NoPublicSessionCrypte = noPublicSessionCrypte,
                NoPublicSessionClair = noPublicSessionClair,
                MotDePasse = motDePasse,
                NsFormulaire = nsFormulaire
            };

            return await _dorsale.EnvoyerRecevoir<EntrantValiderReprise, bool>(HttpMethod.Post, entrant, _serviceFormulaires, $"{_apiPathForm}/ValiderReprise");
        }

        /// <summary>
        /// FRW1-010 - Obtenir un numéro de confirmation
        /// </summary>
        /// <returns>Numero de Confirmation</returns>
        public async Task<long> ObtenirNumeroConfirmation()
        {
            return await _dorsale.Recevoir<long>(_serviceFormulaires, $"{_apiPathForm}/ObtenirNumeroConfirmation/");
        }

        /// <summary>
        /// Orchestrer Production Formulaire
        /// </summary>
        /// <param name="nsFormulaire"></param>
        /// <param name="noConfirmation"></param>
        /// <param name="dateExecution"></param>
        /// <param name="nombreConvois"></param>
        /// <returns></returns>
        public async Task<bool> DiffererOrchestrerProductionFormulaire(int nsFormulaire, long noConfirmation, DateTime? dateExecution, int? nombreConvois)
        {
            string langue = CultureInfoExtensions.LangueUtilisateur;

            var entrant = new EntrantDiffererOrchestrer()
            { DateExecution = dateExecution };

            return await _dorsale.EnvoyerRecevoir<EntrantDiffererOrchestrer, bool>(HttpMethod.Post, entrant, _serviceFormulaires, $"{_apiPathForm}/DiffererOrchestrerProductionFormulaire/{nsFormulaire}/{noConfirmation}?langue={langue}&nombreConvois={nombreConvois ?? 0}");
        }

        /// <summary>
        /// Obtenir les données d'un formulaire
        /// </summary>
        /// <param name="nsFormulaire"></param>
        /// <returns></returns>
        public async Task<DonneesFormulaire> ObtenirDonnees(int nsFormulaire)
        {
            var a = await _dorsale.Recevoir<DonneesFormulaire>(_serviceFormulaires, $"{_apiPathForm}/ObtenirDonnees/{nsFormulaire}");

            return a;
        }

        /// <summary>
        /// Produire le document en sortie
        /// </summary>
        /// <param name="idFormulaire"></param>
        /// <param name="jsonData"></param>
        /// <returns></returns>
        public async Task<RetourFusionnerDonnees[]?> ProduireDocument(int idFormulaire, string jsonData)
        {
            string langue = CultureInfoExtensions.LangueUtilisateur;

            var a = await _dorsale.EnvoyerRecevoir<string, RetourFusionnerDonnees[]>(HttpMethod.Post,
                                                            jsonData,
                                                            _serviceFormulaires,
                                                            $"{_apiPathGabarit}/SimulerOrchestrer/{idFormulaire}?langue={langue}");

            return a;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="idSystemeAutorise"></param>
        /// <param name="typeFormulaire"></param>
        /// <param name="identifiantUtilisateur"></param>
        /// <param name="jsonData"></param>
        /// <returns></returns>
        public async Task<RetourCreerReprendreFormulaireIndividu> CreerFormulaireIndividu(int idSystemeAutorise, string typeFormulaire, string? identifiantUtilisateur, string? jsonData)
        {
            var a = await _dorsale.EnvoyerRecevoir<EntrantCreerFormulaireIndividu, RetourCreerReprendreFormulaireIndividu>(
                                                                HttpMethod.Post,
                                                                new EntrantCreerFormulaireIndividu
                                                                {
                                                                    IdentifiantUtilisateur = identifiantUtilisateur,
                                                                    JsonData = jsonData,
                                                                    TypeFormulaire = typeFormulaire,
                                                                    IdSystemeAutorise = idSystemeAutorise
                                                                },
                                                                _serviceFormulaires,
                                                                $"{_apiPathForm}/CreerFormulaireIndividu");

            return a;
        }

        /// <summary>
        /// RechercherFormulaires
        /// </summary>
        /// <param name="idSystemeAutorise"></param>
        /// <param name="identifiantUtilisateur"></param>
        /// <param name="entrant"></param>
        /// <returns></returns>
        public async Task<IEnumerable<RetourRechercherFormulaires>> RechercherFormulaires(int idSystemeAutorise, string identifiantUtilisateur, EntrantRechercherFormulairesSIS? entrant)
        {
            var entrantRechercherFormulaires = new EntrantRechercherFormulaires
            {
                IdSystemeAutorise = idSystemeAutorise,
                IdentifiantUtilisateur = identifiantUtilisateur,
            };

            if (entrant != null)
            {
                entrantRechercherFormulaires.EtatsFormulaireRecherche = entrant.EtatsFormulaireRecherche;
                entrantRechercherFormulaires.CodeTypeFormulaire = entrant.CodeTypeFormulaire;
                entrantRechercherFormulaires.NoConfirmation = entrant.NoConfirmation;
                entrantRechercherFormulaires.NoPublicFormulaire = entrant.NoPublicFormulaire;
            }

            var a = await _dorsale.EnvoyerRecevoir<EntrantRechercherFormulaires, IEnumerable<RetourRechercherFormulaires>>(
                                                                HttpMethod.Post,
                                                                entrantRechercherFormulaires,
                                                                _serviceFormulaires,
                                                                $"{_apiPathForm}/RechercherFormulaires");
            return a;
        }

        /// <summary>
        /// ObtenirIdentifiantSessionFormulaire
        /// </summary>
        /// <param name="idSystemeAutorise"></param>
        /// <param name="identifiantUtilisateur"></param>
        /// <param name="noFormulairePublic"></param>
        /// <param name="jsonData"></param>
        /// <returns></returns>
        public async Task<RetourCreerReprendreFormulaireIndividu> ObtenirIdentifiantSessionFormulaire(int idSystemeAutorise, string identifiantUtilisateur, string noFormulairePublic, string? jsonData)
        {
            var entrant = new EntrantObtenirIdentifiantSessionFormulaire
            {
                NoPublicFormulaire = noFormulairePublic,
                IdentifiantUtilisateur = identifiantUtilisateur,
                IdSystemeAutorise = idSystemeAutorise,
                JsonData = jsonData
            };

            var a = await _dorsale.EnvoyerRecevoir<EntrantObtenirIdentifiantSessionFormulaire, RetourCreerReprendreFormulaireIndividu>(
                                                               HttpMethod.Post,
                                                               entrant,
                                                               _serviceFormulaires,
                                                               $"{_apiPathForm}/ObtenirIdentifiantSessionFormulaire");

            return a;
        }

        public async Task<IEnumerable<InfoFormulaire>> ObtenirListeFormulaires(int idSystemeAutorise)
        {
            var a = await _dorsale.Recevoir<List<InfoFormulaire>>(
                _serviceFormulaires,
                $"{_apiPathForm}/ObtenirListeFormulaires?idSystemeAutorise={idSystemeAutorise}");

            return a;
        }

        /// <summary>
        /// SupprimerFormulaire
        /// </summary>
        /// <param name="idSystemeAutorise"></param>
        /// <param name="identifiantUtilisateur"></param>
        /// <param name="noFormulairePublic"></param>
        /// <returns></returns>
        public async Task SupprimerFormulaire(int idSystemeAutorise, string identifiantUtilisateur, string noFormulairePublic)
        {
            var entrant = new EntrantSupprimerFormulaire
            {
                IdentifiantUtilisateur = identifiantUtilisateur,
                IdSystemeAutorise = idSystemeAutorise,
                NoPublicFormulaire = noFormulairePublic
            };
            await _dorsale.Envoyer(entrant, _serviceFormulaires,
                                                  $"{_apiPathForm}/SupprimerFormulaire");
        }

        /// <summary>
        /// Permet de déployer les formulaires d'un système
        /// </summary>
        /// <param name="idSystemeAutorise"></param>
        /// <param name="contenuZip"></param>
        /// <returns></returns>
        public async Task DeployerSysteme(int idSystemeAutorise, byte[] contenuZip)
        {
            var entrant = new EntrantDeployerFormulaire
            {
                IdSystemeAutorise = idSystemeAutorise,
                ContenuZip = contenuZip
            };
            await _dorsale.EnvoyerRecevoir<EntrantDeployerFormulaire, Rien>(HttpMethod.Put, entrant, _serviceFormulaires,
                                                  $"{_apiPathForm}/DeployerSysteme");
        }

        /// <summary>
        /// Obtenir un Schema
        /// </summary>
        /// <param name="type">form seulement pour le moment</param>
        /// <returns></returns>
        public async Task<byte[]?> ObtenirSchema(string type)
        {
            return await _dorsale.Recevoir<byte[]>(_serviceFormulaires,
                                                   $"{_apiPathForm}/ObtenirSchema/{type}");
        }
    }
}
