using System.Security.Cryptography;
using System.Text;

namespace NTSkelbimuSistemaSaitynai.Security
{
    public static class PasswordHasher
    {
        // Format: PBKDF2$<iterations>$<salt-hex>$<hash-hex>
        private const string Prefix = "PBKDF2";
        private const int DefaultIterations = 100_000;
        private const int SaltSize = 16; // 128-bit
        private const int HashSize = 32; // 256-bit

        public static string Hash(string password)
        {
            using var rng = RandomNumberGenerator.Create();
            var salt = new byte[SaltSize];
            rng.GetBytes(salt);

            var hash = Rfc2898DeriveBytes.Pbkdf2(
                password: password,
                salt: salt,
                iterations: DefaultIterations,
                hashAlgorithm: HashAlgorithmName.SHA256,
                outputLength: HashSize);

            return string.Join('$', new[]
            {
                Prefix,
                DefaultIterations.ToString(),
                Convert.ToHexString(salt).ToLowerInvariant(),
                Convert.ToHexString(hash).ToLowerInvariant()
            });
        }

        public static bool Verify(string password, string stored)
        {
            if (string.IsNullOrWhiteSpace(stored)) return false;
            var parts = stored.Split('$');
            if (parts.Length != 4) return false;
            if (!string.Equals(parts[0], Prefix, StringComparison.Ordinal)) return false;
            if (!int.TryParse(parts[1], out var iterations)) return false;

            try
            {
                var salt = Convert.FromHexString(parts[2]);
                var hash = Convert.FromHexString(parts[3]);

                var computed = Rfc2898DeriveBytes.Pbkdf2(
                    password: password,
                    salt: salt,
                    iterations: iterations,
                    hashAlgorithm: HashAlgorithmName.SHA256,
                    outputLength: hash.Length);

                return CryptographicOperations.FixedTimeEquals(hash, computed);
            }
            catch
            {
                return false;
            }
        }

        public static bool IsHashed(string value)
        {
            if (string.IsNullOrEmpty(value)) return false;
            var parts = value.Split('$');
            if (parts.Length != 4) return false;
            if (!string.Equals(parts[0], Prefix, StringComparison.Ordinal)) return false;
            if (!int.TryParse(parts[1], out _)) return false;
            try
            {
                _ = Convert.FromHexString(parts[2]);
                _ = Convert.FromHexString(parts[3]);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
