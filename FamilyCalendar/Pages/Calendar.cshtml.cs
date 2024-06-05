using FamilyCalendar.Entries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyCalendar.Pages;

[Authorize]
public class CalendarModel(IEntryRepository entryRepository) : PageModel
{
  private readonly IEntryRepository _entryRepository = entryRepository;

  [BindProperty(SupportsGet = true)]
  public required Guid CalendarId { get; set; }

  public IActionResult OnGetRefresh()
  {
    return Partial("_Calendar", this);
  }

  public async Task<IEnumerable<Entry>> GetEntriesAsync()
  {
    var entryDtos = await _entryRepository.GetByDateRangeAsync(
      CalendarId,
      DateTimeOffset.UtcNow.Date,
      DateTimeOffset.UtcNow.Date.Add(TimeSpan.FromDays(1))
    );

    return entryDtos.OrderBy(e => e.Date.ToString("s"));
  }
}
