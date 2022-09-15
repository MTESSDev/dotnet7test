using FRW.PR.Extra.Services;
using FRW.TR.Commun;
using FRW.TR.Commun.Services;
using FRW.TR.Commun.Utils;
using FRW.TR.Contrats.Constantes;
using FRW.TR.Contrats.Yaml;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FRW.PR.Extra
{
    public class TexteEditeService : TexteEditeServiceBase, ITexteEditeService
    {
        private readonly IFormulairesService _formService;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _config;

        public TexteEditeService(IFormulairesService formulairesService, IConfiguration config, IMemoryCache memoryCache) : base()
        {
            _formService = formulairesService;
            _config = config;
            _cache = memoryCache;
        }

        public override async Task<IDictionary<string, object>?> ObtenirValeursAsync(string langue, TexteEditeQuery? texteEditeQuery)
        {
            int secondesExpiration =_config.GetValue<int>("FRW:DureeCacheGeneral");
            var textes = await _cache.GetOrCreateAsync($"textes-edites-default-{langue}", async entry =>
                                 {
                                     int secondesExpiration = _config.GetValue<int>("FRW:DureeCacheGeneral");

                                     entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(secondesExpiration);
                                     var defaultConfig = await _formService.ObtenirContenuFichierConfiguration(0, "default", "0", NiveauConfig.GLOBAL);
                                     return OutilsYaml.DeserializerStringTextes<TextesYaml>(defaultConfig!);
                                 });

            return textes.Textes;
        }
    }
}