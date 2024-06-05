using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace FamilyCalendar.Pages;

public class LogoutModel(IOptions<FamilyCalendarSettings> settings) : PageModel
{
  private readonly string _baseHref = settings.Value.BaseHref;

  public async Task<IActionResult> OnGetAsync()
  {
    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return LocalRedirect(_baseHref);
  }
}
