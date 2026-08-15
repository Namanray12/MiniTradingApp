using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MiniTrading.Application.Dtos;
using MiniTrading.Application.DTOs;
using MiniTrading.Application.Interfaces;
using MiniTrading.Domain.Constants;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace MiniTrading.Application.Services;

public class WebSocketPriceStreamService : BackgroundService
{
    private readonly IAuthService _authService;
    private readonly IPriceCache _priceCache;
    private readonly IPricePublisher _pricePublisher;
    private readonly IConfiguration _configuration;

    public WebSocketPriceStreamService(
        IAuthService authService,
        IPriceCache priceCache,
        IPricePublisher pricePublisher,
        IConfiguration configuration)
    {
        _authService = authService;
        _priceCache = priceCache;
        _pricePublisher = pricePublisher;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _priceCache.SetWebSocketStatus(AppConstant.Messages.Connecting);

                var authResult = await _authService.GetAuthTokenAsync(stoppingToken);

                if (!authResult.IsSuccess || string.IsNullOrEmpty(authResult.Token))
                {
                    _priceCache.SetWebSocketStatus(AppConstant.Messages.Error);
                    await Task.Delay(AppConstant.WebSocketFeed.ReconnectDelayMs, stoppingToken);
                    continue;
                }

                await FetchInitialMarketSymbolsAsync(authResult.Token, stoppingToken);

                var baseWsUrl = _configuration[AppConstant.ConfigurationKeys.ActTraderWebSocketUrl] ?? AppConstant.WebSocketFeed.BaseEndpointUrl;
                var wsUrl = $"{baseWsUrl}{Uri.EscapeDataString(authResult.Token)}";

                using var ws = new ClientWebSocket();
                await ws.ConnectAsync(new Uri(wsUrl), stoppingToken);

                if (ws.State == WebSocketState.Open)
                {
                    _priceCache.SetWebSocketStatus(AppConstant.Messages.Connected);

                    var buffer = new byte[AppConstant.WebSocketFeed.BufferSize];

                    while (ws.State == WebSocketState.Open && !stoppingToken.IsCancellationRequested)
                    {
                        var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), stoppingToken);
                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, stoppingToken);
                            _priceCache.SetWebSocketStatus(AppConstant.Messages.Disconnected);
                            break;
                        }

                        var jsonString = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        ProcessPriceMessage(jsonString);
                    }
                }
            }
            catch (Exception)
            {
                _priceCache.SetWebSocketStatus(AppConstant.Messages.Error);
            }

            if (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(AppConstant.WebSocketFeed.ReconnectDelayMs, stoppingToken);
            }
        }
    }

    private async Task FetchInitialMarketSymbolsAsync(string token, CancellationToken cancellationToken)
    {
        try
        {
            using var client = new HttpClient();
            var url = $"{AppConstant.RestMarket.BaseSymbolsUrl}{Uri.EscapeDataString(token)}";
            var response = await client.GetAsync(url, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                ProcessPriceMessage(content);
            }
        }
        catch (Exception)
        {
        }
    }

    private void ProcessPriceMessage(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in root.EnumerateArray())
                {
                    ParseAndCacheSingleTick(item);
                }
            }
            else if (root.ValueKind == JsonValueKind.Object)
            {
                if (root.TryGetProperty(AppConstant.WebSocketFeed.ResultProperty, out var resProp) && resProp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in resProp.EnumerateArray()) ParseAndCacheSingleTick(item);
                }
                else if (root.TryGetProperty(AppConstant.WebSocketFeed.DataProperty, out var dataProp))
                {
                    if (dataProp.ValueKind == JsonValueKind.Array)
                        foreach (var item in dataProp.EnumerateArray()) ParseAndCacheSingleTick(item);
                    else if (dataProp.ValueKind == JsonValueKind.Object)
                        ParseAndCacheSingleTick(dataProp);
                }
                else
                {
                    ParseAndCacheSingleTick(root);
                }
            }
        }
        catch (Exception)
        {
        }
    }

    private void ParseAndCacheSingleTick(JsonElement element)
    {
        string symbol = GetStringProperty(element, AppConstant.WebSocketFeed.SymbolAliases);
        if (string.IsNullOrEmpty(symbol)) return;

        decimal buy = GetDecimalProperty(element, AppConstant.WebSocketFeed.BuyAliases);
        decimal sell = GetDecimalProperty(element, AppConstant.WebSocketFeed.SellAliases);
        decimal lastPrice = GetDecimalProperty(element, AppConstant.WebSocketFeed.LastPriceAliases);
        decimal changePct = GetDecimalProperty(element, AppConstant.WebSocketFeed.ChangePercentAliases);
        decimal high = GetDecimalProperty(element, AppConstant.WebSocketFeed.HighAliases);
        decimal low = GetDecimalProperty(element, AppConstant.WebSocketFeed.LowAliases);

        decimal ask = buy > AppConstant.Calculations.Zero ? buy : lastPrice;
        decimal bid = sell > AppConstant.Calculations.Zero ? sell : lastPrice;

        if (lastPrice <= AppConstant.Calculations.Zero && ask > AppConstant.Calculations.Zero && bid > AppConstant.Calculations.Zero)
        {
            lastPrice = (ask + bid) / AppConstant.Calculations.Two;
        }

        var tick = new PriceTickDto
        {
            Symbol = symbol.ToUpperInvariant(),
            Bid = bid,
            Ask = ask,
            LastPrice = lastPrice > AppConstant.Calculations.Zero ? lastPrice : (ask > AppConstant.Calculations.Zero ? ask : bid),
            ChangePercentage = changePct,
            High = high > AppConstant.Calculations.Zero ? high : ask,
            Low = low > AppConstant.Calculations.Zero ? low : bid,
            Timestamp = DateTime.UtcNow
        };

        if (tick.LastPrice > AppConstant.Calculations.Zero || tick.Bid > AppConstant.Calculations.Zero || tick.Ask > AppConstant.Calculations.Zero)
        {
            _priceCache.UpdatePrice(tick);
            _ = _pricePublisher.PublishPriceAsync(tick);
        }
    }

    private string GetStringProperty(JsonElement element, string[] propNames)
    {
        foreach (var name in propNames)
        {
            if (element.TryGetProperty(name, out var p))
            {
                var val = p.GetString();
                if (!string.IsNullOrEmpty(val)) return val;
            }
        }
        return string.Empty;
    }

    private decimal GetDecimalProperty(JsonElement element, string[] propNames)
    {
        foreach (var name in propNames)
        {
            if (element.TryGetProperty(name, out var p))
            {
                if (p.ValueKind == JsonValueKind.Number && p.TryGetDecimal(out var val)) return val;
                if (p.ValueKind == JsonValueKind.String && decimal.TryParse(p.GetString(), out var strVal)) return strVal;
            }
        }
        return AppConstant.Calculations.Zero;
    }
}