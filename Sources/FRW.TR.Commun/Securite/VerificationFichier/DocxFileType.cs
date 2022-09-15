namespace FRW.TR.Commun.Securite.VerificationFichier
{
    public sealed class DocxFileType : FileType
    {
        public DocxFileType()
        {
            Name = "DOCX";
            Description = "MS Office Open XML Format Document";
            AddExtensions("docx");
            AddSignatures(
                new byte[] { 0x50, 0x4B, 0x03, 0x04, 0x14, 0x00, 0x06, 0x00 }
            );
            AddMimes(new[] { "application/vnd.openxmlformats-officedocument.wordprocessingml.document" });
        }

        public override bool EstFusionnable => false;

        public override string? Vignette => null; 

    }
}
