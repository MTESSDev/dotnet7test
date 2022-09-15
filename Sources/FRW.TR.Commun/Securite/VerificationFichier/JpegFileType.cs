namespace FRW.TR.Commun.Securite.VerificationFichier
{
    public sealed class JpegFileType : FileType
    {
        public JpegFileType()
        {
            Name = "JPEG";
            Description = "JPEG IMAGE";
            AddExtensions("jfif", "jpe", "jpeg", "jpg");
            AddSignatures(
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 },
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE2 },
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE3 }, 
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE8 }
            );
            AddMimes(new[] { "image/jpeg", "images/pjpeg" });
        }
        public override bool EstFusionnable => true;

        public override string? Vignette => null;
    }
}
