using System;

namespace FRW.TR.Commun
{
    public static class Constantes
    {
        //État du formulaire pour le suivi
        public const string EtatCreer = "CRE";
        public const string EtatMettreAJour = "MAJ";
        public const string EtatCourriel = "COURRIEL";
        public const string EtatReprise = "REP";
        public const string EtatSupprime = "SUP";
        public const string EtatExpire = "EXPIRE";
        public const string EtatTransmis = "TRANSMIS";

        //Codes de nature de session
        public const string SessionCourriel = "COURRIEL";
        public const string SessionSysteme = "SYSTEME";
        public const string SessionUtilisateur = "UTIL";
    }
}
