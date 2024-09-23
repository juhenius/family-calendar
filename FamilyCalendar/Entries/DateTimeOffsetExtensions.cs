using Ical.Net.DataTypes;

namespace FamilyCalendar.Entries;

public static class DateTimeOffsetExtensions
{
  public static IDateTime ToIDateTime(this DateTimeOffset self)
  {
    return new CalDateTime(self.UtcDateTime);
  }

  public static DateTimeOffset ToDateTimeOffset(this IDateTime self)
  {
    return self.AsDateTimeOffset;
  }
}
