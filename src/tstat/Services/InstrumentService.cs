using Tinkoff.InvestApi;
using Tinkoff.InvestApi.V1;

namespace tstat.Services;

public class InstrumentService
{
    private readonly InvestApiClient _client;

    public InstrumentService(string token)
    {
        _client = InvestApiClientFactory.Create(token);
    }

    public async Task<string> GetTickerByFigi(string figi)
    {
        try
        {
            var request = new InstrumentRequest { IdType = InstrumentIdType.Figi, Id = figi };
            
            // Пробуем получить акцию
            try
            {
                var shareResponse = await _client.Instruments.ShareByAsync(request);
                return shareResponse.Instrument.Ticker;
            }
            catch { }

            // Пробуем получить фьючерс
            try
            {
                var futureResponse = await _client.Instruments.FutureByAsync(request);
                return futureResponse.Instrument.Ticker;
            }
            catch { }

            // Пробуем получить облигацию
            try
            {
                var bondResponse = await _client.Instruments.BondByAsync(request);
                return bondResponse.Instrument.Ticker;
            }
            catch { }

            return figi; // Возвращаем FIGI если не удалось получить тикер
        }
        catch
        {
            return figi;
        }
    }
}