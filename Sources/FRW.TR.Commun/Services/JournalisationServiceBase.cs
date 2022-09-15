using CAC.GestionContexte;
using FRW.TR.Commun.Helpers;
using FRW.TR.Contrats;
using FRW.TR.Contrats.Journalisation;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace FRW.TR.Commun.Services
{
    public class JournalisationServiceBase : IJournalisationServiceBase
    {
        private readonly IDorsale _dorsale;
        private IHttpContextAccessor _httpContextAccessor;
        protected const string ServiceExterne = "FRW.SV.GestionFormulaires";

        public JournalisationServiceBase(IDorsale dorsale, IHttpContextAccessor httpAccessor)
        {
            _dorsale = dorsale;
            _httpContextAccessor = httpAccessor;
        }

        public void JournaliserSIG(CodeOptionTransaction codeOptionTransaction, string codeTransaction, string codePartieVariable, object valeurPartieVariable, int? idFormulaire = null, Guid? idSession = null)
        {
            List<PartieVariable<string>>? listePartieVariable = null;
            var partieVariable = new PartieVariable<object>(codePartieVariable, valeurPartieVariable);

            if (partieVariable != null)
            {
                listePartieVariable = new List<PartieVariable<string>>();

                var options = new JsonSerializerOptions();
                options.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;

                listePartieVariable.Add(new PartieVariable<string>(codePartieVariable, JsonSerializer.Serialize(partieVariable.Valeur, partieVariable.Valeur.GetType(), options)));
            }

            var entrant = new EntrantJournalisation()
            {
                CodeOptionTransaction = codeOptionTransaction,
                CodeTransaction = codeTransaction,
                PartiesVariables = listePartieVariable,
                NomUrl = ObtenirNomUrl(),
                UserAgent = ObtenirUserAgent(),
                NumeroIdentifiantDossiersCourrant = idFormulaire.ToString(),
                AdresseIp = IP.GetIpAddress(_httpContextAccessor),
                IdSession = idSession.ToString()
            };

            var retour = _dorsale.Envoyer(entrant, ServiceExterne, "/api/v1/Journalisation/Journaliser");
        }

        private string ObtenirUserAgent()
        {
            return _httpContextAccessor.HttpContext!.Request.Headers["User-Agent"].ToString();
        }

        private string ObtenirNomUrl()
        {
            var path = Path.Combine(_httpContextAccessor.HttpContext!.Request.Host.ToString(), _httpContextAccessor.HttpContext.Request.Path);
            var url = $"{path}{_httpContextAccessor.HttpContext.Request.QueryString}";
            return url;
        }

    }
}
