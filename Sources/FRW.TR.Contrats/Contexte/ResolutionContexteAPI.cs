using CAC.GestionContexte.Resolution;
using System;

namespace FRW.TR.Contrats.Contexte
{
    public class ResolutionContexteAPI : IResolutionContexte
    {
        private readonly IContexteCAC _contexteCAC;
        public ResolutionContexteAPI(IContexteCAC contexteCAC)
        {
            _contexteCAC = contexteCAC;
        }

        public string ObtenirCodeSysteme() => _contexteCAC.CodeSysteme;


        public string ObtenirCodeUtilisateur() => _contexteCAC.CodeUtilisateur;

        public string ObtenirCodeUtilisateurComplet() => _contexteCAC.CodeUtilisateurComplet;

        public string ObtenirCodeUtilisateurCompletUPN() => _contexteCAC.CodeUtilisateurCompletUPN;

        public DateTime ObtenirDateHeureProduction() => _contexteCAC.DateHeureProduction;

        public DateTime ObtenirDateProduction() => _contexteCAC.DateProduction;

        public string ObtenirDomaine() => _contexteCAC.Domaine;

        public string ObtenirDomaineUPN() => _contexteCAC.DomaineUPN;

        public string ObtenirEnvironnement() => _contexteCAC.Environnement;

        public bool ObtenirEstProduction() => _contexteCAC.EstProduction;

        public string ObtenirPhase() => _contexteCAC.Phase;

        public string ObtenirPoste() => _contexteCAC.Poste;
    }
}
