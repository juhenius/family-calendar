using Amazon.DynamoDBv2.Model;
using NSubstitute;

namespace FamilyCalendar.Entries.Tests;

public partial class EntryRepositoryTests
{
  [Fact]
  public async Task GetAsync_UsesGivenTable()
  {
    var getItemResponse = new GetItemResponse { Item = [] };
    _dynamoDb.GetItemAsync(Arg.Any<GetItemRequest>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(getItemResponse));

    await _repository.GetAsync(Guid.NewGuid(), CancellationToken.None);

    await _dynamoDb.Received(1).GetItemAsync(Arg.Is<GetItemRequest>(request =>
        request.TableName == _testTableName
    ), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task GetAsync_FetchesItemWithMatchingId()
  {
    var id = Guid.NewGuid();
    var getItemResponse = new GetItemResponse { Item = [] };
    _dynamoDb.GetItemAsync(Arg.Any<GetItemRequest>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(getItemResponse));

    await _repository.GetAsync(id, CancellationToken.None);

    await _dynamoDb.Received(1).GetItemAsync(Arg.Is<GetItemRequest>(request =>
        request.Key["pk"].S == id.ToString() &&
        request.Key["sk"].S == id.ToString()
    ), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task GetAsync_ReturnsNull_WhenItemDoesNotExist()
  {
    var id = Guid.NewGuid();
    var getItemResponse = new GetItemResponse { Item = [] };
    _dynamoDb.GetItemAsync(Arg.Any<GetItemRequest>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(getItemResponse));

    var result = await _repository.GetAsync(id, CancellationToken.None);

    Assert.Null(result);
  }

  [Fact]
  public async Task GetAsync_ReturnsEntryDto_WhenItemExists()
  {
    var id = Guid.NewGuid();
    var getItemResponse = new GetItemResponse
    {
      Item = new Dictionary<string, AttributeValue>
      {
        { "pk", new AttributeValue { S = id.ToString() } },
        { "sk", new AttributeValue { S = id.ToString() } },
        { "id", new AttributeValue { S = id.ToString() } },
        { "title", new AttributeValue { S = "Entry" } },
      }
    };
    _dynamoDb.GetItemAsync(Arg.Any<GetItemRequest>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(getItemResponse));

    var result = await _repository.GetAsync(id, CancellationToken.None);

    Assert.NotNull(result);
    Assert.Equal(id, result.Id);
    Assert.Equal("Entry", result.Title);
  }
}
