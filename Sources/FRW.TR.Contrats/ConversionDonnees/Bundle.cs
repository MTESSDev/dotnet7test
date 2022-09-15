using FRW.TR.Contrats.Assignateur;
using System.Collections.Generic;

namespace FRW.TR.Contrats.ConversionDonnees
{
    public class Bundle
    {
        public bool Actif { get; set; }
        public string NomSortie { get; set; } = default!;
        public List<string> Templates { get; set; } = default!;
        public EstampilleElement? Estampille { get; set; }
    }
}
