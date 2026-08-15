using MiniTrading.Application.DTOs;

namespace MiniTrading.Application.Interfaces;

public interface IAuthService
{
    Task<AuthDto> GetAuthTokenAsync(CancellationToken cancellationToken = default);
}