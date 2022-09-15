using FRW.TR.Contrats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FRW.PR.Extra.Services
{
    public class SuiviEtatService : ISuiviEtatService
    {
        private readonly IDorsale _dorsale;
        private const string _serviceDestination = "FRW.SV.GestionFormulaires";
        private const string _apiPathSession = "/api/v1/SuiviEtat";

        public SuiviEtatService(IDorsale dorsale)
        {
            _dorsale = dorsale;
        }

        /// <summary>
        /// Creer Suivi Etat Formulaire
        /// </summary>
        /// <param name="noSequenceFormulaire"></param>
        /// <param name="etat"></param>
        /// <returns></returns>
        public async Task CreerSuiviEtatFormulaire(int noSequenceFormulaire, string etat)
        {
            await _dorsale.Envoyer(new Rien(), _serviceDestination, $"{_apiPathSession}/Creer/{noSequenceFormulaire}/{etat}");
        }

        /// <summary>
        /// Obtenir Suivi Etat Formulaire
        /// </summary>
        /// <param name="noSequenceFormulaire"></param>
        /// <returns></returns>
        public async Task<string> ObtenirSuiviEtatFormulaire(int noSequenceFormulaire)
        {   
            var a = await _dorsale.Recevoir<string>(_serviceDestination, $"{_apiPathSession}/Obtenir/{noSequenceFormulaire}");

            return a;
        }
    }
}
