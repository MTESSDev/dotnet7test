using HashidsNet;
using System;
using System.Text;

namespace FRW.TR.Commun.Extensions
{
    public static class ProtectionDonneesExtensions
    {
        public static string EncoderStringId(this string id, string salt = "")
        {
            var hashids = new Hashids(salt);
            return hashids.EncodeHex(ToHexString(id));
        }

        public static string EncoderId(this int id, string salt)
        {
            var hashids = new Hashids(salt);
            return hashids.Encode(Math.Abs(id));
        }

        public static int DecoderId(this string hash, string salt)
        {
            var hashids = new Hashids(salt);
            return hashids.Decode(hash)[0];
        }

        public static string DecoderStringId(this string hash, string salt = "")
        {
            var hashids = new Hashids(salt);
            var text = hashids.DecodeHex(hash);
            return FromHexString(text);
        }

        public static string ToHexString(string str)
        {
            var sb = new StringBuilder();
            var bytes = Encoding.Unicode.GetBytes(str);

            foreach (var t in bytes)
            {
                sb.Append(t.ToString("X2"));
            }

            return sb.ToString();
        }

        public static string FromHexString(string hexString)
        {
            if (hexString is null) { return ""; }

            var bytes = new byte[hexString.Length / 2];
            for (var i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }

            return Encoding.Unicode.GetString(bytes);
        }
    }
}
