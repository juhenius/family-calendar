using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace FamilyCalendar.Pages;

public class LoginModel(IOptions<FamilyCalendarSettings> settings) : PageModel
{
  [BindProperty]
  public required InputModel Input { get; set; }
  public string? Message { get; set; }

  private readonly string _viewerPassword = settings.Value.ViewerPassword;
  private readonly string _administratorPassword = settings.Value.AdministratorPassword;

  public class InputModel
  {
    [Required]
    [DataType(DataType.Password)]
    public required string Password { get; set; }
  }

  public async Task<IActionResult> OnPostAsync([FromQuery] string returnUrl)
  {
    if (!ModelState.IsValid)
    {
      return Page();
    }

    if (Input.Password != _viewerPassword && Input.Password != _administratorPassword)
    {
      Message = "Invalid password";
      return Page();
    }

    var claims = new List<Claim>
    {
      new(ClaimTypes.Name, "Name"),
      new(ClaimTypes.Role, Input.Password switch
      {
        var p when p == _viewerPassword => "Viewer",
        var p when p == _administratorPassword => "Administrator",
        _ => "Unknown",
      }),
    };

    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var authProperties = new AuthenticationProperties();

    await HttpContext.SignInAsync(
      CookieAuthenticationDefaults.AuthenticationScheme,
      new ClaimsPrincipal(claimsIdentity),
      authProperties);

    return LocalRedirect(returnUrl);
  }
}
