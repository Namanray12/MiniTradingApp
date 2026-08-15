using MiniTrading.Application.Dtos;
using MiniTrading.Application.DTOs;
using MiniTrading.Application.Interfaces;
using MiniTrading.Domain.Constants;
using System.Collections.Concurrent;

namespace MiniTrading.Application.Services;

public class PriceCache : IPriceCache
{
    private readonly ConcurrentDictionary<string, PriceTickDto> _prices = new(StringComparer.OrdinalIgnoreCase);
    private string _wsStatus = AppConstant.Messages.Disconnected;

    public void UpdatePrice(PriceTickDto tick)
    {
        var rawKey = tick.Symbol.ToUpperInvariant();
        var cleanKey = rawKey.Replace("/", string.Empty).Replace("-", string.Empty);

        _prices[rawKey] = tick;
        _prices[cleanKey] = tick;
    }

    public PriceTickDto? GetPrice(string symbol)
    {
        if (string.IsNullOrWhiteSpace(symbol)) return null;

        var rawKey = symbol.Trim().ToUpperInvariant();
        var cleanKey = rawKey.Replace("/", string.Empty).Replace("-", string.Empty);

        if (_prices.TryGetValue(rawKey, out var tick)) return tick;
        if (_prices.TryGetValue(cleanKey, out tick)) return tick;

        return null;
    }

    public IEnumerable<PriceTickDto> GetAllPrices()
    {
        return _prices.Values.DistinctBy(p => p.Symbol);
    }

    public void SetWebSocketStatus(string status)
    {
        _wsStatus = status;
    }

    public string GetWebSocketStatus()
    {
        return _wsStatus;
    }
}