using FRW.TR.Contrats;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FRW.PR.Extra.Services
{
    public class RIRService : IRIRService
    {
        private readonly IDorsale _dorsale;
        private const string _serviceDestination = "FRW.SV.GestionFormulaires";
        private const string _apiPathSession = "/api/v1/RIR";

        public RIRService(IDorsale dorsale)
        {
            _dorsale = dorsale;
        }
        public async Task<List<Adresse>> RechercherAdresse(string codePostal, int noCivique)
        {

            return await _dorsale.Recevoir<List<Adresse>>(_serviceDestination, $"{_apiPathSession}/RechercherAdresse/{codePostal}{noCivique}");
        }

        public async Task<List<Adresse>> RechercherAdresseParCodePostal(string codePostal)
        {
            return await _dorsale.Recevoir<List<Adresse>>(_serviceDestination, $"{_apiPathSession}/RechercherAdresseParCodePostal/{codePostal}");
        }
    }
}
