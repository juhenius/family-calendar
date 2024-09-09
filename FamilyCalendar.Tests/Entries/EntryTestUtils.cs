using FamilyCalendar.Entries;

namespace FamilyCalendar.Tests.Entries;

public class EntryTestUtils
{
  public static Entry CreateTestEntry()
  {
    return new Entry
    {
      Id = Guid.NewGuid(),
      CalendarId = Guid.NewGuid(),
      Title = "New Entry",
      Date = DateTimeOffset.UtcNow,
      Participants = ["Tester"],
      Recurrence = [],
      Prompt = "Doctor Appointment now at Doctors office",
      CreatedAt = DateTimeOffset.UtcNow,
    };
  }
}
