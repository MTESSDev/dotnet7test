using System.Collections.Generic;

namespace FRW.TR.Contrats.Assignateur
{
    public class Binder
    {
        public Dictionary<string, Dictionary<string, string>>? Config { get; set; }
        public List<BundleElement>? Bundles { get; set; }
        public List<TemplateElement>? Templates { get; set; }
        public Dictionary<string, Dictionary<string, BindElement>>? Bind { get; set; }
    }
}
