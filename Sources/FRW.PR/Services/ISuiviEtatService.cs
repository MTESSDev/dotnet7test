using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FRW.PR.Extra.Services
{
    public interface ISuiviEtatService
    {
        public Task CreerSuiviEtatFormulaire(int noSequenceFormulaire, string etat);

        public Task<string> ObtenirSuiviEtatFormulaire(int noSequenceFormulaire);
    }
}
