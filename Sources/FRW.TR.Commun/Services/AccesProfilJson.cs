using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CAC.AccesProfil.Client;
using FRW.TR.Contrats.Contexte;
using Microsoft.Extensions.Configuration;

namespace FRW.TR.Commun.Services
{
    public class AccesProfilJson : AccesProfilBase, IProfilFRW
    {
        private readonly IConfiguration _config;

        public AccesProfilJson(IConfiguration config) : base(chargementManuel: true)
        {
            _config = config;
            Preparration();
        }
        protected override IDictionary<string, string> InterpreterSource()
        {
            return _config.GetSection("ServicesExternes:CAC.AccesProfil:Data").Get<Dictionary<string, string>>();
        }

        protected override async Task<IDictionary<string, string>> InterpreterSourceAsync()
        {
            return await Task.FromResult(_config.GetSection("ServicesExternes:CAC.AccesProfil:Data").Get<Dictionary<string, string>>());
        }

        public void ForcerChargementProfil(string codeUtilisateurComplet)
        {
            CodeCompletUtilisateur = codeUtilisateurComplet;
        }

        public string ObtenirValeurCacherErreur(string nomVariable, string valeurRemplacement)
        {
            try
            {
                return ObtenirValeur(nomVariable);
            }
            catch (ArgumentException)
            {
                return valeurRemplacement;
            }
        }
    }
}
