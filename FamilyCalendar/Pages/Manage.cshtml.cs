using FamilyCalendar.Entries;
using FamilyCalendar.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

namespace FamilyCalendar.Pages;

[Authorize(Roles = "Administrator")]
public class ManageModel(IEntryRepository entryRepository, IEntryParser entryParser, IPartialViewRenderer partialViewRenderer, ILogger<IndexModel> logger) : PageModel
{
  private readonly ILogger<IndexModel> _logger = logger;
  private readonly IEntryRepository _entryRepository = entryRepository;
  private readonly IPartialViewRenderer _partialViewRenderer = partialViewRenderer;
  private readonly IEntryParser _entryParser = entryParser;
  private readonly List<Entry> _newEntries = [];

  [BindProperty(SupportsGet = true)]
  public required Guid CalendarId { get; set; }

  [BindProperty]
  public string? EntryInput { get; set; } = default;

  public async Task<IActionResult> OnPostAddEntryAsync(CancellationToken cancellationToken)
  {
    if (string.IsNullOrEmpty(EntryInput))
    {
      throw new ArgumentException("Parameter EntryInput is missing");
    }

    var entryId = Guid.NewGuid();
    var now = DateTimeOffset.Now;
    var entry = await _entryParser.ParseFromString(EntryInput, CalendarId, entryId, now, cancellationToken);
    var success = await _entryRepository.CreateAsync(entry, cancellationToken);

    if (!success)
    {
      _logger.LogError("Failed to write entry");
    }

    _newEntries.Add(entry);

    var html = $@"
      {await _partialViewRenderer.RenderPartialViewToStringAsync("Manage/_AddEntry", this, PageContext, TempData)}
      {await _partialViewRenderer.RenderPartialViewToStringAsync("Manage/_EntryList", GetEntryListModel(true), PageContext, TempData)}
    ";

    return Content(html, "text/html");
  }

  public async Task<IActionResult> OnPatchReparseEntry([FromForm] Guid entryId, CancellationToken cancellationToken)
  {
    var entry = await _entryRepository.GetAsync(CalendarId, entryId, cancellationToken);
    if (entry is not null)
    {
      var now = entry.CreatedAt.ToOffset(DateTimeOffset.Now.Offset);
      var reparsedEntry = await _entryParser.ParseFromString(entry.Prompt, CalendarId, entry.Id, now, cancellationToken);
      var success = await _entryRepository.UpdateAsync(reparsedEntry, cancellationToken);
      if (!success)
      {
        _logger.LogError("Failed to write entry");
      }

      _newEntries.Add(reparsedEntry);
    }

    return Partial("Manage/_EntryList", GetEntryListModel(true));
  }

  public async Task<IActionResult> OnDeleteDeleteEntry([FromForm] Guid entryId, CancellationToken cancellationToken)
  {
    var success = await _entryRepository.DeleteAsync(CalendarId, entryId, cancellationToken);

    if (!success)
    {
      _logger.LogError("Failed to delete entry");
    }

    return Partial("Manage/_EntryList", GetEntryListModel(true));
  }

  public EntryListModel GetEntryListModel(bool outOfBandSwap)
  {
    return new EntryListModel(_entryRepository)
    {
      CalendarId = CalendarId,
      NewEntries = _newEntries,
      OutOfBandSwap = outOfBandSwap,
    };
  }
}

public class EntryListModel(IEntryRepository entryRepository)
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