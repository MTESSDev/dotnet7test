using System.Collections.Generic;

namespace FRW.TR.Contrats.Assignateur
{
    public class BundleElement
    {
        public string NomSortie { get; set; } = default!;
        public string? ConditionsEt { get; set; }
        public string? ConditionsOu { get; set; }
        public List<string>? Templates { get; set; }
        public EstampilleElement? Estampille { get; set; }
    }
}
