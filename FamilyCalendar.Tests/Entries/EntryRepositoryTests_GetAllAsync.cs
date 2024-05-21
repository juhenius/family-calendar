using Amazon.DynamoDBv2.Model;
using NSubstitute;

namespace FamilyCalendar.Entries.Tests;

public partial class EntryRepositoryTests
{
  [Fact]
  public async Task GetAllAsync_UsesGivenTable()
  {
    var scanResponse = new ScanResponse { Items = [] };
    _dynamoDb.ScanAsync(Arg.Any<ScanRequest>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(scanResponse));

    await _repository.GetAllAsync(CancellationToken.None);

    await _dynamoDb.Received(1).ScanAsync(Arg.Is<ScanRequest>(request =>
        request.TableName == _testTableName
    ), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task GetAllAsync_ReturnsAllEntryDtos_WhenItemsExists()
  {
    var id1 = Guid.NewGuid();
    var id2 = Guid.NewGuid();

    var scanResponse = new ScanResponse
    {
      Items = new List<Dictionary<string, AttributeValue>>
        {
          new Dictionary<string, AttributeValue>
          {
            { "pk", new AttributeValue { S = id1.ToString() } },
            { "sk", new AttributeValue { S = id1.ToString() } },
            { "id", new AttributeValue { S = id1.ToString() } },
            { "title", new AttributeValue { S = "Entry 1" } },
          },
          new Dictionary<string, AttributeValue>
          {
            { "pk", new AttributeValue { S = id2.ToString() } },
            { "sk", new AttributeValue { S = id2.ToString() } },
            { "id", new AttributeValue { S = id2.ToString() } },
            { "title", new AttributeValue { S = "Entry 2" } },
          }
        }
    };
    _dynamoDb.ScanAsync(Arg.Any<ScanRequest>(), Arg.Any<CancellationToken>()).Returns(scanResponse);

    var results = await _repository.GetAllAsync(CancellationToken.None);

    Assert.Equal(2, results.Count());
    Assert.Contains(results, e => e.Title == "Entry 1");
    Assert.Contains(results, e => e.Title == "Entry 2");
  }
}
