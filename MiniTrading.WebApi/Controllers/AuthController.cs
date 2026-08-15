using Microsoft.AspNetCore.Mvc;
using MiniTrading.Application.DTOs;
using MiniTrading.Application.Interfaces;

namespace MiniTrading.WebApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService )
    {
        _authService = authService;
    }

    [HttpPost("token")]
    public async Task<IActionResult> GetToken(CancellationToken cancellationToken)
    {
        var result = await _authService.GetAuthTokenAsync(cancellationToken);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }
}