namespace FRW.TR.Commun.Securite.VerificationFichier
{
    public sealed class PdfFileType : FileType
    {
        public PdfFileType()
        {
            Name = "PDF";
            Description = "PDF file";
            NombreOctetsPourVerification = 128;
            AddExtensions("pdf");
            AddSignatures(
                new byte[] { 0x25, 0x50, 0x44, 0x46 }
            );
            AddMimes(new[] { "application/pdf" });
        }
        public override bool EstFusionnable => true;

        public override string? Vignette => null;
    }
}
