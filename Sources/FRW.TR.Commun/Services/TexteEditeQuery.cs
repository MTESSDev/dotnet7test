using System.Collections.Generic;
using System.Threading.Tasks;

namespace FRW.TR.Commun.Services
{
    public class TexteEditeQuery
    {
        public int SystemeAutorise { get; set; }
        public string TypeFormulaire { get; set; } = default!;
        public string Version { get; set; } = default!;
    }
}
