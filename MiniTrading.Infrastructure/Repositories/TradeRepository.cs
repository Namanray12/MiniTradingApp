using Microsoft.EntityFrameworkCore;
using MiniTrading.Application.Interfaces;
using MiniTrading.Domain.Entities;

namespace MiniTrading.Infrastructure.Persistence.Repositories;

public class TradeRepository : ITradeRepository
{
    private readonly ApplicationDbContext _context;

    public TradeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Trade> AddAsync(Trade trade, CancellationToken cancellationToken = default)
    {
        await _context.Trades.AddAsync(trade, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return trade;
    }

    public async Task<IEnumerable<Trade>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Trades
            .OrderByDescending(t => t.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetNextTradeNumberAsync(CancellationToken cancellationToken = default)
    {
        var count = await _context.Trades.CountAsync(cancellationToken);
        return count + 1;
    }
}