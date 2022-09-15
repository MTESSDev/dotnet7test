using FRW.PR.Extra.Services;
using FRW.TR.Commun;
using FRW.TR.Contrats.Assignateur;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace FRW.PR.Extra.Pages
{
    public class SaveBindingModel : PageModel
    {
        private readonly ILogger<SaveBindingModel> _logger;
        private readonly IFormulairesService _formulairesService;

        public SaveBindingModel(ILogger<SaveBindingModel> logger, IFormulairesService formulaireService)
        {
            _logger = logger;
            _formulairesService = formulaireService;
        }


        public async Task<IActionResult> OnPost(int idSystemeAutorise, string id, string gabarit, [FromBody] Dictionary<string, BindElement> data)
        {
            /*var cleanId = id.Replace('@', '/');
            //var cleanPdf = pdf.Replace('@', '/');
            if (!System.IO.File.Exists(@$"mapping/{cleanId}/ecsbind.yml"))
            {
                return NotFound();
            }

            var bind = OutilsYaml.LireFicher<Binder>(@$"mapping/{cleanId}/ecsbind.yml");*/

            var cfg = await _formulairesService.ObtenirContenuFichierBinding(idSystemeAutorise, id);

            if (string.IsNullOrWhiteSpace(cfg))
                return NotFound();

            var bind = OutilsYaml.DeserializerString<Binder>(cfg);

            if (bind.Bind is null)
                bind.Bind = new Dictionary<string, Dictionary<string, BindElement>>();

            if (bind.Bind?.ContainsKey(gabarit) ?? false)
            {
                bind.Bind[gabarit] = data;
            }
            else
            {
                bind.Bind?.Add(gabarit, data);
            }

            var yaml = OutilsYaml.SerialiserString(bind);
            //OutilsYaml.EcrireFichier(bind, @$"mapping/{cleanId}/ecsbind.yml");
            return Content(yaml, "text/plain");
        }
    }
}
