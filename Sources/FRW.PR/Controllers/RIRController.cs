using FRW.PR.Extra.Services;
using FRW.TR.Contrats;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Collections.Generic;
using System.Threading.Tasks;
using FRW.TR.Commun.Extensions;
using System;

namespace FRW.PR.Extra.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class RIRController : Controller
    {
        private readonly IRIRService _rirService;
        private readonly ILogger FrwLog = Serilog.Log.ForContext<SoumettreController>();
        public RIRController(IRIRService rirService)
        {
            _rirService = rirService;
        }


        /// <summary>
        /// FRW215 - Rechercher et obtenir une adresse postale normalisée
        /// </summary>
        /// <param name="codePostal"></param>
        /// <param name="noCivique"></param>
        /// <returns></returns>
        [HttpGet("{codePostal}/{noCivique}")]
        public async Task<ActionResult<List<Adresse>>> RechercherAdresse(string codePostal, int noCivique)
        {
            try
            {
                var retour = await _rirService.RechercherAdresse(codePostal, noCivique);

                return Ok(retour);
            }
            catch (Exception ex)
            {
                FrwLog.Error(ex, "Erreur RechercherAdresse");
                return StatusCode(500);
            }
        }

        /// <summary>
        /// FRW215 - Rechercher et obtenir une adresse postale normalisée par code postal
        /// </summary>
        /// <param name="codePostal"></param>
        /// <returns></returns>
        [HttpGet("{codePostal}")]
        public async Task<ActionResult<List<Adresse>>> RechercherAdresseParCodePostal(string codePostal)
        {
            try
            {
                var retour = await _rirService.RechercherAdresseParCodePostal(codePostal);

                return Ok(retour);
            }
            catch (Exception ex)
            {
                FrwLog.Error(ex, "Erreur RechercherAdresseParCodePostal");
                return StatusCode(500);
            }
        }
    }
}
