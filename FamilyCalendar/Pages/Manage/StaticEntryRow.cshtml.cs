using FamilyCalendar.Entries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

namespace FamilyCalendar.Pages.Manage;

[Authorize(Roles = "Administrator")]
public class StaticEntryRowModel(IEntryRepository entryRepository, IEntryParser entryParser, ILogger<StaticEntryRowModel> logger) : PageModel
{
  private readonly IEntryRepository _entryRepository = entryRepository;
  private readonly IEntryParser _entryParser = entryParser;
  private readonly ILogger<StaticEntryRowModel> _logger = logger;

  [BindProperty(SupportsGet = true)]
  public required Guid CalendarId { get; set; }

  [BindProperty(SupportsGet = true)]
  public required Guid EntryId { get; set; }

  public required Entry Entry { get; set; }

  public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
  {
    var entry = await _entryRepository.GetAsync(CalendarId, EntryId, cancellationToken);
    if (entry is null)
    {
      return NotFound();
    }

    Entry = entry;
    return Page();
  }

  public async Task<IActionResult> OnPatchReparseEntryAsync(CancellationToken cancellationToken)
  {
    var entry = await _entryRepository.GetAsync(CalendarId, EntryId, cancellationToken);
    if (entry is null)
    {
      return NotFound();
    }

    var now = entry.CreatedAt.ToOffset(DateTimeOffset.Now.Offset);
    var reparsedEntry = await _entryParser.ParseFromString(entry.Prompt, CalendarId, entry.Id, now, cancellationToken);
    if (reparsedEntry is null)
    {
      return BadRequest();
    }

    var success = await _entryRepository.UpdateAsync(reparsedEntry, cancellationToken);
    if (!success)
    {
      _logger.LogError("Failed to write entry");
    }

    Entry = reparsedEntry;
    return Page();
  }
}

public class PartialStaticEntryRowModel(Entry entry)
{
  public Entry Entry { get; init; } = entry;
}
