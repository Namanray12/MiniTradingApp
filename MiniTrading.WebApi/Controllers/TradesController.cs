using Microsoft.AspNetCore.Mvc;
using MiniTrading.Application.Interfaces;

namespace MiniTrading.WebApi.Controllers;

[ApiController]
[Route("api/trades")]
public class TradesController : ControllerBase
{
    private readonly ITradingService _tradingService;

    public TradesController(ITradingService tradingService)
    {
        _tradingService = tradingService;
    }

    [HttpGet]
    public async Task<IActionResult> GetTrades(CancellationToken cancellationToken)
    {
        var trades = await _tradingService.GetTradeHistoryAsync(cancellationToken);
        return Ok(trades);
    }
}