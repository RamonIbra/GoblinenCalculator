namespace GoblinenCalculator.Services;

public record ExchangeRate(decimal EurToSek, DateTimeOffset RetrievedAt);

public interface IExchangeRateService
{
    Task<ExchangeRate> GetEurToSekRateAsync(bool forceRefresh = false, CancellationToken cancellationToken = default);
}
