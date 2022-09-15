using FRW.TR.Contrats;
using System;
using System.Threading.Tasks;

namespace FRW.PR.Extra.Services
{
    public interface ISystemeAutoriseService
    {
        Task<SystemeAutorise> Obtenir(Guid noPublicSystemeAutorise);

        Task<bool> ValiderCleAuthentification(int nsSystemeAutorise, string cleAuthentification);

        Task<int> ValiderSystemeApiKey(Guid? guid, string? apiKey);
    }
}
