using System.Runtime.CompilerServices;         
using Microsoft.AspNetCore.Authorization;                   
using Microsoft.AspNetCore.SignalR;
using Real_Time_Stock_Price_Monitor.Models;
using Real_Time_Stock_Price_Monitor.Services;
using System.Threading.Channels;

namespace Real_Time_Stock_Price_Monitor.Hubs;

[Authorize]
public class StockHub : Hub
{
    private readonly SubscriberStore _store;
    private readonly ILogger<StockHub> _logger;

    public StockHub(SubscriberStore store, ILogger<StockHub> logger)
    {
        _store = store;
        _logger = logger;
    }

    [Authorize(Roles = "Admin,Trader")]
    public async IAsyncEnumerable<StockPriceTick> StreamPrice(string symbol, 
                                        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        symbol = symbol.Trim().ToUpperInvariant();
        string connId = Context.ConnectionId;
        string userName = Context.User?.Identity?.Name?? "Unknown";

        _logger.LogInformation("Trader stream Started: {User} => {Symbol}", userName, symbol);


        //Register Subscriber and get their dedicated channel
        var channel = _store.Subscribe(symbol, connId);

        try
        {
            await foreach(var tick in channel.Reader.ReadAllAsync(cancellationToken))
            {
                yield return tick;
            }
        }
        finally
        {
            _store.Unsubscribe(symbol, connId);
            _logger.LogInformation(
                "Trader stream ended: {User} → {Symbol}", userName, symbol);
        }

    }

    [Authorize(Roles = "Admin,Trader,Viewer")]
    public async IAsyncEnumerable<DelayedPriceTick> StreamDelayedPrice(string symbol,
                                     [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        symbol = symbol.Trim().ToUpperInvariant();
        string connId = Context.ConnectionId;

        var channel = _store.Subscribe(symbol, connId + "-delayed");

        try
        {
            await Task.Delay(TimeSpan.FromSeconds(15), cancellationToken);

            await foreach (var tick in channel.Reader.ReadAllAsync(cancellationToken))
            {
                yield return new DelayedPriceTick
                {
                    Symbol      = tick.Symbol,
                    Price       = tick.Price,
                    Timestamp   = tick.Timestamp,
                    DeliveredAt = DateTime.UtcNow
                };

                await Task.Delay(TimeSpan.FromSeconds(15), cancellationToken);
            }
        }
        finally
        {
            _store.Unsubscribe(symbol, connId + "-delayed");
        }
    }

    public async Task ControlFeed(string command, string message)
    {
        string adminName = Context.User?.Identity?.Name ?? "Admin";

        await Clients.All.SendAsync("FeedControlEvent", command, message, adminName);
    }

    public override async Task OnConnectedAsync()            
    {
        string userName = Context.User?.Identity?.Name ?? "unknown";
        string role     = Context.User?.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value ?? "";

        _logger.LogInformation(
            "Connected: {User} [{Role}] → {ConnId}",
            userName, role, Context.ConnectionId);

    
        await Clients.Caller.SendAsync("Connected",    
            Context.ConnectionId, userName, role);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _store.UnsubscribeAll(Context.ConnectionId);            // Line 22
        await base.OnDisconnectedAsync(exception);
    }
}
