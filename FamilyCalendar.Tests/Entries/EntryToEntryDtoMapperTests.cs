using FamilyCalendar.Entries;

namespace FamilyCalendar.Tests.Entries;

public class EntryToEntryDtoMapperTests
{
  [Fact]
  public void ToEntryDto_ShouldCorrectlyMapEntryDtoToEntry()
  {
    var entry = EntryTestUtils.CreateTestEntry();

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
    Assert.Equal(entry.Recurrence, entryDto.Recurrence);
    Assert.Equal(entry.Prompt, entryDto.Prompt);
    Assert.Equal(entry.CreatedAt, entryDto.CreatedAt);
  }

  [Fact]
  public void ToEntryDto_MapsDatesToUtc()
  {
    var entry = EntryTestUtils.CreateTestEntry().With(
      date: new DateTimeOffset(2024, 5, 31, 14, 5, 0, TimeSpan.FromHours(8)),
      createdAt: new DateTimeOffset(2023, 5, 20, 5, 30, 0, TimeSpan.FromHours(8))
    );

    var entryDto = entry.ToEntryDto();

    Assert.Equal(TimeSpan.Zero, entryDto.Date.Offset);
    Assert.Equal(TimeSpan.Zero, entryDto.CreatedAt.Offset);
  }

  [Fact]
  public void ToEntryDto_ResolvesDisplayStartDate()
  {
    var entry = EntryTestUtils.CreateTestEntry();

    var entryDto = entry.ToEntryDto();

    Assert.Equal(entry.ResolveDisplayStartDate(), entryDto.DisplayStartDate);
  }

  [Fact]
  public void ToEntryDto_ResolvesDisplayEndDate()
  {
    var entry = EntryTestUtils.CreateTestEntry();

    var entryDto = entry.ToEntryDto();

    Assert.Equal(entry.ResolveDisplayEndDate(), entryDto.DisplayEndDate);
  }
}
