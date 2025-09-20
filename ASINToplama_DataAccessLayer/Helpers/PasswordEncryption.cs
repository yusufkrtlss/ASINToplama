using System.Security.Cryptography;

namespace ASINToplama_DataAccessLayer.Helpers
{
    public static class PasswordEncryption
    {
        public static string Hash(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(16); // Güvenli rastgele salt oluştur

            // Yeni constructor ile SHA256 algoritmasını ve iterasyon sayısını belirt
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(32); // 256-bit hash (32 byte)

            byte[] hashBytes = new byte[salt.Length + hash.Length];
            Array.Copy(salt, 0, hashBytes, 0, salt.Length);
            Array.Copy(hash, 0, hashBytes, salt.Length, hash.Length);

            return Convert.ToBase64String(hashBytes);
        }

        public static bool CheckHashed(string hashedPassword, string password)
        {
            byte[] storedHashBytes = Convert.FromBase64String(hashedPassword);
            byte[] salt = new byte[16];
            Array.Copy(storedHashBytes, 0, salt, 0, 16);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(32);

            for (int i = 0; i < hash.Length; i++)
            {
                if (storedHashBytes[i + 16] != hash[i])
                    return false;
            }
            return true;
        }

        public static string GenerateRandom(int length = 16)
        {
            var randomBytes = new byte[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes).Substring(0, length);
        }
    }
}
