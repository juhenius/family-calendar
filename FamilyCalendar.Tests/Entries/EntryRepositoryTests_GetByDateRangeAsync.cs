using Amazon.DynamoDBv2.Model;
using NSubstitute;

namespace FamilyCalendar.Entries.Tests;

public partial class EntryRepositoryTests
{
  [Fact]
  public async Task GetByDateRangeAsync_UsesGivenTable()
  {
    var calendarId = Guid.NewGuid();
    var from = DateTimeOffset.UtcNow;
    var to = DateTimeOffset.UtcNow.Add(TimeSpan.FromDays(1));

    var queryResponse = new QueryResponse { Items = [] };
    _dynamoDb.QueryAsync(Arg.Any<QueryRequest>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(queryResponse));

    await _repository.GetByDateRangeAsync(calendarId, from, to, CancellationToken.None);

    await _dynamoDb.Received(1).QueryAsync(Arg.Is<QueryRequest>(request =>
        request.TableName == _testTableName
    ), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task GetByDateRangeAsync_UsesGivenDateRange()
  {
    var calendarId = Guid.NewGuid();
    var from = DateTimeOffset.UtcNow;
    var to = DateTimeOffset.UtcNow.Add(TimeSpan.FromDays(1));

    var queryResponse = new QueryResponse { Items = [] };
    _dynamoDb.QueryAsync(Arg.Any<QueryRequest>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(queryResponse));

    await _repository.GetByDateRangeAsync(calendarId, from, to, CancellationToken.None);

    await _dynamoDb.Received(1).QueryAsync(Arg.Is<QueryRequest>(request =>
        request.ExpressionAttributeValues[":from"].S == from.ToString("s") &&
        request.ExpressionAttributeValues[":to"].S == to.ToString("s")
    ), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task GetByDateRangeAsync_ReturnsMatchingEntries_WhenEntriesExists()
  {
    var calendarId = Guid.NewGuid();
    var from = DateTimeOffset.UtcNow;
    var to = DateTimeOffset.UtcNow.Add(TimeSpan.FromDays(1));

    var id1 = Guid.NewGuid();
    var id2 = Guid.NewGuid();

    var queryResponse = new QueryResponse
    {
      Items = [
        new() {
          { "id", new AttributeValue { S = id1.ToString() } },
          { "title", new AttributeValue { S = "Entry 1" } },
        },
        new() {
          { "id", new AttributeValue { S = id2.ToString() } },
          { "title", new AttributeValue { S = "Entry 2" } },
        }
      ]
    };
    _dynamoDb.QueryAsync(Arg.Any<QueryRequest>(), Arg.Any<CancellationToken>()).Returns(queryResponse);

    var results = await _repository.GetByDateRangeAsync(calendarId, from, to, CancellationToken.None);

    Assert.Equal(2, results.Count());
    Assert.Contains(results, e => e.Title == "Entry 1");
    Assert.Contains(results, e => e.Title == "Entry 2");
  }
}
