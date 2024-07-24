using System.Security.Cryptography;

namespace Finantech.Utils
{
    public static class RandomGenerate
    {
        public static string Generate32BytesToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
    }
}
