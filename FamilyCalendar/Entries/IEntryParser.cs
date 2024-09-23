namespace FamilyCalendar.Entries;

public interface IEntryParser
{
  Task<EntryParseResult> ParseFromString(string prompt, DateTimeOffset localTime, string timeZone, CancellationToken cancellationToken = default);
}

public record EntryParseResult
{
  public required string Title { get; init; }
  public required DateTimeOffset Date { get; init; }
  public string? Location { get; init; }
  public required List<string> Participants { get; init; }
  public required List<string> Recurrence { get; init; }
  public required string Prompt { get; init; }
  public required DateTimeOffset LocalTime { get; init; }
  public required string TimeZone { get; init; }

  public Entry ToEntry(Guid id, Guid calendarId)
  {
    return new Entry()
    {
      Id = id,
      CalendarId = calendarId,
      Title = Title,
      Date = Date,
      Location = Location,
      Participants = Participants,
      Recurrence = Recurrence,
      Prompt = Prompt,
      CreatedAt = LocalTime.ToUniversalTime(),
      TimeZone = TimeZone,
    };
  }
}
