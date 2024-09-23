using Amazon.DynamoDBv2.Model;
using NSubstitute;

namespace FamilyCalendar.Entries.Tests;

public partial class EntryRepositoryTests
{
  [Fact]
  public async Task GetAsync_UsesGivenTable()
  {
    var calendarId = Guid.NewGuid();
    var entryId = Guid.NewGuid();
    var getItemResponse = new GetItemResponse { Item = [] };
    _dynamoDb.GetItemAsync(Arg.Any<GetItemRequest>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(getItemResponse));

    await _repository.GetAsync(calendarId, entryId, CancellationToken.None);

    await _dynamoDb.Received(1).GetItemAsync(Arg.Is<GetItemRequest>(request =>
        request.TableName == _testTableName
    ), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task GetAsync_FetchesFromMatchingCalendar()
  {
    var calendarId = Guid.NewGuid();
    var entryId = Guid.NewGuid();
    var getItemResponse = new GetItemResponse { Item = [] };
    _dynamoDb.GetItemAsync(Arg.Any<GetItemRequest>(), Arg.Any<CancellationToken>()).Returns(getItemResponse);

    await _repository.GetAsync(calendarId, entryId, CancellationToken.None);

    await _dynamoDb.Received(1).GetItemAsync(Arg.Is<GetItemRequest>(request =>
        request.Key["pk"].S == calendarId.ToString()
    ), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task GetAsync_FetchesEntryWithMatchingId()
  {
    var calendarId = Guid.NewGuid();
    var entryId = Guid.NewGuid();
    var getItemResponse = new GetItemResponse { Item = [] };
    _dynamoDb.GetItemAsync(Arg.Any<GetItemRequest>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(getItemResponse));

    await _repository.GetAsync(calendarId, entryId, CancellationToken.None);

    await _dynamoDb.Received(1).GetItemAsync(Arg.Is<GetItemRequest>(request =>
        request.Key["sk"].S == entryId.ToEntrySk()
    ), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task GetAsync_ReturnsNull_WhenEntryDoesNotExist()
  {
    var calendarId = Guid.NewGuid();
    var entryId = Guid.NewGuid();
    var getItemResponse = new GetItemResponse { Item = [] };
    _dynamoDb.GetItemAsync(Arg.Any<GetItemRequest>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(getItemResponse));

    var result = await _repository.GetAsync(calendarId, entryId, CancellationToken.None);

    Assert.Null(result);
  }

  [Fact]
  public async Task GetAsync_ReturnsEntry_WhenEntryExists()
  {
    var calendarId = Guid.NewGuid();
    var entryId = Guid.NewGuid();
    var getItemResponse = new GetItemResponse
    {
      Item = new() {
        { "id", new AttributeValue { S = entryId.ToString() } },
        { "calendarId", new AttributeValue { S = calendarId.ToString() } },
        { "title", new AttributeValue { S = "Entry" } },
        { "prompt", new AttributeValue { S = "Prompt" } },
        { "createdAt", new AttributeValue { S = "2024-07-13T20:55:23Z" } },
        { "timeZone", new AttributeValue { S = "Europe/Amsterdam" } },
      }
    };
    _dynamoDb.GetItemAsync(Arg.Any<GetItemRequest>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(getItemResponse));

    var result = await _repository.GetAsync(calendarId, entryId, CancellationToken.None);

    Assert.NotNull(result);
    Assert.Equal(entryId, result.Id);
    Assert.Equal(calendarId, result.CalendarId);
    Assert.Equal("Entry", result.Title);
  }
}
