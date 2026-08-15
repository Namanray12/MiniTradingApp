using Microsoft.AspNetCore.SignalR;
using MiniTrading.Application.Dtos;
using MiniTrading.Application.DTOs;
using MiniTrading.Application.Interfaces;
using MiniTrading.Domain.Constants;

namespace MiniTrading.Application.Services;

public class SignalRPricePublisher : IPricePublisher
{
    private readonly IHubContext<TradingHubWrapper> _hubContext;

    public SignalRPricePublisher(IHubContext<TradingHubWrapper> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task PublishPriceAsync(PriceTickDto tick, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.All.SendAsync(AppConstant.SignalR.ReceivePriceUpdateMethod, tick, cancellationToken);
    }

    public async Task PublishTradeAsync(TradeDto trade, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.All.SendAsync(AppConstant.SignalR.ReceiveTradeUpdateMethod, trade, cancellationToken);
    }
}

public class TradingHubWrapper : Hub
{
}