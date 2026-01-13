using System.Security.Cryptography;

namespace Application.Common
{
    public static class TokenGenerator
    {
        public static string GenerateResumeToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(32);

            return Convert.ToBase64String(bytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .TrimEnd('=');
        }
    }
}