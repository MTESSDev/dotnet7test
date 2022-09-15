using System;
using System.Collections.Generic;
using System.Text;

namespace FRW.TR.Contrats
{
    public class SessionFormulaire
    {
        public int NsFormulaire { get; set; }

        public string? CodeNatureSession { get; set; }

        public Guid IdSessionPublique { get; set; }

        public DateTime DateCreation { get; set; }

        public bool SessionConsommee { get; set; }

        public DateTime? DateExpiration { get; set; }
    }
}
