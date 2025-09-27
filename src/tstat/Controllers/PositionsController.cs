using Microsoft.AspNetCore.Mvc;
using tstat.Models;
using tstat.Services;

namespace tstat.Controllers;

public class PositionsController : Controller
{
    private readonly IConfiguration _configuration;

    public PositionsController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<IActionResult> Index()
    {
        var model = new PositionsPageViewModel();
        
        var token = _configuration["TinkoffToken"];
        if (string.IsNullOrEmpty(token))
        {
            ViewBag.Error = "Токен Тинькофф не настроен";
            return View(model);
        }

        try
        {
            var service = new TinkoffService(token);
            model.Accounts = await service.GetAccountsAsync();
        }
        catch (Exception ex)
        {
            ViewBag.Error = ex.Message;
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> GetPositions(string accountId, DateTime fromDate, DateTime toDate)
    {
        var model = new PositionsPageViewModel
        {
            SelectedAccountId = accountId,
            FromDate = fromDate,
            ToDate = toDate
        };

        var token = _configuration["TinkoffToken"];
        if (string.IsNullOrEmpty(token))
        {
            ViewBag.Error = "Токен Тинькофф не настроен";
            return View("Index", model);
        }

        try
        {
            var service = new TinkoffService(token);
            model.Accounts = await service.GetAccountsAsync();
            
            if (!string.IsNullOrEmpty(accountId))
            {
                var operations = await service.GetOperationsAsync(accountId, fromDate, toDate);
                var instrumentService = new InstrumentService(token);
                model.Positions = await CalculatePositionsAsync(operations, instrumentService);
            }
        }
        catch (Exception ex)
        {
            ViewBag.Error = ex.Message;
        }

        return View("Index", model);
    }

    private async Task<List<PositionViewModel>> CalculatePositionsAsync(List<OperationViewModel> operations, InstrumentService instrumentService)
    {
        var positions = new Dictionary<string, PositionViewModel>();

        // Фильтруем операции с торговыми инструментами
        var validOperations = operations.Where(o => 
            !string.IsNullOrEmpty(o.Figi) && 
            o.Quantity != 0 && 
            (o.Type.Contains("Buy") || o.Type.Contains("Sell")))
            .ToList();

        foreach (var op in validOperations)
        {
            if (!string.IsNullOrEmpty(op.Figi) && !positions.ContainsKey(op.Figi))
            {
                positions[op.Figi] = new PositionViewModel
                {
                    Figi = op.Figi,
                    InstrumentName = op.InstrumentName ?? op.Figi,
                    Ticker = await instrumentService.GetTickerByFigi(op.Figi),
                    InstrumentType = op.Type.Contains("Buy") || op.Type.Contains("Sell") ? "Stock" : "Future",
                    Currency = op.Currency
                };
            }

            var position = positions[op.Figi];
            var trade = new TradeViewModel
            {
                Date = op.Date,
                Type = op.Type,
                Quantity = op.Quantity,
                Price = op.Price,
                Amount = op.Amount
            };
            position.Trades.Add(trade);

            // Расчет позиции
            {
                var quantity = op.Type.Contains("Buy") ? op.Quantity : -op.Quantity;
                
                if (position.OpenQuantity == 0)
                {
                    position.OpenQuantity = quantity;
                    position.AveragePrice = Math.Abs(op.Price);
                }
                else if ((position.OpenQuantity > 0 && quantity > 0) || (position.OpenQuantity < 0 && quantity < 0))
                {
                    // Увеличение позиции
                    var totalCost = Math.Abs(position.OpenQuantity * position.AveragePrice) + Math.Abs(quantity * op.Price);
                    position.OpenQuantity += quantity;
                    position.AveragePrice = position.OpenQuantity != 0 ? totalCost / Math.Abs(position.OpenQuantity) : 0;
                }
                else
                {
                    // Закрытие/уменьшение позиции
                    var closedQuantity = Math.Min(Math.Abs(quantity), Math.Abs(position.OpenQuantity));
                    var pnl = closedQuantity * (Math.Abs(op.Price) - position.AveragePrice) * 
                             (position.OpenQuantity > 0 ? 1 : -1) * (quantity > 0 ? 1 : -1);
                    
                    position.RealizedPnL += pnl;
                    position.OpenQuantity += quantity;
                    
                    if (position.OpenQuantity == 0)
                    {
                        position.AveragePrice = 0;
                    }
                }
            }

            position.LastTradeDate = op.Date;
        }

        return positions.Values.Where(p => p.Trades.Any()).OrderByDescending(p => p.LastTradeDate).ToList();
    }
}