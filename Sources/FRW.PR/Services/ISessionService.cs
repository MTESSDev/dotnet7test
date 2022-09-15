using FRW.TR.Contrats;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FRW.PR.Extra.Services
{
    public interface ISessionService
    {
        Task<SortantCreerSession> Creer(int nsForm, string codeNatureSession);
        Task<SessionFormulaire> Obtenir(Guid noPublicSession);
        Task<IEnumerable<SessionFormulaire>> ObtenirSessionFormulaire(EntrantObtenirSessionFormulaire entrant);
        Task<AppelSortant> Supprimer(Guid noPublicSession, string codeNatureSession);
        Task<DateTime?> MiseAJourDateExpirationSession(Guid noPublicSession);
        Task Consommer(Guid noPublicSession);    
    }
}