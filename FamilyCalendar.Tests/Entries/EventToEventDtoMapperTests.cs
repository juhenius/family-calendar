using FamilyCalendar.Entries;

namespace FamilyCalendar.Tests.Entries;

public class EventToEventDtoMapperTests
{
  [Fact]
  public void ToEntryDto_ShouldCorrectlyMapEntryDtoToEntry()
  {
    var entry = new Entry
    {
      Id = Guid.NewGuid(),
      CalendarId = Guid.NewGuid(),
      Title = "Doctor Appointment",
      Date = DateTime.UtcNow,
      Member = "Tester",
    };

    var entryDto = entry.ToEntryDto();

    Assert.NotNull(entryDto);
    Assert.Equal(entry.CalendarId.ToString(), entryDto.Pk);
    Assert.Equal(entry.Id.ToEntrySk(), entryDto.Sk);
    Assert.Equal(entry.Id, entryDto.Id);
    Assert.Equal(entry.CalendarId, entryDto.CalendarId);
    Assert.Equal(entry.Title, entryDto.Title);
    Assert.Equal(entry.Date, entryDto.Date);
    Assert.Equal(entry.Member, entryDto.Member);
  }

  [Fact]
  public void ToEntryDto_MapsDatesToUtc()
  {
    var entry = new Entry
    {
      Id = Guid.NewGuid(),
      CalendarId = Guid.NewGuid(),
      Title = "Doctor Appointment",
      Date = new DateTime(2024, 5, 31, 14, 5, 0, DateTimeKind.Unspecified),
      Member = "Tester",
    };

    var entryDto = entry.ToEntryDto();

    Assert.Equal(DateTimeKind.Utc, entryDto.Date.Kind);
  }
}