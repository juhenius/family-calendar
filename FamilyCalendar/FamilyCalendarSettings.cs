namespace FamilyCalendar;

public record FamilyCalendarSettings
{
  public required string DynamoDbTable { get; init; }
  public required string EntriesByDateIndex { get; init; }
}