using FRW.PR.Extra.Models;
using FRW.PR.Extra.Services;
using FRW.PR.Extra.Utils;
using FRW.PR.Services;
using FRW.PR.Extranet.Utils.Swagger;
using FRW.TR.Commun;
using FRW.TR.Commun.Extensions;
using FRW.TR.Commun.Utils;
using FRW.TR.Contrats;
using FRW.TR.Contrats.Journalisation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using FRW.PR.Extra.Models.Components;
using FRW.TR.Contrats.Composants;
using YamlHttpClient.Utils;
using FRW.TR.Commun.Services;

namespace FRW.PR.Extra.Controllers
{

    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SoumettreController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly IFormulairesService _formulairesService;
        private readonly ISessionService _sessionService;
        private readonly IJournalisationService _journalisationService;
        private readonly IConfiguration _config;
        private readonly IContexteDevAccesseur _contexte;
        private readonly ILogger FrwLog = Serilog.Log.ForContext<SoumettreController>();
        private readonly IConfigurationApplication _configApp;

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="authService"></param>
        /// <param name="formService"></param>
        /// <param name="sessionService"></param>
        /// <param name="journalisationService"></param>
        /// <param name="configuration"></param>
        /// <param name="contexte"></param>
        /// <param name="configurationApplication"></param>
        public SoumettreController(AuthService authService, IFormulairesService formService, ISessionService sessionService, IJournalisationService journalisationService, IConfiguration configuration, IContexteDevAccesseur contexte, IConfigurationApplication configurationApplication)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _formulairesService = formService ?? throw new ArgumentNullException(nameof(formService));
            _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
            _journalisationService = journalisationService ?? throw new ArgumentNullException(nameof(journalisationService));
            _config = configuration;
            _contexte = contexte;
            _configApp = configurationApplication;
        }

        /// <summary>
        /// Créer un cookie de test
        /// </summary>
        /// <param name="nsForm">Id formulaire</param>
        /// <returns>Id publique raccourcie de la session utilisateur</returns>
        [HttpPost("{nsForm}")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreerCookie(int nsForm)
        {
            if (_config.GetValue<bool>("estProduction"))
                return NotFound();

            var formulaireBD = await _formulairesService.ObtenirDonnees(nsForm);

            var infos = await _authService.ConnecterAsync(formulaireBD.IdSystemeAutorise, formulaireBD.TypeFormulaire, nsForm);

            return Ok(infos.idSession.GetShortGuid());
        }

        /// <summary>
        /// FRW311 - Gérer la soumission d'un formulaire dynamique
        /// </summary>
        /// <param name="shortInstance">Id publique raccourcie de la session utilisateur</param>
        /// <param name="version">La version du fichier de config à utiliser</param>
        /// <param name="forcerPDF">Permet de bypasser les validations et retourner un PDF</param>
        /// <returns></returns>
        [HttpPost("{shortInstance}")]
        [JsonContent]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Transmission(string shortInstance, [FromQuery] string version, [FromQuery] bool forcerPDF)
        {
            FrwLog.Debut(nameof(Transmission), new { shortInstance, version, forcerPDF });
            var instance = shortInstance.GetGuid();
            var (idFormulaire, _) = await _authService.RecupererValiderInformationSession(instance, false) ?? throw new InvalidOperationException("Session invalide.");

            var formulaireBD = await _formulairesService.ObtenirDonnees(idFormulaire!);

            if (!idFormulaire.Equals(formulaireBD.NsFormulaire))
            {
                throw new InvalidOperationException("Numéro séquentiel de formulaire différent dans la BD.");
            }

            if (formulaireBD.NoConfirmation is { })
            {
                Log.Information($"Tentative de mise à jour d'un formulaire déjà transmis. {instance}");
                return BadRequest(new { Id = "99", Url = CultureInfoExtensions.LangueUrl + "/reprise/" });
            }

            var (Donnees, _, Systeme, Form) = await LireFormulaire(formulaireBD.ContenuFormulaire);

            await _configApp.Initialiser(formulaireBD.IdSystemeAutorise, formulaireBD.TypeFormulaire, "0");

            if (string.IsNullOrWhiteSpace(_configApp.ConfigurationFormulaire))
                return NotFound();

            DynamicForm? defaultCfg = OutilsYaml.DeserializerString<DynamicForm>(_configApp.ConfigurationGlobale!);

            var dynamicForm = OutilsYaml.DeserializerString<DynamicForm>(_configApp.ConfigurationFormulaire!);

            if (Validateur.ValiderFormulaire(Form, _configApp, dynamicForm, defaultCfg) is { } retour && !forcerPDF)
            {
                if (retour.Any())
                {
                    var erreurs = new ValidationProblemDetails(retour)
                    {
                        Type = "validation"
                    };

                    return ValidationProblem(erreurs);
                }
            }

            // Reconvertir le formulaire "épuré" et validé en JSON pour la sauvegarde
            var formulaireNettoye = JsonConvert.SerializeObject(Donnees);

            if (forcerPDF)
            {
                // Protection pour la PROD
                if (!(_config.GetValue<bool>("FRW:AfficherInfosDebug") || bool.Parse(HttpContext.Request.Headers["X-DEBUG"].FirstOrDefault() ?? "false")))
                {
                    return BadRequest();
                }

                var pdfs = await _formulairesService.ProduireDocument(idFormulaire, formulaireNettoye);

                return Ok(pdfs);
            }
            else
            {
                if (await TraitementConfirmerTransmission(idFormulaire, formulaireBD.IdSystemeAutorise, formulaireBD.TypeFormulaire, version!, formulaireNettoye) is { } noConfirmation)
                {
                    // Journalisation SIG
                    var partieVariable = new
                    {
                        NumeroSequentielFormulaire = idFormulaire,
                        AdresseCourriel = GestionContenu.ObtenirCourriel(Systeme),
                        // TODO: En attente de la décision de sécurité
                        // ContenuFormulaire = formulaireNettoye,
                        NumeroConfirmation = noConfirmation,
                    };
                    _journalisationService.JournaliserSIG(CodeOptionTransaction.CRE, "FRW21102", "Transmission du formulaire", partieVariable, idFormulaire, null);

                    //On fait le ménage + retourne OK juste si tout a fonctionné
                    await _authService.FermerFormulaireAsync(instance);
                    FrwLog.Fin(nameof(Transmission), "ok");
                    return Ok(noConfirmation);
                }
                else
                {
                    FrwLog.Fin(nameof(Transmission), "Problem");
                    return Problem(statusCode: 500);
                }
            }
        }

        private async Task<long?> TraitementConfirmerTransmission(int idFormulaire, int idSystemeAutorise, string typeFormulaire, string version, string json)
        {
            try
            {
                var noConfirmation = await _formulairesService.ObtenirNumeroConfirmation();

                await MiseAJour(null, idFormulaire, idSystemeAutorise, typeFormulaire, version, json, noConfirmation);

                try
                {
                    await _formulairesService.DiffererOrchestrerProductionFormulaire((int)idFormulaire!, noConfirmation, null, null);
                }
                catch (Exception)
                {
                    // Retirer le no. de confirmation
                    await MiseAJour(null, idFormulaire, idSystemeAutorise, typeFormulaire, version, json, null);
                    throw;
                }

                return noConfirmation;
            }
            catch (Exception ex)
            {
                FrwLog.Error(ex, "Impossible de compléter la soumission du formulaire.");
                return null;
            }
        }

        /// <summary>
        /// Gérer la mise à jour de l'entrée dans la table des formulaires aux changements de section et au clic "Enregistrer"
        /// </summary>
        /// <param name="typeFormulaire">ex: 3003</param>
        /// <param name="idSystemeAutorise">ex: 1</param>
        /// <param name="version">ex: 0</param>
        /// <param name="shortInstance">ex: i3-AbG0-5UeQwX2MYv5E6w</param>
        /// <param name="estActionUtilisateur"></param>
        /// <param name="estMiseAJourExpiration"></param>
        /// <returns></returns>
        [HttpPost("{idSystemeAutorise}/{typeFormulaire}/{version}/{shortInstance}")]
        [JsonContent]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> MiseAJour(int idSystemeAutorise, string typeFormulaire, string version, string shortInstance, [FromQuery] bool? estActionUtilisateur, [FromQuery] bool? estMiseAJourExpiration)
        {
            FrwLog.Debut(nameof(MiseAJour), new { idSystemeAutorise, typeFormulaire, version, shortInstance, estActionUtilisateur, estMiseAJourExpiration });
            var instance = shortInstance.GetGuid();
            var (_, Json, Systeme, _) = await LireFormulaire();
            SortantCreerSession? sessionCourriel = null;

            var infoSession = await _authService.RecupererValiderInformationSession(instance, (bool)estMiseAJourExpiration!);

            if (infoSession is null)
            {
                Log.Information($"Tentative de mise à jour du formulaire sans autorisation. {instance}");
                return BadRequest(new { Id = "100" });
            }

            var formulaireBD = await _formulairesService.ObtenirDonnees((int)infoSession?.idFormulaire!);

            if (formulaireBD.NoConfirmation is { })
            {
                Log.Information($"Tentative de mise à jour d'un formulaire déjà transmis. {instance}");
                return BadRequest(new { Id = "99", Url = CultureInfoExtensions.LangueUrl + "/reprise/" });
            }

            var envoyerCourriel = DoitEnvoyerCourriel(Systeme);
            if (envoyerCourriel)
            {
                sessionCourriel = await _sessionService.Creer((int)infoSession?.idFormulaire!, Constantes.SessionCourriel);
            }

            await _formulairesService.Maj((int)infoSession?.idFormulaire!, envoyerCourriel, typeFormulaire, version, Json!, sessionCourriel?.NoPublicSession, idSystemeAutorise, Systeme);

            // Journaliser seulement lors du clic sur le bouton Enregistrer et non à chaque changement de section
            if (estActionUtilisateur ?? false)
            {
                // Journaliser les informations dans SIG
                var partieVariable = new
                {
                    NoSequentiel = infoSession?.idFormulaire,
                    AdresseCourriel = GestionContenu.ObtenirCourriel(Systeme),
                };
                _journalisationService.JournaliserSIG(CodeOptionTransaction.CRE, "FRW21101", "Enregistrement du formulaire", partieVariable, infoSession?.idFormulaire!, sessionCourriel?.NoPublicSession);
            }
            FrwLog.Fin(nameof(MiseAJour), new { ExpirationClient = infoSession?.expiration.ToExpirationClient() });
            return Ok(infoSession?.expiration.ToExpirationClient());
        }

        /// <summary>
        /// Gérer la mise à jour de l'entrée dans la table des formulaires aux changements de section et au clic "Enregistrer"
        /// </summary>
        /// <param name="typeFormulaire">ex: 3003</param>
        /// <param name="idSystemeAutorise">ex: 1</param>
        /// <param name="version">ex: 0</param>
        /// <returns></returns>
        [HttpPost("{idSystemeAutorise}/{typeFormulaire}/{version}")]
        [JsonContent]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ObtenirChampsObsoletes(int idSystemeAutorise, string typeFormulaire, string version)
        {
            await _configApp.Initialiser();

            var (_, _, _, Form) = await LireFormulaire();

            if (string.IsNullOrWhiteSpace(_configApp.ConfigurationFormulaire))
                return NotFound();

            DynamicForm? defaultCfg = OutilsYaml.DeserializerString<DynamicForm>(_configApp.ConfigurationGlobale!);

            var dynamicForm = OutilsYaml.DeserializerString<DynamicForm>(_configApp.ConfigurationFormulaire);

            if (dynamicForm is { })
            {
                var inputs = new List<ComponentValidation>();
                var formData = new List<ComponentBinding>();
                var compteurDoublons = new Dictionary<string, int>();

                var jsonAvant = JsonConvert.SerializeObject(Form);
                var jsonFlatAvant = JsonHelper.DeserializeAndFlatten(jsonAvant ?? string.Empty, false, ".", ".{0}") as Dictionary<string, object>;

                Validateur.ObtenirChampsEffectifsSelonConfiguration(Form, _configApp, dynamicForm, defaultCfg, ref inputs, ref formData, ref compteurDoublons);

                var jsonApres = JsonConvert.SerializeObject(Form);
                var jsonFlatApres = JsonHelper.DeserializeAndFlatten(jsonApres ?? string.Empty, false, ".", ".{0}") as Dictionary<string, object>;

                return Ok(jsonFlatAvant.Keys.Except(jsonFlatApres.Keys).ToArray());
            }

            //Fake pour tester 
            //return Ok(new string[] { "HabiteAvecAutreAdulte", "RevenusEmploi.0.RevenuEmploiSourceNomEmployeur" });

            return Ok(Array.Empty<string>());
        }


        /// <summary>
        /// Gérer la mise à jour de l'entrée dans la table des formulaires aux changements de section et au clic "Enregistrer"
        /// </summary>
        /// <param name="shortInstance">ex: i3-AbG0-5UeQwX2MYv5E6w</param>
        /// <returns></returns>
        [HttpPost("{shortInstance}")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Verifier(string shortInstance)
        {
            FrwLog.Debut(nameof(Verifier), shortInstance);
            var instance = shortInstance.GetGuid();

            var infoSession = await _authService.RecupererValiderInformationSession(instance, false);

            if (infoSession is null)
            {
                FrwLog.Debug("Tentative de mise à jour du formulaire sans autorisation. {Instance}", instance);
                return BadRequest(new { Id = "100" });
            }
            FrwLog.Fin(nameof(Verifier));
            return Ok();
        }

        /// <summary>
        /// Deconnecter la session de l'utilisateur si celle-ci est expirer
        /// </summary>
        /// <param name="shortInstance">ex: i3-AbG0-5UeQwX2MYv5E6w</param>
        /// <param name="entrantDeconnexion"></param>
        /// <returns></returns>
        [HttpPost("{shortInstance}")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeconnecterSessionUtilisateurAsync(string shortInstance, [FromBody] EntrantDeconnexion entrantDeconnexion)
        {
            FrwLog.Debut(nameof(DeconnecterSessionUtilisateurAsync), shortInstance);
            var instance = shortInstance.GetGuidOrNull();

            if (instance != null)
            {
                try
                {
                    FrwLog.Information(nameof(DeconnecterSessionUtilisateurAsync), "ok");
                    return Ok("ok");
                }
                finally
                {
                    FrwLog.Fin(nameof(DeconnecterSessionUtilisateurAsync), "Response.OnCompleted");
                    // Permet de traiter du code avant que le contenu HTTP se dispose, et sans faire attendre l'appelant pour un retour
                    Response.OnCompleted(async () =>
                    {
                        await DeconnecterSessionUtilisateurInterneAsync((Guid)instance, entrantDeconnexion);
                    });
                }
            }
            else
            {
                FrwLog.Information($"Tentative de suppresion de la session de l'utilisateur a echouer.");

                // Nécessaire pour laisser la fonction "async"
                await Task.Delay(TimeSpan.FromMilliseconds(1));
                FrwLog.Fin(nameof(DeconnecterSessionUtilisateurAsync), "BadRequest");
                return BadRequest();
            }
        }

        private async Task DeconnecterSessionUtilisateurInterneAsync(Guid instance, EntrantDeconnexion entrantDeconnexion)
        {
            // On attends un peu
            await Task.Delay(TimeSpan.FromSeconds(10));

            // On réobtient la session encore
            var sessionApres = await _sessionService.Obtenir(instance);

            // On vérifie si l'expiration a changée, si inchangée, on consomme la session
            if (((DateTime)sessionApres.DateExpiration!).ToExpirationClient().Equals(entrantDeconnexion.Expiration))
                await _sessionService.Consommer(instance);
        }

        /// <summary>
        /// Gérer la mise à jour de l'entrée dans la table des formulaires aux changements de section et au clic "Enregistrer"
        /// </summary>
        /// <param name="noSession"></param>
        /// <param name="idFormulaire"></param>
        /// <param name="idSystemeAutorise"></param>
        /// <param name="typeFormulaire"></param>
        /// <param name="version"></param>
        /// <param name="contenu"></param>
        /// <param name="noConfirmation"></param>
        /// <returns></returns>
        private async Task MiseAJour(Guid? noSession, int idFormulaire, int idSystemeAutorise, string typeFormulaire, string version, string contenu, long? noConfirmation = null)
        {
            await _formulairesService.Maj(
                (int)idFormulaire!,
                false,
                typeFormulaire,
                version,
                contenu,
                noSession,
                idSystemeAutorise,
                null,
                noConfirmation);
        }

        private static bool DoitEnvoyerCourriel(IDictionary<object, object>? systeme)
        {
            if (systeme is null) return false;

            if (systeme.TryGetValue("infosEnregistrement", out var infos) && infos is IDictionary<object, object> infosDict)
            {
                if (infosDict.TryGetValue("courrielEnvoye", out var courrielEnvoye) && courrielEnvoye is bool courrielDejaEnvoye)
                {
                    if (!courrielDejaEnvoye)
                    {
                        if (infosDict.TryGetValue("courriel", out var courriel) && courriel is string courrielStr
                            && infosDict.TryGetValue("motPasse", out var motPasse) && motPasse is string motPasseStr)
                        {
                            if (!string.IsNullOrWhiteSpace(courrielStr) && !string.IsNullOrWhiteSpace(motPasseStr))
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        private async Task<(IDictionary<object, object> Donnees,
            string? Json,
            IDictionary<object, object>? Systeme,
            IDictionary<object, object> Form)>
            LireFormulaire(string? jsonDataFromUserBD = null)
        {
            string? jsonDataFromUser = null;

            using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                jsonDataFromUser = await reader.ReadToEndAsync();
            }

            // Pour le debug
            if (jsonDataFromUser is null || jsonDataFromUser is { } && jsonDataFromUser.Equals("string"))
            {
                jsonDataFromUser = jsonDataFromUserBD ?? string.Empty;
            }

            var objFRW = JsonConvert.DeserializeObject<IDictionary<object, object>>(
                                    jsonDataFromUser ?? string.Empty,
                                    new JsonSerializerSettings()
                                    {
                                        Converters = new JsonConverter[] {
                                                new ConvertisseurFRW() }
                                    });

            if (objFRW is null) { throw new Exception("No data received."); }

            IDictionary<object, object>? systeme = null;
            IDictionary<object, object>? form = null;

            if (objFRW["systeme"] != null)
            {
                systeme = (IDictionary<object, object>)objFRW["systeme"];
            }

            if (objFRW["form"] != null)
            {
                form = (IDictionary<object, object>)objFRW["form"];
            }
            return (Donnees: objFRW, Json: jsonDataFromUser, Systeme: systeme, Form: form!);
        }
    }
}
