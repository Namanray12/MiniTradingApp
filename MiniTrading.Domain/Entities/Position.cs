namespace MiniTrading.Domain.Entities;

public class Position
{
    public string Symbol { get; set; } = string.Empty;
    public decimal NetQuantity { get; set; }
    public decimal AveragePrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal UnrealizedPnL { get; set; }
    public decimal UnrealizedPnLPercentage { get; set; }
}