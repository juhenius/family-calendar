using FamilyCalendar.Entries;
using FamilyCalendar.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;

namespace FamilyCalendar.Pages.Manage;

[Authorize(Roles = "Administrator")]
public class IndexModel(IEntryRepository entryRepository, IEntryParser entryParser, IPartialViewRenderer partialViewRenderer) : PageModel
{
  private readonly IEntryRepository _entryRepository = entryRepository;
  private readonly IPartialViewRenderer _partialViewRenderer = partialViewRenderer;
  private readonly IEntryParser _entryParser = entryParser;
  private readonly List<Entry> _newEntries = [];

  [BindProperty(SupportsGet = true)]
  public required Guid CalendarId { get; set; }

  [BindProperty]
  public required AddEntryInputModel Input { get; set; }

  public class AddEntryInputModel
  {
    [Required]
    public required string Prompt { get; init; }
    [Required]
    public DateTimeOffset CurrentTime { get; set; }
    [Required]
    public required string TimeZone { get; init; }
  }

  public async Task<IActionResult> OnPostAddEntryAsync(CancellationToken cancellationToken)
  {
    if (!ModelState.IsValid)
    {
      return BadRequest();
    }

    var entryId = Guid.NewGuid();
    var parseResult = await _entryParser.ParseFromString(Input.Prompt, Input.CurrentTime, Input.TimeZone, cancellationToken);
    var entry = parseResult.ToEntry(entryId, CalendarId);
    await _entryRepository.CreateAsync(entry, cancellationToken);
    _newEntries.Add(entry);

    var html = $@"
      {await _partialViewRenderer.RenderPartialViewToStringAsync("_AddEntry", this, PageContext, TempData)}
      {await _partialViewRenderer.RenderPartialViewToStringAsync("_EntryList", GetPartialEntryListModel(true), PageContext, TempData)}
    ";

    return Content(html, "text/html");
  }

  public async Task<IActionResult> OnDeleteDeleteEntryAsync([FromForm] Guid entryId, CancellationToken cancellationToken)
  {
    await _entryRepository.DeleteAsync(CalendarId, entryId, cancellationToken);
    return Partial("_EntryList", GetPartialEntryListModel(true));
  }

  public PartialEntryListModel GetPartialEntryListModel(bool outOfBandSwap)
  {
    return new PartialEntryListModel(_entryRepository)
    {
      CalendarId = CalendarId,
      NewEntries = _newEntries,
      OutOfBandSwap = outOfBandSwap,
    };
  }
}

public class PartialEntryListModel(IEntryRepository entryRepository)
{
  private readonly IEntryRepository _entryRepository = entryRepository;

  public required Guid CalendarId { get; init; }
  public List<Entry> NewEntries { get; init; } = [];
  public required bool OutOfBandSwap { get; init; }

  public async Task<IEnumerable<Entry>> GetEntriesAsync()
  {
    var entryDtos = await _entryRepository.GetAllAsync(CalendarId);
    return entryDtos.OrderByDescending(e => e.Date.ToString("s"));
  }

  public bool IsNewEntry(Entry entry)
  {
    return NewEntries.Contains(entry);
  }
}
