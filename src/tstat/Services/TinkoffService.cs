using Tinkoff.InvestApi;
using Tinkoff.InvestApi.V1;
using tstat.Models;

namespace tstat.Services;

public class TinkoffService
{
    private readonly InvestApiClient _client;

    public TinkoffService(string token)
    {
        _client = InvestApiClientFactory.Create(token);
    }

    public async Task<List<AccountViewModel>> GetAccountsAsync()
    {
        try
        {
            var response = await _client.Users.GetAccountsAsync(new GetAccountsRequest());
            
            return response.Accounts.Select(account => new AccountViewModel
            {
                Id = account.Id,
                Name = account.Name,
                Type = account.Type.ToString(),
                Status = account.Status.ToString()
            }).ToList();
        }
        catch (Exception ex)
        {
            throw new Exception($"Ошибка получения счетов: {ex.Message}");
        }
    }

    public async Task<List<OperationViewModel>> GetOperationsAsync(string accountId, DateTime from, DateTime to)
    {
        try
        {
            var request = new OperationsRequest
            {
                AccountId = accountId,
                From = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(from.ToUniversalTime()),
                To = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(to.ToUniversalTime())
            };

            var response = await _client.Operations.GetOperationsAsync(request);

            return response.Operations.Select(op => new OperationViewModel
            {
                Id = op.Id,
                AccountId = accountId,
                Date = op.Date.ToDateTime(),
                Type = op.OperationType.ToString(),
                Amount = (decimal)op.Payment.Units + (decimal)op.Payment.Nano / 1_000_000_000m,
                Currency = op.Payment.Currency,
                InstrumentName = op.InstrumentType,
                Figi = op.Figi,
                Quantity = (int)op.Quantity,
                Price = (decimal)op.Price.Units + (decimal)op.Price.Nano / 1_000_000_000m,
                Status = op.State.ToString()
            }).OrderByDescending(x => x.Date).ToList();
        }
        catch (Exception ex)
        {
            throw new Exception($"Ошибка получения операций: {ex.Message}");
        }
    }
}