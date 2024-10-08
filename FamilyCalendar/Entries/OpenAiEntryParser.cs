using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace FamilyCalendar.Entries;

public class OpenAiEntryParser(IChatCompletionService chatCompletionService) : IEntryParser
{
  private readonly IChatCompletionService _chatCompletionService = chatCompletionService;
  private const double Temperature = 0.2;

  public async Task<EntryParseResult> ParseFromString(string prompt, DateTimeOffset localTime, string timeZone, CancellationToken cancellationToken = default)
  {
    var chatHistory = InitChatHistory(localTime, timeZone);
    var unvalidatedEntryJson = await ParseEntry(prompt, chatHistory, cancellationToken);
    var validatedEntryJson = await ValidateEntry(unvalidatedEntryJson, chatHistory, cancellationToken);
    var entry = JsonSerializer.Deserialize<EntryJson>(validatedEntryJson) ?? throw new ArgumentException("unable to parse input");

    return new EntryParseResult()
    {
      Title = entry.Title,
      Date = ParseDate(entry.Date),
      Location = entry.Location,
      Participants = entry.Participants ?? [],
      Recurrence = entry.Recurrence ?? [],
      Prompt = prompt,
      LocalTime = localTime,
      TimeZone = entry.TimeZone ?? timeZone,
    };
  }

  private static ChatHistory InitChatHistory(DateTimeOffset localTime, string timeZone)
  {
    var result = new ChatHistory();
    result.AddSystemMessage(
      @$"You are an expert calendar organizer who specializes in turning natural
      language description of an calendar event into a standardized json
      representation. Your job is to accept text description of an event and reply
      with a single json object matching the given schema without any formatting:
      {{
        ""type"": ""object"",
        ""properties"": {{
          ""title"": {{
            ""type"": ""string"",
            ""description"": ""Title and description of the event, without recurrence related info.""
          }},
          ""date"": {{
            ""type"": ""string"",
            ""format"": ""date-time"",
            ""description"": ""Date and time of the event in UTC using format 'yyyy-MM-ddTHH:mm:ssZ'"",
            ""example"": ""2024-06-10T20:55:23Z""
          }},
          ""location"": {{
            ""type"": [""string"", ""null""],
            ""description"": ""Location of the event""
          }},
          ""timeZone"": {{
            ""type"": [""string"", ""null""],
            ""description"": ""Time zone of the event if it can be deduced from the location, otherwise use the current local timezone""
          }},
          ""participants"": {{
            ""type"": ""array"",
            ""description"": ""Names of the persons participating in the event. Empty array for events without participants."",
            ""items"": {{
              ""type"": ""string"",
              ""example"": ""Seppo""
            }}
          }},
          ""recurrence"": {{
            ""type"": ""array"",
            ""description"": ""List of recurrence rule patterns in RFC 5545 format. Empty array for non recurring events."",
            ""items"": {{
              ""type"": ""string"",
              ""description"": ""Rule pattern in RFC 5545 format"",
              ""example"": ""FREQ=DAILY;UNTIL=20240831T115959""
            }}
          }}
        }}
      }}

      Current UTC time is {localTime.ToUniversalTime():s}.
      Current local time is {localTime:s}.
      Current timezone offset is {localTime.Offset}
      Current timezone is {timeZone}

      Known participants are:
      - Matti
      - Teppo
      - Seppo

      Please remove participants from the event title and move them to participants.

      Remember to be extra precise on the dates as you want to make sure calendar
      events are accurate. Use UTC timezone for all date and time related fields.
      For relative dates use current time as reference.
      If location is in different time zone, use that for the event time while still keeping the actual value in UTC.
      Please ensure that weeks start from Monday and not Sunday.
      If you are unsure, remember that events are most likely created into the future.
      If no date is provided, use current time."
    );

    return result;
  }

  private async Task<string> ParseEntry(string input, ChatHistory chatHistory, CancellationToken cancellationToken)
  {
    chatHistory.AddUserMessage($"Please parse this calendar event: {input}");

    var executionSettings = new OpenAIPromptExecutionSettings { Temperature = Temperature };
    var response = await _chatCompletionService.GetChatMessageContentsAsync(chatHistory, executionSettings, null, cancellationToken);
    return response[response.Count - 1].ToString();
  }

  private async Task<string> ValidateEntry(string entryJson, ChatHistory chatHistory, CancellationToken cancellationToken)
  {
    chatHistory.AddUserMessage(
      @$"Please validate the accuracy of the following json and respond only with the fixed version.
      Remove also any markdown or other formatting formatting that is not part of json.
      {entryJson}"
    );

    var executionSettings = new OpenAIPromptExecutionSettings { Temperature = Temperature };
    var response = await _chatCompletionService.GetChatMessageContentsAsync(chatHistory, executionSettings, null, cancellationToken);
    return response[response.Count - 1].ToString();
  }

  private static DateTimeOffset ParseDate(string input)
  {
    var format = "yyyy-MM-ddTHH:mm:ssZ";
    return DateTimeOffset.TryParseExact(input, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result)
      ? result
      : throw new ArgumentException("unable to parse date");
  }
}

internal sealed class EntryJson
{
  [JsonPropertyName("title")]
  public required string Title { get; set; } = default!;

  [JsonPropertyName("date")]
  public string Date { get; set; } = default!;

  [JsonPropertyName("location")]
  public string? Location { get; set; } = default;

  [JsonPropertyName("timeZone")]
  public string? TimeZone { get; set; } = default;

  [JsonPropertyName("participants")]
  public List<string> Participants { get; set; } = default!;

  [JsonPropertyName("recurrence")]
  public List<string> Recurrence { get; set; } = default!;
}
