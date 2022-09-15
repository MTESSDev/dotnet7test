using System;
using System.Collections.Generic;
using System.Text;

namespace FRW.TR.Contrats.Assignateur
{
    public class TemplateElement
    {
        public string Id { get; set; } = default!;
        public string Name { get; set; } = default!;
        [Obsolete("Utiliser Gabarit au lieu de Pdf.")]
        public object Pdf { get { return Gabarit; } set { Gabarit = value; } }
        public object Gabarit { get; set; } = default!;
        public string? ConditionsEt { get; set; }
        public string? ConditionsOu { get; set; }
        public bool ToujoursProduire { get; set; } = false;
        public List<BlocIgnorer>? Ignorer { get; set; }
    }
}
