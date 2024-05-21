namespace FamilyCalendar.Entries.Tests;

public class EntryDtoToEntryMapperTests
{
  [Fact]
  public void ToEntry_ShouldCorrectlyMapEntryDtoToEntry()
  {
    var entryDto = new EntryDto
    {
      Id = Guid.NewGuid(),
      Title = "Doctor Appointment",
      Date = new DateTime(2023, 5, 20)
    };

    var entry = entryDto.ToEntry();

    Assert.NotNull(entry);
    Assert.Equal(entryDto.Id, entry.Id);
    Assert.Equal(entryDto.Title, entry.Title);
    Assert.Equal(entryDto.Date, entry.Date);
  }
}
