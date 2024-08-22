using FamilyCalendar.Entries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;

namespace FamilyCalendar.Pages.Manage;

[Authorize(Roles = "Administrator")]
public class EditableEntryRowModel(IEntryRepository entryRepository) : PageModel
{
  private readonly IEntryRepository _entryRepository = entryRepository;

  [BindProperty(SupportsGet = true)]
  public required Guid CalendarId { get; set; }

  [BindProperty(SupportsGet = true)]
  public required Guid EntryId { get; set; }

  public required Entry Entry { get; set; }

  [BindProperty]
  public required UpdateEntryInputModel Input { get; set; }

  public class UpdateEntryInputModel
  {
    [Required]
    public required string Title { get; init; }
    [Required]
    [DataType(DataType.DateTime)]
    public required DateTimeOffset Date { get; init; }
    public string? Location { get; init; }
    public string? Participants { get; init; }
  }

  public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
  {
    var entry = await _entryRepository.GetAsync(CalendarId, EntryId, cancellationToken);
    if (entry is null)
    {
      return NotFound();
    }

    Entry = entry;
    Input = new UpdateEntryInputModel()
    {
      Title = Entry.Title,
      Date = Entry.Date,
      Location = Entry.Location,
      Participants = string.Join(", ", Entry.Participants),
    };

    return Page();
  }

  public async Task<IActionResult> OnPatchAsync(CancellationToken cancellationToken)
  {
    var entry = await _entryRepository.GetAsync(CalendarId, EntryId, cancellationToken);
    if (entry is null)
    {
      return NotFound();
    }

    if (!ModelState.IsValid)
    {
      Entry = entry;
      return Page();
    }

    var updatedEntry = entry.With(
      title: Input.Title,
      date: Input.Date,
      location: Input.Location,
      participants: ParseParticipants(Input.Participants)
    );

    await _entryRepository.UpdateAsync(updatedEntry, cancellationToken);
    Entry = updatedEntry;
    return Page();
  }

  private static List<string> ParseParticipants(string? input)
  {
    return input is null
      ? []
      : input.Split(",").Select(v => v.Trim()).ToList();
  }
}
