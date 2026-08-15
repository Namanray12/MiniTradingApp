namespace MiniTrading.Application.DTOs;

public class PositionDto
{
    public string Symbol { get; set; } = string.Empty;
    public decimal NetQuantity { get; set; }
    public decimal AveragePrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal UnrealizedPnL { get; set; }
    public decimal UnrealizedPnLPercentage { get; set; }
}