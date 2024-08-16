using FamilyCalendar.Entries;

namespace FamilyCalendar.Tests.Entries;

public class EntryTests
{
  [Fact]
  public void With_ShouldReplaceId()
  {
    var originalEntry = CreateEntry();

    var expectedId = Guid.NewGuid();
    var newEntry = originalEntry.With(id: expectedId);

    Assert.Equal(expectedId, newEntry.Id);
    Assert.Equal(originalEntry.CalendarId, newEntry.CalendarId);
    Assert.Equal(originalEntry.Title, newEntry.Title);
    Assert.Equal(originalEntry.Date, newEntry.Date);
    Assert.Equal(originalEntry.Location, newEntry.Location);
    Assert.Equal(originalEntry.Participants, newEntry.Participants);
    Assert.Equal(originalEntry.Prompt, newEntry.Prompt);
    Assert.Equal(originalEntry.CreatedAt, newEntry.CreatedAt);
  }

  [Fact]
  public void With_ShouldReplaceCalendarId()
  {
    var originalEntry = CreateEntry();

    var expectedCalendarId = Guid.NewGuid();
    var newEntry = originalEntry.With(calendarId: expectedCalendarId);

    Assert.Equal(originalEntry.Id, newEntry.Id);
    Assert.Equal(expectedCalendarId, newEntry.CalendarId);
    Assert.Equal(originalEntry.Title, newEntry.Title);
    Assert.Equal(originalEntry.Date, newEntry.Date);
    Assert.Equal(originalEntry.Location, newEntry.Location);
    Assert.Equal(originalEntry.Participants, newEntry.Participants);
    Assert.Equal(originalEntry.Prompt, newEntry.Prompt);
    Assert.Equal(originalEntry.CreatedAt, newEntry.CreatedAt);
  }

  [Fact]
  public void With_ShouldReplaceTitle()
  {
    var originalEntry = CreateEntry();

    var expectedTitle = "new title";
    var newEntry = originalEntry.With(title: expectedTitle);

    Assert.Equal(originalEntry.Id, newEntry.Id);
    Assert.Equal(originalEntry.CalendarId, newEntry.CalendarId);
    Assert.Equal(expectedTitle, newEntry.Title);
    Assert.Equal(originalEntry.Date, newEntry.Date);
    Assert.Equal(originalEntry.Location, newEntry.Location);
    Assert.Equal(originalEntry.Participants, newEntry.Participants);
    Assert.Equal(originalEntry.Prompt, newEntry.Prompt);
    Assert.Equal(originalEntry.CreatedAt, newEntry.CreatedAt);
  }

  [Fact]
  public void With_ShouldReplaceDate()
  {
    var originalEntry = CreateEntry();

    var expectedDate = new DateTimeOffset(2024, 5, 20, 5, 30, 0, TimeSpan.Zero);
    var newEntry = originalEntry.With(date: expectedDate);

    Assert.Equal(originalEntry.Id, newEntry.Id);
    Assert.Equal(originalEntry.CalendarId, newEntry.CalendarId);
    Assert.Equal(originalEntry.Title, newEntry.Title);
    Assert.Equal(expectedDate, newEntry.Date);
    Assert.Equal(originalEntry.Location, newEntry.Location);
    Assert.Equal(originalEntry.Participants, newEntry.Participants);
    Assert.Equal(originalEntry.Prompt, newEntry.Prompt);
    Assert.Equal(originalEntry.CreatedAt, newEntry.CreatedAt);
  }

  [Fact]
  public void With_ShouldReplaceLocation()
  {
    var originalEntry = CreateEntry();

    var expectedLocation = "new location";
    var newEntry = originalEntry.With(location: expectedLocation);

    Assert.Equal(originalEntry.Id, newEntry.Id);
    Assert.Equal(originalEntry.CalendarId, newEntry.CalendarId);
    Assert.Equal(originalEntry.Title, newEntry.Title);
    Assert.Equal(originalEntry.Date, newEntry.Date);
    Assert.Equal(expectedLocation, newEntry.Location);
    Assert.Equal(originalEntry.Participants, newEntry.Participants);
    Assert.Equal(originalEntry.Prompt, newEntry.Prompt);
    Assert.Equal(originalEntry.CreatedAt, newEntry.CreatedAt);
  }

  [Fact]
  public void With_ShouldReplaceParticipants()
  {
    var originalEntry = CreateEntry();

    List<string> expectedParticipants = ["test1", "test2"];
    var newEntry = originalEntry.With(participants: expectedParticipants);

    Assert.Equal(originalEntry.Id, newEntry.Id);
    Assert.Equal(originalEntry.CalendarId, newEntry.CalendarId);
    Assert.Equal(originalEntry.Title, newEntry.Title);
    Assert.Equal(originalEntry.Date, newEntry.Date);
    Assert.Equal(originalEntry.Location, newEntry.Location);
    Assert.Equal(expectedParticipants, newEntry.Participants);
    Assert.Equal(originalEntry.Prompt, newEntry.Prompt);
    Assert.Equal(originalEntry.CreatedAt, newEntry.CreatedAt);
  }

  [Fact]
  public void With_ShouldReplacePrompt()
  {
    var originalEntry = CreateEntry();

    var expectedPrompt = "new prompt";
    var newEntry = originalEntry.With(prompt: expectedPrompt);

    Assert.Equal(originalEntry.Id, newEntry.Id);
    Assert.Equal(originalEntry.CalendarId, newEntry.CalendarId);
    Assert.Equal(originalEntry.Title, newEntry.Title);
    Assert.Equal(originalEntry.Date, newEntry.Date);
    Assert.Equal(originalEntry.Location, newEntry.Location);
    Assert.Equal(originalEntry.Participants, newEntry.Participants);
    Assert.Equal(expectedPrompt, newEntry.Prompt);
    Assert.Equal(originalEntry.CreatedAt, newEntry.CreatedAt);
  }

  [Fact]
  public void With_ShouldReplaceCreatedAt()
  {
    var originalEntry = CreateEntry();

    var expectedCreatedAt = new DateTimeOffset(2024, 5, 20, 5, 30, 0, TimeSpan.Zero);
    var newEntry = originalEntry.With(createdAt: expectedCreatedAt);

    Assert.Equal(originalEntry.Id, newEntry.Id);
    Assert.Equal(originalEntry.CalendarId, newEntry.CalendarId);
    Assert.Equal(originalEntry.Title, newEntry.Title);
    Assert.Equal(originalEntry.Date, newEntry.Date);
    Assert.Equal(originalEntry.Location, newEntry.Location);
    Assert.Equal(originalEntry.Participants, newEntry.Participants);
    Assert.Equal(originalEntry.Prompt, newEntry.Prompt);
    Assert.Equal(expectedCreatedAt, newEntry.CreatedAt);
  }

  private static Entry CreateEntry()
  {
    return new Entry
    {
      Id = Guid.NewGuid(),
      CalendarId = Guid.NewGuid(),
      Title = "Doctor Appointment",
      Date = new DateTimeOffset(2023, 6, 10, 13, 45, 0, TimeSpan.Zero),
      Location = "Doctors office",
      Participants = ["Tester"],
      Prompt = "Doctor Appointment now at Doctors office",
      CreatedAt = new DateTimeOffset(2023, 5, 20, 5, 30, 0, TimeSpan.Zero),
    };
  }
}
