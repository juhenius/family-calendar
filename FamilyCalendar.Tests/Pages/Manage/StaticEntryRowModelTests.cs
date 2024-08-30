using FamilyCalendar.Entries;
using FamilyCalendar.Pages.Manage;
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
  public async Task OnGetAsync_SetsEntryWhenEntryIsFound()
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
    var reparsedEntry = CreateTestEntry().With(title: "reparsed");

    _entryRepository.GetAsync(_calendarId, _entryId, Arg.Any<CancellationToken>())
      .Returns(Task.FromResult<Entry?>(originalEntry));

    _entryParser.ParseFromString(originalEntry.Prompt, _calendarId, _entryId, Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
      .Returns(Task.FromResult(reparsedEntry));

    await _pageModel.OnPatchReparseEntryAsync(CancellationToken.None);

    await _entryRepository.Received(1).UpdateAsync(
      Arg.Is<Entry>(entry => entry.Title == reparsedEntry.Title),
      Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task OnPatchReparseEntryAsync_UsesOriginalPrompt()
  {
    var originalEntry = CreateTestEntry().With(title: "original");

    _entryRepository.GetAsync(_calendarId, _entryId, Arg.Any<CancellationToken>())
      .Returns(Task.FromResult<Entry?>(originalEntry));

    await _pageModel.OnPatchReparseEntryAsync(CancellationToken.None);

    await _entryParser.Received(1).ParseFromString(originalEntry.Prompt, _calendarId, _entryId,
      Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task OnPatchReparseEntryAsync_UsesEntryCreatedAtForRelativeTimes()
  {
    var originalEntry = CreateTestEntry().With(title: "original");

    _entryRepository.GetAsync(_calendarId, _entryId, Arg.Any<CancellationToken>())
      .Returns(Task.FromResult<Entry?>(originalEntry));

    await _pageModel.OnPatchReparseEntryAsync(CancellationToken.None);

    await _entryParser.Received(1).ParseFromString(Arg.Any<string>(), _calendarId, _entryId,
      originalEntry.CreatedAt, Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task OnPatchReparseEntryAsync_UsesCurrentTimezone()
  {
    var originalEntry = CreateTestEntry().With(title: "original");

    _entryRepository.GetAsync(_calendarId, _entryId, Arg.Any<CancellationToken>())
      .Returns(Task.FromResult<Entry?>(originalEntry));

    await _pageModel.OnPatchReparseEntryAsync(CancellationToken.None);

    await _entryParser.Received(1).ParseFromString(Arg.Any<string>(), _calendarId, _entryId,
      Arg.Is<DateTimeOffset>(now => now.Offset == DateTimeOffset.Now.Offset), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task OnPatchReparseEntryAsync_SetsEntryWhenEntryIsFound()
  {
    var originalEntry = CreateTestEntry().With(title: "original");
    var reparsedEntry = CreateTestEntry().With(title: "reparsed");

    _entryRepository.GetAsync(_calendarId, _entryId, Arg.Any<CancellationToken>())
      .Returns(Task.FromResult<Entry?>(originalEntry));

    _entryParser.ParseFromString(originalEntry.Prompt, _calendarId, _entryId, Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
      .Returns(Task.FromResult(reparsedEntry));

    var result = await _pageModel.OnPatchReparseEntryAsync(CancellationToken.None);

    Assert.IsType<PageResult>(result);
    Assert.Equal(reparsedEntry.Title, _pageModel.Entry.Title);
  }

  private Entry CreateTestEntry()
  {
    return new Entry
    {
      Id = _entryId,
      CalendarId = _calendarId,
      Title = "New Entry",
      Date = DateTimeOffset.UtcNow,
      Participants = ["Tester"],
      Recurrence = ["test rule"],
      Prompt = "Doctor Appointment now at Doctors office",
      CreatedAt = new DateTimeOffset(2024, 5, 31, 14, 5, 0, TimeSpan.FromHours(8)),
    };
  }
}
