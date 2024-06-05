namespace FamilyCalendar;

public record FamilyCalendarSettings
{
  public required string DynamoDbTable { get; init; }
  public required string EntriesByDateIndex { get; init; }
  public required string BaseHref { get; set; }
  public required string ViewerPassword { get; set; }
  public required string AdministratorPassword { get; set; }
}