using System.Security.Cryptography;

namespace SecurePasswordManager.Authentication
{
    public class MasterPasswordService
    {
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int Iterations = 100_000;

        public byte[] GenerateSalt()
        {
            return RandomNumberGenerator.GetBytes(SaltSize);
        }

        public byte[] DeriveKey(string password, byte[] salt)
        {
            return Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA256,
                KeySize
            );
        }

        public bool VerifyPassword(
            string password,
            byte[] salt,
            byte[] expectedKey)
        {
            byte[] derivedKey = DeriveKey(password, salt);

            return CryptographicOperations.FixedTimeEquals(
                derivedKey,
                expectedKey
            );
        }
    }
}