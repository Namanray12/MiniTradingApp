using Microsoft.AspNetCore.Mvc;
using MiniTrading.Application.Interfaces;

namespace MiniTrading.WebApi.Controllers;

[ApiController]
[Route("api/prices")]
public class PricesController : ControllerBase
{
    private readonly IPriceCache _priceCache;

    public PricesController(IPriceCache priceCache)
    {
        _priceCache = priceCache;
    }

    [HttpGet]
    public IActionResult GetPrices()
    {
        var prices = _priceCache.GetAllPrices();
        return Ok(prices);
    }

    [HttpGet("{symbol}")]
    public IActionResult GetPrice(string symbol)
    {
        var price = _priceCache.GetPrice(symbol);
        if (price == null) return NotFound();
        return Ok(price);
    }
}