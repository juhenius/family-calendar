using FamilyCalendar.Common;
using FamilyCalendar.Entries;
using FamilyCalendar.Pages.Manage;
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
      EntryInput = "not relevant",
      PageContext = new PageContext
      {
        ViewData = viewData
      },
      MetadataProvider = modelMetadataProvider,
    };
  }

  [Fact]
  public async Task OnPostAddEntryAsync_ReturnsBadRequestForMissingInput()
  {
    _pageModel.EntryInput = null;

    var result = await _pageModel.OnPostAddEntryAsync(CancellationToken.None);

    Assert.IsType<BadRequestResult>(result);
  }

  [Fact]
  public async Task OnPostAddEntryAsync_ReturnsBadRequestForEmptyInput()
  {
    _pageModel.EntryInput = "";

    var result = await _pageModel.OnPostAddEntryAsync(CancellationToken.None);

    Assert.IsType<BadRequestResult>(result);
  }

  [Fact]
  public async Task OnPostAddEntryAsync_ParsesEntryFromInput()
  {
    var expectedInput = "expected input";
    _pageModel.EntryInput = expectedInput;

    await _pageModel.OnPostAddEntryAsync(CancellationToken.None);

    await _entryParser.Received(1).ParseFromString(
      expectedInput, _calendarId, Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task OnPostAddEntryAsync_UsesCurrentTimeForRelativeDates()
  {
    var now = DateTimeOffset.Now;

    await _pageModel.OnPostAddEntryAsync(CancellationToken.None);

    await _entryParser.Received(1).ParseFromString(
      Arg.Any<string>(), _calendarId, Arg.Any<Guid>(),
      Arg.Is<DateTimeOffset>(v => IsCloseTo(v, now, TimeSpan.FromSeconds(1))),
      Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task OnPostAddEntryAsync_CreatesNewEntry()
  {
    var entry = CreateTestEntry();
    _entryParser.ParseFromString(
      Arg.Any<string>(), _calendarId, Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
      .Returns(Task.FromResult(entry));

    await _pageModel.OnPostAddEntryAsync(CancellationToken.None);

    await _entryRepository.Received(1).CreateAsync(
      entry,
      Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task OnDeleteDeleteEntryAsync_DeletesTheEntry()
  {
    var entryId = Guid.NewGuid();

    await _pageModel.OnDeleteDeleteEntryAsync(entryId, CancellationToken.None);

    await _entryRepository.Received(1).DeleteAsync(_calendarId, entryId, Arg.Any<CancellationToken>());
  }

  private static bool IsCloseTo(DateTimeOffset a, DateTimeOffset b, TimeSpan offset)
  {
    return (b - a) < offset;
  }

  private Entry CreateTestEntry()
  {
    return new Entry
    {
      Id = Guid.NewGuid(),
      CalendarId = _calendarId,
      Title = "New Entry",
      Date = DateTimeOffset.UtcNow,
      Participants = ["Tester"],
      Recurrence = [],
      Prompt = "Doctor Appointment now at Doctors office",
      CreatedAt = new DateTimeOffset(2024, 5, 31, 14, 5, 0, TimeSpan.FromHours(8)),
    };
  }
}
