using MiniTrading.Application.DTOs;

namespace MiniTrading.Application.Interfaces;

public interface IPriceCache
{
    void UpdatePrice(PriceTickDto tick);
    PriceTickDto? GetPrice(string symbol);
    IEnumerable<PriceTickDto> GetAllPrices();
    void SetWebSocketStatus(string status);
    string GetWebSocketStatus();
}