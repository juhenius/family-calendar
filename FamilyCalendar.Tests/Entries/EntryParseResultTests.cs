using FamilyCalendar.Entries;

namespace FamilyCalendar.Tests.Entries;

public class EntryParseResultTests
{
  [Fact]
  public void ToEntry_ShouldCorrectlyMapFields()
  {
    var expectedId = Guid.NewGuid();
    var expectedCalendarId = Guid.NewGuid();
    var parseResult = new EntryParseResult
    {
      Title = "Doctor Appointment",
      Date = new DateTimeOffset(2023, 5, 20, 14, 5, 0, TimeSpan.Zero),
      Location = "Doctors office",
      Participants = ["Tester"],
      Recurrence = [],
      Prompt = "Doctor Appointment now at Doctors office",
      LocalTime = new DateTimeOffset(2023, 5, 20, 5, 30, 0, TimeSpan.FromHours(2)),
      TimeZone = "Europe/Amsterdam",
    };

    var entry = parseResult.ToEntry(expectedId, expectedCalendarId);

    Assert.NotNull(entry);
    Assert.Equal(expectedId, entry.Id);
    Assert.Equal(expectedCalendarId, entry.CalendarId);
    Assert.Equal(parseResult.Title, entry.Title);
    Assert.Equal(parseResult.Date, entry.Date);
    Assert.Equal(parseResult.Location, entry.Location);
    Assert.Equal(parseResult.Participants, entry.Participants);
    Assert.Equal(parseResult.Recurrence, entry.Recurrence);
    Assert.Equal(parseResult.Prompt, entry.Prompt);
    Assert.Equal(parseResult.LocalTime, entry.CreatedAt);
    Assert.Equal(parseResult.TimeZone, entry.TimeZone);
  }

  [Fact]
  public void ToEntry_ReturnsCreatedAtInUtc()
  {
    var expectedId = Guid.NewGuid();
    var expectedCalendarId = Guid.NewGuid();
    var parseResult = new EntryParseResult
    {
      Title = "Doctor Appointment",
      Date = new DateTimeOffset(2023, 5, 20, 14, 5, 0, TimeSpan.Zero),
      Location = "Doctors office",
      Participants = ["Tester"],
      Recurrence = [],
      Prompt = "Doctor Appointment now at Doctors office",
      LocalTime = new DateTimeOffset(2023, 5, 20, 5, 30, 0, TimeSpan.FromHours(2)),
      TimeZone = "Europe/Amsterdam",
    };

    var entry = parseResult.ToEntry(expectedId, expectedCalendarId);

    Assert.NotNull(entry);
    Assert.Equal(TimeSpan.Zero, entry.CreatedAt.Offset);
  }
}
