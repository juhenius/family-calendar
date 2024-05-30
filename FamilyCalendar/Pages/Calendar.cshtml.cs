using FamilyCalendar.Entries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyCalendar.Pages;

public class CalendarModel(IEntryRepository entryRepository) : PageModel
{
  private readonly IEntryRepository _entryRepository = entryRepository;

  [BindProperty]
  public required Guid CalendarId { get; set; }

  public IActionResult OnGetRefresh()
  {
    return Partial("_Calendar", this);
  }

  public async Task<IEnumerable<Entry>> GetEntriesAsync()
  {
    var entryDtos = await _entryRepository.GetByDateRangeAsync(
      CalendarId,
      DateTime.UtcNow.Date,
      DateTime.UtcNow.Date.Add(TimeSpan.FromDays(1))
    );

    return entryDtos.OrderBy(e => e.Date.ToString("s"));
  }
}
