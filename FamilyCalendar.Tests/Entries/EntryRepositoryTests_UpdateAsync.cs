using System.Net;
using Amazon.DynamoDBv2.Model;
using NSubstitute;

namespace FamilyCalendar.Entries.Tests;

public partial class EntryRepositoryTests
{
  [Fact]
  public async Task UpdateAsync_UsesGivenTable()
  {
    var entry = new EntryDto { Id = Guid.NewGuid(), Title = "New Entry" };
    var putItemResponse = new PutItemResponse { HttpStatusCode = HttpStatusCode.OK };
    _dynamoDb.PutItemAsync(Arg.Any<PutItemRequest>(), Arg.Any<CancellationToken>()).Returns(putItemResponse);

    var result = await _repository.UpdateAsync(entry, CancellationToken.None);

    await _dynamoDb.Received(1).PutItemAsync(Arg.Is<PutItemRequest>(request =>
        request.TableName == _testTableName
    ), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task UpdateAsync_SetsEntryId()
  {
    var entry = new EntryDto { Id = Guid.NewGuid(), Title = "New Entry" };
    var putItemResponse = new PutItemResponse { HttpStatusCode = HttpStatusCode.InternalServerError };
    _dynamoDb.PutItemAsync(Arg.Any<PutItemRequest>(), Arg.Any<CancellationToken>()).Returns(putItemResponse);

    await _repository.UpdateAsync(entry, CancellationToken.None);

    await _dynamoDb.Received(1).PutItemAsync(Arg.Is<PutItemRequest>(request =>
      request.Item["pk"].S == entry.Id.ToString() &&
      request.Item["sk"].S == entry.Id.ToString() &&
      request.Item["id"].S == entry.Id.ToString()
    ), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task UpdateAsync_SetsUpdatedAt()
  {
    var entry = new EntryDto { Id = Guid.NewGuid(), Title = "New Entry" };
    var now = DateTime.UtcNow;
    var putItemResponse = new PutItemResponse { HttpStatusCode = HttpStatusCode.InternalServerError };
    _dynamoDb.PutItemAsync(Arg.Any<PutItemRequest>(), Arg.Any<CancellationToken>()).Returns(putItemResponse);

    await _repository.UpdateAsync(entry, CancellationToken.None);

    await _dynamoDb.Received(1).PutItemAsync(Arg.Is<PutItemRequest>(request =>
      request.Item.ContainsKey("updatedAt") &&
      isCloseTo(request.Item["updatedAt"].S, now)
    ), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task UpdateAsync_SetsEntryAttributes()
  {
    var entry = new EntryDto { Id = Guid.NewGuid(), Title = "New Entry" };
    var putItemResponse = new PutItemResponse { HttpStatusCode = HttpStatusCode.InternalServerError };
    _dynamoDb.PutItemAsync(Arg.Any<PutItemRequest>(), Arg.Any<CancellationToken>()).Returns(putItemResponse);

    await _repository.UpdateAsync(entry, CancellationToken.None);

    await _dynamoDb.Received(1).PutItemAsync(Arg.Is<PutItemRequest>(request =>
      request.Item["title"].S == entry.Title
    ), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task UpdateAsync_ReturnsTrue_WhenItemIsCreated()
  {
    var entry = new EntryDto { Id = Guid.NewGuid(), Title = "New Entry" };
    var putItemResponse = new PutItemResponse { HttpStatusCode = HttpStatusCode.OK };
    _dynamoDb.PutItemAsync(Arg.Any<PutItemRequest>(), Arg.Any<CancellationToken>()).Returns(putItemResponse);

    var result = await _repository.UpdateAsync(entry, CancellationToken.None);

    Assert.True(result);
  }

  [Fact]
  public async Task UpdateAsync_ReturnsFalse_WhenCreateFails()
  {
    var entry = new EntryDto { Id = Guid.NewGuid(), Title = "New Entry" };
    var putItemResponse = new PutItemResponse { HttpStatusCode = HttpStatusCode.InternalServerError };
    _dynamoDb.PutItemAsync(Arg.Any<PutItemRequest>(), Arg.Any<CancellationToken>()).Returns(putItemResponse);

    var result = await _repository.UpdateAsync(entry, CancellationToken.None);

    Assert.False(result);
  }
}
