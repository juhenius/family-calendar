using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace FamilyCalendar.Pages;

public class IndexModel : PageModel
{
  [BindProperty]
  public string? Text { get; set; }

  [BindProperty(SupportsGet = true)]
  public string? Id { get; set; }

  private readonly ILogger<IndexModel> logger;
  private readonly IAmazonDynamoDB dynamoDb;
  private readonly IRazorViewEngine viewEngine;

  public IndexModel(IAmazonDynamoDB dynamoDb, IRazorViewEngine viewEngine, ILogger<IndexModel> logger)
  {
    this.dynamoDb = dynamoDb;
    this.viewEngine = viewEngine;
    this.logger = logger;
  }

  public IActionResult OnGetTasks()
  {
    return Partial("_Tasks", this);
  }

  public async Task<IActionResult> OnPostAddTaskAsync()
  {
    var task = new Task
    {
      Id = Guid.NewGuid(),
      Text = Text!
    };

    var json = JsonSerializer.Serialize(task);
    var item = Document.FromJson(json).ToAttributeMap();

    var createItemRequest = new PutItemRequest
    {
      TableName = "FamilyCalendar",
      Item = item
    };

    var response = await dynamoDb.PutItemAsync(createItemRequest);
    if (response.HttpStatusCode != HttpStatusCode.OK)
    {
      logger.LogError("Failed to write DB item");
    }

    var html = $"{await RenderPartialViewToStringAsync("_Tasks", this)} {await RenderPartialViewToStringAsync("_AddTask", this)}";
    return Content(html, "text/html");
  }

  public async Task<IActionResult> OnDeleteDeleteTask()
  {
    var deletedItemRequest = new DeleteItemRequest
    {
      TableName = "FamilyCalendar",
      Key = new Dictionary<string, AttributeValue>
            {
                { "pk", new AttributeValue { S = Id.ToString() } },
                { "sk", new AttributeValue { S = Id.ToString() } }
            }
    };

    var response = await dynamoDb.DeleteItemAsync(deletedItemRequest);
    if (response.HttpStatusCode != HttpStatusCode.OK)
    {
      logger.LogError("Failed to delete DB item");
    }

    return Partial("_Tasks", this);
  }

  public async Task<IEnumerable<Task>> GetTasksAsync()
  {
    var scanRequest = new ScanRequest
    {
      TableName = "FamilyCalendar",
    };
    var response = await dynamoDb.ScanAsync(scanRequest);
    return response.Items.Select(x =>
    {
      var json = Document.FromAttributeMap(x).ToJson();
      return JsonSerializer.Deserialize<Task>(json);
    })!;
  }

  private async Task<string> RenderPartialViewToStringAsync(string viewName, object model)
  {
    var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
    {
      Model = model
    };

    var viewResult = viewEngine.GetView(null, viewName, false);
    if (viewResult.View == null)
    {
      viewResult = viewEngine.FindView(PageContext, viewName, false);
      if (viewResult.View == null)
      {
        throw new ArgumentNullException($"A view with the name {viewName} could not be found");
      }
    }

    using var stringWriter = new StringWriter();
    var viewContext = new ViewContext(
    PageContext,
    viewResult.View,
    viewData,
    TempData,
    stringWriter,
    new HtmlHelperOptions()
    );

    await viewResult.View.RenderAsync(viewContext);
    return stringWriter.ToString();
  }
}


public class Task
{
  [JsonPropertyName("pk")]
  public string Pk => Id.ToString();
  [JsonPropertyName("sk")]
  public string Sk => Id.ToString();
  public Guid Id { get; init; } = default!;
  public string Text { get; init; } = default!;
}
