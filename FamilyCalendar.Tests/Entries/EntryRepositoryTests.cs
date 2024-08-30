using Amazon.DynamoDBv2;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace FamilyCalendar.Entries.Tests;

public partial class EntryRepositoryTests
{
  private readonly IAmazonDynamoDB _dynamoDb = Substitute.For<IAmazonDynamoDB>();
  private readonly IOptions<FamilyCalendarSettings> _settings;
  private readonly EntryRepository _repository;
  private readonly string _testTableName = "TestTable";
  private readonly string _testEntriesByDateIndex = "TestEntriesByDateIndex";

  public EntryRepositoryTests()
  {
    _settings = Options.Create(new FamilyCalendarSettings()
    {
      DynamoDbTable = _testTableName,
      EntriesByDateIndex = _testEntriesByDateIndex,
      BaseHref = "",
      OpenAiApiKey = "",
      OpenAiModelId = "",
      ViewerPassword = "",
      AdministratorPassword = "",
    });

    _repository = new EntryRepository(_dynamoDb, _settings);
  }

  private static bool IsCloseTo(string value, DateTimeOffset now, TimeSpan offset)
  {
    return TryParseDateTime(value, out var parsedValue) && (now - parsedValue) < offset;
  }

  private static bool IsCloseTo(string value, DateTimeOffset now)
  {
    return IsCloseTo(value, now, TimeSpan.FromSeconds(5));
  }

  private static bool IsEqualTo(string value, DateTimeOffset now)
  {
    return TryParseDateTime(value, out var parsedValue) && now == parsedValue.ToUniversalTime();
  }

  private static bool TryParseDateTime(string value, out DateTimeOffset result)
  {
    if (DateTimeOffset.TryParse(value, out var parsedValue))
    {
      var localDateTime = parsedValue.DateTime;
      result = new DateTimeOffset(localDateTime, TimeSpan.Zero).ToUniversalTime();
      return true;
    }

    result = DateTimeOffset.UtcNow;
    return false;
  }

  private static Entry CreateTestEntry()
  {
    return new Entry
    {
      Id = Guid.NewGuid(),
      CalendarId = Guid.NewGuid(),
      Title = "New Entry",
      Date = DateTimeOffset.UtcNow,
      Participants = ["Tester"],
      Recurrence = ["test rule"],
      Prompt = "Doctor Appointment now at Doctors office",
      CreatedAt = DateTimeOffset.UtcNow,
    };
  }
}
