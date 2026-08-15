namespace MiniTrading.Application.Dtos;

public class HealthStatusDto
{
    public string Status { get; set; } = string.Empty;
    public string WebSocketStatus { get; set; } = string.Empty;
    public DateTime ServerTime { get; set; }
    public int CachedSymbolsCount { get; set; }
}