using FRW.PR.Extra.Services;
using FRW.PR.Extra.Utils;
using FRW.PR.Services;
using FRW.TR.Commun;
using FRW.TR.Commun.Services;
using FRW.TR.Contrats;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;
using System;
using System.Threading.Tasks;

namespace FRW.PR.Extra.Middlewares
{
    public class DevcfgMiddleware
    {
        private readonly RequestDelegate _next;
        public DevcfgMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext, IConfigurationApplication configurationApplication, IContexteDevAccesseur contexte)
        {
            var isValid = true;

            if (contexte.EstActif)
            {
                var route = httpContext.Request.Path.Value.Replace("/fr/", "/", StringComparison.InvariantCultureIgnoreCase)
                                                          .Replace("/en/", "/", StringComparison.InvariantCultureIgnoreCase);

                var template = Match("/{Action?}/{Systeme?}/{TypeFormulaire?}/{autre1?}/{autre2?}", route);

                template.TryGetValue("Action", out var action);
                template.TryGetValue("Systeme", out var systeme);
                template.TryGetValue("TypeFormulaire", out var typeFormulaire);

                var strAction = action?.ToString() ?? string.Empty;
                var strTypeFormulaire = typeFormulaire?.ToString() ?? string.Empty;
                var strSysteme = systeme?.ToString() ?? string.Empty;

                if (!strAction.Equals("api")
                    && strAction.Equals($"form", StringComparison.CurrentCultureIgnoreCase)
                    && !strTypeFormulaire.Equals($"devcfg", StringComparison.CurrentCultureIgnoreCase))
                {
                    httpContext.Response.Cookies.Append($"FRWDEVCFGSYS", strSysteme, new CookieOptions() { Expires = DateTime.Now.AddDays(365) });

                    if (!string.IsNullOrEmpty(strSysteme))
                    {
                        if (contexte.ConfigDev is { })
                        {
                            await configurationApplication.Initialiser(int.Parse(strSysteme), "devcfg", "0");

                            var dynamicForm = OutilsYaml.DeserializerString<DynamicForm>(configurationApplication.ConfigurationFormulaire ?? string.Empty);
                            var validation = Validateur.ValiderFormulaire(contexte.ConfigDev, configurationApplication, dynamicForm, null);

                            isValid = validation.IsValid;
                            configurationApplication.EstInitialise = false;
                        }
                        else
                        {
                            isValid = false;
                        }

                    }

                }

                if (!isValid)
                {
                    httpContext.Response.Redirect($"/form/{strSysteme}/devcfg/"); //?redirect={HttpUtility.UrlEncode(httpContext.Request.Path)}");
                }
            }

            if (isValid)
                await _next(httpContext);
        }

        public RouteValueDictionary Match(string routeTemplate, string requestPath)
        {
            var template = TemplateParser.Parse(routeTemplate);

            var matcher = new TemplateMatcher(template, GetDefaults(template));

            var rvd = new RouteValueDictionary();

            matcher.TryMatch(requestPath, rvd);

            return rvd;
        }

        // This method extracts the default argument values from the template.
        private RouteValueDictionary GetDefaults(RouteTemplate parsedTemplate)
        {
            var result = new RouteValueDictionary();

            foreach (var parameter in parsedTemplate.Parameters)
            {
                if (parameter.DefaultValue != null)
                {
                    result.Add(parameter.Name, parameter.DefaultValue);
                }
            }

            return result;
        }
    }


    public static class DevcfgMiddlewareExtensions
    {
        public static IApplicationBuilder UseDevcfgMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<DevcfgMiddleware>();
        }
    }
}
