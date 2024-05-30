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
      Member = "Tester",
    };

    var entry = entryDto.ToEntry();

    Assert.NotNull(entry);
    Assert.Equal(entryDto.Id, entry.Id);
    Assert.Equal(entryDto.CalendarId, entry.CalendarId);
    Assert.Equal(entryDto.Title, entry.Title);
    Assert.Equal(entryDto.Date, entry.Date);
    Assert.Equal(entryDto.Member, entry.Member);
  }
}
