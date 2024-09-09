namespace FamilyCalendar;

public record FamilyCalendarSettings
{
  public required string DynamoDbTable { get; init; }
  public required string EntriesByDisplayEndDateIndex { get; init; }
  public required string BaseHref { get; set; }
  public required string OpenAiModelId { get; set; }
  public required string OpenAiApiKey { get; set; }
  public required string ViewerPassword { get; set; }
  public required string AdministratorPassword { get; set; }
}
