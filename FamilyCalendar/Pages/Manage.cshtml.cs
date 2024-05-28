using FamilyCalendar.Entries;
using FamilyCalendar.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyCalendar.Pages;

public class ManageModel : PageModel
{
  [BindProperty]
  public string? Text { get; set; }

  [BindProperty(SupportsGet = true)]
  public Guid? Id { get; set; }

  private readonly ILogger<IndexModel> _logger;
  private readonly IEntryRepository _entryRepository;
  private readonly IPartialViewRenderer _partialViewRenderer;

  public ManageModel(IEntryRepository entryRepository, IPartialViewRenderer partialViewRenderer, ILogger<IndexModel> logger)
  {
    _entryRepository = entryRepository;
    _partialViewRenderer = partialViewRenderer;
    _logger = logger;
  }

  public async Task<IActionResult> OnPostAddEntryAsync(CancellationToken cancellationToken)
  {
    var entry = new Entry()
    {
      Id = Guid.NewGuid(),
      Title = Text!,
      Date = DateTime.UtcNow,
    };

    var success = await _entryRepository.CreateAsync(entry.ToEntryDto(), cancellationToken);

    if (!success)
    {
      _logger.LogError("Failed to write DB item");
    }

    var html = $@"
      {await _partialViewRenderer.RenderPartialViewToStringAsync("_AddEntry", this, PageContext, TempData)}
      {await _partialViewRenderer.RenderPartialViewToStringAsync("_EntryList", GetEntryListModel(true), PageContext, TempData)}
    ";

    return Content(html, "text/html");

  }

  public async Task<IActionResult> OnDeleteDeleteEntry(CancellationToken cancellationToken)
  {
    var success = await _entryRepository.DeleteAsync(Id.Value, cancellationToken);

    if (!success)
    {
      _logger.LogError("Failed to delete DB item");
    }

    return Partial("_EntryList", GetEntryListModel(true));
  }

  public EntryListModel GetEntryListModel(bool outOfBandSwap)
  {
    return new EntryListModel(_entryRepository)
    {
      OutOfBandSwap = outOfBandSwap
    };
  }
}

public class EntryListModel
{
  private readonly IEntryRepository _entryRepository;
  public required bool OutOfBandSwap { get; set; } = false;


  public EntryListModel(IEntryRepository entryRepository)
  {
    _entryRepository = entryRepository;
  }

  public async Task<IEnumerable<Entry>> GetEntriesAsync()
  {
    var entryDtos = await _entryRepository.GetAllAsync();
    return entryDtos.Select(e => e.ToEntry());
  }
}
