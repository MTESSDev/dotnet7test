using FRW.PR.Extra.Services;
using FRW.TR.Commun.Extensions;
using FRW.TR.Contrats;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FRW.PR.Extra.Controllers
{
    /// <summary>
    /// Service intersystème
    /// </summary>
    [Route("/api/v1/[controller]/[action]")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ApiController]
    public class SISController : Controller
    {
        private readonly IFormulairesService _formulairesService;
        private readonly ISystemeAutoriseService _systemeAutoriseService;
        
        /// <summary>
        /// Clé Api
        /// </summary>
        [FromHeader(Name = "X-ApiKey")]
        public string? ApiKey { get; set; }

        
        /// <summary>
        ///  Numéro public du système autorisé
        /// </summary>
        [FromHeader(Name = "X-NoPublicSystemeAutorise")]
        public string? NoPublicSystemeAutorise { get; set; }

        /// <summary>
        /// Identifiant de l'utilisateur
        /// </summary>
        [FromHeader(Name = "X-IdentifiantUtilisateur")]
        public string? IdentifiantUtilisateur { get; set; }


        private readonly ILogger FrwLog = Serilog.Log.ForContext<SoumettreController>();

        /// <summary />
        public SISController(IFormulairesService formulairesService, ISystemeAutoriseService systemeAutoriseService)
        {
            _formulairesService = formulairesService;
            _systemeAutoriseService = systemeAutoriseService;
        }

        /// <summary>
        /// FRW111 - Créer un formulaire Web pour un individu.
        /// </summary>
        /// <param name="typeFormulaire"></param>
        /// <param name="jsonData"></param>
        /// <returns></returns>
        [HttpPost("{typeFormulaire}")]
        public async Task<ActionResult<RetourCreerReprendreFormulaireIndividu>> CreerFormulaireIndividu(
            string typeFormulaire,
            [FromBody] object jsonData)
        {

            if (string.IsNullOrEmpty(IdentifiantUtilisateur)) throw new ArgumentNullException(nameof(IdentifiantUtilisateur));
            FrwLog.Debut(nameof(CreerFormulaireIndividu), new { NoPublicSystemeAutorise, typeFormulaire, IdentifiantUtilisateur });

            var systemeAutoriseId = await _systemeAutoriseService.ValiderSystemeApiKey(NoPublicSystemeAutorise.GetGuidOrNull(), ApiKey);
            if (systemeAutoriseId == 0)
            {
                FrwLog.Fin(nameof(CreerFormulaireIndividu));
                return ValidationProblem("Systeme non autorise");
            }

            string? json = jsonData is null ? "{}" : jsonData.ToString();

            var retour = await _formulairesService.CreerFormulaireIndividu(systemeAutoriseId, typeFormulaire, IdentifiantUtilisateur, json);
            FrwLog.Fin(nameof(CreerFormulaireIndividu), retour);
            return Ok(retour);

        }

        /// <summary>
        /// FRW112 - obtenir les formulaire web d'un individu
        /// </summary>
        /// <param name="entrant"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<IEnumerable<RetourRechercherFormulaires>>> ObtenirFormulairesIndividu(
            [FromBody] EntrantRechercherFormulairesSIS? entrant)
        {
            if (string.IsNullOrEmpty(IdentifiantUtilisateur)) throw new ArgumentNullException(nameof(IdentifiantUtilisateur));
            FrwLog.Debut(nameof(ObtenirFormulairesIndividu), null);

            var systemeAutoriseId = await _systemeAutoriseService.ValiderSystemeApiKey(NoPublicSystemeAutorise.GetGuidOrNull(), ApiKey);
            if (systemeAutoriseId == 0)
            {
                FrwLog.Fin(nameof(ObtenirFormulairesIndividu));
                return ValidationProblem("Systeme non autorise");
            }
           
            var retour = await _formulairesService.RechercherFormulaires(systemeAutoriseId, IdentifiantUtilisateur, entrant);
            FrwLog.Fin(nameof(ObtenirFormulairesIndividu), retour);
            return Ok(retour);

        }


        /// <summary>
        /// FRW113 - Obtenir un identifiant de session pour un formulaire
        /// </summary>
        /// <param name="noFormulairePublic"></param>
        /// <param name="jsonData"></param>
        /// <returns></returns>
        [HttpPost("{noFormulairePublic}")]
        public async Task<ActionResult<RetourCreerReprendreFormulaireIndividu>> ObtenirIdentifiantSessionFormulaire(
            string noFormulairePublic,
            [FromBody] object? jsonData)
        {
            if (string.IsNullOrEmpty(IdentifiantUtilisateur)) throw new ArgumentNullException(nameof(IdentifiantUtilisateur));
            FrwLog.Debut(nameof(ObtenirIdentifiantSessionFormulaire), noFormulairePublic);

            var systemeAutoriseId = await _systemeAutoriseService.ValiderSystemeApiKey(NoPublicSystemeAutorise.GetGuidOrNull(), ApiKey);
            if (systemeAutoriseId == 0)
            {
                FrwLog.Fin(nameof(ObtenirIdentifiantSessionFormulaire));
                return ValidationProblem("Systeme non autorise");
            }

            string? json = jsonData != null ? jsonData.ToString() : "{}" ;

            var retour = await _formulairesService.ObtenirIdentifiantSessionFormulaire(systemeAutoriseId, IdentifiantUtilisateur, noFormulairePublic, json);

            FrwLog.Fin(nameof(ObtenirIdentifiantSessionFormulaire), retour);
            return Ok(retour);
            
        }

        /// <summary>
        /// FRW114 - Supprimer un formulaire web
        /// </summary>
        /// <param name="noFormulairePublic"></param>
        /// <returns></returns>
        [HttpGet("{noFormulairePublic}")]
        public async Task<ActionResult> SupprimerFormulaire(string noFormulairePublic)
        {
            if (string.IsNullOrEmpty(IdentifiantUtilisateur)) throw new ArgumentNullException(nameof(IdentifiantUtilisateur));
            FrwLog.Debut(nameof(SupprimerFormulaire), noFormulairePublic);

            var systemeAutoriseId = await _systemeAutoriseService.ValiderSystemeApiKey(NoPublicSystemeAutorise.GetGuidOrNull(), ApiKey);
            if (systemeAutoriseId == 0)
            {
                FrwLog.Fin(nameof(SupprimerFormulaire));
                return ValidationProblem("Systeme non autorise");
            }

            await _formulairesService.SupprimerFormulaire(systemeAutoriseId, IdentifiantUtilisateur, noFormulairePublic);
            FrwLog.Fin(nameof(SupprimerFormulaire));
            return Ok();
        }


        /// <summary>
        /// Obtenir un schema
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpGet("{type}")]
        public async Task<ActionResult> ObtenirSchema(string type)
        {
            FrwLog.Debut(nameof(SupprimerFormulaire), type);

            var schema = await _formulairesService.ObtenirSchema(type);

            FrwLog.Fin(nameof(SupprimerFormulaire));

             return File(schema ?? Array.Empty<byte>(), "application/json; charset=utf-8");
        }

        /// <summary>
        /// FRWxxx - Déployer un système au complet
        /// </summary>
        /// <returns></returns>
        [HttpPost()]
        [Consumes("application/json")]
        public async Task<ActionResult> DeployerSysteme(EntrantDeployerSysteme entrant)
        {
            FrwLog.Debut(nameof(DeployerSysteme), null);

            var systemeAutoriseId = await _systemeAutoriseService.ValiderSystemeApiKey(NoPublicSystemeAutorise.GetGuidOrNull(), ApiKey);

            if (systemeAutoriseId == 0)
            {
                FrwLog.Fin(nameof(SupprimerFormulaire));
                return ValidationProblem("Systeme non autorise");
            }

            await _formulairesService.DeployerSysteme(systemeAutoriseId, entrant.Zip);

            FrwLog.Fin(nameof(DeployerSysteme));

            return Accepted();
        }
    }

    public class EntrantDeployerSysteme
    {
        public byte[] Zip { get; set; } = default!;
    }
}
