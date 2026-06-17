namespace Real_Time_Stock_Price_Monitor.Models;

public record LoginRequest (string Username, string Password);
public record LoginResponse (string Token, string Role, string Name);
    