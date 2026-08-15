using MiniTrading.Application.Dtos;
using MiniTrading.Application.DTOs;

namespace MiniTrading.Application.Interfaces;

public interface ITradingService
{
    Task<TradeDto> PlaceOrderAsync(TradeDto tradeDto, CancellationToken cancellationToken = default);
    Task<IEnumerable<TradeDto>> GetTradeHistoryAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<PositionDto>> GetPositionsAsync(CancellationToken cancellationToken = default);
    Task<HealthStatusDto> GetHealthStatusAsync(CancellationToken cancellationToken = default);
}