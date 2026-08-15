using Microsoft.AspNetCore.Mvc;
using MiniTrading.Application.Interfaces;

namespace MiniTrading.WebApi.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    private readonly ITradingService _tradingService;

    public HealthController(ITradingService tradingService)
    {
        _tradingService = tradingService;
    }

    [HttpGet]
    public async Task<IActionResult> GetHealth(CancellationToken cancellationToken)
    {
        var health = await _tradingService.GetHealthStatusAsync(cancellationToken);
        return Ok(health);
    }
}