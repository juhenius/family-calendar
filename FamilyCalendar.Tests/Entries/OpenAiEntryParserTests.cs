using FamilyCalendar.Entries;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using NSubstitute;

namespace FamilyCalendar.Tests.Entries;

public class OpenAiEntryParserTests
{
  private readonly OpenAiEntryParser _openAiEntryParser;
  private readonly IChatCompletionService _chatCompletionService = Substitute.For<IChatCompletionService>();

  public OpenAiEntryParserTests()
  {
    _openAiEntryParser = new OpenAiEntryParser(_chatCompletionService);
  }

  [Fact]
  public async Task ParseFromString_SendsInputToAi()
  {
    var input = "expected input";
    SetDefaultReply();

    await ExecuteParseFromStringWithDefaults(input: input);

    await _chatCompletionService.Received().GetChatMessageContentsAsync(
      Arg.Is<ChatHistory>(chat => ChatHistoryContains(chat, input)),
      Arg.Any<PromptExecutionSettings>(),
      Arg.Any<Kernel?>(),
      Arg.Any<CancellationToken>()
    );
  }

  [Fact]
  public async Task ParseFromString_UsesGivenTimeZone()
  {
    var timeZone = "Europe/Amsterdam";
    SetDefaultReply();

    await ExecuteParseFromStringWithDefaults(timeZone: timeZone);

    await _chatCompletionService.Received().GetChatMessageContentsAsync(
      Arg.Is<ChatHistory>(chat => ChatHistoryContains(chat, timeZone)),
      Arg.Any<PromptExecutionSettings>(),
      Arg.Any<Kernel?>(),
      Arg.Any<CancellationToken>()
    );
  }

  [Fact]
  public async Task ParseFromString_UsesGivenLocalTime()
  {
    var localTime = new DateTimeOffset(2024, 5, 31, 14, 5, 0, TimeSpan.FromHours(8));
    SetDefaultReply();

    await ExecuteParseFromStringWithDefaults(localTime: localTime);

    await _chatCompletionService.Received().GetChatMessageContentsAsync(
      Arg.Is<ChatHistory>(chat => ChatHistoryContains(chat, localTime.ToUniversalTime().ToString("s"))
        && ChatHistoryContains(chat, localTime.ToString("s"))
        && ChatHistoryContains(chat, localTime.Offset.ToString())),
      Arg.Any<PromptExecutionSettings>(),
      Arg.Any<Kernel?>(),
      Arg.Any<CancellationToken>()
    );
  }

  [Fact]
  public async Task ParseFromString_ValidatesParseResultWithAi()
  {
    var reply = new ReplyBuilder().WithTitle("title").WithDate("2024-07-12T20:00:00Z").Build();
    SetReply(reply);

    await ExecuteParseFromStringWithDefaults();

    await _chatCompletionService.Received().GetChatMessageContentsAsync(
      Arg.Is<ChatHistory>(chat => ChatHistoryContains(chat, reply)),
      Arg.Any<PromptExecutionSettings>(),
      Arg.Any<Kernel?>(),
      Arg.Any<CancellationToken>()
    );
  }

  [Fact]
  public async Task ParseFromString_ParsesTitleFromResponse()
  {
    var title = "expected title";
    SetReply(new ReplyBuilder().WithTitle(title).Build());

    var result = await ExecuteParseFromStringWithDefaults();

    Assert.Equal(title, result.Title);
  }

  [Fact]
  public async Task ParseFromString_ThrowsWhenTitleIsMissingFromResponse()
  {
    SetReply(new ReplyBuilder().WithTitle(null).Build());

    await Assert.ThrowsAnyAsync<Exception>(() => ExecuteParseFromStringWithDefaults());
  }

  [Fact]
  public async Task ParseFromString_ParsesDateFromResponse()
  {
    SetReply(new ReplyBuilder().WithDate("2024-06-10T20:00:00Z").Build());

    var result = await ExecuteParseFromStringWithDefaults();

    Assert.Equal(new DateTimeOffset(2024, 6, 10, 20, 0, 0, TimeSpan.Zero), result.Date);
  }

  [Fact]
  public async Task ParseFromString_ThrowsWhenDateIsMissingFromResponse()
  {
    SetReply(new ReplyBuilder().WithDate(null).Build());

    await Assert.ThrowsAnyAsync<Exception>(() => ExecuteParseFromStringWithDefaults());
  }

  [Fact]
  public async Task ParseFromString_ThrowsWhenDateIsNotCorrectlyFormatedInResponse()
  {
    SetReply(new ReplyBuilder().WithDate("2024-06-10T20:00:00").Build());

    await Assert.ThrowsAnyAsync<Exception>(() => ExecuteParseFromStringWithDefaults());
  }

  [Fact]
  public async Task ParseFromString_ParsesLocationFromResponse()
  {
    var location = "expected location";
    SetReply(new ReplyBuilder().WithLocation(location).Build());

    var result = await ExecuteParseFromStringWithDefaults();

    Assert.Equal(location, result.Location);
  }

  [Fact]
  public async Task ParseFromString_ReturnsEmptyLocationWhenItIsMissingFromResponse()
  {
    SetReply(new ReplyBuilder().WithLocation(null).Build());

    var result = await ExecuteParseFromStringWithDefaults();

    Assert.Null(result.Location);
  }

  [Fact]
  public async Task ParseFromString_ParsesParticipantsFromResponse()
  {
    List<string> participants = ["expected participant 1", "expected participant 1"];
    SetReply(new ReplyBuilder().WithParticipants(participants).Build());

    var result = await ExecuteParseFromStringWithDefaults();

    Assert.Equal(participants, result.Participants);
  }

  [Fact]
  public async Task ParseFromString_ReturnsEmptyParticipantsWhenItIsMissingFromResponse()
  {
    SetReply(new ReplyBuilder().WithParticipants(null).Build());

    var result = await ExecuteParseFromStringWithDefaults();

    Assert.Empty(result.Participants);
  }

  [Fact]
  public async Task ParseFromString_ParsesRecurrenceFromResponse()
  {
    List<string> recurrence = ["expected recurrence 1", "expected recurrence 1"];
    SetReply(new ReplyBuilder().WithRecurrence(recurrence).Build());

    var result = await ExecuteParseFromStringWithDefaults();

    Assert.Equal(recurrence, result.Recurrence);
  }

  [Fact]
  public async Task ParseFromString_ReturnsEmptyRecurrenceNonRecurringEntry()
  {
    SetReply(new ReplyBuilder().WithRecurrence(null).Build());

    var result = await ExecuteParseFromStringWithDefaults();

    Assert.Empty(result.Recurrence);
  }

  [Fact]
  public async Task ParseFromString_ParsesTimeZoneFromResponse()
  {
    var timeZone = "Europe/Amsterdam";
    SetReply(new ReplyBuilder().WithTimeZone(timeZone).Build());

    var result = await ExecuteParseFromStringWithDefaults();

    Assert.Equal(timeZone, result.TimeZone);
  }

  [Fact]
  public async Task ParseFromString_AddsLocalTimeZoneToResultIfMissingFromResponse()
  {
    var timeZone = "Europe/Amsterdam";
    SetReply(new ReplyBuilder().WithTimeZone(null).Build());

    var result = await ExecuteParseFromStringWithDefaults(timeZone: timeZone);

    Assert.Equal(timeZone, result.TimeZone);
  }

  [Fact]
  public async Task ParseFromString_AddsPromptToResult()
  {
    var input = "expected input";
    SetDefaultReply();

    var result = await ExecuteParseFromStringWithDefaults(input: input);

    Assert.Equal(input, result.Prompt);
  }

  [Fact]
  public async Task ParseFromString_AddsLocalTimeToResult()
  {
    var localTime = new DateTimeOffset(2024, 5, 31, 14, 5, 0, TimeSpan.FromHours(8));
    SetDefaultReply();

    var result = await ExecuteParseFromStringWithDefaults(localTime: localTime);

    Assert.Equal(localTime, result.LocalTime, TimeSpan.FromSeconds(1));
  }

  private Task<EntryParseResult> ExecuteParseFromStringWithDefaults(
    string? input = null,
    DateTimeOffset? localTime = null,
    string? timeZone = null,
    CancellationToken? cancellationToken = null)
  {
    return _openAiEntryParser.ParseFromString(
      input ?? "not relevan",
      localTime ?? DateTimeOffset.UtcNow,
      timeZone ?? "Europe/Helsinki",
      cancellationToken ?? CancellationToken.None);
  }

  private void SetDefaultReply()
  {
    SetReply(new ReplyBuilder().Build());
  }

  private void SetReply(string message)
  {
    IReadOnlyList<ChatMessageContent> chatResponse = [new ChatMessageContent(AuthorRole.Assistant, message)];
    _chatCompletionService.GetChatMessageContentsAsync(
      Arg.Any<ChatHistory>(),
      Arg.Any<PromptExecutionSettings>(),
      Arg.Any<Kernel?>(),
      Arg.Any<CancellationToken>()
    ).Returns(Task.FromResult(chatResponse));
  }

  private static bool ChatHistoryContains(ChatHistory chat, string input)
  {
    return chat.ToList().Exists(content => content.ToString().Contains(input));
  }

  private sealed class ReplyBuilder
  {
    private string? title = "title";
    private string? date = "2024-06-10T20:55:23Z";
    private string? location = "location";
    private string? timeZone = "Europe/Helsinki";
    private List<string>? participants = ["expected participant"];
    private List<string>? recurrence = ["expected recurrence"];


    public ReplyBuilder WithTitle(string? value)
    {
      title = value;
      return this;
    }

    public ReplyBuilder WithDate(string? value)
    {
      date = value;
      return this;
    }

    public ReplyBuilder WithLocation(string? value)
    {
      location = value;
      return this;
    }

    public ReplyBuilder WithTimeZone(string? value)
    {
      timeZone = value;
      return this;
    }

    public ReplyBuilder WithParticipants(List<string>? value)
    {
      participants = value;
      return this;
    }

    public ReplyBuilder WithRecurrence(List<string>? value)
    {
      recurrence = value;
      return this;
    }

    public string Build()
    {
      List<string> fields = [];

      if (title is not null)
      {
        fields.Add($"\"title\": \"{title}\"");
      }

      if (date is not null)
      {
        fields.Add($"\"date\": \"{date}\"");
      }

      if (location is not null)
      {
        fields.Add($"\"location\": \"{location}\"");
      }

      if (timeZone is not null)
      {
        fields.Add($"\"timeZone\": \"{timeZone}\"");
      }

      if (participants is not null)
      {
        var values = participants.Count == 0 ? "" : $"\"{string.Join("\",\"", participants)}\"";
        fields.Add($"\"participants\": [{values}]");
      }

      if (recurrence is not null)
      {
        var values = recurrence.Count == 0 ? "" : $"\"{string.Join("\",\"", recurrence)}\"";
        fields.Add($"\"recurrence\": [{values}]");
      }

      return $"{{ {string.Join(", ", fields)} }}";
    }
  }
}
