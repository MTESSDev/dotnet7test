using FRW.TR.Commun.Services;
using FRW.TR.Contrats;
using Microsoft.AspNetCore.Http;

namespace FRW.PR.Extra.Services
{
    public class JournalisationService : JournalisationServiceBase, IJournalisationService
    {
        public JournalisationService(IDorsale dorsale, IHttpContextAccessor httpAccessor) : base(dorsale, httpAccessor)
        { }

    }
}