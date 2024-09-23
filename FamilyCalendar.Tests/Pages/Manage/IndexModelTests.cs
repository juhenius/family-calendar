using FamilyCalendar.Common;
using FamilyCalendar.Entries;
using FamilyCalendar.Pages.Manage;
using FamilyCalendar.Tests.Entries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using NSubstitute;

namespace FamilyCalendar.Tests.Pages.Manage;

public class IndexModelTests
{
  private readonly IEntryRepository _entryRepository;
  private readonly IEntryParser _entryParser;
  private readonly IPartialViewRenderer _partialViewRenderer;
  private readonly Guid _calendarId;
  private readonly IndexModel _pageModel;

  public IndexModelTests()
  {
    _entryRepository = Substitute.For<IEntryRepository>();
    _entryParser = Substitute.For<IEntryParser>();
    _partialViewRenderer = Substitute.For<IPartialViewRenderer>();
    _calendarId = Guid.NewGuid();

    // https://github.com/dotnet/aspnetcore/blob/c85baf8db0c72ae8e68643029d514b2e737c9fae/src/Mvc/Mvc.RazorPages/test/PageModelTest.cs#L1922
    var modelMetadataProvider = new EmptyModelMetadataProvider();
    var viewData = new ViewDataDictionary(modelMetadataProvider, new ModelStateDictionary());
    _pageModel = new IndexModel(_entryRepository, _entryParser, _partialViewRenderer)
    {
      CalendarId = _calendarId,
      Input = null!,
      PageContext = new PageContext
      {
        ViewData = viewData
      },
      MetadataProvider = modelMetadataProvider,
    };
  }

  [Fact]
  public async Task OnPostAddEntryAsync_ReturnsBadRequestForInvalidInput()
  {
    _pageModel.ModelState.AddModelError("Input.Prompt", "The Prompt field is required.");

    var result = await _pageModel.OnPostAddEntryAsync(CancellationToken.None);

    Assert.IsType<BadRequestResult>(result);
  }

  [Fact]
  public async Task OnPostAddEntryAsync_ParsesEntryFromPrompt()
  {
    var expectedPrompt = "expected input";
    _pageModel.Input = CreateInput(prompt: expectedPrompt);
    ReturnValidResultFromParser();

    await _pageModel.OnPostAddEntryAsync(CancellationToken.None);

    await _entryParser.Received(1).ParseFromString(
      expectedPrompt, Arg.Any<DateTimeOffset>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task OnPostAddEntryAsync_UsesGivenCurrentTimeForRelativeDates()
  {
    var expectedCurrentTime = new DateTimeOffset(2024, 5, 31, 14, 5, 0, TimeSpan.FromHours(8));
    _pageModel.Input = CreateInput(currentTime: expectedCurrentTime);
    ReturnValidResultFromParser();

    await _pageModel.OnPostAddEntryAsync(CancellationToken.None);

    await _entryParser.Received(1).ParseFromString(Arg.Any<string>(), expectedCurrentTime, Arg.Any<string>(),
      Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task OnPostAddEntryAsync_UsesGivenTimeZone()
  {
    var expectedTimezone = "expected timezone";
    _pageModel.Input = CreateInput(timeZone: expectedTimezone);
    ReturnValidResultFromParser();

    await _pageModel.OnPostAddEntryAsync(CancellationToken.None);

    await _entryParser.Received(1).ParseFromString(Arg.Any<string>(), Arg.Any<DateTimeOffset>(), expectedTimezone,
      Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task OnPostAddEntryAsync_PersistsNewEntry()
  {
    _pageModel.Input = CreateInput();
    var parseResult = EntryTestUtils.CreateTestParseResult();
    ReturnValidResultFromParser(parseResult);

    await _pageModel.OnPostAddEntryAsync(CancellationToken.None);

    await _entryRepository.Received(1).CreateAsync(
      Arg.Is<Entry>(e => e.Title == parseResult.Title),
      Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task OnPostAddEntryAsync_UsesGivenCalendar()
  {
    _pageModel.Input = CreateInput();
    ReturnValidResultFromParser();

    await _pageModel.OnPostAddEntryAsync(CancellationToken.None);

    await _entryRepository.Received(1).CreateAsync(
      Arg.Is<Entry>(e => e.CalendarId == _calendarId),
      Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task OnDeleteDeleteEntryAsync_DeletesTheEntry()
  {
    var entryId = Guid.NewGuid();

    await _pageModel.OnDeleteDeleteEntryAsync(entryId, CancellationToken.None);

    await _entryRepository.Received(1).DeleteAsync(_calendarId, entryId, Arg.Any<CancellationToken>());
  }

  private void ReturnValidResultFromParser(EntryParseResult? result = null)
  {
    _entryParser.ParseFromString(Arg.Any<string>(), Arg.Any<DateTimeOffset>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
      .Returns(Task.FromResult(result ?? EntryTestUtils.CreateTestParseResult()));
  }

  private static IndexModel.AddEntryInputModel CreateInput(
    string? prompt = null,
    DateTimeOffset? currentTime = null,
    string? timeZone = null
    )
  {
    return new IndexModel.AddEntryInputModel()
    {
      Prompt = prompt ?? "not relevant",
      CurrentTime = currentTime ?? DateTimeOffset.Now,
      TimeZone = timeZone ?? "Europe/Helsinki",
    };
  }
}
