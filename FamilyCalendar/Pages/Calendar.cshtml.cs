using FamilyCalendar.Entries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyCalendar.Pages;

public class CalendarModel : PageModel
{
  private readonly IEntryRepository _entryRepository;

  public CalendarModel(IEntryRepository entryRepository)
  {
    _entryRepository = entryRepository;
  }

  public IActionResult OnGetRefresh()
  {
    return Partial("_Calendar", this);
  }

  public async Task<IEnumerable<Entry>> GetEntriesAsync()
  {
    var entryDtos = await _entryRepository.GetAllAsync();
    return entryDtos.Select(e => e.ToEntry());
  }
}
