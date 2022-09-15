using FRW.TR.Contrats;
using System;
using System.Threading.Tasks;

namespace FRW.PR.Extra.Services
{
    public class SystemeAutoriseService : ISystemeAutoriseService
    {
        private readonly IDorsale _dorsale;
        private const string _serviceDestination = "FRW.SV.GestionFormulaires";
        private const string _apiPathSession = "/api/v1/SystemeAutorise";

        public SystemeAutoriseService(IDorsale dorsale)
        {
            _dorsale = dorsale;
        }

        public async Task<int> ValiderSystemeApiKey(Guid? guid, string? apiKey)
        {
            if (apiKey is null) return 0;
            if (guid is null) return 0;

            var systemeAutorise = await Obtenir((Guid)guid);
            int id = systemeAutorise?.ID ?? 0;

            if (id == 0 || !await ValiderCleAuthentification(id, apiKey))
            {
                return 0;
            }

            return id;
        }

        public async Task<SystemeAutorise> Obtenir(Guid noPublicSystemeAutorise)
        {
            return await _dorsale.Recevoir<SystemeAutorise>(_serviceDestination, $"{_apiPathSession}/ObtenirParClePublic/{noPublicSystemeAutorise}");
        }

        public async Task<bool> ValiderCleAuthentification(int nsSystemeAutorise, string cleAuthentification)
        {
            return await _dorsale.Recevoir<bool>(_serviceDestination, $"{_apiPathSession}/ValiderCleAuthentification/{nsSystemeAutorise}/{cleAuthentification}");
        }
    }
}
