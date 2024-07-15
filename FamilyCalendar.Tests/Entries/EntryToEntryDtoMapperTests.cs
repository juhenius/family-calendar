using FamilyCalendar.Entries;

namespace FamilyCalendar.Tests.Entries;

public class EntryToEntryDtoMapperTests
{
  [Fact]
  public void ToEntryDto_ShouldCorrectlyMapEntryDtoToEntry()
  {
    var entry = new Entry
    {
      Id = Guid.NewGuid(),
      CalendarId = Guid.NewGuid(),
      Title = "Doctor Appointment",
      Date = DateTimeOffset.UtcNow,
      Location = "Doctors office",
      Participants = ["Tester"],
      Prompt = "Doctor Appointment now at Doctors office",
    };

    var entryDto = entry.ToEntryDto();

    Assert.NotNull(entryDto);
    Assert.Equal(entry.CalendarId.ToString(), entryDto.Pk);
    Assert.Equal(entry.Id.ToEntrySk(), entryDto.Sk);
    Assert.Equal(entry.Id, entryDto.Id);
    Assert.Equal(entry.CalendarId, entryDto.CalendarId);
    Assert.Equal(entry.Title, entryDto.Title);
    Assert.Equal(entry.Date, entryDto.Date);
    Assert.Equal(entry.Location, entryDto.Location);
    Assert.Equal(entry.Participants, entryDto.Participants);
    Assert.Equal(entry.Prompt, entryDto.Prompt);
  }

  [Fact]
  public void ToEntryDto_MapsDatesToUtc()
  {
    var entry = new Entry
    {
      Id = Guid.NewGuid(),
      CalendarId = Guid.NewGuid(),
      Title = "Doctor Appointment",
      Date = new DateTimeOffset(2024, 5, 31, 14, 5, 0, TimeSpan.FromHours(8)),
      Participants = ["Tester"],
      Prompt = "Doctor Appointment now at Doctors office",
    };

    var entryDto = entry.ToEntryDto();

    Assert.Equal(TimeSpan.Zero, entryDto.Date.Offset);
  }
}