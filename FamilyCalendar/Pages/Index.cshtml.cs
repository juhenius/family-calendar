using FamilyCalendar.Entries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace FamilyCalendar.Pages;

public class IndexModel : PageModel
{
  [BindProperty]
  public string? Text { get; set; }

  [BindProperty(SupportsGet = true)]
  public Guid? Id { get; set; }

  private readonly ILogger<IndexModel> _logger;
  private readonly IEntryRepository _entryRepository;
  private readonly IRazorViewEngine _viewEngine;

  public IndexModel(IEntryRepository entryRepository, IRazorViewEngine viewEngine, ILogger<IndexModel> logger)
  {
    _entryRepository = entryRepository;
    _viewEngine = viewEngine;
    _logger = logger;
  }

  public IActionResult OnGetTasks()
  {
    return Partial("_Tasks", this);
  }

  public async Task<IActionResult> OnPostAddTaskAsync(CancellationToken cancellationToken)
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

    var html = $"{await RenderPartialViewToStringAsync("_Tasks", this)} {await RenderPartialViewToStringAsync("_AddTask", this)}";
    return Content(html, "text/html");
  }

  public async Task<IActionResult> OnDeleteDeleteTask(CancellationToken cancellationToken)
  {
    var success = await _entryRepository.DeleteAsync(Id.Value, cancellationToken);

    if (!success)
    {
      _logger.LogError("Failed to delete DB item");
    }

    return Partial("_Tasks", this);
  }

  public async Task<IEnumerable<Entry>> GetTasksAsync()
  {
    var entryDtos = await _entryRepository.GetAllAsync();
    return entryDtos.Select(e => e.ToEntry());
  }

  private async Task<string> RenderPartialViewToStringAsync(string viewName, object model)
  {
    var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
    {
      Model = model
    };

    var viewResult = _viewEngine.GetView(null, viewName, false);
    if (viewResult.View == null)
    {
      viewResult = _viewEngine.FindView(PageContext, viewName, false);
      if (viewResult.View == null)
      {
        throw new ArgumentNullException($"A view with the name {viewName} could not be found");
      }
    }

    using var stringWriter = new StringWriter();
    var viewContext = new ViewContext(
      PageContext,
      viewResult.View,
      viewData,
      TempData,
      stringWriter,
      new HtmlHelperOptions()
    );

    await viewResult.View.RenderAsync(viewContext);
    return stringWriter.ToString();
  }
}
