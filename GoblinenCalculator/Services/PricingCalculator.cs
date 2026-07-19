using GoblinenCalculator.Models;

namespace GoblinenCalculator.Services;

public static class PricingCalculator
{
    // Finds the bracket whose upper bound is the first one strictly greater than
    // baseEur, falling back to the highest bracket (the catch-all) when the price
    // exceeds every configured limit.
    public static PriceBracket? MatchBracket(decimal baseEur, IReadOnlyList<PriceBracket> brackets)
    {
        if (brackets.Count == 0)
        {
            return null;
        }

        var sorted = brackets
            .OrderBy(b => b.UpperBoundEur ?? decimal.MaxValue)
            .ToList();

        return sorted.FirstOrDefault(b => baseEur < b.UpperBoundEur) ?? sorted[^1];
    }

    public static decimal AdjustedEur(decimal baseEur, PriceBracket? bracket) =>
        baseEur * (bracket?.Factor ?? 1m);
}
