using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace FRW.PR.Extranet.Pages
{
    public class PafModel : PageModel
    {
        private readonly IConfiguration _config;

        public PafModel(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Appeler la page Paf pour forcer une erreur technique
        /// </summary>
        /// <returns></returns>
        public ActionResult OnGet()
        {
            if (!_config.GetValue<bool>("estProduction"))
            {
                throw new Exception("PAF - Erreur technique simulée");
            }
            else
            {
                return NotFound();
            }
        }
    }
}
