using Real_Time_Stock_Price_Monitor.Models;

namespace Real_Time_Stock_Price_Monitor.Services;

public class StockPriceFeedService : BackgroundService     
{
    private readonly SubscriberStore _store;
    private readonly ILogger<StockPriceFeedService> _logger;
    private readonly Random _rng = new();

    // Simulated starting prices for demo symbols
    private readonly Dictionary<string, decimal> _basePrices = new()
    {
        ["AAPL"] = 189.50m,
        ["MSFT"] = 415.20m,
        ["GOOG"] = 175.40m,
        ["NVDA"] = 875.30m,
        ["TSLA"] = 215.60m
    };

    private Dictionary<string, decimal> _currentPrices;       

    public StockPriceFeedService(
        SubscriberStore store,
        ILogger<StockPriceFeedService> logger)
    {
        _store  = store;
        _logger = logger;
        _currentPrices = new Dictionary<string, decimal>(_basePrices);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("StockPriceFeedService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                GenerateAndBroadcastTicks();                  
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating price ticks");
            }

            await Task.Delay(500, stoppingToken);           
        }
    }

    private void GenerateAndBroadcastTicks()                
    {
        // Only generate ticks for symbols that have subscribers
        foreach (var symbol in _store.GetWatchedSymbols())    
        {
            var newPrice = SimulateNewPrice(symbol);         
            var oldPrice = _currentPrices.GetValueOrDefault(symbol, newPrice);
            _currentPrices[symbol] = newPrice;

            var tick = new StockPriceTick                 
            {
                Symbol    = symbol,
                Price     = newPrice,
                Bid       = newPrice - 0.01m,
                Ask       = newPrice + 0.01m,
                Volume    = Random.Shared.NextInt64(1000, 50000),
                Change    = Math.Round(newPrice - _basePrices[symbol], 2),
                ChangePct = Math.Round((newPrice - _basePrices[symbol])
                              / _basePrices[symbol] * 100m, 4),
                Timestamp = DateTime.UtcNow
            };

            _store.BroadcastTick(tick);                
        }
    }

    private decimal SimulateNewPrice(string symbol)        
    {
        var current    = _currentPrices.GetValueOrDefault(symbol, 100m);
        
        var changePct  = (decimal)(_rng.NextDouble() * 0.005 - 0.0025);  
        var newPrice   = Math.Max(1m, current * (1 + changePct));
        return Math.Round(newPrice, 2);
    }
}