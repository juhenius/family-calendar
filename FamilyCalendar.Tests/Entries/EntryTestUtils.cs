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
      TimeZone = "Europe/Helsinki",
    };
  }

  public static EntryParseResult CreateTestParseResult(
    string? title = null,
    DateTimeOffset? date = null,
    string? location = null,
    List<string>? participants = null,
    List<string>? recurrence = null,
    string? prompt = null,
    DateTimeOffset? localTime = null,
    string? timeZone = null)
  {
    return new EntryParseResult
    {
      Title = title ?? "New Entry",
      Date = date ?? DateTimeOffset.UtcNow,
      Location = location ?? null,
      Participants = participants ?? ["Tester"],
      Recurrence = recurrence ?? [],
      Prompt = prompt ?? "Doctor Appointment now at Doctors office",
      LocalTime = localTime ?? new DateTimeOffset(2024, 5, 31, 14, 5, 0, TimeSpan.Zero),
      TimeZone = timeZone ?? "Europe/Amsterdam",
    };
  }
}
