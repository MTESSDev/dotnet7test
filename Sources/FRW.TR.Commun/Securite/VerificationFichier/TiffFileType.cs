namespace FRW.TR.Commun.Securite.VerificationFichier
{
    public sealed class TiffFileType : FileType
    {
        public TiffFileType()
        {
            Name = "TIFF";
            Description = "TIFF file";
            AddExtensions("tiff");
            AddSignatures(
                new byte[] { 0x49, 0x20, 0x49 },
                new byte[] { 0x49, 0x49, 0x2A, 0x00 },
                new byte[] { 0x4D, 0x4D, 0x00, 0x2A },
                new byte[] { 0x4D, 0x4D, 0x00, 0x2B }
            );
            AddMimes(new[] { "image/tiff", "image/x-tiff" });
        }
        public override bool EstFusionnable => true;

        public override string? Vignette => null;
    }
}
