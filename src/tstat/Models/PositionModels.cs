namespace tstat.Models;

public class PositionViewModel
{
    public string Figi { get; set; } = string.Empty;
    public string InstrumentName { get; set; } = string.Empty;
    public string Ticker { get; set; } = string.Empty;
    public string InstrumentType { get; set; } = string.Empty;
    public int OpenQuantity { get; set; }
    public decimal AveragePrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal UnrealizedPnL { get; set; }
    public decimal RealizedPnL { get; set; }
    public decimal TotalPnL => UnrealizedPnL + RealizedPnL;
    public string Currency { get; set; } = string.Empty;
    public DateTime LastTradeDate { get; set; }
    public List<TradeViewModel> Trades { get; set; } = new();
}

public class TradeViewModel
{
    public DateTime Date { get; set; }
    public string Type { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Amount { get; set; }
}

public class PositionsPageViewModel
{
    public List<AccountViewModel> Accounts { get; set; } = new();
    public List<PositionViewModel> Positions { get; set; } = new();
    public string? SelectedAccountId { get; set; }
    public DateTime FromDate { get; set; } = DateTime.Today.AddYears(-10);
    public DateTime ToDate { get; set; } = DateTime.Today;
    public decimal TotalRealizedPnL => Positions.Sum(p => p.RealizedPnL);
    public decimal TotalUnrealizedPnL => Positions.Sum(p => p.UnrealizedPnL);
    public decimal TotalPnL => TotalRealizedPnL + TotalUnrealizedPnL;
}