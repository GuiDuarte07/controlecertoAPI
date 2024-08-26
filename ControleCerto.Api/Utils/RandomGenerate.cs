using Microsoft.AspNetCore.WebUtilities;
using System.Security.Cryptography;

namespace ControleCerto.Utils
{
    public static class RandomGenerate
    {
        public static string Generate32BytesToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return WebEncoders.Base64UrlEncode(randomNumber);
            }
        }
    }
}
