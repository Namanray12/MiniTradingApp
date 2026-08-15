using MiniTrading.Domain.Enums;

namespace MiniTrading.Application.DTOs;

public class TradeDto
{
    public string TradeId { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public TradeSide Side { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public DateTime Timestamp { get; set; }
    public TradeStatus Status { get; set; }
    public string Message { get; set; } = string.Empty;
}