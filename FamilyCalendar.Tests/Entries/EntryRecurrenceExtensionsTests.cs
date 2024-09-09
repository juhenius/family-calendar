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
}
