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
    SetDefaultReply();

    var input = "expected input";
    var calendarId = Guid.NewGuid();
    await _openAiEntryParser.ParseFromString(input, calendarId, CancellationToken.None);

    await _chatCompletionService.Received(1).GetChatMessageContentsAsync(
      Arg.Is<ChatHistory>(chat => ChatHistoryContains(chat, input)),
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

    var entry = await _openAiEntryParser.ParseFromString("not relevant", Guid.NewGuid(), CancellationToken.None);

    Assert.Equal(title, entry.Title);
  }

  [Fact]
  public async Task ParseFromString_ThrowsWhenTitleIsMissingFromResponse()
  {
    SetReply(new ReplyBuilder().WithTitle(null).Build());

    await Assert.ThrowsAnyAsync<Exception>(() => _openAiEntryParser.ParseFromString("not relevant", Guid.NewGuid(), CancellationToken.None));
  }

  [Fact]
  public async Task ParseFromString_ParsesDateFromResponse()
  {
    SetReply(new ReplyBuilder().WithDate("2024-06-10T20:00:00Z").Build());

    var entry = await _openAiEntryParser.ParseFromString("not relevant", Guid.NewGuid(), CancellationToken.None);

    Assert.Equal(new DateTimeOffset(2024, 6, 10, 20, 0, 0, TimeSpan.Zero), entry.Date);
  }

  [Fact]
  public async Task ParseFromString_ThrowsWhenDateIsMissingFromResponse()
  {
    SetReply(new ReplyBuilder().WithDate(null).Build());
    await Assert.ThrowsAnyAsync<Exception>(() => _openAiEntryParser.ParseFromString("not relevant", Guid.NewGuid(), CancellationToken.None));
  }

  [Fact]
  public async Task ParseFromString_ThrowsWhenDateIsNotCorrectlyFormatedInResponse()
  {
    SetReply(new ReplyBuilder().WithDate("2024-06-10T20:00:00").Build());
    await Assert.ThrowsAnyAsync<Exception>(() => _openAiEntryParser.ParseFromString("not relevant", Guid.NewGuid(), CancellationToken.None));
  }

  [Fact]
  public async Task ParseFromString_ParsesLocationFromResponse()
  {
    var location = "expected location";
    SetReply(new ReplyBuilder().WithLocation(location).Build());

    var entry = await _openAiEntryParser.ParseFromString("not relevant", Guid.NewGuid(), CancellationToken.None);

    Assert.Equal(location, entry.Location);
  }

  [Fact]
  public async Task ParseFromString_ShouldNotThrowWhenLocationIsMissingFromResponse()
  {
    SetReply(new ReplyBuilder().WithLocation(null).Build());

    var exception = await Record.ExceptionAsync(() => _openAiEntryParser.ParseFromString("not relevant", Guid.NewGuid(), CancellationToken.None));

    Assert.Null(exception);
  }

  [Fact]
  public async Task ParseFromString_ParsesParticipantsFromResponse()
  {
    List<string> participants = ["expected participant 1", "expected participant 1"];
    SetReply(new ReplyBuilder().WithParticipants(participants).Build());

    var entry = await _openAiEntryParser.ParseFromString("not relevant", Guid.NewGuid(), CancellationToken.None);

    Assert.Equal(participants, entry.Participants);
  }

  [Fact]
  public async Task ParseFromString_ReturnsEmptyParticipantsWhenItIsMissingFromResponse()
  {
    SetReply(new ReplyBuilder().WithParticipants(null).Build());

    var entry = await _openAiEntryParser.ParseFromString("not relevant", Guid.NewGuid(), CancellationToken.None);

    Assert.Empty(entry.Participants);
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
    private List<string>? participants = ["expected participant"];


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

    public ReplyBuilder WithParticipants(List<string>? value)
    {
      participants = value;
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

      if (participants is not null)
      {
        var values = participants.Count == 0 ? "" : $"\"{string.Join("\",\"", participants)}\"";
        fields.Add($"\"participants\": [{values}]");
      }

      return $"{{ {string.Join(", ", fields)} }}";
    }
  }
}
