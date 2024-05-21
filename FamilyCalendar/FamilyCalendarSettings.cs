namespace FamilyCalendar;

public record FamilyCalendarSettings
{
  public required string DynamoDbTable { get; init; }
}