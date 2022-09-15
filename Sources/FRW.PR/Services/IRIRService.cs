using FRW.TR.Contrats;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FRW.PR.Extra.Services
{
    public interface IRIRService
    {
        /// <summary>
        /// FRW215 - Rechercher et obtenir une adresse postale normalisée
        /// </summary>
        /// <param name="codePostal"></param>
        /// <param name="noCivique"></param>
        /// <returns></returns>
        Task<List<Adresse>> RechercherAdresse(string codePostal, int noCivique);

        /// <summary>
        /// FRW215 - Rechercher et obtenir une adresse postale normalisée par code postal
        /// </summary>
        /// <param name="codePostal"></param>
        /// <returns></returns>
        Task<List<Adresse>> RechercherAdresseParCodePostal(string codePostal);

    }
}
