using Amazon.DynamoDBv2.Model;
using NSubstitute;

namespace FamilyCalendar.Entries.Tests;

public partial class EntryRepositoryTests
{
  [Fact]
  public async Task GetAllAsync_UsesGivenTable()
  {
    var calendarId = Guid.NewGuid();
    var queryResponse = new QueryResponse { Items = [] };
    _dynamoDb.QueryAsync(Arg.Any<QueryRequest>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(queryResponse));

    await _repository.GetAllAsync(calendarId, CancellationToken.None);

    await _dynamoDb.Received(1).QueryAsync(Arg.Is<QueryRequest>(request =>
        request.TableName == _testTableName
    ), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task GetAllAsync_ReturnsAllEntries_WhenEntriesExists()
  {
    var calendarId = Guid.NewGuid();
    var id1 = Guid.NewGuid();
    var id2 = Guid.NewGuid();

    var queryResponse = new QueryResponse
    {
      Items = [
        new() {
          { "id", new AttributeValue { S = id1.ToString() } },
          { "title", new AttributeValue { S = "Entry 1" } },
          { "prompt", new AttributeValue { S = "Prompt 1" } },
          { "createdAt", new AttributeValue { S = "2024-07-13T20:55:23Z" } },
          { "timeZone", new AttributeValue { S = "Europe/Amsterdam" } },
        },
        new() {
          { "id", new AttributeValue { S = id2.ToString() } },
          { "title", new AttributeValue { S = "Entry 2" } },
          { "prompt", new AttributeValue { S = "Prompt 2" } },
          { "createdAt", new AttributeValue { S = "2024-07-14T20:55:23Z" } },
          { "timeZone", new AttributeValue { S = "Europe/Amsterdam" } },
        }
      ]
    };
    _dynamoDb.QueryAsync(Arg.Any<QueryRequest>(), Arg.Any<CancellationToken>()).Returns(queryResponse);

    var results = await _repository.GetAllAsync(calendarId, CancellationToken.None);

    Assert.Equal(2, results.Count());
    Assert.Contains(results, e => e.Title == "Entry 1");
    Assert.Contains(results, e => e.Title == "Entry 2");
  }
}
