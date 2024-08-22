using System.Net;
using Amazon.DynamoDBv2.Model;
using NSubstitute;

namespace FamilyCalendar.Entries.Tests;

public partial class EntryRepositoryTests
{
  [Fact]
  public async Task DeleteAsync_UsesGivenTable()
  {
    var calendarId = Guid.NewGuid();
    var entryId = Guid.NewGuid();
    var deleteItemResponse = new DeleteItemResponse { HttpStatusCode = HttpStatusCode.OK };
    _dynamoDb.DeleteItemAsync(Arg.Any<DeleteItemRequest>(), Arg.Any<CancellationToken>()).Returns(deleteItemResponse);

    await _repository.DeleteAsync(calendarId, entryId, CancellationToken.None);

    await _dynamoDb.Received(1).DeleteItemAsync(Arg.Is<DeleteItemRequest>(request =>
        request.TableName == _testTableName
    ), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task DeleteAsync_DeletesFromMatchingCalendar()
  {
    var calendarId = Guid.NewGuid();
    var entryId = Guid.NewGuid();
    var deleteItemResponse = new DeleteItemResponse { HttpStatusCode = HttpStatusCode.OK };
    _dynamoDb.DeleteItemAsync(Arg.Any<DeleteItemRequest>(), Arg.Any<CancellationToken>()).Returns(deleteItemResponse);

    await _repository.DeleteAsync(calendarId, entryId, CancellationToken.None);

    await _dynamoDb.Received(1).DeleteItemAsync(Arg.Is<DeleteItemRequest>(request =>
        request.Key["pk"].S == calendarId.ToString()
    ), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task DeleteAsync_DeletesEntryWithMatchingId()
  {
    var calendarId = Guid.NewGuid();
    var entryId = Guid.NewGuid();
    var deleteItemResponse = new DeleteItemResponse { HttpStatusCode = HttpStatusCode.OK };
    _dynamoDb.DeleteItemAsync(Arg.Any<DeleteItemRequest>(), Arg.Any<CancellationToken>()).Returns(deleteItemResponse);

    await _repository.DeleteAsync(calendarId, entryId, CancellationToken.None);

    await _dynamoDb.Received(1).DeleteItemAsync(Arg.Is<DeleteItemRequest>(request =>
        request.Key["sk"].S == entryId.ToEntrySk()
    ), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task DeleteAsync_ReturnsWhenEntryIsDeleted()
  {
    var calendarId = Guid.NewGuid();
    var entryId = Guid.NewGuid();
    var deleteItemResponse = new DeleteItemResponse { HttpStatusCode = HttpStatusCode.OK };
    _dynamoDb.DeleteItemAsync(Arg.Any<DeleteItemRequest>(), Arg.Any<CancellationToken>()).Returns(deleteItemResponse);

    var exception = await Xunit.Record.ExceptionAsync(() => _repository.DeleteAsync(calendarId, entryId, CancellationToken.None));

    Assert.Null(exception);

  }

  [Fact]
  public async Task DeleteAsync_ThrowsWhenDeleteFails()
  {
    var calendarId = Guid.NewGuid();
    var entryId = Guid.NewGuid();
    var deleteItemResponse = new DeleteItemResponse { HttpStatusCode = HttpStatusCode.InternalServerError };
    _dynamoDb.DeleteItemAsync(Arg.Any<DeleteItemRequest>(), Arg.Any<CancellationToken>()).Returns(deleteItemResponse);

    await Assert.ThrowsAnyAsync<Exception>(() => _repository.DeleteAsync(calendarId, entryId, CancellationToken.None));
  }
}
