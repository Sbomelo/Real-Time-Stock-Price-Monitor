using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Real_Time_Stock_Price_Monitor.Models;

namespace Real_Time_Stock_Price_Monitor.Services;
    public class TokenService
    {
        private static readonly Dictionary<string,(string Password, string Role, string Name)> 
        _users = new()
        {
            ["admin"]  = ("admin123",  "Admin",  "Sibonelo"),
            ["alice"]  = ("alice123",  "Trader", "Mduduzi"),
            ["bob"]    = ("bob123",    "Trader", "Elethu"),
            ["viewer"] = ("viewer123", "Viewer", "Lumiyo")
        };

        private const string _secret = "meridian-capital-jwt-signing-secret-key-must-be-32-chars";

        public LoginResponse? Authenticate(LoginRequest request)
        {
            if(!_users.TryGetValue(request.Username.ToLowerInvariant(), out var user))
                return null;


            if(user.Password != request.Password)
                return null;


            var token = GenerateJwt(request.Username, user.Name, user.Role);
            return new LoginResponse(token, user.Role, user.Name);

        }

        public string GenerateJwt(string username, string name, string role)
        {
            //Building the singning key from the _secret
            var key = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(_secret));


            //Signing the key 
            var creds = new SigningCredentials(key,
                            SecurityAlgorithms.HmacSha256);


            //Defining Claims
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, username),
                new Claim(ClaimTypes.Name, name),
                new Claim(ClaimTypes.Role, role)
            };


            //Setting up the JWT
            var token = new JwtSecurityToken(
                issuer : "merdian-capital",
                audience : "meridaian-capital",
                claims : claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials : creds
            );

            //Serialize the JwtSecurityToken object into the standard JWT string format
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }