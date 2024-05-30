using FamilyCalendar.Entries;
using FamilyCalendar.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyCalendar.Pages;

public class ManageModel(IEntryRepository entryRepository, IPartialViewRenderer partialViewRenderer, ILogger<IndexModel> logger) : PageModel
{
  private readonly ILogger<IndexModel> _logger = logger;
  private readonly IEntryRepository _entryRepository = entryRepository;
  private readonly IPartialViewRenderer _partialViewRenderer = partialViewRenderer;

  [BindProperty]
  public required Guid CalendarId { get; set; }

  [BindProperty]
  public string Title { get; set; } = default!;

  public async Task<IActionResult> OnPostAddEntryAsync(CancellationToken cancellationToken)
  {
    if (string.IsNullOrEmpty(Title))
    {
      throw new ArgumentException("Entry Title is missing");
    }

    var entry = new Entry()
    {
      Id = Guid.NewGuid(),
      CalendarId = CalendarId,
      Title = Title,
      Date = DateTime.UtcNow,
      Member = "test1",
    };

    var success = await _entryRepository.CreateAsync(entry, cancellationToken);

    if (!success)
    {
      _logger.LogError("Failed to write entry");
    }

    var html = $@"
      {await _partialViewRenderer.RenderPartialViewToStringAsync("_AddEntry", this, PageContext, TempData)}
      {await _partialViewRenderer.RenderPartialViewToStringAsync("_EntryList", GetEntryListModel(true), PageContext, TempData)}
    ";

    return Content(html, "text/html");

  }

  public async Task<IActionResult> OnDeleteDeleteEntry([FromForm] Guid entryId, CancellationToken cancellationToken)
  {
    var success = await _entryRepository.DeleteAsync(CalendarId, entryId, cancellationToken);

    if (!success)
    {
      _logger.LogError("Failed to delete entry");
    }

    return Partial("_EntryList", GetEntryListModel(true));
  }

  public EntryListModel GetEntryListModel(bool outOfBandSwap)
  {
    return new EntryListModel(_entryRepository)
    {
      CalendarId = CalendarId,
      OutOfBandSwap = outOfBandSwap
    };
  }
}

public class EntryListModel(IEntryRepository entryRepository)
{
  private readonly IEntryRepository _entryRepository = entryRepository;
  public required Guid CalendarId { get; set; }
  public required bool OutOfBandSwap { get; set; }

  public async Task<IEnumerable<Entry>> GetEntriesAsync()
  {
    var entryDtos = await _entryRepository.GetAllAsync(CalendarId);
    return entryDtos.OrderByDescending(e => e.Date.ToString("s"));
  }
}