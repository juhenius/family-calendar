using FamilyCalendar.Entries;
using FamilyCalendar.Pages.Manage;
using FamilyCalendar.Tests.Entries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NSubstitute;

namespace FamilyCalendar.Tests.Pages.Manage;

public class EditableEntryRowModelTests
{
  private readonly IEntryRepository _entryRepository;
  private readonly Guid _calendarId;
  private readonly Guid _entryId;
  private readonly EditableEntryRowModel _pageModel;

  public EditableEntryRowModelTests()
  {
    _entryRepository = Substitute.For<IEntryRepository>();
    _calendarId = Guid.NewGuid();
    _entryId = Guid.NewGuid();
    _pageModel = new EditableEntryRowModel(_entryRepository)
    {
      CalendarId = _calendarId,
      EntryId = _entryId,
      Entry = null!,
      Input = null!,
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
  public async Task OnGetAsync_SetsInputWhenEntryIsFound()
  {
    var entry = CreateTestEntry();
    _entryRepository.GetAsync(_calendarId, _entryId, Arg.Any<CancellationToken>())
      .Returns(Task.FromResult<Entry?>(entry));

    var result = await _pageModel.OnGetAsync(CancellationToken.None);

    Assert.IsType<PageResult>(result);
    Assert.Equal(entry.Title, _pageModel.Input.Title);
    Assert.Equal(entry.Date, _pageModel.Input.Date);
    Assert.Equal(entry.Location, _pageModel.Input.Location);
    Assert.True(IsEqualParticipants(entry.Participants, _pageModel.Input.Participants));
    Assert.True(IsEqualRecurrence(entry.Recurrence, _pageModel.Input.Recurrence));
  }

  [Fact]
  public async Task OnPatchAsync_ReturnsNotFoundWhenEntryDoesNotExist()
  {
    _entryRepository.GetAsync(_calendarId, _entryId, Arg.Any<CancellationToken>())
      .Returns(Task.FromResult<Entry?>(null));

    var result = await _pageModel.OnPatchAsync(CancellationToken.None);

    Assert.IsType<NotFoundResult>(result);
  }

  [Fact]
  public async Task OnPatchAsync_ShouldNotUpdateEntryFromInvalidInput()
  {
    _pageModel.ModelState.AddModelError("Input.Title", "The Title field is required.");

    var entry = CreateTestEntry();
    _entryRepository.GetAsync(_calendarId, _entryId, Arg.Any<CancellationToken>())
      .Returns(Task.FromResult<Entry?>(entry));

    var result = await _pageModel.OnPatchAsync(CancellationToken.None);

    Assert.IsType<PageResult>(result);
    await _entryRepository.Received(0).UpdateAsync(
      Arg.Any<Entry>(),
      Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task OnPatchAsync_ShouldUpdateEntryTitleFromInput()
  {
    var entry = CreateTestEntry().With(title: "original");
    _pageModel.Input = CreateInput(entry, title: "updated");
    _entryRepository.GetAsync(_calendarId, _entryId, Arg.Any<CancellationToken>())
      .Returns(Task.FromResult<Entry?>(entry));

    var result = await _pageModel.OnPatchAsync(CancellationToken.None);

    Assert.IsType<PageResult>(result);
    Assert.Equal(_pageModel.Input.Title, _pageModel.Entry.Title);
    await _entryRepository.Received(1).UpdateAsync(
      Arg.Is<Entry>(entry => entry.Title == _pageModel.Input.Title),
      Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task OnPatchAsync_ShouldUpdateEntryDateFromInput()
  {
    var entry = CreateTestEntry().With(date: DateTimeOffset.Now);
    _pageModel.Input = CreateInput(entry, date: DateTimeOffset.Now.AddHours(1));
    _entryRepository.GetAsync(_calendarId, _entryId, Arg.Any<CancellationToken>())
      .Returns(Task.FromResult<Entry?>(entry));

    var result = await _pageModel.OnPatchAsync(CancellationToken.None);

    Assert.IsType<PageResult>(result);
    Assert.Equal(_pageModel.Input.Date, _pageModel.Entry.Date);
    await _entryRepository.Received(1).UpdateAsync(
      Arg.Is<Entry>(entry => entry.Date == _pageModel.Input.Date),
      Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task OnPatchAsync_ShouldUpdateEntryLocationFromInput()
  {
    var entry = CreateTestEntry().With(location: "original");
    _pageModel.Input = CreateInput(entry, location: "updated");
    _entryRepository.GetAsync(_calendarId, _entryId, Arg.Any<CancellationToken>())
      .Returns(Task.FromResult<Entry?>(entry));

    var result = await _pageModel.OnPatchAsync(CancellationToken.None);

    Assert.IsType<PageResult>(result);
    Assert.Equal(_pageModel.Input.Location, _pageModel.Entry.Location);
    await _entryRepository.Received(1).UpdateAsync(
      Arg.Is<Entry>(entry => entry.Location == _pageModel.Input.Location),
      Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task OnPatchAsync_ShouldUpdateEntryParticipantsFromInput()
  {
    var entry = CreateTestEntry().With(participants: ["original"]);
    _pageModel.Input = CreateInput(entry, participants: "updated");
    _entryRepository.GetAsync(_calendarId, _entryId, Arg.Any<CancellationToken>())
      .Returns(Task.FromResult<Entry?>(entry));

    var result = await _pageModel.OnPatchAsync(CancellationToken.None);

    Assert.IsType<PageResult>(result);
    Assert.True(IsEqualParticipants(_pageModel.Entry.Participants, _pageModel.Input.Participants));
    await _entryRepository.Received(1).UpdateAsync(
      Arg.Is<Entry>(entry => IsEqualParticipants(entry.Participants, _pageModel.Input.Participants)),
      Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task OnPatchAsync_ShouldUpdateEntryRecurrenceFromInput()
  {
    var entry = CreateTestEntry().With(recurrence: ["original"]);
    _pageModel.Input = CreateInput(entry, recurrence: "updated");
    _entryRepository.GetAsync(_calendarId, _entryId, Arg.Any<CancellationToken>())
      .Returns(Task.FromResult<Entry?>(entry));

    var result = await _pageModel.OnPatchAsync(CancellationToken.None);

    Assert.IsType<PageResult>(result);
    Assert.True(IsEqualRecurrence(_pageModel.Entry.Recurrence, _pageModel.Input.Recurrence));
    await _entryRepository.Received(1).UpdateAsync(
      Arg.Is<Entry>(entry => IsEqualRecurrence(entry.Recurrence, _pageModel.Input.Recurrence)),
      Arg.Any<CancellationToken>());
  }

  private Entry CreateTestEntry()
  {
    return EntryTestUtils.CreateTestEntry().With(id: _entryId, calendarId: _calendarId);
  }

  private static EditableEntryRowModel.UpdateEntryInputModel CreateInput(
    Entry entry,
    string? title = null,
    DateTimeOffset? date = null,
    string? location = null,
    string? participants = null,
    string? recurrence = null
    )
  {
    return new EditableEntryRowModel.UpdateEntryInputModel()
    {
      Title = title ?? entry.Title,
      Date = date ?? entry.Date,
      Location = location ?? entry.Location,
      Participants = participants ?? string.Join(", ", entry.Participants),
      Recurrence = recurrence ?? string.Join("\n", entry.Recurrence),
    };
  }

  private static bool IsEqualParticipants(List<string> fromEntry, string? fromInput)
  {
    return string.Join(", ", fromEntry) == fromInput;
  }

  private static bool IsEqualRecurrence(List<string> fromEntry, string? fromInput)
  {
    return string.Join("\n", fromEntry) == fromInput;
  }
}
