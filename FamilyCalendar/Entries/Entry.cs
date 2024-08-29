namespace FamilyCalendar.Entries;

public class Entry
{
  public required Guid Id { get; init; } = Guid.NewGuid();
  public required Guid CalendarId { get; init; }
  public required string Title { get; init; }
  public required DateTimeOffset Date { get; init; }
  public string? Location { get; init; }
  public required List<string> Participants { get; init; }
  public required List<string> Recurrence { get; init; }
  public required string Prompt { get; init; }
  public required DateTimeOffset CreatedAt { get; init; }

  public override bool Equals(object? obj)
  {
    if (obj == null || GetType() != obj.GetType())
    {
      return false;
    }

    var other = (Entry)obj;
    return Id == other.Id;
  }

  public override int GetHashCode()
  {
    return Id.GetHashCode();
  }

  public Entry With(
    Guid? id = null,
    Guid? calendarId = null,
    string? title = null,
    DateTimeOffset? date = null,
    string? location = null,
    List<string>? participants = null,
    List<string>? recurrence = null,
    string? prompt = null,
    DateTimeOffset? createdAt = null)
  {
    return new Entry
    {
      Id = id ?? Id,
      CalendarId = calendarId ?? CalendarId,
      Title = title ?? Title,
      Date = date ?? Date,
      Location = location ?? Location,
      Participants = participants ?? Participants,
      Recurrence = recurrence ?? Recurrence,
      Prompt = prompt ?? Prompt,
      CreatedAt = createdAt ?? CreatedAt
    };
  }
}
