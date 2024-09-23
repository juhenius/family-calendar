using FamilyCalendar.Entries;
using FamilyCalendar.Pages.Manage;
using FamilyCalendar.Tests.Entries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NSubstitute;

namespace FamilyCalendar.Tests.Pages.Manage;

public class StaticEntryRowModelTests
{
  private readonly IEntryRepository _entryRepository;
  private readonly IEntryParser _entryParser;
  private readonly Guid _calendarId;
  private readonly Guid _entryId;
  private readonly StaticEntryRowModel _pageModel;

  public StaticEntryRowModelTests()
  {
    _entryRepository = Substitute.For<IEntryRepository>();
    _entryParser = Substitute.For<IEntryParser>();
    _calendarId = Guid.NewGuid();
    _entryId = Guid.NewGuid();
    _pageModel = new StaticEntryRowModel(_entryRepository, _entryParser)
    {
      CalendarId = _calendarId,
      EntryId = _entryId,
      Entry = null!,
    };
  }

  [Fact]
  public async Task OnGetAsync_ReturnsNotFoundWhenEntryDoesNotExist()
  {
    _entryRepository.GetAsync(_calendarId, _entryId, Arg.Any<CancellationToken>())
      .Returns(Task.FromResult<Entry?>(null));

    var result = await _pageModel.OnGetAsync(CancellationToken.None);

    Assert.IsType<NotFoundResult>(result);
  }

  [Fact]
  public async Task OnGetAsync_DisplaysCorrectEntryWhenItIsFound()
  {
    var entry = CreateTestEntry();
    _entryRepository.GetAsync(_calendarId, _entryId, Arg.Any<CancellationToken>())
      .Returns(Task.FromResult<Entry?>(entry));

    var result = await _pageModel.OnGetAsync(CancellationToken.None);

    Assert.IsType<PageResult>(result);
    Assert.Equal(entry, _pageModel.Entry);
  }

  [Fact]
  public async Task OnPatchReparseEntryAsync_ReturnsNotFoundWhenEntryDoesNotExist()
  {
    _entryRepository.GetAsync(_calendarId, _entryId, Arg.Any<CancellationToken>())
      .Returns(Task.FromResult<Entry?>(null));

    var result = await _pageModel.OnPatchReparseEntryAsync(CancellationToken.None);

    Assert.IsType<NotFoundResult>(result);
  }

  [Fact]
  public async Task OnPatchReparseEntryAsync_UpdatesEntryWithReparsedOne()
  {
    var originalEntry = CreateTestEntry().With(title: "original");
    var reparsedEntry = EntryTestUtils.CreateTestParseResult(title: "reparsed");
    _entryRepository.GetAsync(_calendarId, _entryId, Arg.Any<CancellationToken>())
      .Returns(Task.FromResult<Entry?>(originalEntry));
    _entryParser.ParseFromString(Arg.Any<string>(), Arg.Any<DateTimeOffset>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
      .Returns(Task.FromResult(reparsedEntry));

    await _pageModel.OnPatchReparseEntryAsync(CancellationToken.None);

    await _entryRepository.Received(1).UpdateAsync(
      Arg.Is<Entry>(entry => entry.Title == reparsedEntry.Title),
      Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task OnPatchReparseEntryAsync_UsesOriginalPrompt()
  {
    var expectedPrompt = "original prompt";
    var originalEntry = CreateTestEntry().With(prompt: expectedPrompt);
    _entryRepository.GetAsync(_calendarId, _entryId, Arg.Any<CancellationToken>())
      .Returns(Task.FromResult<Entry?>(originalEntry));
    _entryParser.ParseFromString(Arg.Any<string>(), Arg.Any<DateTimeOffset>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
      .Returns(Task.FromResult(EntryTestUtils.CreateTestParseResult()));

    await _pageModel.OnPatchReparseEntryAsync(CancellationToken.None);

    await _entryParser.Received(1).ParseFromString(expectedPrompt, Arg.Any<DateTimeOffset>(), Arg.Any<string>(),
      Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task OnPatchReparseEntryAsync_UsesOriginalCreatedAtForLocalTime()
  {
    var timeZone = "Europe/Amsterdam";
    var createdAt = new DateTimeOffset(2023, 5, 20, 5, 30, 0, TimeSpan.Zero);
    var expectedLocalTime = new DateTimeOffset(2023, 5, 20, 7, 30, 0, TimeSpan.FromHours(2));
    var originalEntry = CreateTestEntry().With(createdAt: createdAt, timeZone: timeZone);
    _entryRepository.GetAsync(_calendarId, _entryId, Arg.Any<CancellationToken>())
      .Returns(Task.FromResult<Entry?>(originalEntry));
    _entryParser.ParseFromString(Arg.Any<string>(), Arg.Any<DateTimeOffset>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
      .Returns(Task.FromResult(EntryTestUtils.CreateTestParseResult()));

    await _pageModel.OnPatchReparseEntryAsync(CancellationToken.None);

    await _entryParser.Received(1).ParseFromString(Arg.Any<string>(), expectedLocalTime, Arg.Any<string>(),
      Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task OnPatchReparseEntryAsync_UsesOriginalTimeZone()
  {
    var expectedTimeZone = "Europe/Amsterdam";
    var originalEntry = CreateTestEntry().With(timeZone: expectedTimeZone);
    _entryRepository.GetAsync(_calendarId, _entryId, Arg.Any<CancellationToken>())
      .Returns(Task.FromResult<Entry?>(originalEntry));
    _entryParser.ParseFromString(Arg.Any<string>(), Arg.Any<DateTimeOffset>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
      .Returns(Task.FromResult(EntryTestUtils.CreateTestParseResult()));

    await _pageModel.OnPatchReparseEntryAsync(CancellationToken.None);

    await _entryParser.Received(1).ParseFromString(Arg.Any<string>(), Arg.Any<DateTimeOffset>(), expectedTimeZone,
      Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task OnPatchReparseEntryAsync_DisplaysUpdatedEntry()
  {
    var originalEntry = CreateTestEntry().With(title: "original");
    var reparsedEntry = EntryTestUtils.CreateTestParseResult(title: "reparsed");
    _entryRepository.GetAsync(_calendarId, _entryId, Arg.Any<CancellationToken>())
      .Returns(Task.FromResult<Entry?>(originalEntry));
    _entryParser.ParseFromString(Arg.Any<string>(), Arg.Any<DateTimeOffset>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
      .Returns(Task.FromResult(reparsedEntry));

    var result = await _pageModel.OnPatchReparseEntryAsync(CancellationToken.None);

    Assert.IsType<PageResult>(result);
    Assert.Equal(reparsedEntry.Title, _pageModel.Entry.Title);
  }

  private Entry CreateTestEntry()
  {
    return EntryTestUtils.CreateTestEntry().With(id: _entryId, calendarId: _calendarId);
  }
}
