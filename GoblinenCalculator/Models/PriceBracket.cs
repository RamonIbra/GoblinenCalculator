namespace GoblinenCalculator.Models;

public class PriceBracket
{
    public int Id { get; set; }

    // Prices strictly below this count as belonging to this bracket.
    // Null means "no upper limit" (the catch-all top bracket).
    public decimal? UpperBoundEur { get; set; }

    public decimal Factor { get; set; }

    public int SortOrder { get; set; }
}
