namespace MiniTrading.Application.DTOs;

public class PriceTickDto
{
    public string Symbol { get; set; } = string.Empty;
    public decimal Bid { get; set; }
    public decimal Ask { get; set; }
    public decimal LastPrice { get; set; }
    public decimal ChangePercentage { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public DateTime Timestamp { get; set; }
}