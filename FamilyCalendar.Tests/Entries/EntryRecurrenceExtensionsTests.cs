using FamilyCalendar.Entries;

namespace FamilyCalendar.Tests.Entries;

public class EntryRecurrenceExtensionsTests
{
  [Fact]
  public void ResolveDisplayStartDate_EqualsEntryDate()
  {
    var entry = EntryTestUtils.CreateTestEntry();

    Assert.Equal(entry.Date, entry.ResolveDisplayStartDate());
  }

  [Fact]
  public void ResolveDisplayEndDate_ResolvesToEntryDateWhenEntryIsNonRecurring()
  {
    var entry = EntryTestUtils.CreateTestEntry().With(
      date: new DateTimeOffset(2024, 9, 10, 13, 0, 0, TimeSpan.Zero),
      recurrence: []);

    Assert.Equal(entry.Date, entry.ResolveDisplayEndDate());
  }

  [Fact]
  public void ResolveDisplayEndDate_ResolvesFromRecurrenceRules()
  {
    var entry = EntryTestUtils.CreateTestEntry().With(
      date: new DateTimeOffset(2024, 9, 10, 13, 0, 0, TimeSpan.Zero),
      recurrence: ["FREQ=DAILY;UNTIL=20240920T130000"]);
    var expectedDisplayEndDate = new DateTimeOffset(2024, 9, 20, 13, 0, 0, TimeSpan.Zero);

    Assert.Equal(expectedDisplayEndDate, entry.ResolveDisplayEndDate());
  }

  [Fact]
  public void ResolveDisplayEndDate_ResolvesToOver100YearsWhenRecurringIsUnlimited()
  {
    var entry = EntryTestUtils.CreateTestEntry().With(
      date: new DateTimeOffset(2024, 9, 10, 13, 0, 0, TimeSpan.Zero),
      recurrence: ["FREQ=YEARLY"]);

    Assert.True(entry.ResolveDisplayEndDate() > DateTimeOffset.Now.AddYears(100));
  }

  [Fact]
  public void ExpandRecurrence_ResolvesToEmptyListWhenEntryIsOutsideGivenRange()
  {
    var entry = EntryTestUtils.CreateTestEntry().With(
      date: new DateTimeOffset(2024, 9, 10, 13, 0, 0, TimeSpan.Zero),
      recurrence: []);
    var rangeStart = new DateTimeOffset(2024, 9, 10, 14, 0, 0, TimeSpan.Zero);
    var rangeEnd = new DateTimeOffset(2024, 9, 10, 15, 0, 0, TimeSpan.Zero);

    var entries = entry.ExpandRecurrenceForDateRange(rangeStart, rangeEnd);

    Assert.Empty(entries);
  }

  [Fact]
  public void ExpandRecurrence_ResolvesToSameEntryWhenEntryIsNonRecurring()
  {
    var entry = EntryTestUtils.CreateTestEntry().With(
      date: new DateTimeOffset(2024, 9, 10, 13, 0, 0, TimeSpan.Zero),
      recurrence: []);
    var rangeStart = new DateTimeOffset(2024, 9, 10, 12, 0, 0, TimeSpan.Zero);
    var rangeEnd = new DateTimeOffset(2024, 9, 10, 14, 0, 0, TimeSpan.Zero);

    var entries = entry.ExpandRecurrenceForDateRange(rangeStart, rangeEnd);

    Assert.Single(entries);
    Assert.Equal(entry, entries.First());
  }

  [Fact]
  public void ExpandRecurrence_ResolvesToRecurringEntries()
  {
    var entry = EntryTestUtils.CreateTestEntry().With(
      date: new DateTimeOffset(2024, 9, 1, 13, 0, 0, TimeSpan.Zero),
      recurrence: ["FREQ=DAILY;UNTIL=20240925T130000"]);
    var rangeStart = new DateTimeOffset(2024, 9, 10, 0, 0, 0, TimeSpan.Zero);
    var rangeEnd = new DateTimeOffset(2024, 9, 20, 23, 59, 59, TimeSpan.Zero);

    var entries = entry.ExpandRecurrenceForDateRange(rangeStart, rangeEnd);

    Assert.Equal(11, entries.Count());
  }
}
