using Microsoft.AspNetCore.Identity;
using Real_Time_Stock_Price_Monitor.Models;

namespace Real_Time_Stock_Price_Monitor.Data;

public static class DbSeeder
{
    private static readonly (string Username, string DisplayName, string Role)[] DemoUsers =
    {
        ("admin",  "Sibonelo", "Admin"),
        ("alice",  "Alice",    "Trader"),
        ("bob",    "Bob",      "Trader"),
        ("viewer", "Lumiyo",   "Viewer")
    };

    // Runs only when the Users table is empty. Override a demo password with an
    // env var like Seed__Passwords__admin; fallback is "<username>123".
    public static void SeedDemoUsers(AppDbContext db, IConfiguration config)
    {
        if (db.Users.Any()) return;

        var hasher = new PasswordHasher<AppUser>();

        foreach (var (username, displayName, role) in DemoUsers)
        {
            var password = config[$"Seed:Passwords:{username}"];
            if (string.IsNullOrWhiteSpace(password))
                password = $"{username}123";

            var user = new AppUser { Username = username, DisplayName = displayName, Role = role };
            user.PasswordHash = hasher.HashPassword(user, password);
            db.Users.Add(user);
        }

        db.SaveChanges();
    }
}
