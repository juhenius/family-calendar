using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyCalendar.Pages;

[Authorize]
public class IndexModel : PageModel
{
}
