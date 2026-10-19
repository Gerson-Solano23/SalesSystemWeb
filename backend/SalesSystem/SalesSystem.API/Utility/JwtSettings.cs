using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace SalesSystem.API.Utility
{
    public class JwtSettings
    {
        public const string SectionName = "Jwt";
        private const int MinKeyLength = 32;

        /// <summary>Signing key. Read from Jwt:Key, falling back to the CERTIFICATE_KEY environment variable.</summary>
        public string Key { get; set; } = string.Empty;
        public string Issuer { get; set; } = "SalesSystem.API";
        public string Audience { get; set; } = "SalesSystemWeb";
        public double ExpirationHours { get; set; } = 8;

        public static JwtSettings FromConfiguration(IConfiguration configuration)
        {
            var settings = configuration.GetSection(SectionName).Get<JwtSettings>() ?? new JwtSettings();

            if (string.IsNullOrWhiteSpace(settings.Key))
            {
                settings.Key = Environment.GetEnvironmentVariable("CERTIFICATE_KEY") ?? string.Empty;
            }

            if (settings.Key.Length < MinKeyLength)
            {
                throw new InvalidOperationException(
                    $"JWT signing key is missing or shorter than {MinKeyLength} characters. " +
                    "Set the CERTIFICATE_KEY environment variable (or Jwt__Key).");
            }

            return settings;
        }

        public SymmetricSecurityKey GetSigningKey() => new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key));
    }
}
