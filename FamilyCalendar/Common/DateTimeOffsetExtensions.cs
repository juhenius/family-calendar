using System.Globalization;
using NodaTime;
using NodaTime.Extensions;

namespace FamilyCalendar.Common;

public static class DateTimeOffsetExtensions
{
  public static DateTimeOffset InTimeZone(this DateTimeOffset self, string timeZoneId)
  {
    var zone = DateTimeZoneProviders.Tzdb[timeZoneId];
    var zonedTime = self.ToInstant().InZone(zone);
    return zonedTime.ToDateTimeOffset();
  }

  public static string ToIsoString(this DateTimeOffset self)
  {
    return self.ToString("o", CultureInfo.InvariantCulture);
  }
}
