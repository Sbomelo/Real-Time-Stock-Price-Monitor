using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Real_Time_Stock_Price_Monitor.Data;
using Real_Time_Stock_Price_Monitor.Models;

namespace Real_Time_Stock_Price_Monitor.Services;

public class TokenService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;
    private readonly PasswordHasher<AppUser> _hasher = new();

    public TokenService(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public async Task<LoginResponse?> AuthenticateAsync(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrEmpty(request.Password))
            return null;

        var username = request.Username.Trim().ToLowerInvariant();
        var user = await _db.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Username == username);
        if (user is null) return null;

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed) return null;

        var token = GenerateJwt(user.Username, user.DisplayName, user.Role);
        return new LoginResponse(token, user.Role, user.DisplayName);
    }

    public string GenerateJwt(string username, string name, string role)
    {
        var key = JwtSettings.GetSigningKey(_config);
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, username),
            new Claim(ClaimTypes.Name, name),
            new Claim(ClaimTypes.Role, role)
        };

        var token = new JwtSecurityToken(
            issuer: JwtSettings.Issuer,
            audience: JwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}