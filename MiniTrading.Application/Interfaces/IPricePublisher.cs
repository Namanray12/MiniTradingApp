using MiniTrading.Application.DTOs;

namespace MiniTrading.Application.Interfaces;

public interface IPricePublisher
{
    Task PublishPriceAsync(PriceTickDto tick, CancellationToken cancellationToken = default);
    Task PublishTradeAsync(TradeDto trade, CancellationToken cancellationToken = default);
}