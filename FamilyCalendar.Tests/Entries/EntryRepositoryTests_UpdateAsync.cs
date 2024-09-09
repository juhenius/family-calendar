using System.Net;
using Amazon.DynamoDBv2.Model;
using FamilyCalendar.Tests.Entries;
using NSubstitute;

namespace FamilyCalendar.Entries.Tests;

public partial class EntryRepositoryTests
{
  [Fact]
  public async Task UpdateAsync_UsesGivenTable()
  {
    var entry = EntryTestUtils.CreateTestEntry();
    var putItemResponse = new PutItemResponse { HttpStatusCode = HttpStatusCode.OK };
    _dynamoDb.PutItemAsync(Arg.Any<PutItemRequest>(), Arg.Any<CancellationToken>()).Returns(putItemResponse);

    await _repository.UpdateAsync(entry, CancellationToken.None);

    await _dynamoDb.Received(1).PutItemAsync(Arg.Is<PutItemRequest>(request =>
        request.TableName == _testTableName
    ), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task UpdateAsync_UsesCalendarIdAsPartitionKey()
  {
    var entry = EntryTestUtils.CreateTestEntry();
    var putItemResponse = new PutItemResponse { HttpStatusCode = HttpStatusCode.OK };
    _dynamoDb.PutItemAsync(Arg.Any<PutItemRequest>(), Arg.Any<CancellationToken>()).Returns(putItemResponse);

    await _repository.UpdateAsync(entry, CancellationToken.None);

    await _dynamoDb.Received(1).PutItemAsync(Arg.Is<PutItemRequest>(request =>
      request.Item["pk"].S == entry.CalendarId.ToString()
    ), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task UpdateAsync_UsesEntryIdWithPrefixAsSortingKey()
  {
    var entry = EntryTestUtils.CreateTestEntry();
    var putItemResponse = new PutItemResponse { HttpStatusCode = HttpStatusCode.OK };
    _dynamoDb.PutItemAsync(Arg.Any<PutItemRequest>(), Arg.Any<CancellationToken>()).Returns(putItemResponse);

    await _repository.UpdateAsync(entry, CancellationToken.None);

    await _dynamoDb.Received(1).PutItemAsync(Arg.Is<PutItemRequest>(request =>
      request.Item["sk"].S == entry.Id.ToEntrySk()
    ), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task UpdateAsync_SetsUpdatedAt()
  {
    var entry = EntryTestUtils.CreateTestEntry();
    var now = DateTimeOffset.UtcNow;
    var putItemResponse = new PutItemResponse { HttpStatusCode = HttpStatusCode.OK };
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
    var entry = EntryTestUtils.CreateTestEntry();
    var putItemResponse = new PutItemResponse { HttpStatusCode = HttpStatusCode.OK };
    _dynamoDb.PutItemAsync(Arg.Any<PutItemRequest>(), Arg.Any<CancellationToken>()).Returns(putItemResponse);

    await _repository.UpdateAsync(entry, CancellationToken.None);

    await _dynamoDb.Received(1).PutItemAsync(Arg.Is<PutItemRequest>(request =>
      request.Item["id"].S == entry.Id.ToString() &&
      request.Item["title"].S == entry.Title &&
      IsEqualTo(request.Item["date"].S, entry.Date) &&
      request.Item["location"].S == entry.Location &&
      request.Item["participants"].L[0].S == entry.Participants[0]
    ), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task UpdateAsync_ReturnsWhenEntryIsUpdated()
  {
    var entry = EntryTestUtils.CreateTestEntry();
    var putItemResponse = new PutItemResponse { HttpStatusCode = HttpStatusCode.OK };
    _dynamoDb.PutItemAsync(Arg.Any<PutItemRequest>(), Arg.Any<CancellationToken>()).Returns(putItemResponse);

    var exception = await Xunit.Record.ExceptionAsync(() => _repository.UpdateAsync(entry, CancellationToken.None));

    Assert.Null(exception);
  }

  [Fact]
  public async Task UpdateAsync_ThrowsWhenUpdateFails()
  {
    var entry = EntryTestUtils.CreateTestEntry();
    var putItemResponse = new PutItemResponse { HttpStatusCode = HttpStatusCode.InternalServerError };
    _dynamoDb.PutItemAsync(Arg.Any<PutItemRequest>(), Arg.Any<CancellationToken>()).Returns(putItemResponse);

    await Assert.ThrowsAnyAsync<Exception>(() => _repository.UpdateAsync(entry, CancellationToken.None));
  }
}
