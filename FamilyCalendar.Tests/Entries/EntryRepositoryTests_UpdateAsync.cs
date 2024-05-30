using System.Net;
using Amazon.DynamoDBv2.Model;
using NSubstitute;

namespace FamilyCalendar.Entries.Tests;

public partial class EntryRepositoryTests
{
  [Fact]
  public async Task UpdateAsync_UsesGivenTable()
  {
    var entry = CreateTestEntry();
    var putItemResponse = new PutItemResponse { HttpStatusCode = HttpStatusCode.OK };
    _dynamoDb.PutItemAsync(Arg.Any<PutItemRequest>(), Arg.Any<CancellationToken>()).Returns(putItemResponse);

    var result = await _repository.UpdateAsync(entry, CancellationToken.None);

    await _dynamoDb.Received(1).PutItemAsync(Arg.Is<PutItemRequest>(request =>
        request.TableName == _testTableName
    ), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task UpdateAsync_UsesCalendarIdAsPartitionKey()
  {
    var entry = CreateTestEntry();
    var putItemResponse = new PutItemResponse { HttpStatusCode = HttpStatusCode.InternalServerError };
    _dynamoDb.PutItemAsync(Arg.Any<PutItemRequest>(), Arg.Any<CancellationToken>()).Returns(putItemResponse);

    await _repository.UpdateAsync(entry, CancellationToken.None);

    await _dynamoDb.Received(1).PutItemAsync(Arg.Is<PutItemRequest>(request =>
      request.Item["pk"].S == entry.CalendarId.ToString()
    ), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task UpdateAsync_UsesEntryIdWithPrefixAsSortingKey()
  {
    var entry = CreateTestEntry();
    var putItemResponse = new PutItemResponse { HttpStatusCode = HttpStatusCode.InternalServerError };
    _dynamoDb.PutItemAsync(Arg.Any<PutItemRequest>(), Arg.Any<CancellationToken>()).Returns(putItemResponse);

    await _repository.UpdateAsync(entry, CancellationToken.None);

    await _dynamoDb.Received(1).PutItemAsync(Arg.Is<PutItemRequest>(request =>
      request.Item["sk"].S == entry.Id.ToEntrySk()
    ), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task UpdateAsync_SetsUpdatedAt()
  {
    var entry = CreateTestEntry();
    var now = DateTime.UtcNow;
    var putItemResponse = new PutItemResponse { HttpStatusCode = HttpStatusCode.InternalServerError };
    _dynamoDb.PutItemAsync(Arg.Any<PutItemRequest>(), Arg.Any<CancellationToken>()).Returns(putItemResponse);

    await _repository.UpdateAsync(entry, CancellationToken.None);

    await _dynamoDb.Received(1).PutItemAsync(Arg.Is<PutItemRequest>(request =>
      request.Item.ContainsKey("updatedAt") &&
      IsCloseTo(request.Item["updatedAt"].S, now)
    ), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task UpdateAsync_SetsEntryAttributes()
  {
    var entry = CreateTestEntry();
    var putItemResponse = new PutItemResponse { HttpStatusCode = HttpStatusCode.InternalServerError };
    _dynamoDb.PutItemAsync(Arg.Any<PutItemRequest>(), Arg.Any<CancellationToken>()).Returns(putItemResponse);

    await _repository.UpdateAsync(entry, CancellationToken.None);

    await _dynamoDb.Received(1).PutItemAsync(Arg.Is<PutItemRequest>(request =>
      request.Item["id"].S == entry.Id.ToString() &&
      request.Item["title"].S == entry.Title &&
      IsEqualTo(request.Item["date"].S, entry.Date) &&
      request.Item["member"].S == entry.Member
    ), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task UpdateAsync_ReturnsTrue_WhenEntryIsUpdated()
  {
    var entry = CreateTestEntry();
    var putItemResponse = new PutItemResponse { HttpStatusCode = HttpStatusCode.OK };
    _dynamoDb.PutItemAsync(Arg.Any<PutItemRequest>(), Arg.Any<CancellationToken>()).Returns(putItemResponse);

    var result = await _repository.UpdateAsync(entry, CancellationToken.None);

    Assert.True(result);
  }

  [Fact]
  public async Task UpdateAsync_ReturnsFalse_WhenUpdateFails()
  {
    var entry = CreateTestEntry();
    var putItemResponse = new PutItemResponse { HttpStatusCode = HttpStatusCode.InternalServerError };
    _dynamoDb.PutItemAsync(Arg.Any<PutItemRequest>(), Arg.Any<CancellationToken>()).Returns(putItemResponse);

    var result = await _repository.UpdateAsync(entry, CancellationToken.None);

    Assert.False(result);
  }
}
