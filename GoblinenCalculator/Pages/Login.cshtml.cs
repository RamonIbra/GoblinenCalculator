using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using GoblinenCalculator.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace GoblinenCalculator.Pages;

public class LoginModel(IOptions<AuthOptions> authOptions, IPasswordHasher<string> passwordHasher) : PageModel
{
    [BindProperty]
    [Required]
    [Display(Name = "Username")]
    public string Username { get; set; } = string.Empty;

    [BindProperty]
    [Required]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public string? ErrorMessage { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var auth = authOptions.Value;
        var usernameMatches = string.Equals(Username, auth.Username, StringComparison.Ordinal);
        var passwordVerified = !string.IsNullOrEmpty(auth.PasswordHash)
            && passwordHasher.VerifyHashedPassword(auth.Username, auth.PasswordHash, Password)
                != PasswordVerificationResult.Failed;

        if (!usernameMatches || !passwordVerified)
        {
            ErrorMessage = "Invalid username or password.";
            return Page();
        }

        var claims = new List<Claim> { new(ClaimTypes.Name, auth.Username) };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
        {
            return LocalRedirect(ReturnUrl);
        }

        return RedirectToPage("/Index");
    }
}
