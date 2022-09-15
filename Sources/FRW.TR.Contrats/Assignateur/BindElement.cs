using System;
using System.Collections.Generic;
using System.Text;

namespace FRW.TR.Contrats.Assignateur
{
    public class BindElement
    {
        public IEnumerable<string>? Champs { get; set; }
        public string? Formule { get; set; }
        public string? FormuleAnglaise { get; set; }
    }
}
