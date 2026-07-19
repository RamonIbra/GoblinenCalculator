namespace GoblinenCalculator.Options;

public class AuthOptions
{
    public const string SectionName = "Auth";

    public string Username { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;
}
