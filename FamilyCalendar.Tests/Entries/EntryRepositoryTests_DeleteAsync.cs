using System.Net;
using Amazon.DynamoDBv2.Model;
using NSubstitute;

namespace FamilyCalendar.Entries.Tests;

public partial class EntryRepositoryTests
{
  [Fact]
  public async Task DeleteAsync_UsesGivenTable()
  {
    var id = Guid.NewGuid();
    var deleteItemResponse = new DeleteItemResponse { HttpStatusCode = HttpStatusCode.OK };
    _dynamoDb.DeleteItemAsync(Arg.Any<DeleteItemRequest>(), Arg.Any<CancellationToken>()).Returns(deleteItemResponse);

    var result = await _repository.DeleteAsync(id, CancellationToken.None);

    await _dynamoDb.Received(1).DeleteItemAsync(Arg.Is<DeleteItemRequest>(request =>
        request.TableName == _testTableName
    ), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task DeleteAsync_DeletesItemWithMatchingId()
  {
    var id = Guid.NewGuid();
    var deleteItemResponse = new DeleteItemResponse { HttpStatusCode = HttpStatusCode.OK };
    _dynamoDb.DeleteItemAsync(Arg.Any<DeleteItemRequest>(), Arg.Any<CancellationToken>()).Returns(deleteItemResponse);

    await _repository.DeleteAsync(id, CancellationToken.None);

    await _dynamoDb.Received(1).DeleteItemAsync(Arg.Is<DeleteItemRequest>(request =>
        request.Key["pk"].S == id.ToString() &&
        request.Key["sk"].S == id.ToString()
    ), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task DeleteAsync_ReturnsTrue_WhenItemIsDeleted()
  {
    var id = Guid.NewGuid();
    var deleteItemResponse = new DeleteItemResponse { HttpStatusCode = HttpStatusCode.OK };
    _dynamoDb.DeleteItemAsync(Arg.Any<DeleteItemRequest>(), Arg.Any<CancellationToken>()).Returns(deleteItemResponse);

    var result = await _repository.DeleteAsync(id, CancellationToken.None);

    Assert.True(result);
  }

  [Fact]
  public async Task DeleteAsync_ReturnsFalse_WhenDeleteFails()
  {
    var id = Guid.NewGuid();
    var deleteItemResponse = new DeleteItemResponse { HttpStatusCode = HttpStatusCode.InternalServerError };
    _dynamoDb.DeleteItemAsync(Arg.Any<DeleteItemRequest>(), Arg.Any<CancellationToken>()).Returns(deleteItemResponse);

    var result = await _repository.DeleteAsync(id, CancellationToken.None);

    Assert.False(result);
  }
}
