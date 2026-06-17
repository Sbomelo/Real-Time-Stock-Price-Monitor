namespace Real_Time_Stock_Price_Monitor.Models;

public record DelayedPriceTick
{
    public required string Symbol { get; init; }
    public required decimal Price { get; init; }
    public DateTime Timestamp { get; init; }
    public DateTime DeliveredAt { get; init; }
}