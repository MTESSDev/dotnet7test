using System;
using FRW.TR.Contrats;

namespace FRW.TR.Commun
{
    public class DorsaleException : Exception
    {

        public string? Url { get; set; }
        public Erreur? Erreur { get; set; }

        private DorsaleException(string message)
           : base(message)
        {
        }

        private DorsaleException()
        {
        }

        private DorsaleException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public DorsaleException(string message, string? url, Erreur? erreur, Exception? innerException) : base(message, innerException)
        {
            Url = url;
            Erreur = erreur;
        }

    }
}
