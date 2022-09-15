namespace FRW.TR.Commun.Securite.VerificationFichier
{
    public sealed class OdtFileType : FileType
    {
        public OdtFileType()
        {
            Name = "ODT";
            Description = "OpenDocument text";
            AddExtensions("odt");
            AddSignatures(
                new byte[] { 0x50, 0x4B, 0x03, 0x04 }
            );
            AddMimes(new[] { "application/vnd.oasis.opendocument.text" });
        }

        public override bool EstFusionnable => false;

        public override string? Vignette => null; 
    }
}
