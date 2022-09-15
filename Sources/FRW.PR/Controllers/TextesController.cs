using FRW.PR.Extra.Models;
using FRW.PR.Extra.Services;
using FRW.TR.Commun;
using FRW.TR.Commun.Services;
using FRW.TR.Commun.Utils;
using FRW.TR.Contrats.Contexte;
using FRW.TR.Contrats.Yaml;
using HandlebarsDotNet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using Serilog;
using Serilog.Context;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRW.PR.Extra.Controllers
{
    [Route("/api/[controller]/[action]")]
    [ApiController]
    public class TextesController : Controller
    {
        private readonly ILogger _log = Log.ForContext<TextesController>();
        private readonly IFormulairesService _formService;
        private readonly ITexteEditeService _texteEditeService;

        public TextesController(ITexteEditeService texteEditeService, IFormulairesService formService)
        {
            _texteEditeService = texteEditeService;
            _formService = formService;
        }

        [HttpGet("{langue}/{systemeAutorise:int?}/{typeFormulaire?}/{version?}")]
        [ResponseCache(Duration = 3600, NoStore = false, VaryByHeader = null)]
        public async Task<IActionResult> ObtenirAsync(string langue, int? systemeAutorise, string? typeFormulaire, int? version)
        {
            langue = langue.ToLower();

            if (!(langue == "en" || langue == "fr")) return BadRequest();

            TexteEditeQuery? textesQuery = null;

            if (systemeAutorise is { } &&
                typeFormulaire is { })
            {
                textesQuery = new TexteEditeQuery()
                {
                    SystemeAutorise = (int)systemeAutorise,
                    TypeFormulaire = typeFormulaire,
                    Version = (version ?? 0).ToString()
                };
            }

            var textes = await _texteEditeService.ObtenirValeursAsync(langue, textesQuery);

            var retour = new StringBuilder($"const textesI18n = {{ \r\n {langue}: ");

            retour.Append(textes.ToJsObj(langue)).Append("\r\n}");

            return Content(retour.ToString(), "text/javascript;charset=UTF-8");
        }
    }

    public static class Ext
    {
        public static string? ToJsObj(this object? obj, string langue)
        {
            if (obj is IDictionary<string, object> dict)
            {
                if (dict.ContainsKey("fr") || dict.ContainsKey("en"))
                {
                    return $"`{dict.GetLocalizedStringFromStringDict(langue, true)}`, ";
                }

                var retour = new StringBuilder("{ ");

                foreach (var item in dict)
                {
                    if (item.Key.Equals("function()"))
                    {
                        retour.Clear();

                        var subDir = item.Value as IDictionary<string, object>;
                        subDir!.TryGetValue("js", out var paramsJs);

                        if (paramsJs is { })
                        {
                            retour.Append(paramsJs?.GetLocalizedStringFromStringDict(langue, true));
                            retour.Append(", ");
                        }
                        else
                        {
                            subDir!.TryGetValue("return", out var returnVal);
                            subDir!.TryGetValue("params", out var paramsVal);

                            var paramsArr = ((IList<object>)paramsVal!).ToArray();

                            for (int i = 0; i < paramsArr.Length; i++)
                            {
                                paramsArr[i] = $"{{{paramsArr[i]}}}";
                            }

                            var paramListStr = string.Join(", ", paramsArr);

                            for (int i = 0; i < paramsArr.Length; i++)
                            {
                                paramsArr[i] = $"${paramsArr[i]}";
                            }

                            retour.Append("function (" + paramListStr + ") { return `" + string.Format(returnVal?.GetLocalizedStringFromStringDict(langue, true)?.ToString().Replace("`", @"\`") ?? string.Empty, paramsArr) + "`; },");
                        }
                    }
                    else
                    {
                        retour.Append($"\r\n{item.Key}: ");
                        retour.Append(item.Value.ToJsObj(langue));
                    }
                }

                if (retour[0].Equals('{'))
                    retour.Append("\r\n}, ");

                return retour.ToString();
            }
            else
            {
                var val = obj?.ToString();
                if (val is null) return "null, ";
                else return $"`{val.Replace("`", @"\`")}`, ";
            }
        }
    }
}
