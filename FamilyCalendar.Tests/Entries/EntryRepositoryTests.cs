using Amazon.DynamoDBv2;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace FamilyCalendar.Entries.Tests;

public partial class EntryRepositoryTests
{
  private readonly IAmazonDynamoDB _dynamoDb = Substitute.For<IAmazonDynamoDB>();
  private readonly IOptions<FamilyCalendarSettings> _settings = Substitute.For<IOptions<FamilyCalendarSettings>>();
  private readonly EntryRepository _repository;
  private readonly string _testTableName = "TestTable";
  private readonly string _testEntriesByDateIndex = "TestEntriesByDateIndex";

  public EntryRepositoryTests()
  {
    _settings.Value.Returns(new FamilyCalendarSettings { DynamoDbTable = _testTableName, EntriesByDateIndex = _testEntriesByDateIndex });
    _repository = new EntryRepository(_dynamoDb, _settings);
  }

  private static bool IsCloseTo(string value, DateTime now)
  {
    return DateTime.TryParse(value, out var parsedValue) && (now - parsedValue) < TimeSpan.FromSeconds(5);
  }

  private static bool IsEqualTo(string value, DateTime now)
  {
    return DateTime.TryParse(value, out var parsedValue) && now == parsedValue.ToUniversalTime();
  }

  private static Entry CreateTestEntry()
  {
    return new Entry
    {
      Id = Guid.NewGuid(),
      CalendarId = Guid.NewGuid(),
      Title = "New Entry",
      Date = DateTime.UtcNow,
      Member = "Tester"
    };
  }
}
