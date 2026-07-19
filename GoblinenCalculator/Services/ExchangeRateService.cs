using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

namespace GoblinenCalculator.Services;

// Free, no-key exchange rate API (European Central Bank reference rates).
// https://frankfurter.dev
public class ExchangeRateService(HttpClient httpClient, IMemoryCache cache, ILogger<ExchangeRateService> logger)
    : IExchangeRateService
{
    private const string CacheKey = "eur-sek-rate";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

    public async Task<ExchangeRate> GetEurToSekRateAsync(bool forceRefresh = false, CancellationToken cancellationToken = default)
    {
        if (!forceRefresh && cache.TryGetValue(CacheKey, out ExchangeRate? cached) && cached is not null)
        {
            return cached;
        }

        try
        {
            using var response = await httpClient.GetAsync(
                "https://api.frankfurter.dev/v1/latest?base=EUR&symbols=SEK", cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            var rate = doc.RootElement.GetProperty("rates").GetProperty("SEK").GetDecimal();

            var result = new ExchangeRate(rate, DateTimeOffset.UtcNow);
            cache.Set(CacheKey, result, CacheDuration);
            return result;
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or KeyNotFoundException)
        {
            logger.LogWarning(ex, "Failed to fetch EUR->SEK exchange rate");

            // Fall back to the last known good rate (even if stale) rather than failing the page.
            if (cache.TryGetValue(CacheKey, out ExchangeRate? stale) && stale is not null)
            {
                return stale;
            }

            throw;
        }
    }
}
