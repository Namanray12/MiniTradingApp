using MiniTrading.Application.Dtos;
using MiniTrading.Application.DTOs;
using MiniTrading.Application.Interfaces;
using MiniTrading.Domain.Constants;
using MiniTrading.Domain.Entities;
using MiniTrading.Domain.Enums;

namespace MiniTrading.Application.Services;

public class TradingService : ITradingService
{
    private readonly ITradeRepository _tradeRepository;
    private readonly IPriceCache _priceCache;
    private readonly IPricePublisher _pricePublisher;

    public TradingService(
        ITradeRepository tradeRepository,
        IPriceCache priceCache,
        IPricePublisher pricePublisher)
    {
        _tradeRepository = tradeRepository;
        _priceCache = priceCache;
        _pricePublisher = pricePublisher;
    }

    public async Task<TradeDto> PlaceOrderAsync(TradeDto tradeDto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tradeDto.Symbol))
        {
            tradeDto.Status = TradeStatus.Rejected;
            tradeDto.Message = AppConstant.Messages.InvalidSymbol;
            return tradeDto;
        }

        if (tradeDto.Quantity <= AppConstant.Calculations.Zero)
        {
            tradeDto.Status = TradeStatus.Rejected;
            tradeDto.Message = AppConstant.Messages.InvalidQuantity;
            return tradeDto;
        }

        var normalizedSymbol = tradeDto.Symbol.ToUpperInvariant();
        var currentTick = _priceCache.GetPrice(normalizedSymbol);

        if (currentTick == null)
        {
            tradeDto.Status = TradeStatus.Rejected;
            tradeDto.Message = AppConstant.Messages.PriceUnavailable;
            return tradeDto;
        }

        var executionPrice = tradeDto.Side == TradeSide.Buy ? currentTick.Ask : currentTick.Bid;
        if (executionPrice <= AppConstant.Calculations.Zero)
        {
            executionPrice = currentTick.LastPrice;
        }

        var nextNumber = await _tradeRepository.GetNextTradeNumberAsync(cancellationToken);
        var tradeId = $"{AppConstant.TradeRules.TradeIdPrefix}{nextNumber.ToString(AppConstant.TradeRules.TradeIdFormat)}";

        var trade = new Trade
        {
            TradeId = tradeId,
            Symbol = normalizedSymbol,
            Side = tradeDto.Side,
            Quantity = tradeDto.Quantity,
            Price = executionPrice,
            Timestamp = DateTime.UtcNow,
            Status = TradeStatus.Filled
        };

        await _tradeRepository.AddAsync(trade, cancellationToken);

        tradeDto.TradeId = trade.TradeId;
        tradeDto.Symbol = trade.Symbol;
        tradeDto.Price = trade.Price;
        tradeDto.Timestamp = trade.Timestamp;
        tradeDto.Status = trade.Status;
        tradeDto.Message = AppConstant.Messages.OrderFilledSuccessfully;

        await _pricePublisher.PublishTradeAsync(tradeDto, cancellationToken);

        return tradeDto;
    }

    public async Task<IEnumerable<TradeDto>> GetTradeHistoryAsync(CancellationToken cancellationToken = default)
    {
        var trades = await _tradeRepository.GetAllAsync(cancellationToken);
        return trades.Select(t => new TradeDto
        {
            TradeId = t.TradeId,
            Symbol = t.Symbol,
            Side = t.Side,
            Quantity = t.Quantity,
            Price = t.Price,
            Timestamp = t.Timestamp,
            Status = t.Status,
            Message = AppConstant.Messages.OrderFilledSuccessfully
        });
    }

    public async Task<IEnumerable<PositionDto>> GetPositionsAsync(CancellationToken cancellationToken = default)
    {
        var trades = await _tradeRepository.GetAllAsync(cancellationToken);
        var filledTrades = trades.Where(t => t.Status == TradeStatus.Filled).ToList();

        var positions = new List<PositionDto>();
        var groupedBySymbol = filledTrades.GroupBy(t => t.Symbol);

        foreach (var group in groupedBySymbol)
        {
            var symbol = group.Key;
            decimal netQuantity = AppConstant.Calculations.Zero;
            decimal totalBuyCost = AppConstant.Calculations.Zero;
            decimal totalBuyQuantity = AppConstant.Calculations.Zero;

            foreach (var trade in group)
            {
                if (trade.Side == TradeSide.Buy)
                {
                    netQuantity += trade.Quantity;
                    totalBuyCost += (trade.Price * trade.Quantity);
                    totalBuyQuantity += trade.Quantity;
                }
                else if (trade.Side == TradeSide.Sell)
                {
                    netQuantity -= trade.Quantity;
                }
            }

            var avgPrice = totalBuyQuantity > AppConstant.Calculations.Zero
                ? totalBuyCost / totalBuyQuantity
                : AppConstant.Calculations.Zero;

            var currentTick = _priceCache.GetPrice(symbol);
            var currentPrice = currentTick?.LastPrice ?? avgPrice;

            var unrealizedPnL = (currentPrice - avgPrice) * netQuantity;
            var unrealizedPnLPercent = avgPrice > AppConstant.Calculations.Zero
                ? (unrealizedPnL / (avgPrice * Math.Abs(netQuantity))) * AppConstant.Calculations.OneHundred
                : AppConstant.Calculations.Zero;

            positions.Add(new PositionDto
            {
                Symbol = symbol,
                NetQuantity = netQuantity,
                AveragePrice = Math.Round(avgPrice, 5),
                CurrentPrice = Math.Round(currentPrice, 5),
                UnrealizedPnL = Math.Round(unrealizedPnL, 2),
                UnrealizedPnLPercentage = Math.Round(unrealizedPnLPercent, 2)
            });
        }

        return positions;
    }

    public Task<HealthStatusDto> GetHealthStatusAsync(CancellationToken cancellationToken = default)
    {
        var wsStatus = _priceCache.GetWebSocketStatus();
        var cachedCount = _priceCache.GetAllPrices().Count();

        var dto = new HealthStatusDto
        {
            Status = wsStatus == AppConstant.Messages.Connected ? AppConstant.Messages.SystemHealthy : AppConstant.Messages.SystemDegraded,
            WebSocketStatus = wsStatus,
            ServerTime = DateTime.UtcNow,
            CachedSymbolsCount = cachedCount
        };

        return Task.FromResult(dto);
    }
}