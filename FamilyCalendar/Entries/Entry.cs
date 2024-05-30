namespace FamilyCalendar.Entries;

public class Entry
{
  public required Guid Id { get; init; } = Guid.NewGuid();
  public required Guid CalendarId { get; init; }
  public required string Title { get; init; }
  public required string Member { get; init; }
  public required DateTimeOffset Date { get; init; }
}
