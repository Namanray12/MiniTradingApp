using Microsoft.AspNetCore.Mvc;
using MiniTrading.Application.Interfaces;

namespace MiniTrading.WebApi.Controllers;

[ApiController]
[Route("api/positions")]
public class PositionsController : ControllerBase
{
    private readonly ITradingService _tradingService;

    public PositionsController(ITradingService tradingService)
    {
        _tradingService = tradingService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPositions(CancellationToken cancellationToken)
    {
        var positions = await _tradingService.GetPositionsAsync(cancellationToken);
        return Ok(positions);
    }
}