using FRW.TR.Commun;
using FRW.TR.Contrats;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FRW.PR.Extra.Services
{
    /// <summary>
    /// FRW326 - Gérer la sécurité des données d'une session utilisateur
    /// </summary>
    public class AuthService
    {
        public const string CLAIM_SESSION_ID = "Session.";

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISessionService _sessionService;
        private readonly IConfiguration _configuration;
        private readonly TimeSpan _expiration = TimeSpan.FromDays(10);

        public AuthService(IHttpContextAccessor httpContextAccessor, ISessionService sessionService, IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor;
            _sessionService = sessionService;
            _configuration = configuration;
        }

        private static AuthenticationProperties ObtenirConfig()
        {

            return new AuthenticationProperties
            {
                //AllowRefresh = <bool>,
                // Refreshing the authentication session should be allowed.

                //ExpiresUtc = DateTimeOffset.UtcNow.AddHours(12),
                // The time at which the authentication ticket expires. A 
                // value set here overrides the ExpireTimeSpan option of 
                // CookieAuthenticationOptions set with AddCookie.

                IsPersistent = false,
                // Whether the authentication session is persisted across 
                // multiple requests. When used with cookies, controls
                // whether the cookie's lifetime is absolute (matching the
                // lifetime of the authentication ticket) or session-based.

                IssuedUtc = DateTimeOffset.UtcNow,
                // The time at which the authentication ticket was issued.

                //RedirectUri = <string>
                // The full path or absolute URI to be used as an http 
                // redirect response value.
            };
        }

        public async Task<(DateTime expiration, Guid idSession)> ConnecterAsync(int idSystemeAutorise, string typeFormulaire, int formulaireId)
        {
            List<Claim> claims;

            if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
            {
                claims = _httpContextAccessor.HttpContext.User.Claims.EffacerExpirees().ToList();
            }
            else
            {
                claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                        new Claim(ClaimTypes.Anonymous, string.Empty),
                        new Claim(ClaimTypes.Role, "Client"),
                    };
            }

            var entrant = new EntrantObtenirSessionFormulaire
            {
                NsFormulaire = formulaireId,
                CodeNatureSession = Constantes.SessionUtilisateur,
                IndicateurSessionValide = true
            };

            var sessionsExistantes = await _sessionService.ObtenirSessionFormulaire(entrant);

            if (sessionsExistantes.Any())
            {
                foreach (var session in sessionsExistantes)
                {
                    var claimFormulaire = claims.LastOrDefault(e => e.Type.Contains($"{idSystemeAutorise}.{typeFormulaire}") && e.Type.StartsWith(CLAIM_SESSION_ID));
                    claims.Remove(claimFormulaire);
                    await _sessionService.Consommer(session.IdSessionPublique);
                }
            }

            var sortantCreerSession = await _sessionService.Creer(formulaireId, Constantes.SessionUtilisateur);
            claims.AjouterAvecExpiration(CLAIM_SESSION_ID + $"{idSystemeAutorise}.{typeFormulaire}.{sortantCreerSession.NoPublicSession}", formulaireId.ToString(), _expiration);

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await _httpContextAccessor.HttpContext.SignInAsync(
                                    CookieAuthenticationDefaults.AuthenticationScheme,
                                    new ClaimsPrincipal(claimsIdentity),
                                    ObtenirConfig());

            return ((DateTime)sortantCreerSession.DateExpiration!, sortantCreerSession.NoPublicSession);
        }

        public async Task DeconnecterAsync()
        {
            await _httpContextAccessor.HttpContext.SignOutAsync();
        }

        public string? ObtenirValeur(string claim)
        {
            return ObtenirClaim(claim)?.valeur;
        }

        public (Claim? claim, string valeur)? ObtenirClaim(string claim)
        {
            var claims = _httpContextAccessor.HttpContext.User.Claims.ToList();
            return claims?.Obtenir(claim);
        }

        /// <summary>
        /// Fermer l'instance d'un formulaire en cours
        /// </summary>
        /// <param name="instance">Id de session à fermer</param>
        public async Task FermerFormulaireAsync(Guid? instance)
        {
            if (instance is null) return;

            await _sessionService.Consommer((Guid)instance);

            if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
            {
                var claims = _httpContextAccessor.HttpContext.User.Claims.ToList();

                var aSupprimer = claims.Where(e => e.Type.Contains(instance.ToString()!));

                if (aSupprimer is { })
                {
                    for (int i = 0; i < aSupprimer.Count(); i++)
                    {
                        claims.Remove(aSupprimer.ElementAt(i));
                    }
                }

                await ProduireCookie(claims);
            }
        }

        private Task ProduireCookie(IEnumerable<Claim> claims)
        {
            var claimsIdentity = new ClaimsIdentity(
                                        claims, CookieAuthenticationDefaults.AuthenticationScheme);

            return _httpContextAccessor.HttpContext.SignInAsync(
                                   CookieAuthenticationDefaults.AuthenticationScheme,
                                   new ClaimsPrincipal(claimsIdentity),
                                   ObtenirConfig());
        }

        public async Task<(int idFormulaire, DateTime expiration)?> RecupererValiderInformationSession(Guid? guidSession, bool mettreAJourExpiration)
        {
            if (guidSession is null) return null;

            int? idFormulaire = null;

            var claims = _httpContextAccessor.HttpContext.User.Claims.ToList();

            var claim = claims.FirstOrDefault(e => e.Type.Contains(guidSession.ToString()!) && e.Type.StartsWith(CLAIM_SESSION_ID));

            var strId = ClaimsExtensions.ObtenirInfos(claim)?.valeur;

            if (int.TryParse(strId, out var idForm))
            {
                idFormulaire = idForm;
            }

            if (idFormulaire is null)
                return null;

            DateTime? expiration = null;

            if (mettreAJourExpiration)
            {
                expiration = await _sessionService.MiseAJourDateExpirationSession((Guid)guidSession);
            }
            else
            {
                var session = await _sessionService.Obtenir((Guid)guidSession);

                if (session is null || session.DateExpiration < DateTime.Now || session.SessionConsommee)
                    return null;

                expiration = session.DateExpiration;
            }

            if (expiration is null)
                return null;

            return ((int)idFormulaire!, (DateTime)expiration!);
        }
    }
}