using MiniTrading.Domain.Entities;

namespace MiniTrading.Application.Interfaces;

public interface ITradeRepository
{
    Task<Trade> AddAsync(Trade trade, CancellationToken cancellationToken = default);
    Task<IEnumerable<Trade>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<int> GetNextTradeNumberAsync(CancellationToken cancellationToken = default);
}