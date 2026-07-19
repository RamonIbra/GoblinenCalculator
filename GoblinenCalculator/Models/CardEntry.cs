namespace GoblinenCalculator.Models;

public class CardEntry
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal BasePriceEur { get; set; }

    public int SortOrder { get; set; }
}
