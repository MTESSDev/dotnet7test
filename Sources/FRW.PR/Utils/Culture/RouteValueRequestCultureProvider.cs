using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;


namespace FRW.PR.Utils.Culture
{
    public class RouteValueRequestCultureProvider : RequestCultureProvider
    {
        private readonly CultureInfo[] _cultures;

        public RouteValueRequestCultureProvider(CultureInfo[] cultures)
        {
            _cultures = cultures;
        }

        /// <summary>
        /// get {culture} route value from path string, 
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns>ProviderCultureResult depends on path {culture} route parameter, or default culture</returns>
        public override Task<ProviderCultureResult> DetermineProviderCultureResult(HttpContext httpContext)
        {
            var defaultCulture = "fr-CA";

            if (httpContext is null) { throw new ArgumentNullException(nameof(httpContext)); }

            var path = httpContext.Request.Path;

            // Si l'url est un appel api, on laisse les autres handler faire le travail
            // Normalement la langue sera dans l'entête HTTP (accept-language)
            if (path.StartsWithSegments("/api"))
            {
                return NullProviderCultureResult;
            }

            // Cette section est uniquement pour la gestion de la langue au retour de CSC
            // Ne pas utiliser dans les URL à l'interne!
            if (httpContext.Request.Query.TryGetValue("langue", out var langueQuery))
            {
                if (langueQuery.ToString().ToLower().Equals("en"))
                {
                    defaultCulture = "en-CA";
                }
                return Task.FromResult(new ProviderCultureResult(defaultCulture));
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                return Task.FromResult(new ProviderCultureResult(defaultCulture));
            }

            var routeValues = httpContext.Request.Path.Value.Split('/');
            if (routeValues.Length <= 1)
            {
                return Task.FromResult(new ProviderCultureResult(defaultCulture));
            }

            if (!_cultures.Any(x => x.TwoLetterISOLanguageName.ToLower() == routeValues[1].ToLower()))
            {
               return Task.FromResult(new ProviderCultureResult(defaultCulture));
            }

            switch (routeValues[1].ToLower())
            {
                case "en":
                    return Task.FromResult(new ProviderCultureResult("en-CA"));
                case "fr":
                    return Task.FromResult(new ProviderCultureResult("fr-CA"));
                default:
                    break;
            }

            return Task.FromResult(new ProviderCultureResult(defaultCulture));
        }
    }
}
