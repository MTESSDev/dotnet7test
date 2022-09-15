using FRW.TR.Contrats;
using System.Collections.Generic;

namespace FRW.PR.Extra.Models
{
    public class RetourRenderPage
    {
        public RetourRenderPage()
        {
            Sections = new List<Section>();
            Config = new Dictionary<string, object?>();
            DynamicForm = null;
        }

        public DynamicForm? DynamicForm { get; internal set; }
        public Dictionary<string, object?> Config { get; internal set; }
        public string? Title { get; internal set; }
        public List<Section> Sections { get; internal set; }
        public string? FormRaw { get; internal set; }
    }
}