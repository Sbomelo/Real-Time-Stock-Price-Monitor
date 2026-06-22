using System.Collections.Concurrent;
using System.Threading.Channels;
using Microsoft.AspNetCore.Authentication;
using Real_Time_Stock_Price_Monitor.Models;

namespace Real_Time_Stock_Price_Monitor.Services;
public class SubscriberStore
{
    //Outer key: Symbol, InnerKey: ConnectionId, Value a channel
    private readonly ConcurrentDictionary<string, 
                    ConcurrentDictionary<string, Channel<StockPriceTick>>> 
                    _channels = new();


    public Channel<StockPriceTick> Subscribe(string symbol, string connectionId)
    {
        var channel = Channel.CreateBounded<StockPriceTick>(
            new BoundedChannelOptions(100)
        {
            FullMode = BoundedChannelFullMode.DropOldest
        });

        var symbolDict = _channels.GetOrAdd(symbol.ToUpperInvariant(),
                       _ => new ConcurrentDictionary<string, Channel<StockPriceTick>>());

        symbolDict[connectionId] = channel;
        return channel;
    }

    public void Unsubscribe(string symbol, string connectionId)
    {
        if(_channels.TryGetValue(symbol.ToUpperInvariant(), out var dict))
        {
            if(dict.TryRemove(connectionId, out var ch))
            {
                ch.Writer.TryComplete();
            }
        }
    }

    public void BroadcastTick(StockPriceTick tick)
    {
        if(!_channels.TryGetValue(tick.Symbol, out var dict))
            return;
        
        foreach (var kv in dict)
        {
            kv.Value.Writer.TryWrite(tick);//TryWrite non blocking if channel full
        }
    }

    public void UnsubscribeAll(string connectionId)
    {
        foreach (var dict in _channels.Values)
        {
            if(dict.TryRemove(connectionId, out var ch))
                ch.Writer.TryComplete();
        }
    }

    public IEnumerable<string> GetWatchedSymbols()
    {
        return _channels.Where(kv => !kv.Value.IsEmpty)
                        .Select(kv => kv.Key);
    }
}