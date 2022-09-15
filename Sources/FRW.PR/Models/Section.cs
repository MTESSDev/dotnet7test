using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace FRW.PR.Extra.Models
{
    public class Section
    {
        public int No { get; set; }
        public string Id { get; set; } = string.Empty;
        public string? Titre { get; set; } = string.Empty;
        [JsonProperty("v-if")]
        public string VIf { get; set; } = string.Empty;
        public string? Classes { get; set; }
        public bool CacherTexteExplicatifChampsObligatoires { get; set; }
        public IEnumerable<Section>? Pages { get; set; }
    }
}