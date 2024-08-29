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
    public string? Recurrence { get; init; }
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
      Recurrence = string.Join("\n", Entry.Recurrence),
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
      participants: ParseParticipants(Input.Participants),
      recurrence: ParseRecurrence(Input.Recurrence)
    );

    await _entryRepository.UpdateAsync(updatedEntry, cancellationToken);
    Entry = updatedEntry;
    return Page();
  }

  private static List<string> ParseParticipants(string? input)
  {
    return Split([","], input);
  }

  private static List<string> ParseRecurrence(string? input)
  {
    return Split(["\r\n", "\n", "\r"], input);
  }

  private static List<string> Split(string[] delimeter, string? input)
  {
    return input is null
      ? []
      : input.Split(delimeter, StringSplitOptions.RemoveEmptyEntries)
          .Select(line => line.Trim())
          .Where(line => !string.IsNullOrWhiteSpace(line))
          .ToList();
  }
}
