namespace FRW.TR.Contrats
{
    public class EntrantDeployerFormulaire
    {
        public int IdSystemeAutorise { get; set; }
        public byte[] ContenuZip { get; set; } = default!;
    }
}