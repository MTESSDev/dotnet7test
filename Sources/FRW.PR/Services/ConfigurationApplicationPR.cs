using FRW.TR.Commun.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using FRW.TR.Contrats.Constantes;

namespace FRW.PR.Extra.Services
{
    public class ConfigurationApplicationPR : ConfigurationApplicationBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IFormulairesService _formulairesService;

        public ConfigurationApplicationPR(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _formulairesService = httpContextAccessor?.HttpContext?.RequestServices.GetService<IFormulairesService>() ??
                                    throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public override async Task Initialiser()
        {
            if (!EstInitialise)
            {
                if (_httpContextAccessor.HttpContext is null) throw new ArgumentNullException(nameof(_httpContextAccessor));

                _httpContextAccessor.HttpContext.Request.RouteValues.TryGetValue("idSystemeAutorise", out var idSystemeAutorise);
                _httpContextAccessor.HttpContext.Request.RouteValues.TryGetValue("SystemeAutorise", out var systemeAutorise);
                _httpContextAccessor.HttpContext.Request.RouteValues.TryGetValue("typeFormulaire", out var typeFormulaire);
                _httpContextAccessor.HttpContext.Request.RouteValues.TryGetValue("id", out var id);
                _httpContextAccessor.HttpContext.Request.RouteValues.TryGetValue("version", out var version);

                var systemeAutoriseString = systemeAutorise?.ToString() ?? idSystemeAutorise?.ToString();
                if (!string.IsNullOrWhiteSpace(systemeAutoriseString))
                    SystemeAutorise = int.Parse(systemeAutoriseString);

                TypeFormulaire = typeFormulaire?.ToString() ?? id?.ToString();
                Version = version?.ToString();

                await ObtenirConfigurationApplication();

                EstInitialise = true;
            }
        }

        public override async Task ObtenirConfigurationApplication()
        {
            return;

            if (!string.IsNullOrWhiteSpace(TypeFormulaire))
                ConfigurationFormulaire = await _formulairesService.ObtenirContenuFichierConfiguration(SystemeAutorise, TypeFormulaire, Version, NiveauConfig.FORMULAIRE);

            if (SystemeAutorise is { })
                ConfigurationSysteme = await _formulairesService.ObtenirContenuFichierConfiguration(SystemeAutorise, "default", "0", NiveauConfig.SYSTEME);

            ConfigurationGlobale = await _formulairesService.ObtenirContenuFichierConfiguration(SystemeAutorise, "default", "0", NiveauConfig.GLOBAL);
        }
    }
}
