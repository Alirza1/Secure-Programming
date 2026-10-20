using System.Security.Cryptography;
using System.Text;

namespace SecurePasswordManager.Encryption
{
    public class EncryptionService
    {
        private const int NonceSize = 12;
        private const int TagSize = 16;

        public string Encrypt(string plaintext, byte[] key)
        {
            byte[] nonce = RandomNumberGenerator.GetBytes(NonceSize);
            byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
            byte[] ciphertext = new byte[plaintextBytes.Length];
            byte[] tag = new byte[TagSize];

            using var aes = new AesGcm(key, TagSize);

            aes.Encrypt(
                nonce,
                plaintextBytes,
                ciphertext,
                tag
            );

            string nonceBase64 = Convert.ToBase64String(nonce);
            string ciphertextBase64 = Convert.ToBase64String(ciphertext);
            string tagBase64 = Convert.ToBase64String(tag);

            return $"{nonceBase64}:{ciphertextBase64}:{tagBase64}";
        }

        public string Decrypt(string encryptedData, byte[] key)
        {
            string[] parts = encryptedData.Split(':');

            if (parts.Length != 3)
            {
                throw new CryptographicException("Invalid encrypted data.");
            }

            byte[] nonce = Convert.FromBase64String(parts[0]);
            byte[] ciphertext = Convert.FromBase64String(parts[1]);
            byte[] tag = Convert.FromBase64String(parts[2]);

            byte[] plaintextBytes = new byte[ciphertext.Length];

            using var aes = new AesGcm(key, TagSize);

            aes.Decrypt(
                nonce,
                ciphertext,
                tag,
                plaintextBytes
            );

            return Encoding.UTF8.GetString(plaintextBytes);
        }
    }
}