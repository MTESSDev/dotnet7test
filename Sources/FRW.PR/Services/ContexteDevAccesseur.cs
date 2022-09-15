using System;
using System.Collections.Generic;
using FRW.TR.Commun.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace FRW.PR.Services
{
    /// <summary/>
    public class ContexteDevAccesseur : IContexteDevAccesseur
    {
        /// <summary/>
        public ContexteDevAccesseur(IHttpContextAccessor httpContextAccessor, IConfiguration config)
        {
            if (httpContextAccessor is null) { throw new ArgumentNullException(nameof(httpContextAccessor)); }

            if (config.GetValue<bool>("estProduction")) { return; }

            EstActif = true;

            httpContextAccessor.HttpContext.Request.Cookies.TryGetValue($"FRWDEVCFGSYS", out var systeme);
            var strSysteme = systeme?.ToString();

            if (!string.IsNullOrWhiteSpace(strSysteme))
            {
                httpContextAccessor.HttpContext.Request.Cookies.TryGetValue($"FRWDEVCFG" + systeme, out var configDev);

                if (!string.IsNullOrWhiteSpace(configDev))
                {
                    ConfigDevBrute = configDev;
                    ConfigDev = JsonConvert.DeserializeObject<IDictionary<object, object>>(
                                             configDev,
                                                 new JsonConverter[] {
                                                    new ConvertisseurFRW() }
                                             );

                    httpContextAccessor.HttpContext.Items.Add("x-vals", ConfigDev);
                }
            }
        }

        /// <summary />
        public IDictionary<object, object>? ConfigDev { get; set; }
        public string? ConfigDevBrute { get; set; }
        /*public string? Action { get; set; }
        public string? TypeFormulaire { get; set; }*/
        //public string? Systeme { get; set; }
        public bool EstActif { get; set; }
    }
}
