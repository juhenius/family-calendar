using System.Net;
using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Options;

namespace FamilyCalendar.Entries;

public class EntryRepository : IEntryRepository
{
  private readonly IAmazonDynamoDB _dynamoDb;
  private readonly string _tableName;

  public EntryRepository(IAmazonDynamoDB dynamoDb, IOptions<FamilyCalendarSettings> settings)
  {
    _dynamoDb = dynamoDb;
    _tableName = settings.Value.DynamoDbTable;
  }

  public async Task<EntryDto?> GetAsync(Guid id, CancellationToken cancellationToken)
  {
    var getItemRequest = new GetItemRequest
    {
      TableName = _tableName,
      Key = new Dictionary<string, AttributeValue> {
        { "pk", new AttributeValue { S = id.ToString() } },
        { "sk", new AttributeValue { S = id.ToString() } },
      }
    };

    var response = await _dynamoDb.GetItemAsync(getItemRequest, cancellationToken);
    if (response.Item.Count == 0)
    {
      return null;
    }

    return ToEntryDto(response.Item);
  }

  public async Task<IEnumerable<EntryDto>> GetAllAsync(CancellationToken cancellationToken)
  {
    var scanRequest = new ScanRequest
    {
      TableName = _tableName
    };

    var response = await _dynamoDb.ScanAsync(scanRequest, cancellationToken);
    return response.Items.Select(ToEntryDto).Where(x => x is not null).Select(e => e!);
  }

  public async Task<bool> CreateAsync(EntryDto entry, CancellationToken cancellationToken)
  {
    entry.UpdatedAt = DateTime.UtcNow;
    var attributes = ToAttributes(entry);

    var createItemRequest = new PutItemRequest
    {
      TableName = _tableName,
      Item = attributes,
      ConditionExpression = "attribute_not_exists(pk) and attribute_not_exists(sk)"
    };

    var response = await _dynamoDb.PutItemAsync(createItemRequest, cancellationToken);
    return response.HttpStatusCode == HttpStatusCode.OK;
  }

  public async Task<bool> UpdateAsync(EntryDto entry, CancellationToken cancellationToken)
  {
    entry.UpdatedAt = DateTime.UtcNow;
    var attributes = ToAttributes(entry);

    var updateItemRequest = new PutItemRequest
    {
      TableName = _tableName,
      Item = attributes,
    };

    var response = await _dynamoDb.PutItemAsync(updateItemRequest, cancellationToken);
    return response.HttpStatusCode == HttpStatusCode.OK;
  }

  public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
  {
    var deletedItemRequest = new DeleteItemRequest
    {
      TableName = _tableName,
      Key = new Dictionary<string, AttributeValue>() {
        { "pk", new AttributeValue { S = id.ToString() } },
        { "sk", new AttributeValue { S = id.ToString() } },
      }
    };

    var response = await _dynamoDb.DeleteItemAsync(deletedItemRequest, cancellationToken);
    return response.HttpStatusCode == HttpStatusCode.OK;
  }

  private Dictionary<string, AttributeValue> ToAttributes(EntryDto entry)
  {
    string json = JsonSerializer.Serialize(entry);
    return Document.FromJson(json).ToAttributeMap();
  }

  private EntryDto? ToEntryDto(Dictionary<string, AttributeValue> attributes)
  {
    var json = Document.FromAttributeMap(attributes).ToJson();
    return JsonSerializer.Deserialize<EntryDto>(json);
  }
}