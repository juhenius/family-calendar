using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace FamilyCalendar.Entries;

public class OpenAiEntryParser(IChatCompletionService chatCompletionService) : IEntryParser
{
  private readonly IChatCompletionService _chatCompletionService = chatCompletionService;

  public async Task<Entry> ParseFromString(string input, Guid calendarId, CancellationToken cancellationToken = default)
  {
    var chatHistory = new ChatHistory();
    chatHistory.AddSystemMessage(
      @$"You are an expert calendar organizer who specializes in turning natural
      language description of an calendar event into a standardized json
      representation. Your job is to accept text description of an event and reply
      with a single json object matching the given schema without any formatting:
      {{
        ""type"": ""object"",
        ""properties"": {{
          ""title"": {{
            ""type"": ""string"",
            ""description"": ""Title and description of the event""
          }},
          ""date"": {{
            ""type"": ""string"",
            ""format"": ""date-time"",
            ""description"": ""Date and time of the event in UTC using format 'yyyy-MM-ddTHH:mm:ssZ'""
          }},
          ""location"": {{
            ""type"": [""string"", ""null""],
            ""description"": ""Location of the event""
          }},
          ""participants"": {{
            ""type"": ""array"",
            ""description"": ""Names of the persons participating in the event"",
            ""items"": {{
              ""type"": ""string""
            }}
          }}
        }}
      }}

      Current in UTC time is {DateTimeOffset.UtcNow:s}.
      Current timezone offset is {DateTimeOffset.Now.Offset}

      Known participants are:
      - Matti
      - Teppo
      - Seppo

      Please remove participants from the event title and move them to participants.

      Remember to be extra precise on the dates as you want to make sure calendar
      events are accurate. Use UTC timezone for all date and time related fields.
      Please ensure that weeks start from Monday and not Sunday.
      If you are unsure, remember that events are most likely created into the future.
      If no date is provided, use current time."
    );

    chatHistory.AddUserMessage($"Please parse this calendar event: {input}");

    var executionSettings = new OpenAIPromptExecutionSettings { Temperature = 0.2 };
    var response = await _chatCompletionService.GetChatMessageContentsAsync(chatHistory, executionSettings, null, cancellationToken);
    var lastMessage = response[response.Count - 1];
    var json = JsonSerializer.Deserialize<EntryJson>(lastMessage.ToString()) ?? throw new ArgumentException("unable to parse input");

    return new Entry()
    {
      Id = Guid.NewGuid(),
      CalendarId = calendarId,
      Title = json.Title,
      Date = ParseDate(json.Date),
      Location = json.Location,
      Participants = json.Participants ?? [],
    };
  }

  private static DateTimeOffset ParseDate(string input)
  {
    var format = "yyyy-MM-ddTHH:mm:ssZ";
    return DateTimeOffset.TryParseExact(input, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result)
      ? result
      : throw new ArgumentException("unable to parse date");
  }
}

sealed internal class EntryJson
{
  [JsonPropertyName("title")]
  public required string Title { get; set; } = default!;

  [JsonPropertyName("date")]
  public string Date { get; set; } = default!;

  [JsonPropertyName("location")]
  public string? Location { get; set; } = default;

  [JsonPropertyName("participants")]
  public List<string> Participants { get; set; } = default!;
}
