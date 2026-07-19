using System.ComponentModel.DataAnnotations;
using GoblinenCalculator.Data;
using GoblinenCalculator.Models;
using GoblinenCalculator.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GoblinenCalculator.Pages;

public class IndexModel(AppDbContext db, IExchangeRateService exchangeRateService) : PageModel
{
    public List<BracketRow> Brackets { get; set; } = [];

    public List<CardRow> Cards { get; set; } = [];

    public decimal TotalSek { get; set; }

    public ExchangeRate? Rate { get; set; }

    public string? RateError { get; set; }

    [BindProperty]
    public List<BracketInput> BracketInputs { get; set; } = [];

    [BindProperty]
    public NewBracketInput NewBracket { get; set; } = new();

    [BindProperty]
    public NewCardInput NewCard { get; set; } = new();

    public record BracketRow(int Id, decimal? UpperBoundEur, decimal Factor);

    public record CardRow(int Id, string Name, decimal BasePriceEur, int? MatchedBracketId, decimal AdjustedEur, decimal Sek);

    public class BracketInput
    {
        public int Id { get; set; }

        public decimal? UpperBoundEur { get; set; }

        [Range(0.0001, 1000)]
        public decimal Factor { get; set; }
    }

    public class NewBracketInput
    {
        [Range(0.01, 1_000_000)]
        public decimal? UpperBoundEur { get; set; }

        [Range(0.0001, 1000)]
        public decimal Factor { get; set; } = 1m;
    }

    public class NewCardInput
    {
        public string Name { get; set; } = string.Empty;

        [Range(0, 1_000_000)]
        public decimal BasePriceEur { get; set; }
    }

    public async Task OnGetAsync() => await LoadAsync();

    public async Task<IActionResult> OnPostSaveBracketsAsync()
    {
        var brackets = await db.PriceBrackets.ToDictionaryAsync(b => b.Id);
        foreach (var input in BracketInputs)
        {
            if (!brackets.TryGetValue(input.Id, out var bracket))
            {
                continue;
            }

            bracket.Factor = input.Factor;

            // The catch-all bracket's bound stays null regardless of what was posted for it.
            if (bracket.UpperBoundEur.HasValue)
            {
                bracket.UpperBoundEur = input.UpperBoundEur;
            }
        }

        await db.SaveChangesAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAddBracketAsync()
    {
        if (NewBracket.UpperBoundEur is decimal upper && upper > 0)
        {
            var maxOrder = await db.PriceBrackets.MaxAsync(b => (int?)b.SortOrder) ?? 0;
            db.PriceBrackets.Add(new PriceBracket
            {
                UpperBoundEur = upper,
                Factor = NewBracket.Factor,
                SortOrder = maxOrder + 1,
            });
            await db.SaveChangesAsync();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteBracketAsync(int id)
    {
        var count = await db.PriceBrackets.CountAsync();
        if (count > 1)
        {
            var bracket = await db.PriceBrackets.FindAsync(id);
            if (bracket is not null)
            {
                db.PriceBrackets.Remove(bracket);
                await db.SaveChangesAsync();
            }
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAddCardAsync()
    {
        if (!string.IsNullOrWhiteSpace(NewCard.Name) || NewCard.BasePriceEur > 0)
        {
            var maxOrder = await db.CardEntries.MaxAsync(c => (int?)c.SortOrder) ?? 0;
            db.CardEntries.Add(new CardEntry
            {
                Name = string.IsNullOrWhiteSpace(NewCard.Name) ? "Card" : NewCard.Name.Trim(),
                BasePriceEur = NewCard.BasePriceEur,
                SortOrder = maxOrder + 1,
            });
            await db.SaveChangesAsync();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteCardAsync(int id)
    {
        var card = await db.CardEntries.FindAsync(id);
        if (card is not null)
        {
            db.CardEntries.Remove(card);
            await db.SaveChangesAsync();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRefreshRateAsync()
    {
        await exchangeRateService.GetEurToSekRateAsync(forceRefresh: true);
        return RedirectToPage();
    }

    private async Task LoadAsync()
    {
        var brackets = await db.PriceBrackets
            .OrderBy(b => b.UpperBoundEur ?? decimal.MaxValue)
            .ToListAsync();

        Brackets = brackets.Select(b => new BracketRow(b.Id, b.UpperBoundEur, b.Factor)).ToList();
        BracketInputs = brackets
            .Select(b => new BracketInput { Id = b.Id, UpperBoundEur = b.UpperBoundEur, Factor = b.Factor })
            .ToList();

        try
        {
            Rate = await exchangeRateService.GetEurToSekRateAsync();
        }
        catch (HttpRequestException)
        {
            RateError = "Could not fetch the live EUR to SEK exchange rate. Try refreshing.";
        }

        var rate = Rate?.EurToSek ?? 0m;
        var cardEntities = await db.CardEntries.OrderBy(c => c.SortOrder).ToListAsync();
        Cards = cardEntities.Select(c =>
        {
            var bracket = PricingCalculator.MatchBracket(c.BasePriceEur, brackets);
            var adjusted = PricingCalculator.AdjustedEur(c.BasePriceEur, bracket);
            return new CardRow(c.Id, c.Name, c.BasePriceEur, bracket?.Id, adjusted, adjusted * rate);
        }).ToList();

        TotalSek = Cards.Sum(c => c.Sek);
    }
}
