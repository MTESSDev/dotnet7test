using FRW.TR.Contrats.Assignateur;
using System.Collections.Generic;

namespace FRW.TR.Contrats.ConversionDonnees
{
    public class Template
    {
        public TemplateElement Proprietes { get; set; } = default!;
        public Dictionary<string, string>? Champs { get; set; }
    }
}