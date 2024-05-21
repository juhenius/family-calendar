namespace FamilyCalendar.Entries.Tests;

public class EventToEventDtoMapperTests
{
  [Fact]
  public void ToEntryDto_ShouldCorrectlyMapEntryDtoToEntry()
  {
    var entry = new Entry
    {
      Id = Guid.NewGuid(),
      Title = "Doctor Appointment",
      Date = new DateTime(2023, 5, 20)
    };

    var entryDto = entry.ToEntryDto();

    Assert.NotNull(entryDto);
    Assert.Equal(entry.Id, entryDto.Id);
    Assert.Equal(entry.Title, entryDto.Title);
    Assert.Equal(entry.Date, entryDto.Date);
  }
}