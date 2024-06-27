namespace FamilyCalendar.Entries;

public class Entry
{
  public required Guid Id { get; init; } = Guid.NewGuid();
  public required Guid CalendarId { get; init; }
  public required string Title { get; init; }
  public required DateTimeOffset Date { get; init; }
  public string? Location { get; init; }
  public required List<string> Participants { get; init; }

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
}
