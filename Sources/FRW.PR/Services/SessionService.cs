using FRW.TR.Contrats;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace FRW.PR.Extra.Services
{
    public class SessionService : ISessionService
    {
        private readonly IDorsale _dorsale;
        private const string _serviceDestination = "FRW.SV.GestionFormulaires";
        private const string _apiPathSession = "/api/v1/Session";

        public SessionService(IDorsale dorsale)
        {
            _dorsale = dorsale;
        }

        public async Task<SortantCreerSession> Creer(int nsForm, string codeNatureSession)
        {
            return await _dorsale.EnvoyerRecevoir<EntrantCreerSession, SortantCreerSession>(
                HttpMethod.Post,
                new EntrantCreerSession
                {
                    CodeNatureSession = codeNatureSession,
                    NsFormulaire = nsForm
                },
                _serviceDestination,
                $"{_apiPathSession}/Creer");
        }

        public async Task<AppelSortant> Supprimer(Guid noPublicSession, string codeNatureSession)
        {
            return await _dorsale.EnvoyerRecevoir<EntrantSupprimerSession, AppelSortant>(
                HttpMethod.Post,
                new EntrantSupprimerSession
                {
                    CodeNatureSession = codeNatureSession,
                    NumeroSession = noPublicSession
                },
                _serviceDestination,
                $"{_apiPathSession}/SupprimerSession");
        }

        public async Task<SessionFormulaire> Obtenir(Guid noPublicSession)
        {
            return await _dorsale.Recevoir<SessionFormulaire>(_serviceDestination, $"{_apiPathSession}/Obtenir/{noPublicSession}");
        }

        public async Task<IEnumerable<SessionFormulaire>> ObtenirSessionFormulaire(EntrantObtenirSessionFormulaire entrant)
        {
            return await _dorsale.EnvoyerRecevoir<EntrantObtenirSessionFormulaire, IEnumerable<SessionFormulaire>>(HttpMethod.Post, entrant, _serviceDestination, $"{_apiPathSession}/ObtenirSessionFormulaire");
        }

        public async Task<DateTime?> MiseAJourDateExpirationSession(Guid noPublicSession)
        {
            return await _dorsale.EnvoyerRecevoir<Rien,DateTime?>(HttpMethod.Post, new Rien(), _serviceDestination, $"{_apiPathSession}/MiseAJourDateExpirationSession/{noPublicSession}");
        }

        public async Task Consommer(Guid noPublicSession)
        {
            await _dorsale.Envoyer(new Rien(), _serviceDestination, $"{_apiPathSession}/Consommer/{noPublicSession}");
        }
    }
}
