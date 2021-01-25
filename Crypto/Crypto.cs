using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Crypto
{
    public class Crypto
    {
        //encryption
        const string encyptionPrefixV1 = "0x01_";
        static byte[] _key = { };
        static byte[] _IV = { 12, 21, 43, 17, 57, 35, 67, 27 };
        const string _encryptKey = "global24"; // MUST be 8 characters
        static DESCryptoServiceProvider _provider = new DESCryptoServiceProvider();

        static Crypto()
        {
            _key = Encoding.UTF8.GetBytes(_encryptKey);
        }

        public static string Encrypt(string s)
        {
            if (string.IsNullOrEmpty(s))
                return s;

            return encyptionPrefixV1 + EncryptStringV1(s);
        }

        public static string Decrypt(string s)
        {
            if (String.IsNullOrEmpty(s))
                return s;
            if (s.Length < encyptionPrefixV1.Length)
                return s;

            string prefix =  s.Substring(0, encyptionPrefixV1.Length);
            if (prefix == encyptionPrefixV1)
                return DecryptStringV1(s.Substring(encyptionPrefixV1.Length));

            return s;
        }

        static string EncryptStringV1(string inputString)
        {
            var memStream = new MemoryStream();
            ICryptoTransform transform = _provider.CreateEncryptor(_key, _IV);
            CryptoStream cryptoStream = new CryptoStream(memStream, transform, CryptoStreamMode.Write);
            byte[] byteInput = Encoding.UTF8.GetBytes(inputString);
            cryptoStream.Write(byteInput, 0, byteInput.Length);
            cryptoStream.FlushFinalBlock();
            return Convert.ToBase64String(memStream.ToArray());
        }
        static string DecryptStringV1(string inputString)
        {
            var byteInput = Convert.FromBase64String(inputString);
            var memStream = new MemoryStream();
            ICryptoTransform transform = _provider.CreateDecryptor(_key, _IV);
            CryptoStream cryptoStream = new CryptoStream(memStream, transform, CryptoStreamMode.Write);
            cryptoStream.Write(byteInput, 0, byteInput.Length);
            cryptoStream.FlushFinalBlock();
            return Encoding.UTF8.GetString(memStream.ToArray());
        }

    }

    public static class CryptoExtension
    {
        public static string Encrypt(this string s)
        {
            return Crypto.Encrypt(s);
        }

        public static string Decrypt(this string s)
        {
            return Crypto.Decrypt(s);
        }
    }
}
