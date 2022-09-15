namespace FRW.TR.Commun.Securite.VerificationFichier
{
    public sealed class PngFileType : FileType
    {
        public PngFileType()
        {
            Name = "PNG";
            Description = "PNG Image";
            AddExtensions("png");
            AddSignatures(
                new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }
            );
            AddMimes(new[] { "image/png" });
        }
        public override bool EstFusionnable => true;

        public override string? Vignette => null;
    }
}
