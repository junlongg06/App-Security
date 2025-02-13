using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace App_Security_Assignment.Helpers
{
    public static class EncryptionHelper
    {
        // ✅ Store the Base64 key here
        private static readonly byte[] Key = Convert.FromBase64String("nVaA4NbOT4yKvaWMQ+Az8J+Kz/YQcqnRmxvnisja9PQ=");

        public static string Encrypt(string text)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentException("Text cannot be null or empty.", nameof(text));

            using (Aes aes = Aes.Create())
            {
                aes.Key = Key; // ✅ Correctly converted key
                aes.GenerateIV();
                byte[] iv = aes.IV;

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                using (var ms = new MemoryStream())
                {
                    ms.Write(iv, 0, iv.Length);

                    using (var cryptoStream = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (var sw = new StreamWriter(cryptoStream))
                    {
                        sw.Write(text);
                    }

                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        public static string Decrypt(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
                throw new ArgumentException("Encrypted text cannot be null or empty.", nameof(encryptedText));

            try
            {
                byte[] buffer = Convert.FromBase64String(encryptedText);
                byte[] iv = new byte[16];

                Array.Copy(buffer, 0, iv, 0, iv.Length);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = Key;
                    aes.IV = iv;

                    using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                    using (var ms = new MemoryStream(buffer, iv.Length, buffer.Length - iv.Length))
                    using (var cryptoStream = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    using (var sr = new StreamReader(cryptoStream))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Decryption failed: {ex.Message}");
                return "DECRYPTION_ERROR";
            }
        }
    }
}
