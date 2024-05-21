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

  public EntryRepositoryTests()
  {
    _settings.Value.Returns(new FamilyCalendarSettings { DynamoDbTable = _testTableName });
    _repository = new EntryRepository(_dynamoDb, _settings);
  }

  private bool isCloseTo(string value, DateTime now)
  {
    return DateTime.TryParse(value, out var parsedValue) && (DateTime.UtcNow - parsedValue) < TimeSpan.FromSeconds(5);
  }
}
