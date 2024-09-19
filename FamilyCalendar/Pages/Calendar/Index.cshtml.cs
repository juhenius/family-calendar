using FamilyCalendar.Entries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyCalendar.Pages.Calendar;

[Authorize]
public class IndexModel(IEntryRepository entryRepository) : PageModel
{
  private readonly IEntryRepository _entryRepository = entryRepository;

  [BindProperty(SupportsGet = true)]
  public required Guid CalendarId { get; set; }

  public IActionResult OnGetRefresh()
  {
    return Partial("Calendar/_Calendar", this);
  }

  public async Task<IEnumerable<Entry>> GetEntriesAsync()
  {
    var rangeStart = DateTimeOffset.UtcNow.Date;
    var rangeEnd = DateTimeOffset.UtcNow.Date.Add(TimeSpan.FromDays(14));
    var entries = await _entryRepository.GetByDateRangeAsync(CalendarId, rangeStart, rangeEnd);
    return entries.SelectMany(e => e.ExpandRecurrenceForDateRange(rangeStart, rangeEnd))
      .OrderBy(e => e.Date.ToString("s"));
  }
}

public class PartialCalendarColumnModel
{
  public required string Participant { get; init; }
  public required IEnumerable<Entry> Entries { get; init; }
}
