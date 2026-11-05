using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Real_Time_Stock_Price_Monitor.Services
{
    public class JwtSettings
    {
        public const string Issuer = "meridian-capital";
        public const string Audience = "meridian-capital";

        public static SymmetricSecurityKey GetSigningKey(IConfiguration config)
        {
            var secret = config["Jwt:Secret"];

            if (string.IsNullOrWhiteSpace(secret) || Encoding.UTF8.GetByteCount(secret) < 32)
                throw new InvalidOperationException(
                    "Jwt:Secret is missing or shorter than 32 bytes. " +
                    "In production set the Jwt__Secret environment variable; " +
                    "for local runs it comes from appsettings.Development.json.");

            return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        }
    }
}
