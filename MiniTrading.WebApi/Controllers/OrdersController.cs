using Microsoft.AspNetCore.Mvc;
using MiniTrading.Application.Dtos;
using MiniTrading.Application.DTOs;
using MiniTrading.Application.Interfaces;
using MiniTrading.Domain.Enums;

namespace MiniTrading.WebApi.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly ITradingService _tradingService;

    public OrdersController(ITradingService tradingService)
    {
        _tradingService = tradingService;
    }

    [HttpPost]
    public async Task<IActionResult> PlaceOrder([FromBody] TradeDto tradeDto, CancellationToken cancellationToken)
    {
        var result = await _tradingService.PlaceOrderAsync(tradeDto, cancellationToken);
        if (result.Status == TradeStatus.Rejected) return BadRequest(result);
        return Ok(result);
    }
}