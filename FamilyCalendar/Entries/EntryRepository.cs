using System.Net;
using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Options;

namespace FamilyCalendar.Entries;

public class EntryRepository(IAmazonDynamoDB dynamoDb, IOptions<FamilyCalendarSettings> settings) : IEntryRepository
{
  private readonly IAmazonDynamoDB _dynamoDb = dynamoDb;
  private readonly string _tableName = settings.Value.DynamoDbTable;
  private readonly string _entriesByDateIndex = settings.Value.EntriesByDateIndex;

  public async Task<Entry?> GetAsync(Guid calendarId, Guid entryId, CancellationToken cancellationToken)
  {
    var getItemRequest = new GetItemRequest
    {
      TableName = _tableName,
      Key = new() {
        { "pk", new AttributeValue { S = calendarId.ToString() } },
        { "sk", new AttributeValue { S = entryId.ToEntrySk() } },
      }
    };

    var response = await _dynamoDb.GetItemAsync(getItemRequest, cancellationToken);
    return response.Item.Count == 0 ? null : ToEntry(response.Item);
  }

  public async Task<IEnumerable<Entry>> GetAllAsync(Guid calendarId, CancellationToken cancellationToken)
  {
    var request = new QueryRequest
    {
      TableName = _tableName,
      KeyConditionExpression = "pk = :pk and begins_with(sk, :skPrefix)",
      ExpressionAttributeValues = new() {
        { ":pk", new AttributeValue { S = calendarId.ToString() } },
        { ":skPrefix", new AttributeValue { S = EntryDto.SkPrefix } },
      }
    };

    var response = await _dynamoDb.QueryAsync(request, cancellationToken);
    return response.Items.Select(ToEntry).Where(x => x is not null).Select(e => e!);
  }

  public async Task<IEnumerable<Entry>> GetByDateRangeAsync(Guid calendarId, DateTimeOffset rangeStart, DateTimeOffset rangeEnd, CancellationToken cancellationToken = default)
  {
    var request = new QueryRequest
    {
      TableName = _tableName,
      IndexName = _entriesByDateIndex,
      KeyConditionExpression = "pk = :pk and #date between :rangeStart and :rangeEnd",
      ExpressionAttributeNames = new() {
        { "#date", "date" }
      },
      ExpressionAttributeValues = new() {
        { ":pk", new AttributeValue { S = calendarId.ToString() } },
        { ":rangeStart", new AttributeValue { S = rangeStart.ToUniversalTime().ToString("s") } },
        { ":rangeEnd", new AttributeValue { S = rangeEnd.ToUniversalTime().ToString("s") } },
      }
    };

    var response = await _dynamoDb.QueryAsync(request, cancellationToken);
    return response.Items.Select(ToEntry).Where(x => x is not null).Select(e => e!);
  }

  public async Task<bool> CreateAsync(Entry entry, CancellationToken cancellationToken)
  {
    var entryDto = entry.ToEntryDto();
    entryDto.UpdatedAt = DateTimeOffset.UtcNow;
    var attributes = ToAttributes(entryDto);

    var createItemRequest = new PutItemRequest
    {
      TableName = _tableName,
      Item = attributes,
      ConditionExpression = "attribute_not_exists(pk) and attribute_not_exists(sk)"
    };

    var response = await _dynamoDb.PutItemAsync(createItemRequest, cancellationToken);
    return response.HttpStatusCode == HttpStatusCode.OK;
  }

  public async Task<bool> UpdateAsync(Entry entry, CancellationToken cancellationToken)
  {
    var entryDto = entry.ToEntryDto();
    entryDto.UpdatedAt = DateTimeOffset.UtcNow;
    var attributes = ToAttributes(entryDto);

    var updateItemRequest = new PutItemRequest
    {
      TableName = _tableName,
      Item = attributes,
    };

    var response = await _dynamoDb.PutItemAsync(updateItemRequest, cancellationToken);
    return response.HttpStatusCode == HttpStatusCode.OK;
  }

  public async Task<bool> DeleteAsync(Guid calendarId, Guid entryId, CancellationToken cancellationToken)
  {
    var deletedItemRequest = new DeleteItemRequest
    {
      TableName = _tableName,
      Key = new() {
        { "pk", new AttributeValue { S = calendarId.ToString() } },
        { "sk", new AttributeValue { S = entryId.ToEntrySk() } },
      }
    };

    var response = await _dynamoDb.DeleteItemAsync(deletedItemRequest, cancellationToken);
    return response.HttpStatusCode == HttpStatusCode.OK;
  }

  private static Dictionary<string, AttributeValue> ToAttributes(EntryDto entryDto)
  {
    var json = JsonSerializer.Serialize(entryDto);
    return Document.FromJson(json).ToAttributeMap();
  }

  private static Entry? ToEntry(Dictionary<string, AttributeValue> attributes)
  {
    var json = Document.FromAttributeMap(attributes).ToJson();
    return JsonSerializer.Deserialize<EntryDto>(json)?.ToEntry();
  }
}