namespace Real_Time_Stock_Price_Monitor.Models;

public record StockPriceTick
{
    public required string Symbol { get; init; }
    public required decimal Price { get; init; }
    public  required decimal Bid { get; init; }
    public required decimal Ask { get; init; }
    public required long Volume { get; init; }
    public required decimal Change { get; init; }
    public required decimal ChangePct { get; init; }
    public DateTime Timestamp { get; init; }
}