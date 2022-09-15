using System.Collections.Generic;

namespace FRW.TR.Contrats.Assignateur
{
    public class EstampilleElement
    {
        public uint PositionX { get; set; }
        public uint PositionY { get; set; }
        public uint TailleFont { get; set; }
        public List<string>? Lignes { get; set; }
    }
}
