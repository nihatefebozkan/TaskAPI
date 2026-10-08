using System.Security.Cryptography;
using System.Text;
using TaskAPI.Application.Interfaces;

namespace TaskAPI.Infrastructure.Security
{
    public class PasswordHasher(IPepperKeyProvider pepperKeyProvider) : IPasswordHasher
    {
        public const string Version = "v1";
        public const int Iterations = 600_000;
        public const int SaltSize = 16;
        public const int HashSize = 32;

        public string Hash(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

            byte[] pepperedPassword = PepperPassword(password);

            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                pepperedPassword,
                salt,
                Iterations,
                HashAlgorithmName.SHA256,
                HashSize);

            return $"{Version}.{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        public bool Verify(string password, string storedHash)
        {
            var parts = storedHash.Split('.');
            if (parts.Length != 4 || parts[0] != Version)
            {
                return false;
            }

            int iterations = int.Parse(parts[1]);
            byte[] salt = Convert.FromBase64String(parts[2]);
            byte[] expectedHash = Convert.FromBase64String(parts[3]);

            byte[] pepperedPassword = PepperPassword(password);

            byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(
                pepperedPassword,
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                expectedHash.Length);

            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
        private byte[] PepperPassword(string password)
        {
            return HMACSHA256.HashData(
                pepperKeyProvider.GetKey("key"),
                Encoding.UTF8.GetBytes(password));
        }
    }
}