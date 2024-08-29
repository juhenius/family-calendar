using FamilyCalendar.Entries;

namespace FamilyCalendar.Tests.Entries;

public class EntryDtoToEntryMapperTests
{
  [Fact]
  public void ToEntry_ShouldCorrectlyMapEntryDtoToEntry()
  {
    var entryDto = new EntryDto
    {
      Id = Guid.NewGuid(),
      CalendarId = Guid.NewGuid(),
      Title = "Doctor Appointment",
      Date = new DateTimeOffset(2023, 5, 20, 14, 5, 0, TimeSpan.Zero),
      Location = "Doctors office",
      Participants = ["Tester"],
      Recurrence = ["test rule"],
      Prompt = "Doctor Appointment now at Doctors office",
      CreatedAt = new DateTimeOffset(2023, 5, 20, 5, 30, 0, TimeSpan.Zero),
    };

    var entry = entryDto.ToEntry();

    Assert.NotNull(entry);
    Assert.Equal(entryDto.Id, entry.Id);
    Assert.Equal(entryDto.CalendarId, entry.CalendarId);
    Assert.Equal(entryDto.Title, entry.Title);
    Assert.Equal(entryDto.Date, entry.Date);
    Assert.Equal(entryDto.Location, entry.Location);
    Assert.Equal(entryDto.Participants, entry.Participants);
    Assert.Equal(entryDto.Recurrence, entry.Recurrence);
    Assert.Equal(entryDto.Prompt, entry.Prompt);
    Assert.Equal(entryDto.CreatedAt, entry.CreatedAt);
  }
}
