using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FRW.TR.Commun.Securite.VerificationFichier
{
    public abstract class FileType
    {
        public static IEnumerable<FileType> FormatsFichiers
        {
            get
            {
                yield return new JpegFileType();
                yield return new PngFileType();
                yield return new TiffFileType();
                yield return new DocxFileType();
                yield return new XlsxFileType();
                yield return new OdtFileType();
                yield return new PdfFileType();
            }
        }

        protected string Description { get; set; } = default!;

        protected string Name { get; set; } = default!;

        protected byte NombreOctetsPourVerification { get; set; } = 0;

        private bool ValidationEnteteEtendue => NombreOctetsPourVerification > 0;

        public List<string> Extensions { get; }
            = new List<string>();

        public List<string> MimeType { get; }
            = new List<string>();

        public abstract bool EstFusionnable { get; }

        public abstract string? Vignette { get; }

        private List<byte[]> Signatures { get; }
            = new List<byte[]>();

        public int SignatureLength => Signatures.Max(m => m.Length);

        protected FileType AddSignatures(params byte[][] bytes)
        {
            Signatures.AddRange(bytes);
            return this;
        }

        protected FileType AddExtensions(params string[] extensions)
        {
            Extensions.AddRange(extensions);
            return this;
        }

        protected FileType AddMimes(params string[] mimes)
        {
            MimeType.AddRange(mimes);
            return this;
        }

        public FileTypeVerifyResult Verify(Stream stream)
        {
            if (stream is null) { throw new ArgumentNullException(nameof(stream)); }
            stream.Position = 0;

            byte[] headerBytes;

            var signatureLength = SignatureLength;

            using (var reader = new BinaryReader(stream, System.Text.Encoding.Default, true))
            {
                if (ValidationEnteteEtendue)
                {
                    signatureLength = NombreOctetsPourVerification;
                }
                headerBytes = reader.ReadBytes(signatureLength);
            }

            var isVerified = false;
            if (ValidationEnteteEtendue)
            {
                foreach (var signature in Signatures)
                {
                    if (NombreOctetsPourVerification < signature.Length)
                    {
                        throw new Exception("Nombre d'octets à rechercher trop petit par rapport à la signature.");
                    }

                    for (var i = 0; i < (headerBytes.Length - signature.Length); i++)
                    {
                        isVerified = headerBytes.Skip(i).Take(signature.Length).SequenceEqual(signature);
                        if (isVerified)
                        {
                            break;
                        }
                    }

                    if (isVerified)
                    {
                        break;
                    }
                }
            }
            else
            {
                isVerified = Signatures.Any(signature =>
                        headerBytes.Take(signature.Length)
                            .SequenceEqual(signature));
            }

            return new FileTypeVerifyResult
            {
                Name = Name,
                Description = Description,
                MimeType = MimeType.FirstOrDefault()!,
                Extensions = Extensions,
                IsVerified = isVerified,
            };
        }
    }

    public class FileTypeVerifyResult
    {
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public bool IsVerified { get; set; }
        public string MimeType { get; set; } = default!;
        public IEnumerable<string> Extensions { get; set; } = default!;
    }
}
